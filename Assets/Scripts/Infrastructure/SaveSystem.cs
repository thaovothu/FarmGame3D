using System;
using System.IO;
using FarmGame.Domain.Entities;
using UnityEngine;

namespace FarmGame.Infrastructure
{
    [Serializable]
    public class SaveData
    {
        // Note: JsonUtility serializes fields (not properties). Ensure Farm and its members
        // are [Serializable] and expose fields (public or [SerializeField]) so they are saved.
        public Farm Farm;

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

        public static void Save(Farm farm)
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

                var saveData = new SaveData
                {
                    Farm = farm
                };
                saveData.SetSaveTimeNow();

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

        public static SaveData Load()
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