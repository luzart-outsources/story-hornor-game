using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Views
{
    public class MainMenu : UIScreen
    {
        // Add your UI component references here
        // [SerializeField] private Button myButton;
        // [SerializeField] private Text myText;
        
        protected override IUIController CreateController()
        {
            return new MainMenuController();
        }
        
        protected override void OnInitialize(IUIData data)
        {
            base.OnInitialize(data);
            
            // Bind UI events here
            // myButton?.onClick.AddListener(OnMyButtonClicked);
            
            // Update UI with data
            if (data is MainMenuData viewData)
            {
                // Update UI elements with viewData
            }
        }
        
        protected override void OnDispose()
        {
            // Unbind UI events here to prevent memory leaks
            // myButton?.onClick.RemoveListener(OnMyButtonClicked);
            
            base.OnDispose();
        }
        
        // Add your UI event handlers here
        // private void OnMyButtonClicked()
        // {
        //     var controller = this.controller as MainMenuController;
        //     controller?.OnMyButtonPressed();
        // }
    }
}
