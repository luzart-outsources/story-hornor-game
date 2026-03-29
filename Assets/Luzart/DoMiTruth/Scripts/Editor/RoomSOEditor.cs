namespace Luzart.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(RoomSO))]
    public class RoomSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var room = (RoomSO)target;

            EditorGUILayout.Space(10);

            if (room.roomPrefab == null)
            {
                EditorGUILayout.HelpBox(
                    "Chưa có roomPrefab.\n" +
                    "Tạo prefab thủ công hoặc kéo prefab có sẵn vào field trên.\n" +
                    "Trong prefab: thêm InteractableObject component, kéo SO vào field 'data'.",
                    MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button("Open Room Prefab", GUILayout.Height(25)))
                    AssetDatabase.OpenAsset(room.roomPrefab);
            }
        }
    }
}
