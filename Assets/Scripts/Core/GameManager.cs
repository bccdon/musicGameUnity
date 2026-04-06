using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PulseHighway.Game;
using PulseHighway.Gameplay;
using PulseHighway.Audio;
using PulseHighway.UI;

namespace PulseHighway.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }
        public bool IsPlaying { get; private set; }
        public float CurrentTime { get; private set; }

        private AudioSource audioSource;
        private LevelConfig levelConfig;
        private ChartData chart;
        private NoteSpawner noteSpawner;
        private HighwayBuilder highwayBuilder;
        private HitLineBuilder hitLineBuilder;
        private HitEffectPool hitEffectPool;
        private GameplayHUD hud;
        private HashSet<int> hitNoteIndices = new HashSet<int>();
        private Dictionary<int, float> activeHolds = new Dictionary<int, float>();
        private bool songStarted;

        // Constants matching original
        public const int LANE_COUNT = 5;
        public const float LANE_WIDTH = 2.5f;
        public const float HIGHWAY_LENGTH = 120f;
        public const float NOTE_SPEED = 50f;
        public const float HIT_LINE_Z = 0f;
        public const float LOOK_AHEAD = 2.4f;
        public const float HOLD_TICK_INTERVAL = 0.1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private AudioClip externalClip;

        public void Initialize(int levelId)
        {
            State = new GameState { currentLevelId = levelId };
            levelConfig = LevelDefinitions.GetLevel(levelId);
            State.currentLevel = levelConfig;
            SetGameplayCamera();
            StartCoroutine(LoadAndStartGame());
        }

        /// <summary>
        /// Initialize with a pre-loaded external AudioClip (Quick Play or Online Music).
        /// </summary>
        public void InitializeWithAudio(AudioClip clip, LevelConfig config)
        {
            externalClip = clip;
            levelConfig = config;
            levelConfig.useExternalAudio = true;
            State = new GameState { currentLevelId = config.id };
            State.currentLevel = levelConfig;
            SetGameplayCamera();
            StartCoroutine(LoadAndStartGame());
        }

        private void SetGameplayCamera()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 8f, 5f);
                cam.transform.LookAt(new Vector3(0f, 0f, -40f));
            }
        }

        private IEnumerator LoadAndStartGame()
        {
            // Build highway
            var highwayGO = new GameObject("Highway");
            highwayGO.transform.SetParent(transform);
            highwayBuilder = highwayGO.AddComponent<HighwayBuilder>();
            highwayBuilder.Build();

            // Build hit line
            var hitLineGO = new GameObject("HitLine");
            hitLineGO.transform.SetParent(transform);
            hitLineBuilder = hitLineGO.AddComponent<HitLineBuilder>();
            hitLineBuilder.Build();

            // Show loading HUD
            var hudGO = new GameObject("GameplayHUD");
            hudGO.transform.SetParent(transform);
            hud = hudGO.AddComponent<GameplayHUD>();
            hud.Build(levelConfig);

            AudioClip clip;

            if (levelConfig.useExternalAudio && externalClip != null)
            {
                // External audio path — skip generation
                hud.ShowLoading("Analyzing audio...");
                yield return null;
                clip = externalClip;
            }
            else
            {
                // Procedural generation path (original)
                hud.ShowLoading("Generating music...");
                yield return null;

                var audioEngine = ProceduralAudioEngine.Instance;
                clip = audioEngine.GenerateTrack(levelConfig);
                yield return null;
            }

            hud.ShowLoading("Analyzing audio...");
            yield return null;

            // Analyze audio + generate chart (works for both paths)
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            // Detect BPM for external audio
            int bpm = levelConfig.bpm;
            if (levelConfig.useExternalAudio && bpm <= 0)
            {
                hud.ShowLoading("Detecting BPM...");
                yield return null;
                float[] mono = AudioFileLoader.GetMonoSamples(clip);
                bpm = BpmDetector.DetectBpm(mono, clip.frequency);
                levelConfig.bpm = bpm;
            }

            hud.ShowLoading("Generating chart...");
            yield return null;

            var analyzer = new AudioAnalyzer();
            var analysisResult = analyzer.Analyze(samples, clip.frequency, bpm);

            // Set duration from actual clip for external audio
            if (levelConfig.useExternalAudio)
                levelConfig.duration = clip.length;

            var chartGen = new ChartGenerator();
            chart = chartGen.Generate(analysisResult, levelConfig);
            State.currentChart = chart;

            yield return null;

            hud.ShowLoading("Ready!");
            yield return new WaitForSeconds(0.5f);
            hud.HideLoading();

            // Setup audio source
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = 0.8f;
            audioSource.playOnAwake = false;

            // Setup note spawner
            var spawnerGO = new GameObject("NoteSpawner");
            spawnerGO.transform.SetParent(transform);
            noteSpawner = spawnerGO.AddComponent<NoteSpawner>();
            noteSpawner.Initialize(chart);

            // Setup hit effects
            var effectsGO = new GameObject("HitEffects");
            effectsGO.transform.SetParent(transform);
            hitEffectPool = effectsGO.AddComponent<HitEffectPool>();
            hitEffectPool.Initialize();

            // Enable input
            Input.InputManager.Instance.SetEnabled(true);
            Input.InputManager.Instance.OnInput += HandleInput;

            // Start playing
            audioSource.Play();
            songStarted = true;
            IsPlaying = true;
        }

        private void Update()
        {
            if (!IsPlaying || !songStarted) return;

            CurrentTime = audioSource.time;

            noteSpawner.UpdateNotes(CurrentTime);
            CheckMissedNotes();
            UpdateHolds();

            // Check if song ended
            if (CurrentTime >= levelConfig.duration - 0.1f || !audioSource.isPlaying)
            {
                if (audioSource.time > 1f)
                    EndGame();
            }
        }

        private void HandleInput(Input.InputEvent inputEvent)
        {
            if (!IsPlaying) return;

            if (inputEvent.type == Input.InputEventType.Press)
            {
                TryHitNote(inputEvent.lane);
                if (highwayBuilder != null)
                    highwayBuilder.FlashLane(inputEvent.lane);
            }
        }

        private void TryHitNote(int lane)
        {
            float currentTime = CurrentTime;
            int bestNoteIndex = -1;
            float bestTimeDiff = float.MaxValue;

            for (int i = 0; i < chart.notes.Length; i++)
            {
                if (hitNoteIndices.Contains(i)) continue;
                if (activeHolds.ContainsKey(i)) continue;

                var note = chart.notes[i];
                if (note.lane != lane) continue;

                float timeDiff = Mathf.Abs(note.time - currentTime);
                if (timeDiff <= TimingWindows.Good && timeDiff < bestTimeDiff)
                {
                    bestTimeDiff = timeDiff;
                    bestNoteIndex = i;
                }
            }

            if (bestNoteIndex >= 0)
            {
                var note = chart.notes[bestNoteIndex];
                Judgment judgment = TimingWindows.GetJudgment(bestTimeDiff);

                hitNoteIndices.Add(bestNoteIndex);
                noteSpawner.HideNote(bestNoteIndex);

                int points = ScoreCalculator.CalculateHitScore(judgment, State.combo);
                State.score += points;

                if (judgment != Judgment.Miss)
                {
                    State.combo++;
                    if (State.combo > State.maxCombo) State.maxCombo = State.combo;
                }
                else
                {
                    State.combo = 0;
                }

                State.judgmentCounts[judgment]++;

                hud.ShowJudgment(judgment, lane);
                hud.UpdateScore(State.score, State.combo);

                // Flying score text
                if (points > 0)
                    hud.SpawnFlyingScore(points, judgment, lane);

                // Particle burst
                Color laneColor = Visual.MaterialFactory.GetLaneColor(lane);
                hitEffectPool.TriggerBurst(lane, laneColor);

                // Camera effects based on judgment
                if (judgment == Judgment.Perfect)
                {
                    CameraShake.Instance?.Shake(0.25f);
                    CameraShake.Instance?.PushForward(-0.4f);
                    hud.FlashScreen(laneColor, 0.12f);
                }
                else if (judgment == Judgment.Great)
                {
                    CameraShake.Instance?.Shake(0.12f);
                    hud.FlashScreen(laneColor, 0.06f);
                }
                else if (judgment == Judgment.Good)
                {
                    CameraShake.Instance?.Shake(0.06f);
                }

                // Start hold tracking
                if (note.type == NoteType.Hold)
                {
                    activeHolds[bestNoteIndex] = currentTime;
                    noteSpawner.StartHold(bestNoteIndex);
                }
            }
        }

        private void CheckMissedNotes()
        {
            float currentTime = CurrentTime;
            for (int i = 0; i < chart.notes.Length; i++)
            {
                if (hitNoteIndices.Contains(i)) continue;
                if (activeHolds.ContainsKey(i)) continue;

                var note = chart.notes[i];
                if (note.time < currentTime - TimingWindows.Good)
                {
                    hitNoteIndices.Add(i);
                    State.combo = 0;
                    State.judgmentCounts[Judgment.Miss]++;
                    hud.ShowJudgment(Judgment.Miss, note.lane);
                    hud.UpdateScore(State.score, State.combo);
                }
            }
        }

        private void UpdateHolds()
        {
            float currentTime = CurrentTime;
            var toRemove = new List<int>();
            var tickUpdates = new List<KeyValuePair<int, float>>();
            var pressedLanes = Input.InputManager.Instance.PressedLanes;

            foreach (var kvp in activeHolds)
            {
                int noteIndex = kvp.Key;
                float lastTick = kvp.Value;
                var note = chart.notes[noteIndex];
                float holdEnd = note.time + note.duration;

                if (!pressedLanes.Contains(note.lane))
                {
                    if (currentTime < holdEnd - 0.1f)
                    {
                        State.combo = 0;
                        State.judgmentCounts[Judgment.Miss]++;
                        hud.ShowJudgment(Judgment.Miss, note.lane);
                        hud.UpdateScore(State.score, State.combo);
                    }
                    toRemove.Add(noteIndex);
                    continue;
                }

                // Hold ticks — collect updates, don't modify dict during iteration
                if (currentTime - lastTick >= HOLD_TICK_INTERVAL)
                {
                    int tickPoints = ScoreCalculator.CalculateHoldTick(State.combo);
                    State.score += tickPoints;
                    tickUpdates.Add(new KeyValuePair<int, float>(noteIndex, currentTime));
                    hud.UpdateScore(State.score, State.combo);
                }

                if (currentTime >= holdEnd)
                {
                    State.combo++;
                    if (State.combo > State.maxCombo) State.maxCombo = State.combo;
                    State.judgmentCounts[Judgment.Perfect]++;
                    State.score += 50;
                    hud.ShowJudgment(Judgment.Perfect, note.lane);
                    hud.UpdateScore(State.score, State.combo);
                    toRemove.Add(noteIndex);
                }
            }

            // Apply tick updates after iteration
            foreach (var update in tickUpdates)
                activeHolds[update.Key] = update.Value;

            foreach (int idx in toRemove)
            {
                activeHolds.Remove(idx);
                noteSpawner.EndHold(idx);
            }
        }

        private void EndGame()
        {
            IsPlaying = false;
            songStarted = false;

            Cleanup();

            if (audioSource != null && audioSource.isPlaying)
                audioSource.Stop();

            // Calculate accuracy based on judgments (not score ratio)
            int totalNotes = chart.notes.Length;
            float accuracy = 0f;
            if (totalNotes > 0)
            {
                float weightedHits =
                    State.judgmentCounts[Judgment.Perfect] * 1.0f +
                    State.judgmentCounts[Judgment.Great] * 0.75f +
                    State.judgmentCounts[Judgment.Good] * 0.5f;
                accuracy = weightedHits / totalNotes;
            }
            accuracy = Mathf.Clamp01(accuracy);

            Rank rank = ScoreCalculator.CalculateRank(accuracy);

            // Save progress
            var levelManager = LevelManager.Instance;
            bool newUnlock = false;
            int unlockedId = -1;
            if (levelManager != null)
            {
                var result = levelManager.CompleteLevel(levelConfig.id, State.score, State.maxCombo, accuracy);
                newUnlock = result.newLevelUnlocked;
                unlockedId = result.unlockedLevelId;
            }

            var gameResult = new GameResult
            {
                levelId = levelConfig.id,
                levelName = levelConfig.name,
                score = State.score,
                combo = State.combo,
                maxCombo = State.maxCombo,
                perfectCount = State.judgmentCounts[Judgment.Perfect],
                greatCount = State.judgmentCounts[Judgment.Great],
                goodCount = State.judgmentCounts[Judgment.Good],
                missCount = State.judgmentCounts[Judgment.Miss],
                accuracy = accuracy,
                rank = rank,
                newLevelUnlocked = newUnlock,
                unlockedLevelId = unlockedId
            };

            // Delay then show results (use Invoke to survive object destruction)
            SceneFlowManager.Instance.ShowResultsDelayed(gameResult, 1.5f);
        }

        private void Cleanup()
        {
            if (Input.InputManager.Instance != null)
            {
                Input.InputManager.Instance.OnInput -= HandleInput;
                Input.InputManager.Instance.SetEnabled(false);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
            Cleanup();
        }
    }
}
