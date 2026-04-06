using UnityEngine;
using UnityEngine.UI;
using PulseHighway.Core;
using PulseHighway.Game;
using PulseHighway.Visual;

namespace PulseHighway.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        private Canvas canvas;
        private Text scoreText;
        private Text comboText;
        private Text songTitleText;
        private Text loadingText;
        private GameObject loadingPanel;
        private JudgmentDisplay judgmentDisplay;
        private ComboDisplay comboDisplay;

        public void Build(LevelConfig config)
        {
            canvas = UIBuilder.CreateCanvas("GameplayHUDCanvas", transform);

            // Top bar background
            var topBar = UIBuilder.CreatePanel(canvas.transform, new Color(0, 0, 0, 0.4f),
                new Vector2(0, 0.92f), new Vector2(1, 1));

            // Exit button (top-left)
            UIBuilder.CreateButton(canvas.transform, "EXIT",
                new Color(1f, 0f, 0.33f, 0.3f), UIBuilder.HexColor("#ff0055"),
                new Vector2(100, 35), new Vector2(-880, 500), 16,
                () => SceneFlowManager.Instance.GoToLevelSelect());

            // Song title (top-center-left)
            songTitleText = UIBuilder.CreateTextSized(canvas.transform,
                $"{config.songTitle}  |  {config.genre.ToUpper()}  |  {config.bpm} BPM",
                18, new Color(1f, 1f, 1f, 0.7f),
                new Vector2(500, 30), new Vector2(-300, 500),
                TextAnchor.MiddleLeft);

            // Score (top-right)
            scoreText = UIBuilder.CreateTextSized(canvas.transform, "0",
                38, UIBuilder.HexColor("#00ccff"),
                new Vector2(300, 50), new Vector2(780, 500),
                TextAnchor.MiddleRight, FontStyle.Bold);

            // Combo (below score)
            comboText = UIBuilder.CreateTextSized(canvas.transform, "",
                32, UIBuilder.HexColor("#ffff00"),
                new Vector2(200, 40), new Vector2(810, 455),
                TextAnchor.MiddleRight, FontStyle.Bold);

            // Judgment display (center)
            var judgmentGO = new GameObject("JudgmentDisplay");
            judgmentGO.transform.SetParent(canvas.transform, false);
            judgmentDisplay = judgmentGO.AddComponent<JudgmentDisplay>();
            judgmentDisplay.Build(canvas.transform);

            // Combo burst display
            var comboGO = new GameObject("ComboDisplay");
            comboGO.transform.SetParent(canvas.transform, false);
            comboDisplay = comboGO.AddComponent<ComboDisplay>();
            comboDisplay.Build(canvas.transform);

            // Bottom keyboard hints
            BuildKeyboardHints();

            // Loading panel (hidden by default)
            BuildLoadingPanel();
        }

        private void BuildKeyboardHints()
        {
            string[] keys = { "D", "F", "J", "K", "L" };
            float hintWidth = 60f;
            float totalWidth = hintWidth * 5 + 4 * 10;
            float startX = -totalWidth * 0.5f + hintWidth * 0.5f;

            for (int i = 0; i < 5; i++)
            {
                Color laneColor = MaterialFactory.GetLaneColor(i);

                var keyPanel = UIBuilder.CreatePanel(canvas.transform,
                    new Color(laneColor.r * 0.15f, laneColor.g * 0.15f, laneColor.b * 0.15f, 0.6f),
                    new Vector2(hintWidth, 40));
                var keyRect = keyPanel.GetComponent<RectTransform>();
                keyRect.anchoredPosition = new Vector2(startX + i * (hintWidth + 10), -500);

                var outline = keyPanel.AddComponent<Outline>();
                outline.effectColor = new Color(laneColor.r, laneColor.g, laneColor.b, 0.5f);
                outline.effectDistance = new Vector2(1, 1);

                UIBuilder.CreateText(keyPanel.transform, keys[i], 22, laneColor,
                    TextAnchor.MiddleCenter, FontStyle.Bold);
            }
        }

        private void BuildLoadingPanel()
        {
            loadingPanel = UIBuilder.CreatePanel(canvas.transform, new Color(0, 0, 0, 0.85f),
                Vector2.zero, Vector2.one);

            loadingText = UIBuilder.CreateTextSized(loadingPanel.transform, "Loading...",
                36, UIBuilder.HexColor("#00ccff"),
                new Vector2(600, 60), Vector2.zero,
                TextAnchor.MiddleCenter, FontStyle.Bold);

            // Subtitle
            UIBuilder.CreateTextSized(loadingPanel.transform, "PULSE HIGHWAY",
                20, new Color(1f, 1f, 1f, 0.3f),
                new Vector2(400, 30), new Vector2(0, -50),
                TextAnchor.MiddleCenter);

            loadingPanel.SetActive(false);
        }

        public void ShowLoading(string message)
        {
            loadingPanel.SetActive(true);
            loadingText.text = message;
        }

        public void HideLoading()
        {
            loadingPanel.SetActive(false);
        }

        public void ShowJudgment(Judgment judgment, int lane)
        {
            judgmentDisplay?.Show(judgment, lane);
        }

        public void UpdateScore(int score, int combo)
        {
            if (scoreText != null)
                scoreText.text = score.ToString("N0");

            if (comboText != null)
            {
                if (combo > 0)
                {
                    comboText.text = $"{combo}x";
                    comboText.gameObject.SetActive(true);

                    // Scale combo text at milestones
                    int[] milestones = { 10, 25, 50, 100, 200, 500, 1000 };
                    foreach (int m in milestones)
                    {
                        if (combo == m)
                        {
                            comboDisplay?.ShowMilestone(combo);
                            break;
                        }
                    }
                }
                else
                {
                    comboText.text = "";
                    comboText.gameObject.SetActive(false);
                }
            }
        }
    }
}
