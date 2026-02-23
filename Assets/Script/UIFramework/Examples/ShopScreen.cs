using UnityEngine;
using UnityEngine.UI;
using UIFramework.Core;
using UIFramework.Communication;

namespace UIFramework.Examples
{
    /// <summary>
    /// Complete example: Shop Screen
    /// Demonstrates full MVVM pattern with event communication
    /// </summary>
    public class ShopScreen : UIScreen, IEventHandler<Events.ItemPurchasedEvent>
    {
        [Header("UI References")]
        [SerializeField] private Text coinsText;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject itemPrefab;
        
        private ShopData shopData;
        
        protected override IUIController CreateController()
        {
            return new ShopController();
        }
        
        protected override void OnInitialize(IUIData data)
        {
            base.OnInitialize(data);
            
            shopData = data as ShopData;
            
            // Subscribe to purchase events
            EventBus.Instance.Subscribe<Events.ItemPurchasedEvent>(this);
            
            // Bind UI events
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
            
            // Populate items
            PopulateShop();
            
            // Inject slide transition
            SetTransition(new Animations.SlideTransition(
                Animations.SlideTransition.SlideDirection.Right, 
                0.3f
            ));
        }
        
        protected override void OnRefresh(IUIData data)
        {
            base.OnRefresh(data);
            
            shopData = data as ShopData;
            UpdateCoinsDisplay();
        }
        
        protected override void OnDispose()
        {
            EventBus.Instance.Unsubscribe<Events.ItemPurchasedEvent>(this);
            
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);
            
            base.OnDispose();
        }
        
        private void PopulateShop()
        {
            if (shopData == null || itemContainer == null)
                return;
            
            // Clear existing items
            foreach (Transform child in itemContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create item UIs
            foreach (var item in shopData.Items)
            {
                CreateItemUI(item);
            }
            
            UpdateCoinsDisplay();
        }
        
        private void CreateItemUI(ShopItemData item)
        {
            if (itemPrefab == null)
                return;
            
            var itemObj = Instantiate(itemPrefab, itemContainer);
            var itemUI = itemObj.GetComponent<ShopItemUI>();
            
            if (itemUI != null)
            {
                itemUI.Setup(item, OnItemClicked);
            }
        }
        
        private void OnItemClicked(ShopItemData item)
        {
            var controller = this.controller as ShopController;
            controller?.OnItemPurchaseRequested(item, shopData.PlayerCoins);
        }
        
        private void UpdateCoinsDisplay()
        {
            if (coinsText != null && shopData != null)
            {
                coinsText.text = $"Coins: {shopData.PlayerCoins}";
            }
        }
        
        private void OnCloseClicked()
        {
            Managers.UIManager.Instance.Hide<ShopScreen>();
        }
        
        // Handle purchase event
        public void Handle(Events.ItemPurchasedEvent eventData)
        {
            // Update data with new coin amount
            shopData = shopData.WithCoins(eventData.RemainingCoins);
            Refresh();
        }
    }
    
    /// <summary>
    /// Shop Controller - handles shop business logic
    /// </summary>
    public class ShopController : UIControllerBase
    {
        public void OnItemPurchaseRequested(ShopItemData item, int playerCoins)
        {
            // Validate purchase
            if (playerCoins < item.Price)
            {
                ShowInsufficientCoinsPopup();
                return;
            }
            
            // Show confirmation
            ShowPurchaseConfirmation(item, playerCoins);
        }
        
        private void ShowPurchaseConfirmation(ShopItemData item, int playerCoins)
        {
            var confirmData = new ConfirmationPopupData(
                "Purchase Item",
                $"Buy {item.Name} for {item.Price} coins?",
                onConfirm: () => ExecutePurchase(item, playerCoins),
                onCancel: () => Debug.Log("Purchase cancelled")
            );
            
            var popup = Managers.UIManager.Instance.Show<ConfirmationPopup>(confirmData);
            
            if (popup != null)
            {
                popup.SetAnimation(new Animations.ScaleAnimation(0.2f));
            }
        }
        
        private void ExecutePurchase(ShopItemData item, int playerCoins)
        {
            Debug.Log($"[ShopController] Purchasing {item.Name}");
            
            // Call domain service (not shown)
            // ShopService.PurchaseItem(item.Id);
            
            int remainingCoins = playerCoins - item.Price;
            
            // Publish event
            EventBus.Instance.Publish(new Events.ItemPurchasedEvent(item.Id, remainingCoins));
            
            // Show success message
            Debug.Log($"Purchased {item.Name}! Remaining: {remainingCoins} coins");
        }
        
        private void ShowInsufficientCoinsPopup()
        {
            var data = new ConfirmationPopupData(
                "Insufficient Coins",
                "You don't have enough coins!",
                onConfirm: () => Debug.Log("Get more coins"),
                onCancel: null
            );
            
            Managers.UIManager.Instance.Show<ConfirmationPopup>(data);
        }
    }
    
    /// <summary>
    /// Shop Data - immutable ViewModel
    /// </summary>
    [System.Serializable]
    public class ShopData : UIDataBase
    {
        public System.Collections.Generic.List<ShopItemData> Items { get; private set; }
        public int PlayerCoins { get; private set; }
        
        public ShopData(System.Collections.Generic.List<ShopItemData> items, int playerCoins)
        {
            Items = items;
            PlayerCoins = playerCoins;
        }
        
        // Controlled mutation - returns new instance
        public ShopData WithCoins(int coins)
        {
            return new ShopData(Items, coins);
        }
    }
    
    [System.Serializable]
    public class ShopItemData
    {
        public string Id;
        public string Name;
        public string Description;
        public int Price;
        public Sprite Icon;
        
        public ShopItemData(string id, string name, string description, int price, Sprite icon = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Icon = icon;
        }
    }
    
    /// <summary>
    /// Shop Item UI Component
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text priceText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button buyButton;
        
        private ShopItemData itemData;
        private System.Action<ShopItemData> onItemClicked;
        
        public void Setup(ShopItemData data, System.Action<ShopItemData> onClick)
        {
            itemData = data;
            onItemClicked = onClick;
            
            if (nameText != null)
                nameText.text = data.Name;
            
            if (priceText != null)
                priceText.text = $"{data.Price} coins";
            
            if (iconImage != null && data.Icon != null)
                iconImage.sprite = data.Icon;
            
            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }
        
        private void OnBuyClicked()
        {
            onItemClicked?.Invoke(itemData);
        }
    }
}

namespace UIFramework.Examples.Events
{
    public class ItemPurchasedEvent : UIFramework.Communication.IUIEvent
    {
        public string ItemId { get; private set; }
        public int RemainingCoins { get; private set; }
        
        public ItemPurchasedEvent(string itemId, int remainingCoins)
        {
            ItemId = itemId;
            RemainingCoins = remainingCoins;
        }
    }
}
