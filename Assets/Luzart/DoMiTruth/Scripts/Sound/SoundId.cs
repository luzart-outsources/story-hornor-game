namespace Luzart
{
    /// <summary>
    /// ID tập trung cho toàn bộ sound trong game.
    /// Thêm sound mới: thêm enum value ở đây rồi map trong SoundConfigSO.
    /// </summary>
    public enum SoundId
    {
        None = 0,

        // ===== Music =====
        BGM_Main = 100,

        // ===== Dialogue =====
        SFX_Typing = 200,

        // ===== Interaction =====
        SFX_CollectItem = 300,
        SFX_Interact = 301,

        // ===== Lock Puzzle =====
        SFX_PasscodeInput = 400,
        SFX_PasscodeWrong = 401,
        SFX_SafeOpen = 402,

        // ===== UI =====
        SFX_MenuClick = 500,
        SFX_NotebookFlip = 501,
        SFX_StartGame = 502,
    }
}
