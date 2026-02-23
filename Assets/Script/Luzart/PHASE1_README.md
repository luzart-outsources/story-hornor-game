# ĐỠ-MI TRUTH - PHASE 1 SETUP GUIDE

## 📋 Overview
Phase 1 Foundation Setup đã hoàn thành các tasks cơ bản để thiết lập nền tảng cho dự án game trinh thám "Đỡ-Mi Truth".

## ✅ Completed Tasks

### TASK 1.1: Project Structure Setup ✓
Đã tạo folder structure hoàn chỉnh cho project:

#### Script Folders:
```
Assets/Script/Luzart/
├── Core/                           # Core enums và classes
├── Data/ScriptableObjects/         # Tất cả ScriptableObjects
├── Game/
│   ├── UI/
│   │   ├── Screens/               # UI Screens (MainMenu, Investigation...)
│   │   ├── Popups/                # UI Popups (Settings, ClueDetail...)
│   │   └── Huds/                  # UI Huds (InvestigationHud...)
│   ├── Controllers/               # Game controllers
│   ├── Managers/                  # Game managers
│   ├── Input/                     # Input strategies
│   ├── Animation/                 # Animation helpers
│   ├── Events/                    # Game events
│   └── Components/                # MonoBehaviour components
```

#### Prefab Folders:
```
Assets/Prefabs/UI/
├── Screens/                       # Screen prefabs
├── Popups/                        # Popup prefabs
├── Huds/                          # Hud prefabs
└── Components/                    # Reusable UI components
```

#### Resources Folders:
```
Assets/Resources/ScriptableObjects/
├── Dialogues/
│   ├── Characters/               # DialogueCharacterSO assets
│   └── Sequences/                # DialogueSequenceSO assets
├── Clues/                        # ClueSO assets
├── InteractableObjects/          # InteractableObjectSO assets
├── Rooms/                        # RoomSO assets
├── Maps/                         # MapSO assets
└── Config/                       # Game config SOs
```

### TASK 1.2: Create Enums File ✓
**File:** `Assets/Script/Luzart/Core/GameEnums.cs`

Đã tạo tất cả enums cần thiết cho game:

#### 📌 LockType
- `None`: Không khóa
- `Passcode`: Khóa bằng mật mã
- `SwipePuzzle`: Khóa bằng puzzle vuốt
- `KeyItem`: Khóa bằng vật phẩm

#### 📌 ClueCategory
- `Evidence`: Bằng chứng vật chất
- `Document`: Tài liệu, thư từ
- `Weapon`: Hung khí
- `Testimony`: Lời khai
- `Other`: Khác

#### 📌 EntryCategory
- `Clue`: Manh mối
- `Character`: Nhân vật
- `Note`: Ghi chú

#### 📌 EncounterTiming
- `BeforeEntry`: Trước khi vào phòng
- `AfterEntry`: Sau khi vào phòng
- `OnObjectInteract`: Khi tương tác với object cụ thể

#### 📌 AnimationType
- `Scale`: Phóng to/thu nhỏ
- `Fade`: Mờ dần
- `Slide`: Trượt
- `Rotate`: Xoay
- `Custom`: Tùy chỉnh

#### 📌 GameState
- `MainMenu`: Màn menu chính
- `Cutscene`: Đang xem cutscene
- `NPCDialogue`: Đang đối thoại với NPC
- `MapSelection`: Đang chọn map
- `Investigation`: Đang điều tra
- `Paused`: Game tạm dừng

### TASK 1.3: Create Game Events File ✓
**File:** `Assets/Script/Luzart/Game/Events/GameEvents.cs`

Đã tạo tất cả events cho EventBus communication:

#### 🎯 Core Game Flow Events:
- `CutsceneCompletedEvent`: Khi cutscene kết thúc
- `BriefingCompletedEvent`: Khi briefing (giao nhiệm vụ) kết thúc
- `MapSelectedEvent`: Khi player chọn một map
- `RoomCompletedEvent`: Khi hoàn thành điều tra phòng
- `GameStateChangedEvent`: Khi game state thay đổi

#### 🎯 Investigation Events:
- `ClueCollectedEvent`: Khi thu thập được clue
- `RoomUnlockedEvent`: Khi unlock được room
- `ObjectClickedEvent`: Khi click vào object

#### 🎯 Dialogue Events:
- `DialogueStartedEvent`: Khi dialogue bắt đầu
- `DialogueCompletedEvent`: Khi dialogue kết thúc
- `NPCEncounteredEvent`: Khi gặp NPC

#### 🎯 System Events:
- `SettingsChangedEvent`: Khi game settings thay đổi

### TASK 1.5: Create SaveLoadManager Interface ✓
**File:** `Assets/Script/Luzart/Game/Managers/ISaveLoadManager.cs`

