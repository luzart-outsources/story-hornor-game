# UI Framework - Quick Start Guide

## Setup Instructions

### 1. Create Root Canvas
1. Create a GameObject in your scene
2. Add a Canvas component (Screen Space - Overlay)
3. Add a CanvasScaler component
4. Add the UIManager component

### 2. Create UI Registry
1. Right-click in Project ? Create ? Luzart ? UI Framework ? UI Registry
2. Save as `Assets/Resources/UIRegistry.asset`
3. Assign to UIManager's `registry` field

### 3. Register Your UIs
In UIRegistry, add entries for each UI:
- **ViewId**: Unique identifier (e.g., "MainMenuScreen")
- **Layer**: HUD, Screen, Popup, Overlay
- **LoadMode**: Direct or Addressable
- **AddressOrPath**: Addressable address or Resources path
- **Prefab**: Direct reference (if LoadMode = Direct)
- **TransitionType**: None, Fade, Slide, Scale
- **EnablePooling**: true for frequently used popups
- **PoolSize**: Number of instances to prewarm

---

## Basic Usage

### Show a Screen:
```csharp
using Luzart.UIFramework;

public class GameManager : MonoBehaviour
{
    async void Start()
    {
        var mainMenu = await UIManager.Instance.ShowAsync<MainMenuScreen>();
    }
}
```

### Show a Popup with Data:
```csharp
var data = new SettingsData
{
    MusicVolume = 0.8f,
    SfxVolume = 0.6f,
    IsFullscreen = true
};

await UIManager.Instance.ShowAsync<SettingsPopup>(data);
```

### Hide UI:
```csharp
UIManager.Instance.Hide<MainMenuScreen>();
```

### Custom Transition:
```csharp
var customTransition = new UISlideTransition(UIDirection.Left, 1000f, 0.5f);
await UIManager.Instance.ShowAsync<MyScreen>(data, customTransition);
```

---

## Creating New UI

### Step 1: Create ViewModel
```csharp
using Luzart.UIFramework;

public class MyScreenViewModel : UIViewModel
{
    private int score;

    public int Score
    {
        get => score;
        set
        {
            if (score != value)
            {
                score = value;
                NotifyDataChanged();
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        score = 0;
    }
}
```

### Step 2: Create View
```csharp
using Luzart.UIFramework;
using UnityEngine;
using UnityEngine.UI;

public class MyScreen : UIScreen
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Button closeButton;

    private MyScreenController controller;

    protected override void Awake()
    {
        base.Awake();
        closeButton?.onClick.AddListener(OnCloseClicked);
    }

    protected override void OnDestroy()
    {
        closeButton?.onClick.RemoveListener(OnCloseClicked);
        base.OnDestroy();
    }

    protected override void OnInitialize(object data)
    {
        base.OnInitialize(data);

        var viewModel = new MyScreenViewModel();
        var eventBus = UIManager.Instance?.EventBus;

        controller = new MyScreenController();
        controller.Setup(this, viewModel, eventBus);
        controller.Initialize();

        UIManager.Instance?.Context.RegisterController<MyScreen>(controller);

        Refresh();
    }

    protected override void OnRefresh()
    {
        base.OnRefresh();
        
        if (controller?.ViewModel != null && scoreText != null)
        {
            scoreText.text = $"Score: {controller.ViewModel.Score}";
        }
    }

    protected override void OnDispose()
    {
        controller?.Dispose();
        controller = null;
        base.OnDispose();
    }

    private void OnCloseClicked()
    {
        controller?.OnCloseClicked();
    }
}
```

### Step 3: Create Controller
```csharp
using Luzart.UIFramework;

public class MyScreenController : UIController<MyScreen, MyScreenViewModel>
{
    private UIEventSubscription scoreChangedSubscription;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        LoadInitialData();
    }

    protected override void SubscribeToEvents()
    {
        base.SubscribeToEvents();
        scoreChangedSubscription = EventBus?.Subscribe<ScoreChangedEvent>(OnScoreChanged);
    }

    protected override void UnsubscribeFromEvents()
    {
        base.UnsubscribeFromEvents();
        scoreChangedSubscription?.Dispose();
    }

    public void OnCloseClicked()
    {
        UIManager.Instance?.Hide<MyScreen>();
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        ViewModel.Score = evt.NewScore;
    }

    private void LoadInitialData()
    {
        // Load from service
        var gameService = UIManager.Instance?.Context.GetService<IGameService>();
        ViewModel.Score = gameService?.GetCurrentScore() ?? 0;
    }
}

public class ScoreChangedEvent : UIEvent
{
    public int NewScore { get; }

    public ScoreChangedEvent(int newScore)
    {
        NewScore = newScore;
    }
}
```

