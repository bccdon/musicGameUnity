using UnityEngine;
using UnityEngine.UI;

namespace PulseHighway.UI
{
    public class ComboDisplay : MonoBehaviour
    {
        private Text milestoneText;
        private float displayTimer;
        private float currentScale = 1f;
        private RectTransform rectTransform;

        public void Build(Transform parent)
        {
            milestoneText = UIBuilder.CreateTextSized(parent, "",
                64, new Color(1f, 0.84f, 0f), // Gold
                new Vector2(500, 80), new Vector2(0, 250),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            rectTransform = milestoneText.GetComponent<RectTransform>();
            milestoneText.gameObject.SetActive(false);
        }

        public void ShowMilestone(int combo)
        {
            if (milestoneText == null) return;

            milestoneText.text = $"{combo}x COMBO!";
            milestoneText.gameObject.SetActive(true);

            displayTimer = 1.2f;
            currentScale = 2f;

            // Color based on combo level
            if (combo >= 500) milestoneText.color = new Color(1f, 0f, 0.33f);      // Red fire
            else if (combo >= 200) milestoneText.color = new Color(1f, 0.5f, 0f);   // Orange
            else if (combo >= 100) milestoneText.color = new Color(1f, 0.84f, 0f);  // Gold
            else if (combo >= 50) milestoneText.color = new Color(0f, 1f, 0.6f);    // Green
            else milestoneText.color = new Color(0f, 0.8f, 1f);                      // Cyan
        }

        private void Update()
        {
            if (displayTimer <= 0) return;

            displayTimer -= Time.deltaTime;

            if (displayTimer <= 0)
            {
                milestoneText.gameObject.SetActive(false);
                return;
            }

            // Scale down
            currentScale = Mathf.Lerp(1f, currentScale, 0.92f);
            rectTransform.localScale = Vector3.one * currentScale;

            // Float up
            float yOffset = 250f + (1.2f - displayTimer) * 30f;
            rectTransform.anchoredPosition = new Vector2(0, yOffset);

            // Fade
            float alpha = Mathf.Clamp01(displayTimer / 0.3f);
            Color c = milestoneText.color;
            milestoneText.color = new Color(c.r, c.g, c.b, alpha);
        }
    }
}
