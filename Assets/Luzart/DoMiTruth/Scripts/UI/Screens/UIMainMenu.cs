namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UIMainMenu : UIBase
    {
        [Header("Buttons")]
        [SerializeField] private Button btnPlay;
        [SerializeField] private Button btnContinue;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnGuide;
        [SerializeField] private Button btnExit;

        [Header("Continue Toggle")]
        [SerializeField] private SelectToggleGameObject continueToggle;

        [Header("Hover Effects")]
        [Tooltip("Gắn tất cả ButtonHoverSelect của các button vào đây để tạo group hover.")]
        [SerializeField] private ButtonHoverSelect[] hoverButtons;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnPlay, OnClickPlay);
            GameUtil.ButtonOnClick(btnContinue, OnClickContinue);
            GameUtil.ButtonOnClick(btnSettings, OnClickSettings);
            GameUtil.ButtonOnClick(btnGuide, OnClickGuide);
            GameUtil.ButtonOnClick(btnExit, OnClickExit);

            SetupHoverGroup();
        }

        private void SetupHoverGroup()
        {
            if (hoverButtons == null || hoverButtons.Length == 0) return;

            for (int i = 0; i < hoverButtons.Length; i++)
            {
                if (hoverButtons[i] != null)
                    hoverButtons[i].SetGroup(hoverButtons);
            }
        }

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);
            RefreshContinueButton();
            ResetAllHover();
        }

        private void ResetAllHover()
        {
            if (hoverButtons == null) return;
            for (int i = 0; i < hoverButtons.Length; i++)
            {
                if (hoverButtons[i] != null)
                    hoverButtons[i].Deselect();
            }
        }

        private void RefreshContinueButton()
        {
            bool hasSave = GameDataManager.Instance.HasSaveData();
            if (continueToggle != null)
                continueToggle.Select(hasSave);
            else if (btnContinue != null)
                btnContinue.gameObject.SetActive(hasSave);
        }

        private void OnClickPlay()
        {
            GameFlowController.Instance.StartNewGame();
        }

        private void OnClickContinue()
        {
            GameFlowController.Instance.ContinueGame();
        }

        private void OnClickSettings()
        {
            UIManager.Instance.ShowUI(UIName.Settings);
        }

        private void OnClickGuide()
        {
            UIManager.Instance.ShowUI(UIName.Guide);
        }

        private void OnClickExit()
        {
            UIManager.Instance.ShowUI(UIName.ConfirmExit);
        }
    }
}
