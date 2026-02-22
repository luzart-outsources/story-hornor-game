using UnityEngine;
using UnityEngine.UI;

namespace Luzart.UIFramework.Examples
{
    public class MainMenuScreen : UIScreen
    {
        [Header("UI References")]
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text playerLevelText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private MainMenuController controller;

        protected override void Awake()
        {
            base.Awake();

            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        protected override void OnDestroy()
        {
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayButtonClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            
            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);

            base.OnDestroy();
        }

        protected override void OnInitialize(object data)
        {
            base.OnInitialize(data);

            var viewModel = new MainMenuViewModel();
            var eventBus = UIManager.Instance?.EventBus;

            controller = new MainMenuController();
            controller.Setup(this, viewModel, eventBus);
            controller.Initialize();

            UIManager.Instance?.Context.RegisterController<MainMenuScreen>(controller);

            if (data is MainMenuData menuData)
            {
                viewModel.PlayerName = menuData.PlayerName;
                viewModel.PlayerLevel = menuData.PlayerLevel;
            }

            Refresh();
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();

            if (controller?.ViewModel != null)
            {
                if (playerNameText != null)
                    playerNameText.text = controller.ViewModel.PlayerName;
                
                if (playerLevelText != null)
                    playerLevelText.text = $"Level {controller.ViewModel.PlayerLevel}";
            }
        }

        protected override void OnDispose()
        {
            controller?.Dispose();
            controller = null;
            base.OnDispose();
        }

        private void OnPlayButtonClicked()
        {
            controller?.OnPlayClicked();
        }

        private void OnSettingsButtonClicked()
        {
            controller?.OnSettingsClicked();
        }

        private void OnQuitButtonClicked()
        {
            controller?.OnQuitClicked();
        }
    }

    public class MainMenuData
    {
        public string PlayerName { get; set; }
        public int PlayerLevel { get; set; }
    }
}
