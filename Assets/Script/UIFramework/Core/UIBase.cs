using UnityEngine;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

namespace UIFramework.Core
{
    public abstract class UIBase : MonoBehaviour
    {
        public string UIAddress { get; set; }
        public UILayer Layer { get; protected set; } = UILayer.Screen;
        public UIState State { get; protected set; } = UIState.Hidden;
        
        protected IUITransition _transition;
        protected Canvas _canvas;
        protected CanvasGroup _canvasGroup;
        
#if UNITASK_SUPPORT
        protected CancellationTokenSource _closeCts;
#endif

        public virtual void Initialize(object data = null)
        {
            if (!_canvas)
                _canvas = GetComponent<Canvas>();
            if (!_canvasGroup)
                _canvasGroup = GetComponent<CanvasGroup>();
            
            OnInitialize(data);
        }

        public void SetTransition(IUITransition transition)
        {
            _transition = transition;
        }

#if UNITASK_SUPPORT
        public virtual async UniTask ShowAsync(CancellationToken cancellationToken = default)
        {
            if (State == UIState.Showing || State == UIState.Visible)
                return;

            State = UIState.Showing;
            gameObject.SetActive(true);

            OnBeforeShow();

            if (_transition != null)
            {
                await _transition.ShowAsync(this, cancellationToken);
            }
            else
            {
                if (_canvasGroup)
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
            }

            State = UIState.Visible;
            OnAfterShow();
        }

        public virtual async UniTask HideAsync(CancellationToken cancellationToken = default)
        {
            if (State == UIState.Hiding || State == UIState.Hidden)
                return;

            State = UIState.Hiding;
            OnBeforeHide();

            if (_transition != null)
            {
                await _transition.HideAsync(this, cancellationToken);
            }
            else
            {
                if (_canvasGroup)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
            }

            State = UIState.Hidden;
            gameObject.SetActive(false);
            OnAfterHide();
        }
#else
        public virtual void Show()
        {
            if (State == UIState.Showing || State == UIState.Visible)
                return;

            State = UIState.Showing;
            gameObject.SetActive(true);

            OnBeforeShow();

            if (_canvasGroup)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            State = UIState.Visible;
            OnAfterShow();
        }

        public virtual void Hide()
        {
            if (State == UIState.Hiding || State == UIState.Hidden)
                return;

            State = UIState.Hiding;
            OnBeforeHide();

            if (_canvasGroup)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            State = UIState.Hidden;
            gameObject.SetActive(false);
            OnAfterHide();
        }
#endif

        public virtual void Refresh()
        {
            OnRefresh();
        }

        public virtual void Dispose()
        {
#if UNITASK_SUPPORT
            _closeCts?.Cancel();
            _closeCts?.Dispose();
            _closeCts = null;
#endif
            OnDispose();
        }

        protected virtual void OnInitialize(object data) { }
        protected virtual void OnBeforeShow() { }
        protected virtual void OnAfterShow() { }
        protected virtual void OnBeforeHide() { }
        protected virtual void OnAfterHide() { }
        protected virtual void OnRefresh() { }
        protected virtual void OnDispose() { }

        protected virtual void OnDestroy()
        {
            Dispose();
        }
    }

    public enum UIState
    {
        Hidden,
        Showing,
        Visible,
        Hiding
    }

    public enum UILayer
    {
        HUD = 0,
        Screen = 1,
        Popup = 2,
        Overlay = 3,
        System = 4
    }
}
