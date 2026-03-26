namespace Luzart
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class GameSaveData
    {
        public List<string> collectedClueIds = new List<string>();
        public List<string> unlockedItemIds = new List<string>();
        public List<string> interactedObjectIds = new List<string>();
        public List<string> metCharacterIds = new List<string>();
        public string currentMapId;
        public int currentFlowStep;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
    }
}
