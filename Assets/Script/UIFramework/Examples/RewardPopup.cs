using System;
using UnityEngine;
using UnityEngine.UI;
using UIFramework.Core;
using UIFramework.MVVM;

namespace UIFramework.Examples
{
    [Serializable]
    public class RewardPopupViewModel : ViewModelBase
    {
        private string _rewardTitle;
        private string _rewardDescription;
        private int _goldAmount;
        private int _gemAmount;

        public string RewardTitle
        {
            get => _rewardTitle;
            set
            {
                if (_rewardTitle != value)
                {
                    _rewardTitle = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string RewardDescription
        {
            get => _rewardDescription;
            set
            {
                if (_rewardDescription != value)
                {
                    _rewardDescription = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int GoldAmount
        {
            get => _goldAmount;
            set
            {
                if (_goldAmount != value)
                {
                    _goldAmount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int GemAmount
        {
            get => _gemAmount;
            set
            {
                if (_gemAmount != value)
                {
                    _gemAmount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RewardPopupViewModel(string title, string description, int gold, int gems)
        {
            _rewardTitle = title;
            _rewardDescription = description;
            _goldAmount = gold;
            _gemAmount = gems;
        }
    }

    public class RewardPopup : UIPopup
    {
        [Header("UI References")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _gemText;
        [SerializeField] private Button _claimButton;
        [SerializeField] private Button _closeButton;

        private RewardPopupViewModel _viewModel;
        private Action _onClaimCallback;

        protected override void OnInitialize(object data)
        {
            base.OnInitialize(data);

            if (data is RewardPopupViewModel viewModel)
            {
                BindViewModel(viewModel);
            }
            else if (data is RewardPopupData popupData)
            {
                _viewModel = new RewardPopupViewModel(
                    popupData.Title,
                    popupData.Description,
                    popupData.GoldAmount,
                    popupData.GemAmount
                );
                _onClaimCallback = popupData.OnClaim;
                BindViewModel(_viewModel);
            }

            if (_claimButton != null)
                _claimButton.onClick.AddListener(OnClaimClicked);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void BindViewModel(RewardPopupViewModel viewModel)
        {
            if (_viewModel != null)
            {
                _viewModel.OnDataChanged -= UpdateView;
            }

            _viewModel = viewModel;

            if (_viewModel != null)
            {
                _viewModel.OnDataChanged += UpdateView;
                UpdateView();
            }
        }

        private void UpdateView()
        {
            if (_viewModel == null)
                return;

            if (_titleText != null)
                _titleText.text = _viewModel.RewardTitle;

            if (_descriptionText != null)
                _descriptionText.text = _viewModel.RewardDescription;

            if (_goldText != null)
                _goldText.text = $"+{_viewModel.GoldAmount}";

            if (_gemText != null)
                _gemText.text = $"+{_viewModel.GemAmount}";
        }

        private void OnClaimClicked()
        {
            _onClaimCallback?.Invoke();
            Manager.UIManager.Instance.Hide<RewardPopup>();
        }

        private void OnCloseClicked()
        {
            Manager.UIManager.Instance.Hide<RewardPopup>();
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (_viewModel != null)
            {
                _viewModel.OnDataChanged -= UpdateView;
            }

            if (_claimButton != null)
                _claimButton.onClick.RemoveListener(OnClaimClicked);

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }

    [Serializable]
    public class RewardPopupData
    {
        public string Title;
        public string Description;
        public int GoldAmount;
        public int GemAmount;
        public Action OnClaim;

        public RewardPopupData(string title, string description, int gold, int gems, Action onClaim = null)
        {
            Title = title;
            Description = description;
            GoldAmount = gold;
            GemAmount = gems;
            OnClaim = onClaim;
        }
    }
}
