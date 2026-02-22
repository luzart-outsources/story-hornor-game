using UnityEngine;

namespace Luzart.UIFramework
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Luzart/UI Framework/UI Config")]
    public class UIConfig : ScriptableObject
    {
        [Header("Performance")]
        [Tooltip("Enable object pooling for popups")]
        public bool enablePooling = true;

        [Tooltip("Default pool size for pooled UIs")]
        public int defaultPoolSize = 3;

        [Tooltip("Frame interval for EventBus cleanup (0 = disabled)")]
        public int eventBusCleanupInterval = 300;

        [Header("Transitions")]
        [Tooltip("Default transition duration in seconds")]
        public float defaultTransitionDuration = 0.3f;

        [Tooltip("Default animation curve for transitions")]
        public AnimationCurve defaultAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Loading")]
        [Tooltip("Use Addressables system for loading")]
        public bool useAddressables = false;

        [Tooltip("Timeout for loading operations (seconds)")]
        public float loadTimeout = 10f;

        [Header("Debug")]
        [Tooltip("Enable debug logging")]
        public bool enableDebugLog = true;

        [Tooltip("Log UI lifecycle events")]
        public bool logLifecycleEvents = false;

        [Header("Memory")]
        [Tooltip("Automatically unload unused assets on scene change")]
        public bool autoUnloadOnSceneChange = true;

        [Tooltip("Max cached UIs before forcing cleanup")]
        public int maxCachedUIs = 20;
    }
}
