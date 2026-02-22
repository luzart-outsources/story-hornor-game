using UnityEngine;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

namespace UIFramework.Core.Transitions
{
    public class ScaleTransition : IUITransition
    {
        private readonly float _duration;
        private readonly Vector3 _startScale;
        private readonly Vector3 _endScale;
        private readonly AnimationCurve _curve;

        public ScaleTransition(float duration = 0.3f, Vector3? startScale = null, Vector3? endScale = null, AnimationCurve curve = null)
        {
            _duration = duration;
            _startScale = startScale ?? new Vector3(0.5f, 0.5f, 1f);
            _endScale = endScale ?? Vector3.one;
            _curve = curve ?? AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

#if UNITASK_SUPPORT
        public async UniTask ShowAsync(UIBase ui, CancellationToken cancellationToken = default)
        {
            var transform = ui.transform;
            var canvasGroup = ui.GetComponent<CanvasGroup>();
            
            transform.localScale = _startScale;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            float elapsed = 0f;
            while (elapsed < _duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                elapsed += Time.deltaTime;
                float t = _curve.Evaluate(elapsed / _duration);
                transform.localScale = Vector3.Lerp(_startScale, _endScale, t);
                if (canvasGroup != null)
                    canvasGroup.alpha = t;
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            transform.localScale = _endScale;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public async UniTask HideAsync(UIBase ui, CancellationToken cancellationToken = default)
        {
            var transform = ui.transform;
            var canvasGroup = ui.GetComponent<CanvasGroup>();
            
            transform.localScale = _endScale;
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            float elapsed = 0f;
            while (elapsed < _duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                elapsed += Time.deltaTime;
                float t = 1f - _curve.Evaluate(elapsed / _duration);
                transform.localScale = Vector3.Lerp(_startScale, _endScale, t);
                if (canvasGroup != null)
                    canvasGroup.alpha = t;
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            transform.localScale = _startScale;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }
#else
        public void Show(UIBase ui)
        {
            ui.transform.localScale = _endScale;
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
            ui.transform.localScale = _startScale;
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
