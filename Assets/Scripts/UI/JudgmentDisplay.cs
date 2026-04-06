using UnityEngine;
using UnityEngine.UI;
using PulseHighway.Core;

namespace PulseHighway.UI
{
    public class JudgmentDisplay : MonoBehaviour
    {
        private Text judgmentText;
        private float displayTimer;
        private float scale = 1f;
        private float shakeOffset;
        private float floatY;
        private bool isMiss;
        private RectTransform rectTransform;

        public void Build(Transform parent)
        {
            judgmentText = UIBuilder.CreateTextSized(parent, "",
                52, Color.white,
                new Vector2(500, 80), new Vector2(0, -250),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            rectTransform = judgmentText.GetComponent<RectTransform>();
            judgmentText.gameObject.SetActive(false);

            // Add outline for text visibility
            var outline = judgmentText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.6f);
            outline.effectDistance = new Vector2(2, 2);
        }

        public void Show(Judgment judgment, int lane)
        {
            if (judgmentText == null) return;

            string text;
            Color color;

            switch (judgment)
            {
                case Judgment.Perfect:
                    text = "PERFECT!";
                    color = new Color(0f, 1f, 0.6f);
                    break;
                case Judgment.Great:
                    text = "GREAT!";
                    color = new Color(0f, 0.8f, 1f);
                    break;
                case Judgment.Good:
                    text = "GOOD";
                    color = new Color(1f, 1f, 0f);
                    break;
                default:
                    text = "MISS";
                    color = new Color(1f, 0.15f, 0.15f);
                    break;
            }

            judgmentText.text = text;
            judgmentText.color = color;
            judgmentText.fontSize = judgment == Judgment.Perfect ? 58 : 48;
            judgmentText.gameObject.SetActive(true);

            displayTimer = 0.8f;
            scale = judgment == Judgment.Perfect ? 2f : 1.6f;
            isMiss = judgment == Judgment.Miss;
            floatY = 0f;
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

            // Scale pop: starts big, shrinks to 1.0
            scale = Mathf.Lerp(1f, scale, 0.88f);
            rectTransform.localScale = Vector3.one * scale;

            // Float upward
            floatY += Time.deltaTime * 80f;

            // Fade out in last 0.3s
            float alpha = Mathf.Clamp01(displayTimer / 0.3f);
            Color c = judgmentText.color;
            judgmentText.color = new Color(c.r, c.g, c.b, alpha);

            if (isMiss)
            {
                shakeOffset = Mathf.Sin(Time.time * 60f) * 8f * Mathf.Clamp01(displayTimer / 0.4f);
                rectTransform.anchoredPosition = new Vector2(shakeOffset, -250f);
            }
            else
            {
                rectTransform.anchoredPosition = new Vector2(0, -250f + floatY);
            }
        }
    }
}
