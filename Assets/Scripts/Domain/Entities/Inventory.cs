                                                                                                            using System;
using System.Collections.Generic;

namespace FarmGame.Domain.Entities
{
    [Serializable]
    public class Inventory
    {
        // Serializable fields for JsonUtility
        public int Gold;
        public Dictionary<CropType, int> Seeds;
        public Dictionary<CropType, int> Harvested;
        public int Milk;
        public int EquipmentLevel;
        public int DairyCowCount; // Số bò sữa trong kho

        public Inventory()
        {
            Gold = 0;
            Seeds = new Dictionary<CropType, int>();
            Harvested = new Dictionary<CropType, int>();
            Milk = 0;
            EquipmentLevel = 1;
            DairyCowCount = 0;
        }

        public void Initialize(int initialGold, int equipmentLevel)
        {
            Gold = initialGold;
            EquipmentLevel = equipmentLevel;
            
            // Initialize all crop types
            foreach (CropType cropType in Enum.GetValues(typeof(CropType)))
            {
                if (!Seeds.ContainsKey(cropType))
                    Seeds[cropType] = 0;
                if (!Harvested.ContainsKey(cropType))
                    Harvested[cropType] = 0;
            }
        }

        public bool HasSeed(CropType cropType)
        {
            return Seeds.ContainsKey(cropType) && Seeds[cropType] > 0;
        }

        public bool UseSeed(CropType cropType)
        {
            if (!HasSeed(cropType)) return false;
            Seeds[cropType]--;
            return true;
        }

        public void AddSeeds(CropType cropType, int amount)
        {
            if (!Seeds.ContainsKey(cropType))
                Seeds[cropType] = 0;
            Seeds[cropType] += amount;
        }

        public void AddHarvest(CropType cropType, int amount)
        {
            if (!Harvested.ContainsKey(cropType))
                Harvested[cropType] = 0;
            Harvested[cropType] += amount;
        }

        public void AddMilk(int amount)
        {
            Milk += amount;
        }

        public void AddGold(int amount)
        {
            Gold += amount;
        }

        public bool SpendGold(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            return true;
        }

        public bool CanAfford(int cost)
        {
            return Gold >= cost;
        }

        public float GetEquipmentBonus()
        {
            // Each level adds 10% bonus
            return (EquipmentLevel - 1) * 0.1f;
        }

        public void UpgradeEquipment()
        {
            EquipmentLevel++;
        }

        public int GetSeedCount(CropType cropType)
        {
            return Seeds.ContainsKey(cropType) ? Seeds[cropType] : 0;
        }

        public int GetHarvestedCount(CropType cropType)
        {
            return Harvested.ContainsKey(cropType) ? Harvested[cropType] : 0;
        }

        public bool SellHarvest(CropType cropType, int amount, int pricePerUnit)
        {
            if (!Harvested.ContainsKey(cropType) || Harvested[cropType] < amount)
                return false;

            Harvested[cropType] -= amount;
            Gold += amount * pricePerUnit;
            return true;
        }

        public bool SellMilk(int amount, int pricePerUnit)
        {
            if (Milk < amount) return false;
            
            Milk -= amount;
            Gold += amount * pricePerUnit;
            return true;
        }
    }
}
