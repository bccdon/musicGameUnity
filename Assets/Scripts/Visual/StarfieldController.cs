using UnityEngine;

namespace PulseHighway.Visual
{
    public class StarfieldController : MonoBehaviour
    {
        private ParticleSystem starfield;
        private float beatIntensity;

        public void Initialize(ParticleSystem ps)
        {
            starfield = ps;
        }

        public void OnBeat(float intensity)
        {
            beatIntensity = Mathf.Max(beatIntensity, intensity);
        }

        private void Update()
        {
            if (starfield == null) return;

            // Decay beat intensity
            beatIntensity *= 0.95f;

            // Modulate starfield based on beat
            var main = starfield.main;
            float baseSize = 0.3f;
            float pulseSize = baseSize + beatIntensity * 0.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(pulseSize * 0.5f, pulseSize);
        }
    }
}
