                                                                                                            using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmGame.Domain.Entities
{
    // Wrapper cho serialization (JsonUtility không support Dictionary)
    [Serializable]
    public class SeedEntry
    {
        public CropType CropType;
        public int Count;
    }

    [Serializable]
    public class HarvestEntry
    {
        public CropType CropType;
        public int Count;
    }

    [Serializable]
    public class Inventory
    {
        // Serializable fields for JsonUtility (arrays thay vì Dictionary)
        public int Gold;
        public SeedEntry[] SeedEntries;
        public HarvestEntry[] HarvestEntries;
        public int Milk;
        public int EquipmentLevel;
        public int DairyCowCount; // Số bò sữa trong kho

        // Runtime dictionaries (không serialize)
        [NonSerialized] private Dictionary<CropType, int> _seeds;
        [NonSerialized] private Dictionary<CropType, int> _harvested;

        public Inventory()
        {
            Gold = 0;
            Milk = 0;
            EquipmentLevel = 1;
            DairyCowCount = 0;
            _seeds = new Dictionary<CropType, int>();
            _harvested = new Dictionary<CropType, int>();
            SyncToArrays();
        }

        // Gọi sau khi deserialize để rebuild dictionaries
        public void RebuildDictionaries()
        {
            _seeds = new Dictionary<CropType, int>();
            _harvested = new Dictionary<CropType, int>();

            if (SeedEntries != null)
            {
                foreach (var entry in SeedEntries)
                    _seeds[entry.CropType] = entry.Count;
            }

            if (HarvestEntries != null)
            {
                foreach (var entry in HarvestEntries)
                    _harvested[entry.CropType] = entry.Count;
            }
        }

        // Gọi trước khi serialize để sync dictionaries → arrays
        public void SyncToArrays()
        {
            if (_seeds != null)
                SeedEntries = _seeds.Select(kv => new SeedEntry { CropType = kv.Key, Count = kv.Value }).ToArray();
            else
                SeedEntries = new SeedEntry[0];

            if (_harvested != null)
                HarvestEntries = _harvested.Select(kv => new HarvestEntry { CropType = kv.Key, Count = kv.Value }).ToArray();
            else
                HarvestEntries = new HarvestEntry[0];
        }

        public void Initialize(int initialGold, int equipmentLevel)
        {
            Gold = initialGold;
            EquipmentLevel = equipmentLevel;
            
            if (_seeds == null) _seeds = new Dictionary<CropType, int>();
            if (_harvested == null) _harvested = new Dictionary<CropType, int>();
            
            // Initialize all crop types
            foreach (CropType cropType in Enum.GetValues(typeof(CropType)))
            {
                if (!_seeds.ContainsKey(cropType))
                    _seeds[cropType] = 0;
                if (!_harvested.ContainsKey(cropType))
                    _harvested[cropType] = 0;
            }
            
            SyncToArrays();
        }

        public bool HasSeed(CropType cropType)
        {
            if (_seeds == null) RebuildDictionaries();
            return _seeds.ContainsKey(cropType) && _seeds[cropType] > 0;
        }

        public bool UseSeed(CropType cropType)
        {
            if (!HasSeed(cropType)) return false;
            _seeds[cropType]--;
            SyncToArrays();
            return true;
        }

        public void AddSeeds(CropType cropType, int amount)
        {
            if (_seeds == null) RebuildDictionaries();
            if (!_seeds.ContainsKey(cropType))
                _seeds[cropType] = 0;
            _seeds[cropType] += amount;
            SyncToArrays();
        }

        public void AddHarvest(CropType cropType, int amount)
        {
            if (_harvested == null) RebuildDictionaries();
            if (!_harvested.ContainsKey(cropType))
                _harvested[cropType] = 0;
            _harvested[cropType] += amount;
            SyncToArrays();
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
            if (_seeds == null) RebuildDictionaries();
            return _seeds.ContainsKey(cropType) ? _seeds[cropType] : 0;
        }

        public int GetHarvestedCount(CropType cropType)
        {
            if (_harvested == null) RebuildDictionaries();
            return _harvested.ContainsKey(cropType) ? _harvested[cropType] : 0;
        }

        public bool SellHarvest(CropType cropType, int amount, int pricePerUnit)
        {
            if (_harvested == null) RebuildDictionaries();
            if (!_harvested.ContainsKey(cropType) || _harvested[cropType] < amount)
                return false;

            _harvested[cropType] -= amount;
            Gold += amount * pricePerUnit;
            SyncToArrays();
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
