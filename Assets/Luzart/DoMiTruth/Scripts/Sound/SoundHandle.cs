namespace Luzart
{
    /// <summary>
    /// Handle trả về từ SoundManager.Play, dùng để stop / check 1 instance cụ thể.
    /// Cấu trúc: slot index trong pool + generation (chống dùng handle cũ cho slot đã bị tái sử dụng).
    /// </summary>
    public struct SoundHandle
    {
        public readonly int slot;
        public readonly int generation;

        public static readonly SoundHandle Invalid = new SoundHandle(-1, 0);

        public SoundHandle(int slot, int generation)
        {
            this.slot = slot;
            this.generation = generation;
        }

        public bool IsValid => slot >= 0;
    }
}
