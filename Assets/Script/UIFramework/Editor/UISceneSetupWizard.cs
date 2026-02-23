using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

namespace UIFramework.Editor
{
    /// <summary>
    /// Scene setup wizard - creates a scene with UIManager ready to go
    /// </summary>
    public class UISceneSetupWizard : EditorWindow
    {
        private string sceneName = "UITestScene";
        private bool createEventSystem = true;
        private bool createUIManager = true;
        private bool createGameController = true;
        
        [MenuItem("Window/UIFramework/Scene Setup Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<UISceneSetupWizard>("Scene Setup");
            window.Show();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("UI Scene Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            sceneName = EditorGUILayout.TextField("Scene Name", sceneName);
            
            EditorGUILayout.Space();
            
            createEventSystem = EditorGUILayout.Toggle("Create EventSystem", createEventSystem);
            createUIManager = EditorGUILayout.Toggle("Create UIManager", createUIManager);
            createGameController = EditorGUILayout.Toggle("Create GameController", createGameController);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Setup Scene", GUILayout.Height(40)))
            {
                SetupScene();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This will create a new scene with:\n" +
                "- EventSystem (for UI input)\n" +
                "- UIManager (with Canvas setup)\n" +
                "- GameController (example controller)\n" +
                "- UIRegistry asset",
                MessageType.Info);
        }
        
        private void SetupScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Create EventSystem if needed
            if (createEventSystem && FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            // Create UIManager
            if (createUIManager)
            {
                var uiManagerGO = new GameObject("[UIManager]");
                var uiManager = uiManagerGO.AddComponent<UIFramework.Managers.UIManager>();
                
                // Create or find UIRegistry
                var registry = FindOrCreateUIRegistry();
                
                // Assign registry via reflection (since it's a private field)
                var field = typeof(UIFramework.Managers.UIManager).GetField("registry", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(uiManager, registry);
                
                EditorUtility.SetDirty(uiManager);
            }
            
            // Create GameController
            if (createGameController)
            {
                var gameControllerGO = new GameObject("[GameController]");
                gameControllerGO.AddComponent<UIFramework.Examples.GameController>();
            }
            
            // Save scene
            var scenePath = $"Assets/Scenes/{sceneName}.unity";
            Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(scene, scenePath);
            
            EditorUtility.DisplayDialog("Success", $"Scene created at {scenePath}", "OK");
        }
        
        private Data.UIRegistry FindOrCreateUIRegistry()
        {
            // Try to find existing registry
            var registries = AssetDatabase.FindAssets("t:UIRegistry");
            
            if (registries.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(registries[0]);
                return AssetDatabase.LoadAssetAtPath<Data.UIRegistry>(path);
            }
            
            // Create new registry
            var registry = ScriptableObject.CreateInstance<Data.UIRegistry>();
            var assetPath = "Assets/Resources/UIRegistry.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            AssetDatabase.CreateAsset(registry, assetPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Created UIRegistry at {assetPath}");
            
            return registry;
        }
    }
}
