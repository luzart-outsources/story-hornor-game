using System;
using System.Threading;
using UnityEngine;

namespace Luzart.UIFramework
{
    public class UICompositeTransition : IUITransition
    {
        private readonly IUITransition[] transitions;

        public UICompositeTransition(params IUITransition[] transitions)
        {
            this.transitions = transitions ?? throw new ArgumentNullException(nameof(transitions));
        }

        public async void PlayShowTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
        {
            if (target == null || transitions.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int completed = 0;
            bool allCompleted = false;

            foreach (var transition in transitions)
            {
                if (transition == null) continue;

                transition.PlayShowTransition(target, () =>
                {
                    completed++;
                    if (completed >= transitions.Length && !allCompleted)
                    {
                        allCompleted = true;
                        onComplete?.Invoke();
                    }
                }, cancellationToken);
            }

            while (!allCompleted && !cancellationToken.IsCancellationRequested)
            {
                await System.Threading.Tasks.Task.Yield();
            }
        }

        public async void PlayHideTransition(RectTransform target, Action onComplete, CancellationToken cancellationToken = default)
        {
            if (target == null || transitions.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int completed = 0;
            bool allCompleted = false;

            foreach (var transition in transitions)
            {
                if (transition == null) continue;

                transition.PlayHideTransition(target, () =>
                {
                    completed++;
                    if (completed >= transitions.Length && !allCompleted)
                    {
                        allCompleted = true;
                        onComplete?.Invoke();
                    }
                }, cancellationToken);
            }

            while (!allCompleted && !cancellationToken.IsCancellationRequested)
            {
                await System.Threading.Tasks.Task.Yield();
            }
        }
    }
}
