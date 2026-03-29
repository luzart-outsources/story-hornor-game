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
    /// Layout: NPC full body trái + board giữa-phải + choice buttons dưới + Detective portrait.
    /// Tự quản lý dialogue flow (không dùng DialogueManager).
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

        [Header("Choice Buttons (dynamic pool)")]
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private ChoiceButtonItem choiceButtonPrefab;
        public List<ChoiceButtonItem> choiceItems = new List<ChoiceButtonItem>();

        [Header("Detective Portrait (góc dưới phải)")]
        [SerializeField] private Image imgDetectivePortrait;
        [SerializeField] private Animator animDetectivePortrait;

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
        private Action onDialogueComplete;
        private Tweener typingTweener;

        // Branching state
        private enum DialoguePhase { Greeting, Choices, Response, Farewell }
        private DialoguePhase currentPhase;
        private int currentLineIndex;
        private List<DialogueLine> currentLines;
        private HashSet<int> chosenIndices = new HashSet<int>();

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
            onDialogueComplete = onComplete;
            chosenIndices.Clear();

            SetupNPCDisplay(tree.npcCharacter, fullBodySprite, fullBodyAnimator);

            if (tree.playerCharacter != null)
                SetupAnimatorOrSprite(imgDetectivePortrait, animDetectivePortrait,
                    tree.playerCharacter.portraitAnimator, tree.playerCharacter.portrait);

            HideChoices();
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
            ShowCurrentBranchingLine();
        }

        private void ShowCurrentBranchingLine()
        {
            if (currentLines == null || currentLineIndex >= currentLines.Count)
            {
                OnPhaseComplete();
                return;
            }

            var line = currentLines[currentLineIndex];
            HideChoices();
            ShowNextButton(true);

            if (txtBoardText != null)
            {
                txtBoardText.text = "";
                float speed = line.typingSpeed > 0 ? line.typingSpeed : 30f;
                typingTweener = txtBoardText.DOSetTextCharByChar(line.text, speed);
            }
        }

        private void OnPhaseComplete()
        {
            switch (currentPhase)
            {
                case DialoguePhase.Greeting:
                case DialoguePhase.Response:
                    ShowChoices();
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

            // Lọc available choices
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

            if (choiceContainer != null)
                choiceContainer.gameObject.SetActive(true);

            // Dùng MasterHelper.InitListObj để spawn/reuse buttons
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

            // Đổi NPC mood animation
            if (choice.npcMoodAnimator != null || choice.npcMoodSprite != null)
            {
                SetupAnimatorOrSprite(imgNPCFullBody, animNPCFullBody,
                    choice.npcMoodAnimator, choice.npcMoodSprite);
            }

            HideChoices();

            // Auto collect testimony
            if (choice.testimonyCue != null)
            {
                GameDataManager.Instance.AddClue(choice.testimonyCue.clueId);
            }

            StartPhase(DialoguePhase.Response, choice.responseLines);
        }

        // ========== LINEAR DIALOGUE (fallback) ==========

        public void StartLinearDialogue(DialogueSequenceSO sequence, Sprite fullBodySprite,
            RuntimeAnimatorController fullBodyAnimator, DialogueCharacterSO npc, Action onComplete)
        {
            currentTree = null;
            currentLinearSequence = sequence;
            onDialogueComplete = onComplete;

            SetupNPCDisplay(npc, fullBodySprite, fullBodyAnimator);
            HideChoices();

            currentLines = sequence.lines;
            currentLineIndex = 0;
            currentPhase = DialoguePhase.Greeting;
            ShowNextButton(true);
            ShowCurrentBranchingLine();
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
            ShowCurrentBranchingLine();
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
            currentLineIndex = 0;
            typingTweener = null;
            chosenIndices.Clear();

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
