using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Animations
{
    /// <summary>
    /// Simple fade transition strategy
    /// Note: Replace LeanTween with DOTween or Unity Animation if LeanTween not available
    /// </summary>
    public class FadeTransition : IUITransition
    {
        private readonly float duration;
        
        public FadeTransition(float duration = 0.3f)
        {
            this.duration = duration;
        }
        
        public void TransitionIn(GameObject target, System.Action onComplete = null)
        {
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.alpha = 0f;
            
            // Simple fade implementation without LeanTween
            FadeCoroutineHelper.Instance.FadeIn(canvasGroup, duration, onComplete);
        }
        
        public void TransitionOut(GameObject target, System.Action onComplete = null)
        {
            var canvasGroup = GetOrAddCanvasGroup(target);
            
            // Simple fade implementation without LeanTween
            FadeCoroutineHelper.Instance.FadeOut(canvasGroup, duration, onComplete);
        }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask TransitionInAsync(GameObject target, System.Threading.CancellationToken cancellationToken = default)
        {
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.alpha = 0f;
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new System.OperationCanceledException();
                
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                await Cysharp.Threading.Tasks.UniTask.Yield();
            }
            
            canvasGroup.alpha = 1f;
        }
        
        public async Cysharp.Threading.Tasks.UniTask TransitionOutAsync(GameObject target, System.Threading.CancellationToken cancellationToken = default)
        {
            var canvasGroup = GetOrAddCanvasGroup(target);
            
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new System.OperationCanceledException();
                
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                await Cysharp.Threading.Tasks.UniTask.Yield();
            }
            
            canvasGroup.alpha = 0f;
        }
        #endif
        
        private CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }
            return canvasGroup;
        }
    }
    
    /// <summary>
    /// Helper for coroutine-based fading (no external dependencies)
    /// </summary>
    public class FadeCoroutineHelper : MonoBehaviour
    {
        private static FadeCoroutineHelper instance;
        public static FadeCoroutineHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("[FadeCoroutineHelper]");
                    instance = go.AddComponent<FadeCoroutineHelper>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        public void FadeIn(CanvasGroup canvasGroup, float duration, System.Action onComplete)
        {
            StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f, duration, onComplete));
        }
        
        public void FadeOut(CanvasGroup canvasGroup, float duration, System.Action onComplete)
        {
            StartCoroutine(FadeCoroutine(canvasGroup, canvasGroup.alpha, 0f, duration, onComplete));
        }
        
        private System.Collections.IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float from, float to, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            
            canvasGroup.alpha = to;
            onComplete?.Invoke();
        }
    }
}
