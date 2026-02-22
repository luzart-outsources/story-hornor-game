#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Luzart.UIFramework.Editor
{
    public class UIDebugWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool showOpenedUIs = true;
        private bool showLayers = true;
        private bool showStacks = true;

        [MenuItem("Luzart/UI Framework/Debug Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<UIDebugWindow>("UI Debug");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see UI debug information.", MessageType.Info);
                return;
            }

            var uiManager = UIManager.Instance;
            if (uiManager == null)
            {
                EditorGUILayout.HelpBox("UIManager instance not found in scene.", MessageType.Warning);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader("UI Framework Debugger");

            showOpenedUIs = EditorGUILayout.Foldout(showOpenedUIs, "Opened UIs", true);
            if (showOpenedUIs)
            {
                DrawOpenedUIs(uiManager);
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Cleanup Dead EventBus References", GUILayout.Height(25)))
            {
                uiManager.EventBus?.CleanupDeadReferences();
                Debug.Log("UIDebugWindow: Cleaned up dead EventBus references.");
            }

            if (GUILayout.Button("Hide All UIs", GUILayout.Height(25)))
            {
                uiManager.HideAll();
                Debug.Log("UIDebugWindow: Hidden all UIs.");
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
        }

        private void DrawOpenedUIs(UIManager uiManager)
        {
            EditorGUI.indentLevel++;

            var openedUIsField = typeof(UIManager).GetField("openedUIs", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (openedUIsField != null)
            {
                var openedUIs = openedUIsField.GetValue(uiManager) as System.Collections.IDictionary;
                
                if (openedUIs == null || openedUIs.Count == 0)
                {
                    EditorGUILayout.LabelField("No UIs currently opened", EditorStyles.miniLabel);
                }
                else
                {
                    foreach (System.Collections.DictionaryEntry entry in openedUIs)
                    {
                        var viewId = entry.Key as string;
                        var ui = entry.Value as UIBase;

                        if (ui != null)
                        {
                            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                            
                            EditorGUILayout.LabelField(viewId, GUILayout.Width(150));
                            EditorGUILayout.LabelField($"State: {ui.State}", GUILayout.Width(120));
                            EditorGUILayout.LabelField($"Layer: {ui.Layer}", GUILayout.Width(100));
                            
                            if (GUILayout.Button("Hide", GUILayout.Width(60)))
                            {
                                uiManager.Hide(viewId);
                            }

                            if (GUILayout.Button("Select", GUILayout.Width(60)))
                            {
                                Selection.activeGameObject = ui.gameObject;
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
#endif
