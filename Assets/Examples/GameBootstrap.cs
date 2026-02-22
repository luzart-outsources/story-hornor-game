using UnityEngine;

namespace Luzart.Examples
{
    /// <summary>
    /// Example: How to use the UI Framework in your game
    /// NOTE: Make sure you have UIManager from Luzart.UIFramework in your scene
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private bool showMainMenuOnStart = true;

        private async void Start()
        {
            InitializeServices();

            if (showMainMenuOnStart)
            {
                await ShowMainMenu();
            }
        }

        private void InitializeServices()
        {
            var uiManager = FindUIManager();
            
            if (uiManager == null)
            {
                Debug.LogError("GameBootstrap: UIManager not found in scene!");
                return;
            }

            var eventBus = uiManager.EventBus;
            eventBus.Subscribe<UIFramework.Examples.PlayGameRequestedEvent>(OnPlayGameRequested);
            eventBus.Subscribe<UIFramework.Examples.SettingsAppliedEvent>(OnSettingsApplied);

            Debug.Log("GameBootstrap: Services initialized");
        }

        private async System.Threading.Tasks.Task ShowMainMenu()
        {
            var uiManager = FindUIManager();
            if (uiManager == null) return;

            var data = new UIFramework.Examples.MainMenuData
            {
                PlayerName = "Player",
                PlayerLevel = 1
            };

            var transition = new UIFramework.UICompositeTransition(
                new UIFramework.UIFadeTransition(0.3f),
                new UIFramework.UIScaleTransition(new Vector3(0.9f, 0.9f, 1f), 0.3f)
            );

            await uiManager.ShowAsync<UIFramework.Examples.MainMenuScreen>(data, transition);
        }

        private void OnPlayGameRequested(UIFramework.Examples.PlayGameRequestedEvent evt)
        {
            Debug.Log("GameBootstrap: Starting game...");
            LoadGameplayScene();
        }

        private void OnSettingsApplied(UIFramework.Examples.SettingsAppliedEvent evt)
        {
            Debug.Log($"GameBootstrap: Settings applied - Music: {evt.MusicVolume}, SFX: {evt.SfxVolume}");
            ApplyAudioSettings(evt.MusicVolume, evt.SfxVolume);
        }

        private async void LoadGameplayScene()
        {
            var uiManager = FindUIManager();
            if (uiManager == null) return;

            uiManager.HideAll();
            
            Debug.Log("Loading gameplay scene...");
            await System.Threading.Tasks.Task.Delay(1000);
            
            Debug.Log("Gameplay scene loaded!");
        }

        private void ApplyAudioSettings(float musicVolume, float sfxVolume)
        {
            AudioListener.volume = musicVolume;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var uiManager = FindUIManager();
                uiManager?.GoBack();
            }
        }

        private void OnDestroy()
        {
            var uiManager = FindUIManager();
            var eventBus = uiManager?.EventBus;
            if (eventBus != null)
            {
                eventBus.CleanupDeadReferences();
            }
        }

        private UIFramework.UIManager FindUIManager()
        {
            return Object.FindObjectOfType<UIFramework.UIManager>();
        }
    }
}
