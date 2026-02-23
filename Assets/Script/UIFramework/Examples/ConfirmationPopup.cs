using UnityEngine;
using UnityEngine.UI;
using UIFramework.Core;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example: Confirmation Popup
    /// </summary>
    public class ConfirmationPopup : UIPopup
    {
        [Header("UI References")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button closeButton;
        
        private System.Action onConfirm;
        private System.Action onCancel;
        
        protected override IUIController CreateController()
        {
            return new ConfirmationPopupController();
        }
        
        protected override void OnInitialize(IUIData data)
        {
            base.OnInitialize(data);
            
            // Bind UI events
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
                
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
                
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCancelClicked);
            
            // Update UI with data
            if (data is ConfirmationPopupData popupData)
            {
                if (titleText != null)
                    titleText.text = popupData.Title;
                    
                if (messageText != null)
                    messageText.text = popupData.Message;
                
                onConfirm = popupData.OnConfirm;
                onCancel = popupData.OnCancel;
            }
        }
        
        protected override void OnDispose()
        {
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmClicked);
                
            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelClicked);
                
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCancelClicked);
            
            onConfirm = null;
            onCancel = null;
            
            base.OnDispose();
        }
        
        private void OnConfirmClicked()
        {
            var controller = this.controller as ConfirmationPopupController;
            controller?.OnConfirm(onConfirm);
        }
        
        private void OnCancelClicked()
        {
            var controller = this.controller as ConfirmationPopupController;
            controller?.OnCancel(onCancel);
        }
    }
    
    /// <summary>
    /// Controller for Confirmation Popup
    /// </summary>
    public class ConfirmationPopupController : UIControllerBase
    {
        public void OnConfirm(System.Action callback)
        {
            Debug.Log("[ConfirmationPopupController] Confirmed");
            
            callback?.Invoke();
            
            // Publish event
            Communication.EventBus.Instance.Publish(new Events.ConfirmationResultEvent(true));
            
            // Close popup
            Managers.UIManager.Instance.Hide<ConfirmationPopup>();
        }
        
        public void OnCancel(System.Action callback)
        {
            Debug.Log("[ConfirmationPopupController] Cancelled");
            
            callback?.Invoke();
            
            // Publish event
            Communication.EventBus.Instance.Publish(new Events.ConfirmationResultEvent(false));
            
            // Close popup
            Managers.UIManager.Instance.Hide<ConfirmationPopup>();
        }
    }
    
    /// <summary>
    /// Data/ViewModel for Confirmation Popup
    /// </summary>
    [System.Serializable]
    public class ConfirmationPopupData : UIDataBase
    {
        public string Title { get; private set; }
        public string Message { get; private set; }
        public System.Action OnConfirm { get; private set; }
        public System.Action OnCancel { get; private set; }
        
        public ConfirmationPopupData(string title, string message, System.Action onConfirm = null, System.Action onCancel = null)
        {
            Title = title;
            Message = message;
            OnConfirm = onConfirm;
            OnCancel = onCancel;
        }
    }
}
