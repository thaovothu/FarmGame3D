using NUnit.Framework;
using System;
using FarmGame.Domain.Entities;

namespace FarmGame.Tests.Domain
{
    [TestFixture]
    public class PlantTests
    {
        [Test]
        public void Plant_InitialState_IsAlive()
        {
            // Arrange
            var plantTime = DateTime.Now;
            var plant = new Plant(CropType.Tomato, plantTime, 10f, 1, 40, 5);

            // Assert
            Assert.IsTrue(plant.IsAlive);
            Assert.AreEqual(0, plant.HarvestCount);
            Assert.AreEqual(0, plant.TotalYield);
            Assert.AreEqual(CropType.Tomato, plant.CropType);
        }

        [Test]
        public void Plant_AfterGrowthTime_HasReadyHarvest()
        {
            // Arrange
            var plantTime = DateTime.Now.AddMinutes(-15);
            var currentTime = DateTime.Now;
            var plant = new Plant(CropType.Tomato, plantTime, 10f, 1, 40, 5);

            // Act
            var readyCount = plant.GetReadyHarvestCount(currentTime, 0);

            // Assert
            Assert.AreEqual(1, readyCount); // 15 minutes / 10 minutes = 1 harvest ready
        }

        [Test]
        public void Plant_Harvest_ReturnsCorrectYield()
        {
            // Arrange
            var plantTime = DateTime.Now.AddMinutes(-15);
            var currentTime = DateTime.Now;
            var plant = new Plant(CropType.Tomato, plantTime, 10f, 1, 40, 5);

            // Act
            var yield = plant.Harvest(currentTime, 0);

            // Assert
            Assert.AreEqual(1, yield);
            Assert.AreEqual(1, plant.HarvestCount);
            Assert.AreEqual(1, plant.TotalYield);
        }

        [Test]
        public void Plant_HarvestMultipleTimes_IncreasesCount()
        {
            // Arrange
            var plantTime = DateTime.Now.AddMinutes(-30);
            var currentTime = DateTime.Now;
            var plant = new Plant(CropType.Tomato, plantTime, 10f, 1, 40, 5);

            // Act
            var yield = plant.Harvest(currentTime, 0);

            // Assert
            Assert.AreEqual(3, yield); // 30 minutes / 10 minutes = 3 harvests
            Assert.AreEqual(3, plant.HarvestCount);
        }

        [Test]
        public void Plant_WithEquipmentBonus_GrowsFaster()
        {
            // Arrange
            var plantTime = DateTime.Now.AddMinutes(-11);
            var currentTime = DateTime.Now;
            var plant = new Plant(CropType.Tomato, plantTime, 1f, 1, 40, 5);

            // Act - With 10% bonus, 10 minutes becomes 9.09 minutes
            var readyCount = plant.GetReadyHarvestCount(currentTime, 0.1f);

            // Assert
            Assert.AreEqual(1, readyCount); // 11 minutes / 9.09 minutes = 1 harvest ready
        }

        [Test]
        public void Plant_AfterMaxHarvests_DiesAndBecomesNotAlive()
        {
            // Arrange
            var plantTime = DateTime.Now.AddMinutes(-500);
            var currentTime = DateTime.Now;
            var plant = new Plant(CropType.Tomato, plantTime, 10f, 1, 40, 5);

            // Act
            plant.Harvest(currentTime, 0);

            // Assert
            Assert.IsFalse(plant.IsAlive);
            Assert.AreEqual(40, plant.HarvestCount);
        }

        [Test]
        public void Plant_CannotHarvestMoreThanLifespan()
        {
            // Arrange
            var plantTime = DateTime.Now.AddMinutes(-500);
            var currentTime = DateTime.Now;
            var plant = new Plant(CropType.Tomato, plantTime, 10f, 1, 40, 5);

            // Act
            var readyCount = plant.GetReadyHarvestCount(currentTime, 0);

            // Assert
            Assert.AreEqual(40, readyCount); // Cannot exceed lifespan of 40
        }

        [Test]
        public void Plant_HasSpoiled_AfterFinalHarvestAndSpoilageTime()
        {
            // Arrange
            var plantTime = DateTime.Now.AddMinutes(-500);
            var currentTime = DateTime.Now;
            var plant = new Plant(CropType.Tomato, plantTime, 10f, 1, 40, 5);
            
            // Harvest to max
            plant.Harvest(currentTime, 0);
            
            // Act - Check spoilage after 70 minutes (spoilage time is 60)
            var futureTime = currentTime.AddMinutes(70);
            var hasSpoiled = plant.HasSpoiled(futureTime, 60);

            // Assert
            Assert.IsTrue(hasSpoiled);
        }

        [Test]
        public void Plant_NotSpoiled_BeforeSpoilageTime()
        {
            // Arrange
            var plantTime = DateTime.Now.AddMinutes(-500);
            var currentTime = DateTime.Now;
            var plant = new Plant(CropType.Tomato, plantTime, 10f, 1, 40, 5);
            
            // Harvest to max
            plant.Harvest(currentTime, 0);
            
            // Act - Check spoilage after 30 minutes (spoilage time is 60)
            var futureTime = currentTime.AddMinutes(30);
            var hasSpoiled = plant.HasSpoiled(futureTime, 60);

            // Assert
            Assert.IsFalse(hasSpoiled);
        }

        [Test]
        public void Plant_GetTimeUntilNextHarvest_ReturnsCorrectTime()
        {
            // Arrange
            var plantTime = DateTime.Now.AddMinutes(-5);
            var currentTime = DateTime.Now;
            var plant = new Plant(CropType.Tomato, plantTime, 10f, 1, 40, 5);

            // Act
            var timeUntilNext = plant.GetTimeUntilNextHarvest(currentTime, 0);

            // Assert
            Assert.AreEqual(5f, timeUntilNext, 0.1f); // Should be ~5 minutes remaining
        }
    }
}
