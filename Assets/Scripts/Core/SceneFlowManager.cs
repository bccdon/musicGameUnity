using UnityEngine;
using System.Collections;
using PulseHighway.Game;

namespace PulseHighway.Core
{
    public enum GameScreen
    {
        None,
        MainMenu,
        LevelSelect,
        Gameplay,
        Results
    }

    public class SceneFlowManager : MonoBehaviour
    {
        public static SceneFlowManager Instance { get; private set; }

        public GameScreen CurrentScreen { get; private set; } = GameScreen.None;

        private GameObject currentScreenRoot;
        private GameState pendingGameState;
        private GameResult pendingResult;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void GoToMainMenu()
        {
            DestroyCurrentScreen();
            CurrentScreen = GameScreen.MainMenu;

            currentScreenRoot = new GameObject("MainMenuScreen");
            var ui = currentScreenRoot.AddComponent<UI.MainMenuUI>();
            ui.Build();

            // Reset camera for menu
            SetMenuCamera();
        }

        public void GoToLevelSelect()
        {
            DestroyCurrentScreen();
            CurrentScreen = GameScreen.LevelSelect;

            currentScreenRoot = new GameObject("LevelSelectScreen");
            var ui = currentScreenRoot.AddComponent<UI.LevelSelectUI>();
            ui.Build();

            SetMenuCamera();
        }

        public void GoToQuickPlay()
        {
            DestroyCurrentScreen();
            CurrentScreen = GameScreen.MainMenu; // Reuse menu enum for now

            currentScreenRoot = new GameObject("QuickPlayScreen");
            var ui = currentScreenRoot.AddComponent<UI.QuickPlayUI>();
            ui.Build();

            SetMenuCamera();
        }

        public void StartLevel(int levelId)
        {
            DestroyCurrentScreen();
            CurrentScreen = GameScreen.Gameplay;

            pendingGameState = new GameState { currentLevelId = levelId };
            currentScreenRoot = new GameObject("GameplayScreen");
            var gm = currentScreenRoot.AddComponent<GameManager>();
            gm.Initialize(levelId);
        }

        public void StartWithExternalAudio(AudioClip clip, LevelConfig config)
        {
            DestroyCurrentScreen();
            CurrentScreen = GameScreen.Gameplay;

            currentScreenRoot = new GameObject("GameplayScreen");
            var gm = currentScreenRoot.AddComponent<GameManager>();
            gm.InitializeWithAudio(clip, config);
        }

        public void ShowResults(GameResult result)
        {
            DestroyCurrentScreen();
            CurrentScreen = GameScreen.Results;
            pendingResult = result;

            currentScreenRoot = new GameObject("ResultsScreen");
            var ui = currentScreenRoot.AddComponent<UI.ResultsScreenUI>();
            ui.Build(result);

            SetMenuCamera();
        }

        public void ShowResultsDelayed(GameResult result, float delay)
        {
            StartCoroutine(ShowResultsCoroutine(result, delay));
        }

        private IEnumerator ShowResultsCoroutine(GameResult result, float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowResults(result);
        }

        private void SetMenuCamera()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 9f, 14f);
                cam.transform.LookAt(new Vector3(0f, 0f, -20f));
            }
        }

        private void DestroyCurrentScreen()
        {
            if (currentScreenRoot != null)
            {
                Destroy(currentScreenRoot);
                currentScreenRoot = null;
            }
        }
    }

    public class GameResult
    {
        public int levelId;
        public string levelName;
        public int score;
        public int combo;
        public int maxCombo;
        public int perfectCount;
        public int greatCount;
        public int goodCount;
        public int missCount;
        public float accuracy;
        public Rank rank;
        public bool newLevelUnlocked;
        public int unlockedLevelId;
    }
}
