namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    public class InteractableObjectSO : ScriptableObject
    {
        [Header("Base")]
        public string objectId;
        public Vector2 hitboxSize = new Vector2(100f, 100f);
        public bool isOneTimeOnly;
        public Sprite highlightSprite;

        [Header("Visibility on Room Load (AND logic)")]
        [Tooltip("Điều kiện để prop HIỆN khi vào room. Để trống = luôn hiện. Tất cả phải thỏa.")]
        public List<PrerequisiteConfig> showConditions = new List<PrerequisiteConfig>();

        [Header("Prerequisites (AND logic — tất cả phải thỏa)")]
        [Tooltip("Để trống = không cần điều kiện")]
        public List<PrerequisiteConfig> prerequisites = new List<PrerequisiteConfig>();

        [Header("Actions khi chưa đủ điều kiện")]
        public List<ActionStepConfig> onPrerequisiteNotMet = new List<ActionStepConfig>();

        [Header("Actions khi đủ điều kiện (hoặc không có prerequisite)")]
        public List<ActionStepConfig> onInteract = new List<ActionStepConfig>();

        /// <summary>Kiểm tra tất cả prerequisites (AND logic).</summary>
        public bool CheckPrerequisites()
        {
            if (prerequisites == null || prerequisites.Count == 0) return true;

            foreach (var p in prerequisites)
            {
                if (!p.Evaluate()) return false;
            }
            return true;
        }
    }
}
