namespace Luzart
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewLockConfig", menuName = "DoMiTruth/Lock Config")]
    public class LockConfigSO : ScriptableObject
    {
        public LockType lockType;
        public string passcode;
        [TextArea(1, 3)] public string hintText;
    }

    public enum LockType
    {
        Passcode = 0,
        SwipePattern = 1,
    }
}
