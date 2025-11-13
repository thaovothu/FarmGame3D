using System;

namespace FarmGame.Domain.Entities
{
    public enum TaskType
    {
        PlantCrop,
        HarvestCrop,
        CollectMilk
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }

    [Serializable]
    public class FarmTask
    {
        // Serializable fields for JsonUtility
        public string Id;
        public TaskType Type;
        public TaskStatus Status;
        public string PlotId;
        public CropType? CropType;
        public long CreatedTimeTicks;
        public string AssignedWorkerId;

        // Property for easy DateTime access
        public DateTime CreatedTime
        {
            get => new DateTime(CreatedTimeTicks);
            set => CreatedTimeTicks = value.Ticks;
        }

        public FarmTask()
        {
            Id = Guid.NewGuid().ToString();
            Status = TaskStatus.Pending;
            CreatedTime = DateTime.Now;
        }

        public FarmTask(TaskType type, string plotId, CropType? cropType = null)
        {
            Id = Guid.NewGuid().ToString();
            Type = type;
            Status = TaskStatus.Pending;
            PlotId = plotId;
            CropType = cropType;
            CreatedTime = DateTime.Now;
        }
    }
}
