using System;
using System.Linq;
using UnityEngine;
using FarmGame.Domain.Entities;

namespace FarmGame.Domain.Services
{
    public class FarmService
    {
        private readonly GameConfig _config;

        public FarmService(GameConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Initialize a new farm with starting resources
        /// </summary>
        public Farm InitializeNewFarm()
        {
            var farm = new Farm();
            var currentTime = DateTime.Now;

            // Initialize inventory
            farm.Inventory.Initialize(_config.InitialGold, _config.InitialEquipmentLevel);
            farm.Inventory.AddSeeds(CropType.Tomato, _config.InitialTomatoSeeds);
            farm.Inventory.AddSeeds(CropType.Blueberry, _config.InitialBlueberrySeeds);
            farm.Inventory.AddSeeds(CropType.Strawberry, _config.InitialStrawberrySeeds);

            // Create initial plots
            for (int i = 0; i < _config.InitialPlots; i++)
            {
                farm.AddPlot(new Plot());
            }

            // Create initial workers
            for (int i = 0; i < _config.InitialWorkers; i++)
            {
                farm.AddWorker(new Worker(_config.WorkerActionTimeMinutes));
            }

            // KHÔNG đặt bò lên plot ban đầu - lưu số lượng trong inventory
            // Player sẽ tự đặt bằng cách click vào plot và chọn từ menu
            farm.Inventory.DairyCowCount = _config.InitialDairyCows;

            farm.GameStartTime = currentTime;
            farm.LastSaveTime = currentTime;

            return farm;
        }

        /// <summary>
        /// Plant a crop on a specific plot
        /// </summary>
        public bool PlantCrop(Farm farm, string plotId, CropType cropType, DateTime currentTime)
        {
            var plot = farm.GetPlotById(plotId);
            if (plot == null || !plot.IsEmpty())
                return false;

            if (!farm.Inventory.HasSeed(cropType))
                return false;

            var cropConfig = _config.GetCropConfig(cropType.ToString());
            if (cropConfig == null)
            {
                Debug.LogError($"[FarmService] PlantCrop FAILED: cropConfig is NULL for cropType={cropType}");
                return false;
            }

            Debug.Log($"[FarmService] PlantCrop: cropType={cropType} → GrowthTime={cropConfig.GrowthTimeMinutes}m, Yield={cropConfig.YieldPerHarvest}, Lifespan={cropConfig.LifespanYields}, SellPrice={cropConfig.SellPrice}");

            var plant = new Plant(
                cropType,
                currentTime,
                cropConfig.GrowthTimeMinutes,
                cropConfig.YieldPerHarvest,
                cropConfig.LifespanYields,
                cropConfig.SellPrice
            );

            Debug.Log($"[FarmService] Planting crop {cropType} on plot {plot.Id} (plantId={plant.Id}, GrowthTimeMinutes={plant.GrowthTimeMinutes})");
            plot.PlantCrop(plant);
            farm.Inventory.UseSeed(cropType);

            return true;
        }

        /// <summary>
        /// Place an animal on a specific plot
        /// </summary>
        public bool PlaceAnimal(Farm farm, string plotId, AnimalType animalType, DateTime currentTime)
        {
            var plot = farm.GetPlotById(plotId);
            if (plot == null || !plot.IsEmpty())
                return false;

            // Kiểm tra inventory có bò không
            if (animalType == AnimalType.DairyCow)
            {
                if (farm.Inventory.DairyCowCount <= 0)
                    return false;
                
                // Trừ 1 bò từ inventory
                farm.Inventory.DairyCowCount--;
            }

            var animalConfig = _config.GetAnimalConfig(animalType.ToString());
            if (animalConfig == null)
            {
                Debug.LogError($"[FarmService] PlaceAnimal FAILED: animalConfig is NULL for animalType={animalType}");
                return false;
            }

            Debug.Log($"[FarmService] PlaceAnimal: animalType={animalType} → ProductionTime={animalConfig.ProductionTimeMinutes}m, Yield={animalConfig.YieldPerProduction}, Lifespan={animalConfig.LifespanProductions}, Price={animalConfig.ProductPrice}");

            var animal = new Animal(
                animalType,
                currentTime,
                animalConfig.ProductionTimeMinutes,
                animalConfig.YieldPerProduction,
                animalConfig.LifespanProductions,
                animalConfig.ProductPrice
            );

            Debug.Log($"[FarmService] Placed animal {animalType} on plot {plot.Id} (animalId={animal.Id}, ProductionTimeMinutes={animal.ProductionTimeMinutes})");
            plot.PlaceAnimal(animal);
            return true;
        }

        /// <summary>
        /// Harvest a crop from a specific plot
        /// </summary>
        public int HarvestCrop(Farm farm, string plotId, DateTime currentTime)
        {
            var plot = farm.GetPlotById(plotId);
            if (plot == null || plot.Status != PlotStatus.HasPlant || plot.Plant == null)
                return 0;

            var equipmentBonus = farm.Inventory.GetEquipmentBonus();
            
            // Log time remaining before spoilage
            var timeUntilSpoilage = plot.Plant.GetTimeUntilSpoilage(currentTime, _config.SpoilageTimeMinutes, equipmentBonus);
            if (timeUntilSpoilage >= 0)
            {
                Debug.Log($"[FarmService] Harvesting {plot.Plant.CropType} on plot {plotId}. Time remaining before spoilage: {timeUntilSpoilage:F2} minutes");
            }
            
            var harvested = plot.Plant.Harvest(currentTime, equipmentBonus);

            if (harvested > 0)
            {
                farm.Inventory.AddHarvest(plot.Plant.CropType, harvested);
                Debug.Log($"[FarmService] Harvested {harvested} {plot.Plant.CropType}. Harvest count: {plot.Plant.HarvestCount}/{plot.Plant.LifespanYields}");
            }

            // If plant is dead, clear the plot
            if (!plot.Plant.IsAlive)
            {
                Debug.Log($"[FarmService] Plant {plot.Plant.CropType} reached end of lifespan. Clearing plot {plotId}");
                plot.Clear();
            }

            return harvested;
        }

        /// <summary>
        /// Collect milk from an animal on a specific plot
        /// </summary>
        public int CollectMilk(Farm farm, string plotId, DateTime currentTime)
        {
            var plot = farm.GetPlotById(plotId);
            if (plot == null || plot.Status != PlotStatus.HasAnimal || plot.Animal == null)
                return 0;

            var equipmentBonus = farm.Inventory.GetEquipmentBonus();
            
            // Log time remaining before spoilage
            var timeUntilSpoilage = plot.Animal.GetTimeUntilSpoilage(currentTime, _config.SpoilageTimeMinutes, equipmentBonus);
            if (timeUntilSpoilage >= 0)
            {
                Debug.Log($"[FarmService] Collecting from {plot.Animal.AnimalType} on plot {plotId}. Time remaining before spoilage: {timeUntilSpoilage:F2} minutes");
            }
            
            var collected = plot.Animal.Collect(currentTime, equipmentBonus);

            if (collected > 0)
            {
                farm.Inventory.AddMilk(collected);
                Debug.Log($"[FarmService] Collected {collected} milk from {plot.Animal.AnimalType}. Production count: {plot.Animal.ProductionCount}/{plot.Animal.LifespanProductions}");
            }

            // If animal is dead, clear the plot
            if (!plot.Animal.IsAlive)
            {
                Debug.Log($"[FarmService] Animal {plot.Animal.AnimalType} reached end of lifespan. Clearing plot {plotId}");
                plot.Clear();
            }

            return collected;
        }

        /// <summary>
        /// Clear spoiled crops/animals from all plots
        /// </summary>
        public void ClearSpoiledPlots(Farm farm, DateTime currentTime)
        {
            var equipmentBonus = farm.Inventory.GetEquipmentBonus();
            foreach (var plot in farm.Plots)
            {
                if (plot.NeedsClearing(currentTime, _config.SpoilageTimeMinutes, equipmentBonus))
                {
                    // Log what is being cleared
                    if (plot.Plant != null)
                    {
                        var timeUntilSpoilage = plot.Plant.GetTimeUntilSpoilage(currentTime, _config.SpoilageTimeMinutes, equipmentBonus);
                        Debug.Log($"[FarmService] ⚠️ SPOILED! Clearing {plot.Plant.CropType} from plot {plot.Id}. Product expired (time remaining was: {timeUntilSpoilage:F2} min)");
                    }
                    else if (plot.Animal != null)
                    {
                        var timeUntilSpoilage = plot.Animal.GetTimeUntilSpoilage(currentTime, _config.SpoilageTimeMinutes, equipmentBonus);
                        Debug.Log($"[FarmService] ⚠️ SPOILED! Clearing {plot.Animal.AnimalType} from plot {plot.Id}. Product expired (time remaining was: {timeUntilSpoilage:F2} min)");
                    }
                    
                    plot.Clear();
                }
            }
        }

        /// <summary>
        /// Get all plots that are ready for harvest
        /// </summary>
        public int GetReadyHarvestCount(Farm farm, DateTime currentTime)
        {
            int count = 0;
            var equipmentBonus = farm.Inventory.GetEquipmentBonus();

            foreach (var plot in farm.Plots)
            {
                if (plot.Status == PlotStatus.HasPlant && plot.Plant != null)
                {
                    if (plot.Plant.GetReadyHarvestCount(currentTime, equipmentBonus) > 0)
                    {
                        count++;
                    }
                }
                else if (plot.Status == PlotStatus.HasAnimal && plot.Animal != null)
                {
                    if (plot.Animal.GetReadyProductionCount(currentTime, equipmentBonus) > 0)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Upgrade farm equipment
        /// </summary>
        public bool UpgradeEquipment(Farm farm)
        {
            if (!farm.Inventory.CanAfford(_config.EquipmentUpgradeCost))
                return false;

            if (farm.Inventory.SpendGold(_config.EquipmentUpgradeCost))
            {
                farm.Inventory.UpgradeEquipment();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Buy a new plot
        /// </summary>
        public bool BuyPlot(Farm farm)
        {
            if (!farm.Inventory.CanAfford(_config.PlotBuyCost))
                return false;

            if (farm.Inventory.SpendGold(_config.PlotBuyCost))
            {
                farm.AddPlot(new Plot());
                return true;
            }

            return false;
        }
    }
}

