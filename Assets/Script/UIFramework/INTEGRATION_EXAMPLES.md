# UI Framework - Integration Examples

## Complete Integration Example

### Scenario: RPG Game with Shop, Inventory, and Combat UI

---

## Step 1: Project Setup

### Define Scripting Symbols (Optional)
```
Edit ? Project Settings ? Player ? Scripting Define Symbols:
- UNITASK_SUPPORT (if using UniTask)
- ADDRESSABLES_SUPPORT (if using Addressables)
```

### Create UIRegistry
```
1. Assets ? Create ? UIFramework ? UI Registry
2. Name: "MainUIRegistry"
3. Save in Assets/Resources/
```

### Create Settings (Optional)
```
1. Assets ? Create ? UIFramework ? Settings
2. Name: "UIFrameworkSettings"
3. Save in Assets/Resources/
4. Configure your preferences
```

---

## Step 2: Create UI Elements

### Using UI Creator Wizard

```
Window ? UIFramework ? UI Creator Wizard

Create these UIs:
1. MainMenuScreen (Screen, with controller & data)
2. InventoryScreen (Screen, with controller & data)
3. ShopScreen (Screen, with controller & data)
4. CombatHud (Hud, with controller & data)
5. ConfirmationPopup (Popup, with controller & data)
6. RewardPopup (Popup, with controller & data)
7. DamageNumberPopup (Popup, NO controller - simple display)
```

---

## Step 3: Configure UIRegistry

```
Open MainUIRegistry asset:

1. MainMenuScreen:
   - ViewId: "MainMenuScreen"
   - Layer: Screen
   - LoadMode: Prefab
   - Prefab: [Assign MainMenuScreen prefab]
   - EnableCaching: ?
   
2. ShopScreen:
   - ViewId: "ShopScreen"
   - Layer: Screen
   - LoadMode: Prefab
   - Prefab: [Assign ShopScreen prefab]
   - EnableCaching: ?
   
3. ConfirmationPopup:
   - ViewId: "ConfirmationPopup"
   - Layer: Popup
   - LoadMode: Prefab
   - Prefab: [Assign ConfirmationPopup prefab]
   - EnableCaching: ?
   
4. DamageNumberPopup:
   - ViewId: "DamageNumberPopup"
   - Layer: Overlay
   - LoadMode: Prefab
   - Prefab: [Assign DamageNumberPopup prefab]
   - EnablePooling: ?
   - PoolSize: 20

Click "Validate Registry"
Click "Generate ViewId Enum"
```

---

## Step 4: Setup Initial Scene

### Option A: Use Scene Setup Wizard
```
Window ? UIFramework ? Scene Setup Wizard
- Scene Name: "MainScene"
- ? Create EventSystem
- ? Create UIManager
- ? Create GameController
Click "Setup Scene"
```

### Option B: Manual Setup
```
1. Create GameObject: "[UIManager]"
   - Add UIManager component
   - Assign UIRegistry
   
2. Create GameObject: "[GameController]"
   - Add your GameController component
   
3. Create GameObject: "EventSystem" (if not exists)
   - Add EventSystem component
   - Add StandaloneInputModule component
```

---

## Step 5: Implement Game Controller

```csharp
using UnityEngine;
using UIFramework.Managers;
using UIFramework.Communication;
using UIFramework.Examples;

public class RPGGameController : MonoBehaviour
{
    private PlayerData playerData;
    
    private void Start()
    {
        // Initialize player data
        playerData = new PlayerData
        {
            Health = 100,
            MaxHealth = 100,
            Coins = 500,
            Level = 1
        };
        
        // Subscribe to UI events
        SubscribeToEvents();
        
        // Show main menu
        ShowMainMenu();
    }
    
    private void SubscribeToEvents()
    {
        // Define your events and subscribe
    }
    
    private void ShowMainMenu()
    {
        var menuData = new MainMenuData("Epic RPG", true);
        var menu = UIManager.Instance.Show<MainMenuScreen>(menuData);
        
        if (menu != null)
        {
            menu.SetTransition(new UIFramework.Animations.FadeTransition(0.5f));
        }
    }
    
    // Player data model
    private class PlayerData
    {
        public int Health;
        public int MaxHealth;
        public int Coins;
        public int Level;
    }
}
```

