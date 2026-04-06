using UnityEngine;
using UnityEngine.UI;
using PulseHighway.Core;
using PulseHighway.Game;

namespace PulseHighway.UI
{
    public class ResultsScreenUI : MonoBehaviour
    {
        private Canvas canvas;
        private float animTimer;
        private Text rankText;
        private Text unlockText;

        public void Build(GameResult result)
        {
            canvas = UIBuilder.CreateCanvas("ResultsCanvas", transform);

            // Full background
            UIBuilder.CreatePanel(canvas.transform, new Color(0.01f, 0.01f, 0.03f, 0.92f),
                Vector2.zero, Vector2.one);

            // Header
            UIBuilder.CreateTextSized(canvas.transform, "LEVEL COMPLETE",
                48, UIBuilder.HexColor("#00ccff"),
                new Vector2(600, 60), new Vector2(0, 420),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            // Level name
            UIBuilder.CreateTextSized(canvas.transform, result.levelName.ToUpper(),
                30, new Color(1f, 1f, 1f, 0.7f),
                new Vector2(500, 40), new Vector2(0, 370),
                TextAnchor.MiddleCenter);

            // Giant rank letter
            rankText = UIBuilder.CreateTextSized(canvas.transform,
                result.rank.ToLetter(),
                160, result.rank.ToColor(),
                new Vector2(200, 200), new Vector2(0, 220),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            // Rank label
            string rankLabel = result.rank switch
            {
                Rank.S => "SUPERB",
                Rank.A => "AMAZING",
                Rank.B => "BRILLIANT",
                Rank.C => "CLEAR",
                Rank.F => "FAILED",
                _ => ""
            };
            UIBuilder.CreateTextSized(canvas.transform, rankLabel,
                22, result.rank.ToColor(),
                new Vector2(300, 30), new Vector2(0, 110),
                TextAnchor.MiddleCenter);

            // Stats container
            float statsY = 30f;
            float statSpacing = 55f;

            // Score
            CreateStatRow(canvas.transform, "SCORE", result.score.ToString("N0"),
                UIBuilder.HexColor("#00ccff"), new Vector2(0, statsY));

            // Max Combo
            CreateStatRow(canvas.transform, "MAX COMBO", $"{result.maxCombo}x",
                UIBuilder.HexColor("#ffff00"), new Vector2(0, statsY - statSpacing));

            // Accuracy
            CreateStatRow(canvas.transform, "ACCURACY", $"{(result.accuracy * 100f):F1}%",
                new Color(1f, 1f, 1f, 0.9f), new Vector2(0, statsY - statSpacing * 2));

            // Judgment breakdown
            float breakdownY = statsY - statSpacing * 3 - 20f;
            CreateJudgmentRow(canvas.transform, "PERFECT", result.perfectCount,
                new Color(0f, 1f, 0.6f), new Vector2(-200, breakdownY));
            CreateJudgmentRow(canvas.transform, "GREAT", result.greatCount,
                new Color(0f, 0.8f, 1f), new Vector2(0, breakdownY));
            CreateJudgmentRow(canvas.transform, "GOOD", result.goodCount,
                new Color(1f, 1f, 0f), new Vector2(200, breakdownY));
            CreateJudgmentRow(canvas.transform, "MISS", result.missCount,
                new Color(1f, 0f, 0.33f), new Vector2(400, breakdownY));

            // Unlock notification
            if (result.newLevelUnlocked && result.unlockedLevelId > 0)
            {
                var unlockLevel = LevelDefinitions.GetLevel(result.unlockedLevelId);
                unlockText = UIBuilder.CreateTextSized(canvas.transform,
                    $"NEW LEVEL UNLOCKED: {unlockLevel.name.ToUpper()}",
                    26, UIBuilder.HexColor("#00ff99"),
                    new Vector2(700, 40), new Vector2(0, breakdownY - 60),
                    TextAnchor.MiddleCenter, FontStyle.Bold);
            }

            // Buttons
            float buttonY = -420f;

            UIBuilder.CreateButton(canvas.transform, "RETRY",
                new Color(1f, 0f, 0.33f, 0.2f), UIBuilder.HexColor("#ff0055"),
                new Vector2(200, 55), new Vector2(-130, buttonY), 22,
                () => SceneFlowManager.Instance.StartLevel(result.levelId));

            UIBuilder.CreateButton(canvas.transform, "CONTINUE",
                new Color(0f, 0.8f, 1f, 0.2f), UIBuilder.HexColor("#00ccff"),
                new Vector2(200, 55), new Vector2(130, buttonY), 22,
                () => SceneFlowManager.Instance.GoToLevelSelect());
        }

        private void CreateStatRow(Transform parent, string label, string value, Color valueColor, Vector2 position)
        {
            UIBuilder.CreateTextSized(parent, label,
                18, new Color(1f, 1f, 1f, 0.5f),
                new Vector2(200, 25), position + new Vector2(-120, 10),
                TextAnchor.MiddleRight);

            UIBuilder.CreateTextSized(parent, value,
                32, valueColor,
                new Vector2(250, 40), position + new Vector2(100, 0),
                TextAnchor.MiddleLeft, FontStyle.Bold);
        }

        private void CreateJudgmentRow(Transform parent, string label, int count, Color color, Vector2 position)
        {
            UIBuilder.CreateTextSized(parent, label,
                14, color,
                new Vector2(120, 20), position + new Vector2(0, 12),
                TextAnchor.MiddleCenter);

            UIBuilder.CreateTextSized(parent, count.ToString(),
                24, color,
                new Vector2(120, 30), position + new Vector2(0, -12),
                TextAnchor.MiddleCenter, FontStyle.Bold);
        }

        private void Update()
        {
            animTimer += Time.deltaTime;

            // Pulse the rank letter
            if (rankText != null)
            {
                float pulse = Mathf.Sin(animTimer * 2f) * 0.1f + 1f;
                rankText.transform.localScale = Vector3.one * pulse;
            }

            // Pulse unlock text
            if (unlockText != null)
            {
                float alpha = Mathf.Sin(animTimer * 3f) * 0.3f + 0.7f;
                Color c = unlockText.color;
                unlockText.color = new Color(c.r, c.g, c.b, alpha);
            }
        }
    }
}
