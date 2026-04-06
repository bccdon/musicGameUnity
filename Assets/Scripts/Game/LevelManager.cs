using UnityEngine;
using PulseHighway.Storage;

namespace PulseHighway.Game
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public const bool DEBUG_UNLOCK_ALL_LEVELS = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Initialize progress for level 1
            var storage = ProgressStorage.Instance;
            if (storage != null)
            {
                var progress = storage.GetProgress(1);
                if (progress == null || !progress.isUnlocked)
                {
                    storage.SaveProgress(new LevelProgress
                    {
                        levelId = 1,
                        isUnlocked = true,
                        highScore = 0,
                        maxCombo = 0,
                        rank = Rank.None
                    });
                }
            }
        }

        public bool IsLevelUnlocked(int levelId)
        {
            if (DEBUG_UNLOCK_ALL_LEVELS) return true;
            if (levelId == 1) return true;

            var progress = ProgressStorage.Instance?.GetProgress(levelId);
            return progress != null && progress.isUnlocked;
        }

        public LevelProgress GetProgress(int levelId)
        {
            return ProgressStorage.Instance?.GetProgress(levelId);
        }

        public (bool newLevelUnlocked, int unlockedLevelId) CompleteLevel(
            int levelId, int score, int maxCombo, float accuracy)
        {
            var rank = ScoreCalculator.CalculateRank(accuracy);
            var storage = ProgressStorage.Instance;
            if (storage == null) return (false, -1);

            // Update current level progress
            var existing = storage.GetProgress(levelId);
            var progress = new LevelProgress
            {
                levelId = levelId,
                isUnlocked = true,
                highScore = existing != null ? Mathf.Max(existing.highScore, score) : score,
                maxCombo = existing != null ? Mathf.Max(existing.maxCombo, maxCombo) : maxCombo,
                rank = existing != null && existing.rank.IsHigherThan(rank) ? existing.rank : rank
            };
            storage.SaveProgress(progress);

            // Check unlock next level
            var levelConfig = LevelDefinitions.GetLevel(levelId);
            bool newUnlock = false;
            int unlockedId = -1;

            if (rank != Rank.F && score >= levelConfig.unlockScore && levelId < 24)
            {
                int nextId = levelId + 1;
                var nextProgress = storage.GetProgress(nextId);
                if (nextProgress == null || !nextProgress.isUnlocked)
                {
                    storage.SaveProgress(new LevelProgress
                    {
                        levelId = nextId,
                        isUnlocked = true,
                        highScore = 0,
                        maxCombo = 0,
                        rank = Rank.None
                    });
                    newUnlock = true;
                    unlockedId = nextId;
                }
            }

            return (newUnlock, unlockedId);
        }
    }

    [System.Serializable]
    public class LevelProgress
    {
        public int levelId;
        public bool isUnlocked;
        public int highScore;
        public int maxCombo;
        public Rank rank;
    }
}
