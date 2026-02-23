using System.Collections.Generic;
using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Managers
{
    /// <summary>
    /// Manages UI layers and sorting
    /// </summary>
    public class UILayerManager
    {
        private readonly Dictionary<UILayer, Transform> layerRoots = new Dictionary<UILayer, Transform>();
        private readonly Dictionary<UILayer, Canvas> layerCanvases = new Dictionary<UILayer, Canvas>();
        private readonly Transform rootTransform;
        
        public UILayerManager(Transform rootTransform)
        {
            this.rootTransform = rootTransform;
            InitializeLayers();
        }
        
        private void InitializeLayers()
        {
            CreateLayer(UILayer.HUD, 0);
            CreateLayer(UILayer.Screen, 100);
            CreateLayer(UILayer.Popup, 200);
            CreateLayer(UILayer.Overlay, 300);
        }
        
        private void CreateLayer(UILayer layer, int sortingOrder)
        {
            var layerObj = new GameObject($"Layer_{layer}");
            layerObj.transform.SetParent(rootTransform);
            layerObj.transform.localPosition = Vector3.zero;
            layerObj.transform.localRotation = Quaternion.identity;
            layerObj.transform.localScale = Vector3.one;
            
            var canvas = layerObj.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
            
            layerObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            layerRoots[layer] = layerObj.transform;
            layerCanvases[layer] = canvas;
        }
        
        public Transform GetLayerRoot(UILayer layer)
        {
            return layerRoots.TryGetValue(layer, out var root) ? root : rootTransform;
        }
        
        public Canvas GetLayerCanvas(UILayer layer)
        {
            return layerCanvases.TryGetValue(layer, out var canvas) ? canvas : null;
        }
        
        public void SetLayerActive(UILayer layer, bool active)
        {
            if (layerRoots.TryGetValue(layer, out var root))
            {
                root.gameObject.SetActive(active);
            }
        }
    }
}
