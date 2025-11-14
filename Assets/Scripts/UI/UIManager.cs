using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FarmGame.Domain.Entities;

namespace FarmGame.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Display Panels")]
        [SerializeField] private Text resourcesText;
        [SerializeField] private Text plotsText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text goldText;  // Text hiển thị số gold
        [SerializeField] private GameObject winPanel;

        [Header("Action Buttons")]
        [SerializeField] private Button upgradeEquipmentButton;
        [SerializeField] private Button buyPlotButton;
        [SerializeField] private Button hireWorkerButton;
        [SerializeField] private Button sellAllButton;

        [Header("Shop Buttons")]
        [SerializeField] private Button buyTomatoSeedsButton;
        [SerializeField] private Button buyBlueberrySeedsButton;
        [SerializeField] private Button buyStrawberrySeedsButton;
        [SerializeField] private Button buyDairyCowButton;

        [Header("Input Fields")]
        [SerializeField] private InputField buyTomatoQuantityInput;
        [SerializeField] private InputField buyBlueberryQuantityInput;

        [Header("Seed Selection UI")]
        [SerializeField] private GameObject seedSelectionPanel;
        [SerializeField] private Transform seedButtonContainer;
        [SerializeField] private Button seedButtonPrefab;
        [SerializeField] private Button closeSeedPanelButton;

        private GameController _gameController;
        private float _messageDisplayTime = 0f;
        private const float MESSAGE_DURATION = 3f;
        private int _selectedPlotIndex = -1;

        public void Initialize(GameController gameController)
        {
            _gameController = gameController;
            SetupButtonListeners();
            
            if (winPanel != null)
                winPanel.SetActive(false);
            
            if (seedSelectionPanel != null)
                seedSelectionPanel.SetActive(false);
            
            if (closeSeedPanelButton != null)
                closeSeedPanelButton.onClick.AddListener(HideSeedSelection);
        }

        private void SetupButtonListeners()
        {
            // Equipment and Farm buttons
            if (upgradeEquipmentButton != null)
                upgradeEquipmentButton.onClick.AddListener(OnUpgradeEquipment);
            
            if (buyPlotButton != null)
                buyPlotButton.onClick.AddListener(OnBuyPlot);
            
            if (hireWorkerButton != null)
                hireWorkerButton.onClick.AddListener(OnHireWorker);
            
            if (sellAllButton != null)
                sellAllButton.onClick.AddListener(OnSellAll);

            // Shop buttons
            if (buyTomatoSeedsButton != null)
                buyTomatoSeedsButton.onClick.AddListener(OnBuyTomatoSeeds);
            
            if (buyBlueberrySeedsButton != null)
                buyBlueberrySeedsButton.onClick.AddListener(OnBuyBlueberrySeeds);
            
            if (buyStrawberrySeedsButton != null)
                buyStrawberrySeedsButton.onClick.AddListener(OnBuyStrawberrySeeds);
            
            if (buyDairyCowButton != null)
                buyDairyCowButton.onClick.AddListener(OnBuyDairyCow);
        }

        public void UpdateDisplay()
        {
            if (_gameController == null || _gameController.Farm == null)
                return;

            UpdateResourcesDisplay();
            UpdatePlotsDisplay();
            UpdateMessageDisplay();
            UpdateGoldDisplay();
        }

        private void UpdateGoldDisplay()
        {
            if (goldText == null) return;
            if (_gameController == null || _gameController.Farm == null) return;

            var gold = _gameController.Farm.Inventory.Gold;
            goldText.text = $"Gold: {gold:N0}";  // N0 = định dạng số có dấu phẩy (1,000)
        }

        private void UpdateResourcesDisplay()
        {
            if (resourcesText == null) return;

            var farm = _gameController.Farm;
            var inventory = farm.Inventory;
            var config = _gameController.Config;

            var sb = new StringBuilder();
            sb.AppendLine($"=== FARM RESOURCES ===");
            sb.AppendLine($"Gold: {inventory.Gold:N0} / {config.GoldTarget:N0}");
            sb.AppendLine($"Equipment Level: {inventory.EquipmentLevel} (+{inventory.GetEquipmentBonus() * 100:F0}% yield)");
            sb.AppendLine();
            
            sb.AppendLine($"=== WORKERS ===");
            sb.AppendLine($"Idle: {farm.GetIdleWorkerCount()} | Working: {farm.GetWorkingWorkerCount()} | Total: {farm.Workers.Count}");
            sb.AppendLine($"Pending Tasks: {farm.TaskQueue.Count}");
            sb.AppendLine();
            
            sb.AppendLine($"=== SEEDS ===");
            sb.AppendLine($"Tomato: {inventory.GetSeedCount(CropType.Tomato)}");
            sb.AppendLine($"Blueberry: {inventory.GetSeedCount(CropType.Blueberry)}");
            sb.AppendLine($"Strawberry: {inventory.GetSeedCount(CropType.Strawberry)}");
            sb.AppendLine();
            
            sb.AppendLine($"=== HARVESTED ===");
            sb.AppendLine($"Tomato: {inventory.GetHarvestedCount(CropType.Tomato)}");
            sb.AppendLine($"Blueberry: {inventory.GetHarvestedCount(CropType.Blueberry)}");
            sb.AppendLine($"Strawberry: {inventory.GetHarvestedCount(CropType.Strawberry)}");
            sb.AppendLine($"Milk: {inventory.Milk}");
            sb.AppendLine();
            
            sb.AppendLine($"=== PLOTS ===");
            sb.AppendLine($"Used: {farm.Plots.Count - farm.GetEmptyPlotCount()} | Empty: {farm.GetEmptyPlotCount()} | Total: {farm.Plots.Count}");

            resourcesText.text = sb.ToString();
        }

        private void UpdatePlotsDisplay()
        {
            if (plotsText == null) return;

            var farm = _gameController.Farm;
            var currentTime = DateTime.Now;
            var equipmentBonus = farm.Inventory.GetEquipmentBonus();

            var sb = new StringBuilder();
            sb.AppendLine("=== PLOT STATUS ===");
            sb.AppendLine();

            for (int i = 0; i < farm.Plots.Count; i++)
            {
                var plot = farm.Plots[i];
                sb.AppendLine($"Plot {i + 1}:");

                if (plot.Status == PlotStatus.Empty)
                {
                    sb.AppendLine("  [EMPTY]");
                }
                else if (plot.Status == PlotStatus.HasPlant && plot.Plant != null)
                {
                    var plant = plot.Plant;
                    sb.AppendLine($"  Crop: {plant.CropType}");
                    sb.AppendLine($"  Harvests: {plant.HarvestCount}/{plant.LifespanYields}");
                    
                    if (plant.IsAlive)
                    {
                        var readyCount = plant.GetReadyHarvestCount(currentTime, equipmentBonus);
                        if (readyCount > 0)
                        {
                            sb.AppendLine($"  READY TO HARVEST! ({readyCount})");
                        }
                        else
                        {
                            var timeUntilNext = plant.GetTimeUntilNextHarvest(currentTime, equipmentBonus);
                            sb.AppendLine($"  Next in: {FormatTime(timeUntilNext)}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("  [DEAD - Needs clearing]");
                    }
                }
                else if (plot.Status == PlotStatus.HasAnimal && plot.Animal != null)
                {
                    var animal = plot.Animal;
                    sb.AppendLine($"  Animal: {animal.AnimalType}");
                    sb.AppendLine($"  Productions: {animal.ProductionCount}/{animal.LifespanProductions}");
                    
                    if (animal.IsAlive)
                    {
                        var readyCount = animal.GetReadyProductionCount(currentTime, equipmentBonus);
                        if (readyCount > 0)
                        {
                            sb.AppendLine($"  READY TO COLLECT! ({readyCount})");
                        }
                        else
                        {
                            var timeUntilNext = animal.GetTimeUntilNextProduction(currentTime, equipmentBonus);
                            sb.AppendLine($"  Next in: {FormatTime(timeUntilNext)}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("  [DEAD - Needs clearing]");
                    }
                }

                sb.AppendLine();
            }

            plotsText.text = sb.ToString();
        }

        private void UpdateMessageDisplay()
        {
            if (messageText == null) return;

            if (_messageDisplayTime > 0)
            {
                _messageDisplayTime -= Time.deltaTime;
                if (_messageDisplayTime <= 0)
                {
                    messageText.text = "";
                }
            }
        }

        private string FormatTime(float minutes)
        {
            if (minutes <= 0)
            {
                return "0s";
            }
            else if (minutes < 1)
            {
                // < 1 phút → hiển thị giây (làm tròn xuống)
                return $"{(int)(minutes * 60)}s";
            }
            else if (minutes < 60)
            {
                // >= 1 phút → hiển thị phút:giây (chính xác)
                int mins = (int)minutes;
                int secs = (int)((minutes - mins) * 60);
                if (secs > 0)
                {
                    return $"{mins}m{secs}s";
                }
                else
                {
                    return $"{mins}m";
                }
            }
            else
            {
                // >= 60 phút → hiển thị giờ:phút
                var hours = (int)(minutes / 60);
                var mins = (int)(minutes % 60);
                return $"{hours}h {mins}m";
            }
        }

        public void ShowMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
                _messageDisplayTime = MESSAGE_DURATION;
            }
            Debug.Log(message);
        }

        // Button handlers
        private void OnUpgradeEquipment()
        {
            if (_gameController.UpgradeEquipment())
            {
                ShowMessage($"Equipment upgraded to level {_gameController.Farm.Inventory.EquipmentLevel}!");
            }
            else
            {
                ShowMessage("Not enough gold to upgrade equipment!");
            }
        }

        private void OnBuyPlot()
        {
            if (_gameController.BuyPlot())
            {
                ShowMessage($"New plot purchased! Total plots: {_gameController.Farm.Plots.Count}");
            }
            else
            {
                ShowMessage("Not enough gold to buy plot!");
            }
        }

        private void OnHireWorker()
        {
            if (_gameController.HireWorker())
            {
                ShowMessage($"Worker hired! Total workers: {_gameController.Farm.Workers.Count}");
            }
            else
            {
                ShowMessage("Not enough gold to hire worker!");
            }
        }

        private void OnSellAll()
        {
            if (_gameController.SellAllHarvest())
            {
                ShowMessage($"All harvest sold! Gold: {_gameController.Farm.Inventory.Gold:N0}");
            }
            else
            {
                ShowMessage("Nothing to sell!");
            }
        }

        private void OnBuyTomatoSeeds()
        {
            int quantity = 1;
            if (buyTomatoQuantityInput != null && int.TryParse(buyTomatoQuantityInput.text, out int parsed))
            {
                quantity = parsed;
            }

            if (_gameController.BuyTomatoSeeds(quantity))
            {
                ShowMessage($"Bought {quantity} tomato seeds!");
            }
            else
            {
                ShowMessage("Not enough gold!");
            }
        }

        private void OnBuyBlueberrySeeds()
        {
            int quantity = 1;
            if (buyBlueberryQuantityInput != null && int.TryParse(buyBlueberryQuantityInput.text, out int parsed))
            {
                quantity = parsed;
            }

            if (_gameController.BuyBlueberrySeeds(quantity))
            {
                ShowMessage($"Bought {quantity} blueberry seeds!");
            }
            else
            {
                ShowMessage("Not enough gold!");
            }
        }

        private void OnBuyStrawberrySeeds()
        {
            if (_gameController.BuyStrawberrySeeds())
            {
                ShowMessage($"Bought {_gameController.Config.StrawberryBulkSize} strawberry seeds (bulk)!");
            }
            else
            {
                ShowMessage("Not enough gold!");
            }
        }

        private void OnBuyDairyCow()
        {
            if (_gameController.BuyDairyCow())
            {
                ShowMessage("Dairy cow purchased and placed!");
            }
            else
            {
                ShowMessage("Not enough gold or no empty plot!");
            }
        }

        public void ShowWinScreen()
        {
            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
            ShowMessage($"CONGRATULATIONS! You've reached {_gameController.Config.GoldTarget:N0} gold!");
        }

        // ========== SEED SELECTION METHODS ==========
        
        /// <summary>
        /// Hiển thị panel chọn hạt giống khi click vào mảnh đất trống
        /// </summary>
        public void ShowSeedSelection(int plotIndex)
        {
            Debug.Log($"UIManager: ShowSeedSelection called for plot {plotIndex}");
            
            if (seedSelectionPanel == null)
            {
                Debug.LogError("UIManager: seedSelectionPanel is null! Please assign it in Inspector.");
                return;
            }
            
            if (seedButtonPrefab == null)
            {
                Debug.LogError("UIManager: seedButtonPrefab is null! Please assign it in Inspector.");
                return;
            }
            
            if (_gameController == null)
            {
                Debug.LogError("UIManager: _gameController is null!");
                return;
            }

            _selectedPlotIndex = plotIndex;
            Debug.Log($"Showing seed selection panel for plot {plotIndex}");

            // Xóa các button cũ
            if (seedButtonContainer != null)
            {
                foreach (Transform child in seedButtonContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                // Nếu không có container, dùng chính panel
                seedButtonContainer = seedSelectionPanel.transform;
                foreach (Transform child in seedButtonContainer)
                {
                    if (child.gameObject != closeSeedPanelButton?.gameObject)
                        Destroy(child.gameObject);
                }
            }

            // Lấy inventory
            var inventory = _gameController.Farm?.Inventory;
            if (inventory == null)
            {
                ShowMessage("Inventory not found!");
                return;
            }

            bool hasItems = false;

            // Tạo button cho từng loại hạt có sẵn
            var seedTypes = new List<(CropType type, int count)>
            {
                (CropType.Tomato, inventory.GetSeedCount(CropType.Tomato)),
                (CropType.Blueberry, inventory.GetSeedCount(CropType.Blueberry)),
                (CropType.Strawberry, inventory.GetSeedCount(CropType.Strawberry))
            };

            foreach (var seed in seedTypes)
            {
                if (seed.count <= 0) continue;
                
                hasItems = true;
                var btn = Instantiate(seedButtonPrefab, seedButtonContainer);
                var btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = $"🌱 {seed.type} ({seed.count})";
                }

                // Capture biến trong closure
                var capturedType = seed.type;
                btn.onClick.AddListener(() =>
                {
                    _gameController.PlantCropOnPlot(_selectedPlotIndex, capturedType);
                    HideSeedSelection();
                });
            }

            // Thêm button cho bò sữa nếu có
            if (inventory.DairyCowCount > 0)
            {
                hasItems = true;
                var btn = Instantiate(seedButtonPrefab, seedButtonContainer);
                var btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = $"🐄 Bò Sữa ({inventory.DairyCowCount})";
                }

                btn.onClick.AddListener(() =>
                {
                    _gameController.PlaceAnimalOnPlot(_selectedPlotIndex, AnimalType.DairyCow);
                    HideSeedSelection();
                });
            }

            if (!hasItems)
            {
                var btn = Instantiate(seedButtonPrefab, seedButtonContainer);
                var btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = "Không có vật phẩm!";
                }
                btn.interactable = false;
            }

            seedSelectionPanel.SetActive(true);
        }

        /// <summary>
        /// Ẩn panel chọn hạt giống
        /// </summary>
        public void HideSeedSelection()
        {
            if (seedSelectionPanel != null)
            {
                seedSelectionPanel.SetActive(false);
            }
            _selectedPlotIndex = -1;
        }

        /// <summary>
        /// Hiển thị thông tin về mảnh đất (thời gian còn lại để thu hoạch)
        /// </summary>
        public void ShowPlotInfo(string info)
        {
            ShowMessage(info);
        }
    }
}
