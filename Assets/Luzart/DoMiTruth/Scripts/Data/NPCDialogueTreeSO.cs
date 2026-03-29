namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewNPCDialogueTree", menuName = "DoMiTruth/NPC Dialogue Tree")]
    public class NPCDialogueTreeSO : ScriptableObject
    {
        public string dialogueId;
        public DialogueCharacterSO npcCharacter;
        public DialogueCharacterSO playerCharacter;

        [Header("Greeting (NPC nói khi gặp lần đầu)")]
        public List<DialogueLine> greetingLines = new List<DialogueLine>();

        [Header("Choices (Detective chọn câu hỏi)")]
        public List<DialogueChoice> choices = new List<DialogueChoice>();

        [Header("Farewell (optional, sau khi hết choices)")]
        public List<DialogueLine> farewellLines = new List<DialogueLine>();
    }

    [Serializable]
    public class DialogueChoice
    {
        [Tooltip("Text hiện trên button lựa chọn")]
        public string choiceLabel;

        [Header("NPC Mood khi trả lời")]
        [Tooltip("Đổi animation NPC khi trả lời choice này. Null = giữ nguyên.")]
        public RuntimeAnimatorController npcMoodAnimator;
        [Tooltip("Fallback sprite nếu không có animator.")]
        public Sprite npcMoodSprite;

        [Header("Response")]
        public List<DialogueLine> responseLines = new List<DialogueLine>();

        [Header("Auto Collect")]
        [Tooltip("Tự động collect clue/testimony khi nghe xong. Null = không collect.")]
        public ClueSO testimonyCue;

        [Tooltip("Ẩn choice này sau khi đã chọn")]
        public bool hideAfterChosen = true;
    }
}
