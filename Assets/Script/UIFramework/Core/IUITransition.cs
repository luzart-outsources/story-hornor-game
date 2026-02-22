#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

namespace UIFramework.Core
{
    public interface IUITransition
    {
#if UNITASK_SUPPORT
        UniTask ShowAsync(UIBase ui, CancellationToken cancellationToken = default);
        UniTask HideAsync(UIBase ui, CancellationToken cancellationToken = default);
#else
        void Show(UIBase ui);
        void Hide(UIBase ui);
#endif
    }
}
