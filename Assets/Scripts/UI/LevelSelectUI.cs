using UnityEngine;
using UnityEngine.UI;
using PulseHighway.Core;
using PulseHighway.Game;

namespace PulseHighway.UI
{
    public class LevelSelectUI : MonoBehaviour
    {
        public void Build()
        {
            var canvas = UIBuilder.CreateCanvas("LevelSelectCanvas", transform);

            // Background
            UIBuilder.CreatePanel(canvas.transform, new Color(0.01f, 0.01f, 0.03f, 0.92f),
                Vector2.zero, Vector2.one);

            // Top bar
            var topBar = UIBuilder.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0.6f),
                new Vector2(0, 0.91f), new Vector2(1, 1));

            // Back button — anchored to top-left
            var backBtn = UIBuilder.CreateButton(topBar.transform, "< BACK",
                new Color(1f, 0f, 0.33f, 0.3f), UIBuilder.HexColor("#ff0055"),
                new Vector2(120, 40), Vector2.zero, 18,
                () => SceneFlowManager.Instance.GoToMainMenu());
            var backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0.5f);
            backRect.anchorMax = new Vector2(0, 0.5f);
            backRect.pivot = new Vector2(0, 0.5f);
            backRect.anchoredPosition = new Vector2(20, 0);

            // Title — centered
            var title = UIBuilder.CreateText(topBar.transform, "CAMPAIGN",
                38, UIBuilder.HexColor("#00ccff"), TextAnchor.MiddleCenter, FontStyle.Bold);

            // Scroll area
            var scrollView = UIBuilder.CreateScrollView(canvas.transform,
                new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.89f));
            var content = scrollView.content;

            // Build cards
            string currentPhase = "";
            foreach (var level in LevelDefinitions.AllLevels)
            {
                if (level.PhaseName != currentPhase)
                {
                    currentPhase = level.PhaseName;
                    CreatePhaseHeader(content, level);
                }
                CreateLevelCard(content, level);
            }
        }

        private void CreatePhaseHeader(RectTransform parent, LevelConfig level)
        {
            var go = new GameObject($"Phase{level.PhaseNumber}");
            go.transform.SetParent(parent, false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 50;
            le.flexibleWidth = 1;

            Color phaseColor = GetPhaseColor(level.PhaseNumber);
            var bg = go.AddComponent<Image>();
            bg.color = new Color(phaseColor.r * 0.2f, phaseColor.g * 0.2f, phaseColor.b * 0.2f, 0.9f);

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(phaseColor.r, phaseColor.g, phaseColor.b, 0.4f);
            outline.effectDistance = new Vector2(0, -2);

            UIBuilder.CreateText(go.transform,
                $"PHASE {level.PhaseNumber}: {level.PhaseName}  -  {level.difficulty.ToUpper()}",
                22, phaseColor, TextAnchor.MiddleCenter, FontStyle.Bold);
        }

        private void CreateLevelCard(RectTransform parent, LevelConfig level)
        {
            bool unlocked = LevelManager.Instance != null && LevelManager.Instance.IsLevelUnlocked(level.id);
            var progress = LevelManager.Instance?.GetProgress(level.id);

            var go = new GameObject($"Level{level.id}");
            go.transform.SetParent(parent, false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 70;
            le.flexibleWidth = 1;

            Color phaseColor = GetPhaseColor(level.PhaseNumber);
            Color bgColor = unlocked
                ? new Color(phaseColor.r * 0.06f, phaseColor.g * 0.06f, phaseColor.b * 0.06f, 0.95f)
                : new Color(0.04f, 0.04f, 0.04f, 0.7f);

            var bg = go.AddComponent<Image>();
            bg.color = bgColor;

            // Horizontal layout using anchored children
            var rect = go.GetComponent<RectTransform>();

            if (unlocked)
            {
                var btn = go.AddComponent<Button>();
                int id = level.id;
                btn.onClick.AddListener(() => ShowMusicPicker(id));
                var colors = btn.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f);
                colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
                btn.colors = colors;
            }

            // Level number — left side
            var numText = CreateAnchoredText(go.transform, level.id.ToString("D2"),
                30, unlocked ? phaseColor : new Color(0.3f, 0.3f, 0.3f),
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(60, 0),
                FontStyle.Bold);

            // Level name — left of center
            CreateAnchoredText(go.transform,
                unlocked ? level.name.ToUpper() : "LOCKED",
                20, unlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f),
                new Vector2(0, 0.5f), new Vector2(0, 1),
                new Vector2(0, 1), new Vector2(75, -5), new Vector2(400, 0),
                FontStyle.Bold);

            // Description
            CreateAnchoredText(go.transform,
                unlocked ? level.description : "Complete previous level",
                14, new Color(0.5f, 0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(0, 0.5f),
                new Vector2(0, 0), new Vector2(75, 5), new Vector2(400, 0));

            if (unlocked)
            {
                // Genre + BPM — right side
                Color genreColor = level.genre switch
                {
                    "synthwave" => UIBuilder.HexColor("#ff0055"),
                    "dnb" => UIBuilder.HexColor("#00ff99"),
                    "techno" => UIBuilder.HexColor("#ffff00"),
                    _ => Color.white
                };

                CreateAnchoredText(go.transform,
                    $"{level.genre.ToUpper()} | {level.bpm} BPM",
                    13, genreColor,
                    new Vector2(1, 0.5f), new Vector2(1, 1),
                    new Vector2(1, 1), new Vector2(-10, -5), new Vector2(180, 0));

                // Rank + score
                if (progress != null && progress.rank != Rank.None)
                {
                    CreateAnchoredText(go.transform,
                        progress.rank.ToLetter(),
                        36, progress.rank.ToColor(),
                        new Vector2(1, 0), new Vector2(1, 1),
                        new Vector2(1, 0.5f), new Vector2(-190, 0), new Vector2(50, 0),
                        FontStyle.Bold);

                    CreateAnchoredText(go.transform,
                        $"{progress.highScore:N0}  |  {progress.maxCombo}x",
                        12, new Color(0.6f, 0.6f, 0.6f),
                        new Vector2(1, 0), new Vector2(1, 0.5f),
                        new Vector2(1, 0), new Vector2(-10, 5), new Vector2(160, 0));
                }
                else
                {
                    CreateAnchoredText(go.transform, "NEW",
                        14, new Color(0.3f, 0.3f, 0.3f),
                        new Vector2(1, 0), new Vector2(1, 1),
                        new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(60, 0));
                }
            }
        }

        private void ShowMusicPicker(int levelId)
        {
            var pickerGO = new GameObject("MusicPicker");
            var picker = pickerGO.AddComponent<MusicPickerPopup>();
            picker.Show(levelId);
        }

        private Text CreateAnchoredText(Transform parent, string text, int fontSize, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 sizeDelta,
            FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.pivot = pivot;
            r.anchoredPosition = position;
            r.sizeDelta = sizeDelta;

            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            t.fontStyle = style;
            t.alignment = TextAnchor.MiddleLeft;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private Color GetPhaseColor(int phase)
        {
            return phase switch
            {
                1 => UIBuilder.HexColor("#00ccff"),
                2 => UIBuilder.HexColor("#ff9900"),
                3 => UIBuilder.HexColor("#ff0055"),
                4 => UIBuilder.HexColor("#ffff00"),
                _ => Color.white
            };
        }
    }
}
