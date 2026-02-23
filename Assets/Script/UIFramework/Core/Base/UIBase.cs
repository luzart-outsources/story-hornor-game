using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// Abstract base class for all UI views (MonoBehaviour)
    /// Follows MVVM pattern - View layer only
    /// </summary>
    public abstract class UIBase : MonoBehaviour, IUIView
    {
        [SerializeField] protected string viewId;
        [SerializeField] protected UILayer layer = UILayer.Screen;
        
        protected IUIController controller;
        protected IUIData data;
        protected IUITransition transition;
        protected IUIAnimation animation;
        
        private UIState state = UIState.None;
        private bool isInitialized = false;
        
        public string ViewId => viewId;
        public UIState State => state;
        public UILayer Layer => layer;
        public bool IsVisible => state == UIState.Visible || state == UIState.Showing;
        
        #region Lifecycle
        
        public virtual void Initialize(IUIData data)
        {
            if (isInitialized)
                return;
                
            this.data = data;
            this.controller = CreateController();
            
            if (controller != null)
            {
                controller.Initialize(this, data);
            }
            
            OnInitialize(data);
            isInitialized = true;
            state = UIState.Hidden;
        }
        
        public virtual void Show()
        {
            if (state == UIState.Showing || state == UIState.Visible)
                return;
                
            state = UIState.Showing;
            gameObject.SetActive(true);
            
            OnBeforeShow();
            
            if (transition != null)
            {
                transition.TransitionIn(gameObject, OnShowComplete);
            }
            else if (animation != null)
            {
                animation.PlayShowAnimation(gameObject, OnShowComplete);
            }
            else
            {
                OnShowComplete();
            }
            
            controller?.OnShow();
        }
        
        public virtual void Hide()
        {
            if (state == UIState.Hiding || state == UIState.Hidden)
                return;
                
            state = UIState.Hiding;
            
            OnBeforeHide();
            
            if (transition != null)
            {
                transition.TransitionOut(gameObject, OnHideComplete);
            }
            else if (animation != null)
            {
                animation.PlayHideAnimation(gameObject, OnHideComplete);
            }
            else
            {
                OnHideComplete();
            }
            
            controller?.OnHide();
        }
        
        public virtual void Refresh()
        {
            if (state != UIState.Visible)
                return;
                
            OnRefresh(data);
            controller?.OnRefresh();
        }
        
        public virtual void Dispose()
        {
            if (state == UIState.Disposed)
                return;
                
            OnDispose();
            controller?.Dispose();
            
            isInitialized = false;
            state = UIState.Disposed;
            
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        
        #endregion
        
        #region Async Support (UniTask)
        
        #if UNITASK_SUPPORT
        public virtual async Cysharp.Threading.Tasks.UniTask ShowAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            if (state == UIState.Showing || state == UIState.Visible)
                return;
                
            state = UIState.Showing;
            gameObject.SetActive(true);
            
            OnBeforeShow();
            
            if (transition != null)
            {
                await transition.TransitionInAsync(gameObject, cancellationToken);
            }
            else if (animation != null)
            {
                await animation.PlayShowAnimationAsync(gameObject, cancellationToken);
            }
            
            OnShowComplete();
            controller?.OnShow();
        }
        
        public virtual async Cysharp.Threading.Tasks.UniTask HideAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            if (state == UIState.Hiding || state == UIState.Hidden)
                return;
                
            state = UIState.Hiding;
            
            OnBeforeHide();
            
            if (transition != null)
            {
                await transition.TransitionOutAsync(gameObject, cancellationToken);
            }
            else if (animation != null)
            {
                await animation.PlayHideAnimationAsync(gameObject, cancellationToken);
            }
            
            OnHideComplete();
            controller?.OnHide();
        }
        #endif
        
        #endregion
        
        #region Strategy Injection
        
        public void SetTransition(IUITransition transition)
        {
            this.transition = transition;
        }
        
        public void SetAnimation(IUIAnimation animation)
        {
            this.animation = animation;
        }
        
        #endregion
        
        #region Callbacks
        
        private void OnShowComplete()
        {
            state = UIState.Visible;
            OnShown();
        }
        
        private void OnHideComplete()
        {
            state = UIState.Hidden;
            gameObject.SetActive(false);
            OnHidden();
        }
        
        #endregion
        
        #region Virtual Methods (Override in derived classes)
        
        protected virtual IUIController CreateController()
        {
            return null; // Override to provide controller
        }
        
        protected virtual void OnInitialize(IUIData data) { }
        protected virtual void OnBeforeShow() { }
        protected virtual void OnShown() { }
        protected virtual void OnBeforeHide() { }
        protected virtual void OnHidden() { }
        protected virtual void OnRefresh(IUIData data) { }
        protected virtual void OnDispose() { }
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void OnDestroy()
        {
            if (state != UIState.Disposed)
            {
                Dispose();
            }
        }
        
        #endregion
    }
}
