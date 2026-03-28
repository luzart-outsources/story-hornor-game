namespace Luzart
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// Một dot trong grid 3x3 của SwipePattern lock.
    /// Gắn vào mỗi dot GameObject, cần có Image component.
    /// </summary>
    public class SwipePatternDot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
    {
        [SerializeField] private Image imgDot;
        [SerializeField] private GameObject objSelected;
        [SerializeField] private GameObject objUnselected;

        private int dotIndex;
        private Action<int> onPointerDown;
        private Action<int> onPointerEnter;
        private Action<int> onPointerUp;

        public void Init(int index, Action<int> onDown, Action<int> onEnter, Action<int> onUp)
        {
            dotIndex = index;
            onPointerDown = onDown;
            onPointerEnter = onEnter;
            onPointerUp = onUp;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (objSelected != null) objSelected.SetActive(selected);
            if (objUnselected != null) objUnselected.SetActive(!selected);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDown?.Invoke(dotIndex);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(dotIndex);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke(dotIndex);
        }
    }
}
