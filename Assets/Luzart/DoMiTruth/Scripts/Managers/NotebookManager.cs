namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    public class NotebookManager : Singleton<NotebookManager>
    {
        [Header("SO Events")]
        [SerializeField] private StringEventChannel onClueCollected;

        // Auto-lấy từ GameConfig, không cần kéo tay
        private List<ClueSO> AllClues => GameFlowController.Instance?.GameConfig?.allClues;
        private List<DialogueCharacterSO> AllCharacters => GameFlowController.Instance?.GameConfig?.allCharacters;

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
            var notebookUI = UIManager.Instance.GetUiActive<UINotebook>(UIName.Notebook);
            if (notebookUI != null)
            {
                notebookUI.RefreshUI();
            }
        }

        public List<ClueSO> GetCollectedClues()
        {
            var result = new List<ClueSO>();
            var clues = AllClues;
            if (clues == null) return result;

            for (int i = 0; i < clues.Count; i++)
            {
                if (clues[i] != null && GameDataManager.Instance.HasClue(clues[i].clueId))
                    result.Add(clues[i]);
            }
            return result;
        }

        public List<DialogueCharacterSO> GetMetCharacters()
        {
            var result = new List<DialogueCharacterSO>();
            var chars = AllCharacters;
            if (chars == null) return result;

            for (int i = 0; i < chars.Count; i++)
            {
                if (chars[i] != null && GameDataManager.Instance.HasMetCharacter(chars[i].characterId))
                    result.Add(chars[i]);
            }
            return result;
        }

        public int GetCollectedClueCount()
        {
            var clues = AllClues;
            if (clues == null) return 0;
            int count = 0;
            for (int i = 0; i < clues.Count; i++)
            {
                if (clues[i] != null && GameDataManager.Instance.HasClue(clues[i].clueId))
                    count++;
            }
            return count;
        }

        public int GetTotalClueCount()
        {
            return AllClues != null ? AllClues.Count : 0;
        }
    }
}
