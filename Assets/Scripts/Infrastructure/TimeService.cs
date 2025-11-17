using System;
using FarmGame.Domain;
using FarmGame.Domain.Entities;

namespace FarmGame.Infrastructure
{
    public class TimeService
    {
        private readonly GameConfig _config;

        public TimeService(GameConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Process offline progress when player returns to the game
        /// </summary>
        public void ProcessOfflineProgress(Farm farm, DateTime currentTime)
        {
            UnityEngine.Debug.Log($"[TimeService.ProcessOfflineProgress] START - LastSaveTime: {farm.LastSaveTime}, CurrentTime: {currentTime}");
            
            if (farm.LastSaveTime >= currentTime)
            {
                UnityEngine.Debug.Log($"[TimeService.ProcessOfflineProgress] SKIPPED - LastSaveTime >= currentTime");
                return;
            }

            var equipmentBonus = farm.Inventory.GetEquipmentBonus();
            UnityEngine.Debug.Log($"[TimeService.ProcessOfflineProgress] Equipment Bonus: {equipmentBonus}, Processing {farm.Plots.Count} plots...");

            // Process each plot
            foreach (var plot in farm.Plots)
            {
                if (plot.Status == PlotStatus.HasPlant && plot.Plant != null)
                {
                    ProcessPlantOfflineProgress(plot.Plant, currentTime, equipmentBonus, farm.Inventory);
                    
                    // Check if plant is dead after harvest
                    if (!plot.Plant.IsAlive)
                    {
                        UnityEngine.Debug.Log($"[TimeService] Plant {plot.Plant.CropType} on plot {plot.Id} died after harvest (reached lifespan). Clearing plot.");
                        plot.Clear();
                    }
                    // Check if plant has spoiled
                    else if (plot.Plant.HasSpoiled(currentTime, _config.SpoilageTimeMinutes, equipmentBonus))
                    {
                        UnityEngine.Debug.Log($"[TimeService] Plant {plot.Plant.CropType} on plot {plot.Id} has SPOILED! Unharvested products expired after {_config.SpoilageTimeMinutes} minutes.");
                        plot.Clear();
                    }
                    else
                    {
                        // Log time remaining if there are ready harvests
                        var timeRemaining = plot.Plant.GetTimeUntilSpoilage(currentTime, _config.SpoilageTimeMinutes, equipmentBonus);
                        if (timeRemaining >= 0)
                        {
                            UnityEngine.Debug.Log($"[TimeService] Plant {plot.Plant.CropType} on plot {plot.Id} has ready harvest. Time until spoilage: {timeRemaining:F2} minutes");
                        }
                    }
                }
                else if (plot.Status == PlotStatus.HasAnimal && plot.Animal != null)
                {
                    ProcessAnimalOfflineProgress(plot.Animal, currentTime, equipmentBonus, farm.Inventory);
                    
                    // Check if animal is dead after collection
                    if (!plot.Animal.IsAlive)
                    {
                        UnityEngine.Debug.Log($"[TimeService] Animal {plot.Animal.AnimalType} on plot {plot.Id} died after production (reached lifespan). Clearing plot.");
                        plot.Clear();
                    }
                    // Check if animal has spoiled
                    else if (plot.Animal.HasSpoiled(currentTime, _config.SpoilageTimeMinutes, equipmentBonus))
                    {
                        UnityEngine.Debug.Log($"[TimeService] Animal {plot.Animal.AnimalType} on plot {plot.Id} has SPOILED! Uncollected products expired after {_config.SpoilageTimeMinutes} minutes.");
                        plot.Clear();
                    }
                    else
                    {
                        // Log time remaining if there are ready productions
                        var timeRemaining = plot.Animal.GetTimeUntilSpoilage(currentTime, _config.SpoilageTimeMinutes, equipmentBonus);
                        if (timeRemaining >= 0)
                        {
                            UnityEngine.Debug.Log($"[TimeService] Animal {plot.Animal.AnimalType} on plot {plot.Id} has ready production. Time until spoilage: {timeRemaining:F2} minutes");
                        }
                    }
                }
            }

            // Complete any worker tasks that finished during offline time
            foreach (var worker in farm.Workers)
            {
                if (worker.Status == WorkerStatus.Working)
                {
                    if (worker.IsTaskComplete(currentTime))
                    {
                        worker.CompleteTask();
                    }
                }
            }

            farm.LastSaveTime = currentTime;
        }

        private void ProcessPlantOfflineProgress(Plant plant, DateTime currentTime, float equipmentBonus, Inventory inventory)
        {
            UnityEngine.Debug.Log($"[TimeService] ProcessPlantOfflineProgress - Plant: {plant.CropType}, IsAlive: {plant.IsAlive}");
            
            if (!plant.IsAlive) return;

            // Worker tự động thu hoạch trong thời gian offline
            var readyHarvests = plant.GetReadyHarvestCount(currentTime, equipmentBonus);
            UnityEngine.Debug.Log($"[TimeService] Plant {plant.CropType} - ReadyHarvests: {readyHarvests}, HarvestCount: {plant.HarvestCount}, LifespanYields: {plant.LifespanYields}");
            
            if (readyHarvests > 0)
            {
                // Tự động thu hoạch tất cả harvest sẵn sàng
                var harvested = plant.Harvest(currentTime, equipmentBonus);
                inventory.AddHarvest(plant.CropType, harvested);
                UnityEngine.Debug.Log($"[TimeService Offline] Worker auto-harvested {harvested} {plant.CropType}(s). Plant IsAlive after harvest: {plant.IsAlive}");
            }
            else
            {
                UnityEngine.Debug.Log($"[TimeService] Plant {plant.CropType} - NO ready harvests (not complete yet or already harvested)");
            }
        }

        private void ProcessAnimalOfflineProgress(Animal animal, DateTime currentTime, float equipmentBonus, Inventory inventory)
        {
            UnityEngine.Debug.Log($"[TimeService] ProcessAnimalOfflineProgress - Animal: {animal.AnimalType}, IsAlive: {animal.IsAlive}");
            
            if (!animal.IsAlive) return;

            // Worker tự động collect sữa trong thời gian offline
            var readyProductions = animal.GetReadyProductionCount(currentTime, equipmentBonus);
            UnityEngine.Debug.Log($"[TimeService] Animal {animal.AnimalType} - ReadyProductions: {readyProductions}, ProductionCount: {animal.ProductionCount}, LifespanProductions: {animal.LifespanProductions}");
            
            if (readyProductions > 0)
            {
                // Tự động collect tất cả production sẵn sàng
                var collected = animal.Collect(currentTime, equipmentBonus);
                inventory.AddMilk(collected);
                UnityEngine.Debug.Log($"[TimeService Offline] Worker auto-collected {collected} milk from {animal.AnimalType}. Animal IsAlive after collect: {animal.IsAlive}");
            }
            else
            {
                UnityEngine.Debug.Log($"[TimeService] Animal {animal.AnimalType} - NO ready productions (not complete yet or already collected)");
            }
        }

        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }
    }
}
