namespace Luzart
{
    using UnityEngine;

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyVolume();
        }

        // ========== PLAY ==========

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("[SoundManager] PlaySFX: clip is null");
                return;
            }
            if (sfxSource == null)
            {
                Debug.LogWarning("[SoundManager] PlaySFX: sfxSource is null");
                return;
            }
            sfxSource.PlayOneShot(clip);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null) return;
            if (clip == null)
            {
                musicSource.Stop();
                return;
            }
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        // ========== SHORTCUT SFX (dùng GameConfig) ==========

        public void PlayDialogueSFX()
        {
            var cfg = GameFlowController.Instance?.GameConfig;
            if (cfg != null) PlaySFX(cfg.sfxDialogue);
        }

        public void PlayMenuClickSFX()
        {
            var cfg = GameFlowController.Instance?.GameConfig;
            if (cfg != null) PlaySFX(cfg.sfxMenuClick);
        }

        public void PlayInteractSFX()
        {
            var cfg = GameFlowController.Instance?.GameConfig;
            if (cfg != null) PlaySFX(cfg.sfxInteract);
        }

        public void PlayPasscodeInputSFX()
        {
            var cfg = GameFlowController.Instance?.GameConfig;
            if (cfg != null) PlaySFX(cfg.sfxPasscodeInput);
        }

        public void PlayPasscodeWrongSFX()
        {
            var cfg = GameFlowController.Instance?.GameConfig;
            if (cfg != null) PlaySFX(cfg.sfxPasscodeWrong);
        }

        public void PlaySafeOpenSFX()
        {
            var cfg = GameFlowController.Instance?.GameConfig;
            if (cfg != null) PlaySFX(cfg.sfxSafeOpen);
        }

        // ========== VOLUME ==========

        public void ApplyVolume()
        {
            var data = GameDataManager.Instance?.Data;
            if (data == null) return;

            if (musicSource != null) musicSource.volume = data.musicVolume;
            if (sfxSource != null) sfxSource.volume = data.sfxVolume;
        }

        public void SetMusicVolume(float vol)
        {
            if (musicSource != null) musicSource.volume = vol;
        }

        public void SetSFXVolume(float vol)
        {
            if (sfxSource != null) sfxSource.volume = vol;
        }
    }
}
