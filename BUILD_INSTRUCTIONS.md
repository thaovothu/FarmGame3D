# Hướng dẫn BUILD và TEST Offline Progress

## VẤN ĐỀ
- Trong Unity Editor Play Mode: Awake() chỉ chạy khi nhấn Play, KHÔNG load save file khi stop/play lại
- Save file chỉ được load khi **chạy file .exe đã build**

## CÁCH TEST OFFLINE PROGRESS ĐÚNG

### Option 1: BUILD GAME (KHUYẾN NGHỊ)

1. **Mở Unity → File → Build Settings**
2. **Click "Add Open Scenes"** để thêm scene hiện tại
3. **Click "Build"** → Chọn folder (ví dụ: `C:\Users\manhpc\FarmGame\Build\`)
4. **Đợi build xong**

5. **TEST:**
   - Chạy file .exe trong folder Build
   - Chơi game một lúc (trồng cây, mua worker, etc)
   - **TẮT GAME** (đóng cửa sổ)
   - **ĐỢI 1-2 PHÚT**
   - **Chạy lại file .exe**
   - → Xem Console logs để verify ProcessOfflineProgress được gọi

### Option 2: Force Load Save trong Editor

Thêm [MenuItem] để force load save file trong Unity Editor:

**File: `Assets/Scripts/Editor/TestOfflineProgress.cs`** (tạo mới)

```csharp
using UnityEditor;
using UnityEngine;

public class TestOfflineProgress
{
    [MenuItem("Tools/Force Load Save File")]
    public static void ForceLoadSave()
    {
        var gameController = GameObject.FindObjectOfType<FarmGame.UI.GameController>();
        if (gameController != null)
        {
            // Force reload save file
            var method = gameController.GetType().GetMethod("LoadOrCreateFarm", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(gameController, null);
            Debug.Log("Force reloaded save file!");
        }
    }
}
```

Sau đó trong Editor:
- **Tools → Force Load Save File** để reload save

## VERIFY LOGS

Khi chạy file .exe build, bạn PHẢI thấy logs này theo thứ tự:

```
========== [GameController.Awake] START ==========
[GameController.Awake] Loading configuration...
[GameController.Awake] Initializing services...
[GameController.Awake] Services initialized. _timeService IsNull: False
[GameController.Awake] About to load or create farm...
[GameController] LoadOrCreateFarm - HasSaveFile: True
[GameController] Loading from save file...
[GameController] SaveData loaded. IsNull: False, Farm IsNull: False
[GameController] About to process offline progress. _timeService IsNull: False
[GameController] Processing offline time: X.XX minutes (from ... to ...)
[TimeService.ProcessOfflineProgress] START - LastSaveTime: ..., CurrentTime: ...
[TimeService.ProcessOfflineProgress] Equipment Bonus: X, Processing Y plots...
[TimeService] ProcessPlantOfflineProgress - Plant: Tomato, IsAlive: True
[Plant.GetReadyHarvestCount] Tomato - TimeSince=X.XXmin, NeedTotal=X.XXmin...
[TimeService Offline] Worker auto-harvested X Tomato(s)
```

## SAVE FILE LOCATION

Save file được lưu tại:
- **Windows**: `C:\Users\<Username>\AppData\LocalLow\DefaultCompany\FarmGame\farm_save.json`

Để xem save file:
1. Mở File Explorer
2. Gõ: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\FarmGame`
3. Xem file `farm_save.json`

## LƯU Ý

- **Unity Editor Play Mode**: Chỉ để test gameplay realtime, KHÔNG test offline
- **Build .exe**: Mới test được offline progress đúng
- **Auto-save**: Game tự save mỗi 30 giây và khi tắt game
