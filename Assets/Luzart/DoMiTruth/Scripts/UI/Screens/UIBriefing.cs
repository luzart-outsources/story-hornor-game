namespace Luzart
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DG.Tweening;

    /// <summary>
    /// Màn 3 - NPC Briefing: Công an giao nhiệm vụ cho thám tử.
    /// Full screen với NPC full body bên trái, bảng tin vụ án ở giữa,
    /// dialogue box bên dưới với portrait + text.
    /// Tự quản lý dialogue flow (không dùng DialogueManager).
    /// </summary>
    public class UIBriefing : UIBase
    {
        [Header("NPC Display")]
        [SerializeField] private Image imgNPCFullBody;
        [SerializeField] private TMP_Text txtNPCLabel;

        [Header("Case Board")]
        [SerializeField] private TMP_Text txtCaseInfo;

        [Header("Dialogue Box")]
        [SerializeField] private Image imgDialoguePortrait;
        [SerializeField] private TMP_Text txtDialogueName;
        [SerializeField] private TMP_Text txtDialogueText;
        [SerializeField] private Button btnNext;

        [Header("Settings")]
        [SerializeField] private Button btnSettings;

        private DialogueSequenceSO currentSequence;
        private int currentLineIndex;
        private Action onBriefingComplete;
        private Tweener typingTweener;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnNext, OnClickNext);

            if (btnSettings != null)
                GameUtil.ButtonOnClick(btnSettings, OnClickSettings);
        }

        /// <summary>
        /// Khởi tạo briefing với dialogue và callback khi xong.
        /// </summary>
        public void StartBriefing(DialogueSequenceSO sequence, Action onComplete)
        {
            currentSequence = sequence;
            currentLineIndex = 0;
            onBriefingComplete = onComplete;

            if (txtCaseInfo != null)
                txtCaseInfo.text = "";

            ShowCurrentLine();
        }

        /// <summary>
        /// Set sprite full body NPC (Công an).
        /// </summary>
        public void SetNPCFullBody(Sprite sprite, string npcLabel = null)
        {
            if (imgNPCFullBody != null && sprite != null)
            {
                imgNPCFullBody.sprite = sprite;
                imgNPCFullBody.gameObject.SetActive(true);
            }

            if (txtNPCLabel != null)
                txtNPCLabel.text = npcLabel ?? "";
        }

        private void ShowCurrentLine()
        {
            if (currentSequence == null || currentLineIndex >= currentSequence.lines.Count)
            {
                EndBriefing();
                return;
            }

            var line = currentSequence.lines[currentLineIndex];

            // Update character display
            if (line.character != null)
            {
                if (imgDialoguePortrait != null && line.character.portrait != null)
                    imgDialoguePortrait.sprite = line.character.portrait;

                if (txtDialogueName != null)
                {
                    txtDialogueName.text = line.character.characterName;
                    txtDialogueName.color = line.character.nameColor;
                }

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

        private void OnClickNext()
        {
            // Nếu đang typing → skip typing trước
            if (typingTweener != null && typingTweener.IsActive() && typingTweener.IsPlaying())
            {
                typingTweener.Complete();
                typingTweener = null;
                return;
            }

            // Chuyển sang dòng tiếp theo
            currentLineIndex++;
            ShowCurrentLine();
        }

        private void EndBriefing()
        {
            currentSequence = null;
            currentLineIndex = 0;
            typingTweener = null;

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
