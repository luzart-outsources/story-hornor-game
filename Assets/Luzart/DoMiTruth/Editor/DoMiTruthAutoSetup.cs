#if UNITY_EDITOR
namespace Luzart.Editor
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.Video;
    using TMPro;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using static Luzart.SelectSwitchGameObject;

    public static class DoMiTruthAutoSetup
    {
        #region Constants
        const string BASE = "Assets/Luzart/DoMiTruth";
        const string DATA = BASE + "/Data";
        const string PREFABS = BASE + "/Prefabs";

        // Reference resolution
        const float REF_W = 1080f;
        const float REF_H = 1920f;

        static readonly string[] DataDirs = {
            DATA + "/Config", DATA + "/Events", DATA + "/Characters",
            DATA + "/Clues", DATA + "/Dialogues", DATA + "/Rooms",
            DATA + "/Maps", DATA + "/Interactables", DATA + "/Locks",
            DATA + "/Actions"
        };
        static readonly string[] PrefabDirs = {
            PREFABS + "/Screens", PREFABS + "/Popups", PREFABS + "/HUD",
            PREFABS + "/Components", PREFABS + "/System"
        };
        #endregion

        #region Public API
        public static void SetupAll()
        {
            EditorUtility.DisplayProgressBar("DoMiTruth Setup", "Creating directories...", 0f);
            EnsureAllDirectories();

            EditorUtility.DisplayProgressBar("DoMiTruth Setup", "Creating core assets...", 0.1f);
            CreateCoreAssets();

            EditorUtility.DisplayProgressBar("DoMiTruth Setup", "Creating prefabs...", 0.3f);
            CreateAllPrefabs();

            EditorUtility.DisplayProgressBar("DoMiTruth Setup", "Creating sample data...", 0.5f);
            CreateSampleData();

            EditorUtility.DisplayProgressBar("DoMiTruth Setup", "Setting up scene...", 0.7f);
            SetupScene();

            EditorUtility.DisplayProgressBar("DoMiTruth Setup", "Wiring references...", 0.9f);
            WireAllReferences();

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[DoMiTruth] Setup All complete!");
            EditorUtility.DisplayDialog("DoMiTruth Setup", "Setup complete!\n\nPress Play to test the game.", "OK");
        }

        public static void CreateCoreAssets()
        {
            EnsureAllDirectories();
            CreateSOIfNotExist<UIRegistrySO>(DATA + "/Config/UIRegistry.asset");
            CreateSOIfNotExist<GameConfigSO>(DATA + "/Config/GameConfig.asset");
            CreateSOIfNotExist<StringEventChannel>(DATA + "/Events/OnClueCollected.asset");
            CreateSOIfNotExist<GameEventChannel>(DATA + "/Events/OnNotebookUpdated.asset");
            CreateSOIfNotExist<GameEventChannel>(DATA + "/Events/OnSettingsChanged.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("[DoMiTruth] Core assets created.");
        }

        public static void CreateAllPrefabs()
        {
            EnsureAllDirectories();

            // Components first (referenced by screens)
            CreateInteractablePrefab();
            CreateMapSelectionItemPrefab();
            CreateNotebookClueItemPrefab();
            CreateNotebookCharacterItemPrefab();

            // System
            CreateToastPrefab();
            CreateLoadingPrefab();

            // Screens
            CreateMainMenuPrefab();
            CreateCutscenePrefab();
            CreateNPCDialoguePrefab();
            CreateMapSelectionPrefab();
            CreateInvestigationPrefab();
            CreateNotebookPrefab();

            // Popups
            CreateSettingsPrefab();
            CreateGuidePrefab();
            CreateClueDetailPrefab();
            CreateLockPuzzlePrefab();
            CreateConfirmExitPrefab();

            // HUD
            CreateInvestigationHudPrefab();

            AssetDatabase.SaveAssets();
            Debug.Log("[DoMiTruth] All prefabs created.");
        }

        public static void CreateSampleData()
        {
            EnsureAllDirectories();
            CreateSampleCharacters();
            CreateSampleClues();
            CreateSampleDialogues();
            CreateSampleLocks();
            CreateSampleActions();
            CreateSampleInteractables();
            CreateSampleRoomsAndMaps();
            AssetDatabase.SaveAssets();
            Debug.Log("[DoMiTruth] Sample data created (9 rooms).");
        }

        public static void SetupScene()
        {
            if (Object.FindFirstObjectByType<UIManager>() != null)
            {
                if (!EditorUtility.DisplayDialog("DoMiTruth", "UIManager already exists. Recreate?", "Yes", "Cancel"))
                    return;
            }
            CreateSceneHierarchy();
            AssetDatabase.SaveAssets();
            Debug.Log("[DoMiTruth] Scene setup complete.");
        }

        public static void WireAllReferences()
        {
            WireUIRegistry();
            WireManagerReferences();
            WirePrefabSOEvents();
            AssetDatabase.SaveAssets();
            Debug.Log("[DoMiTruth] All references wired.");
        }

        public static void CleanPrefabs()
        {
            foreach (var dir in PrefabDirs)
            {
                if (AssetDatabase.IsValidFolder(dir))
                    AssetDatabase.DeleteAsset(dir);
            }
            AssetDatabase.Refresh();
        }

        public static void ValidateSetup()
        {
            int issues = 0;

            if (LoadAsset<UIRegistrySO>(DATA + "/Config/UIRegistry.asset") == null)
            { Debug.LogError("[Validate] UIRegistry.asset missing"); issues++; }
            if (LoadAsset<GameConfigSO>(DATA + "/Config/GameConfig.asset") == null)
            { Debug.LogError("[Validate] GameConfig.asset missing"); issues++; }

            string[] requiredPrefabs = {
                PREFABS + "/Screens/UIMainMenu.prefab",
                PREFABS + "/Screens/UIInvestigation.prefab",
                PREFABS + "/Popups/UILockPuzzle.prefab",
                PREFABS + "/HUD/UIInvestigationHud.prefab",
                PREFABS + "/System/UIToast.prefab",
            };
            foreach (var p in requiredPrefabs)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(p) == null)
                { Debug.LogError($"[Validate] Prefab missing: {p}"); issues++; }
            }

            if (Object.FindFirstObjectByType<UIManager>() == null)
            { Debug.LogError("[Validate] UIManager not in scene"); issues++; }
            if (Object.FindFirstObjectByType<GameFlowController>() == null)
            { Debug.LogError("[Validate] GameFlowController not in scene"); issues++; }
            if (Object.FindFirstObjectByType<Camera>() == null)
            { Debug.LogError("[Validate] No Camera in scene"); issues++; }

            // Check registry entries
            var reg = LoadAsset<UIRegistrySO>(DATA + "/Config/UIRegistry.asset");
            if (reg != null)
            {
                var so = new SerializedObject(reg);
                var entries = so.FindProperty("entries");
                if (entries.arraySize == 0)
                { Debug.LogError("[Validate] UIRegistry has 0 entries"); issues++; }
                else
                    Debug.Log($"[Validate] UIRegistry has {entries.arraySize} entries");
            }

            if (issues == 0)
                Debug.Log("[Validate] All checks passed!");
            else
                Debug.LogWarning($"[Validate] {issues} issue(s) found.");
        }
        #endregion

        #region Scene Setup
        static void CreateSceneHierarchy()
        {
            // --- Destroy existing setup ---
            var existingUIManager = Object.FindFirstObjectByType<UIManager>();
            if (existingUIManager != null)
                Object.DestroyImmediate(existingUIManager.transform.root.gameObject);

            var existingManagers = GameObject.Find("--- Managers ---");
            if (existingManagers != null) Object.DestroyImmediate(existingManagers);

            var existingBootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            if (existingBootstrap != null) Object.DestroyImmediate(existingBootstrap.gameObject);

            // --- Main Camera ---
            if (Object.FindFirstObjectByType<Camera>() == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.orthographic = true;
                cam.orthographicSize = 5;
                camGo.AddComponent<AudioListener>();
            }

            // --- EventSystem ---
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            // --- Canvas ---
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(REF_W, REF_H);
            scaler.matchWidthOrHeight = 0.5f;

            var raycaster = canvasGo.AddComponent<GraphicRaycaster>();

            // --- Layer roots (children of Canvas) ---
            string[] layerNames = { "Layer_Screen", "Layer_Popup", "Layer_HUD", "Layer_System", "Layer_Toast" };
            Transform[] layers = new Transform[layerNames.Length];
            for (int i = 0; i < layerNames.Length; i++)
            {
                var lg = new GameObject(layerNames[i], typeof(RectTransform));
                lg.transform.SetParent(canvasGo.transform, false);
                StretchFill(lg.GetComponent<RectTransform>());
                layers[i] = lg.transform;
            }

            // --- UIManager (on Canvas) ---
            var uiManager = canvasGo.AddComponent<UIManager>();
            uiManager.canvas = canvas;
            uiManager.graphicRaycaster = raycaster;

            var uiSo = new SerializedObject(uiManager);

            // Wire rootOb array
            var rootObProp = uiSo.FindProperty("rootOb");
            rootObProp.arraySize = layers.Length;
            for (int i = 0; i < layers.Length; i++)
                rootObProp.GetArrayElementAtIndex(i).objectReferenceValue = layers[i];

            // Wire registry
            uiSo.FindProperty("registry").objectReferenceValue =
                LoadAsset<UIRegistrySO>(DATA + "/Config/UIRegistry.asset");

            // Wire listSceneCache as empty array
            var cacheProp = uiSo.FindProperty("listSceneCache");
            cacheProp.arraySize = 0;

            uiSo.ApplyModifiedPropertiesWithoutUndo();

            // --- Managers ---
            var managersGo = new GameObject("--- Managers ---");

            CreateManagerInScene<GameDataManager>(managersGo.transform, "GameDataManager", mgr =>
            {
                var s = new SerializedObject(mgr);
                s.FindProperty("onClueCollected").objectReferenceValue =
                    LoadAsset<StringEventChannel>(DATA + "/Events/OnClueCollected.asset");
                s.FindProperty("onNotebookUpdated").objectReferenceValue =
                    LoadAsset<GameEventChannel>(DATA + "/Events/OnNotebookUpdated.asset");
                s.ApplyModifiedPropertiesWithoutUndo();
            });

            CreateManagerInScene<GameFlowController>(managersGo.transform, "GameFlowController", mgr =>
            {
                var s = new SerializedObject(mgr);
                s.FindProperty("gameConfig").objectReferenceValue =
                    LoadAsset<GameConfigSO>(DATA + "/Config/GameConfig.asset");
                s.ApplyModifiedPropertiesWithoutUndo();
            });

            CreateManagerInScene<DialogueManager>(managersGo.transform, "DialogueManager");
            CreateManagerInScene<InvestigationManager>(managersGo.transform, "InvestigationManager");

            CreateManagerInScene<NotebookManager>(managersGo.transform, "NotebookManager", mgr =>
            {
                var s = new SerializedObject(mgr);
                // allClues
                var clueGuids = AssetDatabase.FindAssets("t:ClueSO", new[] { DATA + "/Clues" });
                var allCluesProp = s.FindProperty("allClues");
                allCluesProp.arraySize = clueGuids.Length;
                for (int i = 0; i < clueGuids.Length; i++)
                {
                    allCluesProp.GetArrayElementAtIndex(i).objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<ClueSO>(AssetDatabase.GUIDToAssetPath(clueGuids[i]));
                }
                // allCharacters
                var charGuids = AssetDatabase.FindAssets("t:DialogueCharacterSO", new[] { DATA + "/Characters" });
                var allCharsProp = s.FindProperty("allCharacters");
                allCharsProp.arraySize = charGuids.Length;
                for (int i = 0; i < charGuids.Length; i++)
                {
                    allCharsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<DialogueCharacterSO>(AssetDatabase.GUIDToAssetPath(charGuids[i]));
                }
                s.FindProperty("onClueCollected").objectReferenceValue =
                    LoadAsset<StringEventChannel>(DATA + "/Events/OnClueCollected.asset");
                s.ApplyModifiedPropertiesWithoutUndo();
            });

            // --- Bootstrap ---
            var bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.AddComponent<GameBootstrap>();

            EditorUtility.SetDirty(canvasGo);
        }

        static T CreateManagerInScene<T>(Transform parent, string name, System.Action<T> configure = null) where T : MonoBehaviour
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var comp = go.AddComponent<T>();
            configure?.Invoke(comp);
            return comp;
        }
        #endregion

        #region Helpers - UI Building
        static GameObject CreateUIRoot(string name, Vector2 size, bool stretch)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            if (stretch)
            {
                StretchFill(rt);
            }
            else
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = size;
                rt.anchoredPosition = Vector2.zero;
            }
            return go;
        }

        static void StretchFill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// Set anchors relative to parent and zero out offsets
        /// </summary>
        static void SetAnchors(RectTransform rt, float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Set anchor to a single point + explicit size
        /// </summary>
        static void SetAnchorPoint(RectTransform rt, float anchorX, float anchorY, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = new Vector2(anchorX, anchorY);
            rt.anchorMax = new Vector2(anchorX, anchorY);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
        }

        static (GameObject go, Button btn, TMP_Text text) CreateButton(Transform parent, string name, string label, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            StretchFill(textGo.GetComponent<RectTransform>());

            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 20;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            return (go, go.GetComponent<Button>(), text);
        }

        /// <summary>
        /// Create button inside a LayoutGroup with proper LayoutElement height
        /// </summary>
        static (GameObject go, Button btn, TMP_Text text) CreateLayoutButton(Transform parent, string name, string label, float height)
        {
            var result = CreateButton(parent, name, label, new Vector2(0, height));
            var le = result.go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.flexibleWidth = 1;
            result.go.AddComponent<EffectButton>();
            return result;
        }

        static (GameObject go, TMP_Text text) CreateText(Transform parent, string name, string defaultText, int fontSize = 24)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = defaultText;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            return (go, text);
        }

        static (GameObject go, Image img) CreateImage(Transform parent, string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = size;
            go.GetComponent<Image>().color = Color.gray;
            return (go, go.GetComponent<Image>());
        }

        static Button CreateCloseButton(Transform parent)
        {
            var (go, btn, text) = CreateButton(parent, "BtnClose", "X", new Vector2(50, 50));
            go.AddComponent<EffectButton>();
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-10, -10);
            go.GetComponent<Image>().color = new Color(0.5f, 0.1f, 0.1f);
            return btn;
        }

        static GameObject CreatePopupPanel(Transform parent, string name, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f);

            var vl = go.GetComponent<VerticalLayoutGroup>();
            vl.spacing = 10;
            vl.padding = new RectOffset(20, 20, 20, 20);
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;

            return go;
        }

        static GameObject CreateScrollView(Transform parent, string name, bool stretch = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(ScrollRect));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            if (stretch) StretchFill(rt);

            // Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(go.transform, false);
            StretchFill(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            // Content
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var crt = content.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.offsetMin = new Vector2(0, 0);
            crt.offsetMax = new Vector2(0, 0);
            crt.sizeDelta = new Vector2(0, 0);

            var scrollRect = go.GetComponent<ScrollRect>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = crt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            return go;
        }

        static Slider CreateSlider(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredHeight = 30;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            StretchFill(bg.GetComponent<RectTransform>());
            bg.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fart = fillArea.GetComponent<RectTransform>();
            fart.anchorMin = new Vector2(0, 0.25f);
            fart.anchorMax = new Vector2(1, 0.75f);
            fart.offsetMin = Vector2.zero;
            fart.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            StretchFill(fill.GetComponent<RectTransform>());
            fill.GetComponent<Image>().color = new Color(0.3f, 0.6f, 1f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            StretchFill(handleArea.GetComponent<RectTransform>());

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 0);
            handle.GetComponent<Image>().color = Color.white;

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.value = 1f;

            return slider;
        }

        static void AddShowAnimation(GameObject root)
        {
            var tweenAnim = root.AddComponent<TweenAnimation>();
            var caller = root.AddComponent<TweenAnimationCaller>();

            // Configure caller
            var callerSo = new SerializedObject(caller);
            callerSo.FindProperty("tweenAnimation").objectReferenceValue = tweenAnim;
            callerSo.FindProperty("typeShow").enumValueIndex = (int)ETypeShow.None; // None=0, index 0
            callerSo.ApplyModifiedPropertiesWithoutUndo();

            // Configure TweenAnimation for Scale
            var animSo = new SerializedObject(tweenAnim);
            // EAnimation.Scale = 5, but enumValueIndex is declaration order
            // Move=0,MoveLocal=1,MoveAnchors=2,Float=3,Euler=4,Scale=5 → index 5
            animSo.FindProperty("typeAnimation").enumValueIndex = 5;

            var settingsProp = animSo.FindProperty("tweenAnimationSettings");
            settingsProp.FindPropertyRelative("General.Target").objectReferenceValue = root;
            settingsProp.FindPropertyRelative("General.Duration").floatValue = 0.25f;
            // Ease.OutBack = 27
            settingsProp.FindPropertyRelative("General.Easing").intValue = 27;

            settingsProp.FindPropertyRelative("Values.Vector3From").vector3Value = new Vector3(0.85f, 0.85f, 1f);
            settingsProp.FindPropertyRelative("Values.Vector3To").vector3Value = Vector3.one;
            animSo.ApplyModifiedPropertiesWithoutUndo();

            // Wire caller to UIBase.showAnimation
            var uiBase = root.GetComponent<UIBase>();
            if (uiBase != null)
            {
                var uiSo = new SerializedObject(uiBase);
                uiSo.FindProperty("showAnimation").objectReferenceValue = caller;
                uiSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        #endregion

        #region Helpers - Asset Management
        static void EnsureAllDirectories()
        {
            foreach (var d in DataDirs) EnsureDirectory(d);
            foreach (var d in PrefabDirs) EnsureDirectory(d);
        }

        static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }

        static T CreateSOIfNotExist<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, path);
            return so;
        }

        static T LoadAsset<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        static bool Exists(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Object>(path) != null;
        }

        static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        /// <summary>
        /// Wire a serialized field by name using SerializedObject
        /// </summary>
        static void WireField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[Wire] Field '{fieldName}' not found on {target.GetType().Name}");
            }
        }

        /// <summary>
        /// Get the correct enumValueIndex for a UIName value.
        /// enumValueIndex = declaration order index, NOT the raw int value.
        /// </summary>
        static int GetUINameEnumIndex(UIName name)
        {
            // UIName enum declaration order:
            // 0: None=0, 1: Toast=1, 2: Loading=2,
            // 3: MainMenu=10, 4: Cutscene=11, 5: NPCDialogue=12,
            // 6: MapSelection=13, 7: Investigation=14, 8: Notebook=15,
            // 9: Settings=20, 10: Guide=21, 11: ClueDetail=22,
            // 12: LockPuzzle=23, 13: ConfirmExit=24, 14: InvestigationHud=30
            switch (name)
            {
                case UIName.None: return 0;
                case UIName.Toast: return 1;
                case UIName.Loading: return 2;
                case UIName.MainMenu: return 3;
                case UIName.Cutscene: return 4;
                case UIName.NPCDialogue: return 5;
                case UIName.MapSelection: return 6;
                case UIName.Investigation: return 7;
                case UIName.Notebook: return 8;
                case UIName.Settings: return 9;
                case UIName.Guide: return 10;
                case UIName.ClueDetail: return 11;
                case UIName.LockPuzzle: return 12;
                case UIName.ConfirmExit: return 13;
                case UIName.InvestigationHud: return 14;
                default: return 0;
            }
        }
        #endregion

        #region Prefabs - Components
        static void CreateInteractablePrefab()
        {
            string path = PREFABS + "/Components/InteractableObject.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("InteractableObject", new Vector2(100, 100), false);
            root.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            var comp = root.AddComponent<InteractableObject>();

            var highlight = CreateImage(root.transform, "Highlight", new Vector2(100, 100));
            highlight.img.color = new Color(1, 1, 0, 0.3f);
            highlight.go.SetActive(false);

            WireField(comp, "imgHighlight", highlight.img);
            WireField(comp, "rectTransform", root.GetComponent<RectTransform>());

            SavePrefab(root, path);
        }

        static void CreateMapSelectionItemPrefab()
        {
            string path = PREFABS + "/Components/MapSelectionItem.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("MapSelectionItem", new Vector2(300, 220), false);
            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            root.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var comp = root.AddComponent<MapSelectionItem>();

            var thumb = CreateImage(root.transform, "ImgThumbnail", new Vector2(280, 130));
            thumb.go.AddComponent<LayoutElement>().preferredHeight = 130;

            var txtName = CreateText(root.transform, "TxtName", "Map Name", 20);
            txtName.go.AddComponent<LayoutElement>().preferredHeight = 30;

            var btn = CreateLayoutButton(root.transform, "BtnSelect", "SELECT", 40);

            WireField(comp, "imgThumbnail", thumb.img);
            WireField(comp, "txtName", txtName.text);
            WireField(comp, "btnSelect", btn.btn);

            SavePrefab(root, path);
        }

        static void CreateNotebookClueItemPrefab()
        {
            string path = PREFABS + "/Components/NotebookClueItem.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("NotebookClueItem", new Vector2(0, 80), false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);

            var hl = root.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 10;
            hl.padding = new RectOffset(10, 10, 5, 5);
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = true;
            hl.childAlignment = TextAnchor.MiddleLeft;
            root.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f, 1f);

            var comp = root.AddComponent<NotebookClueItem>();

            var img = CreateImage(root.transform, "ImgClue", new Vector2(60, 60));
            img.go.AddComponent<LayoutElement>().preferredWidth = 60;

            var txtN = CreateText(root.transform, "TxtName", "Clue", 18);
            txtN.text.alignment = TextAlignmentOptions.Left;
            txtN.go.AddComponent<LayoutElement>().flexibleWidth = 1;

            var txtC = CreateText(root.transform, "TxtCategory", "Physical", 14);
            txtC.text.color = Color.gray;
            txtC.go.AddComponent<LayoutElement>().preferredWidth = 80;

            var btnD = CreateButton(root.transform, "BtnDetail", "?", new Vector2(40, 40));
            btnD.go.AddComponent<LayoutElement>().preferredWidth = 40;
            btnD.go.AddComponent<EffectButton>();

            WireField(comp, "imgClue", img.img);
            WireField(comp, "txtName", txtN.text);
            WireField(comp, "txtCategory", txtC.text);
            WireField(comp, "btnDetail", btnD.btn);

            SavePrefab(root, path);
        }

        static void CreateNotebookCharacterItemPrefab()
        {
            string path = PREFABS + "/Components/NotebookCharacterItem.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("NotebookCharacterItem", new Vector2(0, 80), false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);

            var hl = root.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 10;
            hl.padding = new RectOffset(10, 10, 5, 5);
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = true;
            hl.childAlignment = TextAnchor.MiddleLeft;
            root.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f, 1f);

            var comp = root.AddComponent<NotebookCharacterItem>();

            var img = CreateImage(root.transform, "ImgPortrait", new Vector2(60, 60));
            img.go.AddComponent<LayoutElement>().preferredWidth = 60;

            var txtN = CreateText(root.transform, "TxtName", "Character", 20);
            txtN.text.alignment = TextAlignmentOptions.Left;
            txtN.go.AddComponent<LayoutElement>().flexibleWidth = 1;

            WireField(comp, "imgPortrait", img.img);
            WireField(comp, "txtName", txtN.text);

            SavePrefab(root, path);
        }
        #endregion

        #region Prefabs - System
        static void CreateToastPrefab()
        {
            string path = PREFABS + "/System/UIToast.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UIToast", Vector2.zero, true);
            root.AddComponent<CanvasGroup>();
            var toast = root.AddComponent<UIToast>();
            toast.uiName = UIName.Toast;
            toast.isCache = true;

            // Toast background - bottom center
            var bg = CreateImage(root.transform, "Bg", new Vector2(600, 70));
            SetAnchorPoint(bg.go.GetComponent<RectTransform>(), 0.5f, 0.15f, new Vector2(600, 70), Vector2.zero);
            bg.img.color = new Color(0, 0, 0, 0.85f);

            var txt = CreateText(bg.go.transform, "TxtNoti", "Toast Message", 20);
            StretchFill(txt.go.GetComponent<RectTransform>());

            // UIToast has public fields, use SerializedObject to be safe for prefab
            var so = new SerializedObject(toast);
            so.FindProperty("canvasGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
            so.FindProperty("txtNoti").objectReferenceValue = txt.text;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, path);
        }

        static void CreateLoadingPrefab()
        {
            string path = PREFABS + "/System/UILoading.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UILoading", Vector2.zero, true);
            root.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
            var uiBase = root.AddComponent<UIBase>();
            uiBase.uiName = UIName.Loading;
            uiBase.isCache = true;

            var txt = CreateText(root.transform, "TxtLoading", "Loading...", 32);
            SetAnchorPoint(txt.go.GetComponent<RectTransform>(), 0.5f, 0.5f, new Vector2(400, 60), Vector2.zero);

            SavePrefab(root, path);
        }
        #endregion

        #region Prefabs - Screens
        static void CreateMainMenuPrefab()
        {
            string path = PREFABS + "/Screens/UIMainMenu.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UIMainMenu", Vector2.zero, true);
            root.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 1f);
            var comp = root.AddComponent<UIMainMenu>();
            comp.uiName = UIName.MainMenu;
            comp.isCache = true;

            // Content container - center area
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(root.transform, false);
            var crt = content.GetComponent<RectTransform>();
            SetAnchors(crt, 0.15f, 0.25f, 0.85f, 0.75f);

            var vl = content.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 20;
            vl.childAlignment = TextAnchor.MiddleCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;

            // Title
            var title = CreateText(content.transform, "Title", "DO-MI TRUTH", 48);
            title.text.fontStyle = FontStyles.Bold;
            title.go.AddComponent<LayoutElement>().preferredHeight = 80;

            // Spacer
            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(content.transform, false);
            spacer.AddComponent<LayoutElement>().preferredHeight = 30;

            // Buttons
            var btnPlay = CreateLayoutButton(content.transform, "BtnPlay", "NEW GAME", 60);
            var btnCont = CreateLayoutButton(content.transform, "BtnContinue", "CONTINUE", 60);
            var btnSet = CreateLayoutButton(content.transform, "BtnSettings", "SETTINGS", 60);
            var btnGuide = CreateLayoutButton(content.transform, "BtnGuide", "GUIDE", 60);
            var btnExit = CreateLayoutButton(content.transform, "BtnExit", "EXIT", 60);

            // Continue toggle (visibility control)
            var contToggle = btnCont.go.AddComponent<SelectToggleGameObject>();
            contToggle.obSelect = new[] { btnCont.go };
            contToggle.obUnSelect = new GameObject[0];

            AddShowAnimation(root);

            WireField(comp, "btnPlay", btnPlay.btn);
            WireField(comp, "btnContinue", btnCont.btn);
            WireField(comp, "btnSettings", btnSet.btn);
            WireField(comp, "btnGuide", btnGuide.btn);
            WireField(comp, "btnExit", btnExit.btn);
            WireField(comp, "continueToggle", contToggle);

            SavePrefab(root, path);
        }

        static void CreateCutscenePrefab()
        {
            string path = PREFABS + "/Screens/UICutscene.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UICutscene", Vector2.zero, true);
            root.AddComponent<Image>().color = Color.black;
            var comp = root.AddComponent<UICutscene>();
            comp.uiName = UIName.Cutscene;

            // VideoPlayer component on root
            var videoPlayer = root.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;

            // RawImage for video display
            var rawImgGo = new GameObject("VideoDisplay", typeof(RectTransform), typeof(RawImage));
            rawImgGo.transform.SetParent(root.transform, false);
            StretchFill(rawImgGo.GetComponent<RectTransform>());

            // Skip button - bottom right
            var btnSkip = CreateButton(root.transform, "BtnSkip", "SKIP >>", new Vector2(150, 50));
            var srt = btnSkip.go.GetComponent<RectTransform>();
            SetAnchorPoint(srt, 1f, 0f, new Vector2(150, 50), new Vector2(-90, 40));
            btnSkip.go.AddComponent<EffectButton>();

            var skipToggle = btnSkip.go.AddComponent<SelectToggleGameObject>();
            skipToggle.obSelect = new[] { btnSkip.go };
            skipToggle.obUnSelect = new GameObject[0];

            WireField(comp, "videoPlayer", videoPlayer);
            WireField(comp, "videoDisplay", rawImgGo.GetComponent<RawImage>());
            WireField(comp, "btnSkip", btnSkip.btn);
            WireField(comp, "skipToggle", skipToggle);

            SavePrefab(root, path);
        }

        static void CreateNPCDialoguePrefab()
        {
            string path = PREFABS + "/Screens/UINPCDialogue.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UINPCDialogue", Vector2.zero, true);
            var comp = root.AddComponent<UINPCDialogue>();
            comp.uiName = UIName.NPCDialogue;
            comp.isCache = true;

            // Semi-transparent overlay (click to advance)
            root.AddComponent<Image>().color = new Color(0, 0, 0, 0.3f);

            // Dialogue panel at bottom 35% of screen
            var panel = new GameObject("DialoguePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var prt = panel.GetComponent<RectTransform>();
            SetAnchors(prt, 0f, 0f, 1f, 0.35f);
            panel.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.92f);

            // Portrait - left side
            var portrait = CreateImage(panel.transform, "ImgPortrait", new Vector2(130, 130));
            var prrt = portrait.go.GetComponent<RectTransform>();
            SetAnchorPoint(prrt, 0f, 0.5f, new Vector2(130, 130), new Vector2(80, 0));

            // Character Name - top area
            var txtName = CreateText(panel.transform, "TxtName", "Character Name", 24);
            var nrt = txtName.go.GetComponent<RectTransform>();
            SetAnchors(nrt, 0.17f, 0.72f, 0.95f, 0.95f);
            txtName.text.alignment = TextAlignmentOptions.Left;
            txtName.text.fontStyle = FontStyles.Bold;

            // Dialogue text - middle area
            var txtDlg = CreateText(panel.transform, "TxtDialogue", "Dialogue text goes here...", 20);
            var drt = txtDlg.go.GetComponent<RectTransform>();
            SetAnchors(drt, 0.17f, 0.08f, 0.88f, 0.68f);
            txtDlg.text.alignment = TextAlignmentOptions.TopLeft;

            // Next button - bottom right of panel
            var btnNext = CreateButton(panel.transform, "BtnNext", ">>>", new Vector2(80, 45));
            var bnrt = btnNext.go.GetComponent<RectTransform>();
            SetAnchorPoint(bnrt, 1f, 0f, new Vector2(80, 45), new Vector2(-55, 30));
            btnNext.go.AddComponent<EffectButton>();

            AddShowAnimation(root);

            WireField(comp, "imgPortrait", portrait.img);
            WireField(comp, "txtName", txtName.text);
            WireField(comp, "txtDialogue", txtDlg.text);
            WireField(comp, "btnNext", btnNext.btn);

            SavePrefab(root, path);
        }

        static void CreateMapSelectionPrefab()
        {
            string path = PREFABS + "/Screens/UIMapSelection.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UIMapSelection", Vector2.zero, true);
            root.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 1f);
            var comp = root.AddComponent<UIMapSelection>();
            comp.uiName = UIName.MapSelection;

            // Title - top
            var title = CreateText(root.transform, "Title", "SELECT LOCATION", 32);
            var trt = title.go.GetComponent<RectTransform>();
            SetAnchors(trt, 0.1f, 0.88f, 0.9f, 0.96f);
            title.text.fontStyle = FontStyles.Bold;

            // Scroll view with grid
            var scrollGo = CreateScrollView(root.transform, "MapGrid");
            var srt = scrollGo.GetComponent<RectTransform>();
            SetAnchors(srt, 0.05f, 0.08f, 0.95f, 0.85f);

            var gridContent = scrollGo.transform.Find("Viewport/Content");
            var gl = gridContent.gameObject.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(300, 220);
            gl.spacing = new Vector2(20, 20);
            gl.padding = new RectOffset(20, 20, 20, 20);
            gl.childAlignment = TextAnchor.UpperCenter;
            var csf = gridContent.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Close button
            var btnClose = CreateCloseButton(root.transform);
            comp.closeBtn = btnClose;

            AddShowAnimation(root);

            var itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Components/MapSelectionItem.prefab");
            WireField(comp, "gridContainer", gridContent);
            if (itemPrefab != null)
                WireField(comp, "itemPrefab", itemPrefab.GetComponent<MapSelectionItem>());

            SavePrefab(root, path);
        }

        static void CreateInvestigationPrefab()
        {
            string path = PREFABS + "/Screens/UIInvestigation.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UIInvestigation", Vector2.zero, true);
            var comp = root.AddComponent<UIInvestigation>();
            comp.uiName = UIName.Investigation;

            // Background container (larger than screen for panning)
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(root.transform, false);
            var bgrt = bgGo.GetComponent<RectTransform>();
            bgrt.anchorMin = new Vector2(0.5f, 0.5f);
            bgrt.anchorMax = new Vector2(0.5f, 0.5f);
            bgrt.pivot = new Vector2(0.5f, 0.5f);
            bgrt.sizeDelta = new Vector2(2560, 1440);
            bgrt.anchoredPosition = Vector2.zero;
            bgGo.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f);

            // Interactable container (child of background, moves with pan)
            var interContainer = new GameObject("InteractableContainer", typeof(RectTransform));
            interContainer.transform.SetParent(bgGo.transform, false);
            StretchFill(interContainer.GetComponent<RectTransform>());

            // Camera pan controller
            var panCtrl = root.AddComponent<CameraPanController>();

            var interPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Components/InteractableObject.prefab");

            WireField(comp, "imgBackground", bgGo.GetComponent<Image>());
            WireField(comp, "backgroundRect", bgrt);
            WireField(comp, "interactableContainer", interContainer.transform);
            WireField(comp, "cameraPan", panCtrl);
            if (interPrefab != null)
                WireField(comp, "interactablePrefab", interPrefab.GetComponent<InteractableObject>());

            WireField(panCtrl, "backgroundRect", bgrt);
            WireField(panCtrl, "viewportRect", root.GetComponent<RectTransform>());

            SavePrefab(root, path);
        }

        static void CreateNotebookPrefab()
        {
            string path = PREFABS + "/Screens/UINotebook.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UINotebook", Vector2.zero, true);
            root.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
            var comp = root.AddComponent<UINotebook>();
            comp.uiName = UIName.Notebook;

            // Tab bar - top area
            var tabBar = new GameObject("TabBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabBar.transform.SetParent(root.transform, false);
            var tbrt = tabBar.GetComponent<RectTransform>();
            SetAnchors(tbrt, 0.05f, 0.90f, 0.95f, 0.97f);
            var tbl = tabBar.GetComponent<HorizontalLayoutGroup>();
            tbl.spacing = 10;
            tbl.childForceExpandWidth = true;
            tbl.childForceExpandHeight = true;

            var btnClues = CreateButton(tabBar.transform, "BtnCluesTab", "CLUES", Vector2.zero);
            btnClues.go.AddComponent<EffectButton>();
            var btnChars = CreateButton(tabBar.transform, "BtnCharactersTab", "CHARACTERS", Vector2.zero);
            btnChars.go.AddComponent<EffectButton>();

            // Tab toggle images for visual active state
            var clueTabToggle = btnClues.go.AddComponent<SelectToggleImage>();
            var charTabToggle = btnChars.go.AddComponent<SelectToggleImage>();

            // Tab content area
            var tabContent = new GameObject("TabContent", typeof(RectTransform));
            tabContent.transform.SetParent(root.transform, false);
            var tcrt = tabContent.GetComponent<RectTransform>();
            SetAnchors(tcrt, 0.03f, 0.03f, 0.97f, 0.88f);

            // Clue list (tab 0) - scrollview stretch
            var clueScroll = CreateScrollView(tabContent.transform, "ClueList", true);
            var clueContent = clueScroll.transform.Find("Viewport/Content");
            var cvl = clueContent.gameObject.AddComponent<VerticalLayoutGroup>();
            cvl.spacing = 5;
            cvl.childForceExpandWidth = true;
            cvl.childForceExpandHeight = false;
            cvl.padding = new RectOffset(5, 5, 5, 5);
            clueContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Character list (tab 1)
            var charScroll = CreateScrollView(tabContent.transform, "CharacterList", true);
            var charContent = charScroll.transform.Find("Viewport/Content");
            var chvl = charContent.gameObject.AddComponent<VerticalLayoutGroup>();
            chvl.spacing = 5;
            chvl.childForceExpandWidth = true;
            chvl.childForceExpandHeight = false;
            chvl.padding = new RectOffset(5, 5, 5, 5);
            charContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            charScroll.SetActive(false);

            // SelectSwitch for tab switching
            var tabSwitch = tabContent.AddComponent<SelectSwitchGameObject>();
            tabSwitch.obSelects = new GroupGameObject[]
            {
                new GroupGameObject { obGroups = new[] { clueScroll } },
                new GroupGameObject { obGroups = new[] { charScroll } }
            };

            // Close button
            var btnClose = CreateCloseButton(root.transform);
            comp.closeBtn = btnClose;

            AddShowAnimation(root);

            var cluePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Components/NotebookClueItem.prefab");
            var charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Components/NotebookCharacterItem.prefab");

            WireField(comp, "btnCluesTab", btnClues.btn);
            WireField(comp, "btnCharactersTab", btnChars.btn);
            WireField(comp, "tabSwitch", tabSwitch);
            WireField(comp, "clueTabToggle", clueTabToggle);
            WireField(comp, "characterTabToggle", charTabToggle);
            WireField(comp, "clueListContainer", clueContent);
            WireField(comp, "characterListContainer", charContent);
            if (cluePrefab != null)
                WireField(comp, "clueItemPrefab", cluePrefab.GetComponent<NotebookClueItem>());
            if (charPrefab != null)
                WireField(comp, "characterItemPrefab", charPrefab.GetComponent<NotebookCharacterItem>());

            SavePrefab(root, path);
        }
        #endregion

        #region Prefabs - Popups
        static void CreateSettingsPrefab()
        {
            string path = PREFABS + "/Popups/UISettings.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UISettings", Vector2.zero, true);
            root.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
            var comp = root.AddComponent<UISettings>();
            comp.uiName = UIName.Settings;

            var panel = CreatePopupPanel(root.transform, "Panel", 500, 400);

            var title = CreateText(panel.transform, "Title", "SETTINGS", 30);
            title.text.fontStyle = FontStyles.Bold;
            title.go.AddComponent<LayoutElement>().preferredHeight = 50;

            // Music
            var musicLabel = CreateText(panel.transform, "MusicLabel", "Music Volume", 18);
            musicLabel.text.alignment = TextAlignmentOptions.Left;
            musicLabel.go.AddComponent<LayoutElement>().preferredHeight = 30;
            var sliderMusic = CreateSlider(panel.transform, "SliderMusic");

            // SFX
            var sfxLabel = CreateText(panel.transform, "SfxLabel", "SFX Volume", 18);
            sfxLabel.text.alignment = TextAlignmentOptions.Left;
            sfxLabel.go.AddComponent<LayoutElement>().preferredHeight = 30;
            var sliderSfx = CreateSlider(panel.transform, "SliderSfx");

            var btnClose = CreateCloseButton(root.transform);
            comp.closeBtn = btnClose;

            AddShowAnimation(root);

            WireField(comp, "sliderMusic", sliderMusic);
            WireField(comp, "sliderSfx", sliderSfx);

            SavePrefab(root, path);
        }

        static void CreateGuidePrefab()
        {
            string path = PREFABS + "/Popups/UIGuide.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UIGuide", Vector2.zero, true);
            root.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
            var comp = root.AddComponent<UIGuide>();
            comp.uiName = UIName.Guide;

            var panel = CreatePopupPanel(root.transform, "Panel", 600, 700);

            var title = CreateText(panel.transform, "Title", "HOW TO PLAY", 30);
            title.text.fontStyle = FontStyles.Bold;
            title.go.AddComponent<LayoutElement>().preferredHeight = 50;

            var guideText = CreateText(panel.transform, "GuideText",
                "- Tap objects to investigate\n- Collect clues to solve the mystery\n- Check your Notebook for collected evidence\n- Solve puzzles to unlock items",
                18);
            guideText.text.alignment = TextAlignmentOptions.TopLeft;
            guideText.go.AddComponent<LayoutElement>().flexibleHeight = 1;

            var btnClose = CreateCloseButton(root.transform);
            comp.closeBtn = btnClose;

            AddShowAnimation(root);

            SavePrefab(root, path);
        }

        static void CreateClueDetailPrefab()
        {
            string path = PREFABS + "/Popups/UIClueDetail.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UIClueDetail", Vector2.zero, true);
            root.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
            var comp = root.AddComponent<UIClueDetail>();
            comp.uiName = UIName.ClueDetail;

            var panel = CreatePopupPanel(root.transform, "Panel", 500, 550);

            var imgClue = CreateImage(panel.transform, "ImgClue", new Vector2(220, 220));
            imgClue.go.AddComponent<LayoutElement>().preferredHeight = 220;

            var txtName = CreateText(panel.transform, "TxtClueName", "Clue Name", 26);
            txtName.text.fontStyle = FontStyles.Bold;
            txtName.go.AddComponent<LayoutElement>().preferredHeight = 35;

            var txtCat = CreateText(panel.transform, "TxtCategory", "Physical", 16);
            txtCat.text.color = Color.gray;
            txtCat.go.AddComponent<LayoutElement>().preferredHeight = 25;

            var txtDesc = CreateText(panel.transform, "TxtDescription", "Description...", 18);
            txtDesc.text.alignment = TextAlignmentOptions.TopLeft;
            txtDesc.go.AddComponent<LayoutElement>().flexibleHeight = 1;

            // NotebookTarget - an empty transform for fly-to animation target
            var notebookTarget = new GameObject("NotebookTarget", typeof(RectTransform));
            notebookTarget.transform.SetParent(root.transform, false);
            SetAnchorPoint(notebookTarget.GetComponent<RectTransform>(), 1f, 1f, new Vector2(30, 30), new Vector2(-60, -60));

            var btnClose = CreateCloseButton(root.transform);
            comp.closeBtn = btnClose;

            AddShowAnimation(root);

            WireField(comp, "imgClue", imgClue.img);
            WireField(comp, "txtClueName", txtName.text);
            WireField(comp, "txtCategory", txtCat.text);
            WireField(comp, "txtDescription", txtDesc.text);
            WireField(comp, "notebookTarget", notebookTarget.transform);

            SavePrefab(root, path);
        }

        static void CreateLockPuzzlePrefab()
        {
            string path = PREFABS + "/Popups/UILockPuzzle.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UILockPuzzle", Vector2.zero, true);
            root.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
            var comp = root.AddComponent<UILockPuzzle>();
            comp.uiName = UIName.LockPuzzle;

            var panel = CreatePopupPanel(root.transform, "Panel", 500, 400);

            var title = CreateText(panel.transform, "Title", "ENTER PASSCODE", 26);
            title.text.fontStyle = FontStyles.Bold;
            title.go.AddComponent<LayoutElement>().preferredHeight = 40;

            var txtHint = CreateText(panel.transform, "TxtHint", "Hint: ...", 16);
            txtHint.text.color = Color.gray;
            txtHint.go.AddComponent<LayoutElement>().preferredHeight = 30;

            // Passcode panel (tab 0 of panelSwitch)
            var passcodePanel = new GameObject("PasscodePanel", typeof(RectTransform));
            passcodePanel.transform.SetParent(panel.transform, false);
            var ppVl = passcodePanel.AddComponent<VerticalLayoutGroup>();
            ppVl.spacing = 8;
            ppVl.childForceExpandWidth = true;
            ppVl.childForceExpandHeight = false;
            passcodePanel.AddComponent<LayoutElement>().flexibleHeight = 1;

            // Input field
            var inputGo = new GameObject("InputPasscode", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputGo.transform.SetParent(passcodePanel.transform, false);
            inputGo.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
            inputGo.AddComponent<LayoutElement>().preferredHeight = 55;

            var inputText = CreateText(inputGo.transform, "Text", "", 22);
            StretchFill(inputText.go.GetComponent<RectTransform>());
            inputText.go.GetComponent<RectTransform>().offsetMin = new Vector2(10, 5);
            inputText.go.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -5);

            var placeholder = CreateText(inputGo.transform, "Placeholder", "Enter code...", 22);
            StretchFill(placeholder.go.GetComponent<RectTransform>());
            placeholder.go.GetComponent<RectTransform>().offsetMin = new Vector2(10, 5);
            placeholder.go.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -5);
            placeholder.text.color = new Color(1, 1, 1, 0.3f);

            var inputField = inputGo.GetComponent<TMP_InputField>();
            inputField.textComponent = inputText.text;
            inputField.placeholder = placeholder.text;

            // Swipe panel (tab 1 - placeholder)
            var swipePanel = new GameObject("SwipePanel", typeof(RectTransform));
            swipePanel.transform.SetParent(panel.transform, false);
            swipePanel.AddComponent<LayoutElement>().flexibleHeight = 1;
            var swipeText = CreateText(swipePanel.transform, "SwipeText", "Swipe Pattern", 18);
            StretchFill(swipeText.go.GetComponent<RectTransform>());
            swipePanel.SetActive(false);

            // PanelSwitch for lock types
            var panelSwitch = panel.AddComponent<SelectSwitchGameObject>();
            panelSwitch.obSelects = new GroupGameObject[]
            {
                new GroupGameObject { obGroups = new[] { passcodePanel } },
                new GroupGameObject { obGroups = new[] { swipePanel } }
            };

            // Error text
            var txtError = CreateText(panel.transform, "TxtError", "Wrong passcode!", 16);
            txtError.text.color = Color.red;
            txtError.go.AddComponent<LayoutElement>().preferredHeight = 25;
            txtError.go.SetActive(false);

            // Confirm button
            var btnConfirm = CreateLayoutButton(panel.transform, "BtnConfirm", "CONFIRM", 55);

            var btnClose = CreateCloseButton(root.transform);
            comp.closeBtn = btnClose;

            AddShowAnimation(root);

            WireField(comp, "panelSwitch", panelSwitch);
            WireField(comp, "inputPasscode", inputField);
            WireField(comp, "btnConfirm", btnConfirm.btn);
            WireField(comp, "txtHint", txtHint.text);
            WireField(comp, "txtError", txtError.text);

            SavePrefab(root, path);
        }

        static void CreateConfirmExitPrefab()
        {
            string path = PREFABS + "/Popups/UIConfirmExit.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UIConfirmExit", Vector2.zero, true);
            root.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
            var comp = root.AddComponent<UIConfirmExit>();
            comp.uiName = UIName.ConfirmExit;

            var panel = CreatePopupPanel(root.transform, "Panel", 450, 220);

            var title = CreateText(panel.transform, "Title", "EXIT GAME?", 26);
            title.text.fontStyle = FontStyles.Bold;
            title.go.AddComponent<LayoutElement>().preferredHeight = 50;

            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(panel.transform, false);
            spacer.AddComponent<LayoutElement>().flexibleHeight = 1;

            var btnBar = new GameObject("ButtonBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            btnBar.transform.SetParent(panel.transform, false);
            var hbl = btnBar.GetComponent<HorizontalLayoutGroup>();
            hbl.spacing = 30;
            hbl.childForceExpandWidth = true;
            hbl.childForceExpandHeight = true;
            btnBar.AddComponent<LayoutElement>().preferredHeight = 55;

            var btnYes = CreateButton(btnBar.transform, "BtnYes", "YES", Vector2.zero);
            btnYes.go.GetComponent<Image>().color = new Color(0.6f, 0.15f, 0.15f);
            btnYes.go.AddComponent<EffectButton>();

            var btnNo = CreateButton(btnBar.transform, "BtnNo", "NO", Vector2.zero);
            btnNo.go.AddComponent<EffectButton>();

            AddShowAnimation(root);

            WireField(comp, "btnYes", btnYes.btn);
            WireField(comp, "btnNo", btnNo.btn);

            SavePrefab(root, path);
        }
        #endregion

        #region Prefabs - HUD
        static void CreateInvestigationHudPrefab()
        {
            string path = PREFABS + "/HUD/UIInvestigationHud.prefab";
            if (Exists(path)) return;

            var root = CreateUIRoot("UIInvestigationHud", Vector2.zero, true);
            var comp = root.AddComponent<UIInvestigationHud>();
            comp.uiName = UIName.InvestigationHud;
            comp.isCache = true;

            // Top bar - stretch across top
            var topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            topBar.transform.SetParent(root.transform, false);
            var tbrt = topBar.GetComponent<RectTransform>();
            SetAnchors(tbrt, 0f, 0.94f, 1f, 1f);
            topBar.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
            var tbhl = topBar.GetComponent<HorizontalLayoutGroup>();
            tbhl.spacing = 10;
            tbhl.padding = new RectOffset(20, 20, 5, 5);
            tbhl.childForceExpandWidth = false;
            tbhl.childForceExpandHeight = true;
            tbhl.childAlignment = TextAnchor.MiddleLeft;

            // Clue counter
            var txtClue = CreateText(topBar.transform, "TxtClueCount", "0/0", 22);
            txtClue.go.AddComponent<LayoutElement>().preferredWidth = 100;
            txtClue.text.alignment = TextAlignmentOptions.Left;

            // Spacer
            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(topBar.transform, false);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1;

            // Notebook button
            var btnNotebook = CreateButton(topBar.transform, "BtnNotebook", "NOTEBOOK", new Vector2(140, 0));
            btnNotebook.go.AddComponent<LayoutElement>().preferredWidth = 140;
            btnNotebook.go.AddComponent<EffectButton>();

            // Badge on notebook button
            var badge = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            badge.transform.SetParent(btnNotebook.go.transform, false);
            badge.GetComponent<Image>().color = Color.red;
            var brt = badge.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(1, 1);
            brt.anchorMax = new Vector2(1, 1);
            brt.pivot = new Vector2(1, 1);
            brt.sizeDelta = new Vector2(16, 16);
            brt.anchoredPosition = new Vector2(-3, -3);
            badge.SetActive(false);

            var badgeToggle = btnNotebook.go.AddComponent<SelectToggleGameObject>();
            badgeToggle.obSelect = new[] { badge };
            badgeToggle.obUnSelect = new GameObject[0];

            // Settings button
            var btnSettings = CreateButton(topBar.transform, "BtnSettings", "SETTINGS", new Vector2(140, 0));
            btnSettings.go.AddComponent<LayoutElement>().preferredWidth = 140;
            btnSettings.go.AddComponent<EffectButton>();

            WireField(comp, "btnSettings", btnSettings.btn);
            WireField(comp, "btnNotebook", btnNotebook.btn);
            WireField(comp, "txtClueCount", txtClue.text);
            WireField(comp, "notebookBadge", badgeToggle);

            SavePrefab(root, path);
        }
        #endregion

        #region Sample Data
        static void CreateSampleCharacters()
        {
            string dir = DATA + "/Characters";
            CreateCharacter(dir, "Char_Detective", "Tham Tu Minh", new Color(0.3f, 0.7f, 1f));
            CreateCharacter(dir, "Char_Wife", "Ba Lan", new Color(1f, 0.6f, 0.6f));
            CreateCharacter(dir, "Char_Neighbor", "Ong Hung", new Color(0.6f, 1f, 0.6f));
            CreateCharacter(dir, "Char_Butler", "Quan Gia Tam", new Color(0.8f, 0.8f, 0.5f));
            CreateCharacter(dir, "Char_Police", "Canh Sat Phong", new Color(0.5f, 0.5f, 1f));
        }

        static void CreateCharacter(string dir, string fileName, string charName, Color color)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<DialogueCharacterSO>();
            so.characterId = fileName;
            so.characterName = charName;
            so.nameColor = color;
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateSampleClues()
        {
            string dir = DATA + "/Clues";
            CreateClue(dir, "Clue_BrokenVase", "Binh Hoa Vo", "Mot chiec binh hoa bi vo o phong khach. Co vet mau tren manh vo.", ClueCategory.Physical);
            CreateClue(dir, "Clue_Letter", "Thu An Danh", "La thu de doa gui cho nan nhan truoc khi chet.", ClueCategory.Document);
            CreateClue(dir, "Clue_Footprint", "Dau Giay Bun", "Dau giay bun di vao tu cua sau, kich thuoc 42.", ClueCategory.Physical);
            CreateClue(dir, "Clue_Photo", "Anh Cu", "Buc anh gia dinh cu, co nguoi la dung phia sau.", ClueCategory.Photo);
            CreateClue(dir, "Clue_Knife", "Dao Bep", "Con dao bep bi mat, co vet xuoc bat thuong.", ClueCategory.Physical);
            CreateClue(dir, "Clue_Diary", "Nhat Ky", "Cuon nhat ky cua nan nhan, trang cuoi bi xe.", ClueCategory.Document);
            CreateClue(dir, "Clue_Ring", "Nhan Cuoi", "Chiec nhan cuoi bi giau trong ngan keo khoa.", ClueCategory.Physical);
            CreateClue(dir, "Clue_Testimony1", "Loi Khai Ba Lan", "Ba Lan noi rang da ngu ca dem, nhung hang xom nghe thay tieng cai nhau.", ClueCategory.Testimony);
            CreateClue(dir, "Clue_Testimony2", "Loi Khai Ong Hung", "Ong Hung thay nguoi la di ra khoi nha luc 11h dem.", ClueCategory.Testimony);
            CreateClue(dir, "Clue_Medicine", "Lo Thuoc", "Lo thuoc ngu da gan het, mua 2 ngay truoc.", ClueCategory.Physical);
            CreateClue(dir, "Clue_GardenTool", "Dung Cu Vuon", "Cai cuoc co vet mau, bi vut trong bui cay.", ClueCategory.Physical);
            CreateClue(dir, "Clue_Phone", "Tin Nhan", "Tin nhan cuoi cung nan nhan gui: 'Toi biet su that'.", ClueCategory.Document);
        }

        static void CreateClue(string dir, string fileName, string clueName, string desc, ClueCategory cat)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<ClueSO>();
            so.clueId = fileName;
            so.clueName = clueName;
            so.description = desc;
            so.category = cat;
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateSampleDialogues()
        {
            string dir = DATA + "/Dialogues";
            var detective = LoadAsset<DialogueCharacterSO>(DATA + "/Characters/Char_Detective.asset");
            var wife = LoadAsset<DialogueCharacterSO>(DATA + "/Characters/Char_Wife.asset");
            var neighbor = LoadAsset<DialogueCharacterSO>(DATA + "/Characters/Char_Neighbor.asset");
            var butler = LoadAsset<DialogueCharacterSO>(DATA + "/Characters/Char_Butler.asset");

            CreateDialogue(dir, "Dlg_Intro", "intro", new[]
            {
                (detective, "Day la hien truong vu an. Toi can dieu tra ky."),
                (detective, "Hay bat dau tu phong khach.")
            });
            CreateDialogue(dir, "Dlg_Wife", "wife_talk", new[]
            {
                (wife, "Toi khong biet chuyen gi da xay ra..."),
                (wife, "Chong toi da di ngu som dem qua."),
                (detective, "Ba co nghe thay tieng dong gi khong?"),
                (wife, "Khong... toi cung da ngu.")
            });
            CreateDialogue(dir, "Dlg_Neighbor", "neighbor_talk", new[]
            {
                (neighbor, "Toi thay mot nguoi la o gan nha luc khuya."),
                (detective, "Nguoi do trang nhu the nao?"),
                (neighbor, "Mac ao khoac den, doi mu. Khong nhin ro mat.")
            });
            CreateDialogue(dir, "Dlg_Butler", "butler_talk", new[]
            {
                (butler, "Toi da lam viec o day 10 nam roi."),
                (butler, "Dem qua toi o phong rieng, khong nghe thay gi."),
                (detective, "Ong co biet ai co the muon hai chu nha?"),
                (butler, "...Toi khong dam noi.")
            });
        }

        static void CreateDialogue(string dir, string fileName, string dlgId,
            (DialogueCharacterSO character, string text)[] lines)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<DialogueSequenceSO>();
            so.dialogueId = dlgId;
            foreach (var (ch, txt) in lines)
            {
                so.lines.Add(new DialogueLine { character = ch, text = txt, typingSpeed = 30, waitForInput = true });
            }
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateSampleLocks()
        {
            string dir = DATA + "/Locks";
            CreateLock(dir, "Lock_Safe", LockType.Passcode, "1945", "Goi y: Nam lich su quan trong");
            CreateLock(dir, "Lock_Drawer", LockType.Passcode, "0712", "Goi y: Ngay sinh cua nan nhan");
            CreateLock(dir, "Lock_Cabinet", LockType.Passcode, "2468", "Goi y: So chan lien tiep");
        }

        static void CreateLock(string dir, string fileName, LockType type, string code, string hint)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<LockConfigSO>();
            so.lockType = type;
            so.passcode = code;
            so.hintText = hint;
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateSampleActions()
        {
            string dir = DATA + "/Actions";

            var clueIds = new[] { "Clue_Ring", "Clue_Knife", "Clue_GardenTool" };
            foreach (var id in clueIds)
            {
                var clue = LoadAsset<ClueSO>(DATA + "/Clues/" + id + ".asset");
                if (clue != null)
                    CreateCollectClueAction(dir, "Act_Collect_" + id.Replace("Clue_", ""), clue);
            }

            CreateToastAction(dir, "Act_Toast_WrongCode", "Sai mat khau! Hay thu lai.");
            CreateToastAction(dir, "Act_Toast_Unlocked", "Mo khoa thanh cong!");
            CreateClosePopupAction(dir, "Act_ClosePopup");
            CreateWaitAction(dir, "Act_Wait_1s", 1f);
        }

        static void CreateCollectClueAction(string dir, string fileName, ClueSO clue)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<CollectClueConfig>();
            so.clue = clue;
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateToastAction(string dir, string fileName, string msg)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<ShowToastConfig>();
            so.message = msg;
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateClosePopupAction(string dir, string fileName)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<ClosePopupConfig>();
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateWaitAction(string dir, string fileName, float dur)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<WaitConfig>();
            so.duration = dur;
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateSampleInteractables()
        {
            string dir = DATA + "/Interactables";

            // Clue interactables (each unique for each room)
            CreateClueInteractable(dir, "IO_BrokenVase", "Clue_BrokenVase");
            CreateClueInteractable(dir, "IO_Letter", "Clue_Letter");
            CreateClueInteractable(dir, "IO_Footprint_Bath", "Clue_Footprint");
            CreateClueInteractable(dir, "IO_Footprint_Yard", "Clue_Footprint");
            CreateClueInteractable(dir, "IO_Photo_Living", "Clue_Photo");
            CreateClueInteractable(dir, "IO_Photo_Guest", "Clue_Photo");
            CreateClueInteractable(dir, "IO_Diary", "Clue_Diary");
            CreateClueInteractable(dir, "IO_Medicine", "Clue_Medicine");
            CreateClueInteractable(dir, "IO_Phone", "Clue_Phone");
            CreateClueInteractable(dir, "IO_GardenToolClue", "Clue_GardenTool");
            CreateClueInteractable(dir, "IO_Letter_Pond", "Clue_Letter");

            // NPC interactables
            CreateNPCInteractable(dir, "IO_Wife", "Dlg_Wife");
            CreateNPCInteractable(dir, "IO_Neighbor", "Dlg_Neighbor");
            CreateNPCInteractable(dir, "IO_Butler", "Dlg_Butler");

            // Locked items
            CreateLockedInteractable(dir, "IO_Safe", "Lock_Safe",
                new[] { "Act_Toast_Unlocked", "Act_Collect_Ring" },
                new[] { "Act_Toast_WrongCode" });
            CreateLockedInteractable(dir, "IO_Drawer", "Lock_Drawer",
                new[] { "Act_Toast_Unlocked", "Act_Collect_Knife" },
                new[] { "Act_Toast_WrongCode" });
            CreateLockedInteractable(dir, "IO_Cabinet", "Lock_Cabinet",
                new[] { "Act_Toast_Unlocked", "Act_Collect_GardenTool" },
                new[] { "Act_Toast_WrongCode" });
        }

        static void CreateClueInteractable(string dir, string fileName, string clueAssetName)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<InteractableObjectSO>();
            so.objectId = fileName;
            so.interactType = InteractType.Clue;
            so.isOneTimeOnly = true;
            so.hitboxSize = new Vector2(80, 80);
            so.clue = LoadAsset<ClueSO>(DATA + "/Clues/" + clueAssetName + ".asset");
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateNPCInteractable(string dir, string fileName, string dlgAssetName)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<InteractableObjectSO>();
            so.objectId = fileName;
            so.interactType = InteractType.NPC;
            so.hitboxSize = new Vector2(100, 150);
            so.dialogue = LoadAsset<DialogueSequenceSO>(DATA + "/Dialogues/" + dlgAssetName + ".asset");
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateLockedInteractable(string dir, string fileName, string lockName,
            string[] successActions, string[] failActions)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<InteractableObjectSO>();
            so.objectId = fileName;
            so.interactType = InteractType.LockedItem;
            so.hitboxSize = new Vector2(100, 100);
            so.lockConfig = LoadAsset<LockConfigSO>(DATA + "/Locks/" + lockName + ".asset");

            foreach (var a in successActions)
            {
                var cfg = LoadAsset<ActionStepConfig>(DATA + "/Actions/" + a + ".asset");
                if (cfg != null) so.onUnlockSuccess.Add(cfg);
            }
            foreach (var a in failActions)
            {
                var cfg = LoadAsset<ActionStepConfig>(DATA + "/Actions/" + a + ".asset");
                if (cfg != null) so.onUnlockFail.Add(cfg);
            }
            AssetDatabase.CreateAsset(so, path);
        }

        static void CreateSampleRoomsAndMaps()
        {
            string roomDir = DATA + "/Rooms";
            string mapDir = DATA + "/Maps";

            // 9 Rooms - each with UNIQUE interactable references
            var room1 = CreateRoom(roomDir, "Room_LivingRoom", "Phong Khach", "Dlg_Intro",
                ("IO_BrokenVase", 200, 300), ("IO_Photo_Living", 500, 200), ("IO_Wife", 800, 350));
            var room2 = CreateRoom(roomDir, "Room_Kitchen", "Phong Bep", null,
                ("IO_Medicine", 300, 100));
            var room3 = CreateRoom(roomDir, "Room_Study", "Phong Lam Viec", null,
                ("IO_Letter", -100, 200), ("IO_Diary", 200, -50), ("IO_Safe", 500, 100));
            var room4 = CreateRoom(roomDir, "Room_MasterBedroom", "Phong Ngu Chinh", null,
                ("IO_Phone", 100, 200), ("IO_Drawer", 400, 100), ("IO_Butler", -200, 300));
            var room5 = CreateRoom(roomDir, "Room_Bathroom", "Phong Tam", null,
                ("IO_Footprint_Bath", 0, -100));
            var room6 = CreateRoom(roomDir, "Room_GuestRoom", "Phong Khach San", null,
                ("IO_Photo_Guest", 300, 200));
            var room7 = CreateRoom(roomDir, "Room_FrontYard", "San Truoc", null,
                ("IO_Neighbor", 200, 300), ("IO_Footprint_Yard", -100, 0));
            var room8 = CreateRoom(roomDir, "Room_Shed", "Nha Kho", null,
                ("IO_GardenToolClue", -200, 100), ("IO_Cabinet", 300, 50));
            var room9 = CreateRoom(roomDir, "Room_FishPond", "Ho Ca", null,
                ("IO_Letter_Pond", 100, -200));

            // 3 Maps
            CreateMap(mapDir, "Map_Floor1", "Tang 1 - Biet Thu", room1, room2, room3);
            CreateMap(mapDir, "Map_Floor2", "Tang 2 - Biet Thu", room4, room5, room6);
            CreateMap(mapDir, "Map_Garden", "Khu Vuon", room7, room8, room9);

            // Wire GameConfig
            var config = LoadAsset<GameConfigSO>(DATA + "/Config/GameConfig.asset");
            if (config != null)
            {
                config.allMaps.Clear();
                var map1 = LoadAsset<MapSO>(mapDir + "/Map_Floor1.asset");
                var map2 = LoadAsset<MapSO>(mapDir + "/Map_Floor2.asset");
                var map3 = LoadAsset<MapSO>(mapDir + "/Map_Garden.asset");
                if (map1 != null) config.allMaps.Add(map1);
                if (map2 != null) config.allMaps.Add(map2);
                if (map3 != null) config.allMaps.Add(map3);
                EditorUtility.SetDirty(config);
            }
        }

        static RoomSO CreateRoom(string dir, string fileName, string roomName, string entryDlgName,
            params (string ioName, float x, float y)[] interactables)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return LoadAsset<RoomSO>(path);

            var so = ScriptableObject.CreateInstance<RoomSO>();
            so.roomId = fileName;
            so.roomName = roomName;
            so.backgroundSize = new Vector2(2560, 1440);

            if (!string.IsNullOrEmpty(entryDlgName))
                so.entryDialogue = LoadAsset<DialogueSequenceSO>(DATA + "/Dialogues/" + entryDlgName + ".asset");

            foreach (var (ioName, x, y) in interactables)
            {
                var ioData = LoadAsset<InteractableObjectSO>(DATA + "/Interactables/" + ioName + ".asset");
                if (ioData != null)
                    so.interactables.Add(new RoomInteractable { data = ioData, positionOnBackground = new Vector2(x, y) });
                else
                    Debug.LogWarning($"[Room] Interactable not found: {ioName}");
            }

            AssetDatabase.CreateAsset(so, path);
            return so;
        }

        static void CreateMap(string dir, string fileName, string mapName, params RoomSO[] rooms)
        {
            string path = $"{dir}/{fileName}.asset";
            if (Exists(path)) return;
            var so = ScriptableObject.CreateInstance<MapSO>();
            so.mapId = fileName;
            so.mapName = mapName;
            foreach (var r in rooms)
                if (r != null) so.rooms.Add(r);
            AssetDatabase.CreateAsset(so, path);
        }
        #endregion

        #region Wire References
        static void WireUIRegistry()
        {
            var registry = LoadAsset<UIRegistrySO>(DATA + "/Config/UIRegistry.asset");
            if (registry == null) { Debug.LogError("[Wire] UIRegistry not found"); return; }

            var so = new SerializedObject(registry);
            var entries = so.FindProperty("entries");
            entries.ClearArray();

            var uiMap = new (UIName name, string path, int layer, bool cache)[]
            {
                (UIName.Toast, PREFABS + "/System/UIToast.prefab", 4, true),
                (UIName.Loading, PREFABS + "/System/UILoading.prefab", 3, true),
                (UIName.MainMenu, PREFABS + "/Screens/UIMainMenu.prefab", 0, true),
                (UIName.Cutscene, PREFABS + "/Screens/UICutscene.prefab", 0, false),
                (UIName.NPCDialogue, PREFABS + "/Screens/UINPCDialogue.prefab", 1, true),
                (UIName.MapSelection, PREFABS + "/Screens/UIMapSelection.prefab", 0, false),
                (UIName.Investigation, PREFABS + "/Screens/UIInvestigation.prefab", 0, false),
                (UIName.Notebook, PREFABS + "/Screens/UINotebook.prefab", 1, false),
                (UIName.Settings, PREFABS + "/Popups/UISettings.prefab", 1, false),
                (UIName.Guide, PREFABS + "/Popups/UIGuide.prefab", 1, false),
                (UIName.ClueDetail, PREFABS + "/Popups/UIClueDetail.prefab", 1, false),
                (UIName.LockPuzzle, PREFABS + "/Popups/UILockPuzzle.prefab", 1, false),
                (UIName.ConfirmExit, PREFABS + "/Popups/UIConfirmExit.prefab", 1, false),
                (UIName.InvestigationHud, PREFABS + "/HUD/UIInvestigationHud.prefab", 2, true),
            };

            for (int i = 0; i < uiMap.Length; i++)
            {
                var (uiName, path, layer, cache) = uiMap[i];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) { Debug.LogWarning($"[Wire] Prefab not found: {path}"); continue; }

                var uiBase = prefab.GetComponent<UIBase>();
                if (uiBase == null) { Debug.LogWarning($"[Wire] No UIBase on: {path}"); continue; }

                entries.InsertArrayElementAtIndex(entries.arraySize);
                var entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);

                // FIX: Use GetUINameEnumIndex for correct enum dropdown index
                entry.FindPropertyRelative("uiName").enumValueIndex = GetUINameEnumIndex(uiName);
                entry.FindPropertyRelative("prefab").objectReferenceValue = uiBase;
                entry.FindPropertyRelative("layerIndex").intValue = layer;
                entry.FindPropertyRelative("useCache").boolValue = cache;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
            Debug.Log($"[Wire] UIRegistry: {entries.arraySize} entries wired.");
        }

        static void WireManagerReferences()
        {
            // Re-wire scene managers if they exist
            var gdm = Object.FindFirstObjectByType<GameDataManager>();
            if (gdm != null)
            {
                var s = new SerializedObject(gdm);
                s.FindProperty("onClueCollected").objectReferenceValue =
                    LoadAsset<StringEventChannel>(DATA + "/Events/OnClueCollected.asset");
                s.FindProperty("onNotebookUpdated").objectReferenceValue =
                    LoadAsset<GameEventChannel>(DATA + "/Events/OnNotebookUpdated.asset");
                s.ApplyModifiedPropertiesWithoutUndo();
            }

            var nbm = Object.FindFirstObjectByType<NotebookManager>();
            if (nbm != null)
            {
                var s = new SerializedObject(nbm);
                s.FindProperty("onClueCollected").objectReferenceValue =
                    LoadAsset<StringEventChannel>(DATA + "/Events/OnClueCollected.asset");

                var clueGuids = AssetDatabase.FindAssets("t:ClueSO", new[] { DATA + "/Clues" });
                var allCluesProp = s.FindProperty("allClues");
                allCluesProp.arraySize = clueGuids.Length;
                for (int i = 0; i < clueGuids.Length; i++)
                    allCluesProp.GetArrayElementAtIndex(i).objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<ClueSO>(AssetDatabase.GUIDToAssetPath(clueGuids[i]));

                var charGuids = AssetDatabase.FindAssets("t:DialogueCharacterSO", new[] { DATA + "/Characters" });
                var allCharsProp = s.FindProperty("allCharacters");
                allCharsProp.arraySize = charGuids.Length;
                for (int i = 0; i < charGuids.Length; i++)
                    allCharsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<DialogueCharacterSO>(AssetDatabase.GUIDToAssetPath(charGuids[i]));

                s.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void WirePrefabSOEvents()
        {
            // Wire SO Events into prefabs that need them
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/HUD/UIInvestigationHud.prefab");
            if (hud != null)
            {
                var hudComp = hud.GetComponent<UIInvestigationHud>();
                if (hudComp != null)
                {
                    WireField(hudComp, "onClueCollected",
                        LoadAsset<StringEventChannel>(DATA + "/Events/OnClueCollected.asset"));
                    EditorUtility.SetDirty(hud);
                }
            }

            var settings = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Popups/UISettings.prefab");
            if (settings != null)
            {
                var setComp = settings.GetComponent<UISettings>();
                if (setComp != null)
                {
                    WireField(setComp, "onSettingsChanged",
                        LoadAsset<GameEventChannel>(DATA + "/Events/OnSettingsChanged.asset"));
                    EditorUtility.SetDirty(settings);
                }
            }
        }
        #endregion
    }
}
#endif
