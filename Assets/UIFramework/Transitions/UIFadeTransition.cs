using System;
using System.Threading;
using UnityEngine;

namespace Luzart.UIFramework
{
    public class UIFadeTransition : IUITransition
    {
        private readonly float duration;
        private readonly AnimationCurve curve;

        public UIFadeTransition(float duration = 0.3f, AnimationCurve curve = null)
        {
            this.duration = duration;
            this.curve = curve ?? AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        public async void PlayShowTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
            }

            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            try
            {
                await AnimateAlpha(canvasGroup, 0f, 1f, cancellationToken);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UIFadeTransition: Show animation cancelled.");
            }
        }

        public async void PlayHideTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
            }

            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            try
            {
                await AnimateAlpha(canvasGroup, 1f, 0f, cancellationToken);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UIFadeTransition: Hide animation cancelled.");
            }
        }

        private async System.Threading.Tasks.Task AnimateAlpha(CanvasGroup canvasGroup, float from, float to, CancellationToken cancellationToken)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                if (canvasGroup == null)
                    return;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = curve.Evaluate(t);
                canvasGroup.alpha = Mathf.Lerp(from, to, curvedT);

                await System.Threading.Tasks.Task.Yield();
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = to;
            }
        }
    }
}
