using System;
using UnityEngine;

namespace Luzart.UIFramework.Examples
{
    public class SettingsViewModel : UIViewModel
    {
        private float musicVolume = 1f;
        private float sfxVolume = 1f;
        private bool isFullscreen = true;

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                var clampedValue = Mathf.Clamp01(value);
                if (!Mathf.Approximately(musicVolume, clampedValue))
                {
                    musicVolume = clampedValue;
                    NotifyDataChanged();
                }
            }
        }

        public float SfxVolume
        {
            get => sfxVolume;
            set
            {
                var clampedValue = Mathf.Clamp01(value);
                if (!Mathf.Approximately(sfxVolume, clampedValue))
                {
                    sfxVolume = clampedValue;
                    NotifyDataChanged();
                }
            }
        }

        public bool IsFullscreen
        {
            get => isFullscreen;
            set
            {
                if (isFullscreen != value)
                {
                    isFullscreen = value;
                    NotifyDataChanged();
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            musicVolume = 1f;
            sfxVolume = 1f;
            isFullscreen = true;
        }
    }
}
