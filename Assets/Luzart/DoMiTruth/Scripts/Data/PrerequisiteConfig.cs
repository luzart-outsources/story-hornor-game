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

        public bool Evaluate()
        {
            var gdm = GameDataManager.Instance;
            if (gdm == null) return false;

            switch (type)
            {
                case PrerequisiteType.HasClue:
                    return clueRef != null && gdm.HasClue(clueRef.clueId);

                case PrerequisiteType.HasInteracted:
                    return interactableRef != null && gdm.HasInteracted(interactableRef.objectId);

                case PrerequisiteType.IsUnlocked:
                    return interactableRef != null && gdm.IsItemUnlocked(interactableRef.objectId);

                default:
                    return false;
            }
        }
    }
}
