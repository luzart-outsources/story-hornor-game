# Memory Lifecycle Explanation

## Overview

The UI Framework implements a sophisticated memory management system that prevents leaks, reduces GC pressure, and optimizes performance for production games.

## Lifecycle States

```
????????????????????????????????????????????????????????????????
?                    UI INSTANCE LIFECYCLE                      ?
????????????????????????????????????????????????????????????????

    [None] ???????????????????????????????????????
       ?                                          ?
       ? UIManager.Show()                         ?
       ?                                          ?
    [Loading] ? (Addressable/Prefab)              ?
       ?                                          ?
       ? Load Complete                            ?
       ?                                          ?
    Initialize(data)                              ?
       ?                                          ?
       ? - Create Controller                      ?
       ? - Bind Events                            ?
       ? - Set Initial State                      ?
       ?                                          ?
    [Hidden]                                      ?
       ?                                          ?
       ? Show()                                   ?
       ?                                          ?
    [Showing] ? (Animation/Transition)            ?
       ?                                          ?
       ? Animation Complete                       ?
       ?                                          ?
    [Visible]                                     ?
       ?                                          ?
       ? Hide()                                   ?
       ?                                          ?
    [Hiding] ? (Animation/Transition)             ?
       ?                                          ?
       ? Animation Complete                       ?
       ?                                          ?
    [Hidden]                                      ?
       ?                                          ?
       ???? Caching? ???? [Cached] ???????????????
       ?                    (Reuse on next Show)
       ?
       ???? Pooling? ???? [Pooled] ???????????????
       ?                    (Return to pool)
       ?
       ???? Dispose ????? [Disposed]
                          (Memory released)
```

## Memory Management Strategies

### 1. Immediate Disposal (No Caching, No Pooling)

**When**: UI that's rarely used or large memory footprint

```csharp
UIConfig:
  enableCaching = false
  enablePooling = false

Flow:
  Show ? Hide ? Dispose() ? Destroy(GameObject) ? Memory Released
```

**Pros**:
- Lowest memory footprint
- Clean memory immediately

**Cons**:
- Instantiate/Destroy overhead
- GC spike when destroyed

### 2. Caching Strategy

**When**: Medium-frequency UI, acceptable memory trade-off

```csharp
UIConfig:
  enableCaching = true
  enablePooling = false

Flow:
  Show ? Hide ? Move to cachedViews dictionary
  Next Show ? Reuse cached instance ? Skip loading

Memory:
  - 1 instance per UI kept in memory
  - GameObject stays alive but inactive
  - No load/instantiate overhead on reopen
```

**Pros**:
- No reload overhead
- Single instance reuse
- Fast reopen

**Cons**:
- Memory stays allocated
- One instance only

### 3. Object Pooling Strategy

**When**: High-frequency UI (notifications, damage numbers, item tooltips)

```csharp
UIConfig:
  enableCaching = false
  enablePooling = true
  poolSize = 5

Flow:
  Prewarm ? Create 5 instances ? Pool
  Show ? Get from pool (or create new)
  Hide ? Return to pool
  
Memory:
  - Multiple instances pre-created
  - Reused from pool
  - No Instantiate() calls after prewarm
```

**Pros**:
- Zero GC from Instantiate/Destroy
- Multiple concurrent instances
- Predictable memory

**Cons**:
- Higher base memory usage
- Needs tuning for pool size

## Reference Management

### 1. EventBus Weak References

```csharp
Problem: Strong references cause memory leaks
  Handler subscribed ? Object destroyed ? Still in subscriber list ? LEAK

Solution: WeakReference
  private List<WeakReference> subscribers
  
  When publishing:
    - Check if WeakReference.IsAlive
    - Skip dead references
    - Periodic cleanup removes dead references
    
Result: 
  Handler destroyed ? WeakReference.IsAlive = false ? Auto cleanup ? No leak
```

### 2. Controller Disposal

```csharp
UIBase.Dispose():
  1. controller?.Dispose()  ? Cleanup controller
  2. controller = null       ? Release reference
  3. Destroy(gameObject)     ? Destroy view

Controller.Dispose():
  1. Unsubscribe from events
  2. Release domain service references
  3. view = null
  4. data = null
```

### 3. UI Event Unbinding

```csharp
OnInitialize():
  button.onClick.AddListener(OnButtonClicked)  ? Strong reference
  
OnDispose():
  button.onClick.RemoveListener(OnButtonClicked)  ? Must unbind!
  
Why: UnityEvent holds strong reference to handler
     ? If not removed ? Memory leak
```

## Addressable Memory Management

```csharp
Load:
  1. Addressables.LoadAssetAsync<GameObject>(address)
  2. Store handle in loadedHandles dictionary
  3. Instantiate prefab
  4. Track memory usage
  
Unload:
  1. Get handle from loadedHandles
  2. Addressables.Release(handle)  ? Critical!
  3. Remove from dictionary
  4. Destroy(instance)
  
Result:
  - Asset reference count decremented
  - Memory freed when count = 0
  - No asset leaks
```

