namespace Luzart
{
    using TMPro;
    using DG.Tweening;

    /// <summary>
    /// Extension cho TMP_Text.DOSetTextCharByChar — tự play typing SFX mỗi ký tự.
    /// </summary>
    public static class TypingExtensions
    {
        /// <summary>
        /// Giống DOSetTextCharByChar nhưng play sfxTyping mỗi khi 1 ký tự mới hiện ra.
        /// </summary>
        public static Tweener DOSetTextWithSound(this TMP_Text tmp, string text, float charsPerSecond)
        {
            if (tmp == null) return null;
            text ??= string.Empty;

            return tmp.DOSetTextCharByChar(text, charsPerSecond, visibleChar =>
                {
                    if (char.IsWhiteSpace(visibleChar)) return;
                    SoundManager.Instance?.PlayTypingSFX();
                })
                .OnKill(() =>
                {
                });
        }
    }
}
