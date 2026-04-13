namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DG.Tweening;

    /// <summary>
    /// Màn 3 - NPC Briefing: 2 NPC nói chuyện.
    /// - NPC Trái (full body bên trái) → text hiện trong Case Board.
    /// - NPC Phải (portrait nhỏ dưới phải) → text hiện trong Dialogue Box.
    /// Tự quản lý dialogue flow (không dùng DialogueManager).
    /// </summary>
    public class UIBriefing : UIBase
    {
        [Header("NPC Left (Full Body)")]
        [SerializeField] private Image imgNPCFullBody;
        [SerializeField] private Animator animNPCFullBody;
        [SerializeField] private CanvasGroup canvasGroupNPC;

        [Header("NPC Left Label")]
        [SerializeField] private TMP_Text txtNPCLabel;

        [Header("Case Board (text NPC trái)")]
        [SerializeField] private TMP_Text txtCaseInfo;

        [Header("Dialogue Box (text NPC phải)")]
        [SerializeField] private Image imgDialoguePortrait;
        [SerializeField] private Animator animDialoguePortrait;
        [SerializeField] private TMP_Text txtDialogueName;
        [SerializeField] private TMP_Text txtDialogueText;
        [SerializeField] private GameObject dialogueBoxRoot;

        [Header("Buttons")]
        [SerializeField] private Button btnNext;
        [SerializeField] private Button btnSettings;

        [Header("Highlight")]
        [SerializeField] private float highlightAlpha = 1f;
        [SerializeField] private float dimAlpha = 0.4f;
        [SerializeField] private float highlightDuration = 0.3f;

        private DialogueSequenceSO currentSequence;
        private int currentLineIndex;
        private Action onBriefingComplete;
        private Tweener typingTweener;

        private DialogueCharacterSO leftCharacter;
        private DialogueCharacterSO rightCharacter;
        private Dictionary<string, BriefingCharacterSO> briefingCharMap;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnNext, OnClickNext);

            if (btnSettings != null)
                GameUtil.ButtonOnClick(btnSettings, OnClickSettings);
        }

        public void StartBriefing(DialogueSequenceSO sequence, List<BriefingCharacterSO> briefingCharacters, Action onComplete)
        {
            currentSequence = sequence;
            currentLineIndex = 0;
            onBriefingComplete = onComplete;

            // Build lookup map: characterId -> BriefingCharacterSO
            briefingCharMap = new Dictionary<string, BriefingCharacterSO>();
            if (briefingCharacters != null)
            {
                for (int i = 0; i < briefingCharacters.Count; i++)
                {
                    var bc = briefingCharacters[i];
                    if (bc != null && bc.character != null)
                        briefingCharMap[bc.character.characterId] = bc;
                }
            }

            DetectCharacters(sequence);
            SetupNPCLeft();
            ShowCurrentLine();
        }

        private void DetectCharacters(DialogueSequenceSO sequence)
        {
            leftCharacter = null;
            rightCharacter = null;

            if (sequence == null || sequence.lines == null) return;

            for (int i = 0; i < sequence.lines.Count; i++)
            {
                var c = sequence.lines[i].character;
                if (c == null) continue;

                if (leftCharacter == null)
                {
                    leftCharacter = c;
                }
                else if (rightCharacter == null && c.characterId != leftCharacter.characterId)
                {
                    rightCharacter = c;
                    break;
                }
            }
        }

        private void SetupNPCLeft()
        {
            if (leftCharacter == null) return;

            // Label
            if (txtNPCLabel != null)
                txtNPCLabel.text = leftCharacter.characterName.ToUpper();

            // Full body: từ BriefingCharacterSO
            BriefingCharacterSO bc = null;
            briefingCharMap?.TryGetValue(leftCharacter.characterId, out bc);

            if (imgNPCFullBody != null)
            {
                if (bc != null)
                    SetupAnimatorOrSprite(imgNPCFullBody, animNPCFullBody, bc.fullBodyAnimator, bc.fullBodySprite);
                else
                    SetupAnimatorOrSprite(imgNPCFullBody, animNPCFullBody, null, leftCharacter.portrait);
            }
        }

        private void ShowCurrentLine()
        {
            if (currentSequence == null || currentLineIndex >= currentSequence.lines.Count)
            {
                EndBriefing();
                return;
            }

            var line = currentSequence.lines[currentLineIndex];
            if (line.character == null)
            {
                currentLineIndex++;
                ShowCurrentLine();
                return;
            }

            bool isLeft = leftCharacter != null && line.character.characterId == leftCharacter.characterId;

            if (isLeft)
                ShowLeftLine(line);
            else
                ShowRightLine(line);

            // Highlight
            HighlightNPC(isLeft);

            GameDataManager.Instance.MarkCharacterMet(line.character.characterId);
        }

        /// <summary>NPC trái nói → text trong case board.</summary>
        private void ShowLeftLine(DialogueLine line)
        {
            // Ẩn dialogue box
            if (dialogueBoxRoot != null)
                dialogueBoxRoot.SetActive(false);

            // Text trong case board
            if (txtCaseInfo != null)
            {
                txtCaseInfo.text = "";
                float speed = line.typingSpeed > 0 ? line.typingSpeed : 30f;
                typingTweener = txtCaseInfo.DOSetTextWithSound(line.text, speed);
            }
        }

        /// <summary>NPC phải nói → text trong dialogue box.</summary>
        private void ShowRightLine(DialogueLine line)
        {
            // Hiện dialogue box
            if (dialogueBoxRoot != null)
                dialogueBoxRoot.SetActive(true);

            // Portrait
            if (imgDialoguePortrait != null)
                SetupAnimatorOrSprite(imgDialoguePortrait, animDialoguePortrait, line.character.portraitAnimator, line.character.portrait);

            // Name
            if (txtDialogueName != null)
            {
                txtDialogueName.text = line.character.characterName;
                txtDialogueName.color = line.character.nameColor;
            }

            // Text
            if (txtDialogueText != null)
            {
                txtDialogueText.text = "";
                float speed = line.typingSpeed > 0 ? line.typingSpeed : 30f;
                typingTweener = txtDialogueText.DOSetTextWithSound(line.text, speed);
            }
        }

        private void HighlightNPC(bool leftSpeaking)
        {
            if (canvasGroupNPC != null)
            {
                canvasGroupNPC.DOKill();
                canvasGroupNPC.DOFade(leftSpeaking ? highlightAlpha : dimAlpha, highlightDuration);
            }
        }

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
            briefingCharMap = null;

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
