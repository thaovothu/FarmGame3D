using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using FarmGame.Domain;
using FarmGame.Domain.Entities;
using FarmGame.Domain.Services;
using FarmGame.Infrastructure;
namespace FarmGame.UI
{
    public class GameController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private string configFilePath = "Config/game_config.csv";

        // Domain objects
        private GameConfig _config;
        private Farm _farm;
        private FarmService _farmService;
        private ShopService _shopService;
        private WorkerService _workerService;
        private TimeService _timeService;

        // UI Reference
        private UIManager _uiManager;

        // Auto-save timer
        private float _autoSaveInterval = 30f; // Save every 30 seconds
        private float _autoSaveTimer = 0f;

        // Worker update timer
        private float _workerUpdateInterval = 1f; // Update worker tasks every second
        private float _workerUpdateTimer = 0f;

        public Farm Farm => _farm;
        public GameConfig Config => _config;

        private void Awake()
        {
            Debug.Log("========== [GameController.Awake] START ==========");
            
            // Load configuration
            Debug.Log("[GameController.Awake] Loading configuration...");
            LoadConfiguration();

            // Initialize services
            Debug.Log("[GameController.Awake] Initializing services...");
            _farmService = new FarmService(_config);
            _shopService = new ShopService(_config);
            _workerService = new WorkerService(_config, _farmService);
            _timeService = new TimeService(_config);
            Debug.Log($"[GameController.Awake] Services initialized. _timeService IsNull: {_timeService == null}");

            // Load or create new farm
            Debug.Log("[GameController.Awake] About to load or create farm...");
            LoadOrCreateFarm();
            Debug.Log("[GameController.Awake] Farm loaded/created!");

            // Get UI Manager
            _uiManager = FindObjectOfType<UIManager>();
            if (_uiManager != null)
            {
                _uiManager.Initialize(this);
            }

            // Render plots và update UI sau khi load xong
            Debug.Log("[GameController.Awake] Rendering plots and updating UI...");
            FindObjectOfType<Farm3DView>()?.RenderPlots();
            if (_uiManager != null)
            {
                _uiManager.UpdateDisplay();
            }
            
            Debug.Log("========== [GameController.Awake] COMPLETE ==========");
        }

        private void LoadConfiguration()
        {
            try
            {
                // Try to load from Assets/Config first (for Unity Editor)
                var fullPath = Path.Combine(Application.dataPath, "..", configFilePath);
                
                if (!File.Exists(fullPath))
                {
                    // Try Resources folder as fallback
                    var textAsset = Resources.Load<TextAsset>("game_config");
                    if (textAsset != null)
                    {
                        // Create temp file for ConfigLoader
                        fullPath = Path.Combine(Application.temporaryCachePath, "game_config.csv");
                        File.WriteAllText(fullPath, textAsset.text);
                    }
                }

                _config = ConfigLoader.LoadConfig(fullPath);
                Debug.Log("Configuration loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load configuration: {ex.Message}");
                
                // Create default config
                _config = CreateDefaultConfig();
            }
        }

        private GameConfig CreateDefaultConfig()
        {
            var config = new GameConfig
            {
                InitialGold = 100,
                InitialPlots = 3,
                InitialTomatoSeeds = 10,
                InitialBlueberrySeeds = 10,
                InitialStrawberrySeeds = 0,
                InitialDairyCows = 2,
                InitialWorkers = 1,
                InitialEquipmentLevel = 1,
                WorkerActionTimeMinutes = 2,
                WorkerHireCost = 500,
                EquipmentUpgradeCost = 500,
                EquipmentYieldBonusPercent = 10,
                PlotBuyCost = 500,
                StrawberryBulkSize = 10,
                StrawberryBulkPrice = 400,
                GoldTarget = 1000000,
                SpoilageTimeMinutes = 60
            };

            // Add crop configs
            config.Crops["Tomato"] = new CropConfig
            {
                GrowthTimeMinutes = 10,
                YieldPerHarvest = 1,
                LifespanYields = 40,
                SellPrice = 5,
                SeedPrice = 30
            };

            config.Crops["Blueberry"] = new CropConfig
            {
                GrowthTimeMinutes = 15,
                YieldPerHarvest = 1,
                LifespanYields = 40,
                SellPrice = 8,
                SeedPrice = 50
            };

            config.Crops["Strawberry"] = new CropConfig
            {
                GrowthTimeMinutes = 5,
                YieldPerHarvest = 1,
                LifespanYields = 20,
                SellPrice = 3,
                SeedPrice = 40
            };

            config.Animals["DairyCow"] = new AnimalConfig
            {
                ProductionTimeMinutes = 30,
                YieldPerProduction = 1,
                LifespanProductions = 100,
                ProductPrice = 15,
                AnimalPrice = 100
            };

            return config;
        }

