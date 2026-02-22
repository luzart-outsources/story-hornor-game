using UnityEngine;
using UIFramework.Core;

namespace UIFramework.MVVM
{
    public abstract class UIView<TViewModel> : UIBase where TViewModel : IViewModel
    {
        protected TViewModel ViewModel { get; private set; }

        public void BindViewModel(TViewModel viewModel)
        {
            if (ViewModel != null)
            {
                ViewModel.OnDataChanged -= OnViewModelChanged;
            }

            ViewModel = viewModel;

            if (ViewModel != null)
            {
                ViewModel.OnDataChanged += OnViewModelChanged;
                OnViewModelChanged();
            }
        }

        protected override void OnInitialize(object data)
        {
            base.OnInitialize(data);

            if (data is TViewModel vm)
            {
                BindViewModel(vm);
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (ViewModel != null)
            {
                ViewModel.OnDataChanged -= OnViewModelChanged;
            }
        }

        protected abstract void OnViewModelChanged();
    }
}
