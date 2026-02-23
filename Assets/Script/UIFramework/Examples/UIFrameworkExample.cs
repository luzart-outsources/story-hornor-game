using UnityEngine;
using UIFramework.Managers;
using UIFramework.Examples;
using UIFramework.Animations;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example usage of the UI Framework
    /// Shows how to use the system in a real game
    /// </summary>
    public class UIFrameworkExample : MonoBehaviour
    {
        [SerializeField] private UIFramework.Data.UIRegistry registry;
        
        private void Start()
        {
            // Initialize UI Manager
            var uiManager = UIManager.Instance;
            uiManager.SetRegistry(registry);
            
            // Example 1: Show main menu (synchronous)
            ShowMainMenuSync();
            
            // Example 2: Show popup with animation
            // ShowConfirmationPopupWithAnimation();
        }
        
        private void Update()
        {
            // Example: Press Space to show confirmation popup
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ShowConfirmationPopupWithAnimation();
            }
            
            // Example: Press H to show HUD
            if (Input.GetKeyDown(KeyCode.H))
            {
                ShowPlayerHud();
            }
            
            // Example: Press Escape to hide all
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIManager.Instance.HideAll();
            }
        }
        
        private void ShowMainMenuSync()
        {
            var menuData = new MainMenuData("Welcome to the Game!", true);
            
            // Inject fade transition
            var menu = UIManager.Instance.Show<MainMenuScreen>(menuData);
            
            if (menu != null)
            {
                menu.SetTransition(new FadeTransition(0.5f));
            }
        }
        
        private void ShowConfirmationPopupWithAnimation()
        {
            var popupData = new ConfirmationPopupData(
                "Quit Game",
                "Are you sure you want to quit?",
                onConfirm: () => 
                {
                    Debug.Log("User confirmed quit");
                    Application.Quit();
                },
                onCancel: () => 
                {
                    Debug.Log("User cancelled");
                }
            );
            
            var popup = UIManager.Instance.Show<ConfirmationPopup>(popupData);
            
            if (popup != null)
            {
                // Inject scale animation for popup
                popup.SetAnimation(new ScaleAnimation(0.25f));
            }
        }
        
        private void ShowPlayerHud()
        {
            var hudData = new PlayerHudData(
                currentHealth: 80,
                maxHealth: 100,
                coins: 1250,
                level: 5
            );
            
            UIManager.Instance.Show<PlayerHud>(hudData);
        }
        
        #if UNITASK_SUPPORT
        // Example: Async loading with UniTask
        private async Cysharp.Threading.Tasks.UniTask ShowMenuAsync()
        {
            var cts = new System.Threading.CancellationTokenSource();
            
            try
            {
                var menuData = new MainMenuData("Welcome!", true);
                var menu = await UIManager.Instance.ShowAsync<MainMenuScreen>(menuData, cts.Token);
                
                if (menu != null)
                {
                    menu.SetTransition(new FadeTransition(0.5f));
                    Debug.Log("Menu loaded successfully");
                }
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("Menu loading cancelled");
            }
        }
        #endif
        
        private void OnDestroy()
        {
            // Cleanup example: unsubscribe from events if subscribed
            // EventBus.Instance.Unsubscribe<SomeEvent>(this);
        }
    }
}
