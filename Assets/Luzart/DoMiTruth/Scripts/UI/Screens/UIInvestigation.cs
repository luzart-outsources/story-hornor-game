namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UIInvestigation : UIBase
    {
        [SerializeField] private Transform roomContainer;
        [SerializeField] private CameraPanController cameraPan;
        [SerializeField] private Button btnPause;

        private GameObject currentRoomInstance;

        protected override void Setup()
        {
            base.Setup();

            if (btnPause != null)
                GameUtil.ButtonOnClick(btnPause, OnPause);
        }

        public void LoadRoom(RoomSO room)
        {
            if (room == null) return;

            ClearRoom();

            if (room.roomPrefab == null)
            {
                Debug.LogWarning($"[UIInvestigation] Room '{room.roomName}' has no roomPrefab assigned.");
                return;
            }

            var parent = roomContainer != null ? roomContainer : transform;
            currentRoomInstance = Instantiate(room.roomPrefab, parent);

            if (cameraPan != null)
            {
                var bgRect = currentRoomInstance.GetComponent<RectTransform>();
                if (bgRect != null)
                    cameraPan.Setup(bgRect);
            }
        }

        private void ClearRoom()
        {
            if (currentRoomInstance != null)
            {
                Destroy(currentRoomInstance);
                currentRoomInstance = null;
            }
        }

        private void OnPause()
        {
            UIManager.Instance.ShowUI(UIName.Pause);
        }

        // ========== Helpers cho ActionStep ==========

        /// <summary>Image background của room (child đầu tiên có Image trong roomInstance).</summary>
        public Image GetRoomBackground()
        {
            if (currentRoomInstance == null) return null;
            return currentRoomInstance.GetComponentInChildren<Image>();
        }

        /// <summary>Tìm InteractableObject trong room khớp với SO data.</summary>
        public InteractableObject FindPropByData(InteractableObjectSO data)
        {
            if (currentRoomInstance == null || data == null) return null;

            var all = currentRoomInstance.GetComponentsInChildren<InteractableObject>(true);
            foreach (var io in all)
            {
                if (io.Data == data)
                    return io;
            }
            return null;
        }

        /// <summary>Container để spawn prop mới vào.</summary>
        public Transform GetRoomContainer()
        {
            if (currentRoomInstance != null)
                return currentRoomInstance.transform;
            return roomContainer;
        }
    }
}
