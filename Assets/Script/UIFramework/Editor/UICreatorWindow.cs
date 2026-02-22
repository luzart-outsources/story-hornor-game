#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UIFramework.Manager;
using System.IO;
using System.Text;

namespace UIFramework.Editor
{
    public class UICreatorWindow : EditorWindow
    {
        private string _uiName = "NewUI";
        private UIType _uiType = UIType.Screen;
        private bool _useViewModel = true;
        private string _savePath = "Assets/Script/UI";
        private UIConfig _config;

        private enum UIType
        {
            Screen,
            Popup,
            HUD
        }

        [MenuItem("Window/UI Framework/UI Creator")]
        public static void ShowWindow()
        {
            GetWindow<UICreatorWindow>("UI Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create New UI", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            _uiName = EditorGUILayout.TextField("UI Name", _uiName);
            _uiType = (UIType)EditorGUILayout.EnumPopup("UI Type", _uiType);
            _useViewModel = EditorGUILayout.Toggle("Use ViewModel (MVVM)", _useViewModel);
            _savePath = EditorGUILayout.TextField("Save Path", _savePath);

            EditorGUILayout.Space();

            _config = EditorGUILayout.ObjectField("UI Config", _config, typeof(UIConfig), false) as UIConfig;

            EditorGUILayout.Space();

            if (GUILayout.Button("Create UI"))
            {
                CreateUI();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This tool will create:\n" +
                "- UI View script\n" +
                "- ViewModel script (if enabled)\n" +
                "- Register to UIConfig (if provided)",
                MessageType.Info
            );
        }

        private void CreateUI()
        {
            if (string.IsNullOrEmpty(_uiName))
            {
                EditorUtility.DisplayDialog("Error", "UI Name cannot be empty!", "OK");
                return;
            }

            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            string viewScriptPath = Path.Combine(_savePath, $"{_uiName}.cs");
            string viewModelScriptPath = Path.Combine(_savePath, $"{_uiName}ViewModel.cs");

            if (File.Exists(viewScriptPath))
            {
                if (!EditorUtility.DisplayDialog("File Exists", $"{_uiName}.cs already exists. Overwrite?", "Yes", "No"))
                {
                    return;
                }
            }

            CreateViewScript(viewScriptPath);

            if (_useViewModel)
            {
                CreateViewModelScript(viewModelScriptPath);
            }

            AssetDatabase.Refresh();

            if (_config != null)
            {
                RegisterToConfig();
            }

            EditorUtility.DisplayDialog("Success", $"UI '{_uiName}' created successfully!", "OK");
        }

        private void CreateViewScript(string path)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            
            if (_useViewModel)
            {
                sb.AppendLine("using UIFramework.MVVM;");
            }
            else
            {
                sb.AppendLine("using UIFramework.Core;");
            }

            sb.AppendLine();

            string baseClass = _uiType switch
            {
                UIType.Screen => _useViewModel ? $"UIView<{_uiName}ViewModel>" : "UIScreen",
                UIType.Popup => "UIPopup",
                UIType.HUD => "UIHUD",
                _ => "UIBase"
            };

            sb.AppendLine($"public class {_uiName} : {baseClass}");
            sb.AppendLine("{");
            sb.AppendLine("    [Header(\"UI References\")]");
            sb.AppendLine("    // Add your UI component references here");
            sb.AppendLine();
            sb.AppendLine("    protected override void OnInitialize(object data)");
            sb.AppendLine("    {");
            sb.AppendLine("        base.OnInitialize(data);");
            sb.AppendLine("        // Initialize your UI here");
            sb.AppendLine("    }");
            sb.AppendLine();

            if (_useViewModel)
            {
                sb.AppendLine("    protected override void OnViewModelChanged()");
                sb.AppendLine("    {");
                sb.AppendLine("        if (ViewModel == null)");
                sb.AppendLine("            return;");
                sb.AppendLine();
                sb.AppendLine("        // Update UI based on ViewModel");
                sb.AppendLine("    }");
                sb.AppendLine();
            }

            sb.AppendLine("    protected override void OnBeforeShow()");
            sb.AppendLine("    {");
            sb.AppendLine("        base.OnBeforeShow();");
            sb.AppendLine("        // Called before UI is shown");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    protected override void OnAfterShow()");
            sb.AppendLine("    {");
            sb.AppendLine("        base.OnAfterShow();");
            sb.AppendLine("        // Called after UI is shown");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    protected override void OnDispose()");
            sb.AppendLine("    {");
            sb.AppendLine("        base.OnDispose();");
            sb.AppendLine("        // Cleanup resources");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(path, sb.ToString());
        }

        private void CreateViewModelScript(string path)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using UIFramework.MVVM;");
            sb.AppendLine();
            sb.AppendLine("[Serializable]");
            sb.AppendLine($"public class {_uiName}ViewModel : ViewModelBase");
            sb.AppendLine("{");
            sb.AppendLine("    private string _exampleProperty;");
            sb.AppendLine();
            sb.AppendLine("    public string ExampleProperty");
            sb.AppendLine("    {");
            sb.AppendLine("        get => _exampleProperty;");
            sb.AppendLine("        set");
            sb.AppendLine("        {");
            sb.AppendLine("            if (_exampleProperty != value)");
            sb.AppendLine("            {");
            sb.AppendLine("                _exampleProperty = value;");
            sb.AppendLine("                NotifyPropertyChanged();");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public {_uiName}ViewModel()");
            sb.AppendLine("    {");
            sb.AppendLine("        // Initialize default values");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(path, sb.ToString());
        }

        private void RegisterToConfig()
        {
            var element = new UIElement
            {
                UIName = _uiName,
                Address = $"UI/{_uiName}",
                UITypeFullName = $"{_uiName}, Assembly-CSharp"
            };

            _config.AddUIElement(element);
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
