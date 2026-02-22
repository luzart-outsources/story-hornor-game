using UnityEngine;

namespace UIFramework.Core
{
    public abstract class UIHUD : UIBase
    {
        protected override void OnInitialize(object data)
        {
            base.OnInitialize(data);
            Layer = UILayer.HUD;
        }
    }
}
