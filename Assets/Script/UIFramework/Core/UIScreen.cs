using UnityEngine;
using UIFramework.Manager;

namespace UIFramework.Core
{
    public abstract class UIScreen : UIBase
    {
        [SerializeField] private bool _allowMultipleInstances = false;
        
        public bool AllowMultipleInstances => _allowMultipleInstances;

        protected override void OnInitialize(object data)
        {
            base.OnInitialize(data);
            Layer = UILayer.Screen;
        }

        protected override void OnBeforeShow()
        {
            base.OnBeforeShow();
            
            if (!_allowMultipleInstances)
            {
                UIManager.Instance.HideAllScreensExcept(GetType());
            }
        }
    }
}
