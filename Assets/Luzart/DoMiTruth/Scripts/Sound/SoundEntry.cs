namespace Luzart
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Định nghĩa 1 sound: clip, category, random pitch/volume, cooldown, concurrency, loop, ...
    /// Được cấu hình trong SoundConfigSO.
    /// </summary>
    [Serializable]
    public class SoundEntry
    {
        [Tooltip("ID gọi từ code (SoundManager.Instance.Play(id))")]
        public SoundId id = SoundId.None;

        [Tooltip("Category để tính volume & mute")]
        public SoundCategory category = SoundCategory.SFX;

        [Tooltip("Danh sách clip. Nếu có >1 sẽ random mỗi lần Play (chống lặp nghe máy móc)")]
        public AudioClip[] clips;

        [Header("Volume / Pitch")]
        [Range(0f, 1f)]
        [Tooltip("Volume cơ bản của entry (nhân với volume category & master)")]
        public float volume = 1f;

        [Tooltip("Random volume mỗi lần play (giá trị cuối = volume * Random(volumeRandom.x, volumeRandom.y))")]
        public Vector2 volumeRandom = new Vector2(1f, 1f);

        [Tooltip("Random pitch mỗi lần play")]
        public Vector2 pitchRandom = new Vector2(1f, 1f);

        [Header("Throttle / Concurrency")]
        [Tooltip("Thời gian tối thiểu (giây) giữa 2 lần play liên tiếp cùng 1 ID. 0 = không giới hạn")]
        public float minIntervalBetweenPlays = 0f;

        [Tooltip("Số instance tối đa cùng phát của ID này. 0 = không giới hạn")]
        public int maxConcurrent = 0;

        [Tooltip("Khi đạt maxConcurrent: true = cắt instance cũ nhất, false = bỏ qua lần play mới")]
        public bool stealOldestWhenFull = true;

        [Tooltip("Gọi lại sẽ cắt tất cả instance đang chạy của chính ID này")]
        public bool interruptSelf = false;

        [Header("Playback")]
        public bool loop = false;

        [Tooltip("Ưu tiên AudioSource (0 = highest, 256 = lowest). Unity mặc định 128.")]
        [Range(0, 256)]
        public int priority = 128;

        [Tooltip("Sound vẫn chơi khi game bị Pause (ignoreListenerPause)")]
        public bool ignoreListenerPause = false;

        [Tooltip("0 = 2D, 1 = 3D full. UI sound nên để 0.")]
        [Range(0f, 1f)]
        public float spatialBlend = 0f;

        [Header("Fade")]
        [Tooltip("Fade in giây khi play. 0 = không fade")]
        public float fadeIn = 0f;

        [Tooltip("Fade out giây khi stop. 0 = stop ngay")]
        public float fadeOut = 0f;

        // ---------- Runtime helpers ----------

        public AudioClip PickClip()
        {
            if (clips == null || clips.Length == 0) return null;
            if (clips.Length == 1) return clips[0];
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }

        public float PickVolume()
        {
            float mult = UnityEngine.Random.Range(
                Mathf.Min(volumeRandom.x, volumeRandom.y),
                Mathf.Max(volumeRandom.x, volumeRandom.y));
            return Mathf.Clamp01(volume * mult);
        }

        public float PickPitch()
        {
            return UnityEngine.Random.Range(
                Mathf.Min(pitchRandom.x, pitchRandom.y),
                Mathf.Max(pitchRandom.x, pitchRandom.y));
        }
    }
}
