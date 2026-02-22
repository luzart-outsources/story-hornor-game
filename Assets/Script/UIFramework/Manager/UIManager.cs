using System;
using System.Collections.Generic;
using UnityEngine;
using UIFramework.Core;
using UIFramework.Loading;
using UIFramework.Events;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

namespace UIFramework.Manager
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[UIManager]");
                    _instance = go.AddComponent<UIManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [SerializeField] private UIConfig _config;
        [SerializeField] private Transform _hudLayer;
        [SerializeField] private Transform _screenLayer;
        [SerializeField] private Transform _popupLayer;
        [SerializeField] private Transform _overlayLayer;
        [SerializeField] private Transform _systemLayer;

        private IUILoader _loader;
        private readonly Dictionary<Type, UIBase> _activeUIs = new Dictionary<Type, UIBase>();
        private readonly Stack<UIPopup> _popupStack = new Stack<UIPopup>();
        private readonly Dictionary<Type, UIBase> _cachedUIs = new Dictionary<Type, UIBase>();
        private readonly Dictionary<Type, string> _uiAddressMap = new Dictionary<Type, string>();

#if UNITASK_SUPPORT
        private readonly Dictionary<Type, CancellationTokenSource> _loadingOperations = new Dictionary<Type, CancellationTokenSource>();
#endif

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeLayers();
            InitializeLoader();
            InitializeUIRegistry();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _loader?.ReleaseAll();
#if UNITASK_SUPPORT
                foreach (var cts in _loadingOperations.Values)
                {
                    cts?.Cancel();
                    cts?.Dispose();
                }
                _loadingOperations.Clear();
#endif
                EventBus.Instance.Clear();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                EventBus.Instance.ClearDeadReferences();
            }
        }

        private void InitializeLayers()
        {
            if (_hudLayer == null)
            {
                _hudLayer = new GameObject("HUD_Layer").transform;
                _hudLayer.SetParent(transform);
            }
            if (_screenLayer == null)
            {
                _screenLayer = new GameObject("Screen_Layer").transform;
                _screenLayer.SetParent(transform);
            }
            if (_popupLayer == null)
            {
                _popupLayer = new GameObject("Popup_Layer").transform;
                _popupLayer.SetParent(transform);
            }
            if (_overlayLayer == null)
            {
                _overlayLayer = new GameObject("Overlay_Layer").transform;
                _overlayLayer.SetParent(transform);
            }
            if (_systemLayer == null)
            {
                _systemLayer = new GameObject("System_Layer").transform;
                _systemLayer.SetParent(transform);
            }
        }

        private void InitializeLoader()
        {
#if ADDRESSABLES_SUPPORT
            if (_config != null && _config.UseAddressables)
            {
                _loader = new AddressableUILoader(_config.UseCaching);
            }
            else
            {
                _loader = new PrefabUILoader(_config?.UseCaching ?? true);
            }
#else
            if (_config != null && _config.UseAddressables)
            {
                Debug.LogWarning("[UIManager] Addressables requested but not installed. Falling back to PrefabUILoader.");
            }
            _loader = new PrefabUILoader(_config?.UseCaching ?? true);
#endif
        }

        private void InitializeUIRegistry()
        {
            if (_config == null || _config.UIElements == null)
                return;

            foreach (var element in _config.UIElements)
            {
                if (element.UIType != null && !string.IsNullOrEmpty(element.Address))
                {
                    _uiAddressMap[element.UIType] = element.Address;
                }
            }
        }

        public void SetConfig(UIConfig config)
        {
            _config = config;
            InitializeLoader();
            InitializeUIRegistry();
        }