---

## Step 6: Implement Domain Services (Decoupled)

```csharp
using System.Collections.Generic;

namespace Game.Services
{
    /// <summary>
    /// Shop service - domain layer, no UI dependencies
    /// </summary>
    public class ShopService
    {
        private static ShopService instance;
        public static ShopService Instance => instance ?? (instance = new ShopService());
        
        private int playerCoins = 500;
        private List<string> ownedItems = new List<string>();
        
        public List<ShopItemInfo> GetAvailableItems()
        {
            return new List<ShopItemInfo>
            {
                new ShopItemInfo("sword_1", "Iron Sword", 100),
                new ShopItemInfo("armor_1", "Leather Armor", 150),
                new ShopItemInfo("potion_1", "Health Potion", 50),
            };
        }
        
        public bool PurchaseItem(string itemId, int price)
        {
            if (playerCoins < price)
                return false;
            
            playerCoins -= price;
            ownedItems.Add(itemId);
            
            // Publish domain event
            UIFramework.Communication.EventBus.Instance.Publish(
                new UIFramework.Examples.Events.ItemPurchasedEvent(itemId, playerCoins)
            );
            
            return true;
        }
        
        public int GetPlayerCoins() => playerCoins;
    }
    
    public class ShopItemInfo
    {
        public string Id;
        public string Name;
        public int Price;
        
        public ShopItemInfo(string id, string name, int price)
        {
            Id = id;
            Name = name;
            Price = price;
        }
    }
}
```

---

## Step 7: Wire Everything Together

### Main Menu Controller

```csharp
using UIFramework.Core;
using UIFramework.Communication;
using Game.Services;

public class MainMenuController : UIControllerBase
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Debug.Log("[MainMenuController] Initialized");
    }
    
    public void OnPlayButtonPressed()
    {
        Debug.Log("[MainMenuController] Play pressed");
        
        // Start game
        EventBus.Instance.Publish(new StartGameEvent());
        
        // Hide menu, show HUD
        UIFramework.Managers.UIManager.Instance.Hide<UIFramework.Examples.MainMenuScreen>();
        ShowGameHUD();
    }
    
    public void OnShopButtonPressed()
    {
        Debug.Log("[MainMenuController] Shop pressed");
        
        // Get shop data from service
        var shopItems = ShopService.Instance.GetAvailableItems();
        var coins = ShopService.Instance.GetPlayerCoins();
        
        // Convert to UI data
        var uiItems = new System.Collections.Generic.List<UIFramework.Examples.ShopItemData>();
        foreach (var item in shopItems)
        {
            uiItems.Add(new UIFramework.Examples.ShopItemData(
                item.Id, item.Name, "", item.Price
            ));
        }
        
        var shopData = new UIFramework.Examples.ShopData(uiItems, coins);
        
        // Show shop screen
        UIFramework.Managers.UIManager.Instance.Show<UIFramework.Examples.ShopScreen>(shopData);
    }
    
    private void ShowGameHUD()
    {
        var hudData = new UIFramework.Examples.PlayerHudData(100, 100, 500, 1);
        UIFramework.Managers.UIManager.Instance.Show<UIFramework.Examples.PlayerHud>(hudData);
    }
}

public class StartGameEvent : IUIEvent { }
```

---

## Step 8: Testing

### Play Mode Test

```
1. Enter Play Mode
2. Main Menu should appear with fade transition
3. Click "Shop" ? Shop screen slides in
4. Click item ? Confirmation popup scales in
5. Confirm ? Purchase processed
6. Coins updated in shop UI
7. Press ESC ? Shop closes
8. Back to main menu

Window ? UIFramework ? UI Debug Window to monitor
```

### Unit Test Example

