using System;
using System.Text;
using UnityEngine;
using FarmGame.Domain.Entities;

namespace FarmGame.UI
{
    /// <summary>
    /// Console-based UI for testing and demonstration
    /// This shows how the game logic is completely decoupled from Unity UI
    /// </summary>
    public class ConsoleUI : MonoBehaviour
    {
        private GameController _gameController;
        private StringBuilder _consoleOutput;
        private Vector2 _scrollPosition;
        private string _commandInput = "";

        private void Start()
        {
            _gameController = FindObjectOfType<GameController>();
            _consoleOutput = new StringBuilder();
            
            LogToConsole("=== FARM GAME CONSOLE ===");
            LogToConsole("Type 'help' for commands");
            LogToConsole("");
            
            PrintStatus();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));
            
            // Console output
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(Screen.height - 100));
            GUILayout.Label(_consoleOutput.ToString());
            GUILayout.EndScrollView();
            
            // Command input
            GUILayout.BeginHorizontal();
            GUILayout.Label("> ", GUILayout.Width(20));
            _commandInput = GUILayout.TextField(_commandInput, GUILayout.Width(Screen.width - 150));
            if (GUILayout.Button("Execute", GUILayout.Width(100)) || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                ExecuteCommand(_commandInput);
                _commandInput = "";
            }
            GUILayout.EndHorizontal();
            
            GUILayout.EndArea();
        }

        private void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;
            
            LogToConsole($"> {command}");
            
            var parts = command.ToLower().Trim().Split(' ');
            var cmd = parts[0];

            try
            {
                switch (cmd)
                {
                    case "help":
                        ShowHelp();
                        break;
                    
                    case "status":
                    case "s":
                        PrintStatus();
                        break;
                    
                    case "plant":
                        if (parts.Length >= 3)
                        {
                            int plotIndex = int.Parse(parts[1]) - 1;
                            string cropType = parts[2];
                            PlantCrop(plotIndex, cropType);
                        }
                        else
                        {
                            LogToConsole("Usage: plant <plotNumber> <tomato|blueberry|strawberry>");
                        }
                        break;
                    
                    case "harvest":
                    case "h":
                        if (parts.Length >= 2)
                        {
                            int plotIndex = int.Parse(parts[1]) - 1;
                            HarvestPlot(plotIndex);
                        }
                        else
                        {
                            LogToConsole("Usage: harvest <plotNumber>");
                        }
                        break;
                    
                    case "harvestall":
                    case "ha":
                        HarvestAll();
                        break;
                    
                    case "buy":
                        if (parts.Length >= 2)
                        {
                            string item = parts[1];
                            int quantity = parts.Length >= 3 ? int.Parse(parts[2]) : 1;
                            BuyItem(item, quantity);
                        }
                        else
                        {
                            LogToConsole("Usage: buy <tomato|blueberry|strawberry|cow|plot|worker> [quantity]");
                        }
                        break;
                    
                    case "sell":
                        if (parts.Length >= 2)
                        {
                            string item = parts[1];
                            int quantity = parts.Length >= 3 ? int.Parse(parts[2]) : 0;
                            SellItem(item, quantity);
                        }
                        else
                        {
                            LogToConsole("Usage: sell <tomato|blueberry|strawberry|milk|all> [quantity]");
                        }
                        break;
                    
                    case "upgrade":
                        UpgradeEquipment();
                        break;
                    
                    case "worker":
                    case "w":
                        ShowWorkers();
                        break;
                    
                    case "plots":
                    case "p":
                        ShowPlots();
                        break;
                    
                    case "save":
                        _gameController.SaveGame();
                        LogToConsole("Game saved!");
                        break;
                    
                    case "new":
                        _gameController.NewGame();
                        LogToConsole("New game started!");
                        PrintStatus();
                        break;
                    
                    case "clear":
                        _consoleOutput.Clear();
                        break;
                    
                    default:
                        LogToConsole($"Unknown command: {cmd}. Type 'help' for commands.");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogToConsole($"Error: {ex.Message}");
            }
        }

        private void ShowHelp()
        {
            LogToConsole("Available commands:");
            LogToConsole("  status, s - Show farm status");
            LogToConsole("  plots, p - Show detailed plot info");
            LogToConsole("  worker, w - Show worker status");
            LogToConsole("  plant <plot#> <crop> - Plant a crop (tomato/blueberry/strawberry)");
            LogToConsole("  harvest <plot#>, h <plot#> - Harvest a plot");
            LogToConsole("  harvestall, ha - Harvest all ready plots");
            LogToConsole("  buy <item> [qty] - Buy seeds/cow/plot/worker");
            LogToConsole("  sell <item> [qty] - Sell harvest or 'all'");
            LogToConsole("  upgrade - Upgrade equipment");
            LogToConsole("  save - Save game");
            LogToConsole("  new - Start new game");
            LogToConsole("  clear - Clear console");
            LogToConsole("  help - Show this help");
        }

        private void PrintStatus()
        {
            var farm = _gameController.Farm;
            var inv = farm.Inventory;
            
            LogToConsole($"Gold: {inv.Gold:N0} | Equipment: Lv.{inv.EquipmentLevel} (+{inv.GetEquipmentBonus() * 100:F0}%)");
            LogToConsole($"Workers: {farm.GetIdleWorkerCount()} idle, {farm.GetWorkingWorkerCount()} working");
            LogToConsole($"Plots: {farm.GetEmptyPlotCount()} empty / {farm.Plots.Count} total");
            LogToConsole($"Seeds: T:{inv.GetSeedCount(CropType.Tomato)} B:{inv.GetSeedCount(CropType.Blueberry)} S:{inv.GetSeedCount(CropType.Strawberry)}");
            LogToConsole($"Harvest: T:{inv.GetHarvestedCount(CropType.Tomato)} B:{inv.GetHarvestedCount(CropType.Blueberry)} S:{inv.GetHarvestedCount(CropType.Strawberry)} M:{inv.Milk}");
        }

        private void PlantCrop(int plotIndex, string cropTypeName)
        {
            if (plotIndex < 0 || plotIndex >= _gameController.Farm.Plots.Count)
            {
                LogToConsole("Invalid plot number!");
                return;
            }

            CropType cropType;
            if (!Enum.TryParse(cropTypeName, true, out cropType))
            {
                LogToConsole("Invalid crop type!");
                return;
            }

            var plot = _gameController.Farm.Plots[plotIndex];
            if (_gameController.PlantCrop(plot.Id, cropType))
            {
                LogToConsole($"Planted {cropType} on plot {plotIndex + 1}");
            }
            else
            {
                LogToConsole($"Failed to plant {cropType} (no seeds or plot not empty)");
            }
        }

        private void HarvestPlot(int plotIndex)
        {
            if (plotIndex < 0 || plotIndex >= _gameController.Farm.Plots.Count)
            {
                LogToConsole("Invalid plot number!");
                return;
            }

            var plot = _gameController.Farm.Plots[plotIndex];
            
            if (plot.Status == PlotStatus.HasPlant)
            {
                if (_gameController.HarvestCrop(plot.Id))
                    LogToConsole($"Harvested plot {plotIndex + 1}");
                else
                    LogToConsole($"Nothing ready on plot {plotIndex + 1}");
            }
            else if (plot.Status == PlotStatus.HasAnimal)
            {
                if (_gameController.CollectMilk(plot.Id))
                    LogToConsole($"Collected milk from plot {plotIndex + 1}");
                else
                    LogToConsole($"Nothing ready on plot {plotIndex + 1}");
            }
            else
            {
                LogToConsole($"Plot {plotIndex + 1} is empty");
            }
        }

        private void HarvestAll()
        {
            int count = 0;
            foreach (var plot in _gameController.Farm.Plots)
            {
                if (plot.Status == PlotStatus.HasPlant && _gameController.HarvestCrop(plot.Id))
                    count++;
                else if (plot.Status == PlotStatus.HasAnimal && _gameController.CollectMilk(plot.Id))
                    count++;
            }
            LogToConsole($"Harvested {count} plots");
        }

        private void BuyItem(string item, int quantity)
        {
            bool success = false;
            switch (item.ToLower())
            {
                case "tomato":
                    success = _gameController.BuyTomatoSeeds(quantity);
                    break;
                case "blueberry":
                    success = _gameController.BuyBlueberrySeeds(quantity);
                    break;
                case "strawberry":
                    success = _gameController.BuyStrawberrySeeds();
                    break;
                case "cow":
                    success = _gameController.BuyDairyCow();
                    break;
                case "plot":
                    success = _gameController.BuyPlot();
                    break;
                case "worker":
                    success = _gameController.HireWorker();
                    break;
                default:
                    LogToConsole($"Unknown item: {item}");
                    return;
            }
            
            LogToConsole(success ? $"Bought {quantity} {item}" : "Purchase failed (not enough gold or no space)");
        }

        private void SellItem(string item, int quantity)
        {
            bool success = false;
            switch (item.ToLower())
            {
                case "tomato":
                    if (quantity == 0) quantity = _gameController.Farm.Inventory.GetHarvestedCount(CropType.Tomato);
                    success = _gameController.SellTomatoes(quantity);
                    break;
                case "blueberry":
                    if (quantity == 0) quantity = _gameController.Farm.Inventory.GetHarvestedCount(CropType.Blueberry);
                    success = _gameController.SellBlueberries(quantity);
                    break;
                case "strawberry":
                    if (quantity == 0) quantity = _gameController.Farm.Inventory.GetHarvestedCount(CropType.Strawberry);
                    success = _gameController.SellStrawberries(quantity);
                    break;
                case "milk":
                    if (quantity == 0) quantity = _gameController.Farm.Inventory.Milk;
                    success = _gameController.SellMilk(quantity);
                    break;
                case "all":
                    success = _gameController.SellAllHarvest();
                    break;
                default:
                    LogToConsole($"Unknown item: {item}");
                    return;
            }
            
            LogToConsole(success ? $"Sold {item}! Gold: {_gameController.Farm.Inventory.Gold:N0}" : "Nothing to sell");
        }

        private void UpgradeEquipment()
        {
            if (_gameController.UpgradeEquipment())
            {
                LogToConsole($"Equipment upgraded to level {_gameController.Farm.Inventory.EquipmentLevel}");
            }
            else
            {
                LogToConsole("Not enough gold!");
            }
        }

        private void ShowWorkers()
        {
            var farm = _gameController.Farm;
            LogToConsole($"Total Workers: {farm.Workers.Count}");
            LogToConsole($"Idle: {farm.GetIdleWorkerCount()}");
            LogToConsole($"Working: {farm.GetWorkingWorkerCount()}");
            LogToConsole($"Pending Tasks: {farm.TaskQueue.Count}");
        }

        private void ShowPlots()
        {
            var farm = _gameController.Farm;
            var currentTime = DateTime.Now;
            var bonus = farm.Inventory.GetEquipmentBonus();
            
            for (int i = 0; i < farm.Plots.Count; i++)
            {
                var plot = farm.Plots[i];
                if (plot.Status == PlotStatus.Empty)
                {
                    LogToConsole($"Plot {i + 1}: [EMPTY]");
                }
                else if (plot.Status == PlotStatus.HasPlant)
                {
                    var p = plot.Plant;
                    var ready = p.GetReadyHarvestCount(currentTime, bonus);
                    var status = ready > 0 ? $"READY({ready})" : $"Next:{p.GetTimeUntilNextHarvest(currentTime, bonus):F1}m";
                    LogToConsole($"Plot {i + 1}: {p.CropType} [{p.HarvestCount}/{p.LifespanYields}] {status}");
                }
                else if (plot.Status == PlotStatus.HasAnimal)
                {
                    var a = plot.Animal;
                    var ready = a.GetReadyProductionCount(currentTime, bonus);
                    var status = ready > 0 ? $"READY({ready})" : $"Next:{a.GetTimeUntilNextProduction(currentTime, bonus):F1}m";
                    LogToConsole($"Plot {i + 1}: {a.AnimalType} [{a.ProductionCount}/{a.LifespanProductions}] {status}");
                }
            }
        }

        private void LogToConsole(string message)
        {
            _consoleOutput.AppendLine(message);
            
            // Keep console output manageable
            if (_consoleOutput.Length > 10000)
            {
                _consoleOutput.Remove(0, 5000);
            }
            
            // Auto-scroll to bottom
            _scrollPosition.y = float.MaxValue;
        }
    }
}
