using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Luzart.UIFramework
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private UIRegistry registry;
        [SerializeField] private bool useAddressables = false;

        private UILayerManager layerManager;
        private UIStackManager stackManager;
        private UIEventBus eventBus;
        private UIContext context;
        private IUIResourceLoader resourceLoader;

        private readonly Dictionary<string, UIBase> openedUIs = new Dictionary<string, UIBase>();
        private readonly Dictionary<string, UIObjectPool<UIBase>> pools = new Dictionary<string, UIObjectPool<UIBase>>();
        private readonly Dictionary<string, CancellationTokenSource> loadingOperations = new Dictionary<string, CancellationTokenSource>();
        private readonly Dictionary<UITransitionType, IUITransition> transitionCache = new Dictionary<UITransitionType, IUITransition>();

        private static UIManager instance;
        public static UIManager Instance => instance;

        public UIEventBus EventBus => eventBus;
        public UIContext Context => context;

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

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            Cleanup();
        }

        private void Initialize()
        {
            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInChildren<Canvas>();
                if (rootCanvas == null)
                {
                    Debug.LogError("UIManager: No Canvas found!");
                    return;
                }
            }

            if (registry == null)
            {
                Debug.LogError("UIManager: No UIRegistry assigned!");
            }

            layerManager = new UILayerManager(rootCanvas);
            stackManager = new UIStackManager();
            eventBus = new UIEventBus();
            context = new UIContext();

            resourceLoader = useAddressables ? null : new UIPrefabLoader();

            context.RegisterService(eventBus);
            context.RegisterService(this);

            InitializeTransitions();
        }

        private void InitializeTransitions()
        {
            transitionCache[UITransitionType.None] = null;
            transitionCache[UITransitionType.Fade] = new UIFadeTransition();
            transitionCache[UITransitionType.Slide] = new UISlideTransition();
            transitionCache[UITransitionType.Scale] = new UIScaleTransition();
        }

        public async System.Threading.Tasks.Task<T> ShowAsync<T>(object data = null, IUITransition customTransition = null, CancellationToken cancellationToken = default) where T : UIBase
        {
            var viewId = typeof(T).Name;
            return await ShowAsync(viewId, data, customTransition, cancellationToken) as T;
        }

        public async System.Threading.Tasks.Task<UIBase> ShowAsync(string viewId, object data = null, IUITransition customTransition = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(viewId))
            {
                Debug.LogError("UIManager: ViewId is null or empty.");
                return null;
            }

            if (openedUIs.TryGetValue(viewId, out var existingUI))
            {
                if (existingUI.State == UIState.Visible || existingUI.State == UIState.Showing)
                {
                    Debug.LogWarning($"UIManager: UI '{viewId}' is already showing or visible.");
                    return existingUI;
                }
            }

            if (loadingOperations.ContainsKey(viewId))
            {
                Debug.LogWarning($"UIManager: UI '{viewId}' is already being loaded.");
                return null;
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            loadingOperations[viewId] = cts;

            try
            {
                var ui = await LoadUIAsync(viewId, cts.Token);
                
                if (ui == null)
                {
                    Debug.LogError($"UIManager: Failed to load UI '{viewId}'.");
                    return null;
                }

                openedUIs[viewId] = ui;
                layerManager.RegisterUI(ui);

                if (ui.Layer == UILayer.Screen)
                {
                    HideCurrentScreen();
                    stackManager.PushScreen(ui);
                }
                else if (ui.Layer == UILayer.Popup)
                {
                    stackManager.PushPopup(ui);
                }

                var transition = customTransition ?? GetDefaultTransition(viewId);
                if (transition != null)
                {
                    ui.SetTransition(transition);
                }

                ui.Initialize(data);
                ui.Show(cts.Token);

                eventBus.Publish(new UIOpenedEvent(viewId, ui.Layer));

                return ui;
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"UIManager: Loading UI '{viewId}' was cancelled.");
                return null;
            }
            finally
            {
                loadingOperations.Remove(viewId);
                cts?.Dispose();
            }
        }

        public void Hide<T>() where T : UIBase
        {
            var viewId = typeof(T).Name;
            Hide(viewId);
        }

        public void Hide(string viewId)
        {
            if (string.IsNullOrEmpty(viewId))
                return;

            if (loadingOperations.TryGetValue(viewId, out var cts))
            {
                cts.Cancel();
                loadingOperations.Remove(viewId);
            }

            if (!openedUIs.TryGetValue(viewId, out var ui))
                return;

            if (ui.State == UIState.Hidden || ui.State == UIState.Hiding)
                return;

            if (ui.Layer == UILayer.Screen)
            {
                var peeked = stackManager.PeekScreen();
                if (peeked == ui)
                {
                    stackManager.PopScreen();
                }
            }
            else if (ui.Layer == UILayer.Popup)
            {
                var peeked = stackManager.PeekPopup();
                if (peeked == ui)
                {
                    stackManager.PopPopup();
                }
            }

            ui.Hide();
            layerManager.UnregisterUI(ui);
            
            eventBus.Publish(new UIClosedEvent(viewId, ui.Layer));

            ReleaseUI(viewId, ui);
        }

        public void HideAll()
        {
            var uisToHide = openedUIs.Values.ToList();
            
            foreach (var ui in uisToHide)
            {
                Hide(ui.ViewId);
            }

            stackManager.ClearAll();
        }

        public void HideAllIgnore(params string[] ignoreIds)
        {
            var ignoreSet = new HashSet<string>(ignoreIds);
            var uisToHide = openedUIs.Where(kvp => !ignoreSet.Contains(kvp.Key)).Select(kvp => kvp.Value).ToList();
            
            foreach (var ui in uisToHide)
            {
                Hide(ui.ViewId);
            }
        }

        public T Get<T>() where T : UIBase
        {
            var viewId = typeof(T).Name;
            return Get(viewId) as T;
        }

        public UIBase Get(string viewId)
        {
            return openedUIs.TryGetValue(viewId, out var ui) ? ui : null;
        }

        public bool IsOpened<T>() where T : UIBase
        {
            var viewId = typeof(T).Name;
            return IsOpened(viewId);
        }

        public bool IsOpened(string viewId)
        {
            return openedUIs.TryGetValue(viewId, out var ui) && 
                   (ui.State == UIState.Visible || ui.State == UIState.Showing);
        }

        public void GoBack()
        {
            var currentPopup = stackManager.PeekPopup();
            if (currentPopup != null)
            {
                Hide(currentPopup.ViewId);
                return;
            }

            var currentScreen = stackManager.PopScreen();
            if (currentScreen != null)
            {
                Hide(currentScreen.ViewId);
                
                var previousScreen = stackManager.PeekScreen();
                if (previousScreen != null)
                {
                    previousScreen.Show();
                }
            }
        }

        private async System.Threading.Tasks.Task<UIBase> LoadUIAsync(string viewId, CancellationToken cancellationToken)
        {
            var entry = registry?.GetEntry(viewId);
            
            if (entry == null)
            {
                Debug.LogError($"UIManager: No registry entry found for '{viewId}'.");
                return null;
            }

            if (entry.enablePooling && pools.TryGetValue(viewId, out var pool))
            {
                return pool.Get();
            }

            GameObject prefab = null;

            if (entry.loadMode == UILoadMode.Addressable && resourceLoader != null)
            {
                prefab = await resourceLoader.LoadAsync<GameObject>(entry.addressOrPath, cancellationToken);
            }
            else if (entry.loadMode == UILoadMode.Direct)
            {
                prefab = entry.prefab;
            }

            if (prefab == null)
            {
                Debug.LogError($"UIManager: Failed to load prefab for '{viewId}'.");
                return null;
            }

            var layerRoot = layerManager.GetLayerRoot(entry.layer);
            var instance = Instantiate(prefab, layerRoot);
            var ui = instance.GetComponent<UIBase>();

            if (ui == null)
            {
                Debug.LogError($"UIManager: Prefab for '{viewId}' does not have a UIBase component.");
                Destroy(instance);
                return null;
            }

            if (entry.enablePooling)
            {
                var newPool = new UIObjectPool<UIBase>(ui, layerRoot, entry.poolSize, entry.poolSize);
                pools[viewId] = newPool;
            }

            return ui;
        }

        private void HideCurrentScreen()
        {
            var currentScreen = stackManager.PeekScreen();
            if (currentScreen != null && currentScreen.IsVisible)
            {
                currentScreen.Hide();
            }
        }

        private IUITransition GetDefaultTransition(string viewId)
        {
            var entry = registry?.GetEntry(viewId);
            if (entry == null)
                return null;

            return transitionCache.TryGetValue(entry.transitionType, out var transition) ? transition : null;
        }

        private void ReleaseUI(string viewId, UIBase ui)
        {
            if (pools.TryGetValue(viewId, out var pool))
            {
                pool.Release(ui);
            }
            else
            {
                openedUIs.Remove(viewId);
                ui.Dispose();
                
                var entry = registry?.GetEntry(viewId);
                if (entry != null && entry.loadMode == UILoadMode.Addressable)
                {
                    resourceLoader?.Release(entry.addressOrPath);
                }
            }
        }

        private void Cleanup()
        {
            foreach (var cts in loadingOperations.Values)
            {
                cts?.Cancel();
                cts?.Dispose();
            }
            loadingOperations.Clear();

            HideAll();

            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
            pools.Clear();

            resourceLoader?.ReleaseAll();
            eventBus?.Clear();
            context?.Clear();
            layerManager?.Clear();
            stackManager?.ClearAll();

            transitionCache.Clear();
        }

        public void SetCustomTransition(UITransitionType type, IUITransition transition)
        {
            transitionCache[type] = transition;
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Print Active UIs")]
        private void DebugPrintActiveUIs()
        {
            Debug.Log($"=== Active UIs: {openedUIs.Count} ===");
            foreach (var kvp in openedUIs)
            {
                Debug.Log($"  {kvp.Key} - State: {kvp.Value.State}, Layer: {kvp.Value.Layer}");
            }
        }
#endif
    }
}
