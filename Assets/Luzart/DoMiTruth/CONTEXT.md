# CONTEXT FILE - DO MI TRUTH
# Game trinh tham Point & Click 2D tren Unity

---

## 1. TONG QUAN DU AN

- **Ten game:** Do-Mi Truth
- **The loai:** Trinh tham, Kinh di, Giai do (Point & Click)
- **Phong cach:** Dong Duong (Indochine) co dien, tong mau tram toi
- **Nhan vat chinh:** Ngai Loc (Tham tu)
- **Muc tieu:** Kham pha dinh thu, thu thap bang chung de tim ra su that vu an mang
- **Platform:** Windows, man hinh ngang 2D, toan bo tren UI Canvas
- **Engine:** Unity (DOTween, TextMeshPro)

---

## 2. KIEN TRUC DU AN

### Folder Structure
```
Assets/
  Luzart/
    DoMiTruth/
      Data/
        Actions/       -> ActionStep configs (Wait, ShowToast, CollectClue, ShowDialogue, ClosePopup, ShowClueDetail)
        Characters/    -> 5 chars: Butler, Detective, Neighbor, Police, Wife
        Clues/         -> 13 clues (BrokenVase, Diary, Footprint, GardenTool, Knife, Letter, Medicine, Phone, Photo, Ring, Testimony1, Testimony2)
        Config/        -> GameConfig.asset, UIRegistry.asset
        Dialogues/     -> 4 dialogues: Dlg_Butler, Dlg_Intro, Dlg_Neighbor, Dlg_Wife
        Interactables/ -> 14+ interactive objects (IO_*)
        BriefingChars/ -> BriefingCharacterSO per NPC (full body config)
        Locks/         -> 3 locks: Lock_Cabinet, Lock_Drawer, Lock_Safe
        Maps/          -> 3 maps: Map_Floor1, Map_Floor2, Map_Garden
        Rooms/         -> 9 rooms: Bathroom, FishPond, FrontYard, GuestRoom, Kitchen, LivingRoom, MasterBedroom, Shed, Study
        Events/        -> OnNotebookUpdated, OnSettingsChanged, OnClueCollected
      Scripts/
        Data/          -> ScriptableObject definitions
        Data/Actions/  -> Action step system (Command Pattern)
        Managers/      -> GameFlowController, GameDataManager, DialogueManager, InvestigationManager, NotebookManager
        UI/Screens/    -> UIMainMenu, UIMapSelection, UIInvestigation, UICutscene, UINPCDialogue, UINotebook, UIBriefing
        UI/Popups/     -> UILockPuzzle, UIClueDetail, UISettings, UIGuide, UIConfirmExit, UIPause
        UI/HUD/        -> UIInvestigationHud
        UI/Components/ -> InteractableObject, CameraPanController, MapSelectionItem, NotebookClueItem, NotebookCharacterItem, ClueCollectAnimation, SwipePatternDot, ButtonHoverSelect
        Editor/        -> DoMiTruthOneClickSetup (MainMenuPrefabCreator.cs)
      Prefabs/
        Screens/       -> UIMainMenu, UIMapSelection, UIInvestigation, UICutscene, UINPCDialogue, UINotebook, UIBriefing
        Popups/        -> UIClueDetail, UILockPuzzle, UISettings, UIGuide, UIConfirmExit, UIPause
        HUD/           -> UIInvestigationHud
        Components/    -> InteractableObject, MapSelectionItem, NotebookClueItem, NotebookCharacterItem
        System/        -> UILoading, UIToast
    Utility/
      Script/
        UIBase/        -> UIBase.cs, UIManager.cs (+ enum UIName), UIRegistrySO.cs
        NewBaseSelect/ -> BaseSelect.cs, SelectToggleGameObject.cs, SelectSwitchGameObject.cs, + other variants
        Other/         -> GameUtil.cs (Singleton, ButtonOnClick, etc.), SaveLoadUtil.cs, TimeUtils.cs
        Events/        -> GameEventChannel.cs (parameterless), StringEventChannel.cs (string param), Observer.cs
        TweenExtension/ -> TweenExtension.cs, AutoDOFloatTween.cs
        EffectButton/  -> EffectButton.cs, EffectButtonRotate.cs
        ProgressBar/   -> ProgressBarUI.cs
      Prefabs/         -> Button, Noti, etc.
      Resources/       -> UIToast, SpriteResourcesSO
  Scenes/
    MainScene.unity    -> Single scene chua tat ca managers
  Resources/
    UIRegistry.asset, MainUIRegistry.asset, DOTweenSettings.asset
  Plugins/
    Demigiant/DOTween  -> Animation library
  TextMesh Pro/
```

