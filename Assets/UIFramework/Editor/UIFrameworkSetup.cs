#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Luzart.UIFramework.Editor
{
    public static class UIFrameworkSetup
    {
        [MenuItem("GameObject/UI/UI Framework/Setup Complete UI System", false, 0)]
        public static void SetupCompleteUISystem()
        {
            var rootGO = new GameObject("UIRoot");
            
            var canvas = rootGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var canvasScaler = rootGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.matchWidthOrHeight = 0.5f;
            
            rootGO.AddComponent<GraphicRaycaster>();
            
            var uiManager = rootGO.AddComponent<UIManager>();
            
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }
            
            var registry = CreateOrLoadRegistry();
            
            var managerSO = new SerializedObject(uiManager);
            managerSO.FindProperty("registry").objectReferenceValue = registry;
            managerSO.ApplyModifiedProperties();
            
            Selection.activeGameObject = rootGO;
            
            Debug.Log("UIFramework: Complete UI system setup created!");
            EditorUtility.DisplayDialog(
                "UI Framework Setup", 
                "UI system created successfully!\n\n" +
                "Components added:\n" +
                "- UIRoot with Canvas\n" +
                "- UIManager\n" +
                "- EventSystem\n" +
                "- UIRegistry asset\n\n" +
                "Next steps:\n" +
                "1. Add UI entries to UIRegistry\n" +
                "2. Create your UI prefabs\n" +
                "3. See Documentation/QUICKSTART.md",
                "OK"
            );
        }

        [MenuItem("Assets/Create/Luzart/UI Framework/Create UI Screen Template", false, 0)]
        public static void CreateUIScreenTemplate()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create UI Screen",
                "NewScreen.cs",
                "cs",
                "Enter a name for the new screen"
            );

            if (string.IsNullOrEmpty(path))
                return;

            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            var namespaceName = "Luzart.UIFramework.Examples";

            var template = GenerateUIScreenTemplate(fileName, namespaceName);
            System.IO.File.WriteAllText(path, template);
            
            AssetDatabase.Refresh();
            
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            Selection.activeObject = asset;
            
            Debug.Log($"UIFramework: Created UI Screen template at {path}");
        }

        private static UIRegistry CreateOrLoadRegistry()
        {
            var registryPath = "Assets/Resources/UIRegistry.asset";
            
            var registry = AssetDatabase.LoadAssetAtPath<UIRegistry>(registryPath);
            
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<UIRegistry>();
                
                var directory = System.IO.Path.GetDirectoryName(registryPath);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                AssetDatabase.CreateAsset(registry, registryPath);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"UIFramework: Created new UIRegistry at {registryPath}");
            }
            
            return registry;
        }

        private static string GenerateUIScreenTemplate(string className, string namespaceName)
        {
            return $@"using Luzart.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace {namespaceName}
{{
    public class {className} : UIScreen
    {{
        // UI References
        [SerializeField] private Button closeButton;

        private {className}Controller controller;

        protected override void Awake()
        {{
            base.Awake();
            closeButton?.onClick.AddListener(OnCloseClicked);
        }}

        protected override void OnDestroy()
        {{
            closeButton?.onClick.RemoveListener(OnCloseClicked);
            base.OnDestroy();
        }}

        protected override void OnInitialize(object data)
        {{
            base.OnInitialize(data);

            var viewModel = new {className}ViewModel();
            var eventBus = UIManager.Instance?.EventBus;

            controller = new {className}Controller();
            controller.Setup(this, viewModel, eventBus);
            controller.Initialize();

            UIManager.Instance?.Context.RegisterController<{className}>(controller);

            Refresh();
        }}

        protected override void OnRefresh()
        {{
            base.OnRefresh();
            // Update UI elements from ViewModel
        }}

        protected override void OnDispose()
        {{
            controller?.Dispose();
            controller = null;
            base.OnDispose();
        }}

        private void OnCloseClicked()
        {{
            controller?.OnCloseClicked();
        }}
    }}

    // ViewModel
    public class {className}ViewModel : UIViewModel
    {{
        // Add properties here
        public override void Reset()
        {{
            base.Reset();
        }}
    }}

    // Controller
    public class {className}Controller : UIController<{className}, {className}ViewModel>
    {{
        protected override void OnInitialize()
        {{
            base.OnInitialize();
        }}

        protected override void SubscribeToEvents()
        {{
            base.SubscribeToEvents();
        }}

        protected override void UnsubscribeFromEvents()
        {{
            base.UnsubscribeFromEvents();
        }}

        public void OnCloseClicked()
        {{
            UIManager.Instance?.Hide<{className}>();
        }}
    }}
}}
";
        }
    }
}
#endif
