using UIFramework.Communication;

namespace UIFramework.Examples.Events
{
    /// <summary>
    /// Example events for UI communication
    /// </summary>
    
    public class PlayButtonClickedEvent : IUIEvent
    {
    }
    
    public class SettingsRequestedEvent : IUIEvent
    {
    }
    
    public class ConfirmationResultEvent : IUIEvent
    {
        public bool Confirmed { get; private set; }
        
        public ConfirmationResultEvent(bool confirmed)
        {
            Confirmed = confirmed;
        }
    }
    
    public class ScreenChangedEvent : IUIEvent
    {
        public string FromScreen { get; private set; }
        public string ToScreen { get; private set; }
        
        public ScreenChangedEvent(string fromScreen, string toScreen)
        {
            FromScreen = fromScreen;
            ToScreen = toScreen;
        }
    }
    
    public class PopupOpenedEvent : IUIEvent
    {
        public string PopupId { get; private set; }
        
        public PopupOpenedEvent(string popupId)
        {
            PopupId = popupId;
        }
    }
    
    public class PopupClosedEvent : IUIEvent
    {
        public string PopupId { get; private set; }
        
        public PopupClosedEvent(string popupId)
        {
            PopupId = popupId;
        }
    }
}
