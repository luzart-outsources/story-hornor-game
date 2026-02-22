using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Manager
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "UIFramework/UI Config")]
    public class UIConfig : ScriptableObject
    {
        [Header("Loader Settings")]
        [SerializeField] private bool _useAddressables = false;
        [SerializeField] private bool _useCaching = true;

        [Header("UI Registry")]
        [SerializeField] private List<UIElement> _uiElements = new List<UIElement>();

        public bool UseAddressables => _useAddressables;
        public bool UseCaching => _useCaching;
        public List<UIElement> UIElements => _uiElements;

        public void AddUIElement(UIElement element)
        {
            if (IsAddressDuplicate(element.Address))
            {
                Debug.LogWarning($"[UIConfig] Duplicate address detected: {element.Address}");
                return;
            }

            _uiElements.Add(element);
        }

        public void RemoveUIElement(string uiName)
        {
            _uiElements.RemoveAll(e => e.UIName == uiName);
        }

        public bool IsAddressDuplicate(string address)
        {
            int count = 0;
            foreach (var element in _uiElements)
            {
                if (element.Address == address)
                {
                    count++;
                    if (count > 1)
                        return true;
                }
            }
            return false;
        }

        public void ValidateRegistry()
        {
            var duplicates = new HashSet<string>();
            var seen = new HashSet<string>();

            foreach (var element in _uiElements)
            {
                if (string.IsNullOrEmpty(element.Address))
                {
                    Debug.LogWarning($"[UIConfig] Empty address for UI: {element.UIName}");
                    continue;
                }

                if (!seen.Add(element.Address))
                {
                    duplicates.Add(element.Address);
                }
            }

            if (duplicates.Count > 0)
            {
                Debug.LogError($"[UIConfig] Found {duplicates.Count} duplicate addresses!");
                foreach (var dup in duplicates)
                {
                    Debug.LogError($"  - {dup}");
                }
            }
            else
            {
                Debug.Log("[UIConfig] All addresses are unique.");
            }
        }
    }

    [Serializable]
    public class UIElement
    {
        [SerializeField] private string _uiName;
        [SerializeField] private string _address;
        [SerializeField] private string _uiTypeFullName;

        private Type _cachedType;

        public string UIName
        {
            get => _uiName;
            set => _uiName = value;
        }

        public string Address
        {
            get => _address;
            set => _address = value;
        }

        public string UITypeFullName
        {
            get => _uiTypeFullName;
            set
            {
                _uiTypeFullName = value;
                _cachedType = null;
            }
        }

        public Type UIType
        {
            get
            {
                if (_cachedType == null && !string.IsNullOrEmpty(_uiTypeFullName))
                {
                    _cachedType = Type.GetType(_uiTypeFullName);
                }
                return _cachedType;
            }
            set
            {
                _cachedType = value;
                _uiTypeFullName = value?.AssemblyQualifiedName;
            }
        }
    }
}
