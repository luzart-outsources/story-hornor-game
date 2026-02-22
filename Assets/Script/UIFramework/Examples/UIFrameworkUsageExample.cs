using UnityEngine;
using UIFramework.Manager;
using UIFramework.Examples;
using UIFramework.Events;
using UIFramework.Core;
using UIFramework.Core.Transitions;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#endif

namespace UIFramework.Examples
{
    /// <summary>
    /// Example demonstrating how to use the UI Framework
    /// </summary>
    public class UIFrameworkUsageExample : MonoBehaviour
    {
#if UNITASK_SUPPORT
        private async void Start()
#else
        private void Start()
#endif
        {
            InitializeEventListeners();
            
#if UNITASK_SUPPORT
            await ExampleUsageAsync();
#else
            ExampleUsageSync();
#endif
        }

        private void OnDestroy()
        {
            CleanupEventListeners();
        }

        // ============================================
        // ASYNC USAGE (WITH UNITASK)
        // ============================================
#if UNITASK_SUPPORT
        private async UniTask ExampleUsageAsync()
        {
            // Example 1: Show Main Menu Screen
            var mainMenuViewModel = new MainMenuViewModel("PlayerName", 10, 5000);
            await UIManager.Instance.ShowAsync<MainMenuScreen>(
                mainMenuViewModel,
                new FadeTransition(0.3f)
            );

            // Wait 2 seconds
            await UniTask.Delay(2000);

            // Example 2: Show Reward Popup
            var rewardData = new RewardPopupData(
                "Daily Reward",
                "You received your daily login rewards!",
                gold: 1000,
                gems: 100,
                onClaim: () => {
                    Debug.Log("Rewards claimed!");
                    // Add rewards to player inventory
                }
            );

            await UIManager.Instance.ShowAsync<RewardPopup>(
                rewardData,
                new ScaleTransition(0.3f)
            );

            // Wait 2 seconds
            await UniTask.Delay(2000);

            // Example 3: Hide popup
            await UIManager.Instance.HideAsync<RewardPopup>();

            // Example 4: Check if UI is open
            bool isMainMenuOpen = UIManager.Instance.IsOpened<MainMenuScreen>();
            Debug.Log($"Main Menu is open: {isMainMenuOpen}");

            // Example 5: Get UI instance and interact
            var mainMenu = UIManager.Instance.Get<MainMenuScreen>();
            if (mainMenu != null)
            {
                mainMenu.Refresh();
            }

            // Example 6: Show multiple popups (stacking)
            await ShowMultiplePopupsExample();

            // Example 7: Hide all UIs
            await UniTask.Delay(2000);
            await UIManager.Instance.HideAllAsync();
        }

        private async UniTask ShowMultiplePopupsExample()
        {
            // Show first popup
            var popup1Data = new RewardPopupData("Popup 1", "First popup", 100, 10);
            await UIManager.Instance.ShowAsync<RewardPopup>(popup1Data);

            await UniTask.Delay(500);

            // Show second popup (will stack on top)
            var popup2Data = new RewardPopupData("Popup 2", "Second popup", 200, 20);
            await UIManager.Instance.ShowAsync<RewardPopup>(popup2Data);

            // Check popup stack
            int stackCount = UIManager.Instance.GetPopupStackCount();
            Debug.Log($"Popup stack count: {stackCount}");

            var topPopup = UIManager.Instance.GetTopPopup();
            Debug.Log($"Top popup: {topPopup?.GetType().Name}");
        }
#endif

        // ============================================
        // SYNC USAGE (WITHOUT UNITASK)
        // ============================================
#if !UNITASK_SUPPORT
        private void ExampleUsageSync()
        {
            // Example 1: Show Main Menu Screen
            var mainMenuViewModel = new MainMenuViewModel("PlayerName", 10, 5000);
            UIManager.Instance.Show<MainMenuScreen>(
                mainMenuViewModel,
                new FadeTransition(0.3f)
            );

            // Example 2: Show Reward Popup after delay
            Invoke(nameof(ShowRewardPopupExample), 2f);

            // Example 3: Hide all after delay
            Invoke(nameof(HideAllExample), 4f);
        }

