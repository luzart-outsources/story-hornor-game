using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// No animation strategy - instant show/hide
    /// Use this for UIs that don't need animations
    /// </summary>
    public class NoAnimation : IUIAnimation
    {
        public void PlayShowAnimation(GameObject target, System.Action onComplete = null)
        {
            onComplete?.Invoke();
        }
        
        public void PlayHideAnimation(GameObject target, System.Action onComplete = null)
        {
            onComplete?.Invoke();
        }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask PlayShowAnimationAsync(GameObject target, System.Threading.CancellationToken cancellationToken = default)
        {
            await Cysharp.Threading.Tasks.UniTask.Yield();
        }
        
        public async Cysharp.Threading.Tasks.UniTask PlayHideAnimationAsync(GameObject target, System.Threading.CancellationToken cancellationToken = default)
        {
            await Cysharp.Threading.Tasks.UniTask.Yield();
        }
        #endif
    }
}
