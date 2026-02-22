using UnityEngine;

namespace Luzart.UIFramework
{
    public static class UIExtensions
    {
        public static void SetAlpha(this CanvasGroup canvasGroup, float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(alpha);
            }
        }

        public static void SetInteractable(this CanvasGroup canvasGroup, bool interactable)
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
            }
        }

        public static void ResetTransform(this RectTransform rectTransform)
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
                rectTransform.localRotation = Quaternion.identity;
            }
        }

        public static void StretchToParent(this RectTransform rectTransform)
        {
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }
    }
}