## Race Condition Handling

### Problem: Double Open

```
Frame 1: ShowAsync("Popup") ? starts loading
Frame 2: ShowAsync("Popup") ? starts loading again
Frame 3: Both complete ? 2 instances!
```

### Solution

```csharp
private Dictionary<string, CancellationTokenSource> loadingOperations;

ShowAsync(viewId):
  1. Check if loading ? Cancel previous operation
  2. Create new CancellationTokenSource
  3. Store in loadingOperations[viewId]
  4. Start loading
  5. On complete/cancel ? Remove from loadingOperations
  
Result: Only one load operation per viewId at a time
```

### Problem: Open ? Close ? Open Fast

```
Frame 1: ShowAsync("Popup") ? starts loading
Frame 2: Hide("Popup") ? cancel loading
Frame 3: ShowAsync("Popup") ? starts loading again
```

### Solution

```csharp
Hide(viewId):
  1. Check loadingOperations[viewId]
  2. If exists ? cts.Cancel()
  3. Remove from loadingOperations
  4. Continue with hide logic

ShowAsync checks cancellation at each await point
```

## Scene/Domain Reload Safety

### Problem: Static State Survives Reload

```csharp
BAD:
  static Dictionary<string, UIBase> views;  ? Survives domain reload
  ? After reload: Dictionary has invalid references ? CRASH
```

### Solution

```csharp
GOOD:
  - UIManager as MonoBehaviour with DontDestroyOnLoad
  - Instance-based design
  - OnDestroy() cleanup
  - Application lifecycle hooks

OnDestroy():
  1. Cancel all loading operations
  2. HideAll()
  3. ClearCache()
  4. ClearPool()
  5. instance = null  ? Clear singleton
```

## Mobile Pause/Resume

```csharp
OnApplicationPause(bool paused):
  if (paused):
    - Pause animations
    - Cache current state
    - Optional: Release non-critical memory
  else:
    - Resume animations
    - Restore state
    - Reload critical UI if needed
```

## GC Optimization Strategies

### 1. No Allocations Per Frame

```csharp
BAD:
  void Update()
  {
      foreach (var view in GetOpenedViews())  ? Allocates List
      {
          view.DoSomething();
      }
  }

GOOD:
  - Pre-allocate collections
  - Use events instead of polling
  - Cache component references
```

### 2. String Optimization

```csharp
BAD:
  text.text = "Score: " + score;  ? String allocation

GOOD:
  StringBuilder sb;  ? Reusable
  sb.Clear();
  sb.Append("Score: ");
  sb.Append(score);
  text.text = sb.ToString();
```

### 3. Lambda Allocation

```csharp
BAD:
  button.onClick.AddListener(() => DoSomething());  ? Allocates closure

GOOD:
  button.onClick.AddListener(OnButtonClicked);
  private void OnButtonClicked() { DoSomething(); }
```

## Memory Tracking

```csharp
AddressableUILoader:
  - Tracks memory per addressable
  - Logs instance count
  - GetMemoryUsage() for debugging
  
Usage:
  var loader = UIManager.Instance.GetLoader() as AddressableUILoader;
  var usage = loader.GetMemoryUsage();
  foreach (var kvp in usage)
  {
      Debug.Log($"{kvp.Key}: {kvp.Value} instances");
  }
```

## Best Practices Summary

? **DO:**
- Use caching for medium-frequency UI
- Use pooling for high-frequency UI
- Unsubscribe from events in Dispose()
- Cancel async operations on Hide()
- Use WeakReference for event handlers
- Release Addressable handles
- Implement proper Dispose pattern

? **DON'T:**
- Use FindObjectOfType (performance)
- Keep strong references to UI in static fields
- Allocate in Update loops
- Use lambda in AddListener
- Forget to unbind UI events
- Load synchronously from Addressables
- Skip cancellation token handling

## Memory Leak Prevention Checklist

- [ ] All UI events unbound in OnDispose()
- [ ] EventBus uses WeakReference
- [ ] Controllers disposed with views
- [ ] Addressable handles released
- [ ] No static references to UI instances
- [ ] CancellationToken used for async operations
- [ ] Proper cleanup in OnDestroy()
- [ ] No circular references between UI elements

## Monitoring Memory

### In Editor (Debug Window)
1. Open: `Window ? UIFramework ? UI Debug Window`
2. View opened UIs and their states
3. Check memory usage
4. Manual cleanup actions

### At Runtime
```csharp
// Log memory status
UIManager.Instance.LogOpenedViews();

// Get state dictionary
var states = UIManager.Instance.GetOpenedViewsState();

// Force cleanup
UIManager.Instance.ClearCache();
UIManager.Instance.ClearPool();
```

---

**Critical for Production:**
- Always test with Profiler
- Monitor memory in long play sessions
- Test rapid open/close scenarios
- Verify no leaks after scene reload
