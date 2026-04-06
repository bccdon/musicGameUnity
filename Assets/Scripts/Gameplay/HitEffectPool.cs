using UnityEngine;
using PulseHighway.Core;
using PulseHighway.Visual;

namespace PulseHighway.Gameplay
{
    public class HitEffectPool : MonoBehaviour
    {
        private ParticleSystem[] effectPool;
        private int poolIndex;
        private const int POOL_SIZE = 15;

        private float totalWidth;
        private float startX;

        public void Initialize()
        {
            totalWidth = GameManager.LANE_COUNT * GameManager.LANE_WIDTH;
            startX = -totalWidth * 0.5f + GameManager.LANE_WIDTH * 0.5f;

            effectPool = new ParticleSystem[POOL_SIZE];

            for (int i = 0; i < POOL_SIZE; i++)
            {
                var go = new GameObject($"HitEffect_{i}");
                go.transform.SetParent(transform);

                var ps = go.AddComponent<ParticleSystem>();
                ConfigureParticleSystem(ps);
                effectPool[i] = ps;
            }
        }

        private void ConfigureParticleSystem(ParticleSystem ps)
        {
            var main = ps.main;
            main.maxParticles = 60;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 22f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
            main.gravityModifier = 1.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.loop = false;

            // Shape: hemisphere burst
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.3f;

            // Emission: burst
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 35, 50)
            });

            // Color over lifetime: fade out
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.5f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = grad;

            // Size over lifetime: shrink
            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f), new Keyframe(1f, 0f)));

            // Renderer
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = MaterialFactory.CreateParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        public void TriggerBurst(int lane, Color color)
        {
            if (effectPool == null || effectPool.Length == 0) return;

            var ps = effectPool[poolIndex];
            poolIndex = (poolIndex + 1) % POOL_SIZE;

            float x = startX + lane * GameManager.LANE_WIDTH;
            ps.transform.position = new Vector3(x, 0.5f, GameManager.HIT_LINE_Z);

            // Set color
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);

            ps.Clear();
            ps.Play();
        }
    }
}
