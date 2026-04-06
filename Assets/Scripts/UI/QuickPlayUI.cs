using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using PulseHighway.Core;
using PulseHighway.Game;
using PulseHighway.Audio;

namespace PulseHighway.UI
{
    public class QuickPlayUI : MonoBehaviour
    {
        private Canvas canvas;
        private Text fileNameText;
        private Text statusText;
        private Button analyzeButton;
        private GameObject loadingPanel;
        private Text loadingText;
        private string selectedFilePath;
        private string selectedDifficulty = "medium";
        private Button[] difficultyButtons = new Button[4];
        private Image[] difficultyBgs = new Image[4];

        private static readonly Color SelectedColor = UIBuilder.HexColor("#00ccff");
        private static readonly Color UnselectedColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        public void Build()
        {
            canvas = UIBuilder.CreateCanvas("QuickPlayCanvas", transform);

            // Background
            UIBuilder.CreatePanel(canvas.transform, new Color(0.01f, 0.01f, 0.03f, 0.92f),
                Vector2.zero, Vector2.one);

            // Top bar
            var topBar = UIBuilder.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0.6f),
                new Vector2(0, 0.91f), new Vector2(1, 1));

            // Back button
            var backBtn = UIBuilder.CreateButton(topBar.transform, "< BACK",
                new Color(1f, 0f, 0.33f, 0.3f), UIBuilder.HexColor("#ff0055"),
                new Vector2(120, 40), Vector2.zero, 18,
                () => SceneFlowManager.Instance.GoToMainMenu());
            var backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0.5f);
            backRect.anchorMax = new Vector2(0, 0.5f);
            backRect.pivot = new Vector2(0, 0.5f);
            backRect.anchoredPosition = new Vector2(20, 0);

            UIBuilder.CreateText(topBar.transform, "QUICK PLAY",
                38, UIBuilder.HexColor("#ff0055"), TextAnchor.MiddleCenter, FontStyle.Bold);

            // Main content area
            var content = new GameObject("Content");
            content.transform.SetParent(canvas.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.15f, 0.15f);
            contentRect.anchorMax = new Vector2(0.85f, 0.85f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            // Drop zone / file area
            var dropZone = UIBuilder.CreatePanel(content.transform,
                new Color(0.05f, 0.05f, 0.1f, 0.9f), new Vector2(0, 0.55f), new Vector2(1, 1));
            var dropOutline = dropZone.AddComponent<Outline>();
            dropOutline.effectColor = new Color(0f, 0.8f, 1f, 0.3f);
            dropOutline.effectDistance = new Vector2(2, 2);

            // Drop zone text
            UIBuilder.CreateTextSized(dropZone.transform,
                "SELECT AN AUDIO FILE",
                28, new Color(1f, 1f, 1f, 0.5f),
                new Vector2(500, 40), new Vector2(0, 40),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            UIBuilder.CreateTextSized(dropZone.transform,
                "Supported: MP3, WAV, OGG",
                16, new Color(1f, 1f, 1f, 0.3f),
                new Vector2(400, 25), new Vector2(0, 5),
                TextAnchor.MiddleCenter);

            // File name display
            fileNameText = UIBuilder.CreateTextSized(dropZone.transform,
                "No file selected",
                20, UIBuilder.HexColor("#00ccff"),
                new Vector2(500, 30), new Vector2(0, -30),
                TextAnchor.MiddleCenter, FontStyle.Italic);

            // Path input field — paste or type a file path
            var pathInputGO = new GameObject("PathInput");
            pathInputGO.transform.SetParent(dropZone.transform, false);
            var pathInputRect = pathInputGO.AddComponent<RectTransform>();
            pathInputRect.sizeDelta = new Vector2(450, 32);
            pathInputRect.anchoredPosition = new Vector2(0, -65);
            var pathInputBg = pathInputGO.AddComponent<Image>();
            pathInputBg.color = new Color(0.06f, 0.06f, 0.1f, 1f);

            var pathInput = pathInputGO.AddComponent<InputField>();
            pathInput.textComponent = UIBuilder.CreateText(pathInputGO.transform, "",
                13, Color.white, TextAnchor.MiddleLeft);
            pathInput.textComponent.GetComponent<RectTransform>().offsetMin = new Vector2(8, 0);
            pathInput.textComponent.GetComponent<RectTransform>().offsetMax = new Vector2(-8, 0);
            var pathPlaceholder = UIBuilder.CreateText(pathInputGO.transform,
                "Paste file path here (e.g. C:/Music/song.mp3)...",
                13, new Color(1f, 1f, 1f, 0.3f), TextAnchor.MiddleLeft);
            pathPlaceholder.GetComponent<RectTransform>().offsetMin = new Vector2(8, 0);
            pathPlaceholder.GetComponent<RectTransform>().offsetMax = new Vector2(-8, 0);
            pathInput.placeholder = pathPlaceholder;
            pathInput.onEndEdit.AddListener(path => {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    OnFileSelected(path);
                else if (!string.IsNullOrEmpty(path))
                {
                    statusText.text = "File not found";
                    statusText.color = new Color(1f, 0.3f, 0.3f);
                }
            });

            // Browse button (opens Music folder)
            UIBuilder.CreateButton(dropZone.transform, "OPEN MUSIC FOLDER",
                new Color(0f, 0.8f, 1f, 0.2f), UIBuilder.HexColor("#00ccff"),
                new Vector2(250, 40), new Vector2(0, -110), 16,
                () => OpenFileBrowser());

            // Difficulty selector
            var diffArea = new GameObject("DifficultyArea");
            diffArea.transform.SetParent(content.transform, false);
            var diffRect = diffArea.AddComponent<RectTransform>();
            diffRect.anchorMin = new Vector2(0, 0.25f);
            diffRect.anchorMax = new Vector2(1, 0.5f);
            diffRect.offsetMin = Vector2.zero;
            diffRect.offsetMax = Vector2.zero;

            UIBuilder.CreateTextSized(diffArea.transform, "DIFFICULTY",
                20, new Color(1f, 1f, 1f, 0.6f),
                new Vector2(200, 30), new Vector2(0, 30),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            string[] diffs = { "easy", "medium", "hard", "expert" };
            string[] labels = { "EASY", "MEDIUM", "HARD", "EXPERT" };
            Color[] diffColors = {
                UIBuilder.HexColor("#00ccff"),
                UIBuilder.HexColor("#00ff99"),
                UIBuilder.HexColor("#ff9900"),
                UIBuilder.HexColor("#ff0055")
            };

            float btnWidth = 140f;
            float totalW = btnWidth * 4 + 30;
            float startX = -totalW / 2f + btnWidth / 2f;

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                string diff = diffs[i];
                bool isSelected = diff == selectedDifficulty;

                var btn = UIBuilder.CreateButton(diffArea.transform, labels[i],
                    isSelected ? new Color(diffColors[i].r * 0.2f, diffColors[i].g * 0.2f, diffColors[i].b * 0.2f, 0.9f) : UnselectedColor,
                    diffColors[i],
                    new Vector2(btnWidth, 45), new Vector2(startX + i * (btnWidth + 10), -15), 16,
                    () => SelectDifficulty(idx, diffs[idx]));

                difficultyButtons[i] = btn;
                difficultyBgs[i] = btn.GetComponent<Image>();
            }

            // Analyze & Play button
            analyzeButton = UIBuilder.CreateButton(content.transform, "ANALYZE & PLAY",
                new Color(0f, 1f, 0.6f, 0.2f), UIBuilder.HexColor("#00ff99"),
                new Vector2(300, 60), new Vector2(0, -220), 24,
                () => StartAnalysis());

            // Status text
            statusText = UIBuilder.CreateTextSized(content.transform, "",
                16, new Color(1f, 0.3f, 0.3f),
                new Vector2(500, 25), new Vector2(0, -260),
                TextAnchor.MiddleCenter);

            // Loading overlay (hidden)
            BuildLoadingPanel();
        }

        private void BuildLoadingPanel()
        {
            loadingPanel = UIBuilder.CreatePanel(canvas.transform, new Color(0, 0, 0, 0.85f),
                Vector2.zero, Vector2.one);

            loadingText = UIBuilder.CreateTextSized(loadingPanel.transform, "Loading...",
                32, UIBuilder.HexColor("#00ccff"),
                new Vector2(600, 50), new Vector2(0, 20),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            UIBuilder.CreateTextSized(loadingPanel.transform, "QUICK PLAY",
                18, new Color(1f, 1f, 1f, 0.3f),
                new Vector2(300, 25), new Vector2(0, -20),
                TextAnchor.MiddleCenter);

            loadingPanel.SetActive(false);
        }

        private void OpenFileBrowser()
        {
            // Open the music folder and scan for files
            ShowPathInput();
        }

        private void ShowPathInput()
        {
            // Create a music folder if it doesn't exist
            string musicDir = Path.Combine(Application.persistentDataPath, "Music");
            if (!Directory.Exists(musicDir))
                Directory.CreateDirectory(musicDir);

            // Open the folder so user can place files
            Application.OpenURL("file:///" + musicDir.Replace("\\", "/"));

            // Scan for existing music files
            ScanMusicFolder(musicDir);
        }

        private void ScanMusicFolder(string musicDir)
        {
            var files = new System.Collections.Generic.List<string>();
            foreach (string ext in AudioFileLoader.SupportedExtensions)
            {
                string pattern = "*" + ext;
                foreach (string file in Directory.GetFiles(musicDir, pattern))
                    files.Add(file);
            }

            if (files.Count > 0)
            {
                // Pick the first found file (user can put their music in the folder)
                OnFileSelected(files[0]);
                if (files.Count > 1)
                    statusText.text = $"Found {files.Count} files. Using: {Path.GetFileName(files[0])}";
            }
            else
            {
                statusText.text = $"Place audio files in:\n{musicDir}";
                statusText.color = UIBuilder.HexColor("#ff9900");
            }
        }

        private void OnFileSelected(string path)
        {
            selectedFilePath = path;
            fileNameText.text = Path.GetFileName(path);
            fileNameText.color = UIBuilder.HexColor("#00ff99");
            statusText.text = "";
        }

        private void SelectDifficulty(int index, string difficulty)
        {
            selectedDifficulty = difficulty;

            Color[] diffColors = {
                UIBuilder.HexColor("#00ccff"),
                UIBuilder.HexColor("#00ff99"),
                UIBuilder.HexColor("#ff9900"),
                UIBuilder.HexColor("#ff0055")
            };

            for (int i = 0; i < 4; i++)
            {
                bool sel = i == index;
                if (difficultyBgs[i] != null)
                {
                    difficultyBgs[i].color = sel
                        ? new Color(diffColors[i].r * 0.2f, diffColors[i].g * 0.2f, diffColors[i].b * 0.2f, 0.9f)
                        : UnselectedColor;
                }
            }
        }

        private void StartAnalysis()
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                statusText.text = "Please select an audio file first";
                statusText.color = new Color(1f, 0.3f, 0.3f);
                return;
            }

            if (!File.Exists(selectedFilePath))
            {
                statusText.text = "File not found";
                statusText.color = new Color(1f, 0.3f, 0.3f);
                return;
            }

            StartCoroutine(AnalyzeAndPlay());
        }

        private IEnumerator AnalyzeAndPlay()
        {
            loadingPanel.SetActive(true);
            loadingText.text = "Loading audio...";
            yield return null;

            AudioClip clip = null;
            string error = null;

            yield return AudioFileLoader.LoadFromFile(selectedFilePath, (c, e) => { clip = c; error = e; });

            if (clip == null)
            {
                loadingPanel.SetActive(false);
                statusText.text = error ?? "Failed to load audio";
                statusText.color = new Color(1f, 0.3f, 0.3f);
                yield break;
            }

            loadingText.text = "Detecting BPM...";
            yield return null;

            float[] mono = AudioFileLoader.GetMonoSamples(clip);
            int bpm = BpmDetector.DetectBpm(mono, clip.frequency);

            loadingText.text = $"BPM: {bpm} | Preparing...";
            yield return null;

            // Build a config for the custom track
            var config = new LevelConfig
            {
                id = 0,
                name = Path.GetFileNameWithoutExtension(selectedFilePath),
                songTitle = clip.name,
                description = "Quick Play",
                difficulty = selectedDifficulty,
                genre = "custom",
                bpm = bpm,
                duration = clip.length,
                scrollSpeed = GetScrollSpeed(selectedDifficulty),
                noteDensity = GetNoteDensity(selectedDifficulty),
                holdProbability = GetHoldProbability(selectedDifficulty),
                unlockScore = 0,
                useExternalAudio = true
            };

            loadingPanel.SetActive(false);

            // Start the game with external audio
            SceneFlowManager.Instance.StartWithExternalAudio(clip, config);
        }

        private float GetScrollSpeed(string diff)
        {
            return diff switch
            {
                "easy" => 35f,
                "medium" => 50f,
                "hard" => 70f,
                "expert" => 90f,
                _ => 50f
            };
        }

        private float GetNoteDensity(string diff)
        {
            return diff switch
            {
                "easy" => 0.7f,
                "medium" => 1.0f,
                "hard" => 1.5f,
                "expert" => 2.5f,
                _ => 1.0f
            };
        }

        private float GetHoldProbability(string diff)
        {
            return diff switch
            {
                "easy" => 0.05f,
                "medium" => 0.15f,
                "hard" => 0.25f,
                "expert" => 0.4f,
                _ => 0.15f
            };
        }
    }
}
