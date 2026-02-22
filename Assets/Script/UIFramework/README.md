# UI Framework Documentation

## Architecture Overview

The UI Framework is built on a layered architecture following SOLID principles and separation of concerns:

```
???????????????????????????????????????????????????????????
?                 PRESENTATION LAYER                       ?
?  UIScreen, UIPopup, UIHUD (MonoBehaviour Views)         ?
???????????????????????????????????????????????????????????
?                 CONTROLLER LAYER                         ?
?  UI Logic / Presenter / Controller                       ?
???????????????????????????????????????????????????????????
?                 VIEWMODEL LAYER                          ?
?  Data Binding, Immutable Data, State Management         ?
???????????????????????????????????????????????????????????
?                 SERVICE LAYER                            ?
?  UIManager, UILoader, EventBus, UIFactory               ?
???????????????????????????????????????????????????????????
?                 DOMAIN LAYER                             ?
?  Business Logic (No UI Dependencies)                     ?
???????????????????????????????????????????????????????????
```

## Folder Structure

```
Assets/Script/UIFramework/
??? Core/
?   ??? UIBase.cs                  # Base class for all UI
?   ??? UIScreen.cs                # Full screen UI
?   ??? UIPopup.cs                 # Popup/Dialog UI
?   ??? UIHUD.cs                   # HUD elements
?   ??? IUITransition.cs           # Transition interface
?   ??? Transitions/
?       ??? FadeTransition.cs      # Fade animation
?       ??? ScaleTransition.cs     # Scale animation
?       ??? SlideTransition.cs     # Slide animation
??? Manager/
?   ??? UIManager.cs               # Central UI management
?   ??? UIConfig.cs                # Configuration ScriptableObject
??? Loading/
?   ??? IUILoader.cs               # Loader interface
?   ??? PrefabUILoader.cs          # Resources-based loader
?   ??? AddressableUILoader.cs     # Addressables-based loader
??? Events/
?   ??? EventBus.cs                # Event communication system
??? MVVM/
?   ??? ViewModelBase.cs           # Base ViewModel
?   ??? UIView.cs                  # MVVM View base class
??? Examples/
?   ??? MainMenuScreen.cs          # Example screen
?   ??? RewardPopup.cs             # Example popup
??? Editor/
    ??? UIConfigEditor.cs          # Config inspector
    ??? UICreatorWindow.cs         # UI creation tool
    ??? UIDebugWindow.cs           # Runtime debug tool
```

## Core Components

### UIBase
Abstract base class for all UI elements. Provides:
- Lifecycle methods: Initialize, Show, Hide, Refresh, Dispose
- State management: Hidden, Showing, Visible, Hiding
- Transition support via strategy pattern
- Optional async support with UniTask

### UIScreen
Inherits from UIBase. Full-screen UI with:
- Single active screen policy (configurable)
- Automatic hiding of other screens

### UIPopup
Inherits from UIBase. Popup/Dialog UI with:
- Stack management
- Modal/Non-modal support
- Background blocker
- Close on background click option

### UIHUD
Inherits from UIBase. Always-visible HUD elements.

## UIManager

Central manager for all UI operations:

### API Methods

```csharp
// Show UI (async with UniTask)
await UIManager.Instance.ShowAsync<MainMenuScreen>(viewModel, transition);

// Show UI (sync)
UIManager.Instance.Show<MainMenuScreen>(viewModel, transition);

// Hide UI
await UIManager.Instance.HideAsync<MainMenuScreen>();
UIManager.Instance.Hide<MainMenuScreen>();

// Get active UI instance
var menu = UIManager.Instance.Get<MainMenuScreen>();

// Check if UI is opened
bool isOpen = UIManager.Instance.IsOpened<MainMenuScreen>();

// Hide all UIs
await UIManager.Instance.HideAllAsync();
UIManager.Instance.HideAll();

// Hide all except specific types
await UIManager.Instance.HideAllIgnoreAsync(new[] { typeof(UIHUD) });
UIManager.Instance.HideAllIgnore(typeof(UIHUD));

// Popup stack management
var topPopup = UIManager.Instance.GetTopPopup();
int stackCount = UIManager.Instance.GetPopupStackCount();
```

## Loading System

### PrefabUILoader
Loads UI from Resources folder:
```csharp
var loader = new PrefabUILoader(useCaching: true);
```

### AddressableUILoader
Loads UI from Addressables:
```csharp
var loader = new AddressableUILoader(useCaching: true);
```

Features:
- Async loading with cancellation support
- Memory tracking
- Preloading support
- Auto-release on close
- Cache management
- Race condition handling

## MVVM Pattern

