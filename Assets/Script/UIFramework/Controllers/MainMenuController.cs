using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Views
{
    /// <summary>
    /// Controller for MainMenu (logic layer)
    /// </summary>
    public class MainMenuController : UIControllerBase
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Subscribe to events if needed
            // Communication.EventBus.Instance.Subscribe<SomeEvent>(this);
        }
        
        protected override void OnShowInternal()
        {
            base.OnShowInternal();
            // Logic when UI is shown
        }
        
        protected override void OnHideInternal()
        {
            base.OnHideInternal();
            // Logic when UI is hidden
        }
        
        protected override void OnDispose()
        {
            // Unsubscribe from events
            // Communication.EventBus.Instance.Unsubscribe<SomeEvent>(this);
            
            base.OnDispose();
        }
        
        // Add your business logic methods here
        // public void OnMyButtonPressed()
        // {
        //     Debug.Log("Button pressed");
        //     // Handle business logic
        //     // Publish events
        //     // Call services
        // }
    }
}
