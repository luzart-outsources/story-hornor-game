#if UNITY_EDITOR
namespace Luzart.Editor
{
    using UnityEditor;
    using UnityEngine;

    public class DoMiTruthSetupTool : EditorWindow
    {
        private Vector2 scrollPos;

        [MenuItem("Tools/DoMiTruth/Setup Window", false, 0)]
        static void ShowWindow()
        {
            var w = GetWindow<DoMiTruthSetupTool>("DoMiTruth Setup");
            w.minSize = new Vector2(320, 500);
        }

        [MenuItem("Tools/DoMiTruth/1. Setup All", false, 20)]
        public static void MenuSetupAll() => DoMiTruthAutoSetup.SetupAll();

        [MenuItem("Tools/DoMiTruth/2. Create Core Assets", false, 40)]
        public static void MenuCreateCore() => DoMiTruthAutoSetup.CreateCoreAssets();

        [MenuItem("Tools/DoMiTruth/3. Create UI Prefabs", false, 41)]
        public static void MenuCreatePrefabs() => DoMiTruthAutoSetup.CreateAllPrefabs();

        [MenuItem("Tools/DoMiTruth/4. Create Sample Data (9 Rooms)", false, 42)]
        public static void MenuCreateSampleData() => DoMiTruthAutoSetup.CreateSampleData();

        [MenuItem("Tools/DoMiTruth/5. Setup Scene", false, 43)]
        public static void MenuSetupScene() => DoMiTruthAutoSetup.SetupScene();

        [MenuItem("Tools/DoMiTruth/6. Wire All References", false, 44)]
        public static void MenuWireAll() => DoMiTruthAutoSetup.WireAllReferences();

        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("DoMiTruth - Auto Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            DrawSection("ALL-IN-ONE", Color.green, () =>
            {
                if (BigButton("SETUP ALL (Everything)"))
                    DoMiTruthAutoSetup.SetupAll();
            });

            DrawSection("Step-by-Step", Color.cyan, () =>
            {
                if (GUILayout.Button("1. Create Core Assets (Registry, Events, Config)"))
                    DoMiTruthAutoSetup.CreateCoreAssets();

                if (GUILayout.Button("2. Create UI Prefabs (All Screens/Popups/HUD)"))
                    DoMiTruthAutoSetup.CreateAllPrefabs();

                if (GUILayout.Button("3. Create Sample Data (9 Rooms, Characters, Clues)"))
                    DoMiTruthAutoSetup.CreateSampleData();

                if (GUILayout.Button("4. Setup Scene (Canvas, Managers, Bootstrap)"))
                    DoMiTruthAutoSetup.SetupScene();

                if (GUILayout.Button("5. Wire All References"))
                    DoMiTruthAutoSetup.WireAllReferences();
            });

            DrawSection("Utilities", Color.yellow, () =>
            {
                if (GUILayout.Button("Clean & Recreate All Prefabs"))
                {
                    if (EditorUtility.DisplayDialog("Confirm", "Delete and recreate all prefabs?", "Yes", "Cancel"))
                    {
                        DoMiTruthAutoSetup.CleanPrefabs();
                        DoMiTruthAutoSetup.CreateAllPrefabs();
                    }
                }

                if (GUILayout.Button("Validate Setup"))
                    DoMiTruthAutoSetup.ValidateSetup();
            });

            EditorGUILayout.EndScrollView();
        }

        void DrawSection(string title, Color color, System.Action content)
        {
            EditorGUILayout.Space(8);
            var prev = GUI.color;
            GUI.color = color;
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUI.color = prev;
            EditorGUILayout.BeginVertical("box");
            content?.Invoke();
            EditorGUILayout.EndVertical();
        }

        bool BigButton(string label)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                fixedHeight = 40
            };
            return GUILayout.Button(label, style);
        }
    }
}
#endif