### ViewModel Example
```csharp
[Serializable]
public class MainMenuViewModel : ViewModelBase
{
    private string _playerName;
    
    public string PlayerName
    {
        get => _playerName;
        set
        {
            if (_playerName != value)
            {
                _playerName = value;
                NotifyPropertyChanged(); // Triggers UI update
            }
        }
    }
}
```

### View Example
```csharp
public class MainMenuScreen : UIView<MainMenuViewModel>
{
    [SerializeField] private Text _playerNameText;
    
    protected override void OnViewModelChanged()
    {
        if (ViewModel == null) return;
        _playerNameText.text = ViewModel.PlayerName;
    }
}
```

## Event System

Decoupled communication using EventBus:

```csharp
// Subscribe to events
EventBus.Instance.Subscribe<UIOpenedEvent>(OnUIOpened);

// Publish events
EventBus.Instance.Publish(new UIOpenedEvent 
{ 
    UIType = typeof(MainMenuScreen), 
    Data = viewModel 
});

// Unsubscribe
EventBus.Instance.Unsubscribe<UIOpenedEvent>(OnUIOpened);
```

Built-in Events:
- `UIOpenedEvent` - UI was opened
- `UIClosedEvent` - UI was closed
- `UIScreenChangedEvent` - Screen switched
- `UIPopupStackChangedEvent` - Popup stack changed

Features:
- Weak references (no memory leaks)
- Thread-safe
- Auto cleanup of dead references
- Type-safe

## Transition System

Injectable animation strategies:

```csharp
// Fade transition
var fadeTransition = new FadeTransition(duration: 0.3f);
await UIManager.Instance.ShowAsync<MainMenuScreen>(null, fadeTransition);

// Scale transition
var scaleTransition = new ScaleTransition(
    duration: 0.3f,
    startScale: new Vector3(0.5f, 0.5f, 1f)
);

// Slide transition
var slideTransition = new SlideTransition(
    duration: 0.3f,
    direction: SlideTransition.Direction.Bottom
);
```

## Memory Lifecycle

### Opening a Popup Flow
```
1. User calls UIManager.ShowAsync<RewardPopup>(data)
2. UIManager checks if popup is already loading (race condition)
3. UIManager checks if popup is already active
4. UIManager loads prefab via IUILoader
   - If cached: reuse instance
   - If not: load from Resources/Addressables
5. UIManager initializes popup with data
6. UIManager adds to active UIs dictionary
7. UIManager pushes to popup stack
8. Popup.ShowAsync() called with optional transition
9. EventBus publishes UIOpenedEvent
```

### Closing a Popup Flow
```
1. User calls UIManager.HideAsync<RewardPopup>()
2. UIManager cancels any pending load operations
3. UIManager retrieves popup from active UIs
4. Popup.HideAsync() called with optional transition
5. UIManager removes from popup stack
6. UIManager removes from active UIs
7. If caching disabled:
   - Popup.Dispose() called
   - IUILoader.Release() called
   - GameObject destroyed
8. If caching enabled:
   - Popup moved to cache dictionary
9. EventBus publishes UIClosedEvent
```

### Screen Switching Flow
```
1. User calls UIManager.ShowAsync<GameScreen>(data)
2. UIManager loads/retrieves GameScreen
3. GameScreen.Initialize(data) called
4. If AllowMultipleInstances = false:
   - UIManager hides all other screens
5. GameScreen.ShowAsync() called
6. EventBus publishes UIScreenChangedEvent
```

## Memory Safety

### No Memory Leaks
- Weak references in EventBus
- Proper cleanup in Dispose()
- CancellationToken support for async operations
- Auto-release of Addressables handles

### No Excessive References
- Views don't reference other views
- Communication via EventBus
- ViewModel pattern separates data from view
- UIManager uses Type keys, not GameObject references

### GC Friendly
- Object pooling support for frequent popups
- No lambda allocations in Update()
- Cached dictionaries for lookups
- StringBuilder for string operations

## Editor Tools

### UI Creator Window
**Menu: Window > UI Framework > UI Creator**

Features:
- Auto-generate UI scripts (Screen/Popup/HUD)
- Auto-generate ViewModel scripts
- Auto-register to UIConfig
- Customizable templates

### UI Debug Window
**Menu: Window > UI Framework > Debug Window**

Features:
- View active UIs in real-time
- Monitor popup stack
- Track memory usage
- Quick actions (Hide All, Clear EventBus)

### UI Config Inspector
Custom inspector for UIConfig with:
- Validate registry (check duplicates)
- Clear all entries
- UI element count

## Configuration

