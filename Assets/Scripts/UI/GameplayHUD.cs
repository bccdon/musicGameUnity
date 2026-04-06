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
        private Text loadingText;
        private GameObject loadingPanel;
        private JudgmentDisplay judgmentDisplay;
        private ComboDisplay comboDisplay;
        private Image screenFlash;

        // Flying score text pool
        private Text[] flyingScorePool;
        private float[] flyingScoreTimers;
        private Vector2[] flyingScoreVelocities;
        private const int FLYING_POOL_SIZE = 20;

        private int displayScore;
        private int targetScore;
        private float comboScale = 1f;

        public void Build(LevelConfig config)
        {
            canvas = UIBuilder.CreateCanvas("GameplayHUDCanvas", transform);

            // Top bar
            UIBuilder.CreatePanel(canvas.transform, new Color(0, 0, 0, 0.4f),
                new Vector2(0, 0.93f), new Vector2(1, 1));

            // Exit button — anchored top-left
            var exitBtn = UIBuilder.CreateButton(canvas.transform, "EXIT",
                new Color(1f, 0f, 0.33f, 0.3f), UIBuilder.HexColor("#ff0055"),
                new Vector2(90, 30), Vector2.zero, 14,
                () => SceneFlowManager.Instance.GoToLevelSelect());
            var exitRect = exitBtn.GetComponent<RectTransform>();
            exitRect.anchorMin = new Vector2(0, 1);
            exitRect.anchorMax = new Vector2(0, 1);
            exitRect.pivot = new Vector2(0, 1);
            exitRect.anchoredPosition = new Vector2(15, -10);

            // Song title — top center
            UIBuilder.CreateTextSized(canvas.transform,
                $"{config.songTitle}  |  {config.genre.ToUpper()}  |  {config.bpm} BPM",
                16, new Color(1f, 1f, 1f, 0.6f),
                new Vector2(500, 25), new Vector2(0, -18),
                TextAnchor.MiddleCenter);
            canvas.transform.GetChild(canvas.transform.childCount - 1).GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 1);
            canvas.transform.GetChild(canvas.transform.childCount - 1).GetComponent<RectTransform>().anchorMax = new Vector2(0.75f, 1);

            // Score — top right, large
            scoreText = UIBuilder.CreateTextSized(canvas.transform, "0",
                42, UIBuilder.HexColor("#00ccff"),
                new Vector2(300, 50), Vector2.zero,
                TextAnchor.MiddleRight, FontStyle.Bold);
            var scoreRect = scoreText.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(1, 1);
            scoreRect.anchorMax = new Vector2(1, 1);
            scoreRect.pivot = new Vector2(1, 1);
            scoreRect.anchoredPosition = new Vector2(-20, -8);

            // Combo — bottom center, big and bold
            comboText = UIBuilder.CreateTextSized(canvas.transform, "",
                48, UIBuilder.HexColor("#ffff00"),
                new Vector2(300, 60), new Vector2(0, -430),
                TextAnchor.MiddleCenter, FontStyle.Bold);
            var comboOutline = comboText.gameObject.AddComponent<Outline>();
            comboOutline.effectColor = new Color(0, 0, 0, 0.5f);
            comboOutline.effectDistance = new Vector2(2, 2);

            // Judgment display — near bottom (at hit line area)
            var judgmentGO = new GameObject("JudgmentDisplay");
            judgmentGO.transform.SetParent(canvas.transform, false);
            judgmentDisplay = judgmentGO.AddComponent<JudgmentDisplay>();
            judgmentDisplay.Build(canvas.transform);

            // Combo burst
            var comboGO = new GameObject("ComboDisplay");
            comboGO.transform.SetParent(canvas.transform, false);
            comboDisplay = comboGO.AddComponent<ComboDisplay>();
            comboDisplay.Build(canvas.transform);

            // Screen flash overlay (invisible by default)
            var flashGO = UIBuilder.CreatePanel(canvas.transform, new Color(1, 1, 1, 0),
                Vector2.zero, Vector2.one);
            screenFlash = flashGO.GetComponent<Image>();
            screenFlash.raycastTarget = false;

            // Keyboard hints at bottom
            BuildKeyboardHints();

            // Flying score text pool
            BuildFlyingScorePool();

            // Loading panel
            BuildLoadingPanel();
        }

        private void BuildKeyboardHints()
        {
            string[] keys = { "D", "F", "J", "K", "L" };
            float hintW = 55f;
            float total = hintW * 5 + 4 * 8;
            float startX = -total * 0.5f + hintW * 0.5f;

            for (int i = 0; i < 5; i++)
            {
                Color lc = MaterialFactory.GetLaneColor(i);
                var panel = UIBuilder.CreatePanel(canvas.transform,
                    new Color(lc.r * 0.1f, lc.g * 0.1f, lc.b * 0.1f, 0.5f),
                    new Vector2(hintW, 35));
                var rect = panel.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0);
                rect.anchorMax = new Vector2(0.5f, 0);
                rect.pivot = new Vector2(0.5f, 0);
                rect.anchoredPosition = new Vector2(startX + i * (hintW + 8), 10);

                panel.AddComponent<Outline>().effectColor = new Color(lc.r, lc.g, lc.b, 0.4f);
                UIBuilder.CreateText(panel.transform, keys[i], 20, lc,
                    TextAnchor.MiddleCenter, FontStyle.Bold);
            }
        }

        private void BuildFlyingScorePool()
        {
            flyingScorePool = new Text[FLYING_POOL_SIZE];
            flyingScoreTimers = new float[FLYING_POOL_SIZE];
            flyingScoreVelocities = new Vector2[FLYING_POOL_SIZE];

            for (int i = 0; i < FLYING_POOL_SIZE; i++)
            {
                var txt = UIBuilder.CreateTextSized(canvas.transform, "",
                    28, Color.white,
                    new Vector2(200, 35), Vector2.zero,
                    TextAnchor.MiddleCenter, FontStyle.Bold);
                txt.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.5f);
                txt.gameObject.SetActive(false);
                flyingScorePool[i] = txt;
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

            UIBuilder.CreateTextSized(loadingPanel.transform, "PULSE HIGHWAY",
                20, new Color(1f, 1f, 1f, 0.3f),
                new Vector2(400, 30), new Vector2(0, -50),
                TextAnchor.MiddleCenter);

            loadingPanel.SetActive(false);
        }

        public void ShowLoading(string msg) { loadingPanel.SetActive(true); loadingText.text = msg; }
        public void HideLoading() { loadingPanel.SetActive(false); }

        public void ShowJudgment(Judgment judgment, int lane)
        {
            judgmentDisplay?.Show(judgment, lane);
        }

        /// <summary>
        /// Spawn a flying score number that floats upward and fades.
        /// </summary>
        public void SpawnFlyingScore(int points, Judgment judgment, int lane)
        {
            // Find available text
            for (int i = 0; i < FLYING_POOL_SIZE; i++)
            {
                if (flyingScoreTimers[i] <= 0)
                {
                    Color c = judgment switch
                    {
                        Judgment.Perfect => new Color(0f, 1f, 0.6f),
                        Judgment.Great => new Color(0f, 0.8f, 1f),
                        Judgment.Good => new Color(1f, 1f, 0f),
                        _ => new Color(1f, 0.2f, 0.2f)
                    };

                    string prefix = judgment == Judgment.Perfect ? "+" : "";
                    flyingScorePool[i].text = $"{prefix}{points}";
                    flyingScorePool[i].color = c;
                    flyingScorePool[i].fontSize = judgment == Judgment.Perfect ? 34 : 26;
                    flyingScorePool[i].gameObject.SetActive(true);

                    // Position: horizontally spread by lane, vertically near bottom
                    float laneX = (lane - 2) * 80f;
                    float randomX = Random.Range(-20f, 20f);
                    flyingScorePool[i].GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(laneX + randomX, -350f);

                    flyingScoreTimers[i] = 1.2f;
                    flyingScoreVelocities[i] = new Vector2(Random.Range(-15f, 15f), Random.Range(80f, 130f));
                    break;
                }
            }
        }

        /// <summary>
        /// Flash the screen white/color briefly.
        /// </summary>
        public void FlashScreen(Color color, float intensity = 0.15f)
        {
            if (screenFlash != null)
                screenFlash.color = new Color(color.r, color.g, color.b, intensity);
        }

        public void UpdateScore(int score, int combo)
        {
            targetScore = score;

            if (comboText != null)
            {
                if (combo > 0)
                {
                    comboText.text = $"{combo}x";
                    comboText.gameObject.SetActive(true);
                    comboScale = 1.3f; // Pop on each hit

                    int[] milestones = { 10, 25, 50, 100, 200, 500, 1000 };
                    foreach (int m in milestones)
                    {
                        if (combo == m) { comboDisplay?.ShowMilestone(combo); break; }
                    }
                }
                else
                {
                    comboText.text = "";
                    comboText.gameObject.SetActive(false);
                }
            }
        }

        private void Update()
        {
            // Animated score counting
            if (displayScore != targetScore)
            {
                displayScore = (int)Mathf.Lerp(displayScore, targetScore, Time.deltaTime * 12f);
                if (Mathf.Abs(displayScore - targetScore) < 5) displayScore = targetScore;
                if (scoreText != null) scoreText.text = displayScore.ToString("N0");
            }

            // Combo scale pop animation
            if (comboScale > 1.01f)
            {
                comboScale = Mathf.Lerp(comboScale, 1f, Time.deltaTime * 8f);
                if (comboText != null)
                    comboText.transform.localScale = Vector3.one * comboScale;
            }

            // Fade screen flash
            if (screenFlash != null && screenFlash.color.a > 0.001f)
            {
                Color c = screenFlash.color;
                screenFlash.color = new Color(c.r, c.g, c.b, c.a * 0.88f);
            }

            // Animate flying score texts
            for (int i = 0; i < FLYING_POOL_SIZE; i++)
            {
                if (flyingScoreTimers[i] > 0)
                {
                    flyingScoreTimers[i] -= Time.deltaTime;

                    if (flyingScoreTimers[i] <= 0)
                    {
                        flyingScorePool[i].gameObject.SetActive(false);
                        continue;
                    }

                    var rect = flyingScorePool[i].GetComponent<RectTransform>();
                    rect.anchoredPosition += flyingScoreVelocities[i] * Time.deltaTime;
                    flyingScoreVelocities[i].y *= 0.98f; // Slow down

                    // Fade
                    float alpha = Mathf.Clamp01(flyingScoreTimers[i] / 0.4f);
                    Color fc = flyingScorePool[i].color;
                    flyingScorePool[i].color = new Color(fc.r, fc.g, fc.b, alpha);

                    // Scale down
                    float s = Mathf.Clamp01(flyingScoreTimers[i] / 0.8f);
                    rect.localScale = Vector3.one * (0.5f + s * 0.5f);
                }
            }
        }
    }
}
