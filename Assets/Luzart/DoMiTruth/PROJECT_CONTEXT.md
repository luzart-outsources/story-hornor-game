# PROJECT CONTEXT - Do-Mi Truth
## Detective Horror Game | Unity Canvas 2D | Point & Click

---

## 1. ARCHITECTURE OVERVIEW

### Communication Pattern: HYBRID
```
Singleton Direct Call (1-to-1)          SO Event Channel (1-to-many)
GameFlowController → UIManager         OnClueCollected → HUD, Notebook
DialogueManager → UINPCDialogue        OnNotebookUpdated → HUD badge
InvestigationManager → UILockPuzzle    OnSettingsChanged → Audio system
```

### UI System: SO Registry Pattern
```
UIManager (Singleton)
  └── UIRegistrySO (ScriptableObject asset)
       └── List<UIEntry>
            ├── UIName enum key
            ├── UIBase prefab (direct reference)
            ├── int layerIndex (0=Screen, 1=Popup, 2=HUD, 3=System, 4=Toast)
            └── bool useCache
```
- **KHONG** dung Resources.Load - prefab reference truc tiep trong SO
- **KHONG** dung CanvasGroup - animation qua TweenAnimationBase + TweenAnimationCaller
- Show/Hide lifecycle: `UIBase.Show()` → `showAnimation?.CallShow()` → visible
- Hide: `hideAnimation` play xong → `ExecuteHide()` (destroy hoac deactivate)

### ActionStep: Strategy Pattern (Config + Behavior)
```
ActionStepConfig (abstract SO)          ActionStepBehavior (abstract class)
  └── CreateBehavior() ─────────────→    └── Execute() : IEnumerator

CollectClueConfig ──→ CollectClueBehavior      (add clue to inventory)
ShowDialogueConfig ──→ ShowDialogueBehavior    (show dialogue sequence)
ShowToastConfig ──→ ShowToastBehavior          (show toast message)
ClosePopupConfig ──→ ClosePopupBehavior        (close popup)
WaitConfig ──→ WaitBehavior                    (delay)
ShowClueDetailConfig ──→ ShowClueDetailBehavior (show clue detail popup)
```
- Moi ActionStepConfig la 1 SO asset, keo-tha vao InteractableObjectSO
- Chay chain: `foreach config → config.CreateBehavior().Execute()` (coroutine)
- Mo rong: them action moi chi can tao 2 class (Config + Behavior), khong sua code cu

---

## 2. FILE MAP

### Framework Layer (Luzart/Utility)
```
Assets/Luzart/Utility/Script/
├── UIBase/
│   ├── UIManager.cs          # Singleton, SO Registry loading, ShowUI<T>/HideUiActive/HideAll
│   ├── UIBase.cs             # Base class, TweenAnimationCaller show/hide, closeBtn
│   ├── UIRegistrySO.cs       # SO map UIName → prefab + layer + cache
│   └── UIToast/UIToast.cs    # Toast notification (canvasGroup + txtNoti)
├── Events/
│   └── GameEventChannel.cs   # GameEventChannel, GameEventChannel<T>, StringEventChannel
├── NewBaseSelect/
│   ├── BaseSelect.cs         # Abstract base, Select(bool) / Select(int)
│   ├── SelectToggleGameObject.cs  # obSelect[]/obUnSelect[] show/hide by bool
│   ├── SelectToggleImage.cs       # Sprite swap by bool
│   ├── SelectSwitchGameObject.cs  # GroupGameObject[] switch by int index
│   └── SelectSwitchImage.cs       # Sprite[] switch by int index
├── Other/
│   ├── GameUtil.cs           # Singleton<T>, SingletonSaveLoad<T>, ButtonOnClick, etc
│   ├── EffectButton.cs       # Button scale animation on press (1.1x)
│   ├── Observer.cs           # Legacy observer (only for internal framework)
│   ├── TweenExtension.cs     # DOSetTextCharByChar (line 64) - typewriter effect
│   └── SaveLoadUtil.cs       # JSON PlayerPrefs save/load
└── ThirdParty/TweenAnimation/
    ├── TweenAnimation.cs         # EAnimation enum (Scale, FadeByCanvasGroup, Move...)
    └── TweenAnimationCaller.cs   # CallShow(), ETypeShow (None/Awake/Start/OnEnable)
```

