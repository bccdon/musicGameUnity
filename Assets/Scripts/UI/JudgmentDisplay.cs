using UnityEngine;
using UnityEngine.UI;
using PulseHighway.Core;
using PulseHighway.Visual;

namespace PulseHighway.UI
{
    public class JudgmentDisplay : MonoBehaviour
    {
        private Text judgmentText;
        private float displayTimer;
        private float scale = 1f;
        private float shakeOffset;
        private bool isMiss;
        private RectTransform rectTransform;

        public void Build(Transform parent)
        {
            judgmentText = UIBuilder.CreateTextSized(parent, "",
                48, Color.white,
                new Vector2(400, 70), new Vector2(0, 150),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            rectTransform = judgmentText.GetComponent<RectTransform>();
            judgmentText.gameObject.SetActive(false);
        }

        public void Show(Judgment judgment, int lane)
        {
            if (judgmentText == null) return;

            string text;
            Color color;

            switch (judgment)
            {
                case Judgment.Perfect:
                    text = "PERFECT";
                    color = new Color(0f, 1f, 0.6f); // Green
                    break;
                case Judgment.Great:
                    text = "GREAT";
                    color = new Color(0f, 0.8f, 1f); // Blue
                    break;
                case Judgment.Good:
                    text = "GOOD";
                    color = new Color(1f, 1f, 0f); // Yellow
                    break;
                default:
                    text = "MISS";
                    color = new Color(1f, 0f, 0.33f); // Red
                    break;
            }

            judgmentText.text = text;
            judgmentText.color = color;
            judgmentText.gameObject.SetActive(true);

            displayTimer = 0.6f;
            scale = 1.5f;
            isMiss = judgment == Judgment.Miss;
            shakeOffset = 0f;
        }

        private void Update()
        {
            if (displayTimer <= 0) return;

            displayTimer -= Time.deltaTime;

            if (displayTimer <= 0)
            {
                judgmentText.gameObject.SetActive(false);
                return;
            }

            // Scale animation: shrink from 1.5 to 1.0
            scale = Mathf.Lerp(1f, scale, 0.85f);
            rectTransform.localScale = Vector3.one * scale;

            // Fade out in last 0.2s
            float alpha = Mathf.Clamp01(displayTimer / 0.2f);
            Color c = judgmentText.color;
            judgmentText.color = new Color(c.r, c.g, c.b, alpha);

            // Miss: horizontal shake
            if (isMiss)
            {
                shakeOffset = Mathf.Sin(Time.time * 50f) * 5f * Mathf.Clamp01(displayTimer / 0.3f);
                rectTransform.anchoredPosition = new Vector2(shakeOffset, 150);
            }
            else
            {
                rectTransform.anchoredPosition = new Vector2(0, 150);
            }
        }
    }
}