### Step 4: Create Prefab
1. Create UI GameObject with Canvas Group
2. Add your UIScreen/UIPopup component
3. Set ViewId field (must match class name)
4. Set Layer field
5. Assign UI elements to serialized fields
6. Save as prefab

### Step 5: Register in UIRegistry
1. Open UIRegistry asset
2. Add new entry
3. Fill in all fields
4. Click "Validate Entries" (Editor only)

---

## EventBus Communication

### Publishing Events:
```csharp
// From any controller:
EventBus?.Publish(new PlayerDiedEvent(playerId));
```

### Subscribing to Events:
```csharp
protected override void SubscribeToEvents()
{
    base.SubscribeToEvents();
    playerDiedSubscription = EventBus?.Subscribe<PlayerDiedEvent>(OnPlayerDied);
}

private void OnPlayerDied(PlayerDiedEvent evt)
{
    // Show game over popup
    UIManager.Instance?.ShowAsync<GameOverPopup>(new GameOverData { Score = evt.Score });
}

protected override void UnsubscribeFromEvents()
{
    base.UnsubscribeFromEvents();
    playerDiedSubscription?.Dispose();
}
```

---

## Advanced Features

### Preloading:
```csharp
// Preload during loading screen:
var preloadTasks = new[]
{
    UIManager.Instance.ShowAsync<PlayerHUD>(),
    UIManager.Instance.ShowAsync<PauseMenu>()
};

await Task.WhenAll(preloadTasks);

// Hide immediately (cached in memory):
UIManager.Instance.Hide<PauseMenu>();

// Show instantly later (already loaded):
UIManager.Instance.ShowAsync<PauseMenu>(); // No load time
```

### Layer Management:
```csharp
// Get all active popups:
var activePopups = UIManager.Instance.GetActiveUIInLayer(UILayer.Popup);

// Hide all screens except HUD:
UIManager.Instance.HideAllIgnore("PlayerHUD", "MiniMap");
```

### Dependency Injection:
```csharp
// Register services at game start:
var uiManager = UIManager.Instance;
uiManager.Context.RegisterService<IPlayerService>(new PlayerService());
uiManager.Context.RegisterService<IAudioService>(new AudioService());
uiManager.Context.RegisterService<ISaveService>(new SaveService());

// Access in controllers:
var playerService = Context.GetService<IPlayerService>();
var playerData = playerService.GetPlayerData();
ViewModel.PlayerName = playerData.Name;
```

---

## Best Practices

### ? DO:
- Keep ViewModels as POCOs (no Unity dependencies)
- Put business logic in Controllers or Domain Services
- Use EventBus for cross-UI communication
- Dispose subscriptions in UnsubscribeFromEvents()
- Use CancellationToken for async operations
- Enable pooling for frequently opened popups
- Cache transition instances

### ? DON'T:
- Don't put business logic in MonoBehaviour (View)
- Don't use FindObjectOfType
- Don't create static state
- Don't call View methods from other Views
- Don't forget to call base methods
- Don't allocate in Update()
- Don't hold strong references to Controllers

---

## Troubleshooting

### UI doesn't show:
1. Check UIRegistry has entry for ViewId
2. Check prefab has UIBase component
3. Check Canvas is assigned in UIManager
4. Check console for errors

### Memory leaks:
1. Ensure Dispose() is called
2. Check EventBus subscriptions are disposed
3. Use Profiler ? Memory ? Take Snapshot
4. Look for accumulating UIBase instances

### Race conditions:
1. Always use CancellationToken
2. Check loadingOperations dictionary
3. Null-check after await points

### Animation not playing:
1. Ensure CanvasGroup component exists
2. Check transition is assigned
3. Verify duration > 0
4. Check for cancellation

---

## Example Project Structure

```
MyGame/
??? UI/
?   ??? Screens/
?   ?   ??? MainMenu/
?   ?   ?   ??? MainMenuScreen.cs
?   ?   ?   ??? MainMenuController.cs
?   ?   ?   ??? MainMenuViewModel.cs
?   ?   ?   ??? MainMenuScreen.prefab
?   ?   ??? Gameplay/
?   ?   ??? GameOver/
?   ??? Popups/
?   ?   ??? Settings/
?   ?   ??? Shop/
?   ?   ??? Confirmation/
?   ??? HUD/
?       ??? PlayerHUD/
?       ??? MiniMap/
??? Domain/
?   ??? Services/
?   ?   ??? PlayerService.cs
?   ?   ??? AudioService.cs
?   ?   ??? SaveService.cs
?   ??? Models/
??? Resources/
    ??? UIRegistry.asset
```

---

For complete documentation, see MEMORY_LIFECYCLE.md
