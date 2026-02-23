using UnityEngine;
using UIFramework.Managers;
using UIFramework.Data;

namespace UIFramework
{
    /// <summary>
    /// Bootstrap class for UI Framework
    /// Place this in your initial scene to setup the framework
    /// </summary>
    public class UIFrameworkBootstrap : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private UIRegistry uiRegistry;
        [SerializeField] private bool useAddressables = false;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool initializeOnAwake = true;
        
        [Header("Performance")]
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private bool enableDebugLogging = true;
        
        [Header("Pooling Pre-warm")]
        [SerializeField] private PoolConfig[] prewarmConfigs;
        
        private void Awake()
        {
            if (initializeOnAwake)
            {
                Initialize();
            }
        }
        
        public void Initialize()
        {
            Debug.Log("[UIFrameworkBootstrap] Initializing UI Framework...");
            
            // Initialize UIManager
            var uiManager = UIManager.Instance;
            
            if (uiRegistry != null)
            {
                uiManager.SetRegistry(uiRegistry);
                Debug.Log("[UIFrameworkBootstrap] UIRegistry assigned");
            }
            else
            {
                Debug.LogWarning("[UIFrameworkBootstrap] No UIRegistry assigned!");
            }
            
            // Configure DontDestroyOnLoad
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(uiManager.gameObject);
            }
            
            // Pre-warm pools
            if (prewarmConfigs != null && prewarmConfigs.Length > 0)
            {
                PrewarmPools();
            }
            
            // Setup performance monitoring
            if (enablePerformanceMonitoring)
            {
                InvokeRepeating(nameof(LogPerformanceReport), 60f, 60f); // Every minute
            }
            
            Debug.Log("[UIFrameworkBootstrap] UI Framework initialized successfully!");
        }
        
        private void PrewarmPools()
        {
            Debug.Log("[UIFrameworkBootstrap] Pre-warming UI pools...");
            
            foreach (var config in prewarmConfigs)
            {
                if (!string.IsNullOrEmpty(config.viewId) && config.count > 0)
                {
                    UIManager.Instance.PrewarmPool(config.viewId, config.count);
                    Debug.Log($"  - Pre-warmed {config.viewId} with {config.count} instances");
                }
            }
        }
        
        private void LogPerformanceReport()
        {
            if (enableDebugLogging && enablePerformanceMonitoring)
            {
                Utils.UIPerformanceMonitor.Instance.LogReport();
            }
        }
        
        private void OnDestroy()
        {
            // Cleanup
            Communication.EventBus.Instance.Clear();
        }
        
        [System.Serializable]
        public class PoolConfig
        {
            public string viewId;
            public int count = 3;
        }
    }
}
