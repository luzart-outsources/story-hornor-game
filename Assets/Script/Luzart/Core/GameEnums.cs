namespace Luzart.Core
{
    /// <summary>
    /// Loại khóa cho Room/Map
    /// </summary>
    public enum LockType
    {
        None,           // Không khóa
        Passcode,       // Khóa bằng mật mã
        SwipePuzzle,    // Khóa bằng puzzle vuốt
        KeyItem         // Khóa bằng vật phẩm
    }

    /// <summary>
    /// Danh mục của Clue/Evidence
    /// </summary>
    public enum ClueCategory
    {
        Evidence,       // Bằng chứng vật chất
        Document,       // Tài liệu, thư từ
        Weapon,         // Hung khí
        Testimony,      // Lời khai
        Other           // Khác
    }

    /// <summary>
    /// Danh mục trong Notebook
    /// </summary>
    public enum EntryCategory
    {
        Clue,           // Manh mối
        Character,      // Nhân vật
        Note            // Ghi chú
    }

    /// <summary>
    /// Thời điểm gặp NPC trong Room
    /// </summary>
    public enum EncounterTiming
    {
        BeforeEntry,        // Trước khi vào phòng
        AfterEntry,         // Sau khi vào phòng
        OnObjectInteract    // Khi tương tác với object cụ thể
    }

    /// <summary>
    /// Loại animation
    /// </summary>
    public enum AnimationType
    {
        Scale,          // Phóng to/thu nhỏ
        Fade,           // Mờ dần
        Slide,          // Trượt
        Rotate,         // Xoay
        Custom          // Tùy chỉnh
    }

    /// <summary>
    /// Trạng thái game
    /// </summary>
    public enum GameState
    {
        MainMenu,           // Màn menu chính
        Cutscene,           // Đang xem cutscene
        NPCDialogue,        // Đang đối thoại với NPC
        MapSelection,       // Đang chọn map
        Investigation,      // Đang điều tra
        Paused              // Game tạm dừng
    }
}
