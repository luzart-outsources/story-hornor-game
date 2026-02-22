using UnityEngine;
using UIFramework.Manager;

namespace UIFramework.Core
{
    public abstract class UIPopup : UIBase
    {
        [SerializeField] private bool _isModal = true;
        [SerializeField] private bool _closeOnBackgroundClick = true;
        [SerializeField] private GameObject _backgroundBlocker;
        
        public bool IsModal => _isModal;
        public bool CloseOnBackgroundClick => _closeOnBackgroundClick;
        
        public int StackOrder { get; set; }

        protected override void OnInitialize(object data)
        {
            base.OnInitialize(data);
            Layer = UILayer.Popup;
            
            SetupBackgroundBlocker();
        }

        protected override void OnBeforeShow()
        {
            base.OnBeforeShow();
            
            if (_backgroundBlocker != null)
            {
                _backgroundBlocker.SetActive(_isModal);
            }
            
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = _isModal;
            }
        }

        protected override void OnAfterHide()
        {
            base.OnAfterHide();
            
            if (_backgroundBlocker != null)
            {
                _backgroundBlocker.SetActive(false);
            }
        }

        private void SetupBackgroundBlocker()
        {
            if (_backgroundBlocker == null)
                return;

            var button = _backgroundBlocker.GetComponent<UnityEngine.UI.Button>();
            if (button != null && _closeOnBackgroundClick)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnBackgroundClick);
            }
        }

        protected virtual void OnBackgroundClick()
        {
            if (_closeOnBackgroundClick)
            {
                UIManager.Instance.Hide(GetType());
            }
        }

        public void SetModal(bool isModal)
        {
            _isModal = isModal;
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = _isModal;
            }
            if (_backgroundBlocker != null)
            {
                _backgroundBlocker.SetActive(_isModal && State == UIState.Visible);
            }
        }
    }
}
