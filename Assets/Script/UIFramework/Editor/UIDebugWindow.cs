using UnityEngine;
using UnityEditor;
using UIFramework.Managers;
using System.Text;

namespace UIFramework.Editor
{
    /// <summary>
    /// Debug window for UI Manager
    /// Shows opened views, memory usage, pool status
    /// </summary>
    public class UIDebugWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool showOpenedViews = true;
        private bool showMemoryInfo = true;
        private bool showPoolInfo = true;
        
        [MenuItem("Window/UIFramework/UI Debug Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<UIDebugWindow>("UI Debug");
            window.Show();
        }
        
        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("UI Debug Window only works in Play Mode", MessageType.Info);
                return;
            }
            
            var manager = UIManager.Instance;
            if (manager == null)
            {
                EditorGUILayout.HelpBox("UIManager not found", MessageType.Warning);
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Header
            EditorGUILayout.LabelField("UI Framework Debug", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Opened Views Section
            showOpenedViews = EditorGUILayout.Foldout(showOpenedViews, "Opened Views", true);
            if (showOpenedViews)
            {
                DrawOpenedViews(manager);
            }
            
            EditorGUILayout.Space();
            
            // Memory Info Section
            showMemoryInfo = EditorGUILayout.Foldout(showMemoryInfo, "Memory Info", true);
            if (showMemoryInfo)
            {
                DrawMemoryInfo();
            }
            
            EditorGUILayout.Space();
            
            // Pool Info Section
            showPoolInfo = EditorGUILayout.Foldout(showPoolInfo, "Pool Info", true);
            if (showPoolInfo)
            {
                DrawPoolInfo();
            }
            
            EditorGUILayout.Space();
            
            // Action Buttons
            DrawActionButtons(manager);
            
            EditorGUILayout.EndScrollView();
            
            // Auto-repaint
            Repaint();
        }
        
        private void DrawOpenedViews(UIManager manager)
        {
            EditorGUI.indentLevel++;
            
            var states = manager.GetOpenedViewsState();
            
            if (states.Count == 0)
            {
                EditorGUILayout.LabelField("No opened views");
            }
            else
            {
                foreach (var kvp in states)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(200));
                    EditorGUILayout.LabelField(kvp.Value.ToString(), GUILayout.Width(100));
                    
                    if (GUILayout.Button("Hide", GUILayout.Width(60)))
                    {
                        manager.Hide(kvp.Key);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawMemoryInfo()
        {
            EditorGUI.indentLevel++;
            
            var totalMemory = System.GC.GetTotalMemory(false);
            EditorGUILayout.LabelField($"Total GC Memory: {totalMemory / 1024 / 1024} MB");
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawPoolInfo()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Pool information would appear here");
            EditorGUI.indentLevel--;
        }
        
        private void DrawActionButtons(UIManager manager)
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Hide All"))
            {
                manager.HideAll();
            }
            
            if (GUILayout.Button("Clear Cache"))
            {
                manager.ClearCache();
            }
            
            if (GUILayout.Button("Clear Pool"))
            {
                manager.ClearPool();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Log Opened Views"))
            {
                manager.LogOpenedViews();
            }
        }
    }
}
