namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "UIRegistry", menuName = "Luzart/UI Registry")]
    public class UIRegistrySO : ScriptableObject
    {
        [Serializable]
        public class UIEntry
        {
            public UIName uiName;
            public UIBase prefab;
            public int layerIndex;
            public bool useCache;
        }

        [SerializeField] private List<UIEntry> entries = new List<UIEntry>();

        private Dictionary<UIName, UIEntry> lookup;

        private void BuildLookup()
        {
            lookup = new Dictionary<UIName, UIEntry>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.prefab == null) continue;
                if (!lookup.ContainsKey(entry.uiName))
                {
                    lookup.Add(entry.uiName, entry);
                }
                else
                {
                    Debug.LogWarning($"[UIRegistry] Duplicate UIName: {entry.uiName}");
                }
            }
        }

        public UIEntry GetEntry(UIName uiName)
        {
            if (lookup == null) BuildLookup();
            lookup.TryGetValue(uiName, out var entry);
            return entry;
        }

        public bool HasEntry(UIName uiName)
        {
            if (lookup == null) BuildLookup();
            return lookup.ContainsKey(uiName);
        }

        private void OnEnable()
        {
            lookup = null;
        }
    }
}
