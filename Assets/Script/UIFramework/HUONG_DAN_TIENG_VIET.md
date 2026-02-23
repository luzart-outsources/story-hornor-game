# UI Framework - Hướng Dẫn Tiếng Việt

## 🚀 Bắt Đầu Nhanh (5 phút)

### Bước 1: Setup Scene
```
Window → UIFramework → Scene Setup Wizard
- Scene Name: Nhập tên scene của bạn
- Click "Setup Scene"

✓ Scene đã có UIManager và EventSystem!
```

### Bước 2: Tạo UI Registry
```
Project → Click phải → Create → UIFramework → UI Registry
- Đặt tên: "MainUIRegistry"
- Lưu trong Resources/
```

### Bước 3: Tạo UI Đầu Tiên
```
Window → UIFramework → UI Creator Wizard
- UI Name: "MainMenu"
- UI Type: Screen
- ☑ Create Controller
- ☑ Create Data Class
- ☑ Create Prefab
- Click "Create UI"

✓ Framework tự động tạo 3 file:
   - MainMenuScreen.cs (View)
   - MainMenuController.cs (Logic)
   - MainMenuData.cs (Data)
```

### Bước 4: Sử Dụng UI
```csharp
// Hiển thị UI
var data = new MainMenuData("Tên Game", true);
UIManager.Instance.Show<MainMenuScreen>(data);

// Ẩn UI
UIManager.Instance.Hide<MainMenuScreen>();
```

**Xong! UI của bạn đã hoạt động! 🎉**

---

## 🏗️ Kiến Trúc Hệ Thống

### MVVM Pattern (Dễ Hiểu)

```
VIEW (UIBase - MonoBehaviour)
  ↓
  ↓ - Hiển thị UI trên màn hình
  ↓ - Bắt sự kiện button, input
  ↓ - Không chứa logic game
  ↓
  ↓ gọi
  ↓
CONTROLLER (UIControllerBase - Pure C#)
  ↓
  ↓ - Xử lý logic game
  ↓ - Quyết định hiển thị gì
  ↓ - Gọi service, publish event
  ↓
  ↓ cập nhật
  ↓
DATA (UIDataBase - ViewModel)
  ↓
  ↓ - Chứa dữ liệu hiển thị
  ↓ - Immutable (không đổi)
  ↓ - Serializable
```

### Ví Dụ Cụ Thể: Shop UI

```csharp
// 1. DATA - Chứa dữ liệu
public class ShopData : UIDataBase
{
    public int Coins { get; private set; }
    public List<Item> Items { get; private set; }
    
    // Immutable - tạo instance mới khi thay đổi
    public ShopData WithCoins(int newCoins)
    {
        return new ShopData { Coins = newCoins, Items = Items };
    }
}

// 2. VIEW - Hiển thị
public class ShopScreen : UIScreen
{
    [SerializeField] private Text coinsText;
    [SerializeField] private Button buyButton;
    
    protected override void OnInitialize(IUIData data)
    {
        var shopData = data as ShopData;
        coinsText.text = $"Coins: {shopData.Coins}";
        
        // Khi click button → gọi controller
        buyButton.onClick.AddListener(OnBuyClicked);
    }
    
    private void OnBuyClicked()
    {
        var controller = this.controller as ShopController;
        controller.OnBuyPressed();
    }
}

// 3. CONTROLLER - Logic
public class ShopController : UIControllerBase
{
    public void OnBuyPressed()
    {
        // Xử lý logic mua hàng
        if (coins < price)
        {
            ShowError("Không đủ tiền!");
            return;
        }
        
        // Mua thành công
        BuyItem();
        
        // Thông báo cho UI khác (decoupled)
        EventBus.Instance.Publish(new ItemBoughtEvent());
    }
}
```

---

## 📡 EventBus - Giao Tiếp Giữa UI

### Tại Sao Cần EventBus?

**KHÔNG TỐT** ❌:
```csharp
// Shop gọi trực tiếp Inventory
public class ShopScreen : UIScreen
{
    void OnBuyItem()
    {
        // ❌ Tìm object khác (chậm)
        var inventory = FindObjectOfType<InventoryUI>();
        inventory.AddItem(item);
        
        // ❌ Hard reference (tight coupling)
        // ❌ Khó test
        // ❌ Không linh hoạt
    }
}
```

