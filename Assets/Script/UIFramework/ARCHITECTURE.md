# UI Framework - Complete Architecture Document

## ?? Table of Contents
1. [Design Principles](#design-principles)
2. [Layer Architecture](#layer-architecture)
3. [Component Responsibilities](#component-responsibilities)
4. [Design Patterns Used](#design-patterns-used)
5. [Extension Points](#extension-points)
6. [Performance Considerations](#performance-considerations)

---

## Design Principles

### SOLID Compliance

#### 1. Single Responsibility Principle (SRP)
```
? UIBase: Only handles view lifecycle
? UIController: Only handles business logic
? UIData: Only holds data
? UIManager: Only manages UI instances
? EventBus: Only handles event distribution
```

#### 2. Open/Closed Principle (OCP)
```
? Open for extension via:
  - IUITransition (custom transitions)
  - IUIAnimation (custom animations)
  - IUILoader (custom loaders)
  - IInputStrategy (custom input)
  
? Closed for modification:
  - Base classes are abstract/virtual
  - Behavior changed via injection, not modification
```

#### 3. Liskov Substitution Principle (LSP)
```
? Any IUILoader can replace another
? Any IUITransition can replace another
? UIScreen, UIPopup, UIHud all substitute UIBase
```

#### 4. Interface Segregation Principle (ISP)
```
? Small, focused interfaces:
  - IUIView (view operations)
  - IUIController (logic operations)
  - IUIData (data marker)
  - IUITransition (transition only)
  - IUIAnimation (animation only)
```

#### 5. Dependency Inversion Principle (DIP)
```
? High-level (UIManager) depends on abstractions (IUILoader)
? Controllers depend on IUIView, not concrete UIBase
? Views depend on IUIController, not concrete controllers
```

---

## Layer Architecture

```
???????????????????????????????????????????????????????????????
?                      LAYER 0: DOMAIN                         ?
?  ???????????????????????????????????????????????????????    ?
?  ? - Game Services (PlayerService, InventoryService)   ?    ?
?  ? - Business Logic (pure C#, no Unity)                ?    ?
?  ? - Data Models (domain entities)                     ?    ?
?  ? - DOES NOT DEPEND ON UI                             ?    ?
?  ???????????????????????????????????????????????????????    ?
???????????????????????????????????????????????????????????????
                            ?
                            ? (no direct dependency)
                            ? (communication via events/services)
                            ?
???????????????????????????????????????????????????????????????
?                  LAYER 1: COMMUNICATION                      ?
?  ???????????????????????????????????????????????????????    ?
?  ? - EventBus (weak-reference based)                   ?    ?
?  ? - Command Pattern (UICommandInvoker)                ?    ?
?  ? - Service Locator (for DI)                          ?    ?
?  ? - DECOUPLES layers                                  ?    ?
?  ???????????????????????????????????????????????????????    ?
???????????????????????????????????????????????????????????????
                            ?
                            ?
                            ?
???????????????????????????????????????????????????????????????
?                   LAYER 2: UI SERVICE                        ?
?  ???????????????????????????????????????????????????????    ?
?  ? - UIManager (lifecycle management)                  ?    ?
?  ? - UILayerManager (layer sorting)                    ?    ?
?  ? - UIPool (object pooling)                           ?    ?
?  ? - IUILoader implementations                         ?    ?
?  ? - UIRegistry (configuration)                        ?    ?
?  ???????????????????????????????????????????????????????    ?
???????????????????????????????????????????????????????????????
                            ?
                            ?
                            ?
???????????????????????????????????????????????????????????????
?                 LAYER 3: UI PRESENTATION                     ?
?  ???????????????????????????????????????????????????????    ?
?  ? VIEW LAYER (MonoBehaviour)                          ?    ?
?  ? - UIBase, UIScreen, UIPopup, UIHud                  ?    ?
?  ? - Handles: Rendering, Unity events, UI elements     ?    ?
?  ? - NO business logic                                 ?    ?
?  ???????????????????????????????????????????????????????    ?
?                            ?                                 ?
?  ???????????????????????????????????????????????????????    ?
?  ? CONTROLLER LAYER (Pure C#)                          ?    ?
?  ? - UIControllerBase implementations                  ?    ?
?  ? - Handles: Business logic, event handling           ?    ?
?  ? - Testable without Unity                            ?    ?
?  ???????????????????????????????????????????????????????    ?
?                            ?                                 ?
?  ???????????????????????????????????????????????????????    ?
?  ? DATA LAYER (ViewModel)                              ?    ?
?  ? - UIDataBase implementations                        ?    ?
?  ? - Immutable or controlled mutation                  ?    ?
?  ? - Serializable                                      ?    ?
?  ???????????????????????????????????????????????????????    ?
???????????????????????????????????????????????????????????????
                            ?
                            ? (Strategy Injection)
                            ?
???????????????????????????????????????????????????????????????
?                  LAYER 4: STRATEGIES                         ?
?  ???????????????????????????????????????????????????????    ?
?  ? - IUITransition implementations (Fade, Slide)       ?    ?
?  ? - IUIAnimation implementations (Scale, etc.)        ?    ?
?  ? - IInputStrategy implementations                    ?    ?
?  ? - INJECTED at runtime, reusable                     ?    ?
?  ???????????????????????????????????????????????????????    ?
???????????????????????????????????????????????????????????????
```

---

## Component Responsibilities

### UIManager (Service Layer)
**Responsibilities:**
- ? Load UI instances (via IUILoader)
- ? Maintain opened views dictionary
- ? Manage screen navigation stack
- ? Route Show/Hide requests to correct layer
- ? Handle caching and pooling
- ? Coordinate with UILayerManager

**Does NOT:**
- ? Know about business logic
- ? Store game state
- ? Handle UI events directly

### UIBase (View Layer)
**Responsibilities:**
- ? Hold Unity component references
- ? Bind/unbind UI events
- ? Update visual elements
- ? Trigger animations/transitions
- ? Communicate with controller

**Does NOT:**
- ? Contain business logic
- ? Make decisions
- ? Call other UIs directly
- ? Access domain services

### UIController (Logic Layer)
**Responsibilities:**
- ? Handle UI events (button clicks, etc.)
- ? Execute business logic
- ? Publish events via EventBus
- ? Interact with domain services
- ? Update view via ViewModel

**Does NOT:**
- ? Access Unity components directly
- ? Know about other UIs
- ? Instantiate GameObjects

### UIData (ViewModel Layer)
**Responsibilities:**
- ? Hold presentation data
- ? Provide immutable interface
- ? Be serializable
- ? Support controlled mutation

**Does NOT:**
- ? Contain logic
- ? Know about view implementation
- ? Hold Unity object references

### EventBus (Communication Layer)
**Responsibilities:**
- ? Decouple publishers and subscribers
- ? Manage event distribution
- ? Use weak references (no leaks)
- ? Thread-safe operations

**Does NOT:**
- ? Store event history
- ? Guarantee delivery order
- ? Handle synchronization

---

## Design Patterns Used

### 1. Model-View-ViewModel (MVVM)
```
ViewModel (UIData) ?? View (UIBase) ?? Controller
         ?                                    ?
         ??????????? One-way data flow ???????
         
Data changes ? ViewModel updated ? View.Refresh() ? UI updated
```

### 2. Strategy Pattern
```
UIBase uses IUITransition
       uses IUIAnimation
       uses IInputStrategy
       
Different behaviors injected at runtime:
- popup.SetAnimation(new ScaleAnimation());
- screen.SetTransition(new FadeTransition());
```

### 3. Observer Pattern (EventBus)
```
Publisher: controller.Publish(event)
    ?
    ?
EventBus (mediator)
    ?
    ?
Subscribers: handler1, handler2, handler3
```

### 4. Object Pool Pattern
```
Pool maintains queue of reusable instances
Get() ? Dequeue or Create
Return() ? Reset and Enqueue
Zero Instantiate/Destroy overhead
```

### 5. Factory Pattern (UIManager)
```
UIManager.Show<T>()
    ?
    ?? Creates instance via IUILoader
    ?? Initializes with data
    ?? Configures based on UIConfig
    ?? Returns ready-to-use UI
```

### 6. Command Pattern
```
UICommandInvoker executes IUICommand
Supports:
- Execute
- Undo
- History tracking
- Replay
```

### 7. Singleton Pattern (with DI support)
```
UIManager.Instance (convenience)
  BUT
Can also be instantiated directly for testing:
  var manager = new GameObject().AddComponent<UIManager>();
  manager.Initialize();
```

### 8. Repository Pattern (UIRegistry)
```
UIRegistry = Configuration repository
- Stores all UI configs
- Validates on save
- Provides query interface
- ScriptableObject-based (Unity-friendly)
```

---

## Extension Points

### 1. Custom Transitions

```csharp
public class MyCustomTransition : IUITransition
{
    public void TransitionIn(GameObject target, Action onComplete)
    {
        // Your transition logic
        // Use DOTween, Animation, or any system
        onComplete?.Invoke();
    }
    
    public void TransitionOut(GameObject target, Action onComplete)
    {
        // Your transition logic
        onComplete?.Invoke();
    }
}

// Usage:
view.SetTransition(new MyCustomTransition());
```

### 2. Custom Loaders

```csharp
public class AssetBundleUILoader : IUILoader
{
    public GameObject Load(string address, Transform parent)
    {
        // Load from AssetBundle
        var bundle = AssetBundle.LoadFromFile(address);
        var prefab = bundle.LoadAsset<GameObject>("UI");
        return Instantiate(prefab, parent);
    }
    
    public void Unload(GameObject instance)
    {
        // Unload bundle
        Destroy(instance);
    }
}
```

### 3. Custom Input Strategy

```csharp
public class GamepadInputStrategy : IInputStrategy
{
    public void ProcessInput()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            // Handle gamepad back button
        }
    }
}

// Inject into UI
view.SetInputStrategy(new GamepadInputStrategy());
```

### 4. Custom Events

```csharp
public class PlayerLevelUpEvent : IUIEvent
{
    public int NewLevel { get; }
    public int Reward { get; }
    
    public PlayerLevelUpEvent(int level, int reward)
    {
        NewLevel = level;
        Reward = reward;
    }
}

// Publish
EventBus.Instance.Publish(new PlayerLevelUpEvent(5, 100));

// Subscribe
public class RewardPopupController : IEventHandler<PlayerLevelUpEvent>
{
    public void Handle(PlayerLevelUpEvent evt)
    {
        ShowRewardPopup(evt.NewLevel, evt.Reward);
    }
}
```

---

## Performance Considerations

### 1. Canvas Batching

```
Framework uses layer-based canvas structure:

HUD Layer (Canvas, sortingOrder = 0)
  ?? HealthBar, Minimap (batched together)

Screen Layer (Canvas, sortingOrder = 100)
  ?? MainMenu, Settings (batched together)

Popup Layer (Canvas, sortingOrder = 200)
  ?? Confirmation, Reward (batched together)

Benefit: Minimize draw calls, better batching
```

### 2. Zero Allocations

```csharp
// Pre-allocated collections
private Dictionary<string, UIBase> openedViews; // No new() in loops

// Cached references
private Transform layerRoot; // Retrieved once, cached

// No lambda allocations
button.onClick.AddListener(OnButtonClicked); // Method reference
NOT: button.onClick.AddListener(() => DoSomething()); // Lambda allocates
```

### 3. Memory Pooling

```
Popup shown 100 times:
  Without Pool: 100 Instantiate + 100 Destroy = 200 GC events
  With Pool:    1 Instantiate + 99 reuse = 1 GC event
  
Result: 99% reduction in GC overhead
```

### 4. Async Operations

```
UniTask vs C# Task:
  ? UniTask: Zero allocation async/await
  ? C# Task: Allocates Task object per await
  
Framework uses UniTask for zero-alloc async
```

---

## Threading & Multiplayer Safety

### Thread Safety

```csharp
EventBus:
  private readonly object lockObject = new object();
  
  public void Subscribe<T>(...)
  {
      lock (lockObject) // Thread-safe
      {
          // Modify subscribers
      }
  }
```

### Multiplayer Safety

```
No static state that stores game data:
  ? static Dictionary<string, Player> players; // BAD
  ? UIManager as instance (can have multiple)
  
Each game session can have its own UIManager:
  Session 1: UIManager instance 1
  Session 2: UIManager instance 2
  
No interference between sessions
```

---

## Error Handling

### 1. Loading Failures

```csharp
try
{
    var view = await LoadViewAsync(viewId, token);
}
catch (OperationCanceledException)
{
    // User cancelled, cleanup
    return null;
}
catch (Exception ex)
{
    Debug.LogError($"Failed to load {viewId}: {ex}");
    return null;
}
```

### 2. Null Safety

```csharp
// Every public API checks nulls
public void Show(string viewId, IUIData data)
{
    if (string.IsNullOrEmpty(viewId))
    {
        Debug.LogError("ViewId is null");
        return;
    }
    
    var config = registry?.GetConfig(viewId);
    if (config == null)
    {
        Debug.LogError($"No config for {viewId}");
        return;
    }
    
    // Continue...
}
```

### 3. State Validation

```csharp
public void Show()
{
    // Prevent invalid state transitions
    if (state == UIState.Showing || state == UIState.Visible)
    {
        Debug.LogWarning("Already showing/visible");
        return;
    }
    
    state = UIState.Showing;
    // Continue...
}
```

---

## Data Flow Patterns

### Pattern 1: Simple Display (Read-only)

```
Domain ? ViewModel ? View
         (immutable)  (display)

Example: Leaderboard
  LeaderboardService.GetTop10()
    ? LeaderboardData(readonly list)
    ? LeaderboardView.Display()
```

### Pattern 2: Interactive (User Input)

```
User Input ? View ? Controller ? Domain
                         ?
                         ?? EventBus ? Other Systems

Example: Shop Purchase
  User clicks "Buy"
    ? ShopView.OnBuyClicked()
    ? ShopController.OnBuyPressed()
    ? PurchaseService.BuyItem()
    ? EventBus.Publish(ItemPurchasedEvent)
    ? InventoryUI.Handle(event) ? Refresh
```

### Pattern 3: Reactive Updates (Events)

```
Domain Event ? EventBus ? Controller ? Update ViewModel ? Refresh View

Example: Health Change
  Player takes damage
    ? PlayerService fires event
    ? EventBus.Publish(HealthChangedEvent)
    ? PlayerHudController.Handle(event)
    ? Create new PlayerHudData
    ? view.Initialize(newData)
    ? view.Refresh()
    ? UI updated
```

---

## Testing Strategy

### Unit Tests (No Unity)

```csharp
[Test]
public void Controller_ButtonPressed_PublishesEvent()
{
    // Arrange: Pure C# objects
    var controller = new MainMenuController();
    var mockView = new MockUIView();
    var data = new MainMenuData("Test");
    var eventHandler = new MockEventHandler<PlayGameEvent>();
    
    EventBus.Instance.Subscribe(eventHandler);
    controller.Initialize(mockView, data);
    
    // Act
    controller.OnPlayPressed();
    
    // Assert
    Assert.IsTrue(eventHandler.WasCalled);
}
```

### Integration Tests (With Unity)

```csharp
[UnityTest]
public IEnumerator UIManager_ShowPopup_AppearsOnScreen()
{
    // Arrange
    var manager = UIManager.Instance;
    var data = new ConfirmationPopupData("Test", "Message");
    
    // Act
    manager.Show<ConfirmationPopup>(data);
    
    yield return null; // Wait one frame
    
    // Assert
    Assert.IsTrue(manager.IsOpened<ConfirmationPopup>());
}
```

### Mock Services

```csharp
public interface IPlayerService
{
    int GetHealth();
}

// Real implementation
public class PlayerService : IPlayerService
{
    public int GetHealth() => actualPlayer.health;
}

// Mock for testing
public class MockPlayerService : IPlayerService
{
    public int GetHealth() => 100; // Always return 100
}

// Inject mock
ServiceLocator.Instance.Register<IPlayerService>(new MockPlayerService());
```

---

## Memory Management Deep Dive

### Reference Types

```
1. STRONG REFERENCES (can cause leaks):
   - Dictionary<string, UIBase> openedViews
   - Solution: Cleared on Hide/Dispose
   
2. WEAK REFERENCES (safe):
   - EventBus subscribers
   - Solution: Auto-cleanup when object destroyed
   
3. UNMANAGED REFERENCES:
   - Addressable handles
   - Solution: Tracked and released manually
```

### GC Optimization

```
ALLOCATION SOURCES:
1. ? new GameObject() every Show()
   ? Solution: Object pooling

2. ? String concatenation in Update()
   ? Solution: StringBuilder, cached strings

3. ? Lambda in AddListener()
   ? Solution: Method references

4. ? LINQ in Update()
   ? Solution: Pre-allocated lists, foreach

5. ? Boxing value types
   ? Solution: Generic constraints, avoid object
```

### Memory Budget Example

```
Typical Game UI Memory:

HUD (always loaded):           5 MB
Main Menu (cached):           10 MB
Settings Screen (cached):      8 MB
Gameplay Screen (active):     15 MB
Popup Pool (10 instances):     2 MB
--------------------------------------------
Total:                        40 MB

With framework:
- Predictable memory usage
- No surprises
- Easy to profile
```

---

## Scalability

### Supporting 50+ Screens

```
1. Registry System:
   - All UIs registered in ScriptableObject
   - O(1) lookup via dictionary
   - No FindObjectOfType scans
   
2. Lazy Loading:
   - UIs loaded on-demand
   - Addressables: Load from disk/network
   - Memory efficient
   
3. Layer Management:
   - 4 layers × 50 UIs = 200 sorting orders available
   - Automatic layer assignment
   - No overlap conflicts
   
4. Caching Strategy:
   - Critical UIs: Always cached
   - Medium-use UIs: Cache on first load
   - Rare UIs: No cache (load on demand)
   
5. Event System:
   - Weak references = no growth over time
   - Automatic cleanup = no manual tracking
   - Unlimited events/handlers
```

---

## Production Checklist

### Before Shipping

- [ ] All UIs registered in UIRegistry
- [ ] Registry validated (no duplicates)
- [ ] Addressable paths tested
- [ ] Memory profiled (no leaks in long session)
- [ ] All UI events properly unbound
- [ ] Pooling configured for frequent UIs
- [ ] Scene transitions tested
- [ ] Mobile pause/resume tested
- [ ] Race conditions tested (rapid open/close)
- [ ] Performance profiled (60 FPS maintained)

### Code Review Points

- [ ] No FindObjectOfType usage
- [ ] No business logic in MonoBehaviour
- [ ] All IDisposable properly disposed
- [ ] No lambda in Update/FixedUpdate
- [ ] EventBus Subscribe has matching Unsubscribe
- [ ] Cancellation tokens used for async
- [ ] Weak references for long-lived events
- [ ] No static state holding game data

---

## Advanced Features

### Custom Layer Configuration

```csharp
// Extend UILayerManager
public class CustomLayerManager : UILayerManager
{
    public CustomLayerManager(Transform root) : base(root)
    {
        // Add custom layers
        CreateLayer(CustomUILayer.WorldSpace, 50);
        CreateLayer(CustomUILayer.VR, 400);
    }
}
```

### Middleware Pattern

```csharp
public interface IUIMiddleware
{
    void OnBeforeShow(UIBase view);
    void OnAfterShow(UIBase view);
}

// Analytics middleware
public class AnalyticsMiddleware : IUIMiddleware
{
    public void OnAfterShow(UIBase view)
    {
        Analytics.TrackScreenView(view.ViewId);
    }
}

// Inject middleware into UIManager
manager.AddMiddleware(new AnalyticsMiddleware());
```

### UI State Serialization

```csharp
[Serializable]
public class UIStateSnapshot
{
    public List<string> openedScreens;
    public List<string> openedPopups;
    public Dictionary<string, string> cachedData;
}

// Save state
var snapshot = UIManager.Instance.CreateSnapshot();
SaveSystem.Save(snapshot);

// Restore state
var snapshot = SaveSystem.Load<UIStateSnapshot>();
UIManager.Instance.RestoreSnapshot(snapshot);
```

---

## Comparison with Other Frameworks

| Feature | This Framework | Unity UI Toolkit | Other Frameworks |
|---------|----------------|------------------|------------------|
| MVVM Pattern | ? Full | ~ Partial | ~ Varies |
| Addressables | ? Native | ? No | ~ Some |
| Object Pooling | ? Built-in | ? No | ~ Manual |
| Event System | ? Weak-ref | ? No | ? Some |
| Testability | ? High | ~ Medium | ~ Low |
| Memory Safe | ? Yes | ~ Depends | ~ Varies |
| Editor Tools | ? Extensive | ? Good | ~ Basic |
| Learning Curve | Medium | High | Low-Medium |

---

## FAQ

**Q: Do I need UniTask?**
A: No, framework works without it. UniTask is optional for async features.

**Q: Do I need Addressables?**
A: No, can use direct prefab references. Addressables is for advanced loading.

**Q: Can I use DOTween instead of LeanTween?**
A: Yes, create custom IUITransition/IUIAnimation with DOTween.

**Q: How to integrate with existing UI?**
A: Wrap existing UIs in UIBase, add controller, gradually migrate.

**Q: Thread-safe for multiplayer?**
A: EventBus is thread-safe. UIManager should run on main thread (Unity requirement).

**Q: Can I have multiple UIManagers?**
A: Yes, for multi-scene or split-screen scenarios.

---

**Framework Philosophy:**
"Decoupled, Testable, Performant, Scalable"

**Design Goals:**
? Easy to use for beginners
? Powerful for experts
? Production-ready out of the box
? Extensible without modification