        private void ShowRewardPopupExample()
        {
            var rewardData = new RewardPopupData(
                "Daily Reward",
                "You received your daily login rewards!",
                gold: 1000,
                gems: 100,
                onClaim: () => {
                    Debug.Log("Rewards claimed!");
                }
            );

            UIManager.Instance.Show<RewardPopup>(
                rewardData,
                new ScaleTransition(0.3f)
            );
        }

        private void HideAllExample()
        {
            UIManager.Instance.HideAll();
        }
#endif

        // ============================================
        // EVENT SYSTEM EXAMPLE
        // ============================================
        private void InitializeEventListeners()
        {
            EventBus.Instance.Subscribe<UIOpenedEvent>(OnUIOpened);
            EventBus.Instance.Subscribe<UIClosedEvent>(OnUIClosed);
            EventBus.Instance.Subscribe<UIScreenChangedEvent>(OnScreenChanged);
            EventBus.Instance.Subscribe<UIPopupStackChangedEvent>(OnPopupStackChanged);
        }

        private void CleanupEventListeners()
        {
            EventBus.Instance.Unsubscribe<UIOpenedEvent>(OnUIOpened);
            EventBus.Instance.Unsubscribe<UIClosedEvent>(OnUIClosed);
            EventBus.Instance.Unsubscribe<UIScreenChangedEvent>(OnScreenChanged);
            EventBus.Instance.Unsubscribe<UIPopupStackChangedEvent>(OnPopupStackChanged);
        }

        private void OnUIOpened(UIOpenedEvent evt)
        {
            Debug.Log($"[Event] UI Opened: {evt.UIType.Name}");
        }

        private void OnUIClosed(UIClosedEvent evt)
        {
            Debug.Log($"[Event] UI Closed: {evt.UIType.Name}");
        }

        private void OnScreenChanged(UIScreenChangedEvent evt)
        {
            Debug.Log($"[Event] Screen Changed: {evt.FromScreen?.Name} -> {evt.ToScreen?.Name}");
        }

        private void OnPopupStackChanged(UIPopupStackChangedEvent evt)
        {
            Debug.Log($"[Event] Popup Stack Count: {evt.StackCount}");
        }

        // ============================================
        // ADVANCED EXAMPLES
        // ============================================

        // Example: ViewModel update
        private void ExampleViewModelUpdate()
        {
            var mainMenu = UIManager.Instance.Get<MainMenuScreen>();
            if (mainMenu != null)
            {
                // Update ViewModel - UI will automatically update
                // This would work if MainMenuScreen exposes its ViewModel
                // mainMenu.ViewModel.PlayerGold += 100;
            }
        }

        // Example: Custom transition
        private void ExampleCustomTransition()
        {
            var customTransition = new SlideTransition(
                duration: 0.5f,
                direction: SlideTransition.Direction.Bottom,
                distance: 1000f
            );

#if UNITASK_SUPPORT
            UIManager.Instance.ShowAsync<MainMenuScreen>(null, customTransition).Forget();
#else
            UIManager.Instance.Show<MainMenuScreen>(null, customTransition);
#endif
        }

        // Example: Hide all except HUD
        private void ExampleHideAllExceptHUD()
        {
#if UNITASK_SUPPORT
            UIManager.Instance.HideAllIgnoreAsync(new[] { typeof(UIHUD) }).Forget();
#else
            UIManager.Instance.HideAllIgnore(typeof(UIHUD));
#endif
        }

        // Example: Preload UI
#if UNITASK_SUPPORT
        private async UniTask ExamplePreloadUI()
        {
            // This would require access to the loader
            // var loader = new AddressableUILoader();
            // await loader.PreloadAsync("UI/MainMenuScreen");
        }
#endif

        // Example: Register UI dynamically
        private void ExampleDynamicRegistration()
        {
            UIManager.Instance.RegisterUI(
                typeof(MainMenuScreen),
                "UI/MainMenuScreen"
            );
        }

        // Example: Custom event
        public class GameStateChangedEvent : IUIEvent
        {
            public string NewState { get; set; }
        }

        private void ExampleCustomEvent()
        {
            // Publish custom event
            EventBus.Instance.Publish(new GameStateChangedEvent 
            { 
                NewState = "InGame" 
            });

            // Subscribe to custom event
            EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            Debug.Log($"Game state changed to: {evt.NewState}");
        }
    }
}
