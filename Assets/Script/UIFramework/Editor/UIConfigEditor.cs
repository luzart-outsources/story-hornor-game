#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UIFramework.Manager;

namespace UIFramework.Editor
{
    [CustomEditor(typeof(UIConfig))]
    public class UIConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UIConfig config = (UIConfig)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Validate Registry"))
            {
                config.ValidateRegistry();
            }

            if (GUILayout.Button("Clear All"))
            {
                if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all UI elements?", "Yes", "No"))
                {
                    config.UIElements.Clear();
                    EditorUtility.SetDirty(config);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Total UI Elements: {config.UIElements.Count}", EditorStyles.miniLabel);
        }
    }
}
#endif
