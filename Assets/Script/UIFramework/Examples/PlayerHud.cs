using UnityEngine;
using UnityEngine.UI;
using UIFramework.Core;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example: Player HUD (always visible)
    /// </summary>
    public class PlayerHud : UIHud
    {
        [Header("UI References")]
        [SerializeField] private Text healthText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Text coinText;
        [SerializeField] private Text levelText;
        
        protected override IUIController CreateController()
        {
            return new PlayerHudController();
        }
        
        protected override void OnInitialize(IUIData data)
        {
            base.OnInitialize(data);
            UpdateDisplay(data);
        }
        
        protected override void OnRefresh(IUIData data)
        {
            base.OnRefresh(data);
            UpdateDisplay(data);
        }
        
        private void UpdateDisplay(IUIData data)
        {
            if (data is PlayerHudData hudData)
            {
                if (healthText != null)
                    healthText.text = $"{hudData.CurrentHealth}/{hudData.MaxHealth}";
                    
                if (healthBar != null)
                {
                    healthBar.maxValue = hudData.MaxHealth;
                    healthBar.value = hudData.CurrentHealth;
                }
                
                if (coinText != null)
                    coinText.text = hudData.Coins.ToString();
                    
                if (levelText != null)
                    levelText.text = $"Level {hudData.Level}";
            }
        }
    }
    
    /// <summary>
    /// Controller for Player HUD
    /// </summary>
    public class PlayerHudController : UIControllerBase
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Subscribe to events
            Communication.EventBus.Instance.Subscribe<Events.PlayerHealthChangedEvent>(this as Communication.IEventHandler<Events.PlayerHealthChangedEvent>);
            Communication.EventBus.Instance.Subscribe<Events.PlayerCoinsChangedEvent>(this as Communication.IEventHandler<Events.PlayerCoinsChangedEvent>);
        }
        
        protected override void OnDispose()
        {
            // Unsubscribe from events
            Communication.EventBus.Instance.Unsubscribe<Events.PlayerHealthChangedEvent>(this as Communication.IEventHandler<Events.PlayerHealthChangedEvent>);
            Communication.EventBus.Instance.Unsubscribe<Events.PlayerCoinsChangedEvent>(this as Communication.IEventHandler<Events.PlayerCoinsChangedEvent>);
            
            base.OnDispose();
        }
    }
    
    /// <summary>
    /// Data/ViewModel for Player HUD
    /// </summary>
    [System.Serializable]
    public class PlayerHudData : UIDataBase
    {
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public int Coins { get; private set; }
        public int Level { get; private set; }
        
        public PlayerHudData(int currentHealth, int maxHealth, int coins, int level)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            Coins = coins;
            Level = level;
        }
        
        // Controlled mutation methods
        public PlayerHudData WithHealth(int currentHealth, int maxHealth)
        {
            return new PlayerHudData(currentHealth, maxHealth, Coins, Level);
        }
        
        public PlayerHudData WithCoins(int coins)
        {
            return new PlayerHudData(CurrentHealth, MaxHealth, coins, Level);
        }
    }
}

namespace UIFramework.Examples.Events
{
    public class PlayerHealthChangedEvent : Communication.IUIEvent
    {
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        
        public PlayerHealthChangedEvent(int currentHealth, int maxHealth)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }
    
    public class PlayerCoinsChangedEvent : Communication.IUIEvent
    {
        public int Coins { get; private set; }
        
        public PlayerCoinsChangedEvent(int coins)
        {
            Coins = coins;
        }
    }
}
