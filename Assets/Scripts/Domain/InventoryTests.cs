using NUnit.Framework;
using FarmGame.Domain.Entities;

namespace FarmGame.Tests.Domain
{
    [TestFixture]
    public class InventoryTests
    {
        [Test]
        public void Inventory_Initialize_SetsDefaultValues()
        {
            // Arrange
            var inventory = new Inventory();

            // Act
            inventory.Initialize(100, 1);

            // Assert
            Assert.AreEqual(100, inventory.Gold);
            Assert.AreEqual(1, inventory.EquipmentLevel);
        }

        [Test]
        public void Inventory_AddSeeds_IncreasesCount()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.Initialize(100, 1);

            // Act
            inventory.AddSeeds(CropType.Tomato, 10);

            // Assert
            Assert.AreEqual(10, inventory.GetSeedCount(CropType.Tomato));
        }

        [Test]
        public void Inventory_UseSeed_DecreasesCount()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.Initialize(100, 1);
            inventory.AddSeeds(CropType.Tomato, 10);

            // Act
            var success = inventory.UseSeed(CropType.Tomato);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(9, inventory.GetSeedCount(CropType.Tomato));
        }

        [Test]
        public void Inventory_UseSeed_WhenEmpty_ReturnsFalse()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.Initialize(100, 1);

            // Act
            var success = inventory.UseSeed(CropType.Tomato);

            // Assert
            Assert.IsFalse(success);
        }

        [Test]
        public void Inventory_SpendGold_DecreasesAmount()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.Initialize(100, 1);

            // Act
            var success = inventory.SpendGold(30);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(70, inventory.Gold);
        }

        [Test]
        public void Inventory_SpendGold_InsufficientFunds_ReturnsFalse()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.Initialize(50, 1);

            // Act
            var success = inventory.SpendGold(100);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(50, inventory.Gold);
        }

        [Test]
        public void Inventory_SellHarvest_AddsGoldAndRemovesHarvest()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.Initialize(100, 1);
            inventory.AddHarvest(CropType.Tomato, 10);

            // Act
            var success = inventory.SellHarvest(CropType.Tomato, 10, 5);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(150, inventory.Gold); // 100 + (10 * 5)
            Assert.AreEqual(0, inventory.GetHarvestedCount(CropType.Tomato));
        }

        [Test]
        public void Inventory_SellMilk_AddsGoldAndRemovesMilk()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.Initialize(100, 1);
            inventory.AddMilk(5);

            // Act
            var success = inventory.SellMilk(5, 15);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(175, inventory.Gold); // 100 + (5 * 15)
            Assert.AreEqual(0, inventory.Milk);
        }

        [Test]
        public void Inventory_GetEquipmentBonus_ReturnsCorrectValue()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.Initialize(100, 1);

            // Act & Assert
            Assert.AreEqual(0f, inventory.GetEquipmentBonus()); // Level 1 = 0% bonus
            
            inventory.UpgradeEquipment();
            Assert.AreEqual(0.1f, inventory.GetEquipmentBonus()); // Level 2 = 10% bonus
            
            inventory.UpgradeEquipment();
            Assert.AreEqual(0.2f, inventory.GetEquipmentBonus()); // Level 3 = 20% bonus
        }
    }
}
