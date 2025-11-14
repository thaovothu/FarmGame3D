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
        /// Calculate how many productions are ready based on current time
        /// Only returns productions when ALL lifespan productions are complete (can only collect once at end)
        /// </summary>
        public int GetReadyProductionCount(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive) return 0;
            if (ProductionCount > 0) return 0; // Already collected

            var timeSinceLastProduction = (currentTime - LastProductionTime).TotalMinutes;
            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            var totalProductionTime = adjustedProductionTime * LifespanProductions;
            
            // Only ready when ALL productions are complete
            if (timeSinceLastProduction >= totalProductionTime)
            {
                return LifespanProductions; // Return all productions at once
            }
            
            return 0;
        }

        /// <summary>
        /// Get time in minutes until ALL productions complete (ready to collect)
        /// </summary>
        public float GetTimeUntilNextProduction(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive) return 0;
            if (ProductionCount > 0) return 0; // Already collected

            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            if (adjustedProductionTime <= 0f) return 0;
            
            var timeSinceLastProduction = (currentTime - LastProductionTime).TotalMinutes;
            var totalProductionTime = adjustedProductionTime * LifespanProductions;
            
            // If all productions complete, return 0
            if (timeSinceLastProduction >= totalProductionTime)
                return 0;
            
            // Time remaining until all productions complete
            var timeRemaining = totalProductionTime - timeSinceLastProduction;
            return (float)timeRemaining;
        }

        /// <summary>
        /// Check if animal has spoiled (uncollected products expired)
        /// Products spoil if not collected within SpoilageTimeMinutes after ALL productions complete
        /// </summary>
        public bool HasSpoiled(DateTime currentTime, float spoilageTimeMinutes, float equipmentBonus = 0)
        {
            if (!IsAlive) return true;
            if (ProductionCount > 0) return false; // Already collected, can't spoil
            
            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            if (adjustedProductionTime <= 0f) return false;
            
            var timeSinceLastProduction = (currentTime - LastProductionTime).TotalMinutes;
            var totalProductionTime = adjustedProductionTime * LifespanProductions;
            
            // Check if all productions are complete
            if (timeSinceLastProduction < totalProductionTime)
                return false; // Still producing, can't spoil
            
            // Calculate time since all productions completed
            var timeSinceAllComplete = timeSinceLastProduction - totalProductionTime;
            
            // Has spoiled if waited longer than spoilage time after completion
            return timeSinceAllComplete > spoilageTimeMinutes;
        }

        /// <summary>
        /// Get time remaining in minutes before uncollected products spoil
        /// Returns -1 if not all productions complete yet or already collected
        /// </summary>
        public float GetTimeUntilSpoilage(DateTime currentTime, float spoilageTimeMinutes, float equipmentBonus = 0)
        {
            if (!IsAlive) return -1;
            if (ProductionCount > 0) return -1; // Already collected
            
            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            if (adjustedProductionTime <= 0f) return -1;
            
            var timeSinceLastProduction = (currentTime - LastProductionTime).TotalMinutes;
            var totalProductionTime = adjustedProductionTime * LifespanProductions;
            
            // Not ready yet
            if (timeSinceLastProduction < totalProductionTime)
                return -1;
            
            // Calculate time since all productions completed
            var timeSinceAllComplete = timeSinceLastProduction - totalProductionTime;
            var timeRemaining = spoilageTimeMinutes - timeSinceAllComplete;
            
            return (float)Math.Max(0, timeRemaining);
        }

        /// <summary>
        /// Collect production and return the amount collected
        /// </summary>
        public int Collect(DateTime currentTime, float equipmentBonus = 0)
        {
            var readyCount = GetReadyProductionCount(currentTime, equipmentBonus);
            if (readyCount <= 0) return 0;

            ProductionCount += readyCount;
            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            LastProductionTime = LastProductionTime.AddMinutes(readyCount * adjustedProductionTime);
            
            var productionAmount = readyCount * YieldPerProduction;
            TotalProduction += productionAmount;

            // Check if animal is dead after this production
            if (ProductionCount >= LifespanProductions)
            {
                IsAlive = false;
            }

            return productionAmount;
        }
    }
}
