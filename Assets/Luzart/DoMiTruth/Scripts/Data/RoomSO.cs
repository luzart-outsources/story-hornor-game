namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewRoom", menuName = "DoMiTruth/Room")]
    public class RoomSO : ScriptableObject
    {
        public string roomId;
        public string roomName;
        public Sprite backgroundImage;
        public Vector2 backgroundSize = new Vector2(1920f, 1080f);
        public List<RoomInteractable> interactables = new List<RoomInteractable>();
        public DialogueSequenceSO entryDialogue;
    }

    [Serializable]
    public class RoomInteractable
    {
        public InteractableObjectSO data;
        public Vector2 positionOnBackground;
    }
}
