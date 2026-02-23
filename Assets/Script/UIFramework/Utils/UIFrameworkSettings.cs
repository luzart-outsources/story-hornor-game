using UnityEngine;

namespace UIFramework.Utils
{
    /// <summary>
    /// Configuration settings for UI Framework
    /// Can be created as ScriptableObject for project-wide settings
    /// </summary>
    [CreateAssetMenu(fileName = "UIFrameworkSettings", menuName = "UIFramework/Settings")]
    public class UIFrameworkSettings : ScriptableObject
    {
        [Header("Loading")]
        public bool useAddressables = false;
        public bool preloadCriticalUI = true;
        public float asyncLoadTimeout = 10f;
        
        [Header("Animation")]
        public float defaultTransitionDuration = 0.3f;
        public AnimationCurve defaultEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Performance")]
        public bool enableObjectPooling = true;
        public int defaultPoolSize = 5;
        public bool enableCaching = true;
        public int maxCachedViews = 10;
        
        [Header("Debug")]
        public bool enableDebugLogs = true;
        public bool enablePerformanceMonitoring = false;
        public bool logMemoryUsage = false;
        
        [Header("Input")]
        public KeyCode backNavigationKey = KeyCode.Escape;
        public bool enableBackNavigation = true;
        
        [Header("Safety")]
        public bool autoCleanupOnSceneLoad = true;
        public bool validateRegistryOnStart = true;
        
        private static UIFrameworkSettings instance;
        
        public static UIFrameworkSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<UIFrameworkSettings>("UIFrameworkSettings");
                    
                    if (instance == null)
                    {
                        Debug.LogWarning("[UIFrameworkSettings] No settings found in Resources. Using defaults.");
                        instance = CreateInstance<UIFrameworkSettings>();
                    }
                }
                
                return instance;
            }
        }
    }
}
