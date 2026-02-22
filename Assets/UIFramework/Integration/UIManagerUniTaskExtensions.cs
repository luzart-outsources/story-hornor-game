// Optional: UniTask Integration
// Install UniTask package from: https://github.com/Cysharp/UniTask
// Uncomment this file after installation

/*
#if UNITASK_SUPPORT
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Luzart.UIFramework
{
    public static class UIManagerUniTaskExtensions
    {
        public static async UniTask<T> ShowAsyncUniTask<T>(this UIManager manager, object data = null, IUITransition customTransition = null, CancellationToken cancellationToken = default) where T : UIBase
        {
            return await manager.ShowAsync<T>(data, customTransition, cancellationToken).AsUniTask();
        }

        public static async UniTask<UIBase> ShowAsyncUniTask(this UIManager manager, string viewId, object data = null, IUITransition customTransition = null, CancellationToken cancellationToken = default)
        {
            return await manager.ShowAsync(viewId, data, customTransition, cancellationToken).AsUniTask();
        }
    }

    public class UIFadeTransitionUniTask : IUITransition
    {
        private readonly float duration;
        private readonly AnimationCurve curve;

        public UIFadeTransitionUniTask(float duration = 0.3f, AnimationCurve curve = null)
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
                await AnimateAlphaUniTask(canvasGroup, 0f, 1f, cancellationToken);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UIFadeTransitionUniTask: Show animation cancelled.");
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
                await AnimateAlphaUniTask(canvasGroup, 1f, 0f, cancellationToken);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UIFadeTransitionUniTask: Hide animation cancelled.");
            }
        }

        private async UniTask AnimateAlphaUniTask(CanvasGroup canvasGroup, float from, float to, CancellationToken cancellationToken)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (canvasGroup == null)
                    return;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = curve.Evaluate(t);
                canvasGroup.alpha = Mathf.Lerp(from, to, curvedT);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = to;
            }
        }
    }

    // Usage Example:
    // await UIManager.Instance.ShowAsyncUniTask<MainMenuScreen>();
}
#endif
*/