### Game Layer (DoMiTruth)
```
Assets/Luzart/DoMiTruth/
├── Scripts/
│   ├── Data/
│   │   ├── DialogueCharacterSO.cs    # characterId, name, portrait, nameColor
│   │   ├── DialogueSequenceSO.cs     # dialogueId, List<DialogueLine>
│   │   ├── ClueSO.cs                 # clueId, name, image, description, category
│   │   ├── LockConfigSO.cs           # lockType, passcode, hintText
│   │   ├── InteractableObjectSO.cs   # objectId, type, clue/dialogue/lock/actions
│   │   ├── RoomSO.cs                 # roomId, background, List<RoomInteractable>
│   │   ├── MapSO.cs                  # mapId, name, thumbnail, List<RoomSO>
│   │   ├── GameConfigSO.cs           # introCutscene, allMaps, panSpeed, etc
│   │   ├── GameSaveData.cs           # Serializable save data class
│   │   └── Actions/
│   │       ├── ActionStepConfig.cs       # Abstract SO base
│   │       ├── ActionStepBehavior.cs     # Abstract behavior base
│   │       ├── CollectClueConfig.cs      # + CollectClueBehavior
│   │       ├── ShowDialogueConfig.cs     # + ShowDialogueBehavior
│   │       ├── ShowToastConfig.cs        # + ShowToastBehavior
│   │       ├── ClosePopupConfig.cs       # + ClosePopupBehavior
│   │       ├── WaitConfig.cs             # + WaitBehavior
│   │       └── ShowClueDetailConfig.cs   # + ShowClueDetailBehavior
│   │
│   ├── Managers/
│   │   ├── GameDataManager.cs        # SingletonSaveLoad, clue/item/interaction tracking
│   │   ├── GameFlowController.cs     # Singleton, game flow orchestration
│   │   ├── DialogueManager.cs        # Singleton, typewriter, line progression
│   │   ├── InvestigationManager.cs   # Singleton, room loading, object click dispatch
│   │   └── NotebookManager.cs        # Singleton, clue/character database queries
│   │
│   ├── UI/
│   │   ├── Screens/
│   │   │   ├── UIMainMenu.cs         # Play/Continue/Settings/Guide/Exit
│   │   │   ├── UICutscene.cs         # VideoPlayer + skip button
│   │   │   ├── UINPCDialogue.cs      # Portrait + name + typewriter text
│   │   │   ├── UIMapSelection.cs     # Grid of MapSelectionItems
│   │   │   ├── UIInvestigation.cs    # Background + interactables + camera pan
│   │   │   └── UINotebook.cs         # Tab switching (Clues/Characters)
│   │   ├── Popups/
│   │   │   ├── UISettings.cs         # Music/SFX sliders
│   │   │   ├── UIGuide.cs            # Static scroll content
│   │   │   ├── UIClueDetail.cs       # Clue info + fly-to-notebook animation
│   │   │   ├── UILockPuzzle.cs       # Passcode input + panel switching
│   │   │   └── UIConfirmExit.cs      # Yes/No confirm
│   │   ├── HUD/
│   │   │   └── UIInvestigationHud.cs # Settings/Notebook buttons, clue counter
│   │   └── Components/
│   │       ├── InteractableObject.cs     # IPointerClick, hover scale, click dispatch
│   │       ├── CameraPanController.cs    # IDragHandler, pan background
│   │       ├── ClueCollectAnimation.cs   # Static fly-to-notebook DOTween sequence
│   │       ├── MapSelectionItem.cs       # Map thumbnail + name + click
│   │       ├── NotebookClueItem.cs       # Clue item in notebook list
│   │       └── NotebookCharacterItem.cs  # Character item in notebook list
│   │
│   └── GameBootstrap.cs             # Entry point, Start() → ShowMainMenu
│
├── Editor/
│   ├── DoMiTruthSetupTool.cs        # EditorWindow (menu UI)
│   └── DoMiTruthAutoSetup.cs        # Auto-create SOs, prefabs, scene, wiring
│
└── Prefabs/        # (auto-generated by Editor tool)
    Resources/      # (auto-generated by Editor tool)
```