**TỐT** ✅:
```csharp
// Shop publish event, Inventory tự động nhận
public class ShopController : UIControllerBase
{
    void OnBuyItem()
    {
        // ✅ Publish event
        EventBus.Instance.Publish(new ItemBoughtEvent(item));
        
        // ✅ Không biết ai sẽ nhận
        // ✅ Decoupled
        // ✅ Dễ test
        // ✅ Linh hoạt
    }
}

// Inventory tự subscribe
public class InventoryUI : UIScreen, IEventHandler<ItemBoughtEvent>
{
    void Start()
    {
        EventBus.Instance.Subscribe<ItemBoughtEvent>(this);
    }
    
    public void Handle(ItemBoughtEvent evt)
    {
        // Tự động nhận event và xử lý
        AddItem(evt.Item);
    }
    
    void OnDestroy()
    {
        // QUAN TRỌNG: Phải unsubscribe!
        EventBus.Instance.Unsubscribe<ItemBoughtEvent>(this);
    }
}
```

---

## 🎬 Thêm Animation

### Animation Đơn Giản

```csharp
// Show popup với scale animation
var popup = UIManager.Instance.Show<ConfirmationPopup>(data);
popup.SetAnimation(new ScaleAnimation(0.3f));

// Show screen với fade transition
var screen = UIManager.Instance.Show<MainMenuScreen>(data);
screen.SetTransition(new FadeTransition(0.5f));

// Show screen với slide transition
var shop = UIManager.Instance.Show<ShopScreen>(data);
shop.SetTransition(new SlideTransition(
    SlideTransition.SlideDirection.Right, 
    0.4f
));
```

### Không Cần Animation

```csharp
// HUD không cần animation
var hud = UIManager.Instance.Show<PlayerHud>(data);
// Không gọi SetAnimation() → hiển ngay lập tức
```

---

## 🎯 Object Pooling (Tối Ưu Hiệu Suất)

### Khi Nào Dùng Pool?

**Dùng Pool** khi UI xuất hiện thường xuyên:
- Damage numbers (mỗi giây 10+ lần)
- Notifications (mỗi phút vài lần)
- Tooltips (liên tục)

**Không Dùng Pool** khi UI xuất hiện hiếm:
- Settings screen (1 lần/game)
- Game over screen (1 lần/level)

### Cấu Hình Pool

```csharp
// Trong UIRegistry:
- ViewId: "DamageNumber"
- EnablePooling: ✓
- PoolSize: 20 (số lượng instance giữ trong pool)

// Trong Bootstrap:
UIManager.Instance.PrewarmPool("DamageNumber", 20);
```

### Kết Quả
```
Không dùng Pool:
  100 lần show = 100 Instantiate() + 100 Destroy()
  = 200 GC events
  = Lag!

Dùng Pool:
  100 lần show = 1 Instantiate() + 99 reuse
  = 1 GC event
  = Smooth! 🚀
```

---

## 💾 Caching (Giữ UI Trong Memory)

### Khi Nào Dùng Cache?

**Dùng Cache** cho UI hay dùng nhưng không liên tục:
- Shop screen (mở vài lần trong session)
- Inventory (mở thường xuyên)
- Character stats (mở vừa phải)

**Không Cache** UI hiếm khi dùng:
- Tutorial screens (1 lần/game)
- Credits screen (hiếm khi xem)

### Cấu Hình Cache

```csharp
// Trong UIRegistry:
- ViewId: "ShopScreen"
- EnableCaching: ✓

// Framework tự động:
// - Lần đầu: Load và cache
// - Lần sau: Lấy từ cache (instant)
// - Khi Hide: Không destroy, giữ trong memory
```

---

## 🎮 Ví Dụ Hoàn Chỉnh: Game RPG

### 1. Setup Game Controller

```csharp
using UnityEngine;
using UIFramework.Managers;
using UIFramework.Communication;

public class GameController : MonoBehaviour
{
    void Start()
    {
        // Show main menu khi bắt đầu
        ShowMainMenu();
    }
    
    void ShowMainMenu()
    {
        var data = new MainMenuData("Epic RPG", true);
        var menu = UIManager.Instance.Show<MainMenuScreen>(data);
        
        // Thêm fade animation
        menu.SetTransition(new FadeTransition(0.5f));
    }
}
```

