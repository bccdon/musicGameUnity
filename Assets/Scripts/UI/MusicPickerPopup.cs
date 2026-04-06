using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PulseHighway.Core;
using PulseHighway.Game;
using PulseHighway.Audio;

namespace PulseHighway.UI
{
    public class MusicPickerPopup : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject popupRoot;
        private GameObject libraryPanel;
        private GameObject urlPanel;
        private Text statusText;
        private InputField urlInput;
        private int levelId;
        private LevelConfig levelConfig;

        public void Show(int levelId)
        {
            this.levelId = levelId;
            this.levelConfig = LevelDefinitions.GetLevel(levelId);

            canvas = UIBuilder.CreateCanvas("MusicPickerCanvas", transform);
            canvas.sortingOrder = 20; // Above everything

            // Dim background (clickable to close)
            var dimBg = UIBuilder.CreatePanel(canvas.transform, new Color(0, 0, 0, 0.7f),
                Vector2.zero, Vector2.one);
            var dimBtn = dimBg.AddComponent<Button>();
            dimBtn.onClick.AddListener(Close);

            // Popup panel
            popupRoot = UIBuilder.CreatePanel(canvas.transform,
                new Color(0.03f, 0.03f, 0.08f, 0.97f), new Vector2(550, 450));
            var popupRect = popupRoot.GetComponent<RectTransform>();
            popupRect.anchoredPosition = Vector2.zero;

            var outline = popupRoot.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.8f, 1f, 0.4f);
            outline.effectDistance = new Vector2(2, 2);

            // Stop click-through to dim background
            popupRoot.AddComponent<Button>().onClick.AddListener(() => { });

            // Level title
            UIBuilder.CreateTextSized(popupRoot.transform,
                $"LEVEL {levelConfig.id}: {levelConfig.name.ToUpper()}",
                24, UIBuilder.HexColor("#00ccff"),
                new Vector2(500, 35), new Vector2(0, 185),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            UIBuilder.CreateTextSized(popupRoot.transform,
                $"{levelConfig.genre.ToUpper()} | {levelConfig.bpm} BPM | {levelConfig.difficulty.ToUpper()}",
                16, new Color(1f, 1f, 1f, 0.5f),
                new Vector2(400, 25), new Vector2(0, 155),
                TextAnchor.MiddleCenter);

            // Divider
            UIBuilder.CreatePanel(popupRoot.transform,
                new Color(1f, 1f, 1f, 0.1f), new Vector2(480, 1));
            popupRoot.transform.GetChild(popupRoot.transform.childCount - 1).GetComponent<RectTransform>()
                .anchoredPosition = new Vector2(0, 135);

            // Choose music source label
            UIBuilder.CreateTextSized(popupRoot.transform,
                "CHOOSE MUSIC SOURCE",
                18, new Color(1f, 1f, 1f, 0.7f),
                new Vector2(400, 25), new Vector2(0, 110),
                TextAnchor.MiddleCenter);

            // Option 1: Generated Music (default)
            UIBuilder.CreateButton(popupRoot.transform, "GENERATED MUSIC",
                new Color(0f, 1f, 0.6f, 0.15f), UIBuilder.HexColor("#00ff99"),
                new Vector2(420, 50), new Vector2(0, 60), 20,
                () => PlayGenerated());

            // Option 2: Online Library
            UIBuilder.CreateButton(popupRoot.transform, "ONLINE LIBRARY",
                new Color(0f, 0.8f, 1f, 0.15f), UIBuilder.HexColor("#00ccff"),
                new Vector2(420, 50), new Vector2(0, 0), 20,
                () => ShowLibrary());

            // Option 3: Paste URL
            UIBuilder.CreateButton(popupRoot.transform, "PASTE AUDIO URL",
                new Color(1f, 0.6f, 0f, 0.15f), UIBuilder.HexColor("#ff9900"),
                new Vector2(420, 50), new Vector2(0, -60), 20,
                () => ShowUrlInput());

            // Status text
            statusText = UIBuilder.CreateTextSized(popupRoot.transform, "",
                14, new Color(1f, 0.3f, 0.3f),
                new Vector2(480, 25), new Vector2(0, -120),
                TextAnchor.MiddleCenter);

            // Cancel button
            UIBuilder.CreateButton(popupRoot.transform, "CANCEL",
                new Color(0.2f, 0.2f, 0.2f, 0.8f), new Color(0.6f, 0.6f, 0.6f),
                new Vector2(150, 40), new Vector2(0, -180), 16,
                () => Close());

            // Hidden panels
            BuildLibraryPanel();
            BuildUrlPanel();
        }

        private void BuildLibraryPanel()
        {
            libraryPanel = new GameObject("LibraryPanel");
            libraryPanel.transform.SetParent(canvas.transform, false);
            var rect = libraryPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Dim background
            var bg = libraryPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);
            var bgBtn = libraryPanel.AddComponent<Button>();
            bgBtn.onClick.AddListener(() => { libraryPanel.SetActive(false); });

            // Scroll content
            var panel = UIBuilder.CreatePanel(libraryPanel.transform,
                new Color(0.03f, 0.03f, 0.08f, 0.97f), new Vector2(600, 500));

