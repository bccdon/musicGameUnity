using System.Collections.Generic;
using PulseHighway.Game;

namespace PulseHighway.Core
{
    public class GameState
    {
        public int currentLevelId;
        public LevelConfig currentLevel;
        public ChartData currentChart;
        public int score;
        public int combo;
        public int maxCombo;
        public Dictionary<Judgment, int> judgmentCounts = new Dictionary<Judgment, int>
        {
            { Judgment.Perfect, 0 },
            { Judgment.Great, 0 },
            { Judgment.Good, 0 },
            { Judgment.Miss, 0 }
        };
        public float maxPossibleScore;

        public void Reset()
        {
            score = 0;
            combo = 0;
            maxCombo = 0;
            judgmentCounts[Judgment.Perfect] = 0;
            judgmentCounts[Judgment.Great] = 0;
            judgmentCounts[Judgment.Good] = 0;
            judgmentCounts[Judgment.Miss] = 0;
            maxPossibleScore = 0;
        }
    }

    public enum Judgment
    {
        Perfect,
        Great,
        Good,
        Miss
    }
}
