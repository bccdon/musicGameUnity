using UnityEngine;

namespace PulseHighway.Visual
{
    public class NeonGlowEffect : MonoBehaviour
    {
        public Color baseColor = Color.cyan;
        public float pulseSpeed = 2f;
        public float minIntensity = 1.5f;
        public float maxIntensity = 3.5f;

        private MeshRenderer meshRenderer;
        private Material material;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                material = meshRenderer.material;
            }
        }

        private void Update()
        {
            if (material == null) return;

            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);

            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", baseColor * intensity);
        }
    }
}
