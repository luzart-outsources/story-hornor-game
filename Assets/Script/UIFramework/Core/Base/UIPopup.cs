namespace UIFramework.Core
{
    /// <summary>
    /// Base class for UI Popups (overlay dialogs)
    /// </summary>
    public abstract class UIPopup : UIBase
    {
        protected override void OnInitialize(IUIData data)
        {
            base.OnInitialize(data);
            layer = UILayer.Popup;
        }
    }
}
