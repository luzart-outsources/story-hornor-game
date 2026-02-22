using System;

namespace Luzart.UIFramework
{
    public abstract class UIController<TView, TViewModel> : IUIController 
        where TView : UIBase
        where TViewModel : UIViewModel, new()
    {
        protected TView View { get; private set; }
        public TViewModel ViewModel { get; private set; }
        protected UIEventBus EventBus { get; private set; }

        private bool isInitialized;
        private bool isDisposed;

        public void Setup(TView view, TViewModel viewModel, UIEventBus eventBus)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
            ViewModel = viewModel ?? new TViewModel();
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Initialize()
        {
            if (isInitialized) return;

            ViewModel.OnDataChanged += OnViewModelDataChanged;
            SubscribeToEvents();
            OnInitialize();
            
            isInitialized = true;
        }

        public void OnViewShown()
        {
            OnShow();
        }

        public void OnViewHidden()
        {
            OnHide();
        }

        public void Dispose()
        {
            if (isDisposed) return;

            OnDispose();
            UnsubscribeFromEvents();
            
            if (ViewModel != null)
            {
                ViewModel.OnDataChanged -= OnViewModelDataChanged;
                ViewModel.Reset();
            }

            View = null;
            ViewModel = null;
            EventBus = null;
            
            isDisposed = true;
        }

        private void OnViewModelDataChanged()
        {
            View?.Refresh();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual void OnDispose() { }
        protected virtual void SubscribeToEvents() { }
        protected virtual void UnsubscribeFromEvents() { }
    }
}
