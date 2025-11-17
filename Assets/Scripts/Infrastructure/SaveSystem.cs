using System;
using System.IO;
using FarmGame.Domain;
using FarmGame.Domain.Entities;
using UnityEngine;
using FarmGame.UI;

namespace FarmGame.Infrastructure
{
    [Serializable]
    public class PlotPositionData
    {
        public int plotIndex;
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class SaveData
    {
        // Note: JsonUtility serializes fields (not properties). Ensure Farm and its members
        // are [Serializable] and expose fields (public or [SerializeField]) so they are saved.
        public Farm Farm;

        // Lưu vị trí của các plot
        public PlotPositionData[] PlotPositions;

        // ISO 8601 string for save time (serialized)
        public string SaveTimeString;

        // Non-serialized helper to access DateTime easily at runtime
        [NonSerialized] public DateTime SaveTime;

        public void UpdateSaveTimeFromString()
        {
            if (!string.IsNullOrEmpty(SaveTimeString))
            {
                if (DateTime.TryParse(SaveTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                {
                    SaveTime = dt;
                }
                else
                {
                    SaveTime = DateTime.Now;
                    SaveTimeString = SaveTime.ToString("o");
                }
            }
            else
            {
                SaveTime = DateTime.Now;
                SaveTimeString = SaveTime.ToString("o");
            }
        }

        public void SetSaveTimeNow()
        {
            SaveTime = DateTime.Now;
            SaveTimeString = SaveTime.ToString("o");
        }
    }

    public static class SaveSystem
    {
        private static readonly string SaveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        private static readonly string SaveFilePath = Path.Combine(SaveDirectory, "savegame.json");

        public static void Save(Farm farm, Farm3DView farm3DView = null)
        {
            if (farm == null)
            {
                Debug.LogWarning("Save called with null farm. Aborting save.");
                return;
            }

            try
            {
                if (!Directory.Exists(SaveDirectory))
                    Directory.CreateDirectory(SaveDirectory);

                // Sync inventory dictionaries to arrays trước khi serialize
                if (farm.Inventory != null)
                {
                    farm.Inventory.SyncToArrays();
                }

                var saveData = new SaveData
                {
                    Farm = farm
                };
                saveData.SetSaveTimeNow();
                
                // Lưu vị trí của các plot nếu có Farm3DView
                if (farm3DView != null)
                {
                    try
                    {
                        saveData.PlotPositions = farm3DView.GetPlotPositionsForSave();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Could not get plot positions from Farm3DView: {ex.Message}");
                    }
                }

                // Update farm's last save time if property exists
                try
                {
                    farm.LastSaveTime = saveData.SaveTime;
                }
                catch { /* ignore if domain does not expose LastSaveTime as settable */ }

                var json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(SaveFilePath, json);

                Debug.Log($"Game saved to: {SaveFilePath}");
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif

                // quick diagnostic: if Farm serialized to empty object, warn developer
                if (json.Contains("\"Farm\": {}"))
                {
                    Debug.LogWarning("Farm serialized to empty JSON object. " +
                                     "Make sure domain classes (Farm, Plot, Plant, Animal, Inventory, Worker) are marked [Serializable] " +
                                     "and use public fields or [SerializeField] fields (JsonUtility does not serialize properties).");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save game: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static SaveData Load(GameConfig gameConfig = null)
        {
            if (!File.Exists(SaveFilePath))
                return null;

            try
            {
                var json = File.ReadAllText(SaveFilePath);
                var saveData = JsonUtility.FromJson<SaveData>(json);
                if (saveData == null)
                {
                    Debug.LogWarning("Save file exists but deserialized SaveData is null.");
                    return null;
                }

                // populate runtime DateTime helper
                saveData.UpdateSaveTimeFromString();

                // Rebuild inventory dictionaries sau khi deserialize
                if (saveData.Farm != null && saveData.Farm.Inventory != null)
                {
                    saveData.Farm.Inventory.RebuildDictionaries();
                    Debug.Log($"Inventory loaded: Seeds={saveData.Farm.Inventory.SeedEntries?.Length ?? 0} types, Harvested={saveData.Farm.Inventory.HarvestEntries?.Length ?? 0} types");
                }

                // Re-initialize Plant/Animal config after JsonUtility deserialization (constructor not called)
                if (gameConfig != null && saveData.Farm?.Plots != null)
                {
                    foreach (var plot in saveData.Farm.Plots)
                    {
                        if (plot?.Plant != null)
                        {
                            plot.Plant.InitializeConfig(gameConfig);
                        }
                        if (plot?.Animal != null)
                        {
                            plot.Animal.InitializeConfig(gameConfig);
                        }
                    }
                }

                // Validate plants/animals config after load (JsonUtility deserialize may have issues)
                ValidatePlotsAfterLoad(saveData.Farm);

                // Clean invalid animals after load
                CleanInvalidAnimalsAfterLoad(saveData.Farm);

                Debug.Log($"Game loaded from: {SaveFilePath}");
                return saveData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading save file: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public static bool HasSaveFile()
        {
            return File.Exists(SaveFilePath);
        }

        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                    File.Delete(SaveFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save file: {ex.Message}");
            }
        }

        public static string GetSaveFilePath()
        {
            return SaveFilePath;
        }

        public static void ValidatePlotsAfterLoad(Farm farm)
        {
            if (farm?.Plots == null) return;
            
            foreach (var plot in farm.Plots)
            {
                if (plot == null) continue;
                
                // Validate Plant config
                if (plot.Plant != null)
                {
                    if (plot.Plant.GrowthTimeMinutes <= 0 || plot.Plant.YieldPerHarvest <= 0 || plot.Plant.LifespanYields <= 0)
                    {
                        Debug.LogWarning($"[SaveSystem] Plant {plot.Plant.CropType} on plot {plot.Id} has invalid config (Growth={plot.Plant.GrowthTimeMinutes}, Yield={plot.Plant.YieldPerHarvest}, Lifespan={plot.Plant.LifespanYields}). Clearing plot.");
                        plot.Clear();
                    }
                }
                
                // Validate Animal config
                if (plot.Animal != null)
                {
                    if (plot.Animal.ProductionTimeMinutes <= 0 || plot.Animal.YieldPerProduction <= 0 || plot.Animal.LifespanProductions <= 0)
                    {
                        Debug.LogWarning($"[SaveSystem] Animal {plot.Animal.AnimalType} on plot {plot.Id} has invalid config (Production={plot.Animal.ProductionTimeMinutes}, Yield={plot.Animal.YieldPerProduction}, Lifespan={plot.Animal.LifespanProductions}). Clearing plot.");
                        plot.Clear();
                    }
                }
            }
        }

        public static void CleanInvalidAnimalsAfterLoad(Farm farm)
        {
            if (farm?.Plots == null) return;
            foreach (var p in farm.Plots)
            {
                if (p?.Animal == null) continue;
                // Nếu animal có ProductionTimeMinutes <= 0 hoặc DairyCowCount <= 0 thì xóa
                if (p.Animal.ProductionTimeMinutes <= 0 || farm.Inventory.DairyCowCount <= 0)
                {
                    p.Animal = null;
                    p.Status = PlotStatus.Empty;
                }
            }
        }
    }
}