using System;
using System.Threading;
using UnityEngine;

namespace Luzart.UIFramework
{
    public interface IUIView
    {
        string ViewId { get; }
        UIState State { get; }
        UILayer Layer { get; }
        bool IsVisible { get; }
        RectTransform RectTransform { get; }
        
        void Initialize(object data = null);
        void Show(CancellationToken cancellationToken = default);
        void Hide(CancellationToken cancellationToken = default);
        void Refresh();
        void Dispose();
    }

    public interface IUIViewModel
    {
        event Action OnDataChanged;
        void Reset();
    }

    public interface IUIController : IDisposable
    {
        void Initialize();
        void OnViewShown();
        void OnViewHidden();
    }

    public interface IUITransition
    {
        void PlayShowTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default);
        void PlayHideTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default);
    }

    public interface IUIResourceLoader
    {
        System.Threading.Tasks.Task<T> LoadAsync<T>(string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object;
        void Release(string address);
        void ReleaseAll();
    }
}
