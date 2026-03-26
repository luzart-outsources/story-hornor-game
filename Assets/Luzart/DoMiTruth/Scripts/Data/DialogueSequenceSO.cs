namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "DoMiTruth/Dialogue Sequence")]
    public class DialogueSequenceSO : ScriptableObject
    {
        public string dialogueId;
        public List<DialogueLine> lines = new List<DialogueLine>();
        public bool autoAdvance;
        public float autoAdvanceDelay = 2f;
    }

    [Serializable]
    public class DialogueLine
    {
        public DialogueCharacterSO character;
        [TextArea(2, 5)] public string text;
        public float typingSpeed = 30f;
        public bool waitForInput = true;
    }
}
