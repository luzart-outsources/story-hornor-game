using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// No transition strategy - instant show/hide
    /// Use for UIs that don't need transitions
    /// </summary>
    public class NoTransition : IUITransition
    {
        public void TransitionIn(GameObject target, System.Action onComplete = null)
        {
            onComplete?.Invoke();
        }
        
        public void TransitionOut(GameObject target, System.Action onComplete = null)
        {
            onComplete?.Invoke();
        }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask TransitionInAsync(GameObject target, System.Threading.CancellationToken cancellationToken = default)
        {
            await Cysharp.Threading.Tasks.UniTask.Yield();
        }
        
        public async Cysharp.Threading.Tasks.UniTask TransitionOutAsync(GameObject target, System.Threading.CancellationToken cancellationToken = default)
        {
            await Cysharp.Threading.Tasks.UniTask.Yield();
        }
        #endif
    }
}