        private void LoadOrCreateFarm()
        {
            Debug.Log($"[GameController] LoadOrCreateFarm - HasSaveFile: {SaveSystem.HasSaveFile()}");
            
            if (SaveSystem.HasSaveFile())
            {
                // Lần chơi thứ 2, 3, ... → load từ save file
                Debug.Log("[GameController] Loading from save file...");
                var saveData = SaveSystem.Load(_config);
                
                Debug.Log($"[GameController] SaveData loaded. IsNull: {saveData == null}, Farm IsNull: {saveData?.Farm == null}");
                
                if (saveData != null && saveData.Farm != null)
                {
                    _farm = saveData.Farm;
                        // Remove any invalid plants that may have been saved by accident.
                        RemoveInvalidPlants(_farm);
                    
                    // Load vị trí của các plot
                    var farm3DView = FindObjectOfType<Farm3DView>();
                    if (farm3DView != null && saveData.PlotPositions != null)
                    {
                        farm3DView.LoadPlotPositions(saveData.PlotPositions);
                    }
                    
                    // Process offline progress
                    Debug.Log($"[GameController] About to process offline progress. _timeService IsNull: {_timeService == null}");
                    
                    if (_timeService != null)
                    {
                        try
                        {
                            var offlineMinutes = (float)(DateTime.Now - saveData.SaveTime).TotalMinutes;
                            Debug.Log($"[GameController] Processing offline time: {offlineMinutes:F2} minutes (from {saveData.SaveTime} to {DateTime.Now})");
                            _timeService.ProcessOfflineProgress(_farm, DateTime.Now);
                            Debug.Log($"[GameController] ProcessOfflineProgress completed!");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[GameController] Failed to process offline progress: {ex.Message}\nStackTrace: {ex.StackTrace}");
                        }
                    }
                    else
                    {
                        Debug.LogError("[GameController] _timeService is NULL! Cannot process offline progress!");
                    }
                    
                    Debug.Log($"Farm loaded from save. Offline time processed from {saveData.SaveTime} to {DateTime.Now}");
                }
                else
                {
                    Debug.LogWarning("Save file corrupted or empty, creating new farm from config");
                    CreateNewFarm();
                }
            }
            else
            {
                // Lần chơi đầu tiên → tạo farm mới từ config
                Debug.Log("[GameController] No save file found, creating new farm");
                CreateNewFarm();
            }
        }

        private void CreateNewFarm()
        {
            // Tạo farm mới từ config (lấy giá trị InitialTomatoSeeds, InitialBlueberrySeeds, v.v.)
            _farm = _farmService.InitializeNewFarm();
            
            // Lưu ngay để lần sau load được
            SaveGame();
            
            Debug.Log("New farm created from config");
        }

        // private void Update()
        // {
        //     if (_farm == null) return;

        //     var currentTime = DateTime.Now;

        //     // Process worker tasks
        //     _workerUpdateTimer += Time.deltaTime;
        //     if (_workerUpdateTimer >= _workerUpdateInterval)
        //     {
        //         _workerUpdateTimer = 0f;
        //         _workerService.ProcessWorkerTasks(_farm, currentTime);
                
        //         // Auto-queue harvest tasks
        //         _workerService.AutoQueueHarvestTasks(_farm, currentTime);

        //         // Clear spoiled plots
        //         _farmService.ClearSpoiledPlots(_farm, currentTime);
        //     }

        //     // Auto-save
        //     _autoSaveTimer += Time.deltaTime;
        //     if (_autoSaveTimer >= _autoSaveInterval)
        //     {
        //         _autoSaveTimer = 0f;
        //         SaveGame();
        //     }

        //     // Update UI
        //     if (_uiManager != null)
        //     {
        //         _uiManager.UpdateDisplay();
        //     }

