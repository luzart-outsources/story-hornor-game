# Hướng dẫn tải và mở Project Unity (Dành cho người mới)
Tài liệu này sẽ hướng dẫn bạn cách cài đặt môi trường và mở project Story Hornor Game bằng phiên bản Unity 2022.3.62f2.

## Bước 1: Tải Project về máy (Dưới dạng file .zip)
Vì bạn không sử dụng các lệnh Git phức tạp, hãy làm theo các bước sau để tải toàn bộ mã nguồn:

Truy cập vào link GitHub của project: story-hornor-game.

Tìm nút Code màu xanh lá cây ở phía trên bên phải danh sách file.

Nhấp vào đó và chọn Download ZIP.

Sau khi tải xong, hãy Giải nén (Extract) file .zip này ra một thư mục trên máy tính của bạn (Ví dụ: D:/UnityProjects/StoryHornorGame).

## Bước 2: Cài đặt Unity Hub
Unity Hub là phần mềm quản lý các phiên bản Unity và các Project của bạn.

Truy cập Trang chủ Unity và tải về Unity Hub.

Cài đặt và đăng nhập bằng tài khoản Unity (nếu chưa có, bạn có thể tạo miễn phí).

## Bước 3: Cài đặt đúng phiên bản Unity (2022.3.62f2)
Project này yêu cầu phiên bản 2022.3.62f2. Để cài đặt:

Mở Unity Hub.

Chọn tab Installs ở cột bên trái.

Nhấn nút Install Editor.

Nếu không thấy bản 2022.3.62f2 trong danh sách có sẵn, hãy nhấn vào Archive hoặc truy cập Unity Download Archive.

Tìm đến dòng Unity 2022.X, tìm phiên bản 2022.3.62f2 và nhấn nút Unity Hub bên cạnh nó để phần mềm tự động tải và cài đặt.

Lưu ý: Khi cài đặt, hãy đảm bảo chọn thêm module WebGL Build Support hoặc Windows Build Support tùy vào nhu cầu của bạn.

## Bước 4: Thêm và mở Project
Trong Unity Hub, chọn tab Projects ở cột bên trái.

Nhấn nút Add (hoặc mũi tên bên cạnh nút Open -> Add project from disk).

Tìm đến thư mục bạn đã giải nén ở Bước 1. Chọn chính xác thư mục chứa các thư mục con như Assets, ProjectSettings, Packages.

Nhấn Add Project.

Bây giờ, project sẽ xuất hiện trong danh sách. Hãy nhấn vào tên project để mở.

Lưu ý quan trọng khi mở lần đầu
Lần đầu tiên mở project có thể mất từ 5-10 phút để Unity khởi tạo và tải các thư viện (Library). Đây là hiện tượng bình thường.

Nếu Unity hỏi về việc chuyển đổi phiên bản (Version Upgrade), hãy chọn đúng phiên bản 2022.3.62f2 để tránh lỗi code.

### Chúc bạn có những trải nghiệm tuyệt vời với project!



# Cách sử dụng Project:
# Hướng Dẫn Cấu Hình Game "Do Mi Truth"

> Tài liệu dành cho người **không biết dùng Unity**. Hướng dẫn từng bước cách tìm, chỉnh sửa các cài đặt trong game mà không cần viết code.

---

## Mục lục

