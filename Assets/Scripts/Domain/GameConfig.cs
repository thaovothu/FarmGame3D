using System;
using System.Collections.Generic;

namespace FarmGame.Domain
{
    [Serializable]
    public class CropConfig
    {
        public float GrowthTimeMinutes { get; set; }
        public int YieldPerHarvest { get; set; }
        public int LifespanYields { get; set; }
        public int SellPrice { get; set; }
        public int SeedPrice { get; set; }
    }

    [Serializable]
    public class AnimalConfig
    {
        public float ProductionTimeMinutes { get; set; }
        public int YieldPerProduction { get; set; }
        public int LifespanProductions { get; set; }
        public int ProductPrice { get; set; }
        public int AnimalPrice { get; set; }
    }

    [Serializable]
    public class GameConfig
    {
        public Dictionary<string, CropConfig> Crops { get; set; }
        public Dictionary<string, AnimalConfig> Animals { get; set; }
        
        public float WorkerActionTimeMinutes { get; set; }
        public int WorkerHireCost { get; set; }
        
        public int EquipmentUpgradeCost { get; set; }
        public float EquipmentYieldBonusPercent { get; set; }
        
        public int PlotBuyCost { get; set; }
        
        public int InitialGold { get; set; }
        public int InitialPlots { get; set; }
        public int InitialTomatoSeeds { get; set; }
        public int InitialBlueberrySeeds { get; set; }
        public int InitialStrawberrySeeds { get; set; }
        public int InitialDairyCows { get; set; }
        public int InitialWorkers { get; set; }
        public int InitialEquipmentLevel { get; set; }
        
        public int StrawberryBulkSize { get; set; }
        public int StrawberryBulkPrice { get; set; }
        
        public int GoldTarget { get; set; }
        public float SpoilageTimeMinutes { get; set; }

        public GameConfig()
        {
            Crops = new Dictionary<string, CropConfig>();
            Animals = new Dictionary<string, AnimalConfig>();
        }

        public CropConfig GetCropConfig(string cropType)
        {
            return Crops.ContainsKey(cropType) ? Crops[cropType] : null;
        }

        public AnimalConfig GetAnimalConfig(string animalType)
        {
            return Animals.ContainsKey(animalType) ? Animals[animalType] : null;
        }
    }
}
