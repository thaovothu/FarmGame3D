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
        /// </summary>
        public int GetReadyHarvestCount(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive) return 0;

            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            // guard: avoid division by zero / invalid config
            if (adjustedGrowthTime <= 0f) return 0;

            var timeSinceLastHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            var readyHarvests = (int)(timeSinceLastHarvest / adjustedGrowthTime);

            // Can't harvest more than remaining lifespan
            var remainingHarvests = LifespanYields - HarvestCount;
            if (remainingHarvests <= 0) return 0;
            return Math.Min(readyHarvests, remainingHarvests);
        }

        /// <summary>
        /// Get time in minutes until next harvest
        /// </summary>
        public float GetTimeUntilNextHarvest(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive) return 0f;
            if (HarvestCount >= LifespanYields) return 0f;

            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            if (adjustedGrowthTime <= 0f) return 0f;

            var timeSinceLastHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            var remainder = timeSinceLastHarvest % adjustedGrowthTime;
            var timeUntilNext = adjustedGrowthTime - remainder;
            // if already ready, return 0
            if (timeUntilNext < 0f) return 0f;
            return (float)timeUntilNext;
        }

        /// <summary>
        /// Check if plant has spoiled (not harvested in time after final yield)
        /// </summary>
        public bool HasSpoiled(DateTime currentTime, float spoilageTimeMinutes)
        {
            if (!IsAlive) return true;
            if (HarvestCount < LifespanYields) return false;

            // After final harvest, player has spoilageTimeMinutes to collect
            var timeSinceLastPossibleHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            return timeSinceLastPossibleHarvest > spoilageTimeMinutes;
        }

        /// <summary>
        /// Perform harvest and return the amount harvested
        /// </summary>
        public int Harvest(DateTime currentTime, float equipmentBonus = 0)
        {
            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            if (adjustedGrowthTime <= 0f) return 0;

            var readyCount = GetReadyHarvestCount(currentTime, equipmentBonus);
            if (readyCount <= 0) return 0;

            HarvestCount += readyCount;
            // set LastHarvestTime to now (safer than adding minutes to avoid drift)
            LastHarvestTime = currentTime;

            var yieldAmount = readyCount * YieldPerHarvest;
            TotalYield += yieldAmount;

            // Check if plant is dead after this harvest
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
