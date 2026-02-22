using UnityEngine;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

namespace UIFramework.Core.Transitions
{
    public class SlideTransition : IUITransition
    {
        public enum Direction
        {
            Left,
            Right,
            Top,
            Bottom
        }

        private readonly float _duration;
        private readonly Direction _direction;
        private readonly float _distance;
        private readonly AnimationCurve _curve;

        public SlideTransition(float duration = 0.3f, Direction direction = Direction.Bottom, float distance = 1000f, AnimationCurve curve = null)
        {
            _duration = duration;
            _direction = direction;
            _distance = distance;
            _curve = curve ?? AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        private Vector3 GetOffset()
        {
            return _direction switch
            {
                Direction.Left => new Vector3(-_distance, 0, 0),
                Direction.Right => new Vector3(_distance, 0, 0),
                Direction.Top => new Vector3(0, _distance, 0),
                Direction.Bottom => new Vector3(0, -_distance, 0),
                _ => Vector3.zero
            };
        }

#if UNITASK_SUPPORT
        public async UniTask ShowAsync(UIBase ui, CancellationToken cancellationToken = default)
        {
            var rectTransform = ui.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                ui.gameObject.SetActive(true);
                return;
            }

            var canvasGroup = ui.GetComponent<CanvasGroup>();
            Vector3 startPos = rectTransform.anchoredPosition + GetOffset();
            Vector3 endPos = rectTransform.anchoredPosition;

            rectTransform.anchoredPosition = startPos;
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
                rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
                if (canvasGroup != null)
                    canvasGroup.alpha = t;
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            rectTransform.anchoredPosition = endPos;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public async UniTask HideAsync(UIBase ui, CancellationToken cancellationToken = default)
        {
            var rectTransform = ui.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                ui.gameObject.SetActive(false);
                return;
            }

            var canvasGroup = ui.GetComponent<CanvasGroup>();
            Vector3 startPos = rectTransform.anchoredPosition;
            Vector3 endPos = rectTransform.anchoredPosition + GetOffset();

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
                float t = _curve.Evaluate(elapsed / _duration);
                rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
                if (canvasGroup != null)
                    canvasGroup.alpha = 1f - t;
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            rectTransform.anchoredPosition = endPos;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }
#else
        public void Show(UIBase ui)
        {
            var rectTransform = ui.GetComponent<RectTransform>();
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
