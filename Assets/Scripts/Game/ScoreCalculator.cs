using UnityEngine;
using PulseHighway.Core;

namespace PulseHighway.Game
{
    public static class ScoreCalculator
    {
        public static int CalculateHitScore(Judgment judgment, int combo)
        {
            int baseScore = judgment switch
            {
                Judgment.Perfect => 100,
                Judgment.Great => 75,
                Judgment.Good => 50,
                _ => 0
            };

            float multiplier = 1f + combo * 0.1f;
            return Mathf.RoundToInt(baseScore * multiplier);
        }

        public static int CalculateHoldTick(int combo)
        {
            float score = 20f * (1f + combo * 0.05f);
            return Mathf.RoundToInt(score);
        }

        public static Rank CalculateRank(float accuracy)
        {
            if (accuracy >= 0.95f) return Rank.S;
            if (accuracy >= 0.90f) return Rank.A;
            if (accuracy >= 0.80f) return Rank.B;
            if (accuracy >= 0.70f) return Rank.C;
            return Rank.F;
        }
    }
}
