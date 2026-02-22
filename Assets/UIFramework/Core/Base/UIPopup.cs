using UnityEngine;

namespace Luzart.UIFramework
{
    public abstract class UIPopup : UIBase
    {
        [SerializeField] private UIPopupMode popupMode = UIPopupMode.Modal;
        [SerializeField] private GameObject blocker;

        public UIPopupMode PopupMode => popupMode;

        protected override void Awake()
        {
            base.Awake();
            if (blocker != null)
            {
                blocker.SetActive(false);
            }
        }

        protected override void OnBeforeShow()
        {
            base.OnBeforeShow();
            if (popupMode == UIPopupMode.Modal && blocker != null)
            {
                blocker.SetActive(true);
            }
        }

        protected override void OnBeforeHide()
        {
            base.OnBeforeHide();
            if (blocker != null)
            {
                blocker.SetActive(false);
            }
        }
    }
}