1. [Mở Unity và tìm file cấu hình](#1-mở-unity-và-tìm-file-cấu-hình)
2. [Cấu trúc tổng quan của game](#2-cấu-trúc-tổng-quan-của-game)
3. [Thêm / Sửa MAP (Bản đồ)](#3-thêm--sửa-map-bản-đồ)
4. [Thêm / Sửa ROOM (Phòng)](#4-thêm--sửa-room-phòng)
5. [Thêm / Sửa VẬT THỂ TƯƠNG TÁC](#5-thêm--sửa-vật-thể-tương-tác)
6. [Thêm / Sửa MANH MỐI (Clue)](#6-thêm--sửa-manh-mối-clue)
7. [Thêm / Sửa NHÂN VẬT (Character)](#7-thêm--sửa-nhân-vật-character)
8. [Thêm / Sửa HỘI THOẠI (Dialogue)](#8-thêm--sửa-hội-thoại-dialogue)
9. [Thêm / Sửa Ổ KHÓA (Lock)](#9-thêm--sửa-ổ-khóa-lock)
10. [Thêm / Sửa HÀNH ĐỘNG (Action)](#10-thêm--sửa-hành-động-action)
11. [Cấu hình GAME CHUNG (GameConfig)](#11-cấu-hình-game-chung-gameconfig)
12. [Thêm ÂM THANH](#12-thêm-âm-thanh)
13. [KHÔNG xóa vật thể sau khi tương tác](#13-không-xóa-vật-thể-sau-khi-tương-tác)
14. [Đổi VỊ TRÍ vật thể trong phòng](#14-đổi-vị-trí-vật-thể-trong-phòng)
15. [Chỉnh sửa ANIMATION (Hiệu ứng chuyển động)](#15-chỉnh-sửa-animation-hiệu-ứng-chuyển-động)
16. [Danh sách tất cả file hiện có](#16-danh-sách-tất-cả-file-hiện-có)
17. [Mẹo & Lưu ý quan trọng](#17-mẹo--lưu-ý-quan-trọng)

---

## 1. Mở Unity và tìm file cấu hình

### Bước cơ bản trong Unity

1. **Mở Unity Hub** → chọn project `story-hornor-game` → nhấn nút **Open**
2. Đợi Unity load xong (có thể mất 1-2 phút)
3. Nhìn phía dưới Unity, có cửa sổ **Project** (nếu không thấy, nhấn menu `Window > General > Project`)
4. Trong cửa sổ Project, điều hướng tới thư mục: `Assets > Luzart > DoMiTruth > Data`

### Cách chỉnh sửa 1 file cấu hình

1. Trong cửa sổ **Project**, click vào file `.asset` bất kỳ (ví dụ: `Room_LivingRoom`)
2. Bên phải Unity sẽ hiện cửa sổ **Inspector** — đây là nơi hiện tất cả thông tin của file đó
3. **Chỉnh sửa trực tiếp** các ô trong Inspector (gõ text, kéo thả hình ảnh, tick checkbox...)
4. **Nhấn Ctrl+S** để lưu (QUAN TRỌNG — nếu không lưu sẽ mất thay đổi)

### Cách tạo file cấu hình mới

1. Trong cửa sổ **Project**, **click chuột phải** vào thư mục bạn muốn tạo file
2. Chọn `Create > DoMiTruth > [Loại file]` (ví dụ: `Create > DoMiTruth > Room`)
3. Đặt tên file (ví dụ: `Room_Garage`)
4. Click vào file vừa tạo để chỉnh sửa trong Inspector

---

## 2. Cấu trúc tổng quan của game

Game được tổ chức theo cấu trúc phân cấp sau:

```
GameConfig (Cấu hình chung)
  └── Maps (Bản đồ - ví dụ: Tầng 1, Tầng 2, Vườn)
       └── Rooms (Phòng - ví dụ: Phòng khách, Nhà bếp)
            └── Interactable Objects (Vật thể tương tác)
                 ├── Clue (Manh mối - nhặt lên xem)
                 ├── NPC (Nhân vật - nói chuyện)
                 ├── LockedItem (Vật bị khóa - giải puzzle)
                 └── Decoration (Trang trí - chỉ nhìn)
```

**Tất cả file cấu hình** nằm trong: `Assets/Luzart/DoMiTruth/Data/`

| Thư mục | Chứa gì | Số lượng hiện tại |
|---------|---------|-------------------|
| `Config/` | Cấu hình game chung | 2 file |
| `Maps/` | Bản đồ (nhóm phòng) | 3 file |
| `Rooms/` | Phòng (chứa vật thể) | 9 file |
| `Interactables/` | Vật thể tương tác | 19 file |
| `Clues/` | Manh mối thu thập | 12 file |
| `Characters/` | Nhân vật NPC | 5 file |
| `Dialogues/` | Đoạn hội thoại | 4 file |
| `Locks/` | Cấu hình ổ khóa/puzzle | 3 file |
| `Actions/` | Hành động tự động | 7 file |
| `Events/` | Kênh sự kiện (không cần sửa) | 3 file |

---

## 3. Thêm / Sửa MAP (Bản đồ)

**Đường dẫn:** `Assets/Luzart/DoMiTruth/Data/Maps/`

**Hiện có:**
- `Map_Floor1` — Tầng 1
- `Map_Floor2` — Tầng 2
- `Map_Garden` — Vườn

### Các trường cần điền

| Trường | Ý nghĩa | Ví dụ |
|--------|---------|-------|
| **Map Id** | Mã định danh (không trùng với map khác) | `map_basement` |
| **Map Name** | Tên hiển thị cho người chơi | `Tầng Hầm` |
| **Map Thumbnail** | Hình đại diện của map (kéo thả file hình vào đây) | Một ảnh .png |
| **Rooms** | Danh sách các phòng trong map | Kéo thả file Room vào |

### Cách thêm 1 map mới

1. Vào thư mục `Data/Maps/`
2. **Chuột phải** → `Create > DoMiTruth > Map`
3. Đặt tên, ví dụ: `Map_Basement`
4. Click vào file vừa tạo, trong Inspector điền:
   - **Map Id**: `map_basement`
   - **Map Name**: `Tầng Hầm`
   - **Map Thumbnail**: kéo thả 1 hình ảnh vào
   - **Rooms**: nhấn dấu `+` để thêm phòng, rồi kéo thả file Room vào
5. **QUAN TRỌNG**: Mở file `GameConfig` (trong `Data/Config/`) → kéo map mới vào danh sách **All Maps**
6. **Ctrl+S** để lưu

---

## 4. Thêm / Sửa ROOM (Phòng)

**Đường dẫn:** `Assets/Luzart/DoMiTruth/Data/Rooms/`

**Hiện có:**
- `Room_LivingRoom` — Phòng khách
- `Room_Kitchen` — Nhà bếp
- `Room_MasterBedroom` — Phòng ngủ chính
- `Room_Study` — Phòng làm việc
- `Room_Bathroom` — Phòng tắm
- `Room_GuestRoom` — Phòng khách (guest)
- `Room_FrontYard` — Sân trước
- `Room_FishPond` — Hồ cá
- `Room_Shed` — Nhà kho

### Các trường cần điền

| Trường | Ý nghĩa | Ví dụ |
|--------|---------|-------|
| **Room Id** | Mã định danh (không trùng) | `room_garage` |
| **Room Name** | Tên hiển thị | `Nhà Xe` |
| **Background Image** | Hình nền phòng (kéo thả file hình) | Ảnh .png kích thước lớn |
| **Background Size** | Kích thước ảnh nền (pixel) | `2560, 1440` |
| **Interactables** | Danh sách vật thể trong phòng (xem bên dưới) | |
| **Entry Dialogue** | Hội thoại khi mới vào phòng (tùy chọn, để trống nếu không cần) | Kéo file Dialogue vào |

### Cách thêm vật thể vào phòng

Mỗi vật thể trong phòng cần 2 thứ:
1. **Data** — file InteractableObject (kéo thả vào)
2. **Position On Background** — tọa độ vật thể trên ảnh nền (X, Y)

**Cách xác định tọa độ:**
- Tọa độ `(0, 0)` là **giữa** ảnh nền
- X dương = sang **phải**, X âm = sang **trái**
- Y dương = lên **trên**, Y âm = xuống **dưới**
- Ví dụ: nếu ảnh nền 2560x1440, thì góc trái trên là `(-1280, 720)`, góc phải dưới là `(1280, -720)`

### Cách thêm 1 phòng mới

1. Vào thư mục `Data/Rooms/`
2. **Chuột phải** → `Create > DoMiTruth > Room`
3. Đặt tên, ví dụ: `Room_Garage`
4. Điền thông tin trong Inspector
5. Trong mục **Interactables**, nhấn `+` để thêm vật thể:
   - Kéo file Interactable vào ô **Data**
   - Nhập tọa độ vào **Position On Background** (ví dụ: `200, -100`)
6. **QUAN TRỌNG**: Mở file Map tương ứng → thêm phòng này vào danh sách **Rooms**
7. **Ctrl+S** để lưu

---

## 5. Thêm / Sửa VẬT THỂ TƯƠNG TÁC

**Đường dẫn:** `Assets/Luzart/DoMiTruth/Data/Interactables/`

**Hiện có (19 vật thể):**

| File | Loại | Mô tả |
|------|------|-------|
| `IO_BrokenVase` | Clue | Bình hoa vỡ |
| `IO_Butler` | NPC | Quản gia |
| `IO_Cabinet` | LockedItem | Tủ bị khóa |
| `IO_Diary` | Clue | Nhật ký |
| `IO_Drawer` | LockedItem | Ngăn kéo bị khóa |
| `IO_Footprint` | Clue | Dấu chân |
| `IO_Footprint_Bath` | Clue | Dấu chân (phòng tắm) |
| `IO_Footprint_Yard` | Clue | Dấu chân (sân) |
| `IO_GardenToolClue` | Clue | Dụng cụ vườn |
| `IO_Letter` | Clue | Lá thư |
| `IO_Letter_Pond` | Clue | Lá thư (hồ cá) |
| `IO_Medicine` | Clue | Thuốc |
| `IO_Neighbor` | NPC | Hàng xóm |
| `IO_Phone` | Clue | Điện thoại |
| `IO_Photo` | Clue | Ảnh chụp |
| `IO_Photo_Living` | Clue | Ảnh (phòng khách) |
| `IO_Photo_Guest` | Clue | Ảnh (phòng khách guest) |
| `IO_Safe` | LockedItem | Két sắt |
| `IO_Wife` | NPC | Vợ nạn nhân |

### Các trường cần điền

| Trường | Ý nghĩa | Ví dụ |
|--------|---------|-------|
| **Object Id** | Mã định danh (không trùng) | `io_painting` |
| **Interact Type** | Loại tương tác (xem bảng bên dưới) | `Clue` |
| **Hitbox Size** | Kích thước vùng click (pixel) | `100, 100` |
| **Is One Time Only** | Tick = chỉ tương tác được 1 lần | ✅ hoặc bỏ tick |
| **Highlight Sprite** | Hình hiện khi rê chuột vào (tùy chọn) | Kéo hình vào |

### 4 loại tương tác (Interact Type)

| Giá trị | Ý nghĩa | Trường cần điền thêm |
|---------|---------|----------------------|
| **Clue** | Manh mối — click vào sẽ thu thập | **Clue**: kéo file ClueSO vào |
| **NPC** | Nhân vật — click vào sẽ nói chuyện | **Dialogue**: kéo file DialogueSequenceSO vào |
| **LockedItem** | Vật bị khóa — click vào phải giải puzzle | **Lock Config**: kéo file LockConfigSO vào |
| **Decoration** | Trang trí — click vào không làm gì | Không cần điền thêm |

### Trường riêng cho LockedItem

| Trường | Ý nghĩa |
|--------|---------|
| **Lock Config** | File cấu hình ổ khóa (kéo thả) |
| **On Unlock Success** | Danh sách hành động khi mở khóa **thành công** (nhấn `+` để thêm, kéo file Action vào) |
| **On Unlock Fail** | Danh sách hành động khi mở khóa **thất bại** |

### Cách thêm 1 vật thể mới

1. Vào thư mục `Data/Interactables/`
2. **Chuột phải** → `Create > DoMiTruth > Interactable Object`
3. Đặt tên theo quy ước `IO_TenVatThe` (ví dụ: `IO_Painting`)
4. Chọn **Interact Type** phù hợp
5. Điền các trường tương ứng
6. **QUAN TRỌNG**: Mở file Room → thêm vật thể này vào danh sách **Interactables** + đặt tọa độ
7. **Ctrl+S** để lưu

---

## 6. Thêm / Sửa MANH MỐI (Clue)

**Đường dẫn:** `Assets/Luzart/DoMiTruth/Data/Clues/`

**Hiện có (12 manh mối):**
`Clue_BrokenVase`, `Clue_Diary`, `Clue_Footprint`, `Clue_GardenTool`, `Clue_Knife`, `Clue_Letter`, `Clue_Medicine`, `Clue_Phone`, `Clue_Photo`, `Clue_Ring`, `Clue_Testimony1`, `Clue_Testimony2`

### Các trường cần điền

| Trường | Ý nghĩa | Ví dụ |
|--------|---------|-------|
| **Clue Id** | Mã định danh (không trùng) | `clue_bloodstain` |
| **Clue Name** | Tên manh mối hiển thị | `Vết Máu` |
| **Clue Image** | Hình ảnh manh mối (kéo thả file hình) | Ảnh .png |
| **Description** | Mô tả chi tiết (nhiều dòng) | `Một vết máu khô trên sàn nhà...` |
| **Category** | Phân loại manh mối | Xem bảng bên dưới |

### 4 loại phân loại (Category)

| Giá trị | Ý nghĩa | Ví dụ |
|---------|---------|-------|
| **Physical** | Vật chứng vật lý | Dao, bình vỡ, dấu chân |
| **Document** | Tài liệu, giấy tờ | Nhật ký, thư, hóa đơn |
| **Testimony** | Lời khai, lời nói | Lời kể của nhân chứng |
| **Photo** | Ảnh chụp | Ảnh gia đình, ảnh hiện trường |

### Cách thêm 1 manh mối mới

1. Vào thư mục `Data/Clues/`
2. **Chuột phải** → `Create > DoMiTruth > Clue`
3. Đặt tên theo quy ước `Clue_TenManhMoi`
4. Điền thông tin
5. **SAU ĐÓ**: tạo 1 file Interactable (loại Clue) và gắn manh mối này vào
6. **SAU ĐÓ**: thêm Interactable đó vào 1 Room
7. **Ctrl+S** để lưu

---

## 7. Thêm / Sửa NHÂN VẬT (Character)

**Đường dẫn:** `Assets/Luzart/DoMiTruth/Data/Characters/`

**Hiện có:**
- `Char_Butler` — Quản gia
- `Char_Detective` — Thám tử (người chơi)
- `Char_Neighbor` — Hàng xóm
- `Char_Police` — Công an
- `Char_Wife` — Vợ nạn nhân

### Các trường cần điền

| Trường | Ý nghĩa | Ví dụ |
|--------|---------|-------|
| **Character Id** | Mã định danh | `char_gardener` |
| **Character Name** | Tên hiển thị | `Người Làm Vườn` |
| **Portrait** | Ảnh chân dung (kéo thả) | Ảnh .png |
| **Name Color** | Màu tên trong hội thoại (click vào ô màu để chọn) | Trắng mặc định |

### Cách thêm 1 nhân vật mới

1. Vào thư mục `Data/Characters/`
2. **Chuột phải** → `Create > DoMiTruth > Dialogue Character`
3. Đặt tên theo quy ước `Char_TenNV`
4. Điền thông tin
5. **SAU ĐÓ**: tạo file Dialogue sử dụng nhân vật này
6. **Ctrl+S** để lưu

---

## 8. Thêm / Sửa HỘI THOẠI (Dialogue)

**Đường dẫn:** `Assets/Luzart/DoMiTruth/Data/Dialogues/`

**Hiện có:**
- `Dlg_Intro` — Hội thoại mở đầu
- `Dlg_Butler` — Nói chuyện với Quản gia
- `Dlg_Neighbor` — Nói chuyện với Hàng xóm
- `Dlg_Wife` — Nói chuyện với Vợ

### Các trường cần điền

| Trường | Ý nghĩa | Ví dụ |
|--------|---------|-------|
| **Dialogue Id** | Mã định danh | `dlg_gardener` |
| **Lines** | Danh sách câu thoại (xem bên dưới) | |
| **Auto Advance** | Tick = tự chuyển câu (không cần click) | Thường bỏ tick |
| **Auto Advance Delay** | Thời gian chờ giữa các câu (giây) | `2` |

### Mỗi câu thoại (Line) gồm

| Trường | Ý nghĩa | Ví dụ |
|--------|---------|-------|
| **Character** | Ai nói câu này (kéo file Character vào) | `Char_Butler` |
| **Text** | Nội dung câu nói | `Tôi không biết gì cả...` |
| **Typing Speed** | Tốc độ đánh chữ (số lớn = nhanh) | `30` |
| **Wait For Input** | Tick = chờ người chơi click mới chuyển câu | ✅ |

### Cách thêm 1 đoạn hội thoại mới

1. Vào thư mục `Data/Dialogues/`
2. **Chuột phải** → `Create > DoMiTruth > Dialogue Sequence`
3. Đặt tên theo quy ước `Dlg_TenHoiThoai`
4. Trong **Lines**, nhấn `+` để thêm từng câu:
   - Kéo file Character vào ô **Character**
   - Gõ nội dung vào **Text**
   - Chỉnh **Typing Speed** (mặc định 30 là OK)
   - Tick **Wait For Input** nếu muốn chờ click
5. **SAU ĐÓ**: gắn dialogue này vào 1 Interactable (loại NPC) hoặc 1 Room (Entry Dialogue)
6. **Ctrl+S** để lưu

---

## 9. Thêm / Sửa Ổ KHÓA (Lock)

**Đường dẫn:** `Assets/Luzart/DoMiTruth/Data/Locks/`

**Hiện có:**
- `Lock_Cabinet` — Khóa tủ
- `Lock_Drawer` — Khóa ngăn kéo
- `Lock_Safe` — Khóa két sắt

### Các trường cần điền

| Trường | Ý nghĩa | Ví dụ |
|--------|---------|-------|
| **Lock Type** | Loại khóa | `Passcode` hoặc `SwipePattern` |
| **Hint Text** | Gợi ý cho người chơi | `"Ngày sinh của chủ nhà"` |

### Nếu Lock Type = Passcode (mã số)

| Trường | Ý nghĩa | Ví dụ |
|--------|---------|-------|
| **Passcode** | Mã đúng để mở | `1234` |

### Nếu Lock Type = SwipePattern (vẽ hình)

| Trường | Ý nghĩa |
|--------|---------|
| **Swipe Pattern** | Mảng số từ 0-8, thứ tự vẽ qua các chấm |

**Sơ đồ 9 chấm (3x3):**

```
 0  1  2
 3  4  5
 6  7  8
```

Ví dụ: Pattern hình chữ L = `[0, 3, 6, 7, 8]` (trên-trái → dưới-trái → dưới-phải)

### Cách thêm 1 ổ khóa mới

1. Vào thư mục `Data/Locks/`
2. **Chuột phải** → `Create > DoMiTruth > Lock Config`
3. Đặt tên theo quy ước `Lock_TenKhoa`
4. Chọn **Lock Type** và điền thông tin
5. **SAU ĐÓ**: gắn lock này vào 1 Interactable (loại LockedItem)
6. **Ctrl+S** để lưu

---

## 10. Thêm / Sửa HÀNH ĐỘNG (Action)

**Đường dẫn:** `Assets/Luzart/DoMiTruth/Data/Actions/`

Hành động (Action) dùng để **tự động làm gì đó** sau khi người chơi tương tác (ví dụ: mở khóa thành công → thu thập manh mối → hiện thông báo).

**Hiện có:**
| File | Loại | Mô tả |
|------|------|-------|
| `Act_ClosePopup` | ClosePopup | Đóng cửa sổ popup |
| `Act_Collect_GardenTool` | CollectClue | Thu thập dụng cụ vườn |
| `Act_Collect_Knife` | CollectClue | Thu thập dao |
| `Act_Collect_Ring` | CollectClue | Thu thập nhẫn |
| `Act_Toast_Unlocked` | ShowToast | Hiện thông báo "Đã mở khóa" |
| `Act_Toast_WrongCode` | ShowToast | Hiện thông báo "Sai mã" |
| `Act_Wait_1s` | Wait | Chờ 1 giây |

### 6 loại Action có thể tạo

| Loại | Menu tạo | Ý nghĩa | Trường cần điền |
|------|----------|---------|-----------------|
| **CollectClue** | `Create > DoMiTruth > Actions > Collect Clue` | Thu thập 1 manh mối | **Clue**: kéo file Clue vào |
| **ShowClueDetail** | `Create > DoMiTruth > Actions > Show Clue Detail` | Hiện chi tiết manh mối | **Clue**: kéo file Clue vào |
| **ShowDialogue** | `Create > DoMiTruth > Actions > Show Dialogue` | Phát hội thoại | **Dialogue**: kéo file Dialogue vào |
| **ShowToast** | `Create > DoMiTruth > Actions > Show Toast` | Hiện thông báo ngắn | **Message**: nhập nội dung |
| **Wait** | `Create > DoMiTruth > Actions > Wait` | Chờ một khoảng thời gian | **Duration**: số giây |
| **ClosePopup** | `Create > DoMiTruth > Actions > Close Popup` | Đóng popup | **Target UI**: chọn popup cần đóng (hoặc None = đóng cái trên cùng) |

### Ví dụ: Tạo chuỗi hành động khi mở két sắt thành công

1. Tạo `Act_ClosePopup` (đóng popup ổ khóa)
2. Tạo `Act_Wait_05s` (chờ 0.5 giây)
3. Tạo `Act_Collect_Ring` (thu thập nhẫn)
4. Tạo `Act_Toast_Found` (hiện "Bạn tìm thấy chiếc nhẫn!")
5. Mở file `IO_Safe` → trong **On Unlock Success**, thêm 4 action trên theo thứ tự

---

## 11. Cấu hình GAME CHUNG (GameConfig)

**Đường dẫn:** `Assets/Luzart/DoMiTruth/Data/Config/GameConfig.asset`

Click vào file này trong Unity, Inspector sẽ hiện:

### Cutscene (Video mở đầu)

| Trường | Ý nghĩa | Giá trị mặc định |
|--------|---------|-------------------|
| **Intro Cutscene** | File video mở đầu (kéo thả) | |
| **Cutscene Duration** | Thời lượng cutscene (giây) | `30` |
| **Skip Button Delay** | Sau bao lâu mới hiện nút Skip (giây) | `3` |

### Briefing (Giao nhiệm vụ)

| Trường | Ý nghĩa | Giá trị mặc định |
|--------|---------|-------------------|
| **Briefing Dialogue** | Hội thoại giao nhiệm vụ (kéo file Dialogue vào) | |
| **Briefing NPC Sprite** | Hình nhân vật công an | |
| **Briefing NPC Label** | Nhãn tên NPC | `POLICE` |

### Maps

| Trường | Ý nghĩa |
|--------|---------|
| **All Maps** | Danh sách TẤT CẢ map trong game (kéo thả file Map vào) |

> **LƯU Ý**: Khi tạo map mới, **BẮT BUỘC** phải thêm vào danh sách này, nếu không map sẽ không hiện trong game.

### Investigation (Khám phá)

| Trường | Ý nghĩa | Giá trị mặc định |
|--------|---------|-------------------|
| **Pan Speed** | Tốc độ kéo camera | `5` |
| **Pan Edge Threshold** | Khoảng cách mép màn hình để kéo camera (pixel) | `50` |

### Dialogue (Hội thoại)

| Trường | Ý nghĩa | Giá trị mặc định |
|--------|---------|-------------------|
| **Default Typing Speed** | Tốc độ đánh chữ mặc định | `30` |

### Effects (Hiệu ứng)

| Trường | Ý nghĩa | Giá trị mặc định |
|--------|---------|-------------------|
| **Clue Collect Fly Duration** | Thời gian hiệu ứng bay vào sổ tay (giây) | `0.8` |

---

## 12. Thêm ÂM THANH

Hiện tại game **chưa có file âm thanh** nào. Nếu muốn thêm:

### Bước 1: Import file âm thanh vào Unity

1. Tạo thư mục mới: trong cửa sổ Project, chuột phải vào `Assets/Luzart/DoMiTruth/` → `Create > Folder` → đặt tên `Audio`
2. **Kéo thả** file âm thanh (.mp3, .wav, .ogg) từ máy tính vào thư mục `Audio` trong Unity
3. Đợi Unity import xong

### Bước 2: Sử dụng âm thanh

> **Lưu ý**: Hệ thống âm thanh hiện chưa được tích hợp sẵn trong code. Bạn cần nhờ developer thêm AudioManager hoặc gắn AudioSource vào các sự kiện. Tôi (Claude) có thể giúp viết code này nếu bạn cần.

---

## 13. KHÔNG xóa vật thể sau khi tương tác

Mỗi vật thể tương tác có trường **Is One Time Only**:

- **Tick (bật)** = Vật thể **biến mất** sau khi tương tác 1 lần (mặc định cho manh mối)
- **Bỏ tick (tắt)** = Vật thể **vẫn còn**, có thể tương tác lại nhiều lần

### Cách sửa

1. Vào `Data/Interactables/`
2. Click vào file vật thể cần sửa (ví dụ: `IO_BrokenVase`)
3. Trong Inspector, tìm ô **Is One Time Only**
4. **Bỏ tick** nếu muốn giữ vật thể
5. **Ctrl+S** để lưu

---

## 14. Đổi VỊ TRÍ vật thể trong phòng

Vị trí của vật thể được cấu hình **trong file Room**, không phải trong file Interactable.

### Cách sửa

1. Vào `Data/Rooms/`
2. Click vào file phòng chứa vật thể (ví dụ: `Room_LivingRoom`)
3. Trong Inspector, tìm mục **Interactables**
4. Mở rộng từng item, tìm vật thể cần đổi vị trí
5. Sửa **Position On Background** (X, Y):
   - X dương = sang phải, X âm = sang trái
   - Y dương = lên trên, Y âm = xuống dưới
6. **Ctrl+S** để lưu
7. Chạy game để kiểm tra vị trí mới

### Mẹo tìm vị trí đúng

- Bắt đầu ở `(0, 0)` (giữa ảnh) rồi điều chỉnh dần
- Mỗi lần sửa, chạy game thử ngay để xem kết quả
- Ghi chú kích thước ảnh nền (ví dụ: 2560x1440), thì vùng hợp lệ là X từ -1280 đến 1280, Y từ -720 đến 720

---

## 15. Chỉnh sửa ANIMATION (Hiệu ứng chuyển động)

Game sử dụng hệ thống animation tên **DOTween** — tất cả hiệu ứng (fade, scale, di chuyển, đánh chữ...) đều có thể chỉnh sửa **trong Inspector** mà không cần viết code.

### 15.1. Animation là gì trong game này?

| Hiệu ứng | Ở đâu | Mô tả |
|-----------|-------|-------|
| **Fade in/out** | Mở/đóng màn hình UI | Mờ dần vào, mờ dần ra |
| **Scale (phóng to/thu nhỏ)** | Rê chuột vào vật thể | Vật thể phóng to nhẹ khi hover |
| **Di chuyển (Move)** | Thu thập manh mối | Manh mối bay từ vị trí nhặt về sổ tay |
| **Đánh chữ (Typing)** | Hội thoại NPC | Chữ hiện ra từng ký tự |
| **Toast (thông báo)** | Sau khi tương tác | Thông báo hiện rồi mờ đi |

### 15.2. Cách tìm animation trên một UI/vật thể

1. Trong Unity, mở cửa sổ **Hierarchy** (danh sách tất cả object trong scene)
2. Hoặc vào thư mục `Assets/Luzart/DoMiTruth/Prefabs/` → click đúp vào 1 prefab
3. Trong **Inspector** (bên phải), cuộn xuống tìm component có tên:
   - **Tween Animation** — animation đơn lẻ
   - **Sequence Tween Animation** — chuỗi nhiều animation
   - **Tween Animation Caller** — bộ kích hoạt animation
   - Hoặc tìm trong component UI: mục **Show Animation** và **Hide Animation**

### 15.3. Chỉnh Animation đơn lẻ (Tween Animation)

Khi click vào 1 object có component **Tween Animation**, bạn sẽ thấy các trường sau:

#### Cài đặt chung

| Trường | Ý nghĩa | Giá trị mặc định | Cách chỉnh |
|--------|---------|-------------------|------------|
| **Duration** | Thời lượng animation (giây) | `1` | Số nhỏ = nhanh hơn, số lớn = chậm hơn |
| **Easing** | Kiểu chuyển động | `Linear` | Chọn từ dropdown (xem bảng bên dưới) |
| **Delay Start** | Chờ bao lâu trước khi bắt đầu (giây) | `0` | `0` = bắt đầu ngay |
| **Is Ignore Time Scale** | Bỏ qua khi game pause | `false` | Tick nếu muốn animation vẫn chạy khi pause |

#### Cài đặt lặp lại (Loop)

| Trường | Ý nghĩa | Giá trị mặc định |
|--------|---------|-------------------|
| **Is Loop** | Có lặp lại không | `false` (không lặp) |
| **Loop Count** | Số lần lặp (`-1` = vô hạn) | `0` |
| **Loop Type** | Kiểu lặp | `Restart` |

**3 kiểu lặp (Loop Type):**

| Kiểu | Mô tả | Ví dụ |
|------|-------|-------|
| **Restart** | Chạy lại từ đầu | A→B, A→B, A→B... |
| **Yoyo** | Chạy tới rồi chạy ngược lại | A→B→A→B→A... |
| **Incremental** | Mỗi lần cộng thêm | A→B, B→C, C→D... |

#### Giá trị From / To

| Trường | Ý nghĩa |
|--------|---------|
| **Vector3 From** | Giá trị bắt đầu (vị trí/kích thước/góc xoay) |
| **Vector3 To** | Giá trị kết thúc |
| **Float From** | Giá trị số bắt đầu (dùng cho fade) |
| **Float To** | Giá trị số kết thúc |

#### 12 loại animation

| Loại | Mô tả | Dùng cho |
|------|-------|----------|
| **Move** | Di chuyển (world space) | Vật thể bay trên màn hình |
| **MoveLocal** | Di chuyển (local space) | Vật thể di chuyển so với cha |
| **MoveAnchors** | Di chuyển UI | Nút, hộp thoại trượt vào |
| **Scale** | Phóng to/thu nhỏ | Popup xuất hiện, hover effect |
| **Euler** | Xoay | Đồng hồ, biểu tượng quay |
| **FadeByCanvasGroup** | Mờ dần / hiện dần | Fade in/out màn hình |
| **SizeDelta** | Thay đổi kích thước UI | Thanh bar, hộp mở rộng |
| **AnchorMin** | Thay đổi anchor min | Layout responsive |
| **AnchorMax** | Thay đổi anchor max | Layout responsive |
| **Float** | Số bất kỳ (kèm event) | Thanh tiến trình, đếm số |
| **TextMeshProDOText** | Đánh chữ từng ký tự | Text hiện dần |
| **UnityEvent** | Gọi sự kiện | Kích hoạt code khác |

### 15.4. Bảng Easing (Kiểu chuyển động)

Đây là dropdown **Easing** trong mỗi animation. Nó quyết định animation chạy **mượt** hay **giật** như thế nào.

| Easing | Mô tả đơn giản | Khi nào dùng |
|--------|----------------|-------------|
| **Linear** | Tốc độ đều | Animation đơn giản, không cần fancy |
| **OutQuad** | Nhanh rồi chậm dần | Phổ biến nhất — tự nhiên, dễ chịu |
| **InQuad** | Chậm rồi nhanh dần | Vật rơi, tăng tốc |
| **InOutQuad** | Chậm → nhanh → chậm | Di chuyển mượt 2 chiều |
| **OutBack** | Bay quá rồi bật lại | Popup "nảy" vào — vui, đáng yêu |
| **OutBounce** | Nảy như quả bóng | Vật rơi xuống nảy |
| **OutElastic** | Rung lắc như lò xo | Gây chú ý, nhấn mạnh |
| **OutCubic** | Giống OutQuad nhưng mạnh hơn | Chuyển động nhanh, dừng gọn |
| **OutExpo** | Rất nhanh rồi dừng đột ngột | Hiệu ứng dramatic |

> **Mẹo**: Nếu không biết chọn gì, dùng **OutQuad** — nó hoạt động tốt cho hầu hết trường hợp.

### 15.5. Chỉnh Animation của UI (Mở/Đóng màn hình)

Mỗi màn hình UI (popup, screen) đều có thể gắn animation mở và đóng.

1. Vào `Assets/Luzart/DoMiTruth/Prefabs/` → click đúp vào 1 prefab (ví dụ: `Popup_ClueDetail`)
2. Click vào **object gốc** (trên cùng trong Hierarchy)
3. Trong Inspector, tìm component kế thừa từ **UIBase**
4. Tìm 2 trường:
   - **Show Animation** — animation khi MỞ (hiện lên)
   - **Hide Animation** — animation khi ĐÓNG (biến mất)
5. Mỗi cái trỏ tới 1 **Tween Animation Caller** trên object con

**Cách sửa animation mở/đóng:**
1. Click vào object con có **Tween Animation Caller**
2. Tìm component **Tween Animation** trên cùng object
3. Sửa **Duration**, **Easing**, **From/To** values
4. **Ctrl+S** để lưu

### 15.6. Chỉnh Animation khi hover vật thể (Interactable)

Khi rê chuột vào vật thể trong phòng, nó sẽ phóng to nhẹ. Giá trị mặc định:

| Hiệu ứng | Giá trị | Ý nghĩa |
|-----------|---------|---------|
| Hover vào | Scale `1.05` trong `0.2` giây | Phóng to 5% |
| Hover ra | Scale `1.0` trong `0.2` giây | Trở về bình thường |
| Easing | `OutQuad` | Chậm dần tự nhiên |

> **Lưu ý**: Giá trị hover này được viết trong code (`InteractableObject.cs`), không sửa được bằng Inspector. Nếu muốn thay đổi, hãy nhờ tôi (Claude) sửa code.

### 15.7. Chỉnh Animation thu thập manh mối

Khi nhặt manh mối, nó bay từ vị trí nhặt về góc sổ tay. Thời lượng tổng được cấu hình trong **GameConfig**:

| Trường trong GameConfig | Ý nghĩa | Mặc định |
|------------------------|---------|----------|
| **Clue Collect Fly Duration** | Tổng thời gian bay (giây) | `0.8` |

Chi tiết animation bay gồm 3 giai đoạn (tự động tính từ tổng thời gian):

| Giai đoạn | Thời gian | Easing | Mô tả |
|-----------|-----------|--------|-------|
| Phóng to | 30% tổng = 0.24s | OutBack | Manh mối phóng to + nảy |
| Bay về sổ tay | 50% tổng = 0.4s | InQuad | Bay nhanh dần |
| Thu nhỏ + mờ | 50% tổng = 0.4s | InQuad | Nhỏ dần và biến mất |

Nếu muốn hiệu ứng chậm/nhanh hơn: mở `GameConfig.asset` → sửa **Clue Collect Fly Duration** (ví dụ: `1.5` = chậm hơn, `0.4` = nhanh hơn).

### 15.8. Chỉnh tốc độ đánh chữ (Typing Animation)

Trong hội thoại, chữ hiện ra từng ký tự. Tốc độ được cấu hình ở **2 nơi**:

**Nơi 1: Mặc định cho toàn game**
- Mở `GameConfig.asset` → trường **Default Typing Speed**
- Mặc định: `30` (30 ký tự/giây)

**Nơi 2: Cho từng câu thoại riêng**
- Mở file Dialogue (ví dụ: `Dlg_Butler.asset`)
- Trong mỗi **Line**, trường **Typing Speed** sẽ ghi đè lên mặc định
- Đặt `0` = dùng mặc định từ GameConfig

| Giá trị | Tốc độ |
|---------|--------|
| `15` | Chậm — thích hợp cho cảnh buồn, nghiêm túc |
| `30` | Bình thường |
| `60` | Nhanh — thích hợp cho cảnh gấp gáp |
| `100` | Rất nhanh — gần như hiện ngay |

### 15.9. Chỉnh Animation thông báo (Toast)

Thông báo toast (ví dụ: "Đã mở khóa!") có animation mặc định:

| Giai đoạn | Thời gian | Mô tả |
|-----------|-----------|-------|
| Hiện | Ngay lập tức | Thông báo xuất hiện |
| Chờ | 1 giây | Giữ nguyên trên màn hình |
| Mờ dần | 0.5 giây | Fade out rồi biến mất |

> Giá trị này nằm trong code (`UIToast.cs`). Nếu muốn thay đổi, nhờ tôi (Claude) sửa.

### 15.10. Chỉnh Sequence Animation (Chuỗi animation)

**Sequence** cho phép ghép nhiều animation lại với nhau. Tìm component **Sequence Tween Animation** trên object.

Mỗi bước trong sequence có:

| Trường | Ý nghĩa |
|--------|---------|
| **Tween Animation** | Animation nào sẽ chạy (kéo thả) |
| **Sequence Type** | Cách ghép với bước trước |

**3 kiểu ghép (Sequence Type):**

| Kiểu | Mô tả | Ví dụ |
|------|-------|-------|
| **Append** | Chạy **sau khi** bước trước xong | Fade in → rồi mới → Scale up |
| **Join** | Chạy **cùng lúc** với bước trước | Fade in + Scale up đồng thời |
| **Insert** | Chạy tại **thời điểm cụ thể** | Chạy ở giây thứ 0.5 |

### 15.11. Bộ kích hoạt animation (Tween Animation Caller)

Quyết định **khi nào** animation chạy tự động.

| Trường | Ý nghĩa |
|--------|---------|
| **Tween Animation** | Animation nào sẽ chạy (kéo thả) |
| **Type Show** | Khi nào kích hoạt |

**4 kiểu kích hoạt (Type Show):**

| Giá trị | Khi nào chạy |
|---------|-------------|
| **None** | Không tự chạy — phải kích hoạt bằng code |
| **Awake** | Ngay khi object được tạo |
| **Start** | Khi scene bắt đầu |
| **OnEnable** | Mỗi lần object được bật (phổ biến nhất) |

### 15.12. Tổng hợp: Tất cả animation có thể sửa bằng Inspector

| Cái gì | Sửa ở đâu | Trường cần sửa |
|--------|-----------|---------------|
| Tốc độ đánh chữ (toàn game) | `Data/Config/GameConfig.asset` | Default Typing Speed |
| Tốc độ đánh chữ (từng câu) | `Data/Dialogues/Dlg_*.asset` | Mỗi Line → Typing Speed |
| Thời gian bay manh mối | `Data/Config/GameConfig.asset` | Clue Collect Fly Duration |
| Tốc độ kéo camera | `Data/Config/GameConfig.asset` | Pan Speed |
| Animation mở/đóng UI | Prefab UI → component Tween Animation | Duration, Easing, From/To |
| Animation chuỗi | Prefab UI → Sequence Tween Animation | Danh sách bước + Sequence Type |
| Animation lặp lại | Bất kỳ Tween Animation nào | Is Loop, Loop Count, Loop Type |

---

## 16. Danh sách tất cả file hiện có

### Maps (3)
| File | Map Id |
|------|--------|
| `Map_Floor1.asset` | Tầng 1 |
| `Map_Floor2.asset` | Tầng 2 |
| `Map_Garden.asset` | Vườn |

### Rooms (9)
| File | Phòng |
|------|-------|
| `Room_LivingRoom.asset` | Phòng khách |
| `Room_Kitchen.asset` | Nhà bếp |
| `Room_MasterBedroom.asset` | Phòng ngủ chính |
| `Room_Study.asset` | Phòng làm việc |
| `Room_Bathroom.asset` | Phòng tắm |
| `Room_GuestRoom.asset` | Phòng khách (guest) |
| `Room_FrontYard.asset` | Sân trước |
| `Room_FishPond.asset` | Hồ cá |
| `Room_Shed.asset` | Nhà kho |

### Interactables (19)
| File | Loại | Mô tả |
|------|------|-------|
| `IO_BrokenVase.asset` | Clue | Bình hoa vỡ |
| `IO_Butler.asset` | NPC | Quản gia |
| `IO_Cabinet.asset` | LockedItem | Tủ khóa |
| `IO_Diary.asset` | Clue | Nhật ký |
| `IO_Drawer.asset` | LockedItem | Ngăn kéo khóa |
| `IO_Footprint.asset` | Clue | Dấu chân |
| `IO_Footprint_Bath.asset` | Clue | Dấu chân (tắm) |
| `IO_Footprint_Yard.asset` | Clue | Dấu chân (sân) |
| `IO_GardenToolClue.asset` | Clue | Dụng cụ vườn |
| `IO_Letter.asset` | Clue | Lá thư |
| `IO_Letter_Pond.asset` | Clue | Lá thư (hồ) |
| `IO_Medicine.asset` | Clue | Thuốc |
| `IO_Neighbor.asset` | NPC | Hàng xóm |
| `IO_Phone.asset` | Clue | Điện thoại |
| `IO_Photo.asset` | Clue | Ảnh chụp |
| `IO_Photo_Living.asset` | Clue | Ảnh (phòng khách) |
| `IO_Photo_Guest.asset` | Clue | Ảnh (guest) |
| `IO_Safe.asset` | LockedItem | Két sắt |
| `IO_Wife.asset` | NPC | Vợ nạn nhân |

### Clues (12)
| File | Tên |
|------|-----|
| `Clue_BrokenVase.asset` | Bình hoa vỡ |
| `Clue_Diary.asset` | Nhật ký |
| `Clue_Footprint.asset` | Dấu chân |
| `Clue_GardenTool.asset` | Dụng cụ vườn |
| `Clue_Knife.asset` | Dao |
| `Clue_Letter.asset` | Lá thư |
| `Clue_Medicine.asset` | Thuốc |
| `Clue_Phone.asset` | Điện thoại |
| `Clue_Photo.asset` | Ảnh chụp |
| `Clue_Ring.asset` | Nhẫn |
| `Clue_Testimony1.asset` | Lời khai 1 |
| `Clue_Testimony2.asset` | Lời khai 2 |

### Characters (5)
| File | Tên |
|------|-----|
| `Char_Butler.asset` | Quản gia |
| `Char_Detective.asset` | Thám tử |
| `Char_Neighbor.asset` | Hàng xóm |
| `Char_Police.asset` | Công an |
| `Char_Wife.asset` | Vợ |

### Dialogues (4)
| File | Mô tả |
|------|-------|
| `Dlg_Intro.asset` | Mở đầu |
| `Dlg_Butler.asset` | Quản gia |
| `Dlg_Neighbor.asset` | Hàng xóm |
| `Dlg_Wife.asset` | Vợ |

### Locks (3)
| File | Mô tả |
|------|-------|
| `Lock_Cabinet.asset` | Khóa tủ |
| `Lock_Drawer.asset` | Khóa ngăn kéo |
| `Lock_Safe.asset` | Khóa két |

### Actions (7)
| File | Mô tả |
|------|-------|
| `Act_ClosePopup.asset` | Đóng popup |
| `Act_Collect_GardenTool.asset` | Thu thập dụng cụ vườn |
| `Act_Collect_Knife.asset` | Thu thập dao |
| `Act_Collect_Ring.asset` | Thu thập nhẫn |
| `Act_Toast_Unlocked.asset` | Thông báo "Đã mở" |
| `Act_Toast_WrongCode.asset` | Thông báo "Sai mã" |
| `Act_Wait_1s.asset` | Chờ 1 giây |

---

## 17. Mẹo & Lưu ý quan trọng

### Luôn nhớ

- **Ctrl+S** sau mỗi lần sửa — Unity không tự lưu
- Khi tạo file mới, **phải gắn nó vào nơi sử dụng** (ví dụ: tạo Room mới phải thêm vào Map, tạo Interactable mới phải thêm vào Room)
- **Không đổi tên file** khi đã gắn vào nơi khác (sẽ bị mất liên kết)
- **Không xóa file** đang được sử dụng

### Quy ước đặt tên

| Loại | Quy ước | Ví dụ |
|------|---------|-------|
| Map | `Map_TenMap` | `Map_Basement` |
| Room | `Room_TenPhong` | `Room_Garage` |
| Interactable | `IO_TenVatThe` | `IO_Painting` |
| Clue | `Clue_TenManhMoi` | `Clue_Bloodstain` |
| Character | `Char_TenNV` | `Char_Gardener` |
| Dialogue | `Dlg_TenHoiThoai` | `Dlg_Gardener` |
| Lock | `Lock_TenKhoa` | `Lock_Chest` |
| Action | `Act_MoTa` | `Act_Collect_Key` |

### Chuỗi tạo nội dung mới (checklist)

Khi muốn thêm 1 vật thể mới hoàn chỉnh, làm theo thứ tự:

1. ☐ Chuẩn bị hình ảnh (sprite, ảnh nền, chân dung...) → kéo vào Unity
2. ☐ Tạo file Clue/Character/Dialogue/Lock (tùy loại vật thể)
3. ☐ Tạo file Action (nếu cần chuỗi hành động)
4. ☐ Tạo file Interactable → gắn Clue/Dialogue/Lock vào
5. ☐ Mở file Room → thêm Interactable + đặt vị trí
6. ☐ (Nếu tạo Room mới) Mở file Map → thêm Room
7. ☐ (Nếu tạo Map mới) Mở GameConfig → thêm Map vào All Maps
8. ☐ **Ctrl+S** để lưu tất cả
9. ☐ Chạy game thử nghiệm

### Cách chạy game thử

1. Nhấn nút **Play** (hình tam giác ▶) ở giữa phía trên Unity
2. Game sẽ chạy trong cửa sổ Game
3. Nhấn lại nút **Play** để dừng
4. **Lưu ý**: Mọi thay đổi trong khi đang Play sẽ **KHÔNG được lưu** — hãy dừng game trước khi sửa

---

> File này được tạo tự động bởi Claude. Cập nhật lần cuối: 2026-03-30.
