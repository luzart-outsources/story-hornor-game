using System;
using UIFramework.Core;

namespace UIFramework.Views
{
    /// <summary>
    /// Data/ViewModel for MainMenu
    /// Keep data immutable or use controlled mutation
    /// </summary>
    [Serializable]
    public class MainMenuData : UIDataBase
    {
        // Add your data properties here (keep them private set)
        // public string Title { get; private set; }
        // public int Value { get; private set; }
        
        public MainMenuData(/* parameters */)
        {
            // Initialize properties
            // Title = title;
            // Value = value;
        }
        
        // Add controlled mutation methods if needed (return new instance)
        // public MainMenuData WithTitle(string newTitle)
        // {
        //     return new MainMenuData(newTitle, Value);
        // }
    }
}
