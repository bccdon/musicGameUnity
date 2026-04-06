using UnityEngine;

namespace PulseHighway.Game
{
    public enum Rank
    {
        None,
        F,
        C,
        B,
        A,
        S
    }

    public static class RankExtensions
    {
        public static string ToLetter(this Rank rank)
        {
            return rank switch
            {
                Rank.S => "S",
                Rank.A => "A",
                Rank.B => "B",
                Rank.C => "C",
                Rank.F => "F",
                _ => "-"
            };
        }

        public static Color ToColor(this Rank rank)
        {
            return rank switch
            {
                Rank.S => new Color(1f, 0.84f, 0f),      // Gold
                Rank.A => new Color(0f, 1f, 0.6f),        // Green
                Rank.B => new Color(0f, 0.8f, 1f),        // Cyan
                Rank.C => new Color(1f, 0.6f, 0f),        // Orange
                Rank.F => new Color(1f, 0f, 0.33f),       // Red
                _ => new Color(0.5f, 0.5f, 0.5f)          // Gray
            };
        }

        public static bool IsHigherThan(this Rank a, Rank b)
        {
            return (int)a > (int)b;
        }
    }
}
