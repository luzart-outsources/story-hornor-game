namespace Luzart
{
    using System;
    using UnityEngine;
    using DG.Tweening;

    public class DialogueManager : Singleton<DialogueManager>
    {
        private DialogueSequenceSO currentSequence;
        private int currentLineIndex;
        private Action onDialogueComplete;
        private UINPCDialogue dialogueUI;
        private Tweener typingTweener;

        public void StartDialogue(DialogueSequenceSO sequence, Action onComplete = null)
        {
            if (sequence == null || sequence.lines.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            currentSequence = sequence;
            currentLineIndex = 0;
            onDialogueComplete = onComplete;

            dialogueUI = UIManager.Instance.ShowUI<UINPCDialogue>(UIName.NPCDialogue);
            if (dialogueUI != null)
            {
                ShowCurrentLine();
            }
        }

        private void ShowCurrentLine()
        {
            if (currentLineIndex >= currentSequence.lines.Count)
            {
                EndDialogue();
                return;
            }

            var line = currentSequence.lines[currentLineIndex];
            dialogueUI.DisplayLine(line);

            if (line.character != null)
            {
                GameDataManager.Instance.MarkCharacterMet(line.character.characterId);
            }
        }

        public void ShowNextLine()
        {
            if (typingTweener != null && typingTweener.IsActive() && typingTweener.IsPlaying())
            {
                SkipTyping();
                return;
            }

            currentLineIndex++;
            ShowCurrentLine();
        }

        public void SkipTyping()
        {
            if (typingTweener != null && typingTweener.IsActive())
            {
                typingTweener.Complete();
                typingTweener = null;
            }
        }

        public void SetTypingTweener(Tweener tweener)
        {
            typingTweener = tweener;
        }

        public void EndDialogue()
        {
            UIManager.Instance.HideUiActive(UIName.NPCDialogue);

            currentSequence = null;
            currentLineIndex = 0;
            typingTweener = null;

            var callback = onDialogueComplete;
            onDialogueComplete = null;
            callback?.Invoke();
        }
    }
}
