using UnityEngine;
using UnityEngine.UI;

namespace Luzart.UIFramework.Examples
{
    public class SettingsPopup : UIPopup
    {
        [Header("UI References")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button applyButton;

        private SettingsController controller;

        protected override void Awake()
        {
            base.Awake();

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyButtonClicked);
        }

        protected override void OnDestroy()
        {
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggled);
            
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            
            if (applyButton != null)
                applyButton.onClick.RemoveListener(OnApplyButtonClicked);

            base.OnDestroy();
        }

        protected override void OnInitialize(object data)
        {
            base.OnInitialize(data);

            var viewModel = new SettingsViewModel();
            var eventBus = UIManager.Instance?.EventBus;

            controller = new SettingsController();
            controller.Setup(this, viewModel, eventBus);
            controller.Initialize();

            UIManager.Instance?.Context.RegisterController<SettingsPopup>(controller);

            if (data is SettingsData settingsData)
            {
                viewModel.MusicVolume = settingsData.MusicVolume;
                viewModel.SfxVolume = settingsData.SfxVolume;
                viewModel.IsFullscreen = settingsData.IsFullscreen;
            }

            Refresh();
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();

            if (controller?.ViewModel != null)
            {
                if (musicVolumeSlider != null)
                    musicVolumeSlider.SetValueWithoutNotify(controller.ViewModel.MusicVolume);
                
                if (sfxVolumeSlider != null)
                    sfxVolumeSlider.SetValueWithoutNotify(controller.ViewModel.SfxVolume);
                
                if (fullscreenToggle != null)
                    fullscreenToggle.SetIsOnWithoutNotify(controller.ViewModel.IsFullscreen);
            }
        }

        protected override void OnDispose()
        {
            controller?.Dispose();
            controller = null;
            base.OnDispose();
        }

        private void OnMusicVolumeChanged(float value)
        {
            controller?.OnMusicVolumeChanged(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            controller?.OnSfxVolumeChanged(value);
        }

        private void OnFullscreenToggled(bool isOn)
        {
            controller?.OnFullscreenToggled(isOn);
        }

        private void OnCloseButtonClicked()
        {
            controller?.OnCloseClicked();
        }

        private void OnApplyButtonClicked()
        {
            controller?.OnApplyClicked();
        }
    }

    public class SettingsData
    {
        public float MusicVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;
        public bool IsFullscreen { get; set; } = true;
    }
}
