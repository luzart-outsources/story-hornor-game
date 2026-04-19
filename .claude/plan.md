# Fix GDD Discrepancies - Implementation Plan

## Scope: 3 fixes

### Fix 1: Màn 3 - NPC Briefing (Công an giao nhiệm vụ)
**Problem:** GDD yêu cầu sau Cutscene phải có màn NPC Công an giao nhiệm vụ trước khi vào Map Selection. Project hiện tại bỏ qua.

**Changes:**
- **GameConfigSO.cs**: Thêm field `DialogueSequenceSO briefingDialogue` để config dialogue briefing
- **GameFlowController.cs**: Sửa `OnCutsceneComplete()` → gọi briefing dialogue → khi xong mới `ShowMapSelection()`
- Dùng `Dlg_Intro.asset` đã có sẵn làm briefing dialogue

### Fix 2: SwipePattern Lock Implementation
**Problem:** GDD yêu cầu vuốt khóa, project chỉ có enum stub.

**Changes:**
- **LockConfigSO.cs**: Thêm field `int[] swipePattern` để lưu pattern đáp án (mảng index các dot theo thứ tự)
- **UILockPuzzle.cs**: Implement SwipePattern panel logic:
  - Grid 3x3 dots (9 nút)
  - Player click/drag qua các dot để tạo pattern
  - So sánh pattern input vs đáp án
  - Success/fail callback giống Passcode

### Fix 3: Main Menu Button Hover Effect (BaseSelect)
**Problem:** GDD yêu cầu button trồi lên khi hover, dùng BaseSelect system.

**Changes:**
- **Tạo mới `ButtonHoverSelect.cs`**: Component gắn vào mỗi button, implement `IPointerEnterHandler` + `IPointerExitHandler`
  - Có reference tới `SelectToggleGameObject` của chính nó
  - Có reference tới parent group (hoặc dùng event)
  - PointerEnter → Select(true) trên mình, Select(false) trên tất cả anh em
  - PointerExit → Select(false) trên mình
- **UIMainMenu.cs**: Thêm `[SerializeField] ButtonHoverSelect[] hoverButtons` để quản lý group
- Ngoài Unity Editor: Gán SelectToggleGameObject cho mỗi button với 2 GO (normal/raised state)

## File changes summary:
1. `GameConfigSO.cs` - thêm briefing field
2. `GameFlowController.cs` - thêm briefing flow
3. `LockConfigSO.cs` - thêm swipe pattern data
4. `UILockPuzzle.cs` - implement SwipePattern
5. `ButtonHoverSelect.cs` - **NEW** - hover select component
6. `UIMainMenu.cs` - thêm hover group support