### 2. Main Menu UI

```csharp
public class MainMenuScreen : UIScreen
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button shopButton;
    
    protected override void OnInitialize(IUIData data)
    {
        // Bind buttons
        playButton.onClick.AddListener(OnPlayClicked);
        shopButton.onClick.AddListener(OnShopClicked);
    }
    
    void OnPlayClicked()
    {
        // Gọi controller
        var controller = this.controller as MainMenuController;
        controller.OnPlayPressed();
    }
    
    void OnShopClicked()
    {
        var controller = this.controller as MainMenuController;
        controller.OnShopPressed();
    }
    
    protected override void OnDispose()
    {
        // QUAN TRỌNG: Unbind để tránh memory leak
        playButton.onClick.RemoveListener(OnPlayClicked);
        shopButton.onClick.RemoveListener(OnShopClicked);
    }
}
```

### 3. Controller Logic

```csharp
public class MainMenuController : UIControllerBase
{
    public void OnPlayPressed()
    {
        // Ẩn main menu
        UIManager.Instance.Hide<MainMenuScreen>();
        
        // Hiển gameplay HUD
        var hudData = new PlayerHudData(100, 100, 0, 1);
        UIManager.Instance.Show<PlayerHud>(hudData);
        
        // Bắt đầu game
        StartGame();
    }
    
    public void OnShopPressed()
    {
        // Load shop data
        var shopData = LoadShopData();
        
        // Show shop
        var shop = UIManager.Instance.Show<ShopScreen>(shopData);
        
        // Thêm slide animation
        shop.SetTransition(new SlideTransition(
            SlideTransition.SlideDirection.Right, 
            0.3f
        ));
    }
    
    private void StartGame()
    {
        // Logic bắt đầu game
        Debug.Log("Game started!");
    }
}
```

### 4. Shop Mua Item

```csharp
public class ShopController : UIControllerBase
{
    public void OnBuyItem(Item item, int playerCoins)
    {
        // Kiểm tra đủ tiền không
        if (playerCoins < item.Price)
        {
            ShowError("Không đủ tiền!");
            return;
        }
        
        // Show confirmation popup
        var confirmData = new ConfirmationPopupData(
            "Mua Vật Phẩm",
            $"Mua {item.Name} với giá {item.Price} coins?",
            onConfirm: () => ExecuteBuy(item, playerCoins),
            onCancel: null
        );
        
        var popup = UIManager.Instance.Show<ConfirmationPopup>(confirmData);
        popup.SetAnimation(new ScaleAnimation(0.2f));
    }
    
    void ExecuteBuy(Item item, int playerCoins)
    {
        // Thực hiện mua
        int remaining = playerCoins - item.Price;
        
        // Publish event để UI khác tự cập nhật
        EventBus.Instance.Publish(new ItemBoughtEvent(item.Id, remaining));
        
        Debug.Log($"Đã mua {item.Name}!");
    }
}
```

### 5. Inventory Nhận Event

```csharp
public class InventoryScreen : UIScreen, IEventHandler<ItemBoughtEvent>
{
    void OnEnable()
    {
        // Subscribe event
        EventBus.Instance.Subscribe<ItemBoughtEvent>(this);
    }
    
    void OnDisable()
    {
        // QUAN TRỌNG: Unsubscribe!
        EventBus.Instance.Unsubscribe<ItemBoughtEvent>(this);
    }
    
    // Tự động nhận khi có ai mua item
    public void Handle(ItemBoughtEvent evt)
    {
        Debug.Log($"Nhận được item mới: {evt.ItemId}");
        
        // Cập nhật inventory
        AddItemToInventory(evt.ItemId);
        
        // Refresh UI
        Refresh();
    }
}
```

---

## ⚠️ Các Pattern Quan Trọng

### 1. Luôn Unsubscribe EventBus

**SAI** ❌:
```csharp
void Start()
{
    EventBus.Instance.Subscribe<MyEvent>(this);
    // ❌ Quên unsubscribe → Memory leak!
}
```

