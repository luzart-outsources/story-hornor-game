using System;

namespace Luzart.UIFramework
{
    public abstract class UIEvent
    {
        public DateTime Timestamp { get; private set; }

        protected UIEvent()
        {
            Timestamp = DateTime.UtcNow;
        }
    }

    public class UIOpenedEvent : UIEvent
    {
        public string ViewId { get; }
        public UILayer Layer { get; }

        public UIOpenedEvent(string viewId, UILayer layer)
        {
            ViewId = viewId;
            Layer = layer;
        }
    }

    public class UIClosedEvent : UIEvent
    {
        public string ViewId { get; }
        public UILayer Layer { get; }

        public UIClosedEvent(string viewId, UILayer layer)
        {
            ViewId = viewId;
            Layer = layer;
        }
    }

    public class UILayerChangedEvent : UIEvent
    {
        public UILayer Layer { get; }
        public int ActiveCount { get; }

        public UILayerChangedEvent(UILayer layer, int activeCount)
        {
            Layer = layer;
            ActiveCount = activeCount;
        }
    }
}
