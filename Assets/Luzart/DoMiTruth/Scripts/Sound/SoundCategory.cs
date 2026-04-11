namespace Luzart
{
    /// <summary>
    /// Category của sound. Mỗi category có volume / mute riêng.
    /// Music  : Nhạc nền, BGM
    /// SFX    : Hiệu ứng chung trong gameplay
    /// UI     : Click button, flip notebook, ...
    /// Ambient: Tiếng môi trường lặp
    /// Voice  : Voice dialogue / NPC speak
    /// </summary>
    public enum SoundCategory
    {
        Music = 0,
        SFX = 1,
        UI = 2,
        Ambient = 3,
        Voice = 4,
    }

    public static class SoundCategoryExt
    {
        public const int Count = 5;
    }
}