**ĐÚNG** ✅:
```csharp
void OnEnable()
{
    EventBus.Instance.Subscribe<MyEvent>(this);
}

void OnDisable()
{
    EventBus.Instance.Unsubscribe<MyEvent>(this); // ✅
}
```

### 2. Logic Trong Controller, Không Trong View

**SAI** ❌:
```csharp
public class ShopScreen : UIScreen
{
    void OnBuyClicked()
    {
        // ❌ Logic trong MonoBehaviour
        if (PlayerData.Coins < item.Price)
            return;
        
        PlayerData.Coins -= item.Price;
        Inventory.AddItem(item);
    }
}
```

**ĐÚNG** ✅:
```csharp
// VIEW
public class ShopScreen : UIScreen
{
    void OnBuyClicked()
    {
        // ✅ Chỉ gọi controller
        var controller = this.controller as ShopController;
        controller.OnBuyPressed(selectedItem);
    }
}

// CONTROLLER
public class ShopController : UIControllerBase
{
    public void OnBuyPressed(Item item)
    {
        // ✅ Logic ở đây
        // ✅ Test được
        // ✅ Không phụ thuộc Unity
    }
}
```

### 3. Data Immutable

**SAI** ❌:
```csharp
public class PlayerData : UIDataBase
{
    public int Coins; // ❌ Public field, có thể đổi
}

// Ai cũng có thể đổi
data.Coins = 999999; // ❌ Không kiểm soát
```

**ĐÚNG** ✅:
```csharp
public class PlayerData : UIDataBase
{
    public int Coins { get; private set; } // ✅ Private set
    
    // ✅ Controlled mutation - tạo instance mới
    public PlayerData WithCoins(int newCoins)
    {
        return new PlayerData { Coins = newCoins };
    }
}

// Phải dùng method
var newData = data.WithCoins(100); // ✅ Kiểm soát được
```

---

## 🛠️ Editor Tools (Công Cụ Hỗ Trợ)

### 1. UI Creator Wizard ⭐ (Quan Trọng Nhất)
```
Window → UIFramework → UI Creator Wizard

Tự động tạo:
- View script (MonoBehaviour)
- Controller script (Logic)
- Data script (ViewModel)
- Prefab
- Đăng ký vào Registry

Tiết kiệm: 15 phút/UI
```

### 2. UI Debug Window
```
Window → UIFramework → UI Debug Window

Hiển thị:
- UI nào đang mở
- Memory usage
- Cache status
- Pool status

Dùng để: Debug khi UI không hoạt động
```

### 3. Registry Validator
```
Window → UIFramework → Validate All Registries

Kiểm tra:
- Có duplicate ViewId không
- Có missing prefab không
- Config có hợp lệ không

Dùng trước khi: Build game
```

### 4. Scene Setup Wizard
```
Window → UIFramework → Scene Setup Wizard

Tự động tạo:
- EventSystem
- UIManager
- GameController
- UIRegistry

Tiết kiệm: 5 phút setup
```

---

## 💡 Tips Quan Trọng

### Tip 1: Dùng Wizard
```
❌ KHÔNG: Tạo UI manually (lâu, dễ sai)
✅ DÙNG: UI Creator Wizard (nhanh, đúng)
```

### Tip 2: EventBus Cho Giao Tiếp
```
❌ KHÔNG: FindObjectOfType (chậm, không safe)
❌ KHÔNG: Direct reference (coupling)
✅ DÙNG: EventBus.Publish/Subscribe (đúng cách)
```

### Tip 3: Pooling Cho UI Thường Xuyên
```
❌ KHÔNG: Instantiate/Destroy mỗi lần (lag)
✅ DÙNG: Object Pool (smooth)

Ví dụ: Damage number hiển 100 lần/phút
→ Bắt buộc phải dùng Pool!
```

### Tip 4: Cache Cho UI Hay Dùng
```
❌ KHÔNG: Load lại mỗi lần (chậm)
✅ DÙNG: Cache (instant lần 2 trở đi)

Ví dụ: Shop screen mở 5-10 lần/session
→ Nên cache!
```

### Tip 5: Animation Cho UX Tốt
```
❌ KHÔNG: Hiện đột ngột (cứng nhắc)
✅ DÙNG: Animation (mượt mà)

Popup → Scale animation
Screen → Fade transition
Slide menu → Slide transition
```

---