---

## 3. GAME FLOW (Man choi)

```
GameBootstrap (Start)
  |
  v
GameFlowController
  |
  +--> Man 1: ShowMainMenu() --> UIMainMenu
  |      Play -> StartNewGame() -> ResetData -> ShowCutscene
  |      Continue -> ContinueGame() -> ShowMapSelection
  |      Settings -> popup UISettings
  |      Guide -> popup UIGuide
  |      Exit -> popup UIConfirmExit
  |
  +--> Man 2: ShowCutscene() --> UICutscene
  |      VideoPlayer 30s, skip button (delay 3s)
  |      OnCutsceneComplete -> ShowBriefing
  |
  +--> Man 3: ShowBriefing() --> UIBriefing (FULL SCREEN)
  |      NPC Trai: full body ben trai (BriefingCharacterSO) -> noi trong Case Board
  |      NPC Phai: portrait nho duoi phai (DialogueCharacterSO) -> noi trong Dialogue Box
  |      Auto-detect 2 characters tu dialogue lines
  |      Highlight NPC dang noi (CanvasGroup alpha)
  |      Ket thuc -> OnBriefingComplete -> ShowMapSelection
  |      Fallback: neu chua co prefab -> dung DialogueManager
  |
  +--> Man 4: ShowMapSelection() --> UIMapSelection
  |      Grid cac map thumbnail
  |      Click map -> OnMapSelected -> LoadRoom + ShowHUD
  |
  +--> Gameplay: Investigation
         UIInvestigation (background + interactable objects)
         UIInvestigationHud (settings btn -> UIPause, notebook btn, clue counter)
         InteractableObject click -> InvestigationManager handles:
           - Clue -> UIClueDetail -> fly animation -> notebook
           - NPC -> DialogueManager -> UINPCDialogue
           - LockedItem -> UILockPuzzle -> action chain on success/fail
           - Decoration -> nothing
```

---

## 4. DATA STRUCTURES (ScriptableObjects)

### GameConfigSO
```
- introCutscene: VideoClip
- cutsceneDuration: float (30s)
- skipButtonDelay: float (3s)
- briefingDialogue: DialogueSequenceSO       -> Dlg_Intro cho man Cong an
- briefingCharacters: List<BriefingCharacterSO> -> Full body NPC configs cho Briefing
- allMaps: List<MapSO>
- panSpeed: float (5)
- panEdgeThreshold: float (50px)
- defaultTypingSpeed: float (30 chars/sec)
- clueCollectFlyDuration: float (0.8s)
```

### MapSO
```
- mapId: string
- mapName: string
- mapThumbnail: Sprite
- rooms: List<RoomSO>
```

### RoomSO
```
- roomId: string
- roomName: string
- backgroundImage: Sprite
- backgroundSize: Vector2 (default 1920x1080)
- entryDialogue: DialogueSequenceSO (nullable)
- interactables: List<RoomInteractable>
  - data: InteractableObjectSO
  - position: Vector2
```

### InteractableObjectSO
```
- objectId: string
- interactType: InteractType (Clue / NPC / LockedItem / Decoration)
- hitboxSize: Vector2
- highlightSprite: Sprite
- isOneTimeOnly: bool
- clue: ClueSO               (if Clue)
- dialogue: DialogueSequenceSO (if NPC)
- lockConfig: LockConfigSO    (if LockedItem)
- onSuccessActions: List<ActionStepConfig>
- onFailActions: List<ActionStepConfig>
```

### ClueSO
```
- clueId: string
- clueName: string
- clueImage: Sprite
- description: string
- category: ClueCategory (Physical / Document / Testimony / Photo)
```

### DialogueSequenceSO
```
- dialogueId: string
- lines: List<DialogueLine>
- autoAdvance: bool
- autoAdvanceDelay: float (2s)
```

