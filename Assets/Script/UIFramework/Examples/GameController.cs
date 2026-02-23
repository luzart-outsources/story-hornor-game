using UnityEngine;
using UnityEngine.UI;
using UIFramework.Core;
using UIFramework.Communication;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example: Game Controller that responds to UI events
    /// Shows how to decouple UI from game logic
    /// </summary>
    public class GameController : MonoBehaviour, 
        IEventHandler<Events.PlayButtonClickedEvent>,
        IEventHandler<Events.SettingsRequestedEvent>,
        IEventHandler<Events.ConfirmationResultEvent>
    {
        [SerializeField] private Data.UIRegistry uiRegistry;
        
        private void Start()
        {
            // Initialize UI Manager
            Managers.UIManager.Instance.SetRegistry(uiRegistry);
            
            // Subscribe to UI events
            EventBus.Instance.Subscribe<Events.PlayButtonClickedEvent>(this);
            EventBus.Instance.Subscribe<Events.SettingsRequestedEvent>(this);
            EventBus.Instance.Subscribe<Events.ConfirmationResultEvent>(this);
            
            // Show main menu on start
            ShowMainMenu();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events (prevent memory leaks)
            EventBus.Instance.Unsubscribe<Events.PlayButtonClickedEvent>(this);
            EventBus.Instance.Unsubscribe<Events.SettingsRequestedEvent>(this);
            EventBus.Instance.Unsubscribe<Events.ConfirmationResultEvent>(this);
        }
        
        private void ShowMainMenu()
        {
            var menuData = new MainMenuData("Epic Game Title", isFirstTime: true);
            var menu = Managers.UIManager.Instance.Show<MainMenuScreen>(menuData);
            
            // Inject fade transition
            if (menu != null)
            {
                menu.SetTransition(new Animations.FadeTransition(0.5f));
            }
        }
        
        // Handle Play button event
        public void Handle(Events.PlayButtonClickedEvent eventData)
        {
            Debug.Log("[GameController] Starting game...");
            
            // Hide main menu
            Managers.UIManager.Instance.Hide<MainMenuScreen>();
            
            // Show gameplay HUD
            var hudData = new PlayerHudData(100, 100, 0, 1);
            Managers.UIManager.Instance.Show<PlayerHud>(hudData);
            
            // Start game logic
            StartGameplay();
        }
        
        // Handle Settings request
        public void Handle(Events.SettingsRequestedEvent eventData)
        {
            Debug.Log("[GameController] Opening settings...");
            
            // Show settings popup (example - you need to create SettingsPopup)
            ShowConfirmationExample();
        }
        
        // Handle confirmation result
        public void Handle(Events.ConfirmationResultEvent eventData)
        {
            if (eventData.Confirmed)
            {
                Debug.Log("[GameController] User confirmed action");
            }
            else
            {
                Debug.Log("[GameController] User cancelled action");
            }
        }
        
        private void StartGameplay()
        {
            // Your game logic here
            Debug.Log("Game started!");
        }
        
        private void ShowConfirmationExample()
        {
            var data = new ConfirmationPopupData(
                "Settings",
                "Settings feature coming soon!",
                onConfirm: () => Debug.Log("OK pressed"),
                onCancel: null
            );
            
            var popup = Managers.UIManager.Instance.Show<ConfirmationPopup>(data);
            
            if (popup != null)
            {
                // Inject scale animation for popup
                popup.SetAnimation(new Animations.ScaleAnimation(0.25f));
            }
        }
    }
}