#if UNITASK_SUPPORT
        public async UniTask<T> ShowAsync<T>(object data = null, IUITransition transition = null, CancellationToken cancellationToken = default) where T : UIBase
        {
            return await ShowAsync(typeof(T), data, transition, cancellationToken) as T;
        }

        public async UniTask<UIBase> ShowAsync(Type uiType, object data = null, IUITransition transition = null, CancellationToken cancellationToken = default)
        {
            if (_loadingOperations.ContainsKey(uiType))
            {
                Debug.LogWarning($"[UIManager] {uiType.Name} is already loading. Waiting...");
                await UniTask.WaitUntil(() => !_loadingOperations.ContainsKey(uiType), cancellationToken: cancellationToken);
                
                if (cancellationToken.IsCancellationRequested)
                    return null;
            }

            if (_activeUIs.TryGetValue(uiType, out var existingUI))
            {
                if (existingUI.State == UIState.Visible || existingUI.State == UIState.Showing)
                {
                    existingUI.Refresh();
                    return existingUI;
                }
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _loadingOperations[uiType] = cts;

            try
            {
                UIBase ui = await LoadUIAsync(uiType, cts.Token);
                if (ui == null || cts.Token.IsCancellationRequested)
                {
                    return null;
                }

                if (transition != null)
                {
                    ui.SetTransition(transition);
                }

                ui.Initialize(data);
                _activeUIs[uiType] = ui;

                if (ui is UIPopup popup)
                {
                    popup.StackOrder = _popupStack.Count;
                    _popupStack.Push(popup);
                    EventBus.Instance.Publish(new UIPopupStackChangedEvent { StackCount = _popupStack.Count });
                }

                await ui.ShowAsync(cts.Token);

                EventBus.Instance.Publish(new UIOpenedEvent { UIType = uiType, Data = data });

                return ui;
            }
            finally
            {
                if (_loadingOperations.ContainsKey(uiType))
                {
                    _loadingOperations[uiType].Dispose();
                    _loadingOperations.Remove(uiType);
                }
            }
        }

        public async UniTask HideAsync<T>(CancellationToken cancellationToken = default) where T : UIBase
        {
            await HideAsync(typeof(T), cancellationToken);
        }

        public async UniTask HideAsync(Type uiType, CancellationToken cancellationToken = default)
        {
            if (_loadingOperations.ContainsKey(uiType))
            {
                _loadingOperations[uiType].Cancel();
            }

            if (!_activeUIs.TryGetValue(uiType, out var ui))
                return;

            if (ui.State == UIState.Hidden || ui.State == UIState.Hiding)
                return;

            await ui.HideAsync(cancellationToken);

            if (ui is UIPopup popup && _popupStack.Count > 0 && _popupStack.Peek() == popup)
            {
                _popupStack.Pop();
                EventBus.Instance.Publish(new UIPopupStackChangedEvent { StackCount = _popupStack.Count });
            }

            _activeUIs.Remove(uiType);

            if (_config != null && !_config.UseCaching)
            {
                ReleaseUI(uiType, ui);
            }
            else
            {
                _cachedUIs[uiType] = ui;
            }

            EventBus.Instance.Publish(new UIClosedEvent { UIType = uiType });
        }

        private async UniTask<UIBase> LoadUIAsync(Type uiType, CancellationToken cancellationToken)
        {
            if (_cachedUIs.TryGetValue(uiType, out var cached))
            {
                _cachedUIs.Remove(uiType);
                return cached;
            }

            if (!_uiAddressMap.TryGetValue(uiType, out string address))
            {
                Debug.LogError($"[UIManager] No address registered for {uiType.Name}");
                return null;
            }

            Transform parent = GetLayerTransform(uiType);
            var ui = await _loader.LoadAsync<UIBase>(address, parent, cancellationToken);

            if (ui != null)
            {
                ui.UIAddress = address;
            }

            return ui;
        }
#else
        public T Show<T>(object data = null, IUITransition transition = null) where T : UIBase
        {
            return Show(typeof(T), data, transition) as T;
        }

        public UIBase Show(Type uiType, object data = null, IUITransition transition = null)
        {
            if (_activeUIs.TryGetValue(uiType, out var existingUI))
            {
                if (existingUI.State == UIState.Visible || existingUI.State == UIState.Showing)
                {
                    existingUI.Refresh();
                    return existingUI;
                }
            }

            UIBase ui = LoadUI(uiType);
            if (ui == null)
                return null;

            if (transition != null)
            {
                ui.SetTransition(transition);
            }

            ui.Initialize(data);
            _activeUIs[uiType] = ui;

            if (ui is UIPopup popup)
            {
                popup.StackOrder = _popupStack.Count;
                _popupStack.Push(popup);
                EventBus.Instance.Publish(new UIPopupStackChangedEvent { StackCount = _popupStack.Count });
            }

            ui.Show();

            EventBus.Instance.Publish(new UIOpenedEvent { UIType = uiType, Data = data });

            return ui;
        }

        public void Hide<T>() where T : UIBase
        {
            Hide(typeof(T));
        }

        public void Hide(Type uiType)
        {
            if (!_activeUIs.TryGetValue(uiType, out var ui))
                return;

            if (ui.State == UIState.Hidden || ui.State == UIState.Hiding)
                return;

            ui.Hide();

            if (ui is UIPopup popup && _popupStack.Count > 0 && _popupStack.Peek() == popup)
            {
                _popupStack.Pop();
                EventBus.Instance.Publish(new UIPopupStackChangedEvent { StackCount = _popupStack.Count });
            }

            _activeUIs.Remove(uiType);

            if (_config != null && !_config.UseCaching)
            {
                ReleaseUI(uiType, ui);
            }
            else
            {
                _cachedUIs[uiType] = ui;
            }

            EventBus.Instance.Publish(new UIClosedEvent { UIType = uiType });
        }

        private UIBase LoadUI(Type uiType)
        {
            if (_cachedUIs.TryGetValue(uiType, out var cached))
            {
                _cachedUIs.Remove(uiType);
                return cached;
            }

            if (!_uiAddressMap.TryGetValue(uiType, out string address))
            {
                Debug.LogError($"[UIManager] No address registered for {uiType.Name}");
                return null;
            }

            Transform parent = GetLayerTransform(uiType);
            var ui = _loader.Load<UIBase>(address, parent);

            if (ui != null)
            {
                ui.UIAddress = address;
            }

            return ui;
        }
#endif

        public T Get<T>() where T : UIBase
        {
            if (_activeUIs.TryGetValue(typeof(T), out var ui))
            {
                return ui as T;
            }
            return null;
        }

        public bool IsOpened<T>() where T : UIBase
        {
            return IsOpened(typeof(T));
        }

        public bool IsOpened(Type uiType)
        {
            return _activeUIs.ContainsKey(uiType) && 
                   (_activeUIs[uiType].State == UIState.Visible || _activeUIs[uiType].State == UIState.Showing);
        }

#if UNITASK_SUPPORT
        public async UniTask HideAllAsync(CancellationToken cancellationToken = default)
        {
            var tasks = new List<UniTask>();
            var uiTypes = new List<Type>(_activeUIs.Keys);

            foreach (var uiType in uiTypes)
            {
                tasks.Add(HideAsync(uiType, cancellationToken));
            }

            await UniTask.WhenAll(tasks);
        }
        
        public async UniTask HideAllIgnoreAsync(Type[] ignoreTypes, CancellationToken cancellationToken = default)
        {
            var tasks = new List<UniTask>();
            var uiTypes = new List<Type>(_activeUIs.Keys);

            foreach (var uiType in uiTypes)
            {
                bool shouldIgnore = false;
                foreach (var ignoreType in ignoreTypes)
                {
                    if (uiType == ignoreType)
                    {
                        shouldIgnore = true;
                        break;
                    }
                }

                if (!shouldIgnore)
                {
                    tasks.Add(HideAsync(uiType, cancellationToken));
                }
            }

            await UniTask.WhenAll(tasks);
        }
#else
        public void HideAll()
        {
            var uiTypes = new List<Type>(_activeUIs.Keys);
            foreach (var uiType in uiTypes)
            {
                Hide(uiType);
            }
        }

        public void HideAllIgnore(params Type[] ignoreTypes)
        {
            var uiTypes = new List<Type>(_activeUIs.Keys);
            foreach (var uiType in uiTypes)
            {
                bool shouldIgnore = false;
                foreach (var ignoreType in ignoreTypes)
                {
                    if (uiType == ignoreType)
                    {
                        shouldIgnore = true;
                        break;
                    }
                }

                if (!shouldIgnore)
                {
                    Hide(uiType);
                }
            }
        }
#endif

        public void HideAllScreensExcept(Type exceptType)
        {
            var uiTypes = new List<Type>(_activeUIs.Keys);
            foreach (var uiType in uiTypes)
            {
                if (uiType != exceptType && _activeUIs[uiType] is UIScreen)
                {
#if UNITASK_SUPPORT
                    HideAsync(uiType).Forget();
#else
                    Hide(uiType);
#endif
                }
            }
        }

        public UIPopup GetTopPopup()
        {
            return _popupStack.Count > 0 ? _popupStack.Peek() : null;
        }

        public int GetPopupStackCount()
        {
            return _popupStack.Count;
        }

        private Transform GetLayerTransform(Type uiType)
        {
            var instance = System.Activator.CreateInstance(uiType) as UIBase;
            if (instance == null)
                return _screenLayer;

            var layer = instance.Layer;
            Destroy(instance);

            return layer switch
            {
                UILayer.HUD => _hudLayer,
                UILayer.Screen => _screenLayer,
                UILayer.Popup => _popupLayer,
                UILayer.Overlay => _overlayLayer,
                UILayer.System => _systemLayer,
                _ => _screenLayer
            };
        }

        private void ReleaseUI(Type uiType, UIBase ui)
        {
            if (ui == null)
                return;

            ui.Dispose();
            
            if (!string.IsNullOrEmpty(ui.UIAddress))
            {
                _loader.Release(ui.UIAddress, ui.gameObject);
            }
            else
            {
                Destroy(ui.gameObject);
            }
        }

        public void RegisterUI(Type uiType, string address)
        {
            _uiAddressMap[uiType] = address;
        }

        public void UnregisterUI(Type uiType)
        {
            _uiAddressMap.Remove(uiType);
        }
    }
}
