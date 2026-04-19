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

        //[SerializeField] private Image imgHighlight;
        [SerializeField] private RectTransform rectTransform;

        private Tweener hoverTweener;

        public InteractableObjectSO Data => data;

        private void Start()
        {
            if (data == null) return;

            // 1. One-time: đã interact rồi → ẩn vĩnh viễn
            if (data.isOneTimeOnly && GameDataManager.Instance.HasInteracted(data.objectId))
            {
                gameObject.SetActive(false);
                return;
            }

            // 2. Show conditions: prop chỉ hiện khi TẤT CẢ conditions thỏa mãn
            if (data.showConditions != null && data.showConditions.Count > 0)
            {
                foreach (var cond in data.showConditions)
                {
                    if (cond != null && !cond.Evaluate())
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (data == null) return;
            SoundManager.Instance?.PlayInteractSFX();
            InvestigationManager.Instance.OnObjectClicked(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // if (imgHighlight != null)
            //     imgHighlight.enabled = true;
            //
            // if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            // hoverTweener?.Kill();
            // hoverTweener = rectTransform.DOScale(1.05f, 0.2f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // if (imgHighlight != null)
            //     imgHighlight.enabled = false;
            //
            // if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            // hoverTweener?.Kill();
            // hoverTweener = rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        }

        private void OnDestroy()
        {
            hoverTweener?.Kill();
        }
    }
}
