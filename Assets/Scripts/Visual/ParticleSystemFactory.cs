using UnityEngine;

namespace PulseHighway.Visual
{
    public static class ParticleSystemFactory
    {
        public static ParticleSystem CreateComboFireEffect(Transform parent, int lane)
        {
            var go = new GameObject($"ComboFire_{lane}");
            go.transform.SetParent(parent);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 50;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
            main.gravityModifier = -1f; // Float up
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.loop = true;

            Color laneColor = MaterialFactory.GetLaneColor(lane);
            main.startColor = new ParticleSystem.MinMaxGradient(laneColor, Color.yellow);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;

            var emission = ps.emission;
            emission.rateOverTime = 30;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(laneColor, 0f),
                    new GradientColorKey(Color.yellow, 0.5f),
                    new GradientColorKey(new Color(1f, 0.3f, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0.5f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = MaterialFactory.CreateParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            ps.Stop();
            return ps;
        }

        public static ParticleSystem CreateBeatPulseEffect(Transform parent)
        {
            var go = new GameObject("BeatPulse");
            go.transform.SetParent(parent);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 100;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.gravityModifier = 0;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.loop = false;

            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.5f, 0f, 1f, 0.5f),
                new Color(0f, 0.8f, 1f, 0.5f));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 8f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 50)
            });

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = MaterialFactory.CreateParticleMaterial();

            return ps;
        }

        public static ParticleSystem CreateHighwayEdgeSpark(Transform parent, bool leftSide)
        {
            var go = new GameObject(leftSide ? "LeftEdgeSpark" : "RightEdgeSpark");
            go.transform.SetParent(parent);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 30;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.gravityModifier = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.loop = true;

            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0f, 0.5f, 0.8f),
                new Color(0.5f, 0f, 1f, 0.8f));

            var emission = ps.emission;
            emission.rateOverTime = 10;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.1f, 0.5f, 60f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = MaterialFactory.CreateParticleMaterial();

            return ps;
        }
    }
}
