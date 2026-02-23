using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// Base class for UI Screens (full screen views)
    /// </summary>
    public abstract class UIScreen : UIBase
    {
        protected override void OnInitialize(IUIData data)
        {
            base.OnInitialize(data);
            layer = UILayer.Screen;
        }
    }
}
