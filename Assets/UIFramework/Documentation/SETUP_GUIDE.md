# UI Framework Setup Guide

## ?? Initial Setup (5 Minutes)

### Step 1: Create UI Root
1. In Unity Scene Hierarchy: Right-click ? UI ? Canvas
2. Rename to "UIRoot"
3. Configure Canvas:
   - Render Mode: Screen Space - Overlay
   - Pixel Perfect: Unchecked (for performance)

4. Configure Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
   - Match: 0.5 (balanced)

### Step 2: Add UIManager Component
1. Select UIRoot GameObject
2. Add Component ? UIManager (Luzart.UIFramework.UIManager)
3. Assign Root Canvas field (drag UIRoot's Canvas)

### Step 3: Create UIRegistry
1. In Project window: Right-click ? Create ? Luzart ? UI Framework ? UI Registry
2. Save as: Assets/Resources/UIRegistry.asset
3. Select UIRoot ? UIManager component
4. Assign UIRegistry to Registry field

### Step 4: Verify Event System
- Scene should have EventSystem GameObject
- If not, Right-click Hierarchy ? UI ? Event System

**? Basic setup complete!**

---

## ?? Creating Your First UI (10 Minutes)

### Example: Creating a Main Menu Screen

#### Step 1: Create View Script

```csharp
// File: Assets/UI/Screens/MainMenu/MainMenuScreen.cs
using Luzart.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.UI
{
    public class MainMenuScreen : UIScreen
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;

        private MainMenuController controller;

        protected override void Awake()
        {
            base.Awake();
            playButton?.onClick.AddListener(OnPlayClicked);
            settingsButton?.onClick.AddListener(OnSettingsClicked);
        }

        protected override void OnDestroy()
        {
            playButton?.onClick.RemoveListener(OnPlayClicked);
            settingsButton?.onClick.RemoveListener(OnSettingsClicked);
            base.OnDestroy();
        }

        protected override void OnInitialize(object data)
        {
            base.OnInitialize(data);

            var viewModel = new MainMenuViewModel();
            controller = new MainMenuController();
            controller.Setup(this, viewModel, UIManager.Instance.EventBus);
            controller.Initialize();

            Refresh();
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
            if (controller?.ViewModel != null && titleText != null)
            {
                titleText.text = controller.ViewModel.Title;
            }
        }

        protected override void OnDispose()
        {
            controller?.Dispose();
            controller = null;
            base.OnDispose();
        }

        private void OnPlayClicked() => controller?.OnPlayClicked();
        private void OnSettingsClicked() => controller?.OnSettingsClicked();
    }
}
```

#### Step 2: Create ViewModel

```csharp
// File: Assets/UI/Screens/MainMenu/MainMenuViewModel.cs
using Luzart.UIFramework;

namespace MyGame.UI
{
    public class MainMenuViewModel : UIViewModel
    {
        private string title = "Main Menu";

        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    NotifyDataChanged();
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            title = "Main Menu";
        }
    }
}
```

#### Step 3: Create Controller

```csharp
// File: Assets/UI/Screens/MainMenu/MainMenuController.cs
using Luzart.UIFramework;
using UnityEngine;

namespace MyGame.UI
{
    public class MainMenuController : UIController<MainMenuScreen, MainMenuViewModel>
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
            ViewModel.Title = "Welcome to My Game!";
        }

        public void OnPlayClicked()
        {
            Debug.Log("Play button clicked");
            // Hide main menu and load game
            UIManager.Instance.Hide<MainMenuScreen>();
        }

        public void OnSettingsClicked()
        {
            Debug.Log("Settings button clicked");
            // Show settings popup
            UIManager.Instance.ShowAsync<SettingsPopup>();
        }
    }
}
```

#### Step 4: Create Prefab

1. **Hierarchy**: Right-click ? UI ? Panel
2. **Rename** to "MainMenuScreen"
3. **Add Component**: MainMenuScreen script
4. **Configure**:
   - ViewId field: "MainMenuScreen" (must match class name)
   - Layer field: Screen

5. **Add CanvasGroup** component (required for transitions):
   - Alpha: 1
   - Interactable: ?
   - Block Raycasts: ?

6. **Add UI Elements**:
   ```
   MainMenuScreen (Panel)
   ?? TitleText (Text)
   ?? PlayButton (Button)
   ?  ?? Text
   ?? SettingsButton (Button)
      ?? Text
   ```

7. **Assign References**:
   - Drag TitleText to MainMenuScreen ? Title Text field
   - Drag PlayButton to MainMenuScreen ? Play Button field
   - Drag SettingsButton to MainMenuScreen ? Settings Button field

8. **Save as Prefab**:
   - Drag MainMenuScreen to Project window
   - Save to: Assets/UI/Screens/MainMenu/MainMenuScreen.prefab
   - Delete from Hierarchy

#### Step 5: Register in UIRegistry

1. Open UIRegistry asset (Assets/Resources/UIRegistry.asset)
2. Increase "Entries" size by 1
3. Configure new entry:
   ```
   ViewId: MainMenuScreen
   Layer: Screen
   Load Mode: Direct
   Address Or Path: (leave empty for Direct mode)
   Prefab: [Drag MainMenuScreen.prefab here]
   Transition Type: Fade
   Enable Pooling: false (screens usually not pooled)
   Pool Size: 1
   ```
4. Click "Validate Entries" button
5. Save

#### Step 6: Use in Game

```csharp
// File: Assets/Scripts/GameManager.cs
using UnityEngine;
using Luzart.UIFramework;
using MyGame.UI;

public class GameManager : MonoBehaviour
{
    async void Start()
    {
        await UIManager.Instance.ShowAsync<MainMenuScreen>();
    }
}
```

**? Your first screen is ready!**

---

## ?? Creating a Popup (5 Minutes)

### Quick Setup for Settings Popup

#### 1. Create Prefab Structure
```
SettingsPopup (Panel with CanvasGroup)
?? Blocker (Image - black, alpha 0.7) [For modal behavior]
?? Content (Panel)
?  ?? TitleText
?  ?? MusicSlider
?  ?? SfxSlider
?  ?? ApplyButton
?  ?? CloseButton
```

#### 2. Add SettingsPopup Component
- ViewId: "SettingsPopup"
- Layer: Popup
- Popup Mode: Modal
- Assign Blocker GameObject

#### 3. Register in UIRegistry
```
ViewId: SettingsPopup
Layer: Popup
Load Mode: Direct
Prefab: [Assign prefab]
Transition Type: Scale
Enable Pooling: true (opened frequently)
Pool Size: 2
```

#### 4. Show Popup
```csharp
var data = new SettingsData
{
    MusicVolume = 0.8f,
    SfxVolume = 0.6f,
    IsFullscreen = true
};

await UIManager.Instance.ShowAsync<SettingsPopup>(data);
```

---

## ??? Editor Workflow

### Creating UI Template (Fast Way)
1. Assets ? Create ? Luzart ? UI Framework ? Create UI Screen Template
2. Enter name (e.g., "InventoryScreen")
3. Auto-generates: View + ViewModel + Controller in one file
4. Split into separate files if preferred

### Validating Registry
1. Select UIRegistry asset
2. Inspector ? "Validate Entries" button
3. Check console for warnings/errors
4. Fix any duplicate ViewIds or missing references

### Debug Window (Runtime)
1. Enter Play Mode
2. Luzart ? UI Framework ? Debug Window
3. See all active UIs in real-time
4. Hide/Select any UI with one click

---

## ?? Mobile Setup (Additional)

### Safe Area Support
```csharp
public class UISafeAreaAdapter : MonoBehaviour
{
    void Start()
    {
        var safeArea = Screen.safeArea;
        var rectTransform = GetComponent<RectTransform>();
        
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}
```

### Handle Pause/Resume
```csharp
void OnApplicationPause(bool pauseStatus)
{
    if (pauseStatus)
    {
        // Game paused - save UI state if needed
        SaveUIState();
    }
    else
    {
        // Game resumed
        RestoreUIState();
    }
}
```

---

## ?? Service Integration

### Register Services at Game Start
```csharp
void Start()
{
    var uiManager = UIManager.Instance;
    
    // Register domain services
    uiManager.Context.RegisterService<IPlayerService>(new PlayerService());
    uiManager.Context.RegisterService<IAudioService>(new AudioService());
    uiManager.Context.RegisterService<ISaveService>(new SaveService());
    uiManager.Context.RegisterService<IInventoryService>(new InventoryService());
}
```

### Use Services in Controllers
```csharp
public class ShopController : UIController<ShopScreen, ShopViewModel>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        
        var playerService = Context.GetService<IPlayerService>();
        ViewModel.PlayerCoins = playerService.GetCoins();
    }

    public void OnItemPurchased(int itemId, int price)
    {
        var playerService = Context.GetService<IPlayerService>();
        if (playerService.SpendCoins(price))
        {
            var inventoryService = Context.GetService<IInventoryService>();
            inventoryService.AddItem(itemId);
            
            EventBus.Publish(new ItemPurchasedEvent(itemId));
        }
    }
}
```

---

## ?? Common Patterns

### Loading Screen Pattern
```csharp
async Task LoadLevel(string levelName)
{
    // Show loading
    await UIManager.Instance.ShowAsync<LoadingScreen>();
    
    // Load level
    await SceneManager.LoadSceneAsync(levelName);
    
    // Initialize level
    await InitializeLevelAsync();
    
    // Hide loading
    UIManager.Instance.Hide<LoadingScreen>();
    
    // Show gameplay HUD
    await UIManager.Instance.ShowAsync<GameplayHUD>();
}
```

### Pause Menu Pattern
```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (UIManager.Instance.IsOpened<PauseMenu>())
        {
            UIManager.Instance.Hide<PauseMenu>();
            Time.timeScale = 1f;
        }
        else
        {
            UIManager.Instance.ShowAsync<PauseMenu>();
            Time.timeScale = 0f;
        }
    }
}
```

### Confirmation Dialog Pattern
```csharp
async Task<bool> ShowConfirmation(string message)
{
    var data = new ConfirmationData { Message = message };
    var popup = await UIManager.Instance.ShowAsync<ConfirmationPopup>(data);
    
    var tcs = new TaskCompletionSource<bool>();
    popup.OnResult = result => tcs.SetResult(result);
    
    return await tcs.Task;
}

// Usage:
if (await ShowConfirmation("Delete save file?"))
{
    DeleteSaveFile();
}
```

### Reward Popup Pattern
```csharp
void OnQuestCompleted(int rewardAmount)
{
    var data = new RewardData
    {
        Title = "Quest Complete!",
        Amount = rewardAmount,
        Currency = "Gold"
    };
    
    UIManager.Instance.ShowAsync<RewardPopup>(data);
    
    // Popup will auto-close after claim button clicked
}
```

---

## ?? Registry Configuration Guide

### Screen Example
```
ViewId: MainMenuScreen
Layer: Screen
LoadMode: Direct
AddressOrPath: (empty)
Prefab: MainMenuScreen.prefab
TransitionType: Fade
EnablePooling: false (screens rarely pooled)
PoolSize: 1
```

### Popup Example (Frequent)
```
ViewId: RewardPopup
Layer: Popup
LoadMode: Direct
AddressOrPath: (empty)
Prefab: RewardPopup.prefab
TransitionType: Scale
EnablePooling: true (shown frequently)
PoolSize: 3 (prewarm 3 instances)
```

### HUD Example
```
ViewId: PlayerHUD
Layer: HUD
LoadMode: Direct
AddressOrPath: (empty)
Prefab: PlayerHUD.prefab
TransitionType: None (instant)
EnablePooling: false
PoolSize: 1
```

### Addressable Example
```
ViewId: ShopScreen
Layer: Screen
LoadMode: Addressable
AddressOrPath: UI/Screens/ShopScreen (Addressable address)
Prefab: (leave null)
TransitionType: Slide
EnablePooling: false
PoolSize: 1
```

---

## ?? Addressables Setup (Optional)

### 1. Install Addressables Package
- Window ? Package Manager
- Search "Addressables"
- Install

### 2. Mark Assets as Addressable
1. Select UI prefab
2. Inspector ? Addressables checkbox
3. Set address (e.g., "UI/Screens/MainMenu")

### 3. Configure UIManager
1. Select UIRoot
2. UIManager component
3. Check "Use Addressables" toggle

### 4. Update Registry Entries
- LoadMode: Addressable
- AddressOrPath: "UI/Screens/MainMenu"
- Prefab: (leave null)

### 5. Build Addressables
- Window ? Asset Management ? Addressables ? Groups
- Build ? New Build ? Default Build Script

**Benefits:**
- Remote loading (reduce build size)
- Hot update UI without app update
- Faster iteration

---

## ?? Transition Configuration

### Available Transitions

| Type | Description | Use Case |
|------|-------------|----------|
| None | Instant (no animation) | HUD, loading screens |
| Fade | Alpha 0?1 or 1?0 | Smooth general purpose |
| Slide | Move from off-screen | Panels, side menus |
| Scale | Scale 0.8?1 or 1?0.8 | Popups, rewards |
| Custom | Your implementation | Special effects |

### Custom Transition Example
```csharp
// Create custom transition
public class BounceTransition : IUITransition
{
    public async void PlayShowTransition(RectTransform target, Action onComplete, CancellationToken ct)
    {
        // Implement bounce animation using DOTween, Animation, or manual
        await BounceSqueeze(target, ct);
        onComplete?.Invoke();
    }

    public async void PlayHideTransition(RectTransform target, Action onComplete, CancellationToken ct)
    {
        await BounceSqueeze(target, ct);
        onComplete?.Invoke();
    }
}

// Use:
var bounce = new BounceTransition();
await UIManager.Instance.ShowAsync<MyPopup>(null, bounce);
```

### Composite Transition Example
```csharp
// Combine fade + scale
var composite = new UICompositeTransition(
    new UIFadeTransition(0.3f),
    new UIScaleTransition(new Vector3(0.8f, 0.8f, 1f), 0.3f)
);

await UIManager.Instance.ShowAsync<MyScreen>(null, composite);
```

---

## ?? Advanced Patterns

### Pattern 1: Preloading System
```csharp
public class PreloadManager : MonoBehaviour
{
    async void Start()
    {
        // Show loading screen
        await UIManager.Instance.ShowAsync<LoadingScreen>();

        // Preload essential UIs
        var tasks = new[]
        {
            UIManager.Instance.ShowAsync<PlayerHUD>(),
            UIManager.Instance.ShowAsync<PauseMenu>(),
            UIManager.Instance.ShowAsync<SettingsPopup>()
        };

        await System.Threading.Tasks.Task.WhenAll(tasks);

        // Hide non-visible UIs (they're cached now)
        UIManager.Instance.Hide<PauseMenu>();
        UIManager.Instance.Hide<SettingsPopup>();

        // Hide loading
        UIManager.Instance.Hide<LoadingScreen>();

        // Show main menu (instant, no load time)
        await UIManager.Instance.ShowAsync<MainMenuScreen>();
    }
}
```

### Pattern 2: Multi-Step Wizard
```csharp
public class WizardController : MonoBehaviour
{
    private int currentStep = 0;

    public async void ShowNextStep()
    {
        currentStep++;

        switch (currentStep)
        {
            case 1:
                await UIManager.Instance.ShowAsync<WizardStep1>();
                break;
            case 2:
                UIManager.Instance.Hide<WizardStep1>();
                await UIManager.Instance.ShowAsync<WizardStep2>();
                break;
            case 3:
                UIManager.Instance.Hide<WizardStep2>();
                await UIManager.Instance.ShowAsync<WizardStep3>();
                break;
        }
    }

    public void GoBackStep()
    {
        UIManager.Instance.GoBack();
        currentStep--;
    }
}
```

### Pattern 3: Dynamic Content
```csharp
public class InventoryController : UIController<InventoryScreen, InventoryViewModel>
{
    protected override void SubscribeToEvents()
    {
        base.SubscribeToEvents();
        EventBus.Subscribe<ItemAddedEvent>(OnItemAdded);
        EventBus.Subscribe<ItemRemovedEvent>(OnItemRemoved);
    }

    private void OnItemAdded(ItemAddedEvent evt)
    {
        ViewModel.AddItem(evt.ItemId, evt.Count);
    }

    private void OnItemRemoved(ItemRemovedEvent evt)
    {
        ViewModel.RemoveItem(evt.ItemId, evt.Count);
    }
}
```

---

## ?? Common Issues & Solutions

### Issue 1: UI Doesn't Show
**Symptoms**: ShowAsync completes but nothing visible

**Solutions**:
1. Check UIRegistry has entry with matching ViewId
2. Verify prefab assigned (Direct mode) or address set (Addressable mode)
3. Check Canvas is active in scene
4. Verify CanvasGroup alpha = 1
5. Check console for error messages

### Issue 2: Transition Not Playing
**Symptoms**: UI appears instantly without animation

**Solutions**:
1. Add CanvasGroup component to UI root
2. Verify TransitionType is not "None"
3. Check transition duration > 0
4. Ensure UIBase.SetTransition() is called

### Issue 3: Memory Leaks
**Symptoms**: Memory usage grows over time

**Solutions**:
1. Ensure Dispose() is called in OnDispose()
2. Dispose EventBus subscriptions in UnsubscribeFromEvents()
3. Call ViewModel.Reset() to clear event handlers
4. Use Profiler ? Memory ? Take Snapshot
5. Look for accumulating UIBase instances

### Issue 4: Reference Not Set
**Symptoms**: NullReferenceException on UI elements

**Solutions**:
1. Assign all [SerializeField] references in prefab
2. Check references in Awake(), not constructor
3. Null-check before accessing: `if (button != null)`

### Issue 5: EventBus Not Working
**Symptoms**: Events published but not received

**Solutions**:
1. Ensure Subscribe() in SubscribeToEvents()
2. Ensure Dispose() in UnsubscribeFromEvents()
3. Check event type matches exactly
4. Verify EventBus passed to Controller.Setup()

---

## ?? Performance Tuning

### For Large Projects (50+ UIs)

#### 1. Enable Pooling for Frequent Popups
```
RewardPopup ? EnablePooling = true, PoolSize = 5
DailyBonusPopup ? EnablePooling = true, PoolSize = 2
LevelUpPopup ? EnablePooling = true, PoolSize = 3
```

#### 2. Use Addressables for Large Screens
```
ShopScreen (10MB textures) ? LoadMode = Addressable
CharacterScreen (50 models) ? LoadMode = Addressable
```

#### 3. Aggressive Unloading
```csharp
void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // Unload UIs from previous scene
    UIManager.Instance.HideAll();
    Resources.UnloadUnusedAssets();
}
```

#### 4. Layer Optimization
```
HUD Layer: Keep minimal (5-10 elements)
Screen Layer: Only 1 active at a time
Popup Layer: Stack max 3-5 simultaneously
```

---

## ?? Learning Path

### Beginner (Day 1)
1. ? Complete Initial Setup
2. ? Create first screen with template
3. ? Show/Hide using UIManager
4. ? Add button click handlers

### Intermediate (Week 1)
1. ? Create ViewModel with properties
2. ? Implement Controller logic
3. ? Use EventBus for communication
4. ? Create custom transitions

### Advanced (Month 1)
1. ? Implement Addressables loading
2. ? Setup object pooling
3. ? Write unit tests
4. ? Optimize memory usage
5. ? Create custom loaders

---

## ?? Deliverables Checklist

- [x] High-level architecture diagram
- [x] Complete folder structure
- [x] Base class implementation (UIBase, UIScreen, UIPopup, UIHud)
- [x] UIManager implementation
- [x] UILayerManager implementation
- [x] UIStackManager implementation
- [x] Addressable loader implementation (optional)
- [x] Direct prefab loader implementation
- [x] Example screen (MainMenuScreen)
- [x] Example popup (SettingsPopup)
- [x] EventBus implementation
- [x] UIContext (DI container)
- [x] Transition system (Fade, Slide, Scale, Composite)
- [x] Object pooling system
- [x] UIRegistry (ScriptableObject)
- [x] Editor tools (Debug window, template generator, validator)
- [x] Memory lifecycle explanation
- [x] Flow diagrams (opening popup, switching screens)
- [x] Unit test examples
- [x] Integration test examples
- [x] Complete documentation

---

## ?? You're Ready!

Start building your UI system:
1. ? Run setup menu
2. ? Create your first screen
3. ? Register in UIRegistry
4. ? Call ShowAsync()
5. ? Ship it! ??

For complete documentation, see:
- **README.md**: Overview
- **QUICKSTART.md**: Tutorial
- **ARCHITECTURE.md**: Deep dive
- **MEMORY_LIFECYCLE.md**: Flow diagrams

**Questions?** Check Documentation folder or use Debug Window!