```csharp
using NUnit.Framework;
using UIFramework.Testing;

[TestFixture]
public class ShopControllerTests
{
    [Test]
    public void ShopController_InsufficientCoins_ShowsError()
    {
        // Arrange
        var controller = new ShopController();
        var mockView = new MockUIView();
        var item = new ShopItemData("item1", "Sword", "", 100);
        var playerCoins = 50;
        
        controller.Initialize(mockView, null);
        
        // Act
        controller.OnItemPurchaseRequested(item, playerCoins);
        
        // Assert
        // Verify error popup shown
        // (Implementation depends on your test setup)
    }
}
```

---

## Step 9: Production Optimization

### Enable Addressables

```
1. Install Addressables package
2. Add ADDRESSABLES_SUPPORT to scripting defines
3. Mark UI prefabs as Addressables
4. In UIRegistry, change LoadMode to Addressable
5. Set AddressablePath (e.g., "UI/MainMenu")
```

### Enable UniTask

```
1. Install UniTask package
2. Add UNITASK_SUPPORT to scripting defines
3. Use ShowAsync() instead of Show()
```

### Configure Pooling

```
For frequently-spawned UIs:
- Damage numbers
- Notifications
- Tooltips

UIRegistry:
  enablePooling = true
  poolSize = 20-50 (tune based on usage)
  
Bootstrap:
  prewarmConfigs:
    - DamageNumberPopup: 30
    - NotificationPopup: 10
```

---

## Complete Game Flow Example

```
GAME START:
  Bootstrap.Initialize()
    ? UIManager setup
    ? Registry loaded
    ? Pools pre-warmed
    ? Show MainMenu
    
MAIN MENU:
  User clicks "Play"
    ? MainMenuController.OnPlayPressed()
    ? EventBus.Publish(StartGameEvent)
    ? GameController.Handle(StartGameEvent)
    ? Hide MainMenu
    ? Show CombatHud
    ? Start gameplay
    
GAMEPLAY:
  Player takes damage
    ? PlayerService.TakeDamage()
    ? EventBus.Publish(HealthChangedEvent)
    ? CombatHudController.Handle(event)
    ? Update HUD display
    ? Show DamageNumber popup (from pool)
    
  Player opens shop
    ? Input: Press 'S' key
    ? EventBus.Publish(OpenShopEvent)
    ? GameController.Handle(event)
    ? Show ShopScreen (slide transition)
    
SHOP:
  Player buys item
    ? ShopView.OnItemClicked()
    ? ShopController.OnPurchaseRequested()
    ? Show ConfirmationPopup (scale animation)
    ? User confirms
    ? ShopService.PurchaseItem()
    ? EventBus.Publish(ItemPurchasedEvent)
    ? ShopScreen.Handle(event) ? Refresh coins
    ? InventoryScreen.Handle(event) ? Add item
    
  Player closes shop
    ? Press ESC or Close button
    ? Hide ShopScreen
    ? ShopScreen cached (not destroyed)
    ? Back to gameplay
    
GAME END:
  Player dies
    ? Show GameOverPopup
    ? User clicks "Main Menu"
    ? Hide all gameplay UI
    ? Clear cache
    ? Show MainMenu
    ? Reset game state
```

---

## Integration with Existing Systems

### Integrating with Inventory System

```csharp
public class InventoryController : UIControllerBase,
    IEventHandler<ItemPurchasedEvent>,
    IEventHandler<ItemUsedEvent>
{
    private InventoryService inventoryService;
    
    protected override void OnInitialize()
    {
        inventoryService = ServiceLocator.Instance.Get<InventoryService>();
        
        EventBus.Instance.Subscribe<ItemPurchasedEvent>(this);
        EventBus.Instance.Subscribe<ItemUsedEvent>(this);
    }
    
    public void Handle(ItemPurchasedEvent evt)
    {
        // Add item to inventory
        inventoryService.AddItem(evt.ItemId);
        
        // Refresh inventory UI
        view.Refresh();
    }
    
    public void Handle(ItemUsedEvent evt)
    {
        // Remove item from inventory
        inventoryService.RemoveItem(evt.ItemId);
        
        // Refresh inventory UI
        view.Refresh();
    }
}
```

### Integrating with Save System

