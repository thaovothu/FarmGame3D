using System;

namespace FarmGame.Domain.Entities
{
    public enum WorkerStatus
    {
        Idle,
        Working
    }

    [Serializable]
    public class Worker
    {
        // Serializable fields for JsonUtility
        public string Id;
        public WorkerStatus Status;
        public string CurrentTaskId;
        public long TaskStartTimeTicks;
        public float ActionTimeMinutes;

        // Property for easy DateTime access
        public DateTime TaskStartTime
        {
            get => new DateTime(TaskStartTimeTicks);
            set => TaskStartTimeTicks = value.Ticks;
        }

        public Worker()
        {
            Id = Guid.NewGuid().ToString();
            Status = WorkerStatus.Idle;
            CurrentTaskId = null;
        }

        public Worker(float actionTimeMinutes)
        {
            Id = Guid.NewGuid().ToString();
            Status = WorkerStatus.Idle;
            CurrentTaskId = null;
            ActionTimeMinutes = actionTimeMinutes;
        }

        public void AssignTask(string taskId, DateTime startTime)
        {
            Status = WorkerStatus.Working;
            CurrentTaskId = taskId;
            TaskStartTime = startTime;
        }

        public bool IsTaskComplete(DateTime currentTime)
        {
            if (Status != WorkerStatus.Working) return false;
            
            var elapsedTime = (currentTime - TaskStartTime).TotalMinutes;
            return elapsedTime >= ActionTimeMinutes;
        }

        public void CompleteTask()
        {
            Status = WorkerStatus.Idle;
            CurrentTaskId = null;
        }
    }
}
