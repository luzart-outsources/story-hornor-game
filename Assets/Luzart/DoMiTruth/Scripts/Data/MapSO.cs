namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewMap", menuName = "DoMiTruth/Map")]
    public class MapSO : ScriptableObject
    {
        public string mapId;
        public string mapName;
        public Sprite mapThumbnail;
        public List<RoomSO> rooms = new List<RoomSO>();
    }
}
