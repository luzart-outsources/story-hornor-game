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
            ShowBriefing();
        }

        public void ShowBriefing()
        {
            var briefing = gameConfig != null ? gameConfig.briefingDialogue : null;
            if (briefing == null || briefing.lines.Count == 0)
            {
                ShowMapSelection();
                return;
            }

            var ui = UIManager.Instance.ShowUI<UIBriefing>(UIName.Briefing);
            if (ui != null)
            {
                ui.StartBriefing(briefing, gameConfig.briefingCharacters, OnBriefingComplete);
            }
            else
            {
                // Fallback nếu chưa có prefab Briefing → dùng DialogueManager
                DialogueManager.Instance.StartDialogue(briefing, OnBriefingComplete);
            }
        }

        private void OnBriefingComplete()
        {
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

            var room = map.rooms[0];

            // Circle wipe transition
            if (UITransition.Instance != null)
            {
                UITransition.Instance.PlayTransition(
                    onMidpoint: () =>
                    {
                        UIManager.Instance.HideUiActive(UIName.MapSelection);
                        InvestigationManager.Instance.LoadRoom(room);
                        UIManager.Instance.ShowUI(UIName.InvestigationHud);
                    });
            }
            else
            {
                // Fallback không có transition
                UIManager.Instance.HideUiActive(UIName.MapSelection);
                InvestigationManager.Instance.LoadRoom(room);
                UIManager.Instance.ShowUI(UIName.InvestigationHud);
            }
        }

        /// <summary>
        /// Quay về MapSelector từ room (có transition).
        /// </summary>
        public void ReturnToMapSelection()
        {
            if (UITransition.Instance != null)
            {
                UITransition.Instance.PlayTransition(
                    onMidpoint: () =>
                    {
                        InvestigationManager.Instance.UnloadRoom();
                        UIManager.Instance.HideUiActive(UIName.InvestigationHud);
                        UIManager.Instance.ShowUI(UIName.MapSelection);
                    });
            }
            else
            {
                InvestigationManager.Instance.UnloadRoom();
                UIManager.Instance.HideUiActive(UIName.InvestigationHud);
                UIManager.Instance.ShowUI(UIName.MapSelection);
            }
        }

        public void ReturnToMainMenu()
        {
            UIManager.Instance.HideAll();
            ShowMainMenu();
        }
    }
}
