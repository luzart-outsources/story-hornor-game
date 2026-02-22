using System;
using System.Threading;
using UnityEngine;

namespace Luzart.UIFramework
{
    public class UISlideTransition : IUITransition
    {
        private readonly float duration;
        private readonly AnimationCurve curve;
        private readonly UIDirection direction;
        private readonly float distance;

        public UISlideTransition(UIDirection direction = UIDirection.Up, float distance = 1000f, float duration = 0.3f, AnimationCurve curve = null)
        {
            this.direction = direction;
            this.distance = distance;
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

            var startPos = GetOffscreenPosition(target);
            var endPos = Vector2.zero;

            try
            {
                await AnimatePosition(target, startPos, endPos, cancellationToken);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UISlideTransition: Show animation cancelled.");
            }
        }

        public async void PlayHideTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
            }

            var startPos = target.anchoredPosition;
            var endPos = GetOffscreenPosition(target);

            try
            {
                await AnimatePosition(target, startPos, endPos, cancellationToken);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UISlideTransition: Hide animation cancelled.");
            }
        }

        private Vector2 GetOffscreenPosition(RectTransform target)
        {
            return direction switch
            {
                UIDirection.Left => new Vector2(-distance, 0),
                UIDirection.Right => new Vector2(distance, 0),
                UIDirection.Up => new Vector2(0, distance),
                UIDirection.Down => new Vector2(0, -distance),
                _ => Vector2.zero
            };
        }

        private async System.Threading.Tasks.Task AnimatePosition(RectTransform target, Vector2 from, Vector2 to, CancellationToken cancellationToken)
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
                target.anchoredPosition = Vector2.Lerp(from, to, curvedT);

                await System.Threading.Tasks.Task.Yield();
            }

            if (target != null)
            {
                target.anchoredPosition = to;
            }
        }
    }
}
