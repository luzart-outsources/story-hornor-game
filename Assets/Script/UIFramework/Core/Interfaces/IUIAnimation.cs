using System.Threading;
using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// Strategy interface for UI animations
    /// </summary>
    public interface IUIAnimation
    {
        void PlayShowAnimation(GameObject target, System.Action onComplete = null);
        void PlayHideAnimation(GameObject target, System.Action onComplete = null);
        
        #if UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask PlayShowAnimationAsync(GameObject target, CancellationToken cancellationToken = default);
        Cysharp.Threading.Tasks.UniTask PlayHideAnimationAsync(GameObject target, CancellationToken cancellationToken = default);
        #endif
    }
}
