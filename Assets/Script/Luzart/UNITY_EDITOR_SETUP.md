# PHASE 1 - UNITY EDITOR SETUP GUIDE

## ⚠️ CÁC BƯỚC MANUAL CẦN THỰC HIỆN TRONG UNITY EDITOR

Phase 1 đã hoàn thành **4/8 tasks** bằng code automation.
**4 tasks còn lại** cần thực hiện manual trong Unity Editor.

---

## 📋 CHECKLIST

### ⏳ TASK 1.4: UIRegistry Setup

**Steps:**
1. Mở Unity Editor
2. Right-click trong Project window
3. Create > UIFramework > UI Registry
4. Đặt tên: `UIRegistry`
5. Di chuyển vào: `Assets/Resources/UIRegistry.asset`
6. Select UIRegistry asset
7. Trong Inspector, configure:
   - Layer Settings (nếu có)
   - Verify empty configs list

**Verification:**
- [ ] UIRegistry.asset tồn tại tại `Assets/Resources/`
- [ ] Asset có thể select và view trong Inspector
- [ ] No errors in console

**Time Estimate:** 5 minutes

---

### ⏳ TASK 1.6: Setup UIManager in Scene

**Steps:**

#### A. Tạo Scene (nếu chưa có):
1. File > New Scene
2. Đặt tên: `MainScene`
3. Save tại: `Assets/Scenes/MainScene.unity`

#### B. Setup UIManager Hierarchy:

**Option 1: Sử dụng UISceneSetupWizard (RECOMMENDED)**
1. Window > UIFramework > Scene Setup Wizard
2. Click "Setup Scene"
3. Wizard sẽ tự động tạo hierarchy
4. Assign UIRegistry vào UIManager component

**Option 2: Manual Setup**
1. Create Empty GameObject, rename: `[UIManager]`
2. Add component: `UIManager`
3. Create child Canvas, rename: `UICanvas`
4. Set Canvas properties:
   - Render Mode: Screen Space - Overlay
   - Reference Resolution: 1920x1080
   - Match: 0.5 (Width/Height)
5. Tạo 4 child Canvas con trong UICanvas:
   ```
   UICanvas
   ├─ Layer_Hud
   ├─ Layer_Screen
   ├─ Layer_Popup
   └─ Layer_SYSTEM
   ```
6. Mỗi Layer Canvas:
   - Add component: Canvas
   - Add component: GraphicRaycaster
   - Sort Order: Hud=0, Screen=10, Popup=20, SYSTEM=30
7. Create `EventSystem` (nếu chưa có)
8. Select [UIManager] GameObject
9. Assign UIRegistry vào UIManager component

**Verification:**
- [ ] [UIManager] GameObject exists
- [ ] UIManager component có UIRegistry reference
- [ ] UICanvas có 4 layer canvases
- [ ] EventSystem exists trong scene
- [ ] No errors in console

**Time Estimate:** 10-15 minutes (wizard) hoặc 20-30 minutes (manual)

---

### ⏳ TASK 1.7: Create GameManagers GameObject

**Steps:**
1. Trong MainScene, create Empty GameObject
2. Rename: `[GameManagers]`
3. Position: (0, 0, 0)
4. Tag: `Untagged` (hoặc tạo tag "GameManagers" nếu muốn)
5. Tạo script `DontDestroyOnLoadHelper.cs`:
   ```csharp
   using UnityEngine;
   
   namespace Luzart.Core
   {
       public class DontDestroyOnLoadHelper : MonoBehaviour
       {
           private void Awake()
           {
               DontDestroyOnLoad(gameObject);
           }
       }
   }
   ```
6. Save script tại: `Assets/Script/Luzart/Core/DontDestroyOnLoadHelper.cs`
7. Add component `DontDestroyOnLoadHelper` vào [GameManagers]

**Verification:**
- [ ] [GameManagers] GameObject exists
- [ ] DontDestroyOnLoadHelper script attached
- [ ] GameObject persists between scene loads (test in Play Mode)
- [ ] No errors in console

**Time Estimate:** 5-10 minutes

---

### ⏳ TASK 1.8: Test UIFramework Integration

**Steps:**

#### A. Open UI Debug Window:
1. Window > UIFramework > UI Debug Window
2. Dock window (recommended: right side)