            UIBuilder.CreateTextSized(panel.transform, "ONLINE MUSIC LIBRARY",
                24, UIBuilder.HexColor("#00ccff"),
                new Vector2(500, 35), new Vector2(0, 215),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            UIBuilder.CreateTextSized(panel.transform, "Free to use • Pixabay License",
                14, new Color(1f, 1f, 1f, 0.4f),
                new Vector2(400, 20), new Vector2(0, 190),
                TextAnchor.MiddleCenter);

            // Track list
            var tracks = OnlineMusicLibrary.Tracks;
            float yStart = 150f;
            float yStep = 50f;

            for (int i = 0; i < tracks.Length && i < 8; i++)
            {
                var track = tracks[i];
                float y = yStart - i * yStep;

                Color genreColor = track.genre switch
                {
                    "synthwave" => UIBuilder.HexColor("#ff0055"),
                    "dnb" => UIBuilder.HexColor("#00ff99"),
                    "techno" => UIBuilder.HexColor("#ffff00"),
                    _ => Color.white
                };

                var trackBtn = UIBuilder.CreateButton(panel.transform,
                    $"{track.title}  •  {track.artist}  [{track.genre.ToUpper()}]",
                    new Color(0.06f, 0.06f, 0.1f, 0.9f), genreColor,
                    new Vector2(540, 42), new Vector2(0, y), 15,
                    () => DownloadAndPlay(track));
            }

            // Close button
            UIBuilder.CreateButton(panel.transform, "BACK",
                new Color(0.2f, 0.2f, 0.2f, 0.8f), new Color(0.6f, 0.6f, 0.6f),
                new Vector2(120, 35), new Vector2(0, yStart - tracks.Length * yStep - 20), 14,
                () => libraryPanel.SetActive(false));

            libraryPanel.SetActive(false);
        }

        private void BuildUrlPanel()
        {
            urlPanel = new GameObject("UrlPanel");
            urlPanel.transform.SetParent(popupRoot.transform, false);
            var rect = urlPanel.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(420, 90);
            rect.anchoredPosition = new Vector2(0, -110);

            // Input field
            var inputGO = new GameObject("UrlInput");
            inputGO.transform.SetParent(urlPanel.transform, false);
            var inputRect = inputGO.AddComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(420, 35);
            inputRect.anchoredPosition = new Vector2(0, 15);

            var inputBg = inputGO.AddComponent<Image>();
            inputBg.color = new Color(0.08f, 0.08f, 0.12f, 1f);

            urlInput = inputGO.AddComponent<InputField>();
            urlInput.textComponent = UIBuilder.CreateText(inputGO.transform, "",
                14, Color.white, TextAnchor.MiddleLeft);
            urlInput.textComponent.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            urlInput.textComponent.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);

            // Placeholder
            var placeholder = UIBuilder.CreateText(inputGO.transform, "Paste audio URL (MP3/WAV/OGG)...",
                14, new Color(1f, 1f, 1f, 0.3f), TextAnchor.MiddleLeft);
            placeholder.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            placeholder.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);
            urlInput.placeholder = placeholder;

            // Go button
            UIBuilder.CreateButton(urlPanel.transform, "DOWNLOAD & PLAY",
                new Color(1f, 0.6f, 0f, 0.2f), UIBuilder.HexColor("#ff9900"),
                new Vector2(220, 35), new Vector2(0, -25), 14,
                () => DownloadFromUrl());

            urlPanel.SetActive(false);
        }

        private void PlayGenerated()
        {
            Close();
            SceneFlowManager.Instance.StartLevel(levelId);
        }

        private void ShowLibrary()
        {
            libraryPanel.SetActive(true);
        }

        private void ShowUrlInput()
        {
            urlPanel.SetActive(!urlPanel.activeSelf);
        }

        private void DownloadAndPlay(OnlineTrack track)
        {
            libraryPanel.SetActive(false);
            StartCoroutine(DownloadTrackAndStart(track.url, track.title, track.estimatedBpm));
        }

        private void DownloadFromUrl()
        {
            string url = urlInput?.text?.Trim();
            if (string.IsNullOrEmpty(url))
            {
                statusText.text = "Please enter a URL";
                return;
            }
            StartCoroutine(DownloadTrackAndStart(url, "Custom Track", 0));
        }

        private IEnumerator DownloadTrackAndStart(string url, string title, int estimatedBpm)
        {
            statusText.text = "Downloading...";
            statusText.color = UIBuilder.HexColor("#00ccff");

            AudioClip clip = null;
            string error = null;

            yield return AudioFileLoader.LoadFromUrl(url, (c, e) => { clip = c; error = e; },
                progress => { statusText.text = $"Downloading... {progress * 100:F0}%"; });

            if (clip == null)
            {
                statusText.text = error ?? "Download failed";
                statusText.color = new Color(1f, 0.3f, 0.3f);
                yield break;
            }

            statusText.text = "Analyzing...";

            // Detect BPM if not provided
            int bpm = estimatedBpm;
            if (bpm <= 0)
            {
                float[] mono = AudioFileLoader.GetMonoSamples(clip);
                bpm = BpmDetector.DetectBpm(mono, clip.frequency);
            }

            clip.name = title;

            var config = new LevelConfig
            {
                id = levelId,
                name = levelConfig.name,
                songTitle = title,
                description = levelConfig.description,
                difficulty = levelConfig.difficulty,
                genre = levelConfig.genre,
                bpm = bpm,
                duration = clip.length,
                scrollSpeed = levelConfig.scrollSpeed,
                noteDensity = levelConfig.noteDensity,
                holdProbability = levelConfig.holdProbability,
                unlockScore = levelConfig.unlockScore,
                useExternalAudio = true
            };

            Close();
            SceneFlowManager.Instance.StartWithExternalAudio(clip, config);
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}
