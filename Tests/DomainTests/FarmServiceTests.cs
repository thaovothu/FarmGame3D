using NUnit.Framework;
using System;
using FarmGame.Domain;
using FarmGame.Domain.Entities;
using FarmGame.Domain.Services;

namespace FarmGame.Tests.Domain
{
    [TestFixture]
    public class FarmServiceTests
    {
        private GameConfig _config;
        private FarmService _farmService;

        [SetUp]
        public void SetUp()
        {
            _config = CreateTestConfig();
            _farmService = new FarmService(_config);
        }

        private GameConfig CreateTestConfig()
        {
            var config = new GameConfig
            {
                InitialGold = 100,
                InitialPlots = 3,
                InitialTomatoSeeds = 10,
                InitialBlueberrySeeds = 10,
                InitialStrawberrySeeds = 0,
                InitialDairyCows = 2,
                InitialWorkers = 1,
                InitialEquipmentLevel = 1,
                WorkerActionTimeMinutes = 2,
                WorkerHireCost = 500,
                EquipmentUpgradeCost = 500,
                EquipmentYieldBonusPercent = 10,
                PlotBuyCost = 500,
                GoldTarget = 1000000,
                SpoilageTimeMinutes = 60
            };

            config.Crops["Tomato"] = new CropConfig
            {
                GrowthTimeMinutes = 10,
                YieldPerHarvest = 1,
                LifespanYields = 40,
                SellPrice = 5,
                SeedPrice = 30
            };

            config.Crops["Blueberry"] = new CropConfig
            {
                GrowthTimeMinutes = 15,
                YieldPerHarvest = 1,
                LifespanYields = 40,
                SellPrice = 8,
                SeedPrice = 50
            };

            config.Animals["DairyCow"] = new AnimalConfig
            {
                ProductionTimeMinutes = 30,
                YieldPerProduction = 1,
                LifespanProductions = 100,
                ProductPrice = 15,
                AnimalPrice = 100
            };

            return config;
        }

        [Test]
        public void InitializeNewFarm_CreatesValidFarm()
        {
            // Act
            var farm = _farmService.InitializeNewFarm();

            // Assert
            Assert.IsNotNull(farm);
            Assert.AreEqual(3, farm.Plots.Count);
            Assert.AreEqual(1, farm.Workers.Count);
            Assert.AreEqual(100, farm.Inventory.Gold);
            Assert.AreEqual(10, farm.Inventory.GetSeedCount(CropType.Tomato));
            Assert.AreEqual(10, farm.Inventory.GetSeedCount(CropType.Blueberry));
        }

        [Test]
        public void PlantCrop_WithValidPlot_Succeeds()
        {
            // Arrange
            var farm = _farmService.InitializeNewFarm();
            var emptyPlot = farm.Plots.Find(p => p.IsEmpty());

            // Act
            var success = _farmService.PlantCrop(farm, emptyPlot.Id, CropType.Tomato, DateTime.Now);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(PlotStatus.HasPlant, emptyPlot.Status);
            Assert.AreEqual(9, farm.Inventory.GetSeedCount(CropType.Tomato));
        }

        [Test]
        public void PlantCrop_WithoutSeeds_Fails()
        {
            // Arrange
            var farm = _farmService.InitializeNewFarm();
            farm.Inventory.Seeds[CropType.Tomato] = 0;
            var emptyPlot = farm.Plots.Find(p => p.IsEmpty());

            // Act
            var success = _farmService.PlantCrop(farm, emptyPlot.Id, CropType.Tomato, DateTime.Now);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(PlotStatus.Empty, emptyPlot.Status);
        }

        [Test]
        public void HarvestCrop_WhenReady_ReturnsYield()
        {
            // Arrange
            var farm = _farmService.InitializeNewFarm();
            var emptyPlot = farm.Plots.Find(p => p.IsEmpty());
            var plantTime = DateTime.Now.AddMinutes(-15);
            
            _farmService.PlantCrop(farm, emptyPlot.Id, CropType.Tomato, plantTime);

            // Act
            var yield = _farmService.HarvestCrop(farm, emptyPlot.Id, DateTime.Now);

            // Assert
            Assert.AreEqual(1, yield);
            Assert.AreEqual(1, farm.Inventory.GetHarvestedCount(CropType.Tomato));
        }

        [Test]
        public void UpgradeEquipment_WithEnoughGold_Succeeds()
        {
            // Arrange
            var farm = _farmService.InitializeNewFarm();
            farm.Inventory.AddGold(500);

            // Act
            var success = _farmService.UpgradeEquipment(farm);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(2, farm.Inventory.EquipmentLevel);
            Assert.AreEqual(100, farm.Inventory.Gold); // 100 + 500 - 500
        }

        [Test]
        public void BuyPlot_WithEnoughGold_IncreasesPlotCount()
        {
            // Arrange
            var farm = _farmService.InitializeNewFarm();
            farm.Inventory.AddGold(500);
            var initialPlotCount = farm.Plots.Count;

            // Act
            var success = _farmService.BuyPlot(farm);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(initialPlotCount + 1, farm.Plots.Count);
        }
    }
}
