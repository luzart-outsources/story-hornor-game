#if UNITY_EDITOR
namespace Luzart.Editor
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// Scan toàn bộ folder Data/Clues/ và tự đăng ký tất cả ClueSO vào GameConfig.allClues.
    /// Check duplicate bằng clueId, không thêm trùng.
    /// Cũng dọn dẹp null entries nếu có.
    ///
    /// Menu: DoMiTruth → 🔍 Register All Clues
    /// </summary>
    public static class RegisterAllCluesTool
    {
        private const string CLUES_DIR     = "Assets/Luzart/DoMiTruth/Data/Clues";
        private const string GAMECONFIG_PATH = "Assets/Luzart/DoMiTruth/Data/Config/GameConfig.asset";

        [MenuItem("DoMiTruth/🔍 Register All Clues", false, 20)]
        public static void RegisterAllClues()
        {
            var log = new List<string>();
            log.Add("=== Register All Clues ===");
            log.Add("");

            // 1. Load GameConfig
            var config = AssetDatabase.LoadAssetAtPath<GameConfigSO>(GAMECONFIG_PATH);
            if (config == null)
            {
                EditorUtility.DisplayDialog("Error", "GameConfig not found at:\n" + GAMECONFIG_PATH, "OK");
                return;
            }

            if (config.allClues == null)
                config.allClues = new List<ClueSO>();

            // 2. Dọn null entries
            int removed = config.allClues.RemoveAll(c => c == null);
            if (removed > 0)
                log.Add($"  🧹 Removed {removed} null entries");

            // 3. Scan tất cả ClueSO trong folder
            string[] guids = AssetDatabase.FindAssets("t:ClueSO", new[] { CLUES_DIR });
            log.Add($"  📂 Found {guids.Length} ClueSO assets in {CLUES_DIR}");
            log.Add("");

            int added = 0;
            int existed = 0;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var clue = AssetDatabase.LoadAssetAtPath<ClueSO>(path);
                if (clue == null) continue;

                // Check duplicate by clueId
                bool exists = false;
                for (int i = 0; i < config.allClues.Count; i++)
                {
                    if (config.allClues[i] != null && config.allClues[i].clueId == clue.clueId)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    config.allClues.Add(clue);
                    added++;
                    log.Add($"  + {clue.clueName} ({clue.clueId})");
                }
                else
                {
                    existed++;
                }
            }

            if (existed > 0)
                log.Add($"  ✓ {existed} clues already registered");

            // 4. Save
            if (added > 0 || removed > 0)
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }

            log.Add("");
            log.Add($"✅ GameConfig.allClues: {config.allClues.Count} total ({added} new)");

            string report = string.Join("\n", log);
            EditorUtility.DisplayDialog("✅ Register All Clues Done!", report, "OK");
            Debug.Log(report);
        }
    }
}
#endif
