namespace Luzart
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class CameraPanController : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private RectTransform viewportRect;

        private Vector2 dragStartPos;
        private Vector2 bgStartPos;
        private Vector2 minBounds;
        private Vector2 maxBounds;

        public void Setup(RectTransform bgRect)
        {
            backgroundRect = bgRect;
            if (backgroundRect == null) return;

            CalculateBounds();
            ClampPosition();

            Debug.Log($"[CameraPan] bgRect.size={backgroundRect.rect.size}, bgSizeDelta={backgroundRect.sizeDelta}, " +
                      $"viewport={viewportRect?.rect.size}, bounds=({minBounds}, {maxBounds})");
        }

        private void CalculateBounds()
        {
            if (backgroundRect == null || viewportRect == null) return;

            var viewportSize = viewportRect.rect.size;
            // Dùng rect.size thay vì sizeDelta để lấy đúng size thật (kể cả khi dùng stretch anchors)
            var bgSize = backgroundRect.rect.size;

            float halfDiffX = (bgSize.x - viewportSize.x) * 0.5f;
            float halfDiffY = (bgSize.y - viewportSize.y) * 0.5f;

            minBounds = new Vector2(-Mathf.Max(0, halfDiffX), -Mathf.Max(0, halfDiffY));
            maxBounds = new Vector2(Mathf.Max(0, halfDiffX), Mathf.Max(0, halfDiffY));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragStartPos = eventData.position;
            bgStartPos = backgroundRect.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - dragStartPos;
            backgroundRect.anchoredPosition = bgStartPos + delta;
            ClampPosition();
        }

        private void ClampPosition()
        {
            if (backgroundRect == null) return;

            var pos = backgroundRect.anchoredPosition;
            pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
            pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
            backgroundRect.anchoredPosition = pos;
        }
    }
}
