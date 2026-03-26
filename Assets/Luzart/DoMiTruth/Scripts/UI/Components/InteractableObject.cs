namespace Luzart
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using DG.Tweening;

    public class InteractableObject : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image imgHighlight;
        [SerializeField] private RectTransform rectTransform;

        private InteractableObjectSO data;
        private Tweener hoverTweener;

        public InteractableObjectSO Data => data;

        public void Init(InteractableObjectSO objectData, Vector2 position)
        {
            data = objectData;
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = data.hitboxSize;

            if (imgHighlight != null)
            {
                if (data.highlightSprite != null)
                    imgHighlight.sprite = data.highlightSprite;
                imgHighlight.enabled = false;
            }

            if (data.isOneTimeOnly && GameDataManager.Instance.HasInteracted(data.objectId))
            {
                gameObject.SetActive(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            InvestigationManager.Instance.OnObjectClicked(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (imgHighlight != null)
                imgHighlight.enabled = true;

            hoverTweener?.Kill();
            hoverTweener = rectTransform.DOScale(1.05f, 0.2f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (imgHighlight != null)
                imgHighlight.enabled = false;

            hoverTweener?.Kill();
            hoverTweener = rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        }

        private void OnDestroy()
        {
            hoverTweener?.Kill();
        }
    }
}
