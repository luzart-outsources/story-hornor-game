using UnityEngine;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

namespace UIFramework.Core.Transitions
{
    public class FadeTransition : IUITransition
    {
        private readonly float _duration;
        private readonly AnimationCurve _curve;

        public FadeTransition(float duration = 0.3f, AnimationCurve curve = null)
        {
            _duration = duration;
            _curve = curve ?? AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

#if UNITASK_SUPPORT
        public async UniTask ShowAsync(UIBase ui, CancellationToken cancellationToken = default)
        {
            var canvasGroup = ui.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                ui.gameObject.SetActive(true);
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            while (elapsed < _duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                elapsed += Time.deltaTime;
                float t = _curve.Evaluate(elapsed / _duration);
                canvasGroup.alpha = t;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public async UniTask HideAsync(UIBase ui, CancellationToken cancellationToken = default)
        {
            var canvasGroup = ui.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                ui.gameObject.SetActive(false);
                return;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            while (elapsed < _duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                elapsed += Time.deltaTime;
                float t = 1f - _curve.Evaluate(elapsed / _duration);
                canvasGroup.alpha = t;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            canvasGroup.alpha = 0f;
        }
#else
        public void Show(UIBase ui)
        {
            var canvasGroup = ui.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void Hide(UIBase ui)
        {
            var canvasGroup = ui.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
#endif
    }
}
