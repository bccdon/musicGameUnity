using UnityEngine;
using System.Collections.Generic;
using System.IO;
using PulseHighway.Game;

namespace PulseHighway.Storage
{
    public class ProgressStorage : MonoBehaviour
    {
        public static ProgressStorage Instance { get; private set; }

        private Dictionary<int, LevelProgress> progressCache = new Dictionary<int, LevelProgress>();
        private string savePath;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            savePath = Path.Combine(Application.persistentDataPath, "progress.json");
            LoadFromDisk();
        }

        public LevelProgress GetProgress(int levelId)
        {
            progressCache.TryGetValue(levelId, out var progress);
            return progress;
        }

        public void SaveProgress(LevelProgress progress)
        {
            progressCache[progress.levelId] = progress;
            SaveToDisk();
        }

        public Dictionary<int, LevelProgress> GetAllProgress()
        {
            return new Dictionary<int, LevelProgress>(progressCache);
        }

        public void ResetAll()
        {
            progressCache.Clear();
            progressCache[1] = new LevelProgress
            {
                levelId = 1,
                isUnlocked = true,
                highScore = 0,
                maxCombo = 0,
                rank = Rank.None
            };
            SaveToDisk();
        }

        private void LoadFromDisk()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    string json = File.ReadAllText(savePath);
                    var data = JsonUtility.FromJson<SaveData>(json);
                    if (data?.levels != null)
                    {
                        foreach (var entry in data.levels)
                        {
                            System.Enum.TryParse<Rank>(entry.rank, out var rank);
                            progressCache[entry.levelId] = new LevelProgress
                            {
                                levelId = entry.levelId,
                                isUnlocked = entry.isUnlocked,
                                highScore = entry.highScore,
                                maxCombo = entry.maxCombo,
                                rank = rank
                            };
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to load progress: {e.Message}");
            }

            // Ensure level 1 is always unlocked
            if (!progressCache.ContainsKey(1))
            {
                progressCache[1] = new LevelProgress
                {
                    levelId = 1,
                    isUnlocked = true,
                    highScore = 0,
                    maxCombo = 0,
                    rank = Rank.None
                };
            }
        }

        private void SaveToDisk()
        {
            try
            {
                var data = new SaveData();
                foreach (var kvp in progressCache)
                {
                    data.levels.Add(new LevelProgressData
                    {
                        levelId = kvp.Value.levelId,
                        isUnlocked = kvp.Value.isUnlocked,
                        highScore = kvp.Value.highScore,
                        maxCombo = kvp.Value.maxCombo,
                        rank = kvp.Value.rank.ToString()
                    });
                }

                string json = JsonUtility.ToJson(data, true);
                string dir = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(savePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to save progress: {e.Message}");
            }
        }
    }
}
