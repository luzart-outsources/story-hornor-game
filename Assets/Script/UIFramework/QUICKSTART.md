# UI Framework - Quick Start Guide

## Installation

### Step 1: Define Scripting Symbols (Optional)

Go to: `Edit ? Project Settings ? Player ? Scripting Define Symbols`

Add these symbols based on your project needs:

- **UNITASK_SUPPORT**: If you want async/await support with UniTask
  - Requires: UniTask package from OpenUPM or Asset Store
  
- **ADDRESSABLES_SUPPORT**: If you want to load UI via Addressables
  - Requires: Addressables package from Package Manager

### Step 2: Install Dependencies (Optional)

#### For UniTask (Async Support):
```
Option A: Package Manager ? Add package from git URL
  https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask

Option B: Unity Asset Store
  Search for "UniTask" and import
```

#### For Addressables:
```
Window ? Package Manager ? Unity Registry ? Addressables ? Install
```

**Note**: Framework works without these! They're optional for advanced features.

### Step 3: Create UI Registry

1. Right-click in Project window
2. `Create ? UIFramework ? UI Registry`
3. Name it `UIRegistry`
4. Save in `Assets/Resources/` or any folder

### Step 4: Setup UIManager in Scene

#### Option A: Automatic (Recommended)
The UIManager will auto-create itself on first access. Just call:
```csharp
UIManager.Instance.SetRegistry(yourRegistry);
```

#### Option B: Manual Setup
1. Create empty GameObject in scene: `[UIManager]`
2. Add `UIManager` component
3. Assign your `UIRegistry` in Inspector
4. The UIManager persists across scenes (DontDestroyOnLoad)

## Creating Your First UI

### Method 1: Using UI Creator Wizard (Recommended)

1. Open wizard: `Window ? UIFramework ? UI Creator Wizard`
2. Enter UI Name: "MainMenu"
3. Select Type: "Screen"
4. Check: Create Controller, Create Data, Add to Registry
5. Click "Create UI"

This generates:
- ? `MainMenu.cs` (View)
- ? `MainMenuController.cs` (Controller)
- ? `MainMenuData.cs` (Data/ViewModel)
- ? `MainMenu.prefab` (Prefab)

### Method 2: Manual Creation

#### Step 1: Create View Script

```csharp
using UnityEngine;
using UnityEngine.UI;
using UIFramework.Core;

public class MainMenuScreen : UIScreen
{
    [SerializeField] private Button playButton;
    [SerializeField] private Text titleText;
    
    protected override IUIController CreateController()
    {
        return new MainMenuController();
    }
    
    protected override void OnInitialize(IUIData data)
    {
        base.OnInitialize(data);
        
        playButton?.onClick.AddListener(OnPlayClicked);
        
        if (data is MainMenuData menuData)
        {
            titleText.text = menuData.Title;
        }
        
        // Optional: Inject animation
        SetAnimation(new UIFramework.Animations.ScaleAnimation(0.3f));
    }
    
    protected override void OnDispose()
    {
        playButton?.onClick.RemoveListener(OnPlayClicked);
        base.OnDispose();
    }
    
    private void OnPlayClicked()
    {
        var controller = this.controller as MainMenuController;
        controller?.OnPlayPressed();
    }
}
```

#### Step 2: Create Controller

```csharp
using UIFramework.Core;
using UIFramework.Communication;

public class MainMenuController : UIControllerBase
{
    public void OnPlayPressed()
    {
        UnityEngine.Debug.Log("Play pressed");
        
        // Event-based communication (decoupled)
        EventBus.Instance.Publish(new PlayGameEvent());
    }
}

// Define event
public class PlayGameEvent : IUIEvent { }
```

#### Step 3: Create Data/ViewModel

```csharp
using System;
using UIFramework.Core;

[Serializable]
public class MainMenuData : UIDataBase
{
    public string Title { get; private set; }
    
    public MainMenuData(string title)
    {
        Title = title;
    }
}
```

#### Step 4: Create Prefab

1. Create GameObject in scene
2. Add `MainMenuScreen` component
3. Add UI elements (Buttons, Text, etc.)
4. Assign references in Inspector
5. Drag to Project to create prefab
6. Delete from scene

#### Step 5: Register in UIRegistry

1. Open your UIRegistry asset
2. Add new config:
   - **ViewId**: "MainMenuScreen"
   - **Layer**: Screen
   - **LoadMode**: Prefab
   - **Prefab**: (assign your prefab)
   - **EnableCaching**: true (optional)

## Usage in Game Code

### Show UI

```csharp
using UIFramework.Managers;

public class GameStarter : MonoBehaviour
{
    private void Start()
    {
        // Show main menu
        var data = new MainMenuData("Welcome!");
        UIManager.Instance.Show<MainMenuScreen>(data);
    }
}
```

### Show Popup

```csharp
public void ShowQuitConfirmation()
{
    var data = new ConfirmationPopupData(
        "Quit Game",
        "Are you sure?",
        onConfirm: () => Application.Quit(),
        onCancel: () => Debug.Log("Cancelled")
    );
    
    var popup = UIManager.Instance.Show<ConfirmationPopup>(data);
    
    // Optional: Inject scale animation
    if (popup != null)
    {
        popup.SetAnimation(new UIFramework.Animations.ScaleAnimation());
    }
}
```

### Async Loading (with UniTask)

```csharp
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;

public async UniTask LoadMenuAsync()
{
    var cts = new CancellationTokenSource();
    
    var data = new MainMenuData("Welcome!");
    var menu = await UIManager.Instance.ShowAsync<MainMenuScreen>(data, cts.Token);
    
    if (menu != null)
    {
        Debug.Log("Menu loaded!");
    }
}
#endif
```

