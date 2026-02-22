#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Luzart.UIFramework.Editor
{
    [CustomEditor(typeof(UIRegistry))]
    public class UIRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            var registry = (UIRegistry)target;

            if (GUILayout.Button("Validate Entries", GUILayout.Height(30)))
            {
                registry.ValidateEntries();
                EditorUtility.SetDirty(registry);
            }

            if (GUILayout.Button("Rebuild Entry Map", GUILayout.Height(25)))
            {
                registry.BuildEntryMap();
                Debug.Log("UIRegistry: Entry map rebuilt.");
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "ViewId must be unique and match the class name of your UI component.\n" +
                "Use 'Validate Entries' to check for duplicates and missing references.",
                MessageType.Info
            );
        }
    }
}
#endif
