using UnityEngine;
using UnityEngine.UI;
using UIFramework.Core;
using TMPro.EditorUtilities;
using UIFramework.Managers;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example: Main Menu Screen
    /// </summary>
    public class MainMenuScreen : UIScreen
    {
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text titleText;
        
        protected override IUIController CreateController()
        {
            return new MainMenuController();
        }
        
        protected override void OnInitialize(IUIData data)
        {
            base.OnInitialize(data);
            
            // Bind UI events
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
            
            // Update UI with data
            if (data is MainMenuData menuData)
            {
                if (titleText != null)
                    titleText.text = menuData.Title;
            }
        }
        
        protected override void OnDispose()
        {
            // Unbind events to prevent memory leaks
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);
                
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
                
            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);
                
            base.OnDispose();
        }
        
        private void OnPlayClicked()
        {
            var controller = this.controller as MainMenuController;
            controller?.OnPlayButtonPressed();
        }
        
        private void OnSettingsClicked()
        {
            var controller = this.controller as MainMenuController;
            controller?.OnSettingsButtonPressed();
        }
        
        private void OnQuitClicked()
        {
            var controller = this.controller as MainMenuController;
            controller?.OnQuitButtonPressed();
        }
    }
    
    /// <summary>
    /// Controller for Main Menu (logic layer)
    /// </summary>
    public class MainMenuController : UIControllerBase
    {
        public void OnPlayButtonPressed()
        {
            Debug.Log("[MainMenuController] Play button pressed");
            
            // Publish event instead of direct call
            Communication.EventBus.Instance.Publish(new Events.PlayButtonClickedEvent());

            // Or use UIManager to navigate
            // UIManager.Instance.Show<GameplayScreen>();
            UIManager.Instance.Show<MainMenuScreen>();
        }
        
        public void OnSettingsButtonPressed()
        {
            Debug.Log("[MainMenuController] Settings button pressed");
            Communication.EventBus.Instance.Publish(new Events.SettingsRequestedEvent());
        }
        
        public void OnQuitButtonPressed()
        {
            Debug.Log("[MainMenuController] Quit button pressed");
            Application.Quit();
        }
    }
    
    /// <summary>
    /// Data/ViewModel for Main Menu
    /// </summary>
    [System.Serializable]
    public class MainMenuData : UIDataBase
    {
        public string Title { get; private set; }
        public bool IsFirstTime { get; private set; }
        
        public MainMenuData(string title, bool isFirstTime = false)
        {
            Title = title;
            IsFirstTime = isFirstTime;
        }
    }
}
