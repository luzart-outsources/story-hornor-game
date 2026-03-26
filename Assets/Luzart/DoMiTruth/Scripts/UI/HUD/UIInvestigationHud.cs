namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class UIInvestigationHud : UIBase
    {
        [Header("Buttons")]
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnNotebook;

        [Header("Clue Counter")]
        [SerializeField] private TMP_Text txtClueCount;

        [Header("New Clue Badge")]
        [SerializeField] private SelectToggleGameObject notebookBadge;

        [Header("SO Events")]
        [SerializeField] private StringEventChannel onClueCollected;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnSettings, OnClickSettings);
            GameUtil.ButtonOnClick(btnNotebook, OnClickNotebook);
        }

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);
            UpdateClueCount();
            HideBadge();

            if (onClueCollected != null)
                onClueCollected.Register(OnClueCollectedHandler);
        }

        public override void Hide()
        {
            if (onClueCollected != null)
                onClueCollected.Unregister(OnClueCollectedHandler);
            base.Hide();
        }

        private void OnClueCollectedHandler(string clueId)
        {
            UpdateClueCount();
            ShowBadge();
        }

        private void UpdateClueCount()
        {
            if (txtClueCount == null) return;
            int collected = NotebookManager.Instance.GetCollectedClueCount();
            int total = NotebookManager.Instance.GetTotalClueCount();
            txtClueCount.text = $"{collected}/{total}";
        }

        private void ShowBadge()
        {
            if (notebookBadge != null)
                notebookBadge.Select(true);
        }

        private void HideBadge()
        {
            if (notebookBadge != null)
                notebookBadge.Select(false);
        }

        private void OnClickSettings()
        {
            UIManager.Instance.ShowUI(UIName.Settings);
        }

        private void OnClickNotebook()
        {
            HideBadge();
            UIManager.Instance.ShowUI(UIName.Notebook);
        }
    }
}