## Event Communication

### Subscribe to Events

```csharp
using UIFramework.Communication;

public class GameController : MonoBehaviour, IEventHandler<PlayGameEvent>
{
    private void Start()
    {
        EventBus.Instance.Subscribe<PlayGameEvent>(this);
    }
    
    private void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<PlayGameEvent>(this);
    }
    
    public void Handle(PlayGameEvent eventData)
    {
        Debug.Log("Starting game...");
        // Start game logic
    }
}
```

## Performance Optimization

### Enable Pooling for Frequent UI

```csharp
// In UIRegistry, set for DamageNumber popup:
enablePooling = true
poolSize = 10

// Prewarm pool at game start
UIManager.Instance.PrewarmPool("DamageNumberPopup", 10);

// Usage: Zero GC overhead!
for (int i = 0; i < 100; i++)
{
    UIManager.Instance.Show<DamageNumberPopup>(data);
}
```

### Enable Caching for Medium-Frequency UI

```csharp
// In UIRegistry:
enableCaching = true

// First show: Loads from addressable/prefab
UIManager.Instance.Show<SettingsScreen>();

// Hide: Kept in cache
UIManager.Instance.Hide<SettingsScreen>();

// Second show: Instant! Uses cached instance
UIManager.Instance.Show<SettingsScreen>();
```

## Debugging

### UI Debug Window

1. Open: `Window ? UIFramework ? UI Debug Window`
2. Play mode only
3. View:
   - Opened UIs and states
   - Memory usage
   - Pool status
4. Actions:
   - Hide All
   - Clear Cache
   - Clear Pool

### Log Opened Views

```csharp
UIManager.Instance.LogOpenedViews();

// Output:
// [UIManager] Opened Views: 2
//   - MainMenuScreen: Visible
//   - SettingsPopup: Showing
```

## Common Patterns

### Screen Navigation

```csharp
// Show new screen
UIManager.Instance.Show<GameplayScreen>();

// Go back to previous
UIManager.Instance.ShowPreviousScreen();

// Get current screen
var current = UIManager.Instance.GetCurrentScreen();
```

### Popup with Callback

```csharp
void AskForConfirmation()
{
    var data = new ConfirmationPopupData(
        "Delete Item",
        "This cannot be undone!",
        onConfirm: () => DeleteItem(),
        onCancel: () => Debug.Log("Cancelled")
    );
    
    UIManager.Instance.Show<ConfirmationPopup>(data);
}
```

### HUD Updates

```csharp
// Show HUD once
var hudData = new PlayerHudData(100, 100, 0, 1);
var hud = UIManager.Instance.Show<PlayerHud>(hudData);

// Update HUD data
void OnPlayerHealthChanged(int newHealth)
{
    var updatedData = hudData.WithHealth(newHealth, 100);
    hud.Initialize(updatedData);
    hud.Refresh();
}
```

## Testing

### Unit Test Controllers

```csharp
using NUnit.Framework;

[Test]
public void MainMenuController_OnPlayPressed_PublishesEvent()
{
    // Arrange
    var controller = new MainMenuController();
    var mockView = new MockUIView();
    var data = new MainMenuData("Test");
    var eventReceived = false;
    
    EventBus.Instance.Subscribe<PlayGameEvent>(
        new MockEventHandler<PlayGameEvent>(() => eventReceived = true)
    );
    
    controller.Initialize(mockView, data);
    
    // Act
    controller.OnPlayPressed();
    
    // Assert
    Assert.IsTrue(eventReceived);
}
```

## Troubleshooting

### Issue: UI doesn't show

**Check**:
1. UIRegistry assigned to UIManager?
2. ViewId registered in UIRegistry?
3. Prefab/Addressable path correct?
4. Console for errors?

### Issue: Animation doesn't play

**Check**:
1. Animation injected via `SetAnimation()`?
2. GameObject has CanvasGroup? (for fade)
3. GameObject has RectTransform? (for slide)

### Issue: Memory leak

**Check**:
1. Events unbound in OnDispose()?
2. EventBus uses proper Subscribe/Unsubscribe?
3. Controllers disposed?
4. Addressable handles released?

### Issue: UI loads twice (race condition)

**Solution**: Use `ShowAsync()` with proper cancellation handling (built-in)

## Next Steps

1. ? Create more screens/popups using wizard
2. ? Define your UI events in Events folder
3. ? Create custom animations/transitions
4. ? Setup Addressables (if needed)
5. ? Test with Debug Window
6. ? Profile memory in long sessions
7. ? Write unit tests for controllers

## Advanced Topics

### Custom Animation

```csharp
public class RotateAnimation : IUIAnimation
{
    public void PlayShowAnimation(GameObject target, System.Action onComplete)
    {
        // Your custom animation logic
        LeanTween.rotateZ(target, 360f, 0.5f).setOnComplete(() => onComplete?.Invoke());
    }
    
    // Implement other methods...
}

// Usage:
popup.SetAnimation(new RotateAnimation());
```

### Custom Loader

```csharp
public class CustomUILoader : IUILoader
{
    public GameObject Load(string address, Transform parent)
    {
        // Your custom loading logic
        // Could be from Resources, AssetBundle, etc.
    }
    
    // Implement other methods...
}

// Set custom loader
UIManager.Instance.SetLoader(new CustomUILoader());
```

## Support

For issues or questions, check:
- README.md (architecture overview)
- MEMORY_LIFECYCLE.md (memory management)
- FLOWS.md (detailed flow diagrams)
- Example files in Examples/ folder

---

**Happy Coding! ??**
