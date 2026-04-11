namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// SoundManager v2:
    ///  - ID-based Play(SoundId) + data-driven SoundEntry (trong SoundConfigSO)
    ///  - AudioSource pool cho SFX (không bị cắt khi chơi chồng)
    ///  - Cooldown giữa 2 lần Play cùng 1 ID (minIntervalBetweenPlays)
    ///  - Giới hạn concurrent + steal oldest
    ///  - Random clip / pitch / volume
    ///  - Category volume + mute riêng (Music/SFX/UI/Ambient/Voice) + Master
    ///  - BGM fade in/out + crossfade bằng 2 music source
    ///  - 2D / 3D (spatialBlend), attach theo Transform
    ///  - Pause / Resume all
    ///
    /// Legacy wrappers (PlayTypingSFX, SetMusicVolume, ...) được giữ để không phá call site cũ.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        // ----- Config -----
        [Header("Config")]
        [SerializeField] private SoundConfigSO soundConfig;

        [Header("Legacy Sources (giữ để tương thích scene cũ — vẫn dùng cho BGM)")]
        [SerializeField] private AudioSource musicSource;   // primary music
        [SerializeField] private AudioSource sfxSource;     // không còn dùng để play, nhưng giữ để scene cũ không vỡ serialize

        [Header("Pool")]
        [Tooltip("Số AudioSource khởi tạo sẵn cho SFX pool")]
        [SerializeField] private int initialPoolSize = 8;
        [Tooltip("Giới hạn tối đa AudioSource pool có thể grow")]
        [SerializeField] private int maxPoolSize = 32;

        // ----- Runtime state -----
        private Dictionary<SoundId, SoundEntry> _entryMap;
        private Dictionary<SoundId, float> _lastPlayTime;
        private Dictionary<SoundId, List<int>> _activeByid;   // slotIndex list per id

        private class ActiveSound
        {
            public AudioSource source;
            public int generation;
            public SoundEntry entry;
            public Transform attachedTo;
            public Coroutine fadeCo;
            public bool inUse;
            public float startedAt;
        }
        private List<ActiveSound> _pool;
        private int _nextGen = 1;

        // music crossfade
        private AudioSource _musicA;
        private AudioSource _musicB;
        private bool _usingA = true;
        private Coroutine _musicFadeCo;
        private SoundId _currentMusicId = SoundId.None;

        // volumes / mutes (index theo SoundCategory)
        private readonly float[] _categoryVolume = new float[SoundCategoryExt.Count];
        private readonly bool[] _categoryMute = new bool[SoundCategoryExt.Count];
        private float _masterVolume = 1f;
        private bool _masterMute = false;

        // =====================================================================
        // Unity lifecycle
        // =====================================================================
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitVolumes();
            BuildRuntimeMap();
            InitPool();
            InitMusicSources();
            ApplyVolume();
        }

        private void InitVolumes()
        {
            for (int i = 0; i < _categoryVolume.Length; i++) _categoryVolume[i] = 1f;
        }

        private void BuildRuntimeMap()
        {
            _entryMap = new Dictionary<SoundId, SoundEntry>(32);
            _lastPlayTime = new Dictionary<SoundId, float>(32);
            _activeByid = new Dictionary<SoundId, List<int>>(32);

            if (soundConfig == null)
            {
                Debug.LogWarning("[SoundManager] Missing SoundConfigSO");
                return;
            }

            // 1. Entry do user config trong list
            if (soundConfig.entries != null)
            {
                foreach (var e in soundConfig.entries)
                {
                    if (e == null || e.id == SoundId.None) continue;
                    _entryMap[e.id] = e;
                }
            }

            // 2. Fallback: build default entry từ field legacy cho các ID chưa có
            foreach (SoundId id in System.Enum.GetValues(typeof(SoundId)))
            {
                if (id == SoundId.None) continue;
                if (_entryMap.ContainsKey(id)) continue;
                var def = soundConfig.BuildDefaultEntry(id);
                if (def != null) _entryMap[id] = def;
            }
        }

        private void InitPool()
        {
            _pool = new List<ActiveSound>(initialPoolSize);
            for (int i = 0; i < initialPoolSize; i++) _pool.Add(CreateSlot());
        }

        private ActiveSound CreateSlot()
        {
            var go = new GameObject("SfxSource_" + _pool?.Count);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            return new ActiveSound { source = src, inUse = false };
        }

        private void InitMusicSources()
        {
            // Ưu tiên dùng musicSource serialize sẵn làm A, tạo thêm B cho crossfade
            if (musicSource != null)
            {
                _musicA = musicSource;
            }
            else
            {
                var a = new GameObject("MusicSource_A");
                a.transform.SetParent(transform, false);
                _musicA = a.AddComponent<AudioSource>();
                _musicA.playOnAwake = false;
                musicSource = _musicA;
            }

            var b = new GameObject("MusicSource_B");
            b.transform.SetParent(transform, false);
            _musicB = b.AddComponent<AudioSource>();
            _musicB.playOnAwake = false;

            _musicA.loop = true;
            _musicB.loop = true;
        }

        private void Update()
        {
            // Release slot khi sound kết thúc
            for (int i = 0; i < _pool.Count; i++)
            {
                var a = _pool[i];
                if (!a.inUse) continue;

                // attach-to-transform
                if (a.attachedTo != null)
                    a.source.transform.position = a.attachedTo.position;

                if (!a.source.isPlaying && a.fadeCo == null)
                {
                    ReleaseSlot(i);
                }
            }
        }

        // =====================================================================
        // PUBLIC — Play
        // =====================================================================

        public SoundHandle Play(SoundId id)
        {
            return PlayInternal(id, SoundPlayOptions.None);
        }

        public SoundHandle Play(SoundId id, Vector3 worldPosition)
        {
            var opt = SoundPlayOptions.None;
            opt.worldPosition = worldPosition;
            return PlayInternal(id, opt);
        }

        public SoundHandle PlayAttached(SoundId id, Transform parent)
        {
            var opt = SoundPlayOptions.None;
            opt.attachTo = parent;
            return PlayInternal(id, opt);
        }

        public SoundHandle Play(SoundId id, SoundPlayOptions options)
        {
            return PlayInternal(id, options);
        }

        private SoundHandle PlayInternal(SoundId id, SoundPlayOptions opt)
        {
            if (id == SoundId.None) return SoundHandle.Invalid;
            if (_entryMap == null || !_entryMap.TryGetValue(id, out var entry) || entry == null)
                return SoundHandle.Invalid;

            // ----- Cooldown -----
            if (entry.minIntervalBetweenPlays > 0f)
            {
                if (_lastPlayTime.TryGetValue(id, out var last))
                {
                    if (Time.unscaledTime - last < entry.minIntervalBetweenPlays)
                        return SoundHandle.Invalid;
                }
            }

            // ----- Interrupt self -----
            if (entry.interruptSelf) StopAll(id);

            // ----- Concurrency cap -----
            if (entry.maxConcurrent > 0)
            {
                var list = GetActiveList(id);
                CleanupActiveList(list);
                if (list.Count >= entry.maxConcurrent)
                {
                    if (entry.stealOldestWhenFull && list.Count > 0)
                    {
                        int oldSlot = list[0];
                        list.RemoveAt(0);
                        ReleaseSlot(oldSlot);
                    }
                    else
                    {
                        return SoundHandle.Invalid;
                    }
                }
            }

            // ----- Pick clip -----
            var clip = entry.PickClip();
            if (clip == null) return SoundHandle.Invalid;

            // ----- Music routed through music sources -----
            if (entry.category == SoundCategory.Music)
            {
                PlayMusicEntry(id, entry, opt);
                _lastPlayTime[id] = Time.unscaledTime;
                return SoundHandle.Invalid; // music không dùng slot pool
            }

            // ----- Acquire slot -----
            int slot = AcquireSlot();
            if (slot < 0) return SoundHandle.Invalid;

            var active = _pool[slot];
            var src = active.source;
            active.entry = entry;
            active.inUse = true;
            active.attachedTo = opt.attachTo;
            active.generation = _nextGen++;
            active.startedAt = Time.unscaledTime;

            // Configure
            src.clip = clip;
            src.loop = opt.loop ?? entry.loop;
            src.priority = entry.priority;
            src.pitch = opt.pitch ?? entry.PickPitch();
            src.spatialBlend = entry.spatialBlend;
            src.ignoreListenerPause = entry.ignoreListenerPause;
            src.outputAudioMixerGroup = null;

            float baseVol = opt.volume ?? entry.PickVolume();
            float targetVol = baseVol * GetEffectiveCategoryVolume(entry.category);

            // Position
            if (opt.attachTo != null)
                src.transform.position = opt.attachTo.position;
            else if (opt.worldPosition.HasValue)
                src.transform.position = opt.worldPosition.Value;
            else
                src.transform.localPosition = Vector3.zero;

            // Fade in?
            float fadeIn = opt.fadeIn ?? entry.fadeIn;
            if (fadeIn > 0f)
            {
                src.volume = 0f;
                src.PlayDelayed(opt.delay ?? 0f);
                active.fadeCo = StartCoroutine(FadeSource(active, 0f, targetVol, fadeIn, releaseOnEnd: false));
            }
            else
            {
                src.volume = targetVol;
                if ((opt.delay ?? 0f) > 0f) src.PlayDelayed(opt.delay.Value);
                else src.Play();
            }

            // Track active
            if (entry.maxConcurrent > 0)
                GetActiveList(id).Add(slot);

            _lastPlayTime[id] = Time.unscaledTime;
            return new SoundHandle(slot, active.generation);
        }

        // =====================================================================
        // PUBLIC — Stop
        // =====================================================================

        public void Stop(SoundHandle handle)
        {
            if (!handle.IsValid) return;
            if (handle.slot < 0 || handle.slot >= _pool.Count) return;
            var a = _pool[handle.slot];
            if (!a.inUse || a.generation != handle.generation) return;
            StopSlot(handle.slot, a.entry != null ? a.entry.fadeOut : 0f);
        }

        public void StopAll(SoundId id)
        {
            if (_activeByid != null && _activeByid.TryGetValue(id, out var list))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    int slot = list[i];
                    var a = _pool[slot];
                    if (a.inUse && a.entry != null && a.entry.id == id)
                        StopSlot(slot, a.entry.fadeOut);
                }
                list.Clear();
            }
            else
            {
                // không track trong activeByid (maxConcurrent==0), quét pool
                for (int i = 0; i < _pool.Count; i++)
                {
                    var a = _pool[i];
                    if (a.inUse && a.entry != null && a.entry.id == id)
                        StopSlot(i, a.entry.fadeOut);
                }
            }
        }

        public void StopCategory(SoundCategory cat)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                var a = _pool[i];
                if (a.inUse && a.entry != null && a.entry.category == cat)
                    StopSlot(i, a.entry.fadeOut);
            }
            if (cat == SoundCategory.Music) StopMusic(0f);
        }

        public void StopAll()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i].inUse) StopSlot(i, 0f);
            }
            StopMusic(0f);
        }

        public void PauseAll()
        {
            AudioListener.pause = true;
        }

        public void ResumeAll()
        {
            AudioListener.pause = false;
        }

        // =====================================================================
        // PUBLIC — Music
        // =====================================================================

        public void PlayMusic(SoundId id, float fadeIn = 0f)
        {
            var opt = SoundPlayOptions.None;
            opt.fadeIn = fadeIn;
            Play(id, opt);
        }

        public void CrossfadeMusic(SoundId id, float duration = 1f)
        {
            if (id == SoundId.None) { StopMusic(duration); return; }
            if (_entryMap == null || !_entryMap.TryGetValue(id, out var entry) || entry == null) return;
            var clip = entry.PickClip();
            if (clip == null) return;

            if (_currentMusicId == id)
            {
                var cur = _usingA ? _musicA : _musicB;
                if (cur.clip == clip && cur.isPlaying) return;
            }

            var fromSrc = _usingA ? _musicA : _musicB;
            var toSrc = _usingA ? _musicB : _musicA;

            toSrc.clip = clip;
            toSrc.loop = entry.loop;
            toSrc.priority = entry.priority;
            toSrc.pitch = 1f;
            toSrc.volume = 0f;
            toSrc.Play();

            if (_musicFadeCo != null) StopCoroutine(_musicFadeCo);
            _musicFadeCo = StartCoroutine(CrossfadeRoutine(fromSrc, toSrc, entry, duration));

            _usingA = !_usingA;
            _currentMusicId = id;
        }

        private void PlayMusicEntry(SoundId id, SoundEntry entry, SoundPlayOptions opt)
        {
            var clip = entry.PickClip();
            if (clip == null) return;

            var cur = _usingA ? _musicA : _musicB;
            if (_currentMusicId == id && cur.clip == clip && cur.isPlaying) return;

            float fadeIn = opt.fadeIn ?? entry.fadeIn;
            if (fadeIn > 0f)
            {
                CrossfadeMusic(id, fadeIn);
                return;
            }

            cur.clip = clip;
            cur.loop = opt.loop ?? entry.loop;
            cur.priority = entry.priority;
            cur.pitch = 1f;
            cur.volume = entry.PickVolume() * GetEffectiveCategoryVolume(SoundCategory.Music);
            cur.Play();
            _currentMusicId = id;
        }

        public void StopMusic(float fadeOut = 0f)
        {
            if (_musicFadeCo != null) StopCoroutine(_musicFadeCo);

            if (fadeOut <= 0f)
            {
                _musicA?.Stop();
                _musicB?.Stop();
                _currentMusicId = SoundId.None;
                return;
            }
            _musicFadeCo = StartCoroutine(FadeOutBothMusic(fadeOut));
        }

        public void PlayBackgroundMusic()
        {
            PlayMusic(SoundId.BGM_Main);
        }

        // =====================================================================
        // PUBLIC — Volume / Mute
        // =====================================================================

        public void SetMasterVolume(float v)
        {
            _masterVolume = Mathf.Clamp01(v);
            ApplyVolume();
        }

        public void SetMasterMute(bool mute)
        {
            _masterMute = mute;
            ApplyVolume();
        }

        public void SetCategoryVolume(SoundCategory cat, float v)
        {
            _categoryVolume[(int)cat] = Mathf.Clamp01(v);
            ApplyVolume();
        }

        public void SetCategoryMute(SoundCategory cat, bool mute)
        {
            _categoryMute[(int)cat] = mute;
            ApplyVolume();
        }

        public float GetCategoryVolume(SoundCategory cat) => _categoryVolume[(int)cat];
        public bool IsCategoryMuted(SoundCategory cat) => _categoryMute[(int)cat];

        /// <summary>Áp toàn bộ volume category lên các source đang phát.</summary>
        public void ApplyVolume()
        {
            // Kéo volume music/sfx từ GameSaveData để giữ UISettings cũ hoạt động.
            var data = GameDataManager.Instance?.Data;
            if (data != null)
            {
                _categoryVolume[(int)SoundCategory.Music] = data.musicVolume;
                // Các category gameplay còn lại chia sẻ sfxVolume từ settings cũ.
                _categoryVolume[(int)SoundCategory.SFX] = data.sfxVolume;
                _categoryVolume[(int)SoundCategory.UI] = data.sfxVolume;
                _categoryVolume[(int)SoundCategory.Ambient] = data.sfxVolume;
                _categoryVolume[(int)SoundCategory.Voice] = data.sfxVolume;
            }

            // Music — set thẳng nếu không có fade đang chạy
            if (_musicFadeCo == null)
            {
                float musicVol = GetEffectiveCategoryVolume(SoundCategory.Music);
                if (_musicA != null) _musicA.volume = musicVol;
                if (_musicB != null) _musicB.volume = musicVol;
            }

            // Active SFX slots — cập nhật volume theo category hiện tại.
            // Dùng volume base (không re-random) để tránh flicker khi settings thay đổi.
            if (_pool != null)
            {
                for (int i = 0; i < _pool.Count; i++)
                {
                    var a = _pool[i];
                    if (!a.inUse || a.entry == null) continue;
                    if (a.fadeCo != null) continue; // để fade coroutine quản
                    a.source.volume = a.entry.volume * GetEffectiveCategoryVolume(a.entry.category);
                }
            }
        }

        private float GetEffectiveCategoryVolume(SoundCategory cat)
        {
            if (_masterMute || _categoryMute[(int)cat]) return 0f;
            return _masterVolume * _categoryVolume[(int)cat];
        }

        // =====================================================================
        // Pool helpers
        // =====================================================================

        private int AcquireSlot()
        {
            for (int i = 0; i < _pool.Count; i++)
                if (!_pool[i].inUse) return i;

            if (_pool.Count < maxPoolSize)
            {
                _pool.Add(CreateSlot());
                return _pool.Count - 1;
            }

            // Pool full — steal slot có priority thấp nhất (priority cao = giá trị số lớn)
            int stealIdx = -1;
            int worstPri = -1;
            float oldestStart = float.MaxValue;
            for (int i = 0; i < _pool.Count; i++)
            {
                var a = _pool[i];
                if (a.entry == null) continue;
                if (a.entry.priority > worstPri ||
                    (a.entry.priority == worstPri && a.startedAt < oldestStart))
                {
                    worstPri = a.entry.priority;
                    oldestStart = a.startedAt;
                    stealIdx = i;
                }
            }
            if (stealIdx >= 0) ReleaseSlot(stealIdx);
            return stealIdx;
        }

        private void ReleaseSlot(int slot)
        {
            var a = _pool[slot];
            if (a.source.isPlaying) a.source.Stop();
            a.source.clip = null;
            if (a.fadeCo != null) { StopCoroutine(a.fadeCo); a.fadeCo = null; }
            if (a.entry != null && _activeByid.TryGetValue(a.entry.id, out var list))
                list.Remove(slot);
            a.entry = null;
            a.attachedTo = null;
            a.inUse = false;
        }

        private void StopSlot(int slot, float fadeOut)
        {
            var a = _pool[slot];
            if (!a.inUse) return;
            if (fadeOut <= 0f)
            {
                ReleaseSlot(slot);
                return;
            }
            if (a.fadeCo != null) StopCoroutine(a.fadeCo);
            a.fadeCo = StartCoroutine(FadeSource(a, a.source.volume, 0f, fadeOut, releaseOnEnd: true));
        }

        private List<int> GetActiveList(SoundId id)
        {
            if (!_activeByid.TryGetValue(id, out var list))
            {
                list = new List<int>(4);
                _activeByid[id] = list;
            }
            return list;
        }

        private void CleanupActiveList(List<int> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                int slot = list[i];
                if (slot < 0 || slot >= _pool.Count || !_pool[slot].inUse)
                    list.RemoveAt(i);
            }
        }

        // =====================================================================
        // Fade coroutines
        // =====================================================================

        private IEnumerator FadeSource(ActiveSound a, float from, float to, float duration, bool releaseOnEnd)
        {
            if (!a.source.isPlaying && to > 0f) a.source.Play();
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                a.source.volume = Mathf.Lerp(from, to, k);
                yield return null;
            }
            a.source.volume = to;
            a.fadeCo = null;
            if (releaseOnEnd)
            {
                int idx = _pool.IndexOf(a);
                if (idx >= 0) ReleaseSlot(idx);
            }
        }

        private IEnumerator CrossfadeRoutine(AudioSource from, AudioSource to, SoundEntry toEntry, float duration)
        {
            float musicVol = GetEffectiveCategoryVolume(SoundCategory.Music);
            float fromStart = from != null ? from.volume : 0f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, k);
                if (to != null) to.volume = Mathf.Lerp(0f, musicVol, k);
                yield return null;
            }
            if (from != null) { from.Stop(); from.volume = 0f; }
            if (to != null) to.volume = musicVol;
            _musicFadeCo = null;
        }

        private IEnumerator FadeOutBothMusic(float duration)
        {
            float a0 = _musicA != null ? _musicA.volume : 0f;
            float b0 = _musicB != null ? _musicB.volume : 0f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                if (_musicA != null) _musicA.volume = Mathf.Lerp(a0, 0f, k);
                if (_musicB != null) _musicB.volume = Mathf.Lerp(b0, 0f, k);
                yield return null;
            }
            if (_musicA != null) _musicA.Stop();
            if (_musicB != null) _musicB.Stop();
            _currentMusicId = SoundId.None;
            _musicFadeCo = null;
        }

        // =====================================================================
        // LEGACY API — giữ để không vỡ call site cũ
        // =====================================================================

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            // one-shot ad-hoc: mượn 1 slot, play theo SFX category
            int slot = AcquireSlot();
            if (slot < 0) return;
            var a = _pool[slot];
            a.inUse = true;
            a.entry = null; // ad-hoc, không track id
            a.source.clip = clip;
            a.source.loop = false;
            a.source.pitch = 1f;
            a.source.spatialBlend = 0f;
            a.source.volume = GetEffectiveCategoryVolume(SoundCategory.SFX);
            a.source.Play();
            a.generation = _nextGen++;
            a.startedAt = Time.unscaledTime;
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) { StopMusic(0f); return; }
            var cur = _usingA ? _musicA : _musicB;
            if (cur.clip == clip && cur.isPlaying) return;
            cur.clip = clip;
            cur.loop = loop;
            cur.volume = GetEffectiveCategoryVolume(SoundCategory.Music);
            cur.Play();
            _currentMusicId = SoundId.None; // ad-hoc không có id
        }

        public void SetMusicVolume(float vol)
        {
            _categoryVolume[(int)SoundCategory.Music] = Mathf.Clamp01(vol);
            ApplyVolume();
        }

        public void SetSFXVolume(float vol)
        {
            float v = Mathf.Clamp01(vol);
            _categoryVolume[(int)SoundCategory.SFX] = v;
            _categoryVolume[(int)SoundCategory.UI] = v;
            _categoryVolume[(int)SoundCategory.Ambient] = v;
            _categoryVolume[(int)SoundCategory.Voice] = v;
            ApplyVolume();
        }

        // ----- Shortcut wrappers (call site cũ) -----
        public void PlayTypingSFX()        => Play(SoundId.SFX_Typing);
        public void PlayCollectItemSFX()   => Play(SoundId.SFX_CollectItem);
        public void PlayInteractSFX()      => Play(SoundId.SFX_Interact);
        public void PlayPasscodeInputSFX() => Play(SoundId.SFX_PasscodeInput);
        public void PlayPasscodeWrongSFX() => Play(SoundId.SFX_PasscodeWrong);
        public void PlaySafeOpenSFX()      => Play(SoundId.SFX_SafeOpen);
        public void PlayMenuClickSFX()     => Play(SoundId.SFX_MenuClick);
        public void PlayNotebookFlipSFX()  => Play(SoundId.SFX_NotebookFlip);
        public void PlayStartGameSFX()     => Play(SoundId.SFX_StartGame);
    }
}
