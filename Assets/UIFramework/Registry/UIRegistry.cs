using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luzart.UIFramework
{
    [Serializable]
    public class UIRegistryEntry
    {
        public string viewId;
        public UILayer layer;
        public UILoadMode loadMode;
        public string addressOrPath;
        public GameObject prefab;
        public UITransitionType transitionType;
        public bool enablePooling;
        public int poolSize = 1;
    }

    [CreateAssetMenu(fileName = "UIRegistry", menuName = "Luzart/UI Framework/UI Registry")]
    public class UIRegistry : ScriptableObject
    {
        [SerializeField] private List<UIRegistryEntry> entries = new List<UIRegistryEntry>();

        private Dictionary<string, UIRegistryEntry> entryMap;

        public IReadOnlyList<UIRegistryEntry> Entries => entries;

        private void OnEnable()
        {
            BuildEntryMap();
        }

        public void BuildEntryMap()
        {
            entryMap = new Dictionary<string, UIRegistryEntry>();
            
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.viewId))
                {
                    Debug.LogWarning($"UIRegistry: Entry with null or empty viewId found.");
                    continue;
                }

                if (entryMap.ContainsKey(entry.viewId))
                {
                    Debug.LogError($"UIRegistry: Duplicate viewId '{entry.viewId}' found!");
                    continue;
                }

                entryMap[entry.viewId] = entry;
            }
        }

        public UIRegistryEntry GetEntry(string viewId)
        {
            if (entryMap == null)
                BuildEntryMap();

            return entryMap.TryGetValue(viewId, out var entry) ? entry : null;
        }

        public bool HasEntry(string viewId)
        {
            if (entryMap == null)
                BuildEntryMap();

            return entryMap.ContainsKey(viewId);
        }

        public void AddEntry(UIRegistryEntry entry)
        {
            if (entries.Contains(entry))
                return;

            entries.Add(entry);
            BuildEntryMap();
        }

        public void RemoveEntry(string viewId)
        {
            entries.RemoveAll(e => e.viewId == viewId);
            BuildEntryMap();
        }

#if UNITY_EDITOR
        public void ValidateEntries()
        {
            var ids = new HashSet<string>();
            
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.viewId))
                {
                    Debug.LogError("UIRegistry: Entry with null or empty viewId found.", this);
                }
                else if (!ids.Add(entry.viewId))
                {
                    Debug.LogError($"UIRegistry: Duplicate viewId '{entry.viewId}' detected!", this);
                }

                if (entry.loadMode == UILoadMode.Direct && entry.prefab == null)
                {
                    Debug.LogWarning($"UIRegistry: Entry '{entry.viewId}' is Direct mode but has no prefab assigned.", this);
                }

                if (entry.loadMode == UILoadMode.Addressable && string.IsNullOrEmpty(entry.addressOrPath))
                {
                    Debug.LogWarning($"UIRegistry: Entry '{entry.viewId}' is Addressable mode but has no address specified.", this);
                }
            }
        }
#endif
    }
}
