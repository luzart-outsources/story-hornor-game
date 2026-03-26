namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UINotebook : UIBase
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button btnCluesTab;
        [SerializeField] private Button btnCharactersTab;

        [Header("Tab Switching")]
        [SerializeField] private SelectSwitchGameObject tabSwitch;

        [Header("Tab Toggle Visuals")]
        [SerializeField] private SelectToggleImage clueTabToggle;
        [SerializeField] private SelectToggleImage characterTabToggle;

        [Header("Content")]
        [SerializeField] private Transform clueListContainer;
        [SerializeField] private Transform characterListContainer;
        [SerializeField] private NotebookClueItem clueItemPrefab;
        [SerializeField] private NotebookCharacterItem characterItemPrefab;

        private int currentTab;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnCluesTab, () => SwitchTab(0));
            GameUtil.ButtonOnClick(btnCharactersTab, () => SwitchTab(1));
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

            if (tabSwitch != null)
                tabSwitch.Select(tabIndex);

            if (clueTabToggle != null)
                clueTabToggle.Select(tabIndex == 0);
            if (characterTabToggle != null)
                characterTabToggle.Select(tabIndex == 1);

            if (tabIndex == 0)
                PopulateClues();
            else
                PopulateCharacters();
        }

        private void PopulateClues()
        {
            ClearContainer(clueListContainer);

            var clues = NotebookManager.Instance.GetCollectedClues();
            for (int i = 0; i < clues.Count; i++)
            {
                var item = Instantiate(clueItemPrefab, clueListContainer);
                item.Init(clues[i]);
            }
        }

        private void PopulateCharacters()
        {
            ClearContainer(characterListContainer);

            var characters = NotebookManager.Instance.GetMetCharacters();
            for (int i = 0; i < characters.Count; i++)
            {
                var item = Instantiate(characterItemPrefab, characterListContainer);
                item.Init(characters[i]);
            }
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }
    }
}
