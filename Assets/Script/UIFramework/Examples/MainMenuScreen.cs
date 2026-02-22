using System;
using UnityEngine;
using UnityEngine.UI;
using UIFramework.Core;
using UIFramework.MVVM;

namespace UIFramework.Examples
{
    [Serializable]
    public class MainMenuViewModel : ViewModelBase
    {
        private string _playerName;
        private int _playerLevel;
        private int _playerGold;

        public string PlayerName
        {
            get => _playerName;
            set
            {
                if (_playerName != value)
                {
                    _playerName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int PlayerLevel
        {
            get => _playerLevel;
            set
            {
                if (_playerLevel != value)
                {
                    _playerLevel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int PlayerGold
        {
            get => _playerGold;
            set
            {
                if (_playerGold != value)
                {
                    _playerGold = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public MainMenuViewModel(string playerName, int level, int gold)
        {
            _playerName = playerName;
            _playerLevel = level;
            _playerGold = gold;
        }
    }

    public class MainMenuScreen : UIView<MainMenuViewModel>
    {
        [Header("UI References")]
        [SerializeField] private Text _playerNameText;
        [SerializeField] private Text _playerLevelText;
        [SerializeField] private Text _playerGoldText;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _settingsButton;

        protected override void OnInitialize(object data)
        {
            base.OnInitialize(data);

            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayClicked);

            if (_shopButton != null)
                _shopButton.onClick.AddListener(OnShopClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        protected override void OnViewModelChanged()
        {
            if (ViewModel == null)
                return;

            if (_playerNameText != null)
                _playerNameText.text = ViewModel.PlayerName;

            if (_playerLevelText != null)
                _playerLevelText.text = $"Level: {ViewModel.PlayerLevel}";

            if (_playerGoldText != null)
                _playerGoldText.text = $"Gold: {ViewModel.PlayerGold}";
        }

        private void OnPlayClicked()
        {
            Debug.Log("Play button clicked");
        }

        private void OnShopClicked()
        {
            Debug.Log("Shop button clicked");
        }

        private void OnSettingsClicked()
        {
            Debug.Log("Settings button clicked");
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (_playButton != null)
                _playButton.onClick.RemoveListener(OnPlayClicked);

            if (_shopButton != null)
                _shopButton.onClick.RemoveListener(OnShopClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }
    }
}
