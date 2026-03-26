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

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnPlay, OnClickPlay);
            GameUtil.ButtonOnClick(btnContinue, OnClickContinue);
            GameUtil.ButtonOnClick(btnSettings, OnClickSettings);
            GameUtil.ButtonOnClick(btnGuide, OnClickGuide);
            GameUtil.ButtonOnClick(btnExit, OnClickExit);
        }

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);
            RefreshContinueButton();
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
