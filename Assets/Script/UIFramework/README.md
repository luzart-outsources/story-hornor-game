# UI Framework Documentation

## Architecture Overview

```
???????????????????????????????????????????????????????????
?                    PRESENTATION LAYER                    ?
???????????????????????????????????????????????????????????
?  UIView (MonoBehaviour) ? IUITransition ? IUIAnimation  ?
?           ?                                              ?
?  UIController (Logic) ? IInputStrategy                   ?
?           ?                                              ?
?  ViewModel (Data)                                        ?
???????????????????????????????????????????????????????????
                          ?
???????????????????????????????????????????????????????????
?                   UI SERVICE LAYER                       ?
???????????????????????????????????????????????????????????
?  UIManager ? UIRegistry (ScriptableObject)              ?
?     ?                                                    ?
?  IUILoader (Addressable/Prefab)                         ?
?  UIPool (Pooling)                                       ?
?  UILayerManager (HUD/Screen/Popup/Overlay)              ?
???????????????????????????????????????????????????????????
                          ?
???????????????????????????????????????????????????????????
?                 COMMUNICATION LAYER                      ?
???????????????????????????????????????????????????????????
?  EventBus / MessageBroker                               ?
?  UICommand System                                        ?
???????????????????????????????????????????????????????????
                          ?
???????????????????????????????????????????????????????????
?                    DOMAIN LAYER                          ?
???????????????????????????????????????????????????????????
?  Business Logic (Game Services)                         ?
?  Data Models                                             ?
???????????????????????????????????????????????????????????
```

## Folder Structure

```
Assets/
??? Script/
?   ??? UIFramework/
?       ??? Core/
?       ?   ??? Base/
?       ?   ?   ??? UIBase.cs
?       ?   ?   ??? UIControllerBase.cs
?       ?   ?   ??? UIDataBase.cs
?       ?   ?   ??? UIScreen.cs
?       ?   ?   ??? UIPopup.cs
?       ?   ?   ??? UIHud.cs
?       ?   ??? Interfaces/
?       ?   ?   ??? IUIView.cs
?       ?   ?   ??? IUIController.cs
?       ?   ?   ??? IUIData.cs
?       ?   ?   ??? IUITransition.cs
?       ?   ?   ??? IUIAnimation.cs
?       ?   ?   ??? IInputStrategy.cs
?       ?   ?   ??? IUILoader.cs
?       ?   ??? Enums/
?       ?       ??? UILayer.cs
?       ?       ??? UIState.cs
?       ??? Managers/
?       ?   ??? UIManager.cs
?       ?   ??? UILayerManager.cs
?       ??? Loaders/
?       ?   ??? PrefabUILoader.cs
?       ?   ??? AddressableUILoader.cs
?       ??? Data/
?       ?   ??? UIRegistry.cs
?       ??? Pooling/
?       ?   ??? UIPool.cs
?       ??? Communication/
?       ?   ??? EventBus.cs
?       ?   ??? UICommandInvoker.cs
?       ??? Animations/
?       ?   ??? FadeTransition.cs
?       ?   ??? ScaleAnimation.cs
?       ?   ??? SlideTransition.cs
?       ??? Examples/
?       ?   ??? MainMenuScreen.cs
?       ?   ??? ConfirmationPopup.cs
?       ?   ??? PlayerHud.cs
?       ?   ??? Events/
?       ?       ??? UIEvents.cs
?       ??? Editor/
?       ?   ??? UIRegistryEditor.cs
?       ?   ??? UICreatorWizard.cs
?       ?   ??? UIDebugWindow.cs
?       ??? Tests/
?       ?   ??? UIFrameworkTests.cs
?       ??? Generated/
?           ??? UIViewId.cs (auto-generated)
??? Prefabs/
    ??? UI/
        ??? MainMenuScreen.prefab
        ??? ConfirmationPopup.prefab
        ??? PlayerHud.prefab
```

## Memory Lifecycle

### UI Instance Lifecycle

```
[None] 
  ? Load (Addressable/Prefab) 
  ? [Loading]
  ? Initialize(data)
  ? [Hidden]
  ? Show()
  ? [Showing] (with animation/transition)
  ? [Visible]
  ? Hide()
  ? [Hiding] (with animation/transition)
  ? [Hidden]
  ? Cache/Pool/Dispose
  ? [Disposed]
```

