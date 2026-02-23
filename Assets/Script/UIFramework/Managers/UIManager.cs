using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UIFramework.Core;
using UIFramework.Data;
using UIFramework.Loaders;
using UIFramework.Pooling;

namespace UIFramework.Managers
{
    /// <summary>
    /// Central UI Manager - manages all UI lifecycle, loading, caching
    /// Singleton pattern for easy access, but instance-based for testability
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("[UIManager]");
                    instance = go.AddComponent<UIManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        [SerializeField] private UIRegistry registry;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private bool useAddressables = false;
        
        private IUILoader loader;
        private UILayerManager layerManager;
        private UIPool uiPool;
        
        private readonly Dictionary<string, UIBase> openedViews = new Dictionary<string, UIBase>();
        private readonly Dictionary<string, UIBase> cachedViews = new Dictionary<string, UIBase>();
        private readonly Stack<string> screenStack = new Stack<string>();
        
        #if UNITASK_SUPPORT
        private readonly Dictionary<string, CancellationTokenSource> loadingOperations = new Dictionary<string, CancellationTokenSource>();
        #endif
        
        private bool isInitialized = false;
        
        #region Initialization
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            Initialize();
        }
        
        public void Initialize()
        {
            if (isInitialized)
                return;
            
            // Create root canvas if not assigned
            if (rootCanvas == null)
            {
                var canvasObj = new GameObject("UICanvas");
                canvasObj.transform.SetParent(transform);
                rootCanvas = canvasObj.AddComponent<Canvas>();
                rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            // Initialize loader
            if (useAddressables)
            {
                loader = new AddressableUILoader();
            }
            else
            {
                loader = new PrefabUILoader();
            }
            
            // Initialize layer manager
            layerManager = new UILayerManager(rootCanvas.transform);
            
            // Initialize pool
            var poolRoot = new GameObject("UIPool").transform;
            poolRoot.SetParent(transform);
            uiPool = new UIPool(poolRoot);
            
            isInitialized = true;
            
            Debug.Log("[UIManager] Initialized successfully");
        }
        
        public void SetRegistry(UIRegistry registry)
        {
            this.registry = registry;
            
            if (registry != null)
            {
                registry.ValidateRegistry();
            }
        }
        
        #endregion
        
        #region Show/Hide UI
        
        /// <summary>
        /// Show UI by type (synchronous)
        /// </summary>
        public T Show<T>(IUIData data = null) where T : UIBase
        {
            var viewId = typeof(T).Name;
            return Show(viewId, data) as T;
        }
        
        /// <summary>
        /// Show UI by ID (synchronous)
        /// </summary>
        public UIBase Show(string viewId, IUIData data = null)
        {
            if (string.IsNullOrEmpty(viewId))
            {
                Debug.LogError("[UIManager] ViewId is null or empty");
                return null;
            }
            
            // Check if already opened
            if (openedViews.TryGetValue(viewId, out var existingView))
            {
                if (existingView.State == UIState.Showing || existingView.State == UIState.Visible)
                {
                    Debug.LogWarning($"[UIManager] {viewId} is already opened");
                    return existingView;
                }
            }
            
            // Try to get from cache
            UIBase view = GetFromCacheOrPool(viewId);
            
            // Load new instance if not cached
            if (view == null)
            {
                view = LoadView(viewId);
            }
            
            if (view == null)
            {
                Debug.LogError($"[UIManager] Failed to load view: {viewId}");
                return null;
            }
            
            // Initialize and show
            if (view.State == UIState.None || view.State == UIState.Hidden)
            {
                view.Initialize(data);
            }
            
            openedViews[viewId] = view;
            
            // Manage screen stack
            if (view.Layer == UILayer.Screen)
            {
                screenStack.Push(viewId);
            }
            
            view.Show();
            
            return view;
        }
        
        #if UNITASK_SUPPORT
        /// <summary>
        /// Show UI asynchronously (UniTask)
        /// Handles race conditions and cancellation
        /// </summary>
        public async Cysharp.Threading.Tasks.UniTask<T> ShowAsync<T>(IUIData data = null, CancellationToken cancellationToken = default) where T : UIBase
        {
            var viewId = typeof(T).Name;
            var result = await ShowAsync(viewId, data, cancellationToken);
            return result as T;
        }
        
        public async Cysharp.Threading.Tasks.UniTask<UIBase> ShowAsync(string viewId, IUIData data = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(viewId))
            {
                Debug.LogError("[UIManager] ViewId is null or empty");
                return null;
            }
            
            // Check if already opened
            if (openedViews.TryGetValue(viewId, out var existingView))
            {
                if (existingView.State == UIState.Showing || existingView.State == UIState.Visible)
                {
                    Debug.LogWarning($"[UIManager] {viewId} is already opened");
                    return existingView;
                }
            }
            
            // Handle race condition: if already loading, cancel previous and restart
            if (loadingOperations.ContainsKey(viewId))
            {
                loadingOperations[viewId].Cancel();
                loadingOperations.Remove(viewId);
            }
            
            // Create new cancellation token source
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            loadingOperations[viewId] = cts;
            
            try
            {
                // Try to get from cache
                UIBase view = GetFromCacheOrPool(viewId);
                
                // Load new instance if not cached
                if (view == null)
                {
                    view = await LoadViewAsync(viewId, cts.Token);
                }
                
                if (view == null)
                {
                    Debug.LogError($"[UIManager] Failed to load view: {viewId}");
                    return null;
                }
                
                // Check if cancelled during load
                if (cts.Token.IsCancellationRequested)
                {
                    Debug.Log($"[UIManager] Show cancelled: {viewId}");
                    return null;
                }
                
                // Initialize and show
                if (view.State == UIState.None || view.State == UIState.Hidden)
                {
                    view.Initialize(data);
                }
                
                openedViews[viewId] = view;
                
                // Manage screen stack
                if (view.Layer == UILayer.Screen)
                {
                    screenStack.Push(viewId);
                }
                
                await view.ShowAsync(cts.Token);
                
                return view;
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[UIManager] Show operation cancelled: {viewId}");
                return null;
            }
            finally
            {
                loadingOperations.Remove(viewId);
                cts.Dispose();
            }
        }
        #endif
        
        /// <summary>
        /// Hide UI by type
        /// </summary>
        public void Hide<T>() where T : UIBase
        {
            var viewId = typeof(T).Name;
            Hide(viewId);
        }
        
        /// <summary>
        /// Hide UI by ID
        /// </summary>
        public void Hide(string viewId)
        {
            if (!openedViews.TryGetValue(viewId, out var view))
                return;
            
            #if UNITASK_SUPPORT
            // Cancel loading if in progress
            if (loadingOperations.TryGetValue(viewId, out var cts))
            {
                cts.Cancel();
                loadingOperations.Remove(viewId);
            }
            #endif
            
            view.Hide();
            openedViews.Remove(viewId);
            
            // Remove from screen stack
            if (view.Layer == UILayer.Screen && screenStack.Count > 0 && screenStack.Peek() == viewId)
            {
                screenStack.Pop();
            }
            
            // Handle caching/pooling
            var config = registry?.GetConfig(viewId);
            if (config != null)
            {
                if (config.EnablePooling)
                {
                    uiPool.Return(view);
                }
                else if (config.EnableCaching)
                {
                    cachedViews[viewId] = view;
                }
                else
                {
                    view.Dispose();
                }
            }
            else
            {
                view.Dispose();
            }
        }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask HideAsync<T>(CancellationToken cancellationToken = default) where T : UIBase
        {
            var viewId = typeof(T).Name;
            await HideAsync(viewId, cancellationToken);
        }
        
        public async Cysharp.Threading.Tasks.UniTask HideAsync(string viewId, CancellationToken cancellationToken = default)
        {
            if (!openedViews.TryGetValue(viewId, out var view))
                return;
            
            // Cancel loading if in progress
            if (loadingOperations.TryGetValue(viewId, out var cts))
            {
                cts.Cancel();
                loadingOperations.Remove(viewId);
            }
            
            await view.HideAsync(cancellationToken);
            openedViews.Remove(viewId);
            
            // Remove from screen stack
            if (view.Layer == UILayer.Screen && screenStack.Count > 0 && screenStack.Peek() == viewId)
            {
                screenStack.Pop();
            }
            
            // Handle caching/pooling
            var config = registry?.GetConfig(viewId);
            if (config != null)
            {
                if (config.EnablePooling)
                {
                    uiPool.Return(view);
                }
                else if (config.EnableCaching)
                {
                    cachedViews[viewId] = view;
                }
                else
                {
                    view.Dispose();
                }
            }
            else
            {
                view.Dispose();
            }
        }
        #endif
        
        /// <summary>
        /// Hide all UI elements
        /// </summary>
        public void HideAll()
        {
            var viewsToHide = openedViews.Keys.ToList();
            
            foreach (var viewId in viewsToHide)
            {
                Hide(viewId);
            }
        }
        
        /// <summary>
        /// Hide all except specified views
        /// </summary>
        public void HideAllExcept(params string[] ignoreViewIds)
        {
            var ignoreSet = new HashSet<string>(ignoreViewIds);
            var viewsToHide = openedViews.Keys.Where(id => !ignoreSet.Contains(id)).ToList();
            
            foreach (var viewId in viewsToHide)
            {
                Hide(viewId);
            }
        }
        
        #endregion
        
        #region Get UI
        
        public T Get<T>() where T : UIBase
        {
            var viewId = typeof(T).Name;
            return Get(viewId) as T;
        }
        
        public UIBase Get(string viewId)
        {
            return openedViews.TryGetValue(viewId, out var view) ? view : null;
        }
        
        public bool IsOpened<T>() where T : UIBase
        {
            var viewId = typeof(T).Name;
            return IsOpened(viewId);
        }
        
        public bool IsOpened(string viewId)
        {
            return openedViews.ContainsKey(viewId);
        }
        
        #endregion
        
        #region Loading
        
        private UIBase LoadView(string viewId)
        {
            var config = registry?.GetConfig(viewId);
            
            if (config == null)
            {
                Debug.LogError($"[UIManager] No config found for {viewId}");
                return null;
            }
            
            var parent = layerManager.GetLayerRoot(config.Layer);
            GameObject instance = null;
            
            if (config.LoadMode == UILoadMode.Addressable)
            {
                instance = loader.Load(config.AddressablePath, parent);
            }
            else if (config.LoadMode == UILoadMode.Prefab)
            {
                if (loader is PrefabUILoader prefabLoader)
                {
                    instance = prefabLoader.LoadWithPrefab(config.Prefab, parent);
                }
            }
            
            if (instance == null)
                return null;
            
            var view = instance.GetComponent<UIBase>();
            
            if (view == null)
            {
                Debug.LogError($"[UIManager] No UIBase component found on {viewId}");
                Destroy(instance);
                return null;
            }
            
            return view;
        }
        
        #if UNITASK_SUPPORT
        private async Cysharp.Threading.Tasks.UniTask<UIBase> LoadViewAsync(string viewId, CancellationToken cancellationToken = default)
        {
            var config = registry?.GetConfig(viewId);
            
            if (config == null)
            {
                Debug.LogError($"[UIManager] No config found for {viewId}");
                return null;
            }
            
            var parent = layerManager.GetLayerRoot(config.Layer);
            GameObject instance = null;
            
            if (config.LoadMode == UILoadMode.Addressable)
            {
                instance = await loader.LoadAsync(config.AddressablePath, parent, cancellationToken);
            }
            else if (config.LoadMode == UILoadMode.Prefab)
            {
                if (loader is PrefabUILoader prefabLoader)
                {
                    instance = await prefabLoader.LoadWithPrefabAsync(config.Prefab, parent, cancellationToken);
                }
            }
            
            if (instance == null)
                return null;
            
            var view = instance.GetComponent<UIBase>();
            
            if (view == null)
            {
                Debug.LogError($"[UIManager] No UIBase component found on {viewId}");
                Destroy(instance);
                return null;
            }
            
            return view;
        }
        #endif
        
        private UIBase GetFromCacheOrPool(string viewId)
        {
            var config = registry?.GetConfig(viewId);
            
            if (config == null)
                return null;
            
            // Try pool first
            if (config.EnablePooling && cachedViews.TryGetValue(viewId, out var pooledView))
            {
                var parent = layerManager.GetLayerRoot(config.Layer);
                return uiPool.Get(viewId, pooledView, parent);
            }
            
            // Try cache
            if (config.EnableCaching && cachedViews.TryGetValue(viewId, out var cachedView))
            {
                cachedViews.Remove(viewId);
                return cachedView;
            }
            
            return null;
        }
        
        #endregion
        
        #region Preloading
        
        public void Preload(string viewId)
        {
            if (cachedViews.ContainsKey(viewId))
                return;
            
            var view = LoadView(viewId);
            
            if (view != null)
            {
                cachedViews[viewId] = view;
                view.gameObject.SetActive(false);
            }
        }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask PreloadAsync(string viewId, CancellationToken cancellationToken = default)
        {
            if (cachedViews.ContainsKey(viewId))
                return;
            
            var view = await LoadViewAsync(viewId, cancellationToken);
            
            if (view != null)
            {
                cachedViews[viewId] = view;
                view.gameObject.SetActive(false);
            }
        }
        #endif
        
        public void PrewarmPool(string viewId, int count)
        {
            var config = registry?.GetConfig(viewId);
            
            if (config == null || !config.EnablePooling)
                return;
            
            var view = LoadView(viewId);
            
            if (view != null)
            {
                uiPool.Prewarm(viewId, view, count);
            }
        }
        
        #endregion
        
        #region Stack Management
        
        public void ShowPreviousScreen()
        {
            if (screenStack.Count > 1)
            {
                var current = screenStack.Pop();
                Hide(current);
                
                if (screenStack.Count > 0)
                {
                    var previous = screenStack.Peek();
                    Show(previous);
                }
            }
        }
        
        public string GetCurrentScreen()
        {
            return screenStack.Count > 0 ? screenStack.Peek() : null;
        }
        
        #endregion
        
        #region Cleanup
        
        public void ClearCache()
        {
            foreach (var view in cachedViews.Values)
            {
                if (view != null)
                {
                    view.Dispose();
                }
            }
            
            cachedViews.Clear();
        }
        
        public void ClearPool()
        {
            uiPool?.ClearAll();
        }
        
        private void OnDestroy()
        {
            #if UNITASK_SUPPORT
            // Cancel all loading operations
            foreach (var cts in loadingOperations.Values)
            {
                cts?.Cancel();
            }
            loadingOperations.Clear();
            #endif
            
            HideAll();
            ClearCache();
            ClearPool();
            
            instance = null;
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // Handle mobile pause/resume
            if (pauseStatus)
            {
                // Pause logic
            }
            else
            {
                // Resume logic
            }
        }
        
        #endregion
        
        #region Debug
        
        public void LogOpenedViews()
        {
            Debug.Log($"[UIManager] Opened Views: {openedViews.Count}");
            foreach (var kvp in openedViews)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value.State}");
            }
        }
        
        public Dictionary<string, UIState> GetOpenedViewsState()
        {
            var states = new Dictionary<string, UIState>();
            foreach (var kvp in openedViews)
            {
                states[kvp.Key] = kvp.Value.State;
            }
            return states;
        }
        
        #endregion
    }
}
