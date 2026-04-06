using PulseHighway.Core;

namespace PulseHighway.Game
{
    public static class TimingWindows
    {
        public const float Perfect = 0.045f;  // ±45ms (was ±25ms — much more forgiving)
        public const float Great = 0.090f;    // ±90ms (was ±50ms)
        public const float Good = 0.150f;     // ±150ms (was ±100ms)

        // Configurable audio latency offset (ms). Positive = player hears audio late.
        public static float LatencyOffset = 0f;

        public static Judgment GetJudgment(float timeDiff)
        {
            if (timeDiff <= Perfect) return Judgment.Perfect;
            if (timeDiff <= Great) return Judgment.Great;
            if (timeDiff <= Good) return Judgment.Good;
            return Judgment.Miss;
        }
    }
}