### DialogueLine
```
- character: DialogueCharacterSO
- text: string
- typingSpeed: float (30)
- waitForInput: bool (true)
```

### DialogueCharacterSO
```
- characterId: string
- characterName: string
- portrait: Sprite                           (fallback khi khong co portraitAnimator)
- portraitAnimator: RuntimeAnimatorController (optional - animator cho portrait nho trong dialogue box)
- nameColor: Color
```

### BriefingCharacterSO (chi dung cho man Briefing)
```
- character: DialogueCharacterSO             (link de map characterId)
- fullBodySprite: Sprite                     (fallback khi khong co fullBodyAnimator)
- fullBodyAnimator: RuntimeAnimatorController (optional - animator cho full body NPC)
```
Logic: Neu co AnimatorController -> dung Animator. Neu null -> dung Sprite tinh.

### LockConfigSO
```
- lockType: LockType (Passcode / SwipePattern)
- passcode: string                -> cho Passcode
- swipePattern: int[]             -> cho SwipePattern (index 0-8, grid 3x3)
- hintText: string
```

### GameSaveData
```
- collectedClueIds: List<string>
- unlockedItemIds: List<string>
- interactedObjectIds: List<string>
- metCharacterIds: List<string>
- currentMapId: string
- currentFlowStep: int
- musicVolume: float (0-1, default 1)
- sfxVolume: float (0-1, default 1)
Save key: "DMT_SAVE"
```

---

## 5. MANAGERS

### GameFlowController (Singleton)
- Quan ly toan bo flow chuyen man
- StartNewGame, ContinueGame, ShowMainMenu, ShowCutscene, ShowBriefing, ShowMapSelection, OnMapSelected, ReturnToMainMenu
- ShowBriefing: hien UIBriefing full screen, khi xong -> ShowMapSelection

### GameDataManager (SingletonSaveLoad, key "DMT_SAVE")
- AddClue, UnlockItem, MarkInteracted, MarkCharacterMet
- HasClue, HasUnlockedItem, HasInteracted, HasMetCharacter, HasSaveData
- ResetData
- Events: onClueCollected (StringEventChannel), onNotebookUpdated (GameEventChannel)

### DialogueManager (Singleton)
- StartDialogue(DialogueSequenceSO, onComplete)
- ShowNextLine, SkipTyping, EndDialogue
- SetTypingTweener -> DOTween char-by-char
- Dung cho in-game NPC dialogue (UIName.NPCDialogue)

### InvestigationManager (Singleton)
- LoadRoom(RoomSO) -> UIInvestigation
- OnObjectClicked -> HandleClue / HandleNPC / HandleLockedItem / HandleDecoration
- ExecuteActionChain(List<ActionStepConfig>) -> coroutine sequential

### NotebookManager (Singleton)
- GetCollectedClues, GetMetCharacters
- GetCollectedClueCount, GetTotalClueCount

---

## 6. UI SYSTEM

### UIManager (Singleton)
- Registry-based: UIRegistrySO chua mapping UIName -> prefab + layer
- Layers: 0=Screen, 1=Popup, 2=HUD, 3=System, 4=Toast
- ShowUI<T>(UIName), HideUiActive(UIName), HideAll, HideAllUiActive
- ShowToast(string)

### UIName Enum
```
None=0, Toast=1, Loading=2,
MainMenu=10, Cutscene=11, NPCDialogue=12, MapSelection=13, Investigation=14, Notebook=15, Briefing=16,
Settings=20, Guide=21, ClueDetail=22, LockPuzzle=23, ConfirmExit=24, Pause=25,
InvestigationHud=30
```

### UIBase
- uiName, closeBtn, isCache
- Show(onHideDone), Hide(), RefreshUI()
- showAnimation / hideAnimation (TweenAnimationCaller)
- Setup() called once on first Show

### Screens
| Screen | Mo ta |
|--------|-------|
| UIMainMenu | 5 buttons (Play, Continue, Settings, Guide, Exit) + ButtonHoverSelect group |
| UICutscene | VideoPlayer + skip button |
| UIBriefing | Full screen: NPC trai full body + case board (text NPC trai) + dialogue box duoi phai (text NPC phai + portrait). Auto-detect 2 chars |
| UIMapSelection | Grid MapSelectionItem, click -> OnMapSelected |
| UIInvestigation | Room background + spawn InteractableObjects + CameraPanController |
| UINPCDialogue | Portrait + name + text (typing anim) + next button |
| UINotebook | 2 tabs: Clues / Characters |