### Memory Management Rules

1. **No Caching/Pooling**: Instance is destroyed immediately
   - `UIBase.Dispose()` ? `GameObject.Destroy()`
   - Memory released immediately

2. **Caching Enabled**: Instance kept in memory, reused on next Show()
   - Hidden but not destroyed
   - Transform moved to root (inactive)
   - Reinitialized on next Show()

3. **Pooling Enabled**: Instance returned to pool
   - Multiple instances kept in pool
   - Pre-warmed for frequently used UI
   - No instantiate/destroy overhead

### Reference Management

- **EventBus**: Uses WeakReference to prevent memory leaks
- **Controllers**: Disposed when view is disposed
- **UI Events**: Automatically cleaned up via weak references
- **Loaders**: Track handles and release on unload

## Flow: Opening a Popup

```
1. UIManager.Show<ConfirmationPopup>(data)
   ?
2. Check if already opened ? Return existing if visible
   ?
3. Check loading operations ? Cancel previous if exists (race condition)
   ?
4. Get from Cache/Pool ? If found, skip loading
   ?
5. Load (if not cached)
   ?? Get UIConfig from Registry
   ?? Get Layer root (Popup layer)
   ?? Load via IUILoader (Addressable/Prefab)
   ?? Get UIBase component
   ?
6. Initialize
   ?? Create Controller
   ?? controller.Initialize(view, data)
   ?? Bind UI events
   ?? Update UI elements with data
   ?
7. Add to openedViews dictionary
   ?
8. Show()
   ?? Set state = Showing
   ?? SetActive(true)
   ?? OnBeforeShow()
   ?? Play Transition/Animation (if injected)
   ?? controller.OnShow()
   ?? Set state = Visible
```

## Flow: Switching Screens

```
1. UIManager.Show<GameplayScreen>()
   ?
2. Load GameplayScreen (same as popup flow)
   ?
3. Push to screenStack
   ?
4. Show GameplayScreen
   ?
5. (Optional) Hide previous screen
   ?? Get current screen from stack
   ?? UIManager.Hide(previousScreen)
   ?? Handle caching/pooling
```

### Screen Stack Management

```
Stack: [MainMenu] 
  ? Show(GameplayScreen) 
  ? Stack: [MainMenu, GameplayScreen]
  ? ShowPreviousScreen()
  ? Hide(GameplayScreen)
  ? Stack: [MainMenu]
  ? Show(MainMenu)
```

## Usage Examples

### Basic Usage (Synchronous)

```csharp
// Show a screen
var menuData = new MainMenuData("Welcome!", true);
UIManager.Instance.Show<MainMenuScreen>(menuData);

// Show a popup
var confirmData = new ConfirmationPopupData(
    "Confirm Action",
    "Are you sure?",
    onConfirm: () => Debug.Log("Confirmed!"),
    onCancel: () => Debug.Log("Cancelled!")
);
UIManager.Instance.Show<ConfirmationPopup>(confirmData);

// Hide
UIManager.Instance.Hide<ConfirmationPopup>();

// Check if opened
if (UIManager.Instance.IsOpened<MainMenuScreen>())
{
    Debug.Log("Main menu is open");
}
```

### Async Usage (with UniTask)

```csharp
// Define UNITASK_SUPPORT in project settings

public async UniTask ShowMenuAsync()
{
    var cts = new CancellationTokenSource();
    
    try
    {
        var menuData = new MainMenuData("Welcome!", true);
        var menu = await UIManager.Instance.ShowAsync<MainMenuScreen>(
            menuData, 
            cts.Token
        );
        
        if (menu != null)
        {
            Debug.Log("Menu loaded successfully");
        }
    }
    catch (OperationCanceledException)
    {
        Debug.Log("Loading cancelled");
    }
}
```

### Injecting Animations

```csharp
// In your screen/popup class
protected override void OnInitialize(IUIData data)
{
    base.OnInitialize(data);
    
    // Inject scale animation for popup
    SetAnimation(new ScaleAnimation(0.25f));
    
    // Or inject fade transition
    // SetTransition(new FadeTransition(0.3f));
    
    // Or inject slide transition
    // SetTransition(new SlideTransition(SlideDirection.Bottom, 0.3f));
}
```

