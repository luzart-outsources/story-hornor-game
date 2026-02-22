using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luzart.UIFramework
{
    public class UILayerManager
    {
        private readonly Dictionary<UILayer, Transform> layerRoots = new Dictionary<UILayer, Transform>();
        private readonly Dictionary<UILayer, List<UIBase>> activeLayers = new Dictionary<UILayer, List<UIBase>>();
        private readonly Canvas rootCanvas;

        public UILayerManager(Canvas rootCanvas)
        {
            this.rootCanvas = rootCanvas ?? throw new ArgumentNullException(nameof(rootCanvas));
            InitializeLayers();
        }

        private void InitializeLayers()
        {
            var layers = Enum.GetValues(typeof(UILayer));
            int sortOrder = 0;

            foreach (UILayer layer in layers)
            {
                if (layer == UILayer.None) continue;

                var layerObj = new GameObject($"Layer_{layer}");
                layerObj.transform.SetParent(rootCanvas.transform, false);

                var rectTransform = layerObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;

                var canvas = layerObj.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = sortOrder++;

                layerObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                layerRoots[layer] = layerObj.transform;
                activeLayers[layer] = new List<UIBase>();
            }
        }

        public Transform GetLayerRoot(UILayer layer)
        {
            return layerRoots.TryGetValue(layer, out var root) ? root : null;
        }

        public void RegisterUI(UIBase ui)
        {
            if (ui == null) return;

            if (activeLayers.TryGetValue(ui.Layer, out var list))
            {
                if (!list.Contains(ui))
                {
                    list.Add(ui);
                }
            }
        }

        public void UnregisterUI(UIBase ui)
        {
            if (ui == null) return;

            if (activeLayers.TryGetValue(ui.Layer, out var list))
            {
                list.Remove(ui);
            }
        }

        public List<UIBase> GetActiveUIInLayer(UILayer layer)
        {
            return activeLayers.TryGetValue(layer, out var list) ? new List<UIBase>(list) : new List<UIBase>();
        }

        public int GetActiveCount(UILayer layer)
        {
            return activeLayers.TryGetValue(layer, out var list) ? list.Count : 0;
        }

        public void Clear()
        {
            foreach (var list in activeLayers.Values)
            {
                list.Clear();
            }
        }
    }
}
