namespace Luzart
{
    using UnityEngine;

    public class UIMapSelection : UIBase
    {
        [SerializeField] private Transform gridContainer;
        [SerializeField] private MapSelectionItem itemPrefab;

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);
            PopulateMapList();
        }

        private void PopulateMapList()
        {
            ClearContainer();

            var config = GameFlowController.Instance.GameConfig;
            if (config == null || config.allMaps == null) return;

            for (int i = 0; i < config.allMaps.Count; i++)
            {
                var map = config.allMaps[i];
                if (map == null) continue;

                var item = Instantiate(itemPrefab, gridContainer);
                item.Init(map, OnMapSelected);
            }
        }

        private void OnMapSelected(MapSO map)
        {
            GameFlowController.Instance.OnMapSelected(map);
        }

        private void ClearContainer()
        {
            if (gridContainer == null) return;
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(gridContainer.GetChild(i).gameObject);
            }
        }
    }
}
