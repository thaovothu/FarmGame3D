# DEBUG CHECKLIST - Tại sao click vào plot không hiện gì?

## Đã thêm Debug Logs
✅ PlotView.OnMouseDown() - Log khi click
✅ GameController.OnPlotClicked() - Log khi xử lý
✅ UIManager.ShowSeedSelection() - Log khi hiện panel

## Các vấn đề có thể gặp và cách khắc phục:

### 1. Plot Prefab KHÔNG CÓ COLLIDER ⚠️
**Triệu chứng:** Console không log gì khi click vào plot

**Kiểm tra:**
- Mở Unity Editor
- Chọn Plot_0, Plot_1, Plot_2 trong Hierarchy (khi đang Play)
- Xem Inspector có component **BoxCollider** không?

**Khắc phục:**
1. Mở `Assets/Prefabs/Plot.prefab`
2. Add Component → **Box Collider**
3. Điều chỉnh:
   - Center: (0, 0, 0)
   - Size: (1, 0.2, 1)
4. Save prefab

**Hoặc:** Farm3DView.RenderPlots() tự động thêm collider nếu thiếu (đã có code)

---

### 2. CAMERA THIẾU PHYSICS RAYCASTER ⚠️⚠️
**Triệu chứng:** Console không log gì, OnMouseDown không được gọi

**Kiểm tra:**
- Chọn **Main Camera** trong Hierarchy
- Xem Inspector có component **Physics Raycaster** không?

**Khắc phục:**
1. Chọn Main Camera
2. Add Component → **Physics Raycaster**
3. Hoặc thêm **Physics 2D Raycaster** nếu dùng 2D

---

### 3. UIMANAGER CHƯA GÁN REFERENCES ⚠️⚠️⚠️
**Triệu chứng:** Console log "seedSelectionPanel is null!" hoặc "seedButtonPrefab is null!"

**Kiểm tra:**
1. Chọn GameObject có component **UIManager** (thường là Canvas)
2. Xem Inspector → section **"Seed Selection UI"**
3. Các trường này CÓ GÁN chưa:
   - Seed Selection Panel
   - Seed Button Container
   - Seed Button Prefab
   - Close Seed Panel Button

**Khắc phục:** Xem file `HUONG_DAN_SETUP_UI.md` - Bước 2

---

### 4. SEED SELECTION PANEL CHƯA TẠO
**Triệu chứng:** Console log "seedSelectionPanel is null!"

**Khắc phục:** Tạo UI theo `HUONG_DAN_SETUP_UI.md` - Bước 1

---

### 5. EVENTSSYSTEM THIẾU TRONG SCENE
**Triệu chứng:** Mọi UI không hoạt động

**Kiểm tra:**
- Xem Hierarchy có GameObject tên **EventSystem** không?

**Khắc phục:**
1. Right-click Hierarchy → **UI → Event System**
2. Hoặc GameObject → UI → Event System

---

## Cách Test Nhanh (5 phút):

### Bước 1: Play Unity và click vào plot
- Xem Console có log không?

### Bước 2: Phân tích log
```
✓ "PlotView: OnMouseDown called for plot 0"
  → Collider OK, click được nhận

✓ "GameController: OnPlotClicked called for plot 0"
  → GameController nhận được

✓ "Plot 0 is empty, showing seed selection"
  → Logic đúng

✓ "UIManager: ShowSeedSelection called for plot 0"
  → UIManager được gọi

✗ "seedSelectionPanel is null!"
  → NGUYÊN NHÂN: Chưa gán reference trong Inspector
```

### Bước 3: Sửa theo log lỗi

---

## Nếu KHÔNG CÓ LOG GÌ CẢ:

### Khả năng 1: Plot không có Collider
```
→ Mở Prefab/Plot.prefab
→ Add BoxCollider
→ Play lại
```

### Khả năng 2: Camera thiếu Physics Raycaster
```
→ Chọn Main Camera
→ Add Component: Physics Raycaster
→ Play lại
```

### Khả năng 3: Plot chưa được spawn
```
→ Kiểm tra Hierarchy khi Play
→ Có Plot_0, Plot_1, Plot_2 không?
→ Nếu không → Farm3DView không chạy
→ Kiểm tra Farm3DView.Start() có lỗi trong Console
```

---

## Nếu CÓ LOG nhưng panel không hiện:

### Check 1: seedSelectionPanel có active trong scene không?
```
Play → Pause
→ Chọn SeedSelectionPanel trong Hierarchy
→ Xem checkbox active bên cạnh tên
→ Nếu inactive → code SetActive(true) không chạy
```

### Check 2: Panel có nằm ngoài màn hình không?
```
→ Chọn SeedSelectionPanel
→ Xem RectTransform Position
→ Nếu x,y rất lớn → panel nằm ngoài
→ Set Position về (0, 0, 0)
```

### Check 3: Canvas có được setup đúng không?
```
→ Chọn Canvas
→ Render Mode: Screen Space - Overlay
→ Canvas Scaler: Scale With Screen Size (tùy chọn)
```

---

## Nếu panel hiện nhưng không có button:

### Check: Inventory có items không?
```
→ Xem Console log inventory counts
→ Nếu TomatoSeeds = 0, BlueberrySeeds = 0, DairyCowCount = 0
→ Panel sẽ chỉ hiện "Không có vật phẩm!"
```

**Khắc phục:**
1. Xóa save: `C:\Users\manhpc\AppData\LocalLow\DefaultCompany\FarmGame\Saves\savegame.json`
2. Play lại → farm mới sẽ có 10 tomato, 10 blueberry, 2 cows

---

## Quick Fix (Nếu tất cả fail):

### Tạo button test đơn giản:
1. Tạo UI Button trong Canvas
2. Tên: "Test Plot Click"
3. Add script:
```csharp
public void OnTestClick()
{
    var gc = FindObjectOfType<GameController>();
    gc.OnPlotClicked(0); // Test plot 0
}
```
4. Gán onClick → OnTestClick
5. Play → Click button → Panel phải hiện

Nếu button này hoạt động → vấn đề là Plot Collider / Camera Raycaster
Nếu button này KHÔNG hoạt động → vấn đề là UIManager chưa setup

---

## Câu hỏi cần trả lời (gửi kết quả):

1. Khi click vào plot, Console log gì? (copy toàn bộ)
2. Hierarchy có Plot_0, Plot_1, Plot_2 không?
3. Main Camera có Physics Raycaster không?
4. UIManager có gán đủ 4 references không?
5. Hierarchy có SeedSelectionPanel không?
6. EventSystem có trong scene không?

Copy Console logs gửi lại, mình sẽ biết chính xác vấn đề!
