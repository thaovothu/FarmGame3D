using System;
using System.Linq;
using FarmGame.Domain.Entities;

namespace FarmGame.Domain.Services
{
    public class WorkerService
    {
        private readonly GameConfig _config;
        private readonly FarmService _farmService;

        public WorkerService(GameConfig config, FarmService farmService)
        {
            _config = config;
            _farmService = farmService;
        }

        /// <summary>
        /// Hire a new worker
        /// </summary>
        public bool HireWorker(Farm farm)
        {
            if (!farm.Inventory.CanAfford(_config.WorkerHireCost))
                return false;

            if (farm.Inventory.SpendGold(_config.WorkerHireCost))
            {
                farm.AddWorker(new Worker(_config.WorkerActionTimeMinutes));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Process all worker tasks
        /// </summary>
        public void ProcessWorkerTasks(Farm farm, DateTime currentTime)
        {
            // Complete finished tasks
            foreach (var worker in farm.Workers)
            {
                if (worker.Status == WorkerStatus.Working && worker.IsTaskComplete(currentTime))
                {
                    CompleteWorkerTask(farm, worker, currentTime);
                }
            }

            // Assign new tasks to idle workers
            AssignTasksToIdleWorkers(farm, currentTime);
        }

        private void CompleteWorkerTask(Farm farm, Worker worker, DateTime currentTime)
        {
            var task = farm.TaskQueue.Find(t => t.Id == worker.CurrentTaskId);
            if (task == null)
            {
                worker.CompleteTask();
                return;
            }

            bool success = false;

            switch (task.Type)
            {
                case TaskType.PlantCrop:
                    if (task.CropType.HasValue)
                    {
                        success = _farmService.PlantCrop(farm, task.PlotId, task.CropType.Value, currentTime);
                    }
                    break;

                case TaskType.HarvestCrop:
                    var harvested = _farmService.HarvestCrop(farm, task.PlotId, currentTime);
                    success = harvested > 0;
                    break;

                case TaskType.CollectMilk:
                    var collected = _farmService.CollectMilk(farm, task.PlotId, currentTime);
                    success = collected > 0;
                    break;
            }

            task.Status = success ? TaskStatus.Completed : TaskStatus.Failed;
            worker.CompleteTask();

            // Remove completed or failed tasks
            farm.TaskQueue.Remove(task);
        }

        private void AssignTasksToIdleWorkers(Farm farm, DateTime currentTime)
        {
            var idleWorkers = farm.Workers.Where(w => w.Status == WorkerStatus.Idle).ToList();
            var pendingTasks = farm.TaskQueue.Where(t => t.Status == TaskStatus.Pending).ToList();

            foreach (var worker in idleWorkers)
            {
                if (pendingTasks.Count == 0) break;

                var task = pendingTasks[0];
                task.Status = TaskStatus.InProgress;
                task.AssignedWorkerId = worker.Id;
                worker.AssignTask(task.Id, currentTime);

                pendingTasks.RemoveAt(0);
            }
        }

        /// <summary>
        /// Queue a plant task
        /// </summary>
        public void QueuePlantTask(Farm farm, string plotId, CropType cropType)
        {
            var task = new FarmTask(TaskType.PlantCrop, plotId, cropType);
            farm.AddTask(task);
        }

        /// <summary>
        /// Queue a harvest task
        /// </summary>
        public void QueueHarvestTask(Farm farm, string plotId)
        {
            var task = new FarmTask(TaskType.HarvestCrop, plotId);
            farm.AddTask(task);
        }

        /// <summary>
        /// Queue a collect milk task
        /// </summary>
        public void QueueCollectMilkTask(Farm farm, string plotId)
        {
            var task = new FarmTask(TaskType.CollectMilk, plotId);
            farm.AddTask(task);
        }

        /// <summary>
        /// Auto-queue harvest tasks for all ready plots
        /// </summary>
        public void AutoQueueHarvestTasks(Farm farm, DateTime currentTime)
        {
            var equipmentBonus = farm.Inventory.GetEquipmentBonus();

            foreach (var plot in farm.Plots)
            {
                // Skip if already has a task queued for this plot
                if (farm.TaskQueue.Any(t => t.PlotId == plot.Id && t.Status != TaskStatus.Failed))
                    continue;

                if (plot.Status == PlotStatus.HasPlant && plot.Plant != null)
                {
                    if (plot.Plant.GetReadyHarvestCount(currentTime, equipmentBonus) > 0)
                    {
                        QueueHarvestTask(farm, plot.Id);
                    }
                }
                else if (plot.Status == PlotStatus.HasAnimal && plot.Animal != null)
                {
                    if (plot.Animal.GetReadyProductionCount(currentTime, equipmentBonus) > 0)
                    {
                        QueueCollectMilkTask(farm, plot.Id);
                    }
                }
            }
        }
    }
}