### Popups
| Popup | Mo ta |
|-------|-------|
| UISettings | Music slider + SFX slider |
| UIGuide | Static help content |
| UIClueDetail | Clue image + description + close -> fly animation |
| UILockPuzzle | Passcode (text input) + SwipePattern (3x3 grid dots) |
| UIConfirmExit | Yes/No exit confirmation |
| UIPause | Resume / Option (-> Settings) / Exit Game (-> MainMenu). Hien khi bam Settings trong Investigation |

### HUD
| HUD | Mo ta |
|-----|-------|
| UIInvestigationHud | Settings btn (-> UIPause), Notebook btn (badge), Clue counter (X/Total) |

### Components
| Component | Mo ta |
|-----------|-------|
| InteractableObject | Hotspot click + hover highlight + scale effect |
| CameraPanController | Pan/drag camera over room background |
| MapSelectionItem | Map thumbnail button |
| NotebookClueItem | Clue entry in notebook |
| NotebookCharacterItem | Character portrait in notebook |
| ClueCollectAnimation | Fly sprite from clue to notebook (0.8s) |
| SwipePatternDot | Single dot in 3x3 grid, handles pointer events |
| ButtonHoverSelect | Hover -> SelectToggleGameObject(true), exit -> false. Group managed |

---

## 7. BASE SELECT SYSTEM (Utility)

### Hierarchy
```
BaseSelect (abstract) -> IBaseSwitch, IBaseToggle
  BaseSelect<T> (abstract generic)
    SelectToggle -> ISelectBoolCache
      SelectToggleGameObject -> obSelect[] / obUnSelect[] (bat/tat GameObjects)
      SelectToggleImage, SelectToggleTMP_Text, SelectToggleUnityEvent
    SelectSwitch
      SelectSwitchGameObject -> GroupGameObject[] (bat/tat theo index)
      SelectSwitchImage, SelectSwitchTMP_Text, SelectSwitchUnityEvent
```

### SelectToggleGameObject
- Select(true) -> bat obSelect[], tat obUnSelect[]
- Select(false) -> tat obSelect[], bat obUnSelect[]
- Dung cho: button hover (normal/raised), continue toggle, notebook badge, etc.

### SelectSwitchGameObject
- Select(index) -> bat group[index], tat tat ca group khac
- Dung cho: UILockPuzzle panel switching, tab switching

---

## 8. ACTION CHAIN SYSTEM (Command Pattern)

### Base
- ActionStepConfig (abstract SO) -> CreateBehavior() -> ActionStepBehavior
- ActionStepBehavior (abstract) -> Execute() coroutine

### Implementations
| Config | Behavior | Mo ta |
|--------|----------|-------|
| WaitConfig | WaitBehavior | Cho X giay |
| ShowToastConfig | ShowToastBehavior | Hien toast message |
| ShowDialogueConfig | ShowDialogueBehavior | Bat dau dialogue |
| ClosePopupConfig | ClosePopupBehavior | Dong popup hien tai |
| ShowClueDetailConfig | ShowClueDetailBehavior | Hien chi tiet clue |
| CollectClueConfig | CollectClueBehavior | Tu dong thu thap clue |

### Usage
- InteractableObjectSO co onSuccessActions va onFailActions (List<ActionStepConfig>)
- InvestigationManager.ExecuteActionChain() chay tuan tu tung step

---

## 9. EVENT SYSTEM

### SO-based Events
- GameEventChannel (parameterless): onSettingsChanged, onNotebookUpdated
- StringEventChannel (string param): onClueCollected
- Register/Unregister pattern

### Observer Pattern
- Observer.Instance.AddObserver/RemoveObserver
- Dung cho BlockRaycast, etc.

---

## 10. SAVE / LOAD

- GameDataManager extends SingletonSaveLoad<GameSaveData, GameDataManager>
- Save key: "DMT_SAVE" (PlayerPrefs JSON)
- Tracks: collectedClueIds, unlockedItemIds, interactedObjectIds, metCharacterIds, currentMapId, volumes

