namespace UIFramework.Core
{
    /// <summary>
    /// Defines the rendering layers for UI elements
    /// </summary>
    public enum UILayer
    {
        HUD = 0,        // Always visible elements (health bar, minimap)
        Screen = 100,   // Main screens (menu, gameplay)
        Popup = 200,    // Popup windows (dialogs, confirmations)
        Overlay = 300   // System overlays (loading, notifications)
    }
}
