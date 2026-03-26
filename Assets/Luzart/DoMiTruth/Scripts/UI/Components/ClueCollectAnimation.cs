namespace Luzart
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;

    public static class ClueCollectAnimation
    {
        public static void Play(Sprite clueSprite, Vector3 startWorldPos, Transform notebookTarget, float duration, Action onComplete)
        {
            var canvas = UIManager.Instance.canvas;
            if (canvas == null)
            {
                onComplete?.Invoke();
                return;
            }

            var go = new GameObject("ClueCollectAnim", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            go.transform.SetParent(canvas.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.position = startWorldPos;
            rt.sizeDelta = new Vector2(80f, 80f);

            var img = go.GetComponent<Image>();
            img.sprite = clueSprite;
            img.preserveAspect = true;

            var cg = go.GetComponent<CanvasGroup>();

            Vector3 targetPos = notebookTarget != null ? notebookTarget.position : startWorldPos;

            var seq = DOTween.Sequence();
            seq.Append(rt.DOScale(1.3f, duration * 0.3f).SetEase(Ease.OutBack));
            seq.Append(rt.DOMove(targetPos, duration * 0.5f).SetEase(Ease.InQuad));
            seq.Join(rt.DOScale(0.3f, duration * 0.5f).SetEase(Ease.InQuad));
            seq.Join(cg.DOFade(0f, duration * 0.2f).SetDelay(duration * 0.3f));
            seq.OnComplete(() =>
            {
                UnityEngine.Object.Destroy(go);
                onComplete?.Invoke();
            });
        }
    }
}