        //     // Check win condition
        //     if (_farm.HasReachedGoal(_config.GoldTarget))
        //     {
        //         OnGameWon();
        //     }
        // }

        
private void Update()
{
    if (_farm == null) return;

    var currentTime = DateTime.Now;

    // Process worker tasks
    _workerUpdateTimer += Time.deltaTime;
    if (_workerUpdateTimer >= _workerUpdateInterval)
    {
        _workerUpdateTimer = 0f;

        if (_workerService != null)
        {
            try
            {
                _workerService.ProcessWorkerTasks(_farm, currentTime, out bool needsUIRefresh);
                _workerService.AutoQueueHarvestTasks(_farm, currentTime);
                
                // Cập nhật UI và render plots khi worker hoàn thành task
                if (needsUIRefresh)
                {
                    FindObjectOfType<Farm3DView>()?.RenderPlots();
                    _uiManager?.UpdateDisplay();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"WorkerService error: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("GameController.Update: _workerService is null.");
        }

        if (_farmService != null)
        {
            try
            {
                // Store plot count before clearing
                var plotsBeforeClearing = _farm.Plots.Count(p => p.Status != PlotStatus.Empty);
                
                _farmService.ClearSpoiledPlots(_farm, currentTime);
                
                // Check if any plots were cleared and refresh UI
                var plotsAfterClearing = _farm.Plots.Count(p => p.Status != PlotStatus.Empty);
                if (plotsBeforeClearing != plotsAfterClearing)
                {
                    // Some plants/animals were spoiled and cleared - refresh the 3D view
                    FindObjectOfType<Farm3DView>()?.RenderPlots();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"FarmService.ClearSpoiledPlots error: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("GameController.Update: _farmService is null.");
        }
    }

    // Auto-save
    _autoSaveTimer += Time.deltaTime;
    if (_autoSaveTimer >= _autoSaveInterval)
    {
        _autoSaveTimer = 0f;
        SaveGame();
    }

    // Update UI safely
    if (_uiManager == null)
    {
        _uiManager = FindObjectOfType<UIManager>();
        if (_uiManager == null)
        {
            Debug.LogWarning("GameController.Update: UIManager not found.");
        }
    }

    _uiManager?.UpdateDisplay();

    // Check win condition (guard _config)
    if (_config != null)
    {
        try
        {
            if (_farm.HasReachedGoal(_config.GoldTarget))
            {
                OnGameWon();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error checking win condition: {ex.Message}");
        }
    }
}

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        public void SaveGame()
        {
            if (_farm != null)
            {
                // Clean up any invalid/placeholder plants before persisting.
                RemoveInvalidPlants(_farm);
                
                // Lưu vị trí của các plot
                var farm3DView = FindObjectOfType<Farm3DView>();
                SaveSystem.Save(_farm, farm3DView);
                Debug.Log("Game saved");
            }
        }

        // Remove plants that are clearly invalid (likely placeholders or partially-initialized)
        // This prevents accidental persistence of empty/zeroed Plant instances which appear
        // in the save as phantom crops on load.
        private void RemoveInvalidPlants(Domain.Entities.Farm farm)
        {
            if (farm == null || farm.Plots == null) return;
            int removed = 0;
            foreach (var plot in farm.Plots)
            {
                if (plot == null) continue;
                var plant = plot.Plant;
                if (plant == null) continue;
                try
                {
                    if (!plant.IsValidForSave())
                    {
                        Debug.Log($"Removing invalid plant from plot {plot.Id ?? "?"} (cropType={plant.CropType}, growth={plant.GrowthTimeMinutes})");
                        plot.Plant = null;
                        plot.Status = Domain.Entities.PlotStatus.Empty;
                        removed++;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Error while validating plant on plot {plot.Id ?? "?"}: {ex.Message}");
                }
            }

            if (removed > 0)
            {
                Debug.Log($"Removed {removed} invalid plant(s) from farm before save/load.");
            }
        }

        private void OnGameWon()
        {
            Debug.Log($"Congratulations! You've reached {_config.GoldTarget} gold!");
            if (_uiManager != null)
            {
                _uiManager.ShowWinScreen();
            }
            
            // Chuyển sang scene Victory (index 2) sau 10 giây khi đạt 1,000,000 gold
            SaveGame();
            StartCoroutine(LoadVictorySceneAfterDelay(10f));
        }
        
        private System.Collections.IEnumerator LoadVictorySceneAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(2);
        }

        // Public methods for UI to call

        public bool PlantCrop(string plotId, CropType cropType)
        {
            return _farmService.PlantCrop(_farm, plotId, cropType, DateTime.Now);
        }

        public bool HarvestCrop(string plotId)
        {
            return _farmService.HarvestCrop(_farm, plotId, DateTime.Now) > 0;
        }

        public bool CollectMilk(string plotId)
        {
            return _farmService.CollectMilk(_farm, plotId, DateTime.Now) > 0;
        }

        public bool BuyTomatoSeeds(int quantity)
        {
            return _shopService.BuyTomatoSeeds(_farm, quantity);
        }

        public bool BuyBlueberrySeeds(int quantity)
        {
            return _shopService.BuyBlueberrySeeds(_farm, quantity);
        }

        public bool BuyStrawberrySeeds()
        {
            return _shopService.BuyStrawberrySeeds(_farm);
        }

        public bool BuyDairyCow()
        {
            return _shopService.BuyDairyCow(_farm, DateTime.Now);
        }

        public bool SellTomatoes(int quantity)
        {
            return _shopService.SellTomatoes(_farm, quantity);
        }

        public bool SellBlueberries(int quantity)
        {
            return _shopService.SellBlueberries(_farm, quantity);
        }

        public bool SellStrawberries(int quantity)
        {
            return _shopService.SellStrawberries(_farm, quantity);
        }

        public bool SellMilk(int quantity)
        {
            return _shopService.SellMilk(_farm, quantity);
        }

        public bool SellAllHarvest()
        {
            return _shopService.SellAllHarvest(_farm) > 0;
        }

        public bool UpgradeEquipment()
        {
            return _farmService.UpgradeEquipment(_farm);
        }

        public bool BuyPlot()
        {
            bool success = _farmService.BuyPlot(_farm);
            if (success)
            {
                var farm3DView = FindObjectOfType<Farm3DView>();
                int unplacedCount = farm3DView?.GetUnplacedPlotCount() ?? 0;
                _uiManager?.ShowMessage($"New plot purchased! You have {unplacedCount} plot(s) to place. Click background to position.");
            }
            return success;
        }

        public bool HireWorker()
        {
            return _workerService.HireWorker(_farm);
        }

        public void NewGame()
        {
            SaveSystem.DeleteSave();
            CreateNewFarm();
            
            if (_uiManager != null)
            {
                _uiManager.Initialize(this);
            }
        }

        // ========== PLOT INTERACTION METHODS ==========

        /// <summary>
        /// Được gọi khi người chơi click vào mảnh đất
        /// Xử lý các trường hợp: trống, đang trồng, sẵn sàng thu hoạch
        /// </summary>
        public void OnPlotClicked(int plotIndex)
        {
            Debug.Log($"GameController: OnPlotClicked called for plot {plotIndex}");
            
            if (_farm == null)
            {
                Debug.LogError("GameController: _farm is null!");
                return;
            }
            
            if (plotIndex < 0 || plotIndex >= _farm.Plots.Count)
            {
                Debug.LogError($"GameController: Invalid plot index {plotIndex}");
                return;
            }

            var plot = _farm.Plots[plotIndex];
            var now = DateTime.Now;

            // Trường hợp 1: Mảnh đất trống → hiện panel chọn hạt
            if (plot.IsEmpty())
            {
                Debug.Log($"Plot {plotIndex} is empty, showing seed selection");
                
                if (_uiManager == null)
                {
                    Debug.LogError("GameController: _uiManager is null!");
                    _uiManager = FindObjectOfType<UIManager>();
                }
                
                _uiManager?.ShowSeedSelection(plotIndex);
                return;
            }

            // Trường hợp 2: Có cây trồng
            if (plot.Status == PlotStatus.HasPlant && plot.Plant != null)
            {
                if (_farm?.Inventory == null)
                {
                    Debug.LogError("GameController.OnPlotClicked: _farm or Inventory is null!");
                    return;
                }
                
                var equipmentBonus = _farm.Inventory.GetEquipmentBonus();
                var readyHarvests = plot.Plant.GetReadyHarvestCount(now, equipmentBonus);
                
                if (readyHarvests > 0)
                {
                    // GIAI ĐOẠN 2: Cây đã chín - có thể thu hoạch hoặc xem thời gian còn lại trước khi spoil
                    
                    if (_config == null)
                    {
                        Debug.LogError("GameController.OnPlotClicked: _config is null!");
                        return;
                    }
                    
                    if (_farmService == null)
                    {
                        Debug.LogError("GameController.OnPlotClicked: _farmService is null!");
                        return;
                    }
                    
                    // Check if clicking to harvest or just checking status
                    var timeUntilSpoilage = plot.Plant.GetTimeUntilSpoilage(now, _config.SpoilageTimeMinutes, equipmentBonus);
                    
                    if (timeUntilSpoilage >= 0)
                    {
                        // Show spoilage countdown
                        var minutes = (int)timeUntilSpoilage;
                        var seconds = (int)((timeUntilSpoilage - minutes) * 60);
                        
                        string timeMsg;
                        if (minutes > 0)
                            timeMsg = $"{plot.Plant.CropType}: ✅ Sẵn sàng thu hoạch! Còn {minutes} phút {seconds} giây trước khi mất";
                        else
                            timeMsg = $"{plot.Plant.CropType}: ✅ Sẵn sàng thu hoạch! Còn {seconds} giây trước khi mất";
                        
                        _uiManager?.ShowPlotInfo(timeMsg);
                    }
                    
                    // Try to harvest
                    var harvested = _farmService.HarvestCrop(_farm, plot.Id, now);
                    if (harvested > 0)
                    {
                        _uiManager?.ShowMessage($"Thu hoạch được {harvested} {plot.Plant.CropType}!");
                        SaveGame();
                        FindObjectOfType<Farm3DView>()?.RenderPlots();
                        _uiManager?.UpdateDisplay();
                    }
                }
                else
                {
                    // GIAI ĐOẠN 1: Cây chưa chín → hiện thời gian còn lại để chín
                    var timeUntilReady = plot.Plant.GetTimeUntilNextHarvest(now, equipmentBonus);
                    
                    // Log để debug
                    var adjustedGrowthTime = plot.Plant.GrowthTimeMinutes / (1 + equipmentBonus);
                    Debug.Log($"[GameController] Equipment Level: {_farm.Inventory.EquipmentLevel}, Bonus: {equipmentBonus*100:F0}%, Original Growth: {plot.Plant.GrowthTimeMinutes}m, Adjusted: {adjustedGrowthTime}m, Time Until Ready: {timeUntilReady}m");
                    
                    var minutes = (int)timeUntilReady;
                    var seconds = (int)((timeUntilReady - minutes) * 60);
                    
                    string timeMsg;
                    if (minutes > 0)
                        timeMsg = $"{plot.Plant.CropType}: Còn {minutes} phút {seconds} giây để thu hoạch (Equipment +{equipmentBonus*100:F0}%)";
                    else
                        timeMsg = $"{plot.Plant.CropType}: Còn {seconds} giây để thu hoạch (Equipment +{equipmentBonus*100:F0}%)";
                    
                    _uiManager?.ShowPlotInfo(timeMsg);
                }
                return;
            }

            // Trường hợp 3: Có động vật (bò)
            if (plot.Status == PlotStatus.HasAnimal && plot.Animal != null)
            {
                if (_farm?.Inventory == null)
                {
                    Debug.LogError("GameController.OnPlotClicked: _farm or Inventory is null!");
                    return;
                }
                
                var equipmentBonus = _farm.Inventory.GetEquipmentBonus();
                var readyProductions = plot.Animal.GetReadyProductionCount(now, equipmentBonus);
                
                if (readyProductions > 0)
                {
                    // GIAI ĐOẠN 2: Sữa đã chín - có thể lấy hoặc xem thời gian còn lại trước khi spoil
                    
                    if (_config == null)
                    {
                        Debug.LogError("GameController.OnPlotClicked: _config is null!");
                        return;
                    }
                    
                    if (_farmService == null)
                    {
                        Debug.LogError("GameController.OnPlotClicked: _farmService is null!");
                        return;
                    }
                    
                    var timeUntilSpoilage = plot.Animal.GetTimeUntilSpoilage(now, _config.SpoilageTimeMinutes, equipmentBonus);
                    
                    if (timeUntilSpoilage >= 0)
                    {
                        // Show spoilage countdown
                        var minutes = (int)timeUntilSpoilage;
                        var seconds = (int)((timeUntilSpoilage - minutes) * 60);
                        
                        string timeMsg;
                        if (minutes > 0)
                            timeMsg = $"Bò: ✅ Sẵn sàng lấy sữa! Còn {minutes} phút {seconds} giây trước khi mất";
                        else
                            timeMsg = $"Bò: ✅ Sẵn sàng lấy sữa! Còn {seconds} giây trước khi mất";
                        
                        _uiManager?.ShowPlotInfo(timeMsg);
                    }
                    
                    // Try to collect
                    var collected = _farmService.CollectMilk(_farm, plot.Id, now);
                    if (collected > 0)
                    {
                        _uiManager?.ShowMessage($"Thu được {collected} sữa!");
                        SaveGame();
                        FindObjectOfType<Farm3DView>()?.RenderPlots();
                        _uiManager?.UpdateDisplay();
                    }
                }
                else
                {
                    // GIAI ĐOẠN 1: Sữa chưa chín → hiện thời gian còn lại để cho sữa
                    var timeUntilReady = plot.Animal.GetTimeUntilNextProduction(now, equipmentBonus);
                    
                    // Log để debug
                    var adjustedProductionTime = plot.Animal.ProductionTimeMinutes / (1 + equipmentBonus);
                    Debug.Log($"[GameController] Equipment Level: {_farm.Inventory.EquipmentLevel}, Bonus: {equipmentBonus*100:F0}%, Original Production: {plot.Animal.ProductionTimeMinutes}m, Adjusted: {adjustedProductionTime}m, Time Until Ready: {timeUntilReady}m");
                    
                    var minutes = (int)timeUntilReady;
                    var seconds = (int)((timeUntilReady - minutes) * 60);
                    
                    string timeMsg;
                    if (minutes > 0)
                        timeMsg = $"Bò: Còn {minutes} phút {seconds} giây để cho sữa (Equipment +{equipmentBonus*100:F0}%)";
                    else
                        timeMsg = $"Bò: Còn {seconds} giây để cho sữa (Equipment +{equipmentBonus*100:F0}%)";
                    
                    _uiManager?.ShowPlotInfo(timeMsg);
                }
            }
        }

        /// <summary>
        /// Trồng cây lên mảnh đất đã chọn
        /// </summary>
        public void PlantCropOnPlot(int plotIndex, CropType cropType)
        {
            if (_farm == null || _farmService == null) return;
            if (plotIndex < 0 || plotIndex >= _farm.Plots.Count)
            {
                _uiManager?.ShowMessage("Mảnh đất không hợp lệ!");
                return;
            }

            var plotId = _farm.Plots[plotIndex].Id;
            var success = _farmService.PlantCrop(_farm, plotId, cropType, DateTime.Now);
            
            if (success)
            {
                _uiManager?.ShowMessage($"Đã trồng {cropType} trên mảnh đất {plotIndex + 1}!");
                SaveGame();
                FindObjectOfType<Farm3DView>()?.RenderPlots();
                _uiManager?.UpdateDisplay();
            }
            else
            {
                _uiManager?.ShowMessage("Không thể trồng (không có hạt hoặc đất đã có cây)!");
            }
        }

        /// <summary>
        /// Đặt động vật lên mảnh đất đã chọn
        /// </summary>
        public void PlaceAnimalOnPlot(int plotIndex, AnimalType animalType)
        {
            if (_farm == null || _farmService == null) return;
            if (plotIndex < 0 || plotIndex >= _farm.Plots.Count)
            {
                _uiManager?.ShowMessage("Mảnh đất không hợp lệ!");
                return;
            }

            var plotId = _farm.Plots[plotIndex].Id;
            var success = _farmService.PlaceAnimal(_farm, plotId, animalType, DateTime.Now);
            
            if (success)
            {
                _uiManager?.ShowMessage($"Đã đặt {animalType} trên mảnh đất {plotIndex + 1}!");
                SaveGame();
                FindObjectOfType<Farm3DView>()?.RenderPlots();
                _uiManager?.UpdateDisplay();
            }
            else
            {
                _uiManager?.ShowMessage("Không thể đặt (không có động vật hoặc đất đã bị chiếm)!");
            }
        }

        /// <summary>
        /// Đặt plot vào vị trí đã chọn (được gọi khi click background)
        /// </summary>
        public void PlacePlotAtPosition(int plotIndex, Vector3 worldPosition)
        {
            if (_farm == null) return;
            if (plotIndex < 0 || plotIndex >= _farm.Plots.Count)
            {
                _uiManager?.ShowMessage("Mảnh đất không hợp lệ!");
                return;
            }

            var farm3DView = FindObjectOfType<Farm3DView>();
            if (farm3DView != null)
            {
                farm3DView.SetPlotPosition(plotIndex, worldPosition);
                farm3DView.RenderPlots();
                SaveGame();
                _uiManager?.UpdateDisplay();
                Debug.Log($"Plot {plotIndex} placed at position {worldPosition}");
            }
        }
    }
}