---

## 3. UIName ENUM (Clean, Re-indexed)

```csharp
public enum UIName
{
    None = 0,
    Toast = 1,
    Loading = 2,
    MainMenu = 10,
    Cutscene = 11,
    NPCDialogue = 12,
    MapSelection = 13,
    Investigation = 14,
    Notebook = 15,
    Settings = 20,
    Guide = 21,
    ClueDetail = 22,
    LockPuzzle = 23,
    ConfirmExit = 24,
    InvestigationHud = 30,
}
```

---

## 4. GAME FLOW

```
GameBootstrap.Start()
  └→ GameFlowController.ShowMainMenu()
      └→ UIManager.ShowUI<UIMainMenu>(MainMenu)

[Play] → GameFlowController.StartNewGame()
  └→ ShowCutscene() → UIManager.ShowUI<UICutscene>(Cutscene)
      └→ OnCutsceneComplete() → ShowMapSelection()
          └→ UIManager.ShowUI<UIMapSelection>(MapSelection)

[Select Map] → GameFlowController.OnMapSelected(MapSO)
  └→ InvestigationManager.LoadRoom(room)
      ├→ UIManager.ShowUI<UIInvestigation>(Investigation)
      ├→ UIManager.ShowUI<UIInvestigationHud>(InvestigationHud)
      └→ Play entryDialogue (if any)

[Click Object] → InvestigationManager.OnObjectClicked()
  ├→ Clue: ShowUI<UIClueDetail> + GameDataManager.AddClue()
  ├→ NPC: DialogueManager.StartDialogue()
  └→ LockedItem: ShowUI<UILockPuzzle>
       ├→ Success → ExecuteActionChain(onUnlockSuccess)
       └→ Fail → ExecuteActionChain(onUnlockFail)

[Continue] → GameFlowController.ContinueGame()
  └→ Load saved map/room → resume investigation
```

---

## 5. DATA FLOW

### Clue Collection Flow
```
InteractableObject.OnClick
  → InvestigationManager.HandleClue()
    → GameDataManager.AddClue(clueId)        [Singleton direct]
      → Save to PlayerPrefs
      → onClueCollected.Raise(clueId)        [SO Event broadcast]
        → UIInvestigationHud: update counter
        → NotebookManager: mark refresh needed
    → UIManager.ShowUI<UIClueDetail>(clue)   [Singleton direct]
      → OnClose: ClueCollectAnimation.Play() → fly to notebook icon
```

### Lock Puzzle Flow (Item-level only)
```
InteractableObject.OnClick (LockedItem)
  → InvestigationManager.HandleLockedItem()
    → UIManager.ShowUI<UILockPuzzle>
      → Init(lockConfig, onSuccess, onFail)
      → User enters passcode
      → Correct: onSuccess() → ExecuteActionChain(configs)
        → CollectClueConfig.CreateBehavior().Execute()
        → ShowToastConfig.CreateBehavior().Execute()
        → ...
      → Wrong: onFail() → DOShakePosition + error text
```

### Save/Load Flow
```
GameDataManager : SingletonSaveLoad<GameSaveData, GameDataManager>
  KEYLOAD = "DMT_SAVE"
  Load: Awake() → auto-load from PlayerPrefs (JSON)
  Save: AddClue/UnlockItem/MarkInteracted → Save()
  Auto-save: OnApplicationFocus(false) → Save()
```

---

## 6. KEY PATTERNS & CONVENTIONS

### Naming
- UI classes: `UI` prefix (UIMainMenu, UISettings, UIClueDetail)
- ScriptableObjects: suffix `SO` (ClueSO, RoomSO, GameConfigSO)
- Events: suffix `Channel` (GameEventChannel, StringEventChannel)
- Managers: suffix `Manager` (GameDataManager, InvestigationManager)
- Config+Behavior: suffix `Config`/`Behavior` (CollectClueConfig, CollectClueBehavior)

