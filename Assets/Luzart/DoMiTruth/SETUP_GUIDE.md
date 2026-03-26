# SETUP GUIDE - Do-Mi Truth
## Huong dan cai dat va su dung Editor Tools

---

## BUOC 0: YEU CAU

### Packages can thiet (Package Manager)
- **TextMeshPro** (co san trong Unity)
- **DOTween** (Demigiant) - da co trong project
- **Video Player** (co san) - cho cutscene

### Import TMP Essentials
- Neu chua import: `Window > TextMeshPro > Import TMP Essential Resources`

---

## BUOC 1: SETUP TU DONG (1 NUT BAM)

### Cach 1: Setup tat ca (Khuyen nghi)
```
Menu: Tools > DoMiTruth > Setup All (Complete)
```
Tool se tu dong:
1. Tao Core Assets (UIRegistrySO, GameConfigSO, SO Events)
2. Tao tat ca 15+ UI Prefabs voi component day du
3. Tao Sample Data (5 nhan vat, 12 manh moi, 9 phong, 3 ban do)
4. Setup Scene (Canvas, Managers, Layers, Bootstrap)
5. Wire tat ca references (UIRegistry entries, SO Events)
6. Hien thong bao "Setup complete!"

### Cach 2: Tung buoc rieng le
```
Tools > DoMiTruth > Step 1: Create Core Assets
Tools > DoMiTruth > Step 2: Create UI Prefabs
Tools > DoMiTruth > Step 3: Create Sample Data (9 Rooms)
Tools > DoMiTruth > Step 4: Setup Scene
Tools > DoMiTruth > Step 5: Wire All References
```

### Cach 3: Dung EditorWindow
```
Tools > DoMiTruth > Setup Window
```
- Mo cua so voi cac nut bam truc quan
- Co nut Validate de kiem tra setup

---

## BUOC 2: KIEM TRA SAU SETUP

### 2.1 Kiem tra Scene Hierarchy
Sau khi setup, Scene phai co:
```
Canvas
  ├── Layer_Screen
  ├── Layer_Popup
  ├── Layer_HUD
  ├── Layer_System
  └── Layer_Toast
UIManager         (co UIRegistrySO reference)
GameFlowController (co GameConfigSO reference)
GameDataManager   (co SO Event references)
DialogueManager
InvestigationManager
NotebookManager   (co allClues[], allCharacters[])
GameBootstrap
EventSystem
```

### 2.2 Kiem tra Assets
```
Assets/Luzart/DoMiTruth/
├── Prefabs/           (15+ prefab files)
├── Resources/
│   ├── UIRegistry.asset
│   ├── GameConfig.asset
│   ├── Events/        (3 SO Event assets)
│   └── Data/
│       ├── Characters/ (5 files)
│       ├── Clues/      (12 files)
│       ├── Dialogues/  (4 files)
│       ├── Locks/      (3 files)
│       ├── Actions/    (action configs)
│       ├── Interactables/ (14 files)
│       ├── Rooms/      (9 files)
│       └── Maps/       (3 files)
```

### 2.3 Validate
```
Tools > DoMiTruth > Validate Setup
```
- Kiem tra UIRegistrySO co entries
- Kiem tra prefabs ton tai
- Kiem tra scene co managers
- Bao loi neu thieu gi

---

## BUOC 3: PLAY TEST

1. **Nhan Play** trong Unity Editor
2. **MainMenu** xuat hien (5 nut: Play, Continue, Settings, Guide, Exit)
3. **Play** → Cutscene (hoac skip) → Map Selection
4. **Chon Map** → Investigation screen voi background + objects
5. **Click Object**:
   - Clue → ClueDetail popup → fly-to-notebook animation
   - NPC → Dialogue popup voi typewriter effect
   - Locked Item → LockPuzzle → nhap passcode → action chain
6. **Notebook** → xem clues va characters da gap
7. **Settings** → chinh Music/SFX volume

---

## BUOC 4: TUY CHINH CONTENT

### 4.1 Thay doi noi dung Sample Data

**Sua ten nhan vat:**
- Vao `Resources/Data/Characters/` → click SO asset → sua ten, doi sprite

**Sua manh moi:**
- Vao `Resources/Data/Clues/` → click SO asset → sua description, doi image

**Sua dialogue:**
- Vao `Resources/Data/Dialogues/` → click SO asset
- Mo rong `dialogueLines` list → sua text, doi character reference

**Sua phong:**
- Vao `Resources/Data/Rooms/` → click SO asset
- Doi `backgroundImage` sprite
- Chinh vi tri objects trong `interactables` list

### 4.2 Them noi dung moi

**Them nhan vat moi:**
- Right-click trong Project: `Create > DoMiTruth > Dialogue Character`
- Dien characterId, characterName, keo portrait sprite
- Keo vao NotebookManager.allCharacters[] tren Scene

**Them manh moi moi:**
- Right-click: `Create > DoMiTruth > Clue`
- Dien clueId, clueName, description, chon category
- Keo vao NotebookManager.allClues[]

**Them room moi:**
1. Tao InteractableObjectSO cho moi object trong room
2. Tao RoomSO: `Create > DoMiTruth > Room`
3. Set background sprite + size
4. Them interactables (keo InteractableObjectSO + set position)
5. Them RoomSO vao MapSO.rooms

**Them item khoa moi:**
1. Tao LockConfigSO: `Create > DoMiTruth > Lock Config` (set passcode, hint)
2. Tao action configs (CollectClue, ShowToast, etc.)
3. Tao InteractableObjectSO: set type=LockedItem
4. Keo lockConfig + action chains vao onUnlockSuccess/onUnlockFail

