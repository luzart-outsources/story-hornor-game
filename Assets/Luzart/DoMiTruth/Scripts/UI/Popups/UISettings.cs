namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UISettings : UIBase
    {
        [SerializeField] private Slider sliderMusic;
        [SerializeField] private Slider sliderSfx;

        [Header("SO Events")]
        [SerializeField] private GameEventChannel onSettingsChanged;

        protected override void Setup()
        {
            base.Setup();
            sliderMusic?.onValueChanged.AddListener(OnMusicVolumeChanged);
            sliderSfx?.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);

            var data = GameDataManager.Instance.Data;
            if (sliderMusic != null) sliderMusic.SetValueWithoutNotify(data.musicVolume);
            if (sliderSfx != null) sliderSfx.SetValueWithoutNotify(data.sfxVolume);
        }

        private void OnMusicVolumeChanged(float value)
        {
            GameDataManager.Instance.Data.musicVolume = value;
            GameDataManager.Instance.Save();
            onSettingsChanged?.Raise();
        }

        private void OnSfxVolumeChanged(float value)
        {
            GameDataManager.Instance.Data.sfxVolume = value;
            GameDataManager.Instance.Save();
            onSettingsChanged?.Raise();
        }
    }
}
