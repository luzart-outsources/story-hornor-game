using System;
using System.Threading;
using UnityEngine;

namespace Luzart.UIFramework
{
    public abstract class UIBase : MonoBehaviour, IUIView
    {
        [SerializeField] private string viewId;
        [SerializeField] private UILayer layer = UILayer.Screen;
        [SerializeField] private CanvasGroup canvasGroup;
        
        private UIState state = UIState.None;
        private IUITransition transition;
        private CancellationTokenSource lifecycleCts;

        public string ViewId => viewId;
        public UIState State => state;
        public UILayer Layer => layer;
        public bool IsVisible => state == UIState.Visible;
        public RectTransform RectTransform { get; private set; }
        public CanvasGroup CanvasGroup => canvasGroup;

        protected virtual void Awake()
        {
            RectTransform = transform as RectTransform;
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            lifecycleCts = new CancellationTokenSource();
        }

        protected virtual void OnDestroy()
        {
            lifecycleCts?.Cancel();
            lifecycleCts?.Dispose();
            lifecycleCts = null;
        }

        public virtual void Initialize(object data = null)
        {
            if (state != UIState.None && state != UIState.Hidden)
                return;

            state = UIState.Initializing;
            OnInitialize(data);
            state = UIState.Hidden;
        }

        public void Show(CancellationToken cancellationToken = default)
        {
            if (state == UIState.Showing || state == UIState.Visible)
                return;

            state = UIState.Showing;
            gameObject.SetActive(true);

            OnBeforeShow();

            if (transition != null)
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    lifecycleCts.Token
                );

                transition.PlayShowTransition(RectTransform, () =>
                {
                    linkedCts?.Dispose();
                    OnShown();
                }, linkedCts.Token);
            }
            else
            {
                SetVisibility(true);
                OnShown();
            }
        }

        public void Hide(CancellationToken cancellationToken = default)
        {
            if (state == UIState.Hiding || state == UIState.Hidden)
                return;

            state = UIState.Hiding;
            OnBeforeHide();

            if (transition != null)
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    lifecycleCts.Token
                );

                transition.PlayHideTransition(RectTransform, () =>
                {
                    linkedCts?.Dispose();
                    OnHidden();
                }, linkedCts.Token);
            }
            else
            {
                SetVisibility(false);
                OnHidden();
            }
        }

        public virtual void Refresh()
        {
            OnRefresh();
        }

        public virtual void Dispose()
        {
            OnDispose();
            state = UIState.Disposed;
            
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        public void SetTransition(IUITransition transition)
        {
            this.transition = transition;
        }

        protected virtual void SetVisibility(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        private void OnShown()
        {
            state = UIState.Visible;
            SetVisibility(true);
            OnShow();
        }

        private void OnHidden()
        {
            state = UIState.Hidden;
            SetVisibility(false);
            gameObject.SetActive(false);
            OnHide();
        }

        protected virtual void OnInitialize(object data) { }
        protected virtual void OnBeforeShow() { }
        protected virtual void OnShow() { }
        protected virtual void OnBeforeHide() { }
        protected virtual void OnHide() { }
        protected virtual void OnRefresh() { }
        protected virtual void OnDispose() { }
    }
}
