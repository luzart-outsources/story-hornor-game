namespace Luzart
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewRoom", menuName = "DoMiTruth/Room")]
    public class RoomSO : ScriptableObject
    {
        public string roomId;
        public string roomName;
        public GameObject roomPrefab;
        public DialogueSequenceSO entryDialogue;
    }
}
