using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Animations
{
    /// <summary>
    /// Scale animation strategy (common for popups)
    /// </summary>
    public class ScaleAnimation : IUIAnimation
    {
        private readonly float duration;
        private readonly AnimationCurve curve;
        
        public ScaleAnimation(float duration = 0.25f, AnimationCurve curve = null)
        {
            this.duration = duration;
            this.curve = curve ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
        
        public void PlayShowAnimation(GameObject target, System.Action onComplete = null)
        {
            target.transform.localScale = Vector3.zero;
            ScaleCoroutineHelper.Instance.ScaleTo(target.transform, Vector3.one, duration, curve, onComplete);
        }
        
        public void PlayHideAnimation(GameObject target, System.Action onComplete = null)
        {
            ScaleCoroutineHelper.Instance.ScaleTo(target.transform, Vector3.zero, duration, curve, onComplete);
        }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask PlayShowAnimationAsync(GameObject target, System.Threading.CancellationToken cancellationToken = default)
        {
            target.transform.localScale = Vector3.zero;
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new System.OperationCanceledException();
                
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(elapsed / duration);
                target.transform.localScale = Vector3.one * t;
                await Cysharp.Threading.Tasks.UniTask.Yield();
            }
            
            target.transform.localScale = Vector3.one;
        }
        
        public async Cysharp.Threading.Tasks.UniTask PlayHideAnimationAsync(GameObject target, System.Threading.CancellationToken cancellationToken = default)
        {
            float elapsed = 0f;
            Vector3 startScale = target.transform.localScale;
            
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new System.OperationCanceledException();
                
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(1f - (elapsed / duration));
                target.transform.localScale = startScale * t;
                await Cysharp.Threading.Tasks.UniTask.Yield();
            }
            
            target.transform.localScale = Vector3.zero;
        }
        #endif
    }
    
    /// <summary>
    /// Helper for coroutine-based scaling
    /// </summary>
    public class ScaleCoroutineHelper : MonoBehaviour
    {
        private static ScaleCoroutineHelper instance;
        public static ScaleCoroutineHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("[ScaleCoroutineHelper]");
                    instance = go.AddComponent<ScaleCoroutineHelper>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        public void ScaleTo(Transform target, Vector3 targetScale, float duration, AnimationCurve curve, System.Action onComplete)
        {
            StartCoroutine(ScaleCoroutine(target, targetScale, duration, curve, onComplete));
        }
        
        private System.Collections.IEnumerator ScaleCoroutine(Transform target, Vector3 targetScale, float duration, AnimationCurve curve, System.Action onComplete)
        {
            Vector3 startScale = target.localScale;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                if (target == null) yield break;
                
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(elapsed / duration);
                target.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            if (target != null)
                target.localScale = targetScale;
            
            onComplete?.Invoke();
        }
    }
}
