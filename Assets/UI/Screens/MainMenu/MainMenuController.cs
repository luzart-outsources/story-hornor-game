using UnityEngine;

namespace Luzart.UIFramework.Examples
{
    public class MainMenuController : UIController<MainMenuScreen, MainMenuViewModel>
    {
        private UIEventSubscription playGameSubscription;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Debug.Log("MainMenuController: Initialized");
        }

        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            playGameSubscription?.Dispose();
        }

        protected override void OnShow()
        {
            base.OnShow();
            Debug.Log("MainMenuController: View Shown");
        }

        protected override void OnHide()
        {
            base.OnHide();
            Debug.Log("MainMenuController: View Hidden");
        }

        public void OnPlayClicked()
        {
            Debug.Log("MainMenuController: Play button clicked");
            
            EventBus?.Publish(new PlayGameRequestedEvent());
        }

        public void OnSettingsClicked()
        {
            Debug.Log("MainMenuController: Settings button clicked");
            
            UIManager.Instance?.ShowAsync<SettingsPopup>();
        }

        public void OnQuitClicked()
        {
            Debug.Log("MainMenuController: Quit button clicked");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void UpdatePlayerInfo(string name, int level)
        {
            ViewModel.PlayerName = name;
            ViewModel.PlayerLevel = level;
        }
    }

    public class PlayGameRequestedEvent : UIEvent
    {
    }
}
