using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Animations
{
    /// <summary>
    /// Slide transition strategy
    /// </summary>
    public class SlideTransition : IUITransition
    {
        public enum SlideDirection
        {
            Left,
            Right,
            Top,
            Bottom
        }
        
        private readonly float duration;
        private readonly SlideDirection direction;
        private readonly float distance;
        
        public SlideTransition(SlideDirection direction = SlideDirection.Bottom, float duration = 0.3f, float distance = 1000f)
        {
            this.direction = direction;
            this.duration = duration;
            this.distance = distance;
        }
        
        public void TransitionIn(GameObject target, System.Action onComplete = null)
        {
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogWarning("[SlideTransition] No RectTransform found, falling back to transform");
                onComplete?.Invoke();
                return;
            }
            
            Vector2 startPos = GetStartPosition(direction);
            Vector2 endPos = rectTransform.anchoredPosition;
            
            rectTransform.anchoredPosition = startPos;
            
            SlideCoroutineHelper.Instance.SlideTo(rectTransform, endPos, duration, onComplete);
        }
        
        public void TransitionOut(GameObject target, System.Action onComplete = null)
        {
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogWarning("[SlideTransition] No RectTransform found");
                onComplete?.Invoke();
                return;
            }
            
            Vector2 endPos = GetStartPosition(direction);
            
            SlideCoroutineHelper.Instance.SlideTo(rectTransform, endPos, duration, onComplete);
        }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask TransitionInAsync(GameObject target, System.Threading.CancellationToken cancellationToken = default)
        {
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogWarning("[SlideTransition] No RectTransform found");
                return;
            }
            
            Vector2 startPos = GetStartPosition(direction);
            Vector2 endPos = rectTransform.anchoredPosition;
            
            rectTransform.anchoredPosition = startPos;
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new System.OperationCanceledException();
                
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                await Cysharp.Threading.Tasks.UniTask.Yield();
            }
            
            rectTransform.anchoredPosition = endPos;
        }
        
        public async Cysharp.Threading.Tasks.UniTask TransitionOutAsync(GameObject target, System.Threading.CancellationToken cancellationToken = default)
        {
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogWarning("[SlideTransition] No RectTransform found");
                return;
            }
            
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = GetStartPosition(direction);
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new System.OperationCanceledException();
                
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                await Cysharp.Threading.Tasks.UniTask.Yield();
            }
            
            rectTransform.anchoredPosition = endPos;
        }
        #endif
        
        private Vector2 GetStartPosition(SlideDirection dir)
        {
            switch (dir)
            {
                case SlideDirection.Left:
                    return new Vector2(-distance, 0);
                case SlideDirection.Right:
                    return new Vector2(distance, 0);
                case SlideDirection.Top:
                    return new Vector2(0, distance);
                case SlideDirection.Bottom:
                    return new Vector2(0, -distance);
                default:
                    return Vector2.zero;
            }
        }
    }
    
    /// <summary>
    /// Helper for coroutine-based sliding
    /// </summary>
    public class SlideCoroutineHelper : MonoBehaviour
    {
        private static SlideCoroutineHelper instance;
        public static SlideCoroutineHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("[SlideCoroutineHelper]");
                    instance = go.AddComponent<SlideCoroutineHelper>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        public void SlideTo(RectTransform target, Vector2 endPos, float duration, System.Action onComplete)
        {
            StartCoroutine(SlideCoroutine(target, endPos, duration, onComplete));
        }
        
        private System.Collections.IEnumerator SlideCoroutine(RectTransform target, Vector2 endPos, float duration, System.Action onComplete)
        {
            Vector2 startPos = target.anchoredPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                if (target == null) yield break;
                
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                target.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            if (target != null)
                target.anchoredPosition = endPos;
            
            onComplete?.Invoke();
        }
    }
}
