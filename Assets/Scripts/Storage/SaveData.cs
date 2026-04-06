using System.Collections.Generic;

namespace PulseHighway.Storage
{
    [System.Serializable]
    public class SaveData
    {
        public List<LevelProgressData> levels = new List<LevelProgressData>();
    }

    [System.Serializable]
    public class LevelProgressData
    {
        public int levelId;
        public bool isUnlocked;
        public int highScore;
        public int maxCombo;
        public string rank; // "S", "A", "B", "C", "F", "None"
    }
}
