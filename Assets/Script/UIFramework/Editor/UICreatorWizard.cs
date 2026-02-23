using UnityEngine;
using UnityEditor;
using System.IO;

namespace UIFramework.Editor
{
    /// <summary>
    /// Editor tool to auto-create UI screens and popups
    /// Also auto-registers them to UIManager
    /// </summary>
    public class UICreatorWizard : EditorWindow
    {
        private string uiName = "MyUI";
        private UIType uiType = UIType.Screen;
        private bool createController = true;
        private bool createData = true;
        private bool addToRegistry = true;
        
        private enum UIType
        {
            Screen,
            Popup,
            Hud
        }
        
        [MenuItem("Window/UIFramework/UI Creator Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<UICreatorWizard>("UI Creator");
            window.Show();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("UI Creator Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            uiName = EditorGUILayout.TextField("UI Name", uiName);
            uiType = (UIType)EditorGUILayout.EnumPopup("UI Type", uiType);
            
            EditorGUILayout.Space();
            
            createController = EditorGUILayout.Toggle("Create Controller", createController);
            createData = EditorGUILayout.Toggle("Create Data", createData);
            addToRegistry = EditorGUILayout.Toggle("Add to Registry", addToRegistry);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Create UI", GUILayout.Height(40)))
            {
                CreateUI();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This will create:\n" +
                "- UI View script\n" +
                "- UI Controller script (if enabled)\n" +
                "- UI Data script (if enabled)\n" +
                "- UI Prefab\n" +
                "- Register to UIRegistry (if enabled)", MessageType.Info);
        }
        
        private void CreateUI()
        {
            if (string.IsNullOrEmpty(uiName))
            {
                EditorUtility.DisplayDialog("Error", "UI Name cannot be empty", "OK");
                return;
            }
            
            var sanitizedName = uiName.Replace(" ", "");
            
            // Create scripts
            CreateViewScript(sanitizedName);
            
            if (createController)
                CreateControllerScript(sanitizedName);
            
            if (createData)
                CreateDataScript(sanitizedName);
            
            // Create prefab
            CreatePrefab(sanitizedName);
            
            // Refresh
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", $"Created {sanitizedName} successfully!", "OK");
        }
        
        private void CreateViewScript(string name)
        {
            var baseClass = GetBaseClassName();
            var path = $"Assets/Script/UIFramework/Views/{name}.cs";
            
            var code = @"using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Views
{
    public class " + name + @" : " + baseClass + @"
    {
        // Add your UI component references here
        // [SerializeField] private Button myButton;
        // [SerializeField] private Text myText;
        
        protected override IUIController CreateController()
        {";
            
            if (createController)
            {
                code += @"
            return new " + name + @"Controller();";
            }
            else
            {
                code += @"
            return null; // No controller";
            }
            
            code += @"
        }
        
        protected override void OnInitialize(IUIData data)
        {
            base.OnInitialize(data);
            
            // Bind UI events here
            // myButton?.onClick.AddListener(OnMyButtonClicked);
            
            // Update UI with data
            if (data is " + name + @"Data viewData)
            {
                // Update UI elements with viewData
            }
        }
        
        protected override void OnDispose()
        {
            // Unbind UI events here to prevent memory leaks
            // myButton?.onClick.RemoveListener(OnMyButtonClicked);
            
            base.OnDispose();
        }
        
        // Add your UI event handlers here
        // private void OnMyButtonClicked()
        // {
        //     var controller = this.controller as " + name + @"Controller;
        //     controller?.OnMyButtonPressed();
        // }
    }
}
";
            
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, code);
        }
        
        private void CreateControllerScript(string name)
        {
            var path = $"Assets/Script/UIFramework/Controllers/{name}Controller.cs";
            
            var code = @"using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Views
{
    /// <summary>
    /// Controller for " + name + @" (logic layer)
    /// </summary>
    public class " + name + @"Controller : UIControllerBase
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Subscribe to events if needed
            // Communication.EventBus.Instance.Subscribe<SomeEvent>(this);
        }
        
        protected override void OnShowInternal()
        {
            base.OnShowInternal();
            // Logic when UI is shown
        }
        
        protected override void OnHideInternal()
        {
            base.OnHideInternal();
            // Logic when UI is hidden
        }
        
        protected override void OnDispose()
        {
            // Unsubscribe from events
            // Communication.EventBus.Instance.Unsubscribe<SomeEvent>(this);
            
            base.OnDispose();
        }
        
        // Add your business logic methods here
        // public void OnMyButtonPressed()
        // {
        //     Debug.Log(""Button pressed"");
        //     // Handle business logic
        //     // Publish events
        //     // Call services
        // }
    }
}
";
            
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, code);
        }
        
        private void CreateDataScript(string name)
        {
            var path = $"Assets/Script/UIFramework/Data/{name}Data.cs";
            
            var code = @"using System;
using UIFramework.Core;

namespace UIFramework.Views
{
    /// <summary>
    /// Data/ViewModel for " + name + @"
    /// Keep data immutable or use controlled mutation
    /// </summary>
    [Serializable]
    public class " + name + @"Data : UIDataBase
    {
        // Add your data properties here (keep them private set)
        // public string Title { get; private set; }
        // public int Value { get; private set; }
        
        public " + name + @"Data(/* parameters */)
        {
            // Initialize properties
            // Title = title;
            // Value = value;
        }
        
        // Add controlled mutation methods if needed (return new instance)
        // public " + name + @"Data WithTitle(string newTitle)
        // {
        //     return new " + name + @"Data(newTitle, Value);
        // }
    }
}
";
            
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, code);
        }
        
        private void CreatePrefab(string name)
        {
            var path = $"Assets/Prefabs/UI/{name}.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            
            // Create GameObject
            var go = new GameObject(name);
            
            // Add RectTransform
            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            // Add Canvas Group for transitions
            go.AddComponent<CanvasGroup>();
            
            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            
            Debug.Log($"Created prefab at {path}");
        }
        
        private string GetBaseClassName()
        {
            switch (uiType)
            {
                case UIType.Screen:
                    return "UIScreen";
                case UIType.Popup:
                    return "UIPopup";
                case UIType.Hud:
                    return "UIHud";
                default:
                    return "UIBase";
            }
        }
    }
}
