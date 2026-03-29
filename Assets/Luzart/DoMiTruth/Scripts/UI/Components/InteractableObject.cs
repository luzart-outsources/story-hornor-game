namespace Luzart
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using DG.Tweening;

    public class InteractableObject : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Kéo SO vào đây trong Prefab")]
        [SerializeField] private InteractableObjectSO data;

        [SerializeField] private Image imgHighlight;
        [SerializeField] private RectTransform rectTransform;

        private Tweener hoverTweener;

        public InteractableObjectSO Data => data;

        private void Start()
        {
            if (data == null) return;

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
            if (data == null) return;
            InvestigationManager.Instance.OnObjectClicked(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (imgHighlight != null)
                imgHighlight.enabled = true;

            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            hoverTweener?.Kill();
            hoverTweener = rectTransform.DOScale(1.05f, 0.2f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (imgHighlight != null)
                imgHighlight.enabled = false;

            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            hoverTweener?.Kill();
            hoverTweener = rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        }

        private void OnDestroy()
        {
            hoverTweener?.Kill();
        }
    }
}