```csharp
public class SaveLoadController : MonoBehaviour
{
    public void SaveGame()
    {
        var saveData = new SaveData
        {
            currentScreen = UIManager.Instance.GetCurrentScreen(),
            playerData = GetPlayerData(),
            // ... other data
        };
        
        SaveSystem.Save(saveData);
    }
    
    public void LoadGame()
    {
        var saveData = SaveSystem.Load<SaveData>();
        
        // Restore UI state
        if (!string.IsNullOrEmpty(saveData.currentScreen))
        {
            UIManager.Instance.Show(saveData.currentScreen);
        }
        
        // Restore player data
        // ...
    }
}
```

### Integrating with Analytics

```csharp
public class UIAnalyticsMiddleware
{
    public UIAnalyticsMiddleware()
    {
        // Subscribe to screen events
        EventBus.Instance.Subscribe<ScreenChangedEvent>(this);
    }
    
    public void Handle(ScreenChangedEvent evt)
    {
        // Track screen view
        Analytics.CustomEvent("screen_view", new Dictionary<string, object>
        {
            { "screen_name", evt.ToScreen },
            { "from_screen", evt.FromScreen },
            { "timestamp", DateTime.UtcNow }
        });
    }
}
```

---

## Performance Tips

### 1. Preload Critical Screens

```csharp
async void PreloadCriticalUI()
{
    #if UNITASK_SUPPORT
    var tasks = new List<UniTask>
    {
        UIManager.Instance.PreloadAsync("MainMenuScreen"),
        UIManager.Instance.PreloadAsync("PausePopup"),
        UIManager.Instance.PreloadAsync("GameOverScreen")
    };
    
    await UniTask.WhenAll(tasks);
    Debug.Log("Critical UI preloaded");
    #endif
}
```

### 2. Batch UI Updates

```csharp
// BAD: Multiple updates per frame
void Update()
{
    UpdateHealth();
    UpdateMana();
    UpdateCoins();
}

// GOOD: Batch updates
void Update()
{
    if (dataChanged)
    {
        hudView.Refresh();
        dataChanged = false;
    }
}
```

### 3. Use Canvas Groups

```csharp
// For smooth transitions, ensure CanvasGroup on root
// Framework auto-adds if using FadeTransition

// Disable raycasts when hidden
canvasGroup.interactable = false;
canvasGroup.blocksRaycasts = false;
```

---

## Common Patterns

### Pattern 1: Modal Dialog Flow

```csharp
public void ShowDeleteConfirmation(string itemId)
{
    var data = new ConfirmationPopupData(
        "Delete Item",
        "This action cannot be undone!",
        onConfirm: () => DeleteItem(itemId),
        onCancel: () => Debug.Log("Delete cancelled")
    );
    
    var popup = UIManager.Instance.Show<ConfirmationPopup>(data);
    popup.SetAnimation(new ScaleAnimation(0.2f));
}

private void DeleteItem(string itemId)
{
    // Delete logic
    InventoryService.DeleteItem(itemId);
    
    // Publish event
    EventBus.Instance.Publish(new ItemDeletedEvent(itemId));
    
    // Refresh inventory
    var inventory = UIManager.Instance.Get<InventoryScreen>();
    inventory?.Refresh();
}
```

### Pattern 2: Sequential Popups

```csharp
public async UniTask ShowRewardSequence(List<Reward> rewards)
{
    foreach (var reward in rewards)
    {
        var data = new RewardPopupData(reward.Type, reward.Amount);
        await UIManager.Instance.ShowAsync<RewardPopup>(data);
        
        // Wait for user to close
        await UniTask.WaitUntil(() => !UIManager.Instance.IsOpened<RewardPopup>());
    }
    
    Debug.Log("All rewards shown");
}
```

### Pattern 3: Parallel Popups

```csharp
public void ShowMultipleNotifications()
{
    // Pool handles multiple instances
    UIManager.Instance.PrewarmPool("NotificationPopup", 5);
    
    for (int i = 0; i < 5; i++)
    {
        var data = new NotificationData($"Message {i}");
        UIManager.Instance.Show<NotificationPopup>(data);
    }
    
    // All 5 notifications appear simultaneously
}
```

---

## Debugging Tips

