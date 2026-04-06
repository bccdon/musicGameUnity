using PulseHighway.Core;

namespace PulseHighway.Game
{
    public static class TimingWindows
    {
        public const float Perfect = 0.025f;  // ±25ms
        public const float Great = 0.050f;    // ±50ms
        public const float Good = 0.100f;     // ±100ms
        public const float Miss = 0.100f;     // >100ms

        public static Judgment GetJudgment(float timeDiff)
        {
            if (timeDiff <= Perfect) return Judgment.Perfect;
            if (timeDiff <= Great) return Judgment.Great;
            if (timeDiff <= Good) return Judgment.Good;
            return Judgment.Miss;
        }
    }
}
