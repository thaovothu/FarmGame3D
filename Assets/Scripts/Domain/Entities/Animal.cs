using System;

namespace FarmGame.Domain.Entities
{
    public enum AnimalType
    {
        DairyCow
    }

    [Serializable]
    public class Animal
    {
        // Serializable fields for JsonUtility
        public string Id;
        public AnimalType AnimalType;
        public long AcquiredTimeTicks;
        public long LastProductionTimeTicks;
        public int ProductionCount;
        public bool IsAlive;
        public int TotalProduction;
        
        // Config data (loaded from CSV)
        public float ProductionTimeMinutes;
        public int YieldPerProduction;
        public int LifespanProductions;
        public int ProductPrice;

        // Properties for easy DateTime access
        public DateTime AcquiredTime
        {
            get => new DateTime(AcquiredTimeTicks);
            set => AcquiredTimeTicks = value.Ticks;
        }

        public DateTime LastProductionTime
        {
            get => new DateTime(LastProductionTimeTicks);
            set => LastProductionTimeTicks = value.Ticks;
        }
        
        public Animal()
        {
            Id = Guid.NewGuid().ToString();
            IsAlive = true;
            ProductionCount = 0;
            TotalProduction = 0;
        }

        public Animal(AnimalType animalType, DateTime acquiredTime, float productionTime, int yieldPerProduction, int lifespanProductions, int productPrice)
        {
            Id = Guid.NewGuid().ToString();
            AnimalType = animalType;
            AcquiredTime = acquiredTime;
            LastProductionTime = acquiredTime;
            IsAlive = true;
            ProductionCount = 0;
            TotalProduction = 0;
            ProductionTimeMinutes = productionTime;
            YieldPerProduction = yieldPerProduction;
            LifespanProductions = lifespanProductions;
            ProductPrice = productPrice;
        }

        /// <summary>
        /// Re-initialize config from GameConfig after JsonUtility deserialization
        /// JsonUtility doesn't call constructors, so config fields may be 0
        /// </summary>
        public void InitializeConfig(GameConfig gameConfig)
        {
            if (gameConfig == null) return;
            
            var animalConfig = gameConfig.GetAnimalConfig(AnimalType.ToString());
            if (animalConfig != null)
            {
                ProductionTimeMinutes = animalConfig.ProductionTimeMinutes;
                YieldPerProduction = animalConfig.YieldPerProduction;
                LifespanProductions = animalConfig.LifespanProductions;
                ProductPrice = animalConfig.ProductPrice;
                UnityEngine.Debug.Log($"[Animal.InitializeConfig] {AnimalType} → ProductionTime={ProductionTimeMinutes}m, Yield={YieldPerProduction}, Lifespan={LifespanProductions}");
            }
            else
            {
                UnityEngine.Debug.LogError($"[Animal.InitializeConfig] No config found for {AnimalType}!");
            }
            
            // Fix LastProductionTime if it's DateTime.MinValue (Ticks = 0 from JSON)
            if (LastProductionTimeTicks == 0 || LastProductionTime == DateTime.MinValue)
            {
                // Set LastProductionTime to AcquiredTime if not yet collected
                if (ProductionCount == 0)
                {
                    LastProductionTime = AcquiredTime;
                    UnityEngine.Debug.LogWarning($"[Animal.InitializeConfig] {AnimalType} - Fixed LastProductionTime=DateTime.MinValue → AcquiredTime={AcquiredTime:yyyy-MM-dd HH:mm:ss}");
                }
                else
                {
                    // If already collected, set to AcquiredTime (best guess)
                    LastProductionTime = AcquiredTime;
                    UnityEngine.Debug.LogWarning($"[Animal.InitializeConfig] {AnimalType} - Fixed LastProductionTime=DateTime.MinValue → AcquiredTime={AcquiredTime:yyyy-MM-dd HH:mm:ss} (ProductionCount={ProductionCount})");
                }
            }
        }

        /// <summary>
        /// Calculate how many productions are ready based on current time
        /// Returns number of ready productions that haven't been collected yet
        /// </summary>
        public int GetReadyProductionCount(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive)
            {
                UnityEngine.Debug.Log($"[Animal.GetReadyProductionCount] {AnimalType} - NOT ALIVE");
                return 0;
            }
            if (ProductionCount >= LifespanProductions)
            {
                UnityEngine.Debug.Log($"[Animal.GetReadyProductionCount] {AnimalType} - All collected (ProductionCount={ProductionCount}/{LifespanProductions})");
                return 0; // All collected
            }

            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            if (adjustedProductionTime <= 0f)
            {
                UnityEngine.Debug.LogWarning($"[Animal.GetReadyProductionCount] {AnimalType} - Invalid ProductionTime: {ProductionTimeMinutes}, equipmentBonus: {equipmentBonus}");
                return 0;
            }

            var timeSinceLastProduction = (currentTime - LastProductionTime).TotalMinutes;
            
            // Calculate how many production cycles completed
            var cyclesCompleted = (int)(timeSinceLastProduction / adjustedProductionTime);
            
            // Ready productions = cycles completed - already collected
            var readyProductions = Math.Min(cyclesCompleted, LifespanProductions) - ProductionCount;
            
