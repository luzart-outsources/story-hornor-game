namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewInteractable", menuName = "DoMiTruth/Interactable Object")]
    public class InteractableObjectSO : ScriptableObject
    {
        public string objectId;
        public InteractType interactType;
        public Vector2 hitboxSize = new Vector2(100f, 100f);
        public bool isOneTimeOnly;
        public Sprite highlightSprite;

        [Header("Clue Type")]
        public ClueSO clue;

        [Header("NPC Type")]
        public DialogueSequenceSO dialogue;

        [Header("Locked Item Type")]
        public LockConfigSO lockConfig;
        public List<ActionStepConfig> onUnlockSuccess = new List<ActionStepConfig>();
        public List<ActionStepConfig> onUnlockFail = new List<ActionStepConfig>();
    }

    public enum InteractType
    {
        Clue = 0,
        NPC = 1,
        LockedItem = 2,
        Decoration = 3,
    }
}