### 4.3 Them Action Step moi (Strategy Pattern)

Neu can action moi (vi du: play sound effect):

1. Tao `PlaySfxConfig.cs`:
```csharp
namespace Luzart
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "DoMiTruth/Actions/Play SFX")]
    public class PlaySfxConfig : ActionStepConfig
    {
        public AudioClip clip;

        public override ActionStepBehavior CreateBehavior()
        {
            return new PlaySfxBehavior(clip);
        }
    }

    public class PlaySfxBehavior : ActionStepBehavior
    {
        private AudioClip clip;
        public PlaySfxBehavior(AudioClip clip) { this.clip = clip; }

        public override System.Collections.IEnumerator Execute()
        {
            AudioSource.PlayClipAtPoint(clip, Vector3.zero);
            yield return null;
        }
    }
}
```
2. Tao SO asset: `Create > DoMiTruth > Actions > Play SFX`
3. Keo vao InteractableObjectSO.onUnlockSuccess hoac onUnlockFail

---

## BUOC 5: THEM UI MOI

1. Them vao `UIName` enum (trong UIManager.cs):
```csharp
NewScreen = 16,
```

2. Tao script ke thua UIBase:
```csharp
namespace Luzart
{
    public class UINewScreen : UIBase
    {
        // your code
    }
}
```

3. Tao prefab:
   - Tao GameObject trong Scene
   - Them UINewScreen component
   - Set `uiName = NewScreen`
   - Them TweenAnimation + TweenAnimationCaller cho show/hide
   - Save as prefab vao `Assets/Luzart/DoMiTruth/Prefabs/`

4. Them vao UIRegistrySO:
   - Mo `Resources/UIRegistry.asset`
   - Them entry: UIName=NewScreen, prefab=UINewScreen, layer=0, cache=true

5. Su dung:
```csharp
UIManager.Instance.ShowUI<UINewScreen>(UIName.NewScreen);
```

---

## BUOC 6: ART & POLISH

### Them hinh anh
- **Background phong**: keo Sprite vao RoomSO.backgroundImage
- **Anh nhan vat**: keo Sprite vao DialogueCharacterSO.portrait
- **Anh manh moi**: keo Sprite vao ClueSO.clueImage
- **Map thumbnail**: keo Sprite vao MapSO.mapThumbnail

### Them animation cho UI
- Them `TweenAnimation` component len prefab root
- Set `EAnimation.Scale` (scale 0.85→1.0) hoac `EAnimation.Move`
- Them `TweenAnimationCaller` component
- Keo TweenAnimation vao `tweenAnimationBase` field
- Set `typeShow = None` (UIBase se goi manual)
- Keo TweenAnimationCaller vao UIBase.showAnimation

### Them button effect
- Them `EffectButton` component len moi Button
- Default: scale 1.1x khi nhan, 0.1s duration

### Them video cutscene
- Keo VideoClip vao GameConfigSO.introCutscene
- Set cutsceneDuration va skipButtonDelay

---

## XU LY LOI THUONG GAP

### Loi: "UIRegistrySO is null"
- Kiem tra UIManager trong Scene co reference UIRegistrySO
- Chay lai: `Tools > DoMiTruth > Step 5: Wire All References`

### Loi: "Prefab not found in registry"
- Mo UIRegistrySO asset
- Kiem tra UIName co entry tuong ung
- Kiem tra prefab reference khong null

### Loi: "NullReferenceException" o Manager
- Kiem tra Scene co tat ca 5 Managers (GameDataManager, GameFlowController, DialogueManager, InvestigationManager, NotebookManager)
- Chay: `Tools > DoMiTruth > Step 4: Setup Scene`

### Loi: SO Event khong broadcast
- Kiem tra SO Event asset duoc keo vao ca nguon (Raise) va dich (Register)
- Vi du: onClueCollected phai co trong GameDataManager VA UIInvestigationHud

### Loi: Animation khong chay
- Kiem tra TweenAnimationCaller co reference TweenAnimationBase
- Kiem tra TweenAnimationCaller.typeShow = None (UIBase tu goi)
- Kiem tra DOTween da duoc setup (DOTween Utility Panel)

### Loi: Button khong click duoc
- Kiem tra Canvas co GraphicRaycaster
- Kiem tra Scene co EventSystem
- Kiem tra UIManager.BlockRaycast chua bi lock

---

## CAU TRUC MENU TOOLS

```
Tools
└── DoMiTruth
    ├── Setup All (Complete)          # 1 click tao tat ca
    ├── Setup Window                  # EditorWindow UI
    ├── ─── Step by Step ───
    ├── Step 1: Create Core Assets
    ├── Step 2: Create UI Prefabs
    ├── Step 3: Create Sample Data (9 Rooms)
    ├── Step 4: Setup Scene
    ├── Step 5: Wire All References
    ├── ─── Utilities ───
    └── Validate Setup                # Kiem tra setup
```

---

## TOM TAT

| Buoc | Hanh dong | Thoi gian |
|------|-----------|-----------|
| 1 | `Tools > DoMiTruth > Setup All` | ~10 giay |
| 2 | Kiem tra Scene + Assets | 1 phut |
| 3 | Nhan Play de test | ngay lap tuc |
| 4 | Thay art, sua content | tuy y |
| 5 | Them features moi | khi can |

**Chi can 1 click** de co 1 game hoan chinh voi 9 phong, 3 ban do, 12 manh moi, 5 nhan vat!
