namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DG.Tweening;

    /// <summary>
    /// NPC Dialogue Screen — hỗ trợ cả branching (NPCDialogueTreeSO) và linear (DialogueSequenceSO).
    /// Layout: NPC full body trái + board giữa-phải (NPC nói) + dialogue box dưới (Detective nói) + choices.
    /// Phân biệt speaker: NPC → board, Detective → dialogue box dưới.
    /// </summary>
    public class UINPCDialogue : UIBase
    {
        [Header("NPC Full Body (trái)")]
        [SerializeField] private Image imgNPCFullBody;
        [SerializeField] private Animator animNPCFullBody;
        [SerializeField] private CanvasGroup canvasGroupNPC;

        [Header("NPC Label")]
        [SerializeField] private TMP_Text txtNPCLabel;

        [Header("Board (text NPC nói)")]
        [SerializeField] private TMP_Text txtBoardText;

        [Header("Detective Dialogue Box (dưới)")]
        [SerializeField] private GameObject detectiveDialogueBox;
        [SerializeField] private TMP_Text txtDetectiveText;
        [SerializeField] private Image imgDetectivePortrait;
        [SerializeField] private Animator animDetectivePortrait;

        [Header("Choice Buttons (dynamic pool)")]
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private ChoiceButtonItem choiceButtonPrefab;
        public List<ChoiceButtonItem> choiceItems = new List<ChoiceButtonItem>();

        [Header("Buttons")]
        [SerializeField] private Button btnNext;
        [SerializeField] private Button btnSettings;

        [Header("Highlight")]
        [SerializeField] private float highlightAlpha = 1f;
        [SerializeField] private float dimAlpha = 0.5f;
        [SerializeField] private float highlightDuration = 0.3f;

        // State
        private NPCDialogueTreeSO currentTree;
        private DialogueSequenceSO currentLinearSequence;
        private DialogueCharacterSO playerCharacter;
        private Action onDialogueComplete;
        private Tweener typingTweener;

        // Branching state
        private enum DialoguePhase { Greeting, Choices, DetectiveSpeaking, Response, Farewell }
        private DialoguePhase currentPhase;
        private int currentLineIndex;
        private List<DialogueLine> currentLines;
        private HashSet<int> chosenIndices = new HashSet<int>();

        // Pending response sau khi Detective nói xong
        private DialogueChoice pendingChoice;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnNext, OnClickNext);

            if (btnSettings != null)
                GameUtil.ButtonOnClick(btnSettings, OnClickSettings);
        }

        // ========== BRANCHING DIALOGUE ==========

        public void StartBranchingDialogue(NPCDialogueTreeSO tree, Sprite fullBodySprite,
            RuntimeAnimatorController fullBodyAnimator, Action onComplete)
        {
            currentTree = tree;
            currentLinearSequence = null;
            playerCharacter = tree.playerCharacter;
            onDialogueComplete = onComplete;
            chosenIndices.Clear();
            pendingChoice = null;

            SetupNPCDisplay(tree.npcCharacter, fullBodySprite, fullBodyAnimator);

            if (tree.playerCharacter != null)
                SetupAnimatorOrSprite(imgDetectivePortrait, animDetectivePortrait,
                    tree.playerCharacter.portraitAnimator, tree.playerCharacter.portrait);

            HideChoices();
            HideDetectiveDialogue();
            HighlightNPC();
            StartPhase(DialoguePhase.Greeting, tree.greetingLines);
        }

        private void SetupNPCDisplay(DialogueCharacterSO npc, Sprite fullBody, RuntimeAnimatorController fullBodyAnim)
        {
            if (npc == null) return;

            if (txtNPCLabel != null)
                txtNPCLabel.text = npc.characterName.ToUpper();

            SetupAnimatorOrSprite(imgNPCFullBody, animNPCFullBody, fullBodyAnim, fullBody);
            GameDataManager.Instance.MarkCharacterMet(npc.characterId);
        }

        private void StartPhase(DialoguePhase phase, List<DialogueLine> lines)
        {
            currentPhase = phase;
            currentLines = lines;
            currentLineIndex = 0;

            if (lines == null || lines.Count == 0)
            {
                OnPhaseComplete();
                return;
            }

            ShowNextButton(true);
            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            if (currentLines == null || currentLineIndex >= currentLines.Count)
            {
                OnPhaseComplete();
                return;
            }

            var line = currentLines[currentLineIndex];
            HideChoices();
            ShowNextButton(true);

            bool isDetective = IsDetectiveSpeaking(line);

            if (isDetective)
            {
                // Detective nói → dialogue box dưới
                HighlightDetective();
                ShowDetectiveDialogue();
                ClearBoard();

                if (txtDetectiveText != null)
                {
                    txtDetectiveText.text = "";
                    float speed = line.typingSpeed > 0 ? line.typingSpeed : 30f;
                    typingTweener = txtDetectiveText.DOSetTextCharByChar(line.text, speed);
                }
            }
            else
            {
                // NPC nói → board
                HighlightNPC();
                HideDetectiveDialogue();

                if (txtNPCLabel != null && line.character != null)
                    txtNPCLabel.text = line.character.characterName.ToUpper();

                if (txtBoardText != null)
                {
                    txtBoardText.text = "";
                    float speed = line.typingSpeed > 0 ? line.typingSpeed : 30f;
                    typingTweener = txtBoardText.DOSetTextCharByChar(line.text, speed);
                }
            }
        }

        private bool IsDetectiveSpeaking(DialogueLine line)
        {
            if (line.character == null || playerCharacter == null) return false;
            return line.character == playerCharacter;
        }

        private void OnPhaseComplete()
        {
            switch (currentPhase)
            {
                case DialoguePhase.Greeting:
                    ShowChoices();
                    break;

                case DialoguePhase.Response:
                    // Chọn 1 choice → respond xong → Farewell luôn
                    StartPhase(DialoguePhase.Farewell, currentTree?.farewellLines);
                    break;

                case DialoguePhase.DetectiveSpeaking:
                    // Detective nói xong → NPC respond
                    HideDetectiveDialogue();
                    if (pendingChoice != null)
                    {
                        // Đổi NPC mood
                        if (pendingChoice.npcMoodAnimator != null || pendingChoice.npcMoodSprite != null)
                        {
                            SetupAnimatorOrSprite(imgNPCFullBody, animNPCFullBody,
                                pendingChoice.npcMoodAnimator, pendingChoice.npcMoodSprite);
                        }

                        // Auto collect testimony
                        if (pendingChoice.testimonyCue != null)
                            GameDataManager.Instance.AddClue(pendingChoice.testimonyCue.clueId);

                        var responseLines = pendingChoice.responseLines;
                        pendingChoice = null;
                        StartPhase(DialoguePhase.Response, responseLines);
                    }
                    else
                    {
                        ShowChoices();
                    }
                    break;

                case DialoguePhase.Farewell:
                    EndDialogue();
                    break;
            }
        }

        // ========== CHOICES (dynamic pool) ==========

        private void ShowChoices()
        {
            if (currentTree == null || currentTree.choices == null || currentTree.choices.Count == 0)
            {
                StartPhase(DialoguePhase.Farewell, currentTree?.farewellLines);
                return;
            }

            var available = new List<int>();
            for (int i = 0; i < currentTree.choices.Count; i++)
            {
                if (!currentTree.choices[i].hideAfterChosen || !chosenIndices.Contains(i))
                    available.Add(i);
            }

            if (available.Count == 0)
            {
                StartPhase(DialoguePhase.Farewell, currentTree?.farewellLines);
                return;
            }

            currentPhase = DialoguePhase.Choices;
            ShowNextButton(false);
            HideDetectiveDialogue();
            HighlightDetective(); // Detective đang chọn câu hỏi

            if (choiceContainer != null)
                choiceContainer.gameObject.SetActive(true);

            MasterHelper.InitListObj(available.Count, choiceButtonPrefab, choiceItems, choiceContainer, (item, btnIdx) =>
            {
                item.gameObject.SetActive(true);
                int choiceIdx = available[btnIdx];
                var choice = currentTree.choices[choiceIdx];
                item.Init(choice.choiceLabel, () => OnChoiceSelected(choiceIdx));
            });
        }

        private void HideChoices()
        {
            if (choiceContainer != null)
                choiceContainer.gameObject.SetActive(false);
        }

        private void OnChoiceSelected(int choiceIndex)
        {
            if (currentTree == null || choiceIndex < 0 || choiceIndex >= currentTree.choices.Count) return;

            var choice = currentTree.choices[choiceIndex];
            chosenIndices.Add(choiceIndex);
            HideChoices();

            // Detective nói choiceLabel trước → rồi NPC respond
            pendingChoice = choice;

            // Tạo line cho Detective nói
            var detectiveLine = new DialogueLine
            {
                character = playerCharacter,
                text = choice.choiceLabel,
                typingSpeed = 30f,
                waitForInput = true
            };

            StartPhase(DialoguePhase.DetectiveSpeaking, new List<DialogueLine> { detectiveLine });
        }

        // ========== LINEAR DIALOGUE (fallback) ==========

        public void StartLinearDialogue(DialogueSequenceSO sequence, Sprite fullBodySprite,
            RuntimeAnimatorController fullBodyAnimator, DialogueCharacterSO npc, Action onComplete)
        {
            currentTree = null;
            currentLinearSequence = sequence;
            playerCharacter = null; // Linear không phân biệt
            onDialogueComplete = onComplete;
            pendingChoice = null;

            SetupNPCDisplay(npc, fullBodySprite, fullBodyAnimator);
            HideChoices();
            HideDetectiveDialogue();

            currentLines = sequence.lines;
            currentLineIndex = 0;
            currentPhase = DialoguePhase.Greeting;
            ShowNextButton(true);
            ShowCurrentLine();
        }

        // ========== LEGACY (dùng bởi DialogueManager cho linear đơn giản) ==========

        public void DisplayLine(DialogueLine line)
        {
            if (line.character != null)
            {
                if (imgNPCFullBody != null)
                    SetupAnimatorOrSprite(imgNPCFullBody, animNPCFullBody,
                        line.character.portraitAnimator, line.character.portrait);

                if (txtNPCLabel != null)
                    txtNPCLabel.text = line.character.characterName.ToUpper();
            }

            if (txtBoardText != null)
            {
                float speed = line.typingSpeed > 0 ? line.typingSpeed : 30f;
                var tweener = txtBoardText.DOSetTextCharByChar(line.text, speed);
                DialogueManager.Instance.SetTypingTweener(tweener);
            }
        }

        // ========== HIGHLIGHT (ai đang nói) ==========

        private void HighlightNPC()
        {
            if (canvasGroupNPC != null)
                canvasGroupNPC.DOFade(highlightAlpha, highlightDuration);

            // Dim detective portrait
            if (imgDetectivePortrait != null)
            {
                var detColor = imgDetectivePortrait.color;
                detColor.a = dimAlpha;
                imgDetectivePortrait.DOFade(dimAlpha, highlightDuration);
            }
        }

        private void HighlightDetective()
        {
            if (canvasGroupNPC != null)
                canvasGroupNPC.DOFade(dimAlpha, highlightDuration);

            if (imgDetectivePortrait != null)
                imgDetectivePortrait.DOFade(highlightAlpha, highlightDuration);
        }

        // ========== DETECTIVE DIALOGUE BOX ==========

        private void ShowDetectiveDialogue()
        {
            if (detectiveDialogueBox != null)
                detectiveDialogueBox.SetActive(true);
        }

        private void HideDetectiveDialogue()
        {
            if (detectiveDialogueBox != null)
                detectiveDialogueBox.SetActive(false);
        }

        private void ClearBoard()
        {
            if (txtBoardText != null)
                txtBoardText.text = "";
        }

        // ========== COMMON ==========

        private void OnClickNext()
        {
            if (typingTweener != null && typingTweener.IsActive() && typingTweener.IsPlaying())
            {
                typingTweener.Complete();
                typingTweener = null;
                return;
            }

            // Legacy linear qua DialogueManager
            if (currentTree == null && currentLinearSequence == null)
            {
                DialogueManager.Instance.ShowNextLine();
                return;
            }

            currentLineIndex++;
            ShowCurrentLine();
        }

        private void ShowNextButton(bool show)
        {
            if (btnNext != null)
                btnNext.gameObject.SetActive(show);
        }

        private void EndDialogue()
        {
            currentTree = null;
            currentLinearSequence = null;
            currentLines = null;
            playerCharacter = null;
            currentLineIndex = 0;
            typingTweener = null;
            chosenIndices.Clear();
            pendingChoice = null;

            HideDetectiveDialogue();
            Hide();

            var callback = onDialogueComplete;
            onDialogueComplete = null;
            callback?.Invoke();
        }

        private void SetupAnimatorOrSprite(Image img, Animator anim,
            RuntimeAnimatorController controller, Sprite fallbackSprite)
        {
            if (img == null) return;

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

        private void OnClickSettings()
        {
            UIManager.Instance.ShowUI(UIName.Settings);
        }
    }
}
