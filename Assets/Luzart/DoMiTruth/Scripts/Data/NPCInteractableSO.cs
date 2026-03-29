namespace Luzart
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewNPCInteractable", menuName = "DoMiTruth/Interactable/NPC")]
    public class NPCInteractableSO : InteractableObjectSO
    {
        public NPCDialogueTreeSO dialogueTree;

        [Header("Full Body (hiển thị khi nói chuyện)")]
        public Sprite npcFullBodySprite;
        [Tooltip("Optional — nếu null thì dùng Sprite tĩnh.")]
        public RuntimeAnimatorController npcFullBodyAnimator;

        [Header("Fallback (dialogue linear, không branching)")]
        [Tooltip("Nếu dialogueTree null, dùng dialogue linear thay thế.")]
        public DialogueSequenceSO fallbackDialogue;
    }
}