---

## 11. ANIMATION SYSTEMS

| Animation | Script | Chi tiet |
|-----------|--------|----------|
| Clue collect fly | ClueCollectAnimation | Scale out -> move to notebook -> scale down + fade. 0.8s |
| Text typing | UINPCDialogue / UIBriefing | DOTween char-by-char. 30 chars/sec default |
| NPC character anim | UIBriefing / UINPCDialogue | RuntimeAnimatorController (optional). Fallback to static Sprite |
| Interactive hover | InteractableObject | Scale 1.0 -> 1.05 |
| Wrong answer shake | UILockPuzzle | DOShakePosition 0.3s, 10px, 20 vibrato |
| Button hover | ButtonHoverSelect | SelectToggleGameObject bat/tat Normal/Hover GO. Hover scale 1.06x |
| Screen open/close | UIBase | TweenAnimationCaller (optional, designer lam) |

---

## 12. EDITOR TOOLS

### DoMiTruthOneClickSetup (MainMenuPrefabCreator.cs)
Menu: **DoMiTruth -> One Click Setup All**

Tu dong lam:
1. Tao UIMainMenu prefab (5 buttons + hover SelectToggleGameObject)
2. Tao UIPause prefab (Resume / Option / Exit Game)
3. Tao UIBriefing prefab (NPC body + case board + dialogue)
4. Dang ky ca 3 vao UIRegistry (dung layer)
5. Set GameConfig.briefingDialogue = Dlg_Intro
6. Set GameConfig.briefingNPCLabel = "POLICE"
7. Set GameConfig.briefingNPCSprite = Police portrait (neu co)

Menu rieng le:
- DoMiTruth -> Create MainMenu Prefab
- DoMiTruth -> Create Pause Prefab
- DoMiTruth -> Create Briefing Prefab

---

## 13. DESIGN PATTERNS SU DUNG

| Pattern | Su dung |
|---------|---------|
| Singleton | Tat ca Managers (GameFlowController, GameDataManager, DialogueManager, InvestigationManager, NotebookManager, UIManager) |
| Command | Action chain system (ActionStepConfig -> ActionStepBehavior) |
| Observer | SO-based events (GameEventChannel, StringEventChannel), Observer.cs |
| Strategy | InteractType switch trong InvestigationManager |
| State Machine | Game flow states (MainMenu -> Cutscene -> Briefing -> MapSelection -> Investigation) |
| Registry | UIRegistrySO mapping UIName -> prefab |
| Data-Driven | ScriptableObjects cho tat ca data (clue, room, map, dialogue, lock, character) |

---

## 14. THU VIEN THIRD-PARTY

| Library | Su dung |
|---------|---------|
| DOTween | Animation (text typing, shake, fly, scale, fade) |
| TextMeshPro | UI text rendering |
| Unity UI | Canvas-based UI system |

---

## 15. SO LIEU DU AN

| Loai | So luong |
|------|---------|
| ScriptableObject assets | 75+ |
| C# Scripts (DoMiTruth) | 45+ |
| C# Scripts (Utility) | 80+ |
| Prefabs (Game) | 18+ |
| Scenes | 1 (MainScene) |
| Rooms | 9 |
| Maps | 3 |
| Characters | 5 |
| Clues | 13 |
| Dialogues | 4 |
| Interactables | 14+ |
| Locks | 3 |
| Action configs | 7 |

---

## 16. LUU Y KHI PHAT TRIEN

1. **Khong viet code qua nhieu** — dung ScriptableObject de designer keo tha
2. **SOLID + Design Patterns** — tach biet cac he thong
3. **DOTween** cho animation — khong tu implement animation mo/dong man (de designer lam qua TweenAnimationBase)
4. **Prefabs** cho tat ca UI screens/popups — de sua va tai su dung
5. **Editor tools** tren menu bar de auto setup
6. **Save/Load** qua PlayerPrefs JSON, key "DMT_SAVE"
7. **Event-driven** — dung SO events de loose coupling giua cac system
8. **BaseSelect system** — dung SelectToggleGameObject/SelectSwitchGameObject cho moi UI state toggle
