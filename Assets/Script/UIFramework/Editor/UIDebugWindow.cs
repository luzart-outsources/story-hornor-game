#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UIFramework.Manager;
using System.Collections.Generic;
using System.Linq;

namespace UIFramework.Editor
{
    public class UIDebugWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showActiveUIs = true;
        private bool _showPopupStack = true;
        private bool _showEventBus = true;

        [MenuItem("Window/UI Framework/Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<UIDebugWindow>("UI Debug");
        }

        private void OnGUI()
        {
            GUILayout.Label("UI Framework Debug", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Debug information is only available in Play Mode", MessageType.Info);
                return;
            }

            if (UIManager.Instance == null)
            {
                EditorGUILayout.HelpBox("UIManager instance not found", MessageType.Warning);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawActiveUIs();
            EditorGUILayout.Space();
            DrawPopupStack();
            EditorGUILayout.Space();
            DrawMemoryInfo();
            EditorGUILayout.Space();
            DrawQuickActions();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Refresh"))
            {
                Repaint();
            }
        }

        private void DrawActiveUIs()
        {
            _showActiveUIs = EditorGUILayout.Foldout(_showActiveUIs, "Active UIs", true);
            if (!_showActiveUIs)
                return;

            EditorGUI.indentLevel++;

            var activeUIsField = typeof(UIManager).GetField("_activeUIs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (activeUIsField != null)
            {
                var activeUIs = activeUIsField.GetValue(UIManager.Instance) as System.Collections.IDictionary;
                if (activeUIs != null && activeUIs.Count > 0)
                {
                    foreach (System.Collections.DictionaryEntry entry in activeUIs)
                    {
                        var uiType = entry.Key as System.Type;
                        var ui = entry.Value as UIFramework.Core.UIBase;
                        
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{uiType?.Name}", GUILayout.Width(200));
                        EditorGUILayout.LabelField($"State: {ui?.State}", GUILayout.Width(150));
                        EditorGUILayout.LabelField($"Layer: {ui?.Layer}", GUILayout.Width(100));
                        
                        if (GUILayout.Button("Hide", GUILayout.Width(60)))
                        {
                            if (uiType != null)
                            {
#if UNITASK_SUPPORT
                                UIManager.Instance.HideAsync(uiType).Forget();
#else
                                UIManager.Instance.Hide(uiType);
#endif
                            }
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No active UIs");
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawPopupStack()
        {
            _showPopupStack = EditorGUILayout.Foldout(_showPopupStack, "Popup Stack", true);
            if (!_showPopupStack)
                return;

            EditorGUI.indentLevel++;

            int stackCount = UIManager.Instance.GetPopupStackCount();
            EditorGUILayout.LabelField($"Stack Count: {stackCount}");

            if (stackCount > 0)
            {
                var popup = UIManager.Instance.GetTopPopup();
                if (popup != null)
                {
                    EditorGUILayout.LabelField($"Top Popup: {popup.GetType().Name}");
                    EditorGUILayout.LabelField($"Is Modal: {popup.IsModal}");
                    EditorGUILayout.LabelField($"Stack Order: {popup.StackOrder}");
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawMemoryInfo()
        {
            EditorGUILayout.LabelField("Memory Information", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            long totalMemory = System.GC.GetTotalMemory(false);
            EditorGUILayout.LabelField($"Total Memory: {FormatBytes(totalMemory)}");

            EditorGUI.indentLevel--;
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Hide All UIs"))
            {
#if UNITASK_SUPPORT
                UIManager.Instance.HideAllAsync().Forget();
#else
                UIManager.Instance.HideAll();
#endif
            }

            if (GUILayout.Button("Clear Event Bus"))
            {
                Events.EventBus.Instance.Clear();
            }

            EditorGUILayout.EndHorizontal();
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
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
