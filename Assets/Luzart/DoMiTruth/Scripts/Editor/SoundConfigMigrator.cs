#if UNITY_EDITOR
namespace Luzart.EditorTools
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Editor tool: quét field legacy trong SoundConfigSO và tự sinh SoundEntry vào list `entries`.
    /// Chạy: Menu "DoMiTruth/Sound/Migrate Legacy Clips To Entries".
    /// </summary>
    public static class SoundConfigMigrator
    {
        [MenuItem("DoMiTruth/Sound/Migrate Legacy Clips To Entries")]
        public static void Migrate()
        {
            var guids = AssetDatabase.FindAssets("t:SoundConfigSO");
            if (guids == null || guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Sound Migrator", "Không tìm thấy SoundConfigSO nào.", "OK");
                return;
            }

            int totalAdded = 0;
            int totalConfigs = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<SoundConfigSO>(path);
                if (config == null) continue;
                totalConfigs++;

                if (config.entries == null) config.entries = new List<SoundEntry>();

                // index sẵn các id đã có trong entries
                var existing = new HashSet<SoundId>();
                foreach (var e in config.entries)
                    if (e != null) existing.Add(e.id);

                foreach (SoundId id in System.Enum.GetValues(typeof(SoundId)))
                {
                    if (id == SoundId.None) continue;
                    if (existing.Contains(id)) continue;
                    var def = config.BuildDefaultEntry(id);
                    if (def == null) continue;
                    config.entries.Add(def);
                    totalAdded++;
                }

                EditorUtility.SetDirty(config);
                Debug.Log($"[SoundMigrator] Migrated {path}");
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Sound Migrator",
                $"Done.\nConfigs: {totalConfigs}\nEntries added: {totalAdded}", "OK");
        }

        [MenuItem("DoMiTruth/Sound/Clear Entries (Legacy Only)")]
        public static void ClearEntries()
        {
            if (!EditorUtility.DisplayDialog("Sound Migrator",
                "Xoá TOÀN BỘ entries trong các SoundConfigSO (giữ field legacy)?", "Xoá", "Huỷ"))
                return;

            var guids = AssetDatabase.FindAssets("t:SoundConfigSO");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<SoundConfigSO>(path);
                if (config == null) continue;
                config.entries?.Clear();
                EditorUtility.SetDirty(config);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
