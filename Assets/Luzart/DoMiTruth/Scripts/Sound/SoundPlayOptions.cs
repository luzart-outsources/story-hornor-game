namespace Luzart
{
    using UnityEngine;

    /// <summary>
    /// Override tạm thời cho 1 lần gọi Play (không sửa config asset).
    /// Dùng:
    ///   SoundManager.Instance.Play(SoundId.SFX_MenuClick, new SoundPlayOptions { volume = 0.5f });
    /// </summary>
    public struct SoundPlayOptions
    {
        public float? volume;
        public float? pitch;
        public float? delay;
        public float? fadeIn;
        public bool? loop;
        public Vector3? worldPosition;
        public Transform attachTo;

        public static readonly SoundPlayOptions None = default;
    }
}