### Create UI Config
1. Right-click in Project
2. Create > UIFramework > UI Config
3. Configure settings:
   - Use Addressables: true/false
   - Use Caching: true/false
4. Add UI elements manually or use UI Creator

### Register UI
```csharp
UIManager.Instance.RegisterUI(typeof(MainMenuScreen), "UI/MainMenuScreen");
```

Or add to UIConfig ScriptableObject.

## Testing

### Unit Test Example
```csharp
[Test]
public void ViewModel_PropertyChanged_NotifiesObservers()
{
    var viewModel = new MainMenuViewModel("Player", 1, 100);
    bool notified = false;
    
    viewModel.OnDataChanged += () => notified = true;
    viewModel.PlayerName = "NewName";
    
    Assert.IsTrue(notified);
    Assert.AreEqual("NewName", viewModel.PlayerName);
}
```

### Mock Services
```csharp
public class MockUILoader : IUILoader
{
    public UniTask<T> LoadAsync<T>(string address, Transform parent, CancellationToken ct) 
    {
        // Return mock UI
    }
}

// Inject mock
UIManager.Instance.SetLoader(new MockUILoader());
```

## Performance Best Practices

1. **Use Object Pooling** for frequently shown/hidden popups
2. **Preload** common UIs during loading screens
3. **Use Addressables** for large projects
4. **Enable Caching** for better performance
5. **Batch Canvas Updates** - group UI elements by canvas
6. **Avoid Update()** - use events instead
7. **Use UniTask** for async operations (better than coroutines)

## Safety Features

### Scene Reload
- DontDestroyOnLoad on UIManager
- Auto-cleanup on scene unload
- Re-initialization support

### Domain Reload
- Static cleanup in OnDestroy
- EventBus survives domain reload
- Config persists as ScriptableObject

### Mobile Pause/Resume
- OnApplicationPause cleanup
- EventBus dead reference cleanup
- Memory optimization

## Usage Examples

### Basic Screen
```csharp
// Create ViewModel
var viewModel = new MainMenuViewModel("Player", 5, 1000);

// Show screen
await UIManager.Instance.ShowAsync<MainMenuScreen>(viewModel);

// Hide screen
await UIManager.Instance.HideAsync<MainMenuScreen>();
```

### Popup with Callback
```csharp
var popupData = new RewardPopupData(
    "Daily Reward",
    "You received daily rewards!",
    gold: 500,
    gems: 50,
    onClaim: () => {
        Debug.Log("Reward claimed!");
        // Add gold/gems to player
    }
);

await UIManager.Instance.ShowAsync<RewardPopup>(
    popupData,
    new ScaleTransition(0.3f)
);
```

### With Events
```csharp
void Start()
{
    EventBus.Instance.Subscribe<UIOpenedEvent>(OnAnyUIOpened);
}

void OnAnyUIOpened(UIOpenedEvent evt)
{
    Debug.Log($"UI Opened: {evt.UIType.Name}");
}

void OnDestroy()
{
    EventBus.Instance.Unsubscribe<UIOpenedEvent>(OnAnyUIOpened);
}
```

## Multiplayer Considerations

- No static state dependencies
- Instance-based UIManager
- EventBus per instance
- Support for multiple UI roots
- Configurable via ScriptableObject

## Extension Points

### Custom Transitions
```csharp
public class CustomTransition : IUITransition
{
    public async UniTask ShowAsync(UIBase ui, CancellationToken ct)
    {
        // Custom show animation
    }
    
    public async UniTask HideAsync(UIBase ui, CancellationToken ct)
    {
        // Custom hide animation
    }
}
```

### Custom Loaders
```csharp
public class CustomLoader : IUILoader
{
    // Implement IUILoader interface
}
```

### Custom Events
```csharp
public class CustomUIEvent : IUIEvent
{
    public string CustomData { get; set; }
}

EventBus.Instance.Publish(new CustomUIEvent { CustomData = "test" });
```

## Troubleshooting

### UI not showing
- Check if address is registered in UIConfig
- Verify prefab path in Resources or Addressables
- Check layer hierarchy in UIManager

### Memory leak
- Ensure Dispose() is called
- Check for circular references
- Use Weak References in custom code
- Unsubscribe from events in OnDestroy

### Race condition
- Use CancellationToken properly
- Check _loadingOperations in UIManager
- Avoid rapid show/hide calls

## Requirements Met

? Scalable (50+ UIs supported)
? Modular & Extensible
? Decoupled architecture
? Addressable compatible
? Memory safe
? Testable
? Editor-friendly
? Multiplayer-safe
? MVVM pattern
? EventBus communication
? Injectable strategies
? UniTask support
? Performance optimized
? Production-ready code
