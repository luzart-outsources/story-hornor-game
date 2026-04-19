namespace Luzart
{
    using System;
    using UnityEngine;

    public enum PrerequisiteType
    {
        HasClue = 0,        // Đã thu thập ClueSO này
        HasInteracted = 1,  // Đã tương tác với InteractableObjectSO này
        IsUnlocked = 2,     // Đã unlock InteractableObjectSO này (LockPuzzle)
    }

    [Serializable]
    public class PrerequisiteConfig
    {
        public PrerequisiteType type;

        [Tooltip("Dùng khi type = HasClue")]
        public ClueSO clueRef;

        [Tooltip("Dùng khi type = HasInteracted hoặc IsUnlocked")]
        public InteractableObjectSO interactableRef;

        [Tooltip("Đảo ngược kết quả. VD: negate + IsUnlocked = hiện khi CHƯA unlock.")]
        public bool negate;

        public bool Evaluate()
        {
            var gdm = GameDataManager.Instance;
            if (gdm == null) return false;

            bool result;
            switch (type)
            {
                case PrerequisiteType.HasClue:
                    result = clueRef != null && gdm.HasClue(clueRef.clueId);
                    break;

                case PrerequisiteType.HasInteracted:
                    result = interactableRef != null && gdm.HasInteracted(interactableRef.objectId);
                    break;

                case PrerequisiteType.IsUnlocked:
                    result = interactableRef != null && gdm.IsItemUnlocked(interactableRef.objectId);
                    break;

                default:
                    result = false;
                    break;
            }

            return negate ? !result : result;
        }
    }
}
