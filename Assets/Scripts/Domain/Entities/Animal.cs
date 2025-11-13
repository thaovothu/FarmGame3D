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
        /// </summary>
        public int GetReadyProductionCount(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive) return 0;

            var timeSinceLastProduction = (currentTime - LastProductionTime).TotalMinutes;
            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            var readyProductions = (int)(timeSinceLastProduction / adjustedProductionTime);
            
            // Can't produce more than remaining lifespan
            var remainingProductions = LifespanProductions - ProductionCount;
            return Math.Min(readyProductions, remainingProductions);
        }

        /// <summary>
        /// Get time in minutes until next production
        /// </summary>
        public float GetTimeUntilNextProduction(DateTime currentTime, float equipmentBonus = 0)
        {
            if (!IsAlive) return 0;
            if (ProductionCount >= LifespanProductions) return 0;

            var timeSinceLastProduction = (currentTime - LastProductionTime).TotalMinutes;
            var adjustedProductionTime = ProductionTimeMinutes / (1 + equipmentBonus);
            var timeUntilNext = adjustedProductionTime - (timeSinceLastProduction % adjustedProductionTime);
            
            return (float)timeUntilNext;
        }

        /// <summary>
        /// Check if animal has died (not collected in time after final production)
        /// </summary>
        public bool HasSpoiled(DateTime currentTime, float spoilageTimeMinutes)
        {
            if (!IsAlive) return true;
            if (ProductionCount < LifespanProductions) return false;

            // After final production, player has spoilageTimeMinutes to collect
            var timeSinceLastPossibleProduction = (currentTime - LastProductionTime).TotalMinutes;
            return timeSinceLastPossibleProduction > spoilageTimeMinutes;
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