#### B. Play Scene:
1. Click Play button
2. Check console for errors
3. Verify UI Debug Window shows:
   - UIManager initialized
   - Layers registered
   - No errors

#### C. Test Example UI (Optional):
1. Trong Project window, tìm `UIFramework/Examples/`
2. Drag `MainMenuScreen` prefab vào scene
3. Trong Play Mode:
   - Click button để test show/hide
   - Check UI Debug Window state changes
   - Verify transitions work

#### D. Stop Play Mode:
1. Click Stop button
2. Delete example UI prefab (nếu đã add)

**Verification:**
- [ ] UI Debug Window accessible
- [ ] Play Mode không có errors
- [ ] UIManager initializes correctly
- [ ] EventSystem works (clickable UI)
- [ ] UI Debug Window shows proper state
- [ ] Build successful (Ctrl+Shift+B)

**Time Estimate:** 10-15 minutes

---

## ✅ AFTER COMPLETING ALL TASKS

### Final Verification Checklist:
- [ ] UIRegistry asset exists at `Assets/Resources/UIRegistry.asset`
- [ ] [UIManager] GameObject setup với 4 layers
- [ ] [GameManagers] GameObject với DontDestroyOnLoad
- [ ] Play Mode: No errors in console
- [ ] UI Debug Window accessible và working
- [ ] Build successful
- [ ] All 8 Phase 1 tasks marked complete in ProjectProgress.txt

### Update ProjectProgress.txt:
1. Mở `Assets/Script/GDD/ProjectProgress.txt`
2. Thay đổi:
   - `Completed: 4` → `Completed: 8`
   - `In Progress: 4` → `In Progress: 0`
   - `Progress: [■░░░░░░░░░] 8%` → `Progress: [■■░░░░░░░░] 16%`
   - Phase 1: `(4/8 - 50% IN PROGRESS)` → `(8/8 - 100% COMPLETE)`
   - Change all ⏳ to ✓ for tasks 1.4, 1.6, 1.7, 1.8
3. Save file

---

## 🚀 NEXT STEPS AFTER PHASE 1

Khi Phase 1 hoàn thành 100%, tiếp tục:

### Phase 2: Data Layer - ScriptableObjects (12 tasks)
Tạo tất cả ScriptableObject classes:
1. DialogueCharacterSO
2. DialogueSequenceSO
3. ClueSO
4. InteractableObjectSO
5. CameraAreaSO
6. LockConfigSO
7. NPCEncounterSO
8. RoomSO
9. MapSO
10. NotebookEntrySO (+ derived classes)
11. GameSettingsSO
12. AnimationConfigSO

**Estimated Time:** Week 2 (12 tasks × 30-45 mins each)

---

## 📞 TROUBLESHOOTING

### Issue: UIRegistry creation menu not found
**Solution:**
- Verify UIFramework installed correctly
- Check `Assets/Script/UIFramework/Data/UIRegistry.cs` exists
- Restart Unity Editor
- Try Create > ScriptableObject > UIFramework > UIRegistry

### Issue: UISceneSetupWizard not found
**Solution:**
- Check `Assets/Script/UIFramework/Editor/UISceneSetupWizard.cs` exists
- If missing, use Manual Setup option
- Restart Unity Editor

### Issue: EventSystem conflicts
**Solution:**
- Delete existing EventSystem
- UIFramework will auto-create one
- Ensure only one EventSystem in scene

### Issue: Canvas layers not working
**Solution:**
- Verify Sort Order: Hud=0, Screen=10, Popup=20, SYSTEM=30
- Check all layers have Canvas + GraphicRaycaster
- Verify Render Mode: Screen Space - Overlay

### Issue: DontDestroyOnLoad not working
**Solution:**
- Verify script attached to [GameManagers]
- Check Awake() method called
- Test: Play scene, load another scene, verify GameObject persists

---

## 📚 REFERENCES

- Technical Requirement: `Assets/Script/GDD/TechnicalRequirement.txt`
- Phase 1 README: `Assets/Script/Luzart/PHASE1_README.md`
- UIFramework Docs: `Assets/Script/UIFramework/README.md`
- Project Progress: `Assets/Script/GDD/ProjectProgress.txt`

---

**Good luck với Unity Editor setup! 🎮**
