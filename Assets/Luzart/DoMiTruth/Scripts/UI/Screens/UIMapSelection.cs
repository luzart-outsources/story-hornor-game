namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    public class UIMapSelection : UIBase
    {
        [SerializeField] private Transform gridContainer;
        [SerializeField] private MapSelectionItem itemPrefab;

        public List<MapSelectionItem> mapItems = new List<MapSelectionItem>();

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);
            PopulateMapList();
        }

        private void PopulateMapList()
        {
            var config = GameFlowController.Instance.GameConfig;
            if (config == null || config.allMaps == null) return;

            MasterHelper.InitListObj(config.allMaps.Count, itemPrefab, mapItems, gridContainer, (item, index) =>
            {
                item.gameObject.SetActive(true);
                item.Init(config.allMaps[index], OnMapSelected);
            });
        }

        private void OnMapSelected(MapSO map)
        {
            GameFlowController.Instance.OnMapSelected(map);
        }
    }
}
