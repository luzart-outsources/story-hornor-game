namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UIInvestigation : UIBase
    {
        [SerializeField] private Image imgBackground;
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private Transform interactableContainer;
        [SerializeField] private CameraPanController cameraPan;
        [SerializeField] private InteractableObject interactablePrefab;

        public void LoadRoom(RoomSO room)
        {
            if (room == null) return;

            ClearInteractables();

            if (imgBackground != null && room.backgroundImage != null)
                imgBackground.sprite = room.backgroundImage;

            if (backgroundRect != null)
                backgroundRect.sizeDelta = room.backgroundSize;

            if (cameraPan != null)
                cameraPan.Setup(room.backgroundSize);

            SpawnInteractables(room);
        }

        private void SpawnInteractables(RoomSO room)
        {
            if (room.interactables == null || interactablePrefab == null) return;

            var parent = interactableContainer != null ? interactableContainer : backgroundRect;

            for (int i = 0; i < room.interactables.Count; i++)
            {
                var ri = room.interactables[i];
                if (ri.data == null) continue;

                var obj = Instantiate(interactablePrefab, parent);
                obj.Init(ri.data, ri.positionOnBackground);
            }
        }

        private void ClearInteractables()
        {
            var parent = interactableContainer != null ? interactableContainer : backgroundRect;
            if (parent == null) return;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}
