using System.Threading;
using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// Strategy interface for UI transitions
    /// </summary>
    public interface IUITransition
    {
        void TransitionIn(GameObject target, System.Action onComplete = null);
        void TransitionOut(GameObject target, System.Action onComplete = null);
        
        #if UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask TransitionInAsync(GameObject target, CancellationToken cancellationToken = default);
        Cysharp.Threading.Tasks.UniTask TransitionOutAsync(GameObject target, CancellationToken cancellationToken = default);
        #endif
    }
}
