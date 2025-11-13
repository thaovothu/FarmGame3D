using System;

namespace FarmGame.Domain.Entities
{
    public enum PlotStatus
    {
        Empty,
        HasPlant,
        HasAnimal
    }

    [Serializable]
    public class Plot
    {
        // Serializable fields for JsonUtility
        public string Id;
        public PlotStatus Status;
        public Plant Plant;
        public Animal Animal;

        public Plot()
        {
            Id = Guid.NewGuid().ToString();
            Status = PlotStatus.Empty;
        }

        public bool IsEmpty()
        {
            return Status == PlotStatus.Empty;
        }

        public void PlantCrop(Plant plant)
        {
            if (!IsEmpty())
                throw new InvalidOperationException("Plot is not empty");

            Plant = plant;
            Status = PlotStatus.HasPlant;
        }

        public void PlaceAnimal(Animal animal)
        {
            if (!IsEmpty())
                throw new InvalidOperationException("Plot is not empty");

            Animal = animal;
            Status = PlotStatus.HasAnimal;
        }

        public void Clear()
        {
            Plant = null;
            Animal = null;
            Status = PlotStatus.Empty;
        }

        public bool NeedsClearing(DateTime currentTime, float spoilageTimeMinutes)
        {
            if (Status == PlotStatus.HasPlant && Plant != null)
            {
                return !Plant.IsAlive || Plant.HasSpoiled(currentTime, spoilageTimeMinutes);
            }
            else if (Status == PlotStatus.HasAnimal && Animal != null)
            {
                return !Animal.IsAlive || Animal.HasSpoiled(currentTime, spoilageTimeMinutes);
            }
            return false;
        }
    }
}
