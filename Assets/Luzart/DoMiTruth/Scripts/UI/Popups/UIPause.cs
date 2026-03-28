namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Popup Pause hiển thị khi đang Investigation và bấm Settings.
    /// Có 3 nút: Resume, Option (mở Settings popup), Exit Game (về Main Menu).
    /// </summary>
    public class UIPause : UIBase
    {
        [Header("Buttons")]
        [SerializeField] private Button btnResume;
        [SerializeField] private Button btnOption;
        [SerializeField] private Button btnExitGame;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnResume, OnClickResume);
            GameUtil.ButtonOnClick(btnOption, OnClickOption);
            GameUtil.ButtonOnClick(btnExitGame, OnClickExitGame);
        }

        private void OnClickResume()
        {
            Hide();
        }

        private void OnClickOption()
        {
            UIManager.Instance.ShowUI(UIName.Settings);
        }

        private void OnClickExitGame()
        {
            Hide();
            GameFlowController.Instance.ReturnToMainMenu();
        }
    }
}
