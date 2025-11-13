using System;
using FarmGame.Domain.Entities;

namespace FarmGame.Domain.Services
{
    public class ShopService
    {
        private readonly GameConfig _config;

        public ShopService(GameConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Buy tomato seeds
        /// </summary>
        public bool BuyTomatoSeeds(Farm farm, int quantity)
        {
            var cropConfig = _config.GetCropConfig("Tomato");
            if (cropConfig == null) return false;

            var totalCost = cropConfig.SeedPrice * quantity;
            if (!farm.Inventory.CanAfford(totalCost))
                return false;

            if (farm.Inventory.SpendGold(totalCost))
            {
                farm.Inventory.AddSeeds(CropType.Tomato, quantity);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Buy blueberry seeds
        /// </summary>
        public bool BuyBlueberrySeeds(Farm farm, int quantity)
        {
            var cropConfig = _config.GetCropConfig("Blueberry");
            if (cropConfig == null) return false;

            var totalCost = cropConfig.SeedPrice * quantity;
            if (!farm.Inventory.CanAfford(totalCost))
                return false;

            if (farm.Inventory.SpendGold(totalCost))
            {
                farm.Inventory.AddSeeds(CropType.Blueberry, quantity);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Buy strawberry seeds (bulk only)
        /// </summary>
        public bool BuyStrawberrySeeds(Farm farm)
        {
            // Strawberry is sold in bulk only
            var totalCost = _config.StrawberryBulkPrice;
            if (!farm.Inventory.CanAfford(totalCost))
                return false;

            if (farm.Inventory.SpendGold(totalCost))
            {
                farm.Inventory.AddSeeds(CropType.Strawberry, _config.StrawberryBulkSize);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Buy a dairy cow
        /// </summary>
        public bool BuyDairyCow(Farm farm, DateTime currentTime)
        {
            var animalConfig = _config.GetAnimalConfig("DairyCow");
            if (animalConfig == null) return false;

            if (!farm.Inventory.CanAfford(animalConfig.AnimalPrice))
                return false;

            // Find an empty plot
            var emptyPlot = farm.Plots.Find(p => p.IsEmpty());
            if (emptyPlot == null)
                return false;

            if (farm.Inventory.SpendGold(animalConfig.AnimalPrice))
            {
                var cow = new Animal(
                    AnimalType.DairyCow,
                    currentTime,
                    animalConfig.ProductionTimeMinutes,
                    animalConfig.YieldPerProduction,
                    animalConfig.LifespanProductions,
                    animalConfig.ProductPrice
                );
                emptyPlot.PlaceAnimal(cow);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sell tomatoes
        /// </summary>
        public bool SellTomatoes(Farm farm, int quantity)
        {
            var cropConfig = _config.GetCropConfig("Tomato");
            if (cropConfig == null) return false;

            return farm.Inventory.SellHarvest(CropType.Tomato, quantity, cropConfig.SellPrice);
        }

        /// <summary>
        /// Sell blueberries
        /// </summary>
        public bool SellBlueberries(Farm farm, int quantity)
        {
            var cropConfig = _config.GetCropConfig("Blueberry");
            if (cropConfig == null) return false;

            return farm.Inventory.SellHarvest(CropType.Blueberry, quantity, cropConfig.SellPrice);
        }

        /// <summary>
        /// Sell strawberries
        /// </summary>
        public bool SellStrawberries(Farm farm, int quantity)
        {
            var cropConfig = _config.GetCropConfig("Strawberry");
            if (cropConfig == null) return false;

            return farm.Inventory.SellHarvest(CropType.Strawberry, quantity, cropConfig.SellPrice);
        }

        /// <summary>
        /// Sell milk
        /// </summary>
        public bool SellMilk(Farm farm, int quantity)
        {
            var animalConfig = _config.GetAnimalConfig("DairyCow");
            if (animalConfig == null) return false;

            return farm.Inventory.SellMilk(quantity, animalConfig.ProductPrice);
        }

        /// <summary>
        /// Sell all harvested crops
        /// </summary>
        public int SellAllHarvest(Farm farm)
        {
            int totalGold = 0;

            // Sell all tomatoes
            var tomatoCount = farm.Inventory.GetHarvestedCount(CropType.Tomato);
            if (tomatoCount > 0 && SellTomatoes(farm, tomatoCount))
            {
                var config = _config.GetCropConfig("Tomato");
                totalGold += tomatoCount * config.SellPrice;
            }

            // Sell all blueberries
            var blueberryCount = farm.Inventory.GetHarvestedCount(CropType.Blueberry);
            if (blueberryCount > 0 && SellBlueberries(farm, blueberryCount))
            {
                var config = _config.GetCropConfig("Blueberry");
                totalGold += blueberryCount * config.SellPrice;
            }

            // Sell all strawberries
            var strawberryCount = farm.Inventory.GetHarvestedCount(CropType.Strawberry);
            if (strawberryCount > 0 && SellStrawberries(farm, strawberryCount))
            {
                var config = _config.GetCropConfig("Strawberry");
                totalGold += strawberryCount * config.SellPrice;
            }

            // Sell all milk
            var milkCount = farm.Inventory.Milk;
            if (milkCount > 0 && SellMilk(farm, milkCount))
            {
                var config = _config.GetAnimalConfig("DairyCow");
                totalGold += milkCount * config.ProductPrice;
            }

            return totalGold;
        }
    }
}

