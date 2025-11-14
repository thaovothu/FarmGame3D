using NUnit.Framework;
using System;
using FarmGame.Domain.Entities;

namespace FarmGame.Tests.Domain
{
    [TestFixture]
    public class AnimalTests
    {
        [Test]
        public void Animal_InitialState_IsAlive()
        {
            // Arrange
            var acquiredTime = DateTime.Now;
            var animal = new Animal(AnimalType.DairyCow, acquiredTime, 30f, 1, 100, 15);

            // Assert
            Assert.IsTrue(animal.IsAlive);
            Assert.AreEqual(0, animal.ProductionCount);
            Assert.AreEqual(0, animal.TotalProduction);
            Assert.AreEqual(AnimalType.DairyCow, animal.AnimalType);
        }

        [Test]
        public void Animal_AfterProductionTime_HasReadyProduction()
        {
            // Arrange
            var acquiredTime = DateTime.Now.AddMinutes(-35);
            var currentTime = DateTime.Now;
            var animal = new Animal(AnimalType.DairyCow, acquiredTime, 30f, 1, 100, 15);

            // Act
            var readyCount = animal.GetReadyProductionCount(currentTime, 0);

            // Assert
            Assert.AreEqual(1, readyCount); // 35 minutes / 30 minutes = 1 production ready
        }

        [Test]
        public void Animal_Collect_ReturnsCorrectYield()
        {
            // Arrange
            var acquiredTime = DateTime.Now.AddMinutes(-35);
            var currentTime = DateTime.Now;
            var animal = new Animal(AnimalType.DairyCow, acquiredTime, 30f, 1, 100, 15);

            // Act
            var yield = animal.Collect(currentTime, 0);

            // Assert
            Assert.AreEqual(1, yield);
            Assert.AreEqual(1, animal.ProductionCount);
            Assert.AreEqual(1, animal.TotalProduction);
        }

        [Test]
        public void Animal_WithEquipmentBonus_ProducesFaster()
        {
            // Arrange
            var acquiredTime = DateTime.Now.AddMinutes(-28);
            var currentTime = DateTime.Now;
            var animal = new Animal(AnimalType.DairyCow, acquiredTime, 30f, 1, 100, 15);

            // Act - With 10% bonus, 30 minutes becomes 27.27 minutes
            var readyCount = animal.GetReadyProductionCount(currentTime, 0.1f);

            // Assert
            Assert.AreEqual(1, readyCount); // 28 minutes / 27.27 minutes = 1 production ready
        }

        [Test]
        public void Animal_AfterMaxProductions_DiesAndBecomesNotAlive()
        {
            // Arrange
            var acquiredTime = DateTime.Now.AddMinutes(-3100);
            var currentTime = DateTime.Now;
            var animal = new Animal(AnimalType.DairyCow, acquiredTime, 30f, 1, 100, 15);

            // Act
            animal.Collect(currentTime, 0);

            // Assert
            Assert.IsFalse(animal.IsAlive);
            Assert.AreEqual(100, animal.ProductionCount);
        }

        [Test]
        public void Animal_CannotCollectMoreThanLifespan()
        {
            // Arrange
            var acquiredTime = DateTime.Now.AddMinutes(-3500);
            var currentTime = DateTime.Now;
            var animal = new Animal(AnimalType.DairyCow, acquiredTime, 30f, 1, 100, 15);

            // Act
            var readyCount = animal.GetReadyProductionCount(currentTime, 0);

            // Assert
            Assert.AreEqual(100, readyCount); // Cannot exceed lifespan of 100
        }

        [Test]
        public void Animal_HasSpoiled_WhenReadyProductionNotCollectedInTime()
        {
            // Arrange - Animal produces every 30 minutes
            var acquiredTime = DateTime.Now.AddMinutes(-100);
            var currentTime = DateTime.Now;
            var animal = new Animal(AnimalType.DairyCow, acquiredTime, 30f, 1, 100, 15);
            
            // Don't collect, wait for spoilage (ready production waiting > 60 min)
            // First production was ready at acquiredTime + 30min = Now - 70min
            // That production has been waiting for 70 minutes > 60 spoilage time
            
            // Act
            var hasSpoiled = animal.HasSpoiled(currentTime, 60, 0);

            // Assert
            Assert.IsTrue(hasSpoiled);
        }

        [Test]
        public void Animal_NotSpoiled_WhenReadyProductionWithinSpoilageTime()
        {
            // Arrange - Animal produces every 30 minutes
            var acquiredTime = DateTime.Now.AddMinutes(-50);
            var currentTime = DateTime.Now;
            var animal = new Animal(AnimalType.DairyCow, acquiredTime, 30f, 1, 100, 15);
            
            // First production was ready at acquiredTime + 30min = Now - 20min
            // That production has been waiting for 20 minutes < 60 spoilage time
            
            // Act
            var hasSpoiled = animal.HasSpoiled(currentTime, 60, 0);

            // Assert
            Assert.IsFalse(hasSpoiled);
        }

        [Test]
        public void Animal_NotSpoiled_WhenNoReadyProduction()
        {
            // Arrange - Animal produces every 30 minutes
            var acquiredTime = DateTime.Now.AddMinutes(-20);
            var currentTime = DateTime.Now;
            var animal = new Animal(AnimalType.DairyCow, acquiredTime, 30f, 1, 100, 15);
            
            // No production ready yet (only 20 minutes passed, needs 30)
            
            // Act
            var hasSpoiled = animal.HasSpoiled(currentTime, 60, 0);

            // Assert
            Assert.IsFalse(hasSpoiled);
        }
    }
}
