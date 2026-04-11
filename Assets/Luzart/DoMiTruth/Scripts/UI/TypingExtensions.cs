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
            int lastCharCount = 0;

            return tmp.DOSetTextCharByChar(text, charsPerSecond)
                .OnUpdate(() =>
                {
                    // Đếm ký tự hiện tại đang hiển thị
                    int visibleCount = tmp.textInfo.characterCount;
                    if (visibleCount > lastCharCount)
                    {
                        lastCharCount = visibleCount;
                        SoundManager.Instance?.PlayTypingSFX();
                    }
                })
                .OnKill(() => lastCharCount = 0);
        }
    }
}
