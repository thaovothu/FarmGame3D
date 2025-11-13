# HƯỚNG DẪN SETUP UI CHO HỆ THỐNG TRỒNG CÂY

## Đã Thực Hiện
✅ Sửa code trong 3 file:
- `Assets/Scripts/UI/UIManager.cs` - Thêm panel chọn hạt giống
- `Assets/Scripts/UI/GameController.cs` - Thêm logic xử lý click vào đất
- `Assets/Scripts/UI/PlotView.cs` - Đơn giản hóa xử lý click

## Bước 1: Tạo UI Panel Chọn Hạt Giống

### 1.1. Tạo Panel
1. Trong Hierarchy, chuột phải vào Canvas → **UI → Panel**
2. Đổi tên thành `SeedSelectionPanel`
3. Điều chỉnh kích thước và vị trí:
   - **Anchor**: Center-Middle
   - **Width**: 300
   - **Height**: 400
   - **Pos X, Y**: 0, 0

### 1.2. Tạo Container cho Buttons
1. Chuột phải vào `SeedSelectionPanel` → **UI → Vertical Layout Group** (hoặc tạo Empty GameObject)
2. Đổi tên thành `ButtonContainer`
3. Thêm component **Vertical Layout Group**:
   - Spacing: 10
   - Child Force Expand: Width ✓, Height ✗
   - Child Control Size: Width ✓, Height ✓
4. Thêm component **Content Size Fitter**:
   - Vertical Fit: Preferred Size

### 1.3. Tạo Button Prefab cho Seeds
1. Trong Hierarchy, chuột phải vào Canvas (tạm thời) → **UI → Button**
2. Đổi tên thành `SeedButton`
3. Chỉnh sửa Text con của Button:
   - Text: "Tomato (10)"
   - Font Size: 18
   - Alignment: Center
   - Color: Đen hoặc màu dễ đọc
4. Điều chỉnh Button:
   - Width: 250
   - Height: 50
5. **Kéo `SeedButton` từ Hierarchy vào thư mục `Assets/Prefabs/UI/`** để tạo prefab
6. Xóa `SeedButton` khỏi Canvas (đã có prefab)

### 1.4. Tạo Nút Đóng Panel
1. Chuột phải vào `SeedSelectionPanel` → **UI → Button**
2. Đổi tên thành `CloseButton`
3. Text con: "X" hoặc "Đóng"
4. Đặt vị trí góc trên bên phải của Panel:
   - Anchor: Top-Right
   - Pos X: -10, Pos Y: -10
   - Width: 40, Height: 40

### 1.5. Ẩn Panel Ban Đầu
- Chọn `SeedSelectionPanel` trong Hierarchy
- **Bỏ tick** ở checkbox bên cạnh tên (inactive)

## Bước 2: Gán References vào UIManager

1. Chọn GameObject có component **UIManager** trong Hierarchy (thường là Canvas hoặc GameManager)
2. Trong Inspector, tìm section **"Seed Selection UI"**
3. Gán các trường:
   - **Seed Selection Panel**: Kéo `SeedSelectionPanel` vào
   - **Seed Button Container**: Kéo `ButtonContainer` (con của SeedSelectionPanel) vào
   - **Seed Button Prefab**: Kéo prefab `SeedButton` từ thư mục Prefabs vào
   - **Close Seed Panel Button**: Kéo `CloseButton` vào

## Bước 3: Đảm Bảo Plot Có Collider

### 3.1. Kiểm tra Plot Prefab
1. Mở `Assets/Prefabs/Plot.prefab` (hoặc tạo nếu chưa có)
2. Chọn prefab trong Project
3. Trong Inspector, kiểm tra có component **Collider** chưa:
   - BoxCollider
   - hoặc MeshCollider
   - hoặc SphereCollider

### 3.2. Thêm BoxCollider nếu chưa có
1. Click **Add Component**
2. Tìm **Box Collider**
3. Điều chỉnh:
   - Center: (0, 0, 0)
   - Size: (1, 0.2, 1) - hoặc kích thước phù hợp với model đất

## Bước 4: Test Game

### 4.1. Chạy Game
- Nhấn **Play** trong Unity Editor

### 4.2. Test Flow
1. **Click vào mảnh đất trống**:
   - Panel chọn hạt giống sẽ hiện
   - Hiển thị các loại hạt có sẵn (Tomato: 10, Blueberry: 10)
   
2. **Chọn một loại hạt**:
   - Click vào button (ví dụ: Tomato (10))
   - Panel đóng
   - Message hiện: "Đã trồng Tomato trên mảnh đất 1!"
   - Số hạt giảm trong inventory
   - Cây xuất hiện trên đất (nếu có prefab)

