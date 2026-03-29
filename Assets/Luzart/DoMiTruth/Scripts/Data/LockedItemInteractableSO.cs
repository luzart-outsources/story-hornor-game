namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewLockedInteractable", menuName = "DoMiTruth/Interactable/Locked Item")]
    public class LockedItemInteractableSO : InteractableObjectSO
    {
        public LockConfigSO lockConfig;
        public List<ActionStepConfig> onUnlockSuccess = new List<ActionStepConfig>();
        public List<ActionStepConfig> onUnlockFail = new List<ActionStepConfig>();
    }
}
