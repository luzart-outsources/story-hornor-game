namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DG.Tweening;

    /// <summary>
    /// Màn 3 - NPC Briefing: 2 NPC đứng 2 bên, ai nói thì highlight.
    /// Hỗ trợ cả AnimatorController và Sprite tĩnh (fallback).
    /// Tự quản lý dialogue flow (không dùng DialogueManager).
    /// </summary>
    public class UIBriefing : UIBase
    {
        [Header("NPC Left")]
        [SerializeField] private Image imgNPCLeft;
        [SerializeField] private Animator animNPCLeft;
        [SerializeField] private TMP_Text txtNPCLeftLabel;
        [SerializeField] private CanvasGroup canvasGroupLeft;

        [Header("NPC Right")]
        [SerializeField] private Image imgNPCRight;
        [SerializeField] private Animator animNPCRight;
        [SerializeField] private TMP_Text txtNPCRightLabel;
        [SerializeField] private CanvasGroup canvasGroupRight;

        [Header("Case Board")]
        [SerializeField] private TMP_Text txtCaseInfo;

        [Header("Dialogue Box")]
        [SerializeField] private Image imgDialoguePortrait;
        [SerializeField] private Animator animDialoguePortrait;
        [SerializeField] private TMP_Text txtDialogueName;
        [SerializeField] private TMP_Text txtDialogueText;
        [SerializeField] private Button btnNext;

        [Header("Settings")]
        [SerializeField] private Button btnSettings;

        [Header("Highlight")]
        [SerializeField] private float highlightAlpha = 1f;
        [SerializeField] private float dimAlpha = 0.4f;
        [SerializeField] private float highlightScale = 1.02f;
        [SerializeField] private float highlightDuration = 0.3f;

        private DialogueSequenceSO currentSequence;
        private int currentLineIndex;
        private Action onBriefingComplete;
        private Tweener typingTweener;

        private DialogueCharacterSO leftCharacter;
        private DialogueCharacterSO rightCharacter;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnNext, OnClickNext);

            if (btnSettings != null)
                GameUtil.ButtonOnClick(btnSettings, OnClickSettings);
        }

        /// <summary>
        /// Khởi tạo briefing với dialogue và callback khi xong.
        /// Auto-detect 2 characters từ dialogue lines.
        /// </summary>
        public void StartBriefing(DialogueSequenceSO sequence, Action onComplete)
        {
            currentSequence = sequence;
            currentLineIndex = 0;
            onBriefingComplete = onComplete;

            if (txtCaseInfo != null)
                txtCaseInfo.text = "";

            SetupCharactersFromDialogue(sequence);
            ShowCurrentLine();
        }

        private void SetupCharactersFromDialogue(DialogueSequenceSO sequence)
        {
            leftCharacter = null;
            rightCharacter = null;

            if (sequence == null || sequence.lines == null) return;

            // Scan lines to find 2 unique characters
            for (int i = 0; i < sequence.lines.Count; i++)
            {
                var character = sequence.lines[i].character;
                if (character == null) continue;

                if (leftCharacter == null)
                {
                    leftCharacter = character;
                }
                else if (rightCharacter == null && character.characterId != leftCharacter.characterId)
                {
                    rightCharacter = character;
                    break;
                }
            }

            // Setup left NPC
            SetupNPCSlot(imgNPCLeft, animNPCLeft, txtNPCLeftLabel, canvasGroupLeft, leftCharacter);

            // Setup right NPC
            SetupNPCSlot(imgNPCRight, animNPCRight, txtNPCRightLabel, canvasGroupRight, rightCharacter);
        }

        private void SetupNPCSlot(Image img, Animator anim, TMP_Text label, CanvasGroup canvasGroup, DialogueCharacterSO character)
        {
            if (character == null)
            {
                if (img != null) img.gameObject.SetActive(false);
                if (label != null) label.gameObject.SetActive(false);
                return;
            }

            if (img != null)
            {
                img.gameObject.SetActive(true);
                SetupAnimatorOrSprite(img, anim, character.fullBodyAnimator, character.fullBodySprite ?? character.portrait);
            }

            if (label != null)
            {
                label.gameObject.SetActive(true);
                label.text = character.characterName;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = dimAlpha;
        }

        private void ShowCurrentLine()
        {
            if (currentSequence == null || currentLineIndex >= currentSequence.lines.Count)
            {
                EndBriefing();
                return;
            }

            var line = currentSequence.lines[currentLineIndex];

            // Update character display in dialogue box
            if (line.character != null)
            {
                // Portrait: animator hoặc sprite
                if (imgDialoguePortrait != null)
                    SetupAnimatorOrSprite(imgDialoguePortrait, animDialoguePortrait, line.character.portraitAnimator, line.character.portrait);

                if (txtDialogueName != null)
                {
                    txtDialogueName.text = line.character.characterName;
                    txtDialogueName.color = line.character.nameColor;
                }

                // Highlight NPC đang nói
                HighlightSpeaker(line.character);

                // Mark character met
                GameDataManager.Instance.MarkCharacterMet(line.character.characterId);
            }

            // Animate text
            if (txtDialogueText != null)
            {
                float speed = line.typingSpeed > 0 ? line.typingSpeed : 30f;
                typingTweener = txtDialogueText.DOSetTextCharByChar(line.text, speed);
            }
        }

        /// <summary>
        /// Nếu có AnimatorController → enable Animator + set controller.
        /// Nếu không → disable Animator + set Sprite tĩnh.
        /// </summary>
        private void SetupAnimatorOrSprite(Image img, Animator anim, RuntimeAnimatorController controller, Sprite fallbackSprite)
        {
            if (anim != null)
            {
                if (controller != null)
                {
                    anim.runtimeAnimatorController = controller;
                    anim.enabled = true;
                }
                else
                {
                    anim.enabled = false;
                    if (fallbackSprite != null)
                        img.sprite = fallbackSprite;
                }
            }
            else
            {
                if (fallbackSprite != null)
                    img.sprite = fallbackSprite;
            }
        }

        private void HighlightSpeaker(DialogueCharacterSO speaker)
        {
            bool isLeft = leftCharacter != null && speaker.characterId == leftCharacter.characterId;
            bool isRight = rightCharacter != null && speaker.characterId == rightCharacter.characterId;

            SetSlotHighlight(canvasGroupLeft, imgNPCLeft, isLeft);
            SetSlotHighlight(canvasGroupRight, imgNPCRight, isRight);
        }

        private void SetSlotHighlight(CanvasGroup canvasGroup, Image img, bool isActive)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.DOFade(isActive ? highlightAlpha : dimAlpha, highlightDuration);
            }

            if (img != null)
            {
                img.rectTransform.DOKill();
                float targetScale = isActive ? highlightScale : 1f;
                img.rectTransform.DOScale(targetScale, highlightDuration).SetEase(Ease.OutQuad);
            }
        }

        private void OnClickNext()
        {
            if (typingTweener != null && typingTweener.IsActive() && typingTweener.IsPlaying())
            {
                typingTweener.Complete();
                typingTweener = null;
                return;
            }

            currentLineIndex++;
            ShowCurrentLine();
        }

        private void EndBriefing()
        {
            currentSequence = null;
            currentLineIndex = 0;
            typingTweener = null;
            leftCharacter = null;
            rightCharacter = null;

            Hide();

            var callback = onBriefingComplete;
            onBriefingComplete = null;
            callback?.Invoke();
        }

        private void OnClickSettings()
        {
            UIManager.Instance.ShowUI(UIName.Settings);
        }
    }
}
