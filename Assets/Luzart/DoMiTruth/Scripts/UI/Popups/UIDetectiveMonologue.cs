namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DG.Tweening;

    /// <summary>
    /// Dialogue nhỏ dưới cùng màn hình — thám tử tự nói/nhận xét.
    /// Chỉ hiện strip nhỏ: portrait + name bar + text. Không có NPC full body.
    /// Dùng cho monologue ngắn trong investigation (VD: "Locked cabinet. Where is the key?").
    /// </summary>
    public class UIDetectiveMonologue : UIBase
    {
        [Header("Portrait")]
        [SerializeField] private Image imgPortrait;

        [Header("Text")]
        [SerializeField] private TMP_Text txtName;
        [SerializeField] private TMP_Text txtDialogue;

        [Header("Buttons")]
        [SerializeField] private Button btnNext;

        private List<DialogueLine> lines;
        private int currentLineIndex;
        private Action onComplete;
        private Tweener typingTweener;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnNext, OnClickNext);
        }

        /// <summary>
        /// Bắt đầu monologue với 1 DialogueSequenceSO.
        /// </summary>
        public void StartMonologue(DialogueSequenceSO sequence, Action onDone)
        {
            if (sequence == null || sequence.lines == null || sequence.lines.Count == 0)
            {
                onDone?.Invoke();
                Hide();
                return;
            }

            lines = sequence.lines;
            currentLineIndex = 0;
            onComplete = onDone;

            ShowCurrentLine();
        }

        /// <summary>
        /// Bắt đầu monologue với 1 dòng text đơn.
        /// </summary>
        public void StartSingleLine(DialogueCharacterSO character, string text, Action onDone)
        {
            lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    character = character,
                    text = text,
                    typingSpeed = 30f,
                    waitForInput = true,
                }
            };
            currentLineIndex = 0;
            onComplete = onDone;

            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            if (lines == null || currentLineIndex >= lines.Count)
            {
                EndMonologue();
                return;
            }

            var line = lines[currentLineIndex];

            // Portrait + name
            if (line.character != null)
            {
                if (imgPortrait != null && line.character.portrait != null)
                    imgPortrait.sprite = line.character.portrait;

                if (txtName != null)
                {
                    txtName.text = line.character.characterName;
                    txtName.color = line.character.nameColor;
                }
            }

            // Text typing
            if (txtDialogue != null)
            {
                txtDialogue.text = "";
                float speed = line.typingSpeed > 0 ? line.typingSpeed : 30f;
                typingTweener = txtDialogue.DOSetTextWithSound(line.text, speed);
            }
        }

        private void OnClickNext()
        {
            // Skip typing nếu đang chạy
            if (typingTweener != null && typingTweener.IsActive() && typingTweener.IsPlaying())
            {
                typingTweener.Complete();
                typingTweener = null;
                return;
            }

            currentLineIndex++;
            ShowCurrentLine();
        }

        private void EndMonologue()
        {
            lines = null;
            currentLineIndex = 0;
            typingTweener = null;

            Hide();

            var callback = onComplete;
            onComplete = null;
            callback?.Invoke();
        }
    }
}