            UnityEngine.Debug.Log($"[Animal.GetReadyProductionCount] {AnimalType} - LastProductionTime={LastProductionTime:HH:mm:ss}, CurrentTime={currentTime:HH:mm:ss}, TimeSince={timeSinceLastProduction:F2}min, AdjustedProduction={adjustedProductionTime:F2}min, CyclesCompleted={cyclesCompleted}, ProductionCount={ProductionCount}/{LifespanProductions}, ReadyProductions={readyProductions}");
            
            return Math.Max(0, readyProductions);
        }

        /// <summary>
        /// Get time in minutes until next production is ready
        /// </summary>
        public float GetTimeUntilNextProduction(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive) return 0f;
            if (ProductionCount >= LifespanProductions) return 0f; // All collected

            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            if (adjustedProductionTime <= 0f) return 0f;

            var timeSinceLastProduction = (currentTime - LastProductionTime).TotalMinutes;
            
            // Calculate how many production cycles completed
            var cyclesCompleted = (int)(timeSinceLastProduction / adjustedProductionTime);
            var readyProductions = Math.Min(cyclesCompleted, LifespanProductions) - ProductionCount;
            
            // If there are ready productions, return 0
            if (readyProductions > 0)
                return 0f;
            
            // Time remaining until next cycle completes
            var timeToNextCycle = adjustedProductionTime - (timeSinceLastProduction % adjustedProductionTime);
            return (float)timeToNextCycle;
        }

        /// <summary>
        /// Check if animal has spoiled (uncollected products expired)
        /// Products spoil if not collected within SpoilageTimeMinutes after becoming ready
        /// </summary>
        public bool HasSpoiled(DateTime currentTime, float spoilageTimeMinutes, float equipmentBonus = 0)
        {
            if (!IsAlive) return true;
            if (ProductionCount >= LifespanProductions) return false; // All collected, can't spoil
            
            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            if (adjustedProductionTime <= 0f) return false;
            
            var timeSinceLastProduction = (currentTime - LastProductionTime).TotalMinutes;
            
            // Calculate how many production cycles completed
            var cyclesCompleted = (int)(timeSinceLastProduction / adjustedProductionTime);
            var readyProductions = Math.Min(cyclesCompleted, LifespanProductions) - ProductionCount;
            
            // Check if there are ready productions
            if (readyProductions <= 0)
                return false; // Nothing ready yet, can't spoil
            
            // Calculate time since first ready production completed
            var timeOfFirstReadyProduction = LastProductionTime.AddMinutes((ProductionCount + 1) * adjustedProductionTime);
            var timeSinceFirstReady = (currentTime - timeOfFirstReadyProduction).TotalMinutes;
            
            // Has spoiled if waited longer than spoilage time after first ready production
            return timeSinceFirstReady > spoilageTimeMinutes;
        }

        /// <summary>
        /// Get time remaining in minutes before uncollected products spoil
        /// Returns -1 if no ready productions yet or already collected all
        /// </summary>
        public float GetTimeUntilSpoilage(DateTime currentTime, float spoilageTimeMinutes, float equipmentBonus = 0)
        {
            if (!IsAlive) return -1;
            if (ProductionCount >= LifespanProductions) return -1; // All collected
            
            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            if (adjustedProductionTime <= 0f) return -1;
            
            var timeSinceLastProduction = (currentTime - LastProductionTime).TotalMinutes;
            
            // Calculate how many production cycles completed
            var cyclesCompleted = (int)(timeSinceLastProduction / adjustedProductionTime);
            var readyProductions = Math.Min(cyclesCompleted, LifespanProductions) - ProductionCount;
            
            // Not ready yet
            if (readyProductions <= 0)
                return -1;
            
            // Calculate time since first ready production completed
            var timeOfFirstReadyProduction = LastProductionTime.AddMinutes((ProductionCount + 1) * adjustedProductionTime);
            var timeSinceFirstReady = (currentTime - timeOfFirstReadyProduction).TotalMinutes;
            var timeRemaining = spoilageTimeMinutes - timeSinceFirstReady;
            
            return (float)Math.Max(0, timeRemaining);
        }

        /// <summary>
        /// Collect production and return the amount collected
        /// Collects all ready productions at once
        /// </summary>
        public int Collect(DateTime currentTime, float equipmentBonus = 0)
        {
            var readyCount = GetReadyProductionCount(currentTime, equipmentBonus);
            if (readyCount <= 0) return 0;

            ProductionCount += readyCount;
            LastProductionTime = currentTime;
            
            var productionAmount = readyCount * YieldPerProduction;
            TotalProduction += productionAmount;

            UnityEngine.Debug.Log($"[Animal.Collect] {AnimalType} - Collected {readyCount} productions ({productionAmount} units). ProductionCount now: {ProductionCount}/{LifespanProductions}");

            // Animal dies after collecting all productions
            if (ProductionCount >= LifespanProductions)
            {
                IsAlive = false;
                UnityEngine.Debug.Log($"[Animal.Collect] {AnimalType} - Animal died (reached lifespan)");
            }

            return productionAmount;
        }
    }
}
