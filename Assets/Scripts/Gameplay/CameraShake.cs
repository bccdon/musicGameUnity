using UnityEngine;

namespace PulseHighway.Gameplay
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private float shakeIntensity;
        private Vector3 basePosition;
        private bool basePositionSet;

        private void Awake()
        {
            Instance = this;
        }

        private void LateUpdate()
        {
            if (!basePositionSet)
            {
                basePosition = transform.localPosition;
                basePositionSet = true;
            }

            if (shakeIntensity > 0.01f)
            {
                float offsetX = Random.Range(-1f, 1f) * shakeIntensity;
                float offsetY = Random.Range(-1f, 1f) * shakeIntensity;
                transform.localPosition = basePosition + new Vector3(offsetX, offsetY, 0f);

                shakeIntensity *= 0.9f; // Decay
            }
            else
            {
                transform.localPosition = basePosition;
                shakeIntensity = 0f;
            }
        }

        public void Shake(float intensity = 0.3f)
        {
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        }

        public void UpdateBasePosition(Vector3 newBase)
        {
            basePosition = newBase;
            basePositionSet = true;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
