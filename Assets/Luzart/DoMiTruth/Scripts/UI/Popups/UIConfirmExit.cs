namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UIConfirmExit : UIBase
    {
        [SerializeField] private Button btnYes;
        [SerializeField] private Button btnNo;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnYes, OnClickYes);
            GameUtil.ButtonOnClick(btnNo, OnClickNo);
        }

        private void OnClickYes()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnClickNo()
        {
            Hide();
        }
    }
}