### Enable Debug Logs

```csharp
UIFrameworkSettings:
  enableDebugLogs = true
  enablePerformanceMonitoring = true
  logMemoryUsage = true
```

### Use Debug Window

```
Window ? UIFramework ? UI Debug Window
- Monitor opened views in real-time
- Check memory usage
- Force hide/cache/pool actions
```

### Profile Memory

```csharp
void ProfileMemory()
{
    var metrics = UIPerformanceMonitor.Instance.GetAllMetrics();
    
    foreach (var kvp in metrics)
    {
        Debug.Log($"{kvp.Key}: {kvp.Value.MemoryUsage / 1024}KB, " +
                  $"Shows: {kvp.Value.ShowCount}, " +
                  $"Avg Load: {kvp.Value.AverageLoadTime:F3}s");
    }
}
```

---

## Migration from Existing UI

### Step 1: Wrap Existing UI

```csharp
// Your existing UI
public class OldShopScreen : MonoBehaviour
{
    // ... existing code
}

// Wrapper
public class ShopScreenWrapper : UIScreen
{
    [SerializeField] private OldShopScreen oldShop;
    
    protected override void OnInitialize(IUIData data)
    {
        base.OnInitialize(data);
        // Initialize old shop
    }
    
    protected override void OnShown()
    {
        oldShop.Show(); // Call old method
    }
}
```

### Step 2: Extract Controller Logic

```csharp
// Move logic from MonoBehaviour to Controller
// Old (BAD):
public class OldShopScreen : MonoBehaviour
{
    void OnBuyClicked()
    {
        // Business logic here ? BAD
    }
}

// New (GOOD):
public class ShopScreen : UIScreen
{
    void OnBuyClicked()
    {
        var controller = this.controller as ShopController;
        controller.OnBuyPressed();
    }
}

public class ShopController : UIControllerBase
{
    public void OnBuyPressed()
    {
        // Business logic here ? GOOD
    }
}
```

### Step 3: Gradual Migration

```
Phase 1: Wrap existing UIs ? Register in UIManager
Phase 2: Extract controllers ? Move logic out of MonoBehaviour
Phase 3: Implement event system ? Decouple UIs
Phase 4: Optimize ? Add pooling/caching
Phase 5: Full framework ? Use all features
```

---

## Troubleshooting

### "UI doesn't appear"

```
Check:
1. UIRegistry assigned to UIManager? ?
2. ViewId in registry matches class name? ?
3. Prefab assigned in config? ?
4. Prefab has UIBase component? ?
5. Console errors? Check logs
```

### "Animation doesn't play"

```
Check:
1. Animation injected via SetAnimation()? ?
2. CanvasGroup exists? (for fade) ?
3. RectTransform exists? (for slide) ?
4. Duration > 0? ?
```

### "Memory leak detected"

```
Check:
1. Events unbound in OnDispose()? ?
2. EventBus.Unsubscribe() called? ?
3. Addressable handles released? ?
4. No static references to UI? ?
5. Use Profiler to find leak source
```

### "Race condition / Double load"

```
Solution: Use ShowAsync() with CancellationToken
Framework handles race conditions automatically
```

---

## Best Practices Checklist

- [ ] All UI events bound in OnInitialize()
- [ ] All UI events unbound in OnDispose()
- [ ] Controllers handle business logic, not views
- [ ] Data is immutable or controlled mutation
- [ ] EventBus used for cross-UI communication
- [ ] No FindObjectOfType anywhere
- [ ] Pooling for frequent UIs (>10 shows/min)
- [ ] Caching for medium UIs (1-10 shows/min)
- [ ] No caching for rare UIs (<1 show/min)
- [ ] Async loading for large UIs (>5MB)
- [ ] Transitions injected, not hardcoded
- [ ] Registry validated before build
- [ ] Unit tests for controllers
- [ ] Performance profiled in long sessions

---

**You're Ready for Production! ??**

For more details:
- Architecture: ARCHITECTURE.md
- Memory: MEMORY_LIFECYCLE.md
- Flows: FLOWS.md
- Quick Start: QUICKSTART.md
