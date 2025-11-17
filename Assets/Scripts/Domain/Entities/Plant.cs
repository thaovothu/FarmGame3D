using System;

namespace FarmGame.Domain.Entities
{
    public enum CropType
    {
        Tomato,
        Blueberry,
        Strawberry
    }

    [Serializable]
    public class Plant
    {
        // Serializable fields for JsonUtility
        public string Id;
        public CropType CropType;
        public long PlantedTimeTicks;
        public long LastHarvestTimeTicks;
        public int HarvestCount;
        public bool IsAlive;
        public int TotalYield; // Total fruits harvested
        
        // Config data (loaded from CSV)
        public float GrowthTimeMinutes;
        public int YieldPerHarvest;
        public int LifespanYields;
        public int SellPrice;

        // Properties for easy DateTime access
        public DateTime PlantedTime
        {
            get => new DateTime(PlantedTimeTicks);
            set => PlantedTimeTicks = value.Ticks;
        }

        public DateTime LastHarvestTime
        {
            get => new DateTime(LastHarvestTimeTicks);
            set => LastHarvestTimeTicks = value.Ticks;
        }
        
        public Plant()
        {
            Id = Guid.NewGuid().ToString();
            IsAlive = true;
            HarvestCount = 0;
            TotalYield = 0;
        }

        public Plant(CropType cropType, DateTime plantedTime, float growthTime, int yieldPerHarvest, int lifespanYields, int sellPrice)
        {
            Id = Guid.NewGuid().ToString();
            CropType = cropType;
            PlantedTime = plantedTime;
            LastHarvestTime = plantedTime;
            IsAlive = true;
            HarvestCount = 0;
            TotalYield = 0;
            GrowthTimeMinutes = growthTime;
            YieldPerHarvest = yieldPerHarvest;
            LifespanYields = lifespanYields;
            SellPrice = sellPrice;
        }

        /// <summary>
        /// Calculate how many harvests are ready based on current time
        /// Returns LifespanYields only when ALL growth cycles complete
        /// </summary>
        public int GetReadyHarvestCount(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive)
            {
                UnityEngine.Debug.Log($"[Plant.GetReadyHarvestCount] {CropType} - NOT ALIVE");
                return 0;
            }
            if (HarvestCount > 0)
            {
                UnityEngine.Debug.Log($"[Plant.GetReadyHarvestCount] {CropType} - Already harvested (HarvestCount={HarvestCount})");
                return 0; // Already harvested
            }

            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            if (adjustedGrowthTime <= 0f) return 0;

            var timeSinceLastHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            var totalGrowthTime = adjustedGrowthTime * LifespanYields;
            
            UnityEngine.Debug.Log($"[Plant.GetReadyHarvestCount] {CropType} - TimeSince={timeSinceLastHarvest:F2}min, NeedTotal={totalGrowthTime:F2}min, LastHarvest={LastHarvestTime}, Current={currentTime}");
            
            // Only ready when ALL yields complete
            if (timeSinceLastHarvest >= totalGrowthTime)
            {
                UnityEngine.Debug.Log($"[Plant.GetReadyHarvestCount] {CropType} - READY! Returning {LifespanYields} yields");
                return LifespanYields;
            }
            
            UnityEngine.Debug.Log($"[Plant.GetReadyHarvestCount] {CropType} - NOT READY (need {totalGrowthTime - timeSinceLastHarvest:F2} more minutes)");
            return 0;
        }

        /// <summary>
        /// Get time in minutes until ALL harvests complete (ready to harvest)
        /// </summary>
        public float GetTimeUntilNextHarvest(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive) return 0f;
            if (HarvestCount > 0) return 0f; // Already harvested

            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            if (adjustedGrowthTime <= 0f) return 0f;

            var timeSinceLastHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            var totalGrowthTime = adjustedGrowthTime * LifespanYields;
            
            // If all yields complete, return 0
            if (timeSinceLastHarvest >= totalGrowthTime)
                return 0f;
            
            // Time remaining until all yields complete
            var timeRemaining = totalGrowthTime - timeSinceLastHarvest;
            return (float)timeRemaining;
        }

        /// <summary>
        /// Check if plant has spoiled (unharvested products expired)
        /// Products spoil if not harvested within SpoilageTimeMinutes after ALL yields complete
        /// </summary>
        public bool HasSpoiled(DateTime currentTime, float spoilageTimeMinutes, float equipmentBonus = 0)
        {
            if (!IsAlive) return true;
            if (HarvestCount > 0) return false; // Already harvested, can't spoil
            
            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            if (adjustedGrowthTime <= 0f) return false;
            
            var timeSinceLastHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            var totalGrowthTime = adjustedGrowthTime * LifespanYields;
            
            // Check if all yields are complete
            if (timeSinceLastHarvest < totalGrowthTime)
                return false; // Still growing, can't spoil
            
            // Calculate time since all yields completed
            var timeSinceAllComplete = timeSinceLastHarvest - totalGrowthTime;
            
            // Has spoiled if waited longer than spoilage time after completion
            return timeSinceAllComplete > spoilageTimeMinutes;
        }

        /// <summary>
        /// Get time remaining in minutes before unharvested products spoil
        /// Returns -1 if not all yields complete yet or already harvested
        /// </summary>
        public float GetTimeUntilSpoilage(DateTime currentTime, float spoilageTimeMinutes, float equipmentBonus = 0)
        {
            if (!IsAlive) return -1;
            if (HarvestCount > 0) return -1; // Already harvested
            
            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            if (adjustedGrowthTime <= 0f) return -1;
            
            var timeSinceLastHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            var totalGrowthTime = adjustedGrowthTime * LifespanYields;
            
            // Not ready yet
            if (timeSinceLastHarvest < totalGrowthTime)
                return -1;
            
            // Calculate time since all yields completed
            var timeSinceAllComplete = timeSinceLastHarvest - totalGrowthTime;
            var timeRemaining = spoilageTimeMinutes - timeSinceAllComplete;
            
            return (float)Math.Max(0, timeRemaining);
        }

        /// <summary>
        /// Perform harvest and return the amount harvested
        /// Harvests ALL yields at once when complete
        /// </summary>
        public int Harvest(DateTime currentTime, float equipmentBonus = 0)
        {
            var readyCount = GetReadyHarvestCount(currentTime, equipmentBonus);
            if (readyCount <= 0) return 0;

            HarvestCount += readyCount;
            LastHarvestTime = currentTime;
            
            var yieldAmount = readyCount * YieldPerHarvest;
            TotalYield += yieldAmount;

            // Plant dies after harvesting all yields
            if (HarvestCount >= LifespanYields)
            {
                IsAlive = false;
            }

            return yieldAmount;
        }

        /// <summary>
        /// Determine whether this Plant contains valid data that should be persisted.
        /// Some placeholder/default instances may have zeroed config values and
        /// should not be saved or rendered as real plants.
        /// </summary>
        public bool IsValidForSave()
        {
            if (string.IsNullOrEmpty(Id)) return false;
            // Must have sensible config values
            if (GrowthTimeMinutes <= 0f) return false;
            if (YieldPerHarvest <= 0) return false;
            if (LifespanYields <= 0) return false;
            return true;
        }
    }
}
