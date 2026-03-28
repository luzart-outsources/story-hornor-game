namespace Luzart
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewLockConfig", menuName = "DoMiTruth/Lock Config")]
    public class LockConfigSO : ScriptableObject
    {
        public LockType lockType;

        [Header("Passcode")]
        public string passcode;

        [Header("Swipe Pattern")]
        [Tooltip("Thứ tự các dot index (0-8) tạo thành pattern. Grid 3x3, index từ trái qua phải, trên xuống dưới.")]
        public int[] swipePattern;

        [Header("Hint")]
        [TextArea(1, 3)] public string hintText;
    }

    public enum LockType
    {
        Passcode = 0,
        SwipePattern = 1,
    }
}
