using UnityEngine;
using UnityEngine.UI;
using PulseHighway.Core;

namespace PulseHighway.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        private Canvas canvas;
        private float pulseTimer;
        private Text pulseTitleText;

        public void Build()
        {
            canvas = UIBuilder.CreateCanvas("MainMenuCanvas", transform);

            // Full screen dark background
            UIBuilder.CreatePanel(canvas.transform, new Color(0.01f, 0.01f, 0.03f, 0.85f),
                Vector2.zero, Vector2.one);

            // Title container
            var titlePanel = new GameObject("TitlePanel");
            titlePanel.transform.SetParent(canvas.transform, false);
            var titleRect = titlePanel.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.55f);
            titleRect.anchorMax = new Vector2(0.9f, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Title text - PULSE
            pulseTitleText = UIBuilder.CreateTextSized(titlePanel.transform, "PULSE",
                90, UIBuilder.HexColor("#00ccff"),
                new Vector2(800, 100), new Vector2(0, 30),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            // Title text - HIGHWAY
            UIBuilder.CreateTextSized(titlePanel.transform, "HIGHWAY",
                70, UIBuilder.HexColor("#ff0055"),
                new Vector2(800, 80), new Vector2(0, -50),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            // Subtitle
            UIBuilder.CreateTextSized(titlePanel.transform, "THE ADAPTIVE RHYTHM ARENA",
                22, new Color(1f, 1f, 1f, 0.5f),
                new Vector2(800, 40), new Vector2(0, -110),
                TextAnchor.MiddleCenter, FontStyle.Italic);

            // Button container
            var btnPanel = new GameObject("ButtonPanel");
            btnPanel.transform.SetParent(canvas.transform, false);
            var btnRect = btnPanel.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.25f, 0.15f);
            btnRect.anchorMax = new Vector2(0.75f, 0.5f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            // Campaign button
            UIBuilder.CreateButton(btnPanel.transform, "CAMPAIGN",
                new Color(0f, 0.8f, 1f, 0.15f), UIBuilder.HexColor("#00ccff"),
                new Vector2(400, 70), new Vector2(0, 60), 28,
                () => SceneFlowManager.Instance.GoToLevelSelect());

            // Quick Play — file upload screen
            UIBuilder.CreateButton(btnPanel.transform, "QUICK PLAY",
                new Color(1f, 0f, 0.33f, 0.15f), UIBuilder.HexColor("#ff0055"),
                new Vector2(400, 70), new Vector2(0, -30), 28,
                () => SceneFlowManager.Instance.GoToQuickPlay());

            // Version text
            UIBuilder.CreateTextSized(canvas.transform, "v0.1.0 | Unity Edition",
                16, new Color(1f, 1f, 1f, 0.3f),
                new Vector2(300, 30), new Vector2(0, -520),
                TextAnchor.MiddleCenter);

            // Controls hint
            UIBuilder.CreateTextSized(canvas.transform, "Controls: D F J K L  |  1 2 3 4 5",
                18, new Color(1f, 1f, 1f, 0.4f),
                new Vector2(500, 30), new Vector2(0, -480),
                TextAnchor.MiddleCenter);
        }

        private void Update()
        {
            if (pulseTitleText == null) return;
            pulseTimer += Time.deltaTime;
            float pulse = Mathf.Sin(pulseTimer * 2f) * 0.3f + 0.7f;
            pulseTitleText.color = new Color(0f, 0.8f * pulse, 1f * pulse, 1f);
        }
    }
}
