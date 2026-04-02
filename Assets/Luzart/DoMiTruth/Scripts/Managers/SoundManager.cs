namespace Luzart
{
    using UnityEngine;

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private SoundConfigSO soundConfig;

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
            if (clip == null || sfxSource == null) return;
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

        public void PlayBackgroundMusic()
        {
            if (soundConfig != null) PlayMusic(soundConfig.musicBackground);
        }

        // ========== SHORTCUT SFX ==========

        public void PlayTypingSFX()
        {
            if (soundConfig != null) PlaySFX(soundConfig.sfxTyping);
        }

        public void PlayCollectItemSFX()
        {
            if (soundConfig != null) PlaySFX(soundConfig.sfxCollectItem);
        }

        public void PlayInteractSFX()
        {
            if (soundConfig != null) PlaySFX(soundConfig.sfxInteract);
        }

        public void PlayPasscodeInputSFX()
        {
            if (soundConfig != null) PlaySFX(soundConfig.sfxPasscodeInput);
        }

        public void PlayPasscodeWrongSFX()
        {
            if (soundConfig != null) PlaySFX(soundConfig.sfxPasscodeWrong);
        }

        public void PlaySafeOpenSFX()
        {
            if (soundConfig != null) PlaySFX(soundConfig.sfxSafeOpen);
        }

        public void PlayMenuClickSFX()
        {
            if (soundConfig != null) PlaySFX(soundConfig.sfxMenuClick);
        }

        public void PlayNotebookFlipSFX()
        {
            if (soundConfig != null) PlaySFX(soundConfig.sfxNotebookFlip);
        }

        public void PlayStartGameSFX()
        {
            if (soundConfig != null) PlaySFX(soundConfig.sfxStartGame);
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
