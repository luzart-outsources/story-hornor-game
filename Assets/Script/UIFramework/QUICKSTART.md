# UI Framework - Quick Start Guide

## Setup (5 minutes)

### Step 1: Create UI Config
1. Right-click in Project window
2. Select `Create > UIFramework > UI Config`
3. Name it "UIConfig"
4. Configure settings:
   - **Use Addressables**: false (for quick start, use true later)
   - **Use Caching**: true

### Step 2: Create Your First UI

#### Option A: Using UI Creator Tool (Recommended)
1. Open `Window > UI Framework > UI Creator`
2. Set UI Name: "MainMenu"
3. Set UI Type: "Screen"
4. Enable "Use ViewModel (MVVM)"
5. Click "Create UI"
6. Done! Scripts are generated and registered.

#### Option B: Manual Creation
Create a script extending UIScreen:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UIFramework.Core;

public class MainMenu : UIScreen
{
    [SerializeField] private Button _playButton;
    
    protected override void OnInitialize(object data)
    {
        base.OnInitialize(data);
        _playButton.onClick.AddListener(OnPlayClicked);
    }
    
    private void OnPlayClicked()
    {
        Debug.Log("Play clicked!");
    }
    
    protected override void OnDispose()
    {
        base.OnDispose();
        _playButton.onClick.RemoveListener(OnPlayClicked);
    }
}
```

### Step 3: Create UI Prefab
1. Create a Canvas in scene
2. Design your UI
3. Add your UI script component to root
4. Add CanvasGroup component (required for transitions)
5. Save as Prefab in `Resources/UI/MainMenu`
6. Delete from scene

### Step 4: Register UI
Add to UIConfig:
- UI Name: "MainMenu"
- Address: "UI/MainMenu"
- UI Type: "MainMenu, Assembly-CSharp"

Or register in code:
```csharp
UIManager.Instance.RegisterUI(typeof(MainMenu), "UI/MainMenu");
```

### Step 5: Setup UIManager in Scene
1. Create empty GameObject named "[UIManager]"
2. Add UIManager component
3. Assign your UIConfig
4. Setup layer transforms (or let it auto-create)

### Step 6: Use It!

```csharp
using UIFramework.Manager;
using UIFramework.Core.Transitions;

// Show UI
#if UNITASK_SUPPORT
await UIManager.Instance.ShowAsync<MainMenu>(null, new FadeTransition());
#else
UIManager.Instance.Show<MainMenu>(null, new FadeTransition());
#endif

// Hide UI
UIManager.Instance.Hide<MainMenu>();
```

## Common Patterns

### Pattern 1: Screen with ViewModel

**ViewModel:**
```csharp
using UIFramework.MVVM;

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
                NotifyPropertyChanged();
            }
        }
    }
}
```

**View:**
```csharp
using UIFramework.MVVM;

public class MainMenu : UIView<MainMenuViewModel>
{
    [SerializeField] private Text _nameText;
    
    protected override void OnViewModelChanged()
    {
        _nameText.text = ViewModel.PlayerName;
    }
}
```

**Usage:**
```csharp
var viewModel = new MainMenuViewModel { PlayerName = "Player123" };
await UIManager.Instance.ShowAsync<MainMenu>(viewModel);
```

### Pattern 2: Popup with Callback

```csharp
public class ConfirmPopup : UIPopup
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    
    private System.Action _onConfirm;
    private System.Action _onCancel;
    
    protected override void OnInitialize(object data)
    {
        base.OnInitialize(data);
        
        if (data is ConfirmPopupData popupData)
        {
            _onConfirm = popupData.OnConfirm;
            _onCancel = popupData.OnCancel;
        }
        
        _confirmButton.onClick.AddListener(() => {
            _onConfirm?.Invoke();
            UIManager.Instance.Hide<ConfirmPopup>();
        });
        
        _cancelButton.onClick.AddListener(() => {
            _onCancel?.Invoke();
            UIManager.Instance.Hide<ConfirmPopup>();
        });
    }
}

public class ConfirmPopupData
{
    public System.Action OnConfirm;
    public System.Action OnCancel;
}
```

### Pattern 3: Using Events

```csharp
using UIFramework.Events;

void Start()
{
    EventBus.Instance.Subscribe<UIOpenedEvent>(OnUIOpened);
}

void OnUIOpened(UIOpenedEvent evt)
{
    Debug.Log($"UI opened: {evt.UIType.Name}");
}

void OnDestroy()
{
    EventBus.Instance.Unsubscribe<UIOpenedEvent>(OnUIOpened);
}
```

## Transitions

```csharp
// Fade
var fade = new FadeTransition(duration: 0.3f);

// Scale
var scale = new ScaleTransition(duration: 0.3f);

// Slide
var slide = new SlideTransition(
    duration: 0.3f,
    direction: SlideTransition.Direction.Bottom
);

// Use
await UIManager.Instance.ShowAsync<MainMenu>(null, fade);
```

## Enable UniTask Support

1. Install UniTask package from Package Manager
2. Add `UNITASK_SUPPORT` to Scripting Define Symbols:
   - Edit > Project Settings > Player
   - Scripting Define Symbols
   - Add: `UNITASK_SUPPORT`

## Enable Addressables

1. Install Addressables package
2. Mark UI prefabs as Addressable
3. Set Address (e.g., "UI/MainMenu")
4. In UIConfig, set "Use Addressables" to true
5. Use same code - it works automatically!

## Debugging

### Debug Window
Open `Window > UI Framework > Debug Window` to see:
- Active UIs
- Popup stack
- Memory usage
- Quick actions

### Common Issues

**UI not showing?**
- Check UIConfig has the UI registered
- Verify prefab path in Resources or Addressables
- Check Canvas/CanvasGroup components

**Memory leak?**
- Ensure OnDispose() removes listeners
- Unsubscribe from EventBus in OnDestroy
- Check for circular references

**Performance issues?**
- Enable caching in UIConfig
- Use object pooling for frequent popups
- Preload heavy UIs during loading screens

## Next Steps

1. Read full documentation in `README.md`
2. Check examples in `UIFramework/Examples/`
3. Use UI Creator tool for rapid development
4. Setup your game's UI architecture
5. Implement domain layer (business logic)

## Tips

- ? **DO**: Separate business logic from UI
- ? **DO**: Use ViewModels for data
- ? **DO**: Use EventBus for communication
- ? **DO**: Dispose resources properly
- ? **DON'T**: Put business logic in MonoBehaviour
- ? **DON'T**: Reference UIs from other UIs
- ? **DON'T**: Use static state everywhere
- ? **DON'T**: Forget to add CanvasGroup component

## Example Project Structure

```
Assets/
??? Resources/
?   ??? UI/
?       ??? MainMenu.prefab
?       ??? GameScreen.prefab
?       ??? PausePopup.prefab
?       ??? RewardPopup.prefab
??? Script/
?   ??? UIFramework/           # Framework code
?   ??? UI/
?   ?   ??? MainMenu.cs
?   ?   ??? MainMenuViewModel.cs
?   ?   ??? GameScreen.cs
?   ?   ??? ...
?   ??? Domain/                # Business logic
?   ?   ??? PlayerService.cs
?   ?   ??? InventoryService.cs
?   ?   ??? ...
?   ??? Data/                  # Data models
?       ??? PlayerData.cs
?       ??? ...
??? Config/
    ??? UIConfig.asset
```

Happy coding! ??