Đã tạo interface và data structures cho Save/Load system:

#### 📦 ISaveLoadManager Interface:
```csharp
void SaveGame(SaveData data);
SaveData LoadGame();
bool HasSaveData();
void DeleteSaveData();
```

#### 📦 SaveData Structure:
- `collectedClueIds`: List các clue đã thu thập
- `unlockedMapIds`: List các map đã unlock
- `roomCompletionState`: Dictionary trạng thái hoàn thành room
- `settings`: Game settings
- `currentState`: Game state hiện tại
- `saveTimestamp`: Thời gian save
- `saveVersion`: Version của save data

#### 📦 GameSettings Structure:
- `musicVolume`: Âm lượng nhạc nền
- `sfxVolume`: Âm lượng hiệu ứng
- `language`: Ngôn ngữ game
- `enableHints`: Bật/tắt gợi ý

**Note:** Interface này chưa được implement, để dành cho tương lai khi cần thêm Save/Load functionality.

---

## 🚀 Next Steps (Tasks còn lại trong Phase 1)

### TASK 1.4: UIRegistry Setup
- [ ] Tạo UIRegistry asset trong Unity Editor
- [ ] Configure layers (Hud, Screen, Popup, System)

### TASK 1.6: Setup UIManager in Scene
- [ ] Setup UIManager GameObject với proper hierarchy
- [ ] Configure UICanvas với 4 layers
- [ ] Setup EventSystem

### TASK 1.7: Create GameManagers GameObject
- [ ] Tạo [GameManagers] container với DontDestroyOnLoad
- [ ] Chuẩn bị để thêm các Manager components

### TASK 1.8: Test UIFramework Integration
- [ ] Test UIFramework hoạt động đúng
- [ ] Mở UI Debug Window để kiểm tra
- [ ] Verify không có errors

---

## 📝 Usage Guidelines

### Namespace Convention
Tất cả code game sử dụng namespace `Luzart`:
```csharp
using Luzart.Core;              // Enums, core classes
using Luzart.Game.Events;       // Game events
using Luzart.Game.Managers;     // Managers
```

### Naming Conventions
- **SerializeField variables**: camelCase (`[SerializeField] private string characterId`)
- **Classes**: PascalCase (`DialogueSequenceSO`, `GameStateManager`)
- **Methods**: PascalCase (`LoadRoom()`, `OnObjectClicked()`)
- **Properties/Public fields**: PascalCase (`CurrentRoom`, `IsLocked`)
- **Constants**: UPPER_SNAKE_CASE (`const float FADE_DURATION = 0.3f`)
- **Private fields**: camelCase (`private RoomSO currentRoom`)

### Event Usage Example
```csharp
using UIFramework.Communication;
using Luzart.Game.Events;

// Publish event
EventBus.Publish(new ClueCollectedEvent 
{ 
    ClueId = "clue_001", 
    ClueName = "Blood Stain" 
});

// Subscribe to event
EventBus.Subscribe<ClueCollectedEvent>(OnClueCollected);

private void OnClueCollected(ClueCollectedEvent evt)
{
    Debug.Log($"Collected: {evt.ClueName}");
}
```

---

## 🎯 Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                           │
│  UIFramework (ĐÃ CÓ) + Game UI Screens (SẼ TẠO)               │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    GAME FLOW LAYER (SẼ TẠO)                     │
│  GameFlowController, GameStateManager, SaveLoadManager          │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│              DATA LAYER (ScriptableObjects - SẼ TẠO)            │
│  Dialogue, Clues, Rooms, Maps, Configs                         │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    INPUT LAYER (SẼ TẠO)                         │
│  InvestigationInputStrategy, MenuInputStrategy                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📚 Documentation References
- Technical Requirement: `Assets/Script/GDD/TechnicalRequirement.txt`
- Game Design Document: `Assets/Script/GDD/GDD.txt`
- Development Process: `Assets/Script/GDD/GameDevelopmentProcess.txt`
- Project Progress: `Assets/Script/GDD/ProjectProgress.txt`
- UIFramework Docs: `Assets/Script/UIFramework/README.md`

---

## ⚠️ Important Notes
1. **UIFramework** đã tồn tại và hoạt động - KHÔNG thay đổi code UIFramework
2. **Namespace Luzart** cho tất cả game code
3. **EventBus** từ UIFramework được sử dụng cho communication
4. **ScriptableObjects** sẽ được tạo trong Resources để dễ load runtime
5. **Loosely Coupled Design** - các ScriptableObjects reference nhau qua ID (string) khi cần

---

**Status:** Phase 1 - 50% Complete (4/8 tasks done)
**Next Task:** TASK 1.4 - UIRegistry Setup (requires Unity Editor)
**Last Updated:** 2024-02-23
