namespace Luzart
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using System;
    using UnityEngine.UI;

    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private UIRegistrySO registry;

        /// <summary>
        /// Root layers for UI hierarchy:
        /// 0: Screen (MainMenu, Investigation, etc.)
        /// 1: Popup (Settings, ClueDetail, etc.)
        /// 2: HUD (InvestigationHud)
        /// 3: System (Loading)
        /// 4: Toast
        /// </summary>
        public Transform[] rootOb;
        public UIBase[] listSceneCache;

        public Canvas canvas;
        public GraphicRaycaster graphicRaycaster;

        private List<UIBase> listScreenActive = new List<UIBase>();
        private Dictionary<UIName, UIBase> cacheScreen = new Dictionary<UIName, UIBase>();

        public UIName CurrentName { get; private set; }
        public bool IsAction { get; set; }

        private void Awake()
        {
            canvas ??= GetComponent<Canvas>();
            graphicRaycaster ??= GetComponent<GraphicRaycaster>();

            for (int i = 0; i < listSceneCache.Length; i++)
            {
                if (listSceneCache[i] != null && !cacheScreen.ContainsKey(listSceneCache[i].uiName))
                {
                    cacheScreen.Add(listSceneCache[i].uiName, listSceneCache[i]);
                }
            }

            Observer.Instance.AddObserver(ObserverKey.BlockRaycast, BlockRaycast);
            IsAction = false;
        }

        private void OnDestroy()
        {
            Observer.Instance.RemoveObserver(ObserverKey.BlockRaycast, BlockRaycast);
        }

        public void ShowUI(UIName uiName, Action onHideDone = null)
        {
            ShowUI<UIBase>(uiName, onHideDone);
        }

        public T ShowUI<T>(UIName uiName, Action onHideDone = null) where T : UIBase
        {
            UIBase current = listScreenActive.Find(x => x.uiName == uiName);
            if (!current)
            {
                current = LoadUI(uiName);
                if (current == null)
                {
                    Debug.LogError($"[UIManager] Failed to load UI: {uiName}");
                    return null;
                }
                current.uiName = uiName;
                AddScreenActive(current, true);
            }
            current.transform.SetAsLastSibling();
            current.Show(onHideDone);
            CurrentName = uiName;
            return current as T;
        }

        public void ShowToast(string toast)
        {
            var ui = ShowUI<UIToast>(UIName.Toast);
            if (ui != null) ui.Init(toast);
        }

        private void AddScreenActive(UIBase current, bool isTop)
        {
            var idx = listScreenActive.FindIndex(x => x.uiName == current.uiName);
            if (isTop)
            {
                if (idx >= 0) listScreenActive.RemoveAt(idx);
                listScreenActive.Add(current);
            }
            else
            {
                if (idx < 0) listScreenActive.Add(current);
            }
        }

        private static Action actionRefreshUI;

        public static void AddActionRefreshUI(Action callBack)
        {
            actionRefreshUI += callBack;
        }

        public static void RemoveActionRefreshUI(Action callBack)
        {
            actionRefreshUI -= callBack;
        }

        public void RefreshUI()
        {
            int idx = 0;
            while (listScreenActive.Count > idx)
            {
                listScreenActive[idx].RefreshUI();
                idx++;
            }
            actionRefreshUI?.Invoke();
        }

        public T GetUI<T>(UIName uiName) where T : UIBase
        {
            return LoadUI(uiName) as T;
        }

        public UIBase GetUI(UIName uiName)
        {
            return LoadUI(uiName);
        }

        public UIBase GetUiActive(UIName uiName)
        {
            return listScreenActive.Find(x => x.uiName == uiName);
        }

        public T GetUiActive<T>(UIName uiName) where T : UIBase
        {
            var ui = listScreenActive.Find(x => x.uiName == uiName);
            return ui ? ui as T : default;
        }

        private UIBase LoadUI(UIName uiName)
        {
            // Check cache first
            if (cacheScreen.TryGetValue(uiName, out var cached) && cached != null)
            {
                return cached;
            }

            // Load from registry (SO-based, no Resources.Load)
            var entry = registry.GetEntry(uiName);
            if (entry == null || entry.prefab == null)
            {
                Debug.LogError($"[UIManager] UI not found in registry: {uiName}");
                return null;
            }

            int layerIdx = Mathf.Clamp(entry.layerIndex, 0, rootOb.Length - 1);
            var instance = Instantiate(entry.prefab, rootOb[layerIdx]);
            instance.isCache = entry.useCache;

            if (entry.useCache)
            {
                cacheScreen[uiName] = instance;
            }
            else
            {
                if (!cacheScreen.ContainsKey(uiName))
                    cacheScreen.Add(uiName, instance);
                else
                    cacheScreen[uiName] = instance;
            }

            return instance;
        }

        public void RemoveActiveUI(UIName uiName)
        {
            var idx = listScreenActive.FindIndex(x => x.uiName == uiName);
            if (idx >= 0)
            {
                var ui = listScreenActive[idx];
                listScreenActive.RemoveAt(idx);
                if (!ui.isCache && cacheScreen.ContainsKey(uiName))
                {
                    cacheScreen[uiName] = null;
                }
            }
        }

        public void HideAll()
        {
            while (listScreenActive.Count > 0)
            {
                listScreenActive[0].Hide();
            }
        }

        public void HideAllUiActive()
        {
            while (listScreenActive.Count > 0)
            {
                listScreenActive[0].Hide();
            }
        }

        public void HideAllUiActive(params UIName[] ignoreUI)
        {
            for (int i = listScreenActive.Count - 1; i >= 0; i--)
            {
                bool shouldIgnore = false;
                for (int j = 0; j < ignoreUI.Length; j++)
                {
                    if (listScreenActive[i].uiName == ignoreUI[j])
                    {
                        shouldIgnore = true;
                        break;
                    }
                }
                if (!shouldIgnore)
                {
                    listScreenActive[i].Hide();
                }
            }
        }

        public void HideUiActive(UIName uiName)
        {
            var ui = listScreenActive.Find(x => x.uiName == uiName);
            if (ui) ui.Hide();
        }

        public UIBase GetLastUiActive()
        {
            if (listScreenActive.Count == 0) return null;
            return listScreenActive[listScreenActive.Count - 1];
        }

        private void BlockRaycast(object data = null)
        {
            if (data == null) return;
            bool isBlock = (bool)data;
            graphicRaycaster.enabled = !isBlock;
        }

        public void BlockRaycast(bool isBlock)
        {
            graphicRaycaster.enabled = !isBlock;
        }

        public void ShowLoading()
        {
            ShowUI(UIName.Loading);
        }

        public void HideLoading()
        {
            HideUiActive(UIName.Loading);
        }

        public void ShowToastInternet()
        {
            ShowToast(KeyToast.NoInternetLoadAds);
        }
    }

    public enum UIName
    {
        None = 0,
        Toast = 1,
        Loading = 2,
        // Screens
        MainMenu = 10,
        Cutscene = 11,
        NPCDialogue = 12,
        MapSelection = 13,
        Investigation = 14,
        Notebook = 15,
        // Popups
        Settings = 20,
        Guide = 21,
        ClueDetail = 22,
        LockPuzzle = 23,
        ConfirmExit = 24,
        // HUD
        InvestigationHud = 30,
    }
}
