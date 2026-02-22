using UnityEngine;

namespace Luzart.UIFramework.Examples
{
    public class SettingsController : UIController<SettingsPopup, SettingsViewModel>
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
            LoadSettings();
        }

        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
        }

        public void OnMusicVolumeChanged(float value)
        {
            ViewModel.MusicVolume = value;
        }

        public void OnSfxVolumeChanged(float value)
        {
            ViewModel.SfxVolume = value;
        }

        public void OnFullscreenToggled(bool isOn)
        {
            ViewModel.IsFullscreen = isOn;
        }

        public void OnApplyClicked()
        {
            SaveSettings();
            EventBus?.Publish(new SettingsAppliedEvent(
                ViewModel.MusicVolume,
                ViewModel.SfxVolume,
                ViewModel.IsFullscreen
            ));
            
            Debug.Log("Settings applied successfully");
        }

        public void OnCloseClicked()
        {
            UIManager.Instance?.Hide<SettingsPopup>();
        }

        private void LoadSettings()
        {
            ViewModel.MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            ViewModel.SfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
            ViewModel.IsFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("MusicVolume", ViewModel.MusicVolume);
            PlayerPrefs.SetFloat("SfxVolume", ViewModel.SfxVolume);
            PlayerPrefs.SetInt("Fullscreen", ViewModel.IsFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public class SettingsAppliedEvent : UIEvent
    {
        public float MusicVolume { get; }
        public float SfxVolume { get; }
        public bool IsFullscreen { get; }

        public SettingsAppliedEvent(float musicVolume, float sfxVolume, bool isFullscreen)
        {
            MusicVolume = musicVolume;
            SfxVolume = sfxVolume;
            IsFullscreen = isFullscreen;
        }
    }
}
