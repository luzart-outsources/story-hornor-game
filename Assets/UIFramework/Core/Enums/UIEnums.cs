using System;

namespace Luzart.UIFramework
{
    [Flags]
    public enum UILayer
    {
        None = 0,
        Background = 1 << 0,
        HUD = 1 << 1,
        Screen = 1 << 2,
        Popup = 1 << 3,
        Overlay = 1 << 4,
        System = 1 << 5
    }

    public enum UIState
    {
        None,
        Initializing,
        Showing,
        Visible,
        Hiding,
        Hidden,
        Disposed
    }

    public enum UILoadMode
    {
        Direct,
        Addressable
    }

    public enum UIPopupMode
    {
        Modal,
        NonModal
    }

    public enum UITransitionType
    {
        None,
        Fade,
        Slide,
        Scale,
        Custom
    }

    public enum UIDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}
