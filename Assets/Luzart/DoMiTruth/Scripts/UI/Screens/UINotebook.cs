namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// Notebook UI — sổ mở 2 trang.
    /// Trang trái: text description clue.
    /// Trang phải: polaroid ảnh clue + giấy rách tên clue.
    /// Lật trang qua từng clue/character đã thu thập.
    /// Tab: Clues / Characters.
    /// </summary>
    public class UINotebook : UIBase
    {
        [Header("Background")]
        [SerializeField] private Image imgNotebookBg;

        [Header("Tab Buttons")]
        [SerializeField] private Button btnCluesTab;
        [SerializeField] private Button btnCharactersTab;

        [Header("Tab Switching")]
        [SerializeField] private SelectSwitchGameObject tabSwitch;
        [SerializeField] private SelectToggleImage clueTabToggle;
        [SerializeField] private SelectToggleImage characterTabToggle;

        [Header("Trang trái — Description")]
        [SerializeField] private Image imgLeftPage;
        [SerializeField] private TMP_Text txtDescription;

        [Header("Trang phải — Polaroid + Name Tag")]
        [SerializeField] private Image imgRightPage;
        [SerializeField] private Image imgPolaroidFrame;
        [SerializeField] private Image imgPhoto;
        [SerializeField] private Image imgNameTag;
        [SerializeField] private TMP_Text txtName;
        [SerializeField] private TMP_Text txtCategory;

        [Header("Navigation")]
        [SerializeField] private Button btnPrevPage;
        [SerializeField] private Button btnNextPage;
        [SerializeField] private TMP_Text txtPageNumber;
        [SerializeField] private Button btnClose;

        // State
        private int currentTab; // 0 = Clues, 1 = Characters
        private int currentPage;
        private List<ClueSO> collectedClues = new List<ClueSO>();
        private List<DialogueCharacterSO> metCharacters = new List<DialogueCharacterSO>();

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnCluesTab, () => SwitchTab(0));
            GameUtil.ButtonOnClick(btnCharactersTab, () => SwitchTab(1));
            GameUtil.ButtonOnClick(btnPrevPage, PrevPage);
            GameUtil.ButtonOnClick(btnNextPage, NextPage);

            if (btnClose != null)
                GameUtil.ButtonOnClick(btnClose, () => Hide());
        }

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);
            SwitchTab(0);
        }

        public override void RefreshUI()
        {
            SwitchTab(currentTab);
        }

        private void SwitchTab(int tabIndex)
        {
            currentTab = tabIndex;
            currentPage = 0;

            if (tabSwitch != null)
                tabSwitch.Select(tabIndex);

            if (clueTabToggle != null)
                clueTabToggle.Select(tabIndex == 0);
            if (characterTabToggle != null)
                characterTabToggle.Select(tabIndex == 1);

            if (tabIndex == 0)
            {
                collectedClues = NotebookManager.Instance.GetCollectedClues();
                ShowCurrentPage();
            }
            else
            {
                metCharacters = NotebookManager.Instance.GetMetCharacters();
                ShowCurrentPage();
            }
        }

        // ========== DISPLAY ==========

        private void ShowCurrentPage()
        {
            if (currentTab == 0)
                ShowCluePage();
            else
                ShowCharacterPage();
        }

        private void ShowCluePage()
        {
            bool hasItems = collectedClues != null && collectedClues.Count > 0;

            if (hasItems)
            {
                var clue = collectedClues[currentPage];

                // Trang trái
                if (txtDescription != null)
                    txtDescription.text = clue.description;

                // Trang phải — polaroid
                if (imgPhoto != null)
                {
                    imgPhoto.gameObject.SetActive(true);
                    if (clue.clueImage != null)
                        imgPhoto.sprite = clue.clueImage;
                }
                if (imgPolaroidFrame != null) imgPolaroidFrame.gameObject.SetActive(true);
                if (imgNameTag != null) imgNameTag.gameObject.SetActive(true);

                if (txtName != null) txtName.text = clue.clueName;
                if (txtCategory != null) txtCategory.text = clue.category.ToString();
            }
            else
            {
                ShowEmpty("No evidence collected yet.");
            }

            UpdateNavButtons(hasItems ? collectedClues.Count : 0);
        }

        private void ShowCharacterPage()
        {
            bool hasItems = metCharacters != null && metCharacters.Count > 0;

            if (hasItems)
            {
                var ch = metCharacters[currentPage];

                // Trang trái
                if (txtDescription != null)
                    txtDescription.text = ch.characterName;

                // Trang phải — polaroid
                if (imgPhoto != null)
                {
                    imgPhoto.gameObject.SetActive(true);
                    if (ch.portrait != null)
                        imgPhoto.sprite = ch.portrait;
                }
                if (imgPolaroidFrame != null) imgPolaroidFrame.gameObject.SetActive(true);
                if (imgNameTag != null) imgNameTag.gameObject.SetActive(true);

                if (txtName != null) txtName.text = ch.characterName;
                if (txtCategory != null) txtCategory.text = "SUSPECT";
            }
            else
            {
                ShowEmpty("No suspects met yet.");
            }

            UpdateNavButtons(hasItems ? metCharacters.Count : 0);
        }

        private void ShowEmpty(string message)
        {
            if (txtDescription != null) txtDescription.text = message;
            if (imgPhoto != null) imgPhoto.gameObject.SetActive(false);
            if (imgPolaroidFrame != null) imgPolaroidFrame.gameObject.SetActive(false);
            if (imgNameTag != null) imgNameTag.gameObject.SetActive(false);
            if (txtName != null) txtName.text = "";
            if (txtCategory != null) txtCategory.text = "";
        }

        // ========== NAVIGATION ==========

        private void PrevPage()
        {
            if (currentPage <= 0) return;
            currentPage--;
            ShowCurrentPage();
        }

        private void NextPage()
        {
            int max = currentTab == 0 ? collectedClues.Count : metCharacters.Count;
            if (currentPage >= max - 1) return;
            currentPage++;
            ShowCurrentPage();
        }

        private void UpdateNavButtons(int totalItems)
        {
            if (btnPrevPage != null)
                btnPrevPage.gameObject.SetActive(currentPage > 0);

            if (btnNextPage != null)
                btnNextPage.gameObject.SetActive(currentPage < totalItems - 1);

            if (txtPageNumber != null)
                txtPageNumber.text = totalItems > 0 ? $"{currentPage + 1}/{totalItems}" : "0/0";
        }
    }
}
