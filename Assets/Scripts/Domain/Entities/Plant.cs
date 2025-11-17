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
        /// Re-initialize config from GameConfig after JsonUtility deserialization
        /// JsonUtility doesn't call constructors, so config fields may be 0
        /// </summary>
        public void InitializeConfig(GameConfig gameConfig)
        {
            if (gameConfig == null) return;
            
            var cropConfig = gameConfig.GetCropConfig(CropType.ToString());
            if (cropConfig != null)
            {
                GrowthTimeMinutes = cropConfig.GrowthTimeMinutes;
                YieldPerHarvest = cropConfig.YieldPerHarvest;
                LifespanYields = cropConfig.LifespanYields;
                SellPrice = cropConfig.SellPrice;
                UnityEngine.Debug.Log($"[Plant.InitializeConfig] {CropType} → GrowthTime={GrowthTimeMinutes}m, Yield={YieldPerHarvest}, Lifespan={LifespanYields}");
            }
            else
            {
                UnityEngine.Debug.LogError($"[Plant.InitializeConfig] No config found for {CropType}!");
            }
            
            // Fix LastHarvestTime if it's DateTime.MinValue (Ticks = 0 from JSON)
            if (LastHarvestTimeTicks == 0 || LastHarvestTime == DateTime.MinValue)
            {
                // Set LastHarvestTime to PlantedTime if not yet harvested
                if (HarvestCount == 0)
                {
                    LastHarvestTime = PlantedTime;
                    UnityEngine.Debug.LogWarning($"[Plant.InitializeConfig] {CropType} - Fixed LastHarvestTime=DateTime.MinValue → PlantedTime={PlantedTime:yyyy-MM-dd HH:mm:ss}");
                }
                else
                {
                    // If already harvested, set to PlantedTime (best guess)
                    LastHarvestTime = PlantedTime;
                    UnityEngine.Debug.LogWarning($"[Plant.InitializeConfig] {CropType} - Fixed LastHarvestTime=DateTime.MinValue → PlantedTime={PlantedTime:yyyy-MM-dd HH:mm:ss} (HarvestCount={HarvestCount})");
                }
            }
        }

        /// <summary>
        /// Calculate how many harvests are ready based on current time
        /// Returns number of ready harvests that haven't been collected yet
        /// </summary>
        public int GetReadyHarvestCount(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive)
            {
                UnityEngine.Debug.Log($"[Plant.GetReadyHarvestCount] {CropType} - NOT ALIVE");
                return 0;
            }
            if (HarvestCount >= LifespanYields)
            {
                UnityEngine.Debug.Log($"[Plant.GetReadyHarvestCount] {CropType} - All harvests collected (HarvestCount={HarvestCount}/{LifespanYields})");
                return 0; // Already harvested all
            }

            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            if (adjustedGrowthTime <= 0f)
            {
                UnityEngine.Debug.LogWarning($"[Plant.GetReadyHarvestCount] {CropType} - Invalid GrowthTime: {GrowthTimeMinutes}, equipmentBonus: {equipmentBonus}");
                return 0;
            }

            var timeSinceLastHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            
            // Calculate how many growth cycles completed
            var cyclesCompleted = (int)(timeSinceLastHarvest / adjustedGrowthTime);
            
            // Ready harvests = cycles completed - already harvested
            var readyHarvests = Math.Min(cyclesCompleted, LifespanYields) - HarvestCount;
            
            UnityEngine.Debug.Log($"[Plant.GetReadyHarvestCount] {CropType} - LastHarvestTime={LastHarvestTime:HH:mm:ss}, CurrentTime={currentTime:HH:mm:ss}, TimeSince={timeSinceLastHarvest:F2}min, AdjustedGrowth={adjustedGrowthTime:F2}min, CyclesCompleted={cyclesCompleted}, HarvestCount={HarvestCount}/{LifespanYields}, ReadyHarvests={readyHarvests}");
            
            return Math.Max(0, readyHarvests);
        }

        /// <summary>
        /// Get time in minutes until next harvest is ready
        /// </summary>
        public float GetTimeUntilNextHarvest(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive) return 0f;
            if (HarvestCount >= LifespanYields) return 0f; // All harvested

            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            if (adjustedGrowthTime <= 0f) return 0f;

            var timeSinceLastHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            
            // Calculate how many growth cycles completed
            var cyclesCompleted = (int)(timeSinceLastHarvest / adjustedGrowthTime);
            var readyHarvests = Math.Min(cyclesCompleted, LifespanYields) - HarvestCount;
            
            // If there are ready harvests, return 0
            if (readyHarvests > 0)
                return 0f;
            
            // Time remaining until next cycle completes
            var timeToNextCycle = adjustedGrowthTime - (timeSinceLastHarvest % adjustedGrowthTime);
            return (float)timeToNextCycle;
        }

        /// <summary>
        /// Check if plant has spoiled (unharvested products expired)
        /// Products spoil if not harvested within SpoilageTimeMinutes after becoming ready
        /// </summary>
        public bool HasSpoiled(DateTime currentTime, float spoilageTimeMinutes, float equipmentBonus = 0)
        {
            if (!IsAlive) return true;
            if (HarvestCount >= LifespanYields) return false; // All harvested, can't spoil
            
            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            if (adjustedGrowthTime <= 0f) return false;
            
            var timeSinceLastHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            
            // Calculate how many growth cycles completed
            var cyclesCompleted = (int)(timeSinceLastHarvest / adjustedGrowthTime);
            var readyHarvests = Math.Min(cyclesCompleted, LifespanYields) - HarvestCount;
            
            // Check if there are ready harvests
            if (readyHarvests <= 0)
                return false; // Nothing ready yet, can't spoil
            
            // Calculate time since first ready harvest completed
            var timeOfFirstReadyHarvest = LastHarvestTime.AddMinutes((HarvestCount + 1) * adjustedGrowthTime);
            var timeSinceFirstReady = (currentTime - timeOfFirstReadyHarvest).TotalMinutes;
            
            // Has spoiled if waited longer than spoilage time after first ready harvest
            return timeSinceFirstReady > spoilageTimeMinutes;
        }

        /// <summary>
        /// Get time remaining in minutes before unharvested products spoil
        /// Returns -1 if no ready harvests yet or already harvested all
        /// </summary>
        public float GetTimeUntilSpoilage(DateTime currentTime, float spoilageTimeMinutes, float equipmentBonus = 0)
        {
            if (!IsAlive) return -1;
            if (HarvestCount >= LifespanYields) return -1; // All harvested
            
            var adjustedGrowthTime = GrowthTimeMinutes / (1 + equipmentBonus);
            if (adjustedGrowthTime <= 0f) return -1;
            
            var timeSinceLastHarvest = (currentTime - LastHarvestTime).TotalMinutes;
            
            // Calculate how many growth cycles completed
            var cyclesCompleted = (int)(timeSinceLastHarvest / adjustedGrowthTime);
            var readyHarvests = Math.Min(cyclesCompleted, LifespanYields) - HarvestCount;
            
            // Not ready yet
            if (readyHarvests <= 0)
                return -1;
            
            // Calculate time since first ready harvest completed
            var timeOfFirstReadyHarvest = LastHarvestTime.AddMinutes((HarvestCount + 1) * adjustedGrowthTime);
            var timeSinceFirstReady = (currentTime - timeOfFirstReadyHarvest).TotalMinutes;
            var timeRemaining = spoilageTimeMinutes - timeSinceFirstReady;
            
            return (float)Math.Max(0, timeRemaining);
        }

        /// <summary>
        /// Perform harvest and return the amount harvested
        /// Harvests all ready yields at once
        /// </summary>
        public int Harvest(DateTime currentTime, float equipmentBonus = 0)
        {
            var readyCount = GetReadyHarvestCount(currentTime, equipmentBonus);
            if (readyCount <= 0) return 0;

            HarvestCount += readyCount;
            LastHarvestTime = currentTime;
            
            var yieldAmount = readyCount * YieldPerHarvest;
            TotalYield += yieldAmount;

            UnityEngine.Debug.Log($"[Plant.Harvest] {CropType} - Harvested {readyCount} yields ({yieldAmount} units). HarvestCount now: {HarvestCount}/{LifespanYields}");

            // Plant dies after harvesting all yields
            if (HarvestCount >= LifespanYields)
            {
                IsAlive = false;
                UnityEngine.Debug.Log($"[Plant.Harvest] {CropType} - Plant died (reached lifespan)");
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