## 🐛 Debug Thường Gặp

### Lỗi 1: UI Không Hiện

**Nguyên nhân:**
- Chưa đăng ký trong UIRegistry
- Sai ViewId
- Chưa assign prefab

**Cách fix:**
1. Mở UIRegistry
2. Kiểm tra ViewId có khớp class name không
3. Kiểm tra prefab đã assign chưa
4. Chạy Validator

### Lỗi 2: Memory Leak

**Nguyên nhân:**
- Quên unsubscribe EventBus
- Giữ reference không cần thiết

**Cách fix:**
```csharp
// Luôn unsubscribe
void OnDisable()
{
    EventBus.Instance.Unsubscribe<MyEvent>(this);
}

// Luôn unbind UI events
void OnDispose()
{
    button.onClick.RemoveListener(OnClicked);
}
```

### Lỗi 3: Animation Không Chạy

**Nguyên nhân:**
- Chưa inject animation
- Thiếu component (CanvasGroup, RectTransform)

**Cách fix:**
```csharp
// Inject animation
popup.SetAnimation(new ScaleAnimation());

// Kiểm tra component
Debug.Log(popup.GetComponent<CanvasGroup>() != null);
```

---

## ✅ Checklist Trước Khi Deploy

### Code
- [ ] Tất cả UI đã register trong Registry
- [ ] Không có business logic trong MonoBehaviour
- [ ] EventBus subscribe có unsubscribe tương ứng
- [ ] Không có FindObjectOfType
- [ ] Không có lambda trong Update

### Performance
- [ ] UI thường xuyên đã dùng Pool
- [ ] UI hay dùng đã enable Cache
- [ ] Test không leak memory (chơi 1 giờ)
- [ ] Duy trì 60 FPS (PC), 30 FPS (Mobile)

### Testing
- [ ] Test tất cả UI show/hide
- [ ] Test animation
- [ ] Test trên device thật (mobile)
- [ ] Test scene transition

---

## 📚 Học Thêm

### Tài Liệu Tiếng Anh (Chi Tiết Hơn)

- `README.md` - Tổng quan
- `ARCHITECTURE.md` - Kiến trúc chi tiết
- `INTEGRATION_EXAMPLES.md` - Ví dụ thực tế
- `PRODUCTION_CHECKLIST.md` - Checklist đầy đủ

### Video Tutorial (Tự Tạo)

Có thể record video hướng dẫn:
1. Setup framework (5 phút)
2. Create UI với wizard (5 phút)
3. Implement controller (10 phút)
4. EventBus communication (10 phút)
5. Add animation (5 phút)

---

## 🎉 Tổng Kết

### Bạn Đã Có:
✅ Framework UI production-ready  
✅ MVVM pattern chuẩn  
✅ EventBus không memory leak  
✅ Object Pooling tối ưu  
✅ Editor tools tiện lợi  
✅ Examples đầy đủ  
✅ Documentation chi tiết  

### Bạn Có Thể:
✅ Tạo UI trong 2 phút (wizard)  
✅ Show/Hide với 1 dòng code  
✅ Giao tiếp qua EventBus (decoupled)  
✅ Thêm animation dễ dàng  
✅ Test không cần Unity  
✅ Scale lên 50+ UIs  
✅ Ship production game  

---

## 🚀 Bắt Đầu Ngay!

```
1. Window → UIFramework → Scene Setup Wizard
2. Window → UIFramework → UI Creator Wizard
3. Tạo UI đầu tiên
4. Test trong Play Mode
5. Đọc thêm docs khi cần

Chúc bạn code vui vẻ! 🎮🔥
```

---

## 📞 Liên Hệ & Hỗ Trợ

### Cần Giúp?
1. Đọc file INDEX.md (tìm tài liệu liên quan)
2. Xem Examples/ (code mẫu)
3. Dùng UI Debug Window (debug)
4. Check console (error messages)

### Muốn Mở Rộng?
1. Implement interfaces (IUITransition, IUIAnimation, etc.)
2. Tạo custom controller
3. Thêm custom event
4. Viết editor tool riêng

---

**Framework Version**: 1.0.0  
**Status**: ✅ Production Ready  
**Language**: Vietnamese Guide  

**Chúc Bạn Thành Công! 🎯**
