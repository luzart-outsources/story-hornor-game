using System;
using System.Collections.Generic;
using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Data
{
    /// <summary>
    /// ScriptableObject-based UI Registry
    /// Stores all UI configurations and addresses
    /// </summary>
    [CreateAssetMenu(fileName = "UIRegistry", menuName = "UIFramework/UI Registry")]
    public class UIRegistry : ScriptableObject
    {
        [SerializeField] private List<UIConfig> uiConfigs = new List<UIConfig>();
        
        private Dictionary<string, UIConfig> configCache;
        
        public IReadOnlyList<UIConfig> UIConfigs => uiConfigs;
        
        private void OnEnable()
        {
            BuildCache();
        }
        
        private void BuildCache()
        {
            configCache = new Dictionary<string, UIConfig>();
            
            foreach (var config in uiConfigs)
            {
                if (!string.IsNullOrEmpty(config.ViewId))
                {
                    if (configCache.ContainsKey(config.ViewId))
                    {
                        Debug.LogError($"[UIRegistry] Duplicate ViewId detected: {config.ViewId}");
                    }
                    else
                    {
                        configCache[config.ViewId] = config;
                    }
                }
            }
        }
        
        public UIConfig GetConfig(string viewId)
        {
            if (configCache == null)
                BuildCache();
                
            return configCache.TryGetValue(viewId, out var config) ? config : null;
        }
        
        public void AddConfig(UIConfig config)
        {
            if (uiConfigs.Contains(config))
                return;
                
            uiConfigs.Add(config);
            BuildCache();
        }
        
        public void RemoveConfig(string viewId)
        {
            uiConfigs.RemoveAll(c => c.ViewId == viewId);
            BuildCache();
        }
        
        public bool ValidateRegistry()
        {
            HashSet<string> viewIds = new HashSet<string>();
            bool isValid = true;
            
            foreach (var config in uiConfigs)
            {
                if (string.IsNullOrEmpty(config.ViewId))
                {
                    Debug.LogError($"[UIRegistry] Empty ViewId found in config");
                    isValid = false;
                    continue;
                }
                
                if (!viewIds.Add(config.ViewId))
                {
                    Debug.LogError($"[UIRegistry] Duplicate ViewId: {config.ViewId}");
                    isValid = false;
                }
                
                if (config.LoadMode == UILoadMode.Addressable && string.IsNullOrEmpty(config.AddressablePath))
                {
                    Debug.LogError($"[UIRegistry] Addressable path missing for {config.ViewId}");
                    isValid = false;
                }
                
                if (config.LoadMode == UILoadMode.Prefab && config.Prefab == null)
                {
                    Debug.LogError($"[UIRegistry] Prefab missing for {config.ViewId}");
                    isValid = false;
                }
            }
            
            return isValid;
        }
    }
    
    /// <summary>
    /// Configuration for a single UI element
    /// </summary>
    [Serializable]
    public class UIConfig
    {
        [SerializeField] private string viewId;
        [SerializeField] private UILayer layer;
        [SerializeField] private UILoadMode loadMode;
        [SerializeField] private GameObject prefab; // For direct prefab loading
        [SerializeField] private string addressablePath; // For addressable loading
        [SerializeField] private bool enableCaching;
        [SerializeField] private bool enablePooling;
        [SerializeField] private int poolSize = 1;
        
        public string ViewId => viewId;
        public UILayer Layer => layer;
        public UILoadMode LoadMode => loadMode;
        public GameObject Prefab => prefab;
        public string AddressablePath => addressablePath;
        public bool EnableCaching => enableCaching;
        public bool EnablePooling => enablePooling;
        public int PoolSize => poolSize;
    }
    
    public enum UILoadMode
    {
        Prefab,
        Addressable
    }
}
