using UnityEngine;
using UnityEngine.EventSystems;

namespace PulseHighway.Core
{
    public class Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            DontDestroyOnLoad(gameObject);

            // EventSystem is required for all UI interaction
            if (FindObjectOfType<EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.transform.SetParent(transform);
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }

            // Create singletons (order matters: ProgressStorage before LevelManager)
            EnsureSingleton<SceneFlowManager>("SceneFlowManager");
            EnsureSingleton<Input.InputManager>("InputManager");
            EnsureSingleton<Storage.ProgressStorage>("ProgressStorage");
            EnsureSingleton<Game.LevelManager>("LevelManager");
            EnsureSingleton<Audio.ProceduralAudioEngine>("AudioEngine");

            // Setup camera
            SetupCamera();

            // Setup environment
            var envBuilder = gameObject.AddComponent<Gameplay.EnvironmentBuilder>();
            envBuilder.Build();

            // Setup post-processing
            var postProcess = gameObject.AddComponent<Visual.PostProcessingSetup>();
            postProcess.Setup();

            // Setup rendering
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.06f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.05f, 0.03f, 0.08f);

            // Navigate to main menu
            SceneFlowManager.Instance.GoToMainMenu();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGO = new GameObject("MainCamera");
                camGO.tag = "MainCamera";
                cam = camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>();
                camGO.transform.SetParent(transform);
            }

            cam.transform.position = new Vector3(0f, 8f, 5f);
            cam.transform.LookAt(new Vector3(0f, 0f, -40f));
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.03f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 500f;
            cam.allowHDR = true;

            // Add camera shake component
            cam.gameObject.AddComponent<Gameplay.CameraShake>();
        }

        private T EnsureSingleton<T>(string name) where T : MonoBehaviour
        {
            var existing = FindObjectOfType<T>();
            if (existing != null) return existing;

            var go = new GameObject(name);
            go.transform.SetParent(transform);
            return go.AddComponent<T>();
        }
    }
}
