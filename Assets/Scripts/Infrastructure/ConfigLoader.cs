using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FarmGame.Domain;
using FarmGame.Domain.Entities;

namespace FarmGame.Infrastructure
{
    public class ConfigLoader
    {
        public static GameConfig LoadConfig(string filePath)
        {
            var config = new GameConfig();
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Config file not found: {filePath}");
            }

            var lines = File.ReadAllLines(filePath);
            string currentSection = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // Check for section headers
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    continue;
                }

                // Parse data based on current section
                if (currentSection != null)
                {
                    ParseConfigLine(config, currentSection, trimmedLine);
                }
            }

            return config;
        }

        private static void ParseConfigLine(GameConfig config, string section, string line)
        {
            var parts = line.Split(',');

            switch (section)
            {
                case "Crops":
                    if (parts.Length >= 6 && parts[0] != "Type")
                    {
                        var cropConfig = new CropConfig
                        {
                            GrowthTimeMinutes = float.Parse(parts[1]),
                            YieldPerHarvest = int.Parse(parts[2]),
                            LifespanYields = int.Parse(parts[3]),
                            SellPrice = int.Parse(parts[4]),
                            SeedPrice = int.Parse(parts[5])
                        };
                        config.Crops[parts[0]] = cropConfig;
                        UnityEngine.Debug.Log($"[ConfigLoader] Loaded crop: {parts[0]} → GrowthTime={cropConfig.GrowthTimeMinutes}m, Yield={cropConfig.YieldPerHarvest}, Lifespan={cropConfig.LifespanYields}");
                    }
                    break;

                case "Animals":
                    if (parts.Length >= 6 && parts[0] != "Type")
                    {
                        var animalConfig = new AnimalConfig
                        {
                            ProductionTimeMinutes = float.Parse(parts[1]),
                            YieldPerProduction = int.Parse(parts[2]),
                            LifespanProductions = int.Parse(parts[3]),
                            ProductPrice = int.Parse(parts[4]),
                            AnimalPrice = int.Parse(parts[5])
                        };
                        config.Animals[parts[0]] = animalConfig;
                        UnityEngine.Debug.Log($"[ConfigLoader] Loaded animal: {parts[0]} → ProductionTime={animalConfig.ProductionTimeMinutes}m, Yield={animalConfig.YieldPerProduction}, Lifespan={animalConfig.LifespanProductions}");
                    }
                    break;

                case "Workers":
                    if (parts.Length >= 2 && parts[0] != "ActionTimeMinutes")
                    {
                        config.WorkerActionTimeMinutes = float.Parse(parts[0]);
                        config.WorkerHireCost = int.Parse(parts[1]);
                    }
                    break;

                case "Equipment":
                    if (parts.Length >= 2 && parts[0] != "UpgradeCost")
                    {
                        config.EquipmentUpgradeCost = int.Parse(parts[0]);
                        config.EquipmentYieldBonusPercent = float.Parse(parts[1]);
                    }
                    break;

                case "Plots":
                    if (parts.Length >= 1 && parts[0] != "BuyPlotCost")
                    {
                        config.PlotBuyCost = int.Parse(parts[0]);
                    }
                    break;

                case "InitialResources":
                    if (parts.Length >= 8 && parts[0] != "Gold")
                    {
                        config.InitialGold = int.Parse(parts[0]);
                        config.InitialPlots = int.Parse(parts[1]);
                        config.InitialTomatoSeeds = int.Parse(parts[2]);
                        config.InitialBlueberrySeeds = int.Parse(parts[3]);
                        config.InitialStrawberrySeeds = int.Parse(parts[4]);
                        config.InitialDairyCows = int.Parse(parts[5]);
                        config.InitialWorkers = int.Parse(parts[6]);
                        config.InitialEquipmentLevel = int.Parse(parts[7]);
                    }
                    break;

                case "Shop":
                    if (parts.Length >= 2 && parts[0] != "StrawberryBulkSize")
                    {
                        config.StrawberryBulkSize = int.Parse(parts[0]);
                        config.StrawberryBulkPrice = int.Parse(parts[1]);
                    }
                    break;

                case "WinCondition":
                    if (parts.Length >= 1 && parts[0] != "GoldTarget")
                    {
                        config.GoldTarget = int.Parse(parts[0]);
                    }
                    break;

                case "Spoilage":
                    if (parts.Length >= 1 && parts[0] != "SpoilageTimeMinutes")
                    {
                        config.SpoilageTimeMinutes = float.Parse(parts[0]);
                    }
                    break;
            }
        }
    }
}
