// Optional: DOTween Integration
// Install DOTween from: http://dotween.demigiant.com/
// Uncomment this file after installation

/*
#if DOTWEEN_ENABLED
using System;
using System.Threading;
using UnityEngine;
using DG.Tweening;

namespace Luzart.UIFramework.Integration
{
    public class UIDOTweenFadeTransition : IUITransition
    {
        private readonly float duration;
        private readonly Ease ease;

        public UIDOTweenFadeTransition(float duration = 0.3f, Ease ease = Ease.OutQuad)
        {
            this.duration = duration;
            this.ease = ease;
        }

        public void PlayShowTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
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

            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, duration)
                .SetEase(ease)
                .SetUpdate(true) // Works even when Time.timeScale = 0
                .OnComplete(() => onComplete?.Invoke());

            cancellationToken.Register(() =>
            {
                canvasGroup?.DOKill();
            });
        }

        public void PlayHideTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
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

            canvasGroup.DOFade(0f, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());

            cancellationToken.Register(() =>
            {
                canvasGroup?.DOKill();
            });
        }
    }

    public class UIDOTweenSlideTransition : IUITransition
    {
        private readonly float duration;
        private readonly Ease ease;
        private readonly UIDirection direction;
        private readonly float distance;

        public UIDOTweenSlideTransition(UIDirection direction = UIDirection.Up, float distance = 1000f, float duration = 0.3f, Ease ease = Ease.OutBack)
        {
            this.direction = direction;
            this.distance = distance;
            this.duration = duration;
            this.ease = ease;
        }

        public void PlayShowTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
            }

            var startPos = GetOffscreenPosition();
            target.anchoredPosition = startPos;

            target.DOAnchorPos(Vector2.zero, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());

            cancellationToken.Register(() =>
            {
                target?.DOKill();
            });
        }

        public void PlayHideTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
            }

            var endPos = GetOffscreenPosition();

            target.DOAnchorPos(endPos, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());

            cancellationToken.Register(() =>
            {
                target?.DOKill();
            });
        }

        private Vector2 GetOffscreenPosition()
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
    }

    public class UIDOTweenScaleTransition : IUITransition
    {
        private readonly float duration;
        private readonly Ease ease;
        private readonly Vector3 startScale;

        public UIDOTweenScaleTransition(Vector3? startScale = null, float duration = 0.3f, Ease ease = Ease.OutBack)
        {
            this.startScale = startScale ?? new Vector3(0.7f, 0.7f, 1f);
            this.duration = duration;
            this.ease = ease;
        }

        public void PlayShowTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
            }

            target.localScale = startScale;

            target.DOScale(Vector3.one, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());

            cancellationToken.Register(() =>
            {
                target?.DOKill();
            });
        }

        public void PlayHideTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
            }

            target.DOScale(startScale, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());

            cancellationToken.Register(() =>
            {
                target?.DOKill();
            });
        }
    }

    // Usage:
    // UIManager.Instance.SetCustomTransition(UITransitionType.Custom, new UIDOTweenFadeTransition());
    // await UIManager.Instance.ShowAsync<MyScreen>();
}
#endif
*/
