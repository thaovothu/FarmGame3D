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
            if (farm.LastSaveTime >= currentTime)
                return;

            var equipmentBonus = farm.Inventory.GetEquipmentBonus();

            // Process each plot
            foreach (var plot in farm.Plots)
            {
                if (plot.Status == PlotStatus.HasPlant && plot.Plant != null)
                {
                    ProcessPlantOfflineProgress(plot.Plant, currentTime, equipmentBonus, farm.Inventory);
                    
                    // Check if plant has spoiled
                    if (plot.Plant.HasSpoiled(currentTime, _config.SpoilageTimeMinutes))
                    {
                        plot.Clear();
                    }
                }
                else if (plot.Status == PlotStatus.HasAnimal && plot.Animal != null)
                {
                    ProcessAnimalOfflineProgress(plot.Animal, currentTime, equipmentBonus, farm.Inventory);
                    
                    // Check if animal has spoiled
                    if (plot.Animal.HasSpoiled(currentTime, _config.SpoilageTimeMinutes))
                    {
                        plot.Clear();
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
            if (!plant.IsAlive) return;

            // Calculate how many harvests occurred during offline time
            var readyHarvests = plant.GetReadyHarvestCount(currentTime, equipmentBonus);
            
            if (readyHarvests > 0)
            {
                // Auto-harvest during offline time
                var harvested = plant.Harvest(currentTime, equipmentBonus);
                inventory.AddHarvest(plant.CropType, harvested);
            }
        }

        private void ProcessAnimalOfflineProgress(Animal animal, DateTime currentTime, float equipmentBonus, Inventory inventory)
        {
            if (!animal.IsAlive) return;

            // Calculate how many productions occurred during offline time
            var readyProductions = animal.GetReadyProductionCount(currentTime, equipmentBonus);
            
            if (readyProductions > 0)
            {
                // Auto-collect during offline time
                var collected = animal.Collect(currentTime, equipmentBonus);
                inventory.AddMilk(collected);
            }
        }

        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }
    }
}
