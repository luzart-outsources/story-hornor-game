namespace Luzart
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class MapSelectionItem : MonoBehaviour
    {
        [SerializeField] private Image imgThumbnail;
        [SerializeField] private TMP_Text txtName;
        [SerializeField] private Button btnSelect;

        private MapSO mapData;
        private Action<MapSO> onClick;

        public void Init(MapSO map, Action<MapSO> onClickCallback)
        {
            mapData = map;
            onClick = onClickCallback;

            if (imgThumbnail != null && map.mapThumbnail != null)
                imgThumbnail.sprite = map.mapThumbnail;

            if (txtName != null)
                txtName.text = map.mapName;

            if (btnSelect != null)
                GameUtil.ButtonOnClick(btnSelect, OnClick);
        }

        private void OnClick()
        {
            onClick?.Invoke(mapData);
        }
    }
}
