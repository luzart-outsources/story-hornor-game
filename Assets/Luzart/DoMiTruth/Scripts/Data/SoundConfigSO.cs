namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Config toàn bộ sound trong game.
    ///
    /// Hệ thống mới dùng <see cref="entries"/> (list SoundEntry) — mỗi entry cấu hình đủ
    /// clip, category, cooldown, pitch random, fade, ...
    ///
    /// Các field legacy (musicBackground, sfxTyping, ...) GIỮ LẠI để:
    ///   1. Không phá asset SoundConfig.asset hiện tại
    ///   2. SoundManager tự build entry runtime từ field legacy nếu chưa có entry tương ứng
    ///      (xem SoundManager.BuildRuntimeMap). Nhờ vậy dự án vẫn chạy ngay khi chưa migrate.
    /// Dùng menu "DoMiTruth/Sound/Migrate Legacy Clips To Entries" để tự sinh entries từ field legacy.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundConfig", menuName = "DoMiTruth/Sound Config")]
    public class SoundConfigSO : ScriptableObject
    {
        // ====================================================================
        // NEW — list entry có đầy đủ option
        // ====================================================================
        [Header("Entries (new system)")]
        [Tooltip("Danh sách sound đầy đủ option. Entry ở đây sẽ override clip legacy cùng ID.")]
        public List<SoundEntry> entries = new List<SoundEntry>();

        // ====================================================================
        // LEGACY — các field cũ, giữ lại để tương thích asset hiện có
        // ====================================================================
        [Header("Legacy — Music")]
        [Tooltip("Nhạc nền chính")]
        public AudioClip musicBackground;

        [Header("Legacy — SFX Dialogue")]
        [Tooltip("Sound đánh chữ khi text hiện ra từng ký tự")]
        public AudioClip sfxTyping;

        [Header("Legacy — SFX Interaction")]
        [Tooltip("Sound khi nhặt đồ (collect clue)")]
        public AudioClip sfxCollectItem;
        [Tooltip("Sound khi tương tác đồ vật ingame")]
        public AudioClip sfxInteract;

        [Header("Legacy — SFX Lock Puzzle")]
        [Tooltip("Sound khi nhập mật khẩu két sắt")]
        public AudioClip sfxPasscodeInput;
        [Tooltip("Sound khi sai mật khẩu")]
        public AudioClip sfxPasscodeWrong;
        [Tooltip("Sound khi mở két sắt thành công")]
        public AudioClip sfxSafeOpen;

        [Header("Legacy — SFX UI")]
        [Tooltip("Sound khi bấm nút menu")]
        public AudioClip sfxMenuClick;
        [Tooltip("Sound lật sách notebook")]
        public AudioClip sfxNotebookFlip;
        [Tooltip("Sound start game")]
        public AudioClip sfxStartGame;

        // ====================================================================
        // Runtime helpers
        // ====================================================================

        /// <summary>
        /// Map SoundId -> AudioClip từ các field legacy. Dùng làm fallback khi entries chưa có.
        /// </summary>
        public AudioClip GetLegacyClip(SoundId id)
        {
            switch (id)
            {
                case SoundId.BGM_Main:          return musicBackground;
                case SoundId.SFX_Typing:        return sfxTyping;
                case SoundId.SFX_CollectItem:   return sfxCollectItem;
                case SoundId.SFX_Interact:      return sfxInteract;
                case SoundId.SFX_PasscodeInput: return sfxPasscodeInput;
                case SoundId.SFX_PasscodeWrong: return sfxPasscodeWrong;
                case SoundId.SFX_SafeOpen:      return sfxSafeOpen;
                case SoundId.SFX_MenuClick:     return sfxMenuClick;
                case SoundId.SFX_NotebookFlip:  return sfxNotebookFlip;
                case SoundId.SFX_StartGame:     return sfxStartGame;
                default: return null;
            }
        }

        /// <summary>
        /// Tạo entry mặc định cho 1 SoundId từ clip legacy.
        /// Áp dụng cooldown & category hợp lý theo loại.
        /// </summary>
        public SoundEntry BuildDefaultEntry(SoundId id)
        {
            var clip = GetLegacyClip(id);
            if (clip == null) return null;

            var e = new SoundEntry
            {
                id = id,
                clips = new[] { clip },
                volume = 1f,
                volumeRandom = new Vector2(1f, 1f),
                pitchRandom = new Vector2(1f, 1f),
                priority = 128,
            };

            switch (id)
            {
                case SoundId.BGM_Main:
                    e.category = SoundCategory.Music;
                    e.loop = true;
                    e.fadeIn = 1f;
                    e.fadeOut = 1f;
                    break;

                case SoundId.SFX_Typing:
                    e.category = SoundCategory.Voice;
                    e.minIntervalBetweenPlays = 0.03f;
                    e.maxConcurrent = 3;
                    e.pitchRandom = new Vector2(0.95f, 1.05f);
                    e.volumeRandom = new Vector2(0.9f, 1f);
                    break;

                case SoundId.SFX_MenuClick:
                    e.category = SoundCategory.UI;
                    e.minIntervalBetweenPlays = 0.05f;
                    break;

                case SoundId.SFX_NotebookFlip:
                    e.category = SoundCategory.UI;
                    e.minIntervalBetweenPlays = 0.15f;
                    break;

                case SoundId.SFX_PasscodeInput:
                    e.category = SoundCategory.UI;
                    e.minIntervalBetweenPlays = 0.04f;
                    e.pitchRandom = new Vector2(0.97f, 1.03f);
                    break;

                case SoundId.SFX_PasscodeWrong:
                case SoundId.SFX_SafeOpen:
                case SoundId.SFX_StartGame:
                    e.category = SoundCategory.UI;
                    break;

                case SoundId.SFX_CollectItem:
                case SoundId.SFX_Interact:
                default:
                    e.category = SoundCategory.SFX;
                    e.minIntervalBetweenPlays = 0.05f;
                    break;
            }

            return e;
        }
    }
}
