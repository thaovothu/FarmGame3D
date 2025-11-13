using System;
using System.Collections.Generic;

namespace FarmGame.Domain.Entities
{
    [Serializable]
    public class Farm
    {
        // Serializable fields for JsonUtility
        public string Id;
        public List<Plot> Plots = new List<Plot>();
        public List<Worker> Workers = new List<Worker>();
        public List<FarmTask> TaskQueue = new List<FarmTask>();
        public Inventory Inventory = new Inventory();
        public long LastSaveTimeTicks;
        public long GameStartTimeTicks;

        // Properties for easy DateTime access
        public DateTime LastSaveTime
        {
            get => new DateTime(LastSaveTimeTicks);
            set => LastSaveTimeTicks = value.Ticks;
        }

        public DateTime GameStartTime
        {
            get => new DateTime(GameStartTimeTicks);
            set => GameStartTimeTicks = value.Ticks;
        }

        public Farm()
        {
            Id = Guid.NewGuid().ToString();
            Plots = new List<Plot>();
            Workers = new List<Worker>();
            TaskQueue = new List<FarmTask>();
            Inventory = new Inventory();
            GameStartTime = DateTime.Now;
            LastSaveTime = DateTime.Now;
        }

        public void AddPlot(Plot plot)
        {
            Plots.Add(plot);
        }

        public void AddWorker(Worker worker)
        {
            Workers.Add(worker);
        }

        public void AddTask(FarmTask task)
        {
            TaskQueue.Add(task);
        }

        public int GetEmptyPlotCount()
        {
            int count = 0;
            foreach (var plot in Plots)
            {
                if (plot.IsEmpty()) count++;
            }
            return count;
        }

        public int GetIdleWorkerCount()
        {
            int count = 0;
            foreach (var worker in Workers)
            {
                if (worker.Status == WorkerStatus.Idle) count++;
            }
            return count;
        }

        public int GetWorkingWorkerCount()
        {
            int count = 0;
            foreach (var worker in Workers)
            {
                if (worker.Status == WorkerStatus.Working) count++;
            }
            return count;
        }

        public Plot GetPlotById(string plotId)
        {
            return Plots.Find(p => p.Id == plotId);
        }

        public Worker GetWorkerById(string workerId)
        {
            return Workers.Find(w => w.Id == workerId);
        }

        public bool HasReachedGoal(int goalAmount)
        {
            return Inventory.Gold >= goalAmount;
        }
    }
}
