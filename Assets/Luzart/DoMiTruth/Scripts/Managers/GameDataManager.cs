namespace Luzart
{
    using UnityEngine;

    public class GameDataManager : SingletonSaveLoad<GameSaveData, GameDataManager>
    {
        protected override string KEYLOAD => "DMT_SAVE";

        [Header("SO Events")]
        [SerializeField] private StringEventChannel onClueCollected;
        [SerializeField] private GameEventChannel onNotebookUpdated;

        public void AddClue(string clueId)
        {
            if (string.IsNullOrEmpty(clueId) || Data.collectedClueIds.Contains(clueId)) return;
            Data.collectedClueIds.Add(clueId);
            Save();
            onClueCollected?.Raise(clueId);
            onNotebookUpdated?.Raise();
        }

        public void UnlockItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Data.unlockedItemIds.Contains(itemId)) return;
            Data.unlockedItemIds.Add(itemId);
            Save();
        }

        public void MarkInteracted(string objectId)
        {
            if (string.IsNullOrEmpty(objectId) || Data.interactedObjectIds.Contains(objectId)) return;
            Data.interactedObjectIds.Add(objectId);
            Save();
        }

        public void MarkCharacterMet(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || Data.metCharacterIds.Contains(characterId)) return;
            Data.metCharacterIds.Add(characterId);
            Save();
        }

        public bool HasClue(string clueId) => Data.collectedClueIds.Contains(clueId);
        public bool HasInteracted(string objectId) => Data.interactedObjectIds.Contains(objectId);
        public bool IsItemUnlocked(string itemId) => Data.unlockedItemIds.Contains(itemId);
        public bool HasMetCharacter(string characterId) => Data.metCharacterIds.Contains(characterId);
        public bool HasSaveData() => Data.collectedClueIds.Count > 0 || !string.IsNullOrEmpty(Data.currentMapId);

        public void ResetData()
        {
            Data = new GameSaveData();
            Save();
        }
    }
}