3. **Click vào mảnh đất đang trồng cây (chưa chín)**:
   - Message hiện: "Tomato: Còn X phút Y giây để thu hoạch"

4. **Đợi cây chín (hoặc chỉnh thời gian trong CSV)**:
   - Click vào mảnh đất
   - Message: "Thu hoạch được 1 Tomato!"
   - Số sản phẩm trong inventory tăng
   - Cây có thể tiếp tục lớn hoặc bị xóa (tùy lifespan)

5. **Click vào mảnh đất có bò**:
   - Nếu chưa cho sữa: "Bò: Còn X phút Y giây để cho sữa"
   - Nếu đã sẵn sàng: "Thu được 1 sữa!"

## Bước 5: Tùy Chỉnh (Tùy Chọn)

### 5.1. Thay Đổi Thời Gian Trồng (để test nhanh)
1. Mở `Config/game_config.csv`
2. Tìm dòng Tomato
3. Thay `GrowthTimeMinutes` từ 10 → 0.5 (30 giây)
4. Save file
5. Restart game để load config mới

### 5.2. Tăng Số Hạt Ban Đầu
1. Mở `Config/game_config.csv`
2. Tìm `InitialTomatoSeeds`, `InitialBlueberrySeeds`
3. Thay giá trị (ví dụ: 50, 50)
4. Xóa save file cũ: `C:\Users\manhpc\AppData\LocalLow\DefaultCompany\FarmGame\Saves\savegame.json`
5. Restart game

### 5.3. Styling Panel
- Chọn `SeedSelectionPanel`
- Thay đổi màu, opacity trong component **Image**
- Thêm Outline, Shadow cho Text
- Thêm Background Sprite đẹp hơn

## Troubleshooting

### Lỗi: "Seed selection UI not configured properly"
- Kiểm tra đã gán đủ 4 trường trong UIManager Inspector chưa
- Kiểm tra SeedButtonPrefab có component Button và Text con chưa

### Lỗi: "Adding component failed... Add required component Collider"
- Thêm BoxCollider vào Plot prefab (xem Bước 3)

### Panel không hiện khi click vào đất
- Kiểm tra Plot prefab có Collider chưa
- Kiểm tra Camera có component **Physics Raycaster** chưa (thường có sẵn)
- Kiểm tra EventSystem có trong scene chưa (UI → Event System)

### Click vào đất không có phản ứng
1. Chọn Plot prefab trong scene (khi Play)
2. Trong Inspector, kiểm tra component **Plot View** có _controller được gán chưa
3. Kiểm tra Console có lỗi gì không

### Hạt giống không giảm sau khi trồng
- Kiểm tra FarmService.PlantCrop có gọi Inventory.UseSeed chưa (đã có trong code)
- Kiểm tra SaveGame có được gọi không (đã tự động)

## Flow Tổng Quan

```
Player Click Vào Đất
        ↓
  PlotView.OnMouseDown()
        ↓
  GameController.OnPlotClicked(plotIndex)
        ↓
  ┌─────────────────────────────┐
  │ Kiểm tra trạng thái plot    │
  └─────────────────────────────┘
        ↓
  ┌─────────────┬──────────────────┬─────────────┐
  │  Đất trống  │  Đang trồng cây  │  Có động vật│
  └─────────────┴──────────────────┴─────────────┘
        ↓                ↓                  ↓
  UIManager         Chín chưa?         Cho sữa chưa?
  .ShowSeedSelection    ↓                  ↓
        ↓           ┌────┴────┐      ┌─────┴─────┐
  Player chọn hạt   │Chín│Chưa│      │Chưa│Sẵn sàng│
        ↓           ↓    ↓     ↓      ↓    ↓
  GameController  Thu  Show   Thu   Show
  .PlantCropOnPlot hoạch time  sữa   time
        ↓           ↓          ↓
  FarmService    Update     Update
  .PlantCrop     Inventory  Inventory
        ↓           ↓          ↓
  Inventory     SaveGame   SaveGame
  .UseSeed        ↓          ↓
        ↓        RenderPlots
  SaveGame       UpdateDisplay
        ↓
  RenderPlots (hiện cây 3D)
        ↓
  UpdateDisplay (cập nhật UI text)
```

## Kết Luận

Bây giờ bạn đã có hệ thống:
1. ✅ Click vào đất trống → chọn hạt giống → trồng
2. ✅ Click vào cây chưa chín → xem thời gian còn lại
3. ✅ Click vào cây đã chín → thu hoạch
4. ✅ Số hạt và sản phẩm tự động cập nhật inventory
5. ✅ Tự động save game

Chúc bạn code vui! 🎮🌱
