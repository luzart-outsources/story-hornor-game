namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    public class NotebookManager : Singleton<NotebookManager>
    {
        [SerializeField] private ClueSO[] allClues;
        [SerializeField] private DialogueCharacterSO[] allCharacters;

        [Header("SO Events")]
        [SerializeField] private StringEventChannel onClueCollected;

        private void OnEnable()
        {
            if (onClueCollected != null)
                onClueCollected.Register(OnClueCollectedHandler);
        }

        private void OnDisable()
        {
            if (onClueCollected != null)
                onClueCollected.Unregister(OnClueCollectedHandler);
        }

        private void OnClueCollectedHandler(string clueId)
        {
            // Auto-refresh notebook UI if open
            var notebookUI = UIManager.Instance.GetUiActive<UINotebook>(UIName.Notebook);
            if (notebookUI != null)
            {
                notebookUI.RefreshUI();
            }
        }

        public List<ClueSO> GetCollectedClues()
        {
            var result = new List<ClueSO>();
            if (allClues == null) return result;

            for (int i = 0; i < allClues.Length; i++)
            {
                if (allClues[i] != null && GameDataManager.Instance.HasClue(allClues[i].clueId))
                {
                    result.Add(allClues[i]);
                }
            }
            return result;
        }

        public List<DialogueCharacterSO> GetMetCharacters()
        {
            var result = new List<DialogueCharacterSO>();
            if (allCharacters == null) return result;

            for (int i = 0; i < allCharacters.Length; i++)
            {
                if (allCharacters[i] != null && GameDataManager.Instance.HasMetCharacter(allCharacters[i].characterId))
                {
                    result.Add(allCharacters[i]);
                }
            }
            return result;
        }

        public int GetCollectedClueCount()
        {
            if (allClues == null) return 0;
            int count = 0;
            for (int i = 0; i < allClues.Length; i++)
            {
                if (allClues[i] != null && GameDataManager.Instance.HasClue(allClues[i].clueId))
                    count++;
            }
            return count;
        }

        public int GetTotalClueCount()
        {
            return allClues != null ? allClues.Length : 0;
        }
    }
}
