using System;
using System.Threading;

namespace UIFramework.Core
{
    /// <summary>
    /// Base interface for all UI views
    /// </summary>
    public interface IUIView : IDisposable
    {
        string ViewId { get; }
        UIState State { get; }
        UILayer Layer { get; }
        bool IsVisible { get; }
        
        void Initialize(IUIData data);
        void Show();
        void Hide();
        void Refresh();
        
        // Async variants (optional, requires UniTask)
        #if UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask ShowAsync(CancellationToken cancellationToken = default);
        Cysharp.Threading.Tasks.UniTask HideAsync(CancellationToken cancellationToken = default);
        #endif
    }
}
