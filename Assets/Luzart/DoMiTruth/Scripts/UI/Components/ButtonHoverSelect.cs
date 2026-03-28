namespace Luzart
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// Gắn vào mỗi button trong Main Menu.
    /// Khi hover vào thì Select(true) trên toggle của mình, Select(false) trên tất cả anh em trong group.
    /// Khi rời chuột thì Select(false) trên mình.
    ///
    /// Cách dùng:
    /// 1. Gắn ButtonHoverSelect vào mỗi button
    /// 2. Gắn SelectToggleGameObject vào mỗi button với 2 GO (normal/raised)
    /// 3. Kéo SelectToggleGameObject vào field hoverToggle
    /// 4. Kéo tất cả ButtonHoverSelect vào field group (hoặc để UIMainMenu quản lý)
    /// </summary>
    public class ButtonHoverSelect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private SelectToggleGameObject hoverToggle;

        private ButtonHoverSelect[] group;

        public void SetGroup(ButtonHoverSelect[] hoverGroup)
        {
            group = hoverGroup;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (group != null)
            {
                for (int i = 0; i < group.Length; i++)
                {
                    if (group[i] != null && group[i] != this)
                        group[i].Deselect();
                }
            }

            if (hoverToggle != null)
                hoverToggle.Select(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Deselect();
        }

        public void Deselect()
        {
            if (hoverToggle != null)
                hoverToggle.Select(false);
        }
    }
}
