using System;
using System.Threading;
using UnityEngine;

namespace Luzart.UIFramework
{
    public class UIScaleTransition : IUITransition
    {
        private readonly float duration;
        private readonly AnimationCurve curve;
        private readonly Vector3 startScale;

        public UIScaleTransition(Vector3? startScale = null, float duration = 0.3f, AnimationCurve curve = null)
        {
            this.startScale = startScale ?? new Vector3(0.8f, 0.8f, 1f);
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

            try
            {
                await AnimateScale(target, startScale, Vector3.one, cancellationToken);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UIScaleTransition: Show animation cancelled.");
            }
        }

        public async void PlayHideTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
            }

            try
            {
                await AnimateScale(target, Vector3.one, startScale, cancellationToken);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UIScaleTransition: Hide animation cancelled.");
            }
        }

        private async System.Threading.Tasks.Task AnimateScale(RectTransform target, Vector3 from, Vector3 to, CancellationToken cancellationToken)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                if (target == null)
                    return;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = curve.Evaluate(t);
                target.localScale = Vector3.Lerp(from, to, curvedT);

                await System.Threading.Tasks.Task.Yield();
            }

            if (target != null)
            {
                target.localScale = to;
            }
        }
    }
}
