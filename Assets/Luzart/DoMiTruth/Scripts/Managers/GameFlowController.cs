namespace Luzart
{
    using UnityEngine;

    public class GameFlowController : Singleton<GameFlowController>
    {
        [SerializeField] private GameConfigSO gameConfig;
        public GameConfigSO GameConfig => gameConfig;

        public void StartNewGame()
        {
            GameDataManager.Instance.ResetData();
            ShowCutscene();
        }

        public void ContinueGame()
        {
            ShowMapSelection();
        }

        public void ShowMainMenu()
        {
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.MainMenu);
        }

        public void ShowCutscene()
        {
            UIManager.Instance.ShowUI(UIName.Cutscene);
        }

        public void OnCutsceneComplete()
        {
            UIManager.Instance.HideUiActive(UIName.Cutscene);
            ShowMapSelection();
        }

        public void ShowMapSelection()
        {
            UIManager.Instance.HideAllUiActive(UIName.InvestigationHud);
            UIManager.Instance.ShowUI(UIName.MapSelection);
        }

        public void OnMapSelected(MapSO map)
        {
            if (map == null || map.rooms.Count == 0) return;

            GameDataManager.Instance.Data.currentMapId = map.mapId;
            GameDataManager.Instance.Save();

            UIManager.Instance.HideUiActive(UIName.MapSelection);

            var room = map.rooms[0];
            InvestigationManager.Instance.LoadRoom(room);

            UIManager.Instance.ShowUI(UIName.InvestigationHud);
        }

        public void ReturnToMainMenu()
        {
            UIManager.Instance.HideAll();
            ShowMainMenu();
        }
    }
}
