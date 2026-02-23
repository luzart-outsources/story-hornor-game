namespace UIFramework.Core
{
    /// <summary>
    /// Base class for HUD elements (always visible)
    /// </summary>
    public abstract class UIHud : UIBase
    {
        protected override void OnInitialize(IUIData data)
        {
            base.OnInitialize(data);
            layer = UILayer.HUD;
        }
    }
}
