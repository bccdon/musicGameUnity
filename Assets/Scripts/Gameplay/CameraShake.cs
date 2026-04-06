using UnityEngine;

namespace PulseHighway.Gameplay
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private float shakeIntensity;
        private float zPush;
        private Vector3 basePosition;
        private bool basePositionSet;

        private void Awake() { Instance = this; }

        private void LateUpdate()
        {
            if (!basePositionSet)
            {
                basePosition = transform.localPosition;
                basePositionSet = true;
            }

            Vector3 offset = Vector3.zero;

            // Perlin noise shake (smoother than random)
            if (shakeIntensity > 0.005f)
            {
                float t = Time.time * 25f;
                float x = (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * shakeIntensity;
                float y = (Mathf.PerlinNoise(0f, t) - 0.5f) * 2f * shakeIntensity;
                offset = new Vector3(x, y, 0f);
                shakeIntensity *= 0.94f; // Slower decay (was 0.9)
            }
            else
            {
                shakeIntensity = 0f;
            }

            // Z-push (camera zooms forward on hit, springs back)
            if (Mathf.Abs(zPush) > 0.01f)
            {
                offset.z += zPush;
                zPush *= 0.92f;
            }
            else
            {
                zPush = 0f;
            }

            transform.localPosition = basePosition + offset;
        }

        public void Shake(float intensity = 0.3f)
        {
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        }

        public void PushForward(float amount = -0.5f)
        {
            zPush += amount;
        }

        public void UpdateBasePosition(Vector3 newBase)
        {
            basePosition = newBase;
            basePositionSet = true;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