### Using EventBus for Communication

```csharp
// Subscribe to event
public class GameController : IEventHandler<PlayButtonClickedEvent>
{
    public void Initialize()
    {
        EventBus.Instance.Subscribe<PlayButtonClickedEvent>(this);
    }
    
    public void Handle(PlayButtonClickedEvent eventData)
    {
        // Handle the event
        StartGame();
    }
    
    public void Dispose()
    {
        EventBus.Instance.Unsubscribe<PlayButtonClickedEvent>(this);
    }
}

// Publish event (from UI controller)
EventBus.Instance.Publish(new PlayButtonClickedEvent());
```

### Preloading & Caching

```csharp
// Preload a screen
UIManager.Instance.Preload("MainMenuScreen");

// Async preload
await UIManager.Instance.PreloadAsync("GameplayScreen", cancellationToken);

// Prewarm pool (for frequently used popups)
UIManager.Instance.PrewarmPool("ConfirmationPopup", 3);
```

### Testing

```csharp
[Test]
public void Controller_OnButtonPressed_PublishesEvent()
{
    // Arrange
    var controller = new MainMenuController();
    var view = new MockUIView();
    var data = new MainMenuData("Test", false);
    var eventHandler = new TestEventHandler();
    
    EventBus.Instance.Subscribe<PlayButtonClickedEvent>(eventHandler);
    controller.Initialize(view, data);
    
    // Act
    controller.OnPlayButtonPressed();
    
    // Assert
    Assert.IsTrue(eventHandler.WasCalled);
}
```

## Performance Best Practices

1. **Avoid GC Spikes**
   - Use object pooling for frequently used UI
   - Reuse instances via caching
   - Avoid string concatenation in Update loops

2. **No Allocations in Update**
   - Pre-allocate collections
   - Cache component references
   - Use events instead of polling

3. **Canvas Optimization**
   - Each layer has its own Canvas (batch friendly)
   - Use CanvasGroup for transitions
   - Avoid unnecessary raycasts

4. **Memory Safety**
   - EventBus uses WeakReference
   - Controllers disposed with views
   - Addressable handles tracked and released

## Editor Tools

### UI Creator Wizard
`Window ? UIFramework ? UI Creator Wizard`
- Auto-creates View, Controller, Data scripts
- Creates prefab with proper setup
- Option to add to registry

### UI Debug Window
`Window ? UIFramework ? UI Debug Window`
- Shows opened views and states
- Memory usage tracking
- Manual hide/cache/pool actions

### UI Registry Editor
- Validates duplicate ViewIds
- Generates ViewId enum
- Validates addressable paths

## Configuration

### Enable UniTask Support
Add `UNITASK_SUPPORT` to Project Settings ? Player ? Scripting Define Symbols

### Enable Addressables Support
Add `ADDRESSABLES_SUPPORT` to Project Settings ? Player ? Scripting Define Symbols

## Integration Checklist

1. ? Create UIRegistry asset: `Assets ? Create ? UIFramework ? UI Registry`
2. ? Assign registry to UIManager
3. ? Configure UI layers in UILayerManager
4. ? Add UI configs to registry
5. ? Create UI views using wizard or manually
6. ? Test in play mode
7. ? Open Debug Window to monitor

## Multiplayer Safety

- No static state dependencies (except singleton for convenience)
- Instance-based design allows multiple UIManager instances
- EventBus can be instantiated per session
- No FindObjectOfType usage

## Platform Support

- **Mobile**: OnApplicationPause handled
- **PC/Console**: Domain reload safe
- **WebGL**: Memory tracking optimized

## Extension Points

- **IUITransition**: Custom transitions
- **IUIAnimation**: Custom animations
- **IInputStrategy**: Custom input handling
- **IUILoader**: Custom loading strategies
- **UIControllerBase**: Custom business logic

---

**Framework Version**: 1.0  
**Unity Compatibility**: 2019.4+  
**Dependencies**: Optional (UniTask, Addressables)
