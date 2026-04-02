namespace Luzart
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "SoundConfig", menuName = "DoMiTruth/Sound Config")]
    public class SoundConfigSO : ScriptableObject
    {
        [Header("Music")]
        [Tooltip("Nhạc nền chính")]
        public AudioClip musicBackground;

        [Header("SFX — Dialogue")]
        [Tooltip("Sound đánh chữ khi text hiện ra từng ký tự")]
        public AudioClip sfxTyping;

        [Header("SFX — Interaction")]
        [Tooltip("Sound khi nhặt đồ (collect clue)")]
        public AudioClip sfxCollectItem;
        [Tooltip("Sound khi tương tác đồ vật ingame")]
        public AudioClip sfxInteract;

        [Header("SFX — Lock Puzzle")]
        [Tooltip("Sound khi nhập mật khẩu két sắt")]
        public AudioClip sfxPasscodeInput;
        [Tooltip("Sound khi sai mật khẩu")]
        public AudioClip sfxPasscodeWrong;
        [Tooltip("Sound khi mở két sắt thành công")]
        public AudioClip sfxSafeOpen;

        [Header("SFX — UI")]
        [Tooltip("Sound khi bấm nút menu")]
        public AudioClip sfxMenuClick;
        [Tooltip("Sound lật sách notebook")]
        public AudioClip sfxNotebookFlip;
        [Tooltip("Sound start game")]
        public AudioClip sfxStartGame;
    }
}
