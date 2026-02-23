namespace UIFramework.Core
{
    /// <summary>
    /// Abstract base controller for UI logic
    /// Decoupled from MonoBehaviour for testability
    /// </summary>
    public abstract class UIControllerBase : IUIController
    {
        protected IUIView view;
        protected IUIData data;
        
        private bool isDisposed = false;
        
        public virtual void Initialize(IUIView view, IUIData data)
        {
            this.view = view;
            this.data = data;
            OnInitialize();
        }
        
        public virtual void OnShow()
        {
            OnShowInternal();
        }
        
        public virtual void OnHide()
        {
            OnHideInternal();
        }
        
        public virtual void OnRefresh()
        {
            OnRefreshInternal();
        }
        
        public virtual void Dispose()
        {
            if (isDisposed)
                return;
                
            OnDispose();
            
            view = null;
            data = null;
            isDisposed = true;
        }
        
        #region Virtual Methods
        
        protected virtual void OnInitialize() { }
        protected virtual void OnShowInternal() { }
        protected virtual void OnHideInternal() { }
        protected virtual void OnRefreshInternal() { }
        protected virtual void OnDispose() { }
        
        #endregion
    }
}