### UI Animation
- **TweenAnimationCaller** on UIBase prefab → reference TweenAnimationBase
- Show animation: `showAnimation.CallShow()` triggered in `UIBase.Show()`
- Hide animation: `hideAnimation.CallShow()` → wait → `ExecuteHide()`
- Popup default: Scale animation (0.85 → 1.0, EaseOutBack)
- **EffectButton** component on all buttons (scale 1.1x on press)

### State Switching (BaseSelect)
- `SelectToggleGameObject.Select(bool)` → show/hide groups
  - Continue button visibility, notebook badge, lock indicator
- `SelectSwitchGameObject.Select(int)` → switch panels by index
  - Notebook tabs (0=Clues, 1=Characters), LockPuzzle panels (0=Passcode, 1=SwipePattern)
- `SelectToggleImage.Select(bool)` → sprite swap for active/inactive tab

### Button Click Setup
```csharp
GameUtil.ButtonOnClick(button, callback);  // standard click with animation
```

### Typewriter Effect
```csharp
TweenExtension.DOSetTextCharByChar(tmpText, fullText, charsPerSec);
```

---

## 7. SO ASSETS (Auto-generated by Editor Tool)

### Core
- `Assets/Luzart/DoMiTruth/Resources/UIRegistry.asset` (UIRegistrySO)
- `Assets/Luzart/DoMiTruth/Resources/GameConfig.asset` (GameConfigSO)

### SO Events
- `Assets/Luzart/DoMiTruth/Resources/Events/OnClueCollected.asset` (StringEventChannel)
- `Assets/Luzart/DoMiTruth/Resources/Events/OnNotebookUpdated.asset` (GameEventChannel)
- `Assets/Luzart/DoMiTruth/Resources/Events/OnSettingsChanged.asset` (GameEventChannel)

### Sample Data (9 Rooms, 3 Maps)
```
Resources/Data/
├── Characters/     (5 DialogueCharacterSO)
├── Clues/          (12 ClueSO)
├── Dialogues/      (4 DialogueSequenceSO)
├── Locks/          (3 LockConfigSO)
├── Actions/        (action step configs)
├── Interactables/  (14 InteractableObjectSO)
├── Rooms/          (9 RoomSO)
└── Maps/           (3 MapSO)
```

---

## 8. SCENE HIERARCHY (Auto-generated)

```
MainScene
├── Canvas (ScreenSpaceOverlay, 1080x1920, ScaleWithScreenSize)
│   ├── Layer_Screen (0)
│   ├── Layer_Popup (1)
│   ├── Layer_HUD (2)
│   ├── Layer_System (3)
│   └── Layer_Toast (4)
├── UIManager (+ UIRegistrySO reference)
├── GameFlowController (+ GameConfigSO reference)
├── GameDataManager (+ SO Event references)
├── DialogueManager
├── InvestigationManager
├── NotebookManager (+ allClues[], allCharacters[])
├── GameBootstrap
└── EventSystem
```

---

## 9. EXTENDING THE GAME

### Them Action moi
1. Tao `NewActionConfig : ActionStepConfig` (SO, CreateAssetMenu)
2. Tao `NewActionBehavior : ActionStepBehavior` (Execute IEnumerator)
3. Config.CreateBehavior() → return new Behavior(data)
4. Tao SO asset, keo vao InteractableObjectSO.onUnlockSuccess/onUnlockFail

### Them UI moi
1. Them entry vao `UIName` enum
2. Tao class ke thua `UIBase`
3. Tao prefab voi UIBase component
4. Them entry vao UIRegistrySO asset (prefab + layer + cache)
5. Goi `UIManager.Instance.ShowUI<NewUI>(UIName.NewUI)`

### Them Room moi
1. Tao InteractableObjectSO assets cho cac object trong room
2. Tao RoomSO asset, set background + interactables list
3. Them RoomSO vao MapSO.rooms list

### Them Lock Puzzle moi
1. Tao LockConfigSO asset (lockType, passcode, hint)
2. Tao action chain SOs (CollectClueConfig, ShowToastConfig, etc.)
3. Set InteractableObjectSO.interactType = LockedItem
4. Assign lockConfig + onUnlockSuccess + onUnlockFail lists
