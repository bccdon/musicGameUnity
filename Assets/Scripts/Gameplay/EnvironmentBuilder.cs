using UnityEngine;
using PulseHighway.Visual;

namespace PulseHighway.Gameplay
{
    public class EnvironmentBuilder : MonoBehaviour
    {
        private Light purpleLight;
        private Light cyanLight;
        private Light pinkSpot;

        public void Build()
        {
            BuildStarfield();
            BuildFloorGrid();
            BuildLighting();
            BuildSideBeams();
        }

        private void BuildStarfield()
        {
            var starGO = new GameObject("Starfield");
            starGO.transform.SetParent(transform);
            starGO.transform.position = new Vector3(0, 40, -50);

            var starfield = starGO.AddComponent<ParticleSystem>();

            // Stop default emission then configure
            starfield.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = starfield.main;
            main.maxParticles = 5000;
            main.startLifetime = 999f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.loop = false;
            main.gravityModifier = 0f;

            var colorGrad = new Gradient();
            colorGrad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0f, 0.8f, 1f), 0f),
                    new GradientColorKey(new Color(1f, 0f, 0.33f), 0.25f),
                    new GradientColorKey(new Color(1f, 1f, 0f), 0.5f),
                    new GradientColorKey(new Color(0f, 1f, 0.6f), 0.75f),
                    new GradientColorKey(new Color(0.7f, 0.3f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.6f, 0f),
                    new GradientAlphaKey(0.9f, 0.5f),
                    new GradientAlphaKey(0.6f, 1f)
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(colorGrad);

            var shape = starfield.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(250f, 100f, 350f);

            var emission = starfield.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 5000)
            });

            // Disable velocity module entirely to avoid mode conflicts
            var vel = starfield.velocityOverLifetime;
            vel.enabled = false;

            // Color over lifetime: twinkle
            var col = starfield.colorOverLifetime;
            col.enabled = true;
            var twinkle = new Gradient();
            twinkle.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.4f, 0f),
                    new GradientAlphaKey(1f, 0.3f),
                    new GradientAlphaKey(0.4f, 0.6f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
            col.color = twinkle;

            var renderer = starGO.GetComponent<ParticleSystemRenderer>();
            renderer.material = MaterialFactory.CreateStarfieldMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            starfield.Play();
        }

        private void BuildFloorGrid()
        {
            var gridGO = new GameObject("FloorGrid");
            gridGO.transform.SetParent(transform);

            var mf = gridGO.AddComponent<MeshFilter>();
            mf.mesh = MeshFactory.CreateGridMesh(400f, 80);

            var mr = gridGO.AddComponent<MeshRenderer>();
            mr.material = MaterialFactory.CreateGridMaterial();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            gridGO.transform.position = new Vector3(0f, -2f, -80f);
        }

        private void BuildLighting()
        {
            // Directional light — soft fill
            var dirLightGO = new GameObject("DirectionalLight");
            dirLightGO.transform.SetParent(transform);
            var dirLight = dirLightGO.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.6f, 0.5f, 0.8f);
            dirLight.intensity = 0.4f;
            dirLightGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            // Purple point light (left)
            var purpleLightGO = new GameObject("PurpleLight");
            purpleLightGO.transform.SetParent(transform);
            purpleLight = purpleLightGO.AddComponent<Light>();
            purpleLight.type = LightType.Point;
            purpleLight.color = new Color(0.6f, 0.1f, 1f);
            purpleLight.intensity = 4f;
            purpleLight.range = 80f;
            purpleLightGO.transform.position = new Vector3(-15f, 10f, -20f);

            // Cyan point light (right)
            var cyanLightGO = new GameObject("CyanLight");
            cyanLightGO.transform.SetParent(transform);
            cyanLight = cyanLightGO.AddComponent<Light>();
            cyanLight.type = LightType.Point;
            cyanLight.color = new Color(0f, 0.8f, 1f);
            cyanLight.intensity = 4f;
            cyanLight.range = 80f;
            cyanLightGO.transform.position = new Vector3(15f, 10f, -20f);

            // Pink spot from above — dramatic
            var pinkSpotGO = new GameObject("PinkSpot");
            pinkSpotGO.transform.SetParent(transform);
            pinkSpot = pinkSpotGO.AddComponent<Light>();
            pinkSpot.type = LightType.Spot;
            pinkSpot.color = new Color(1f, 0f, 0.5f);
            pinkSpot.intensity = 3f;
            pinkSpot.range = 100f;
            pinkSpot.spotAngle = 100f;
            pinkSpotGO.transform.position = new Vector3(0f, 20f, -50f);
            pinkSpotGO.transform.rotation = Quaternion.Euler(35f, 0f, 0f);

            // Back blue wash
            var backLightGO = new GameObject("BackLight");
            backLightGO.transform.SetParent(transform);
            var backLight = backLightGO.AddComponent<Light>();
            backLight.type = LightType.Point;
            backLight.color = new Color(0.1f, 0.2f, 1f);
            backLight.intensity = 2f;
            backLight.range = 120f;
            backLightGO.transform.position = new Vector3(0f, 5f, -100f);
        }

        private void BuildSideBeams()
        {
            // Vertical neon beams on the sides for atmosphere
            float[] xPositions = { -20f, -16f, 16f, 20f };
            Color[] beamColors = {
                new Color(0.6f, 0f, 1f, 0.15f),
                new Color(1f, 0f, 0.5f, 0.1f),
                new Color(0f, 0.8f, 1f, 0.1f),
                new Color(0.6f, 0f, 1f, 0.15f)
            };

            for (int i = 0; i < xPositions.Length; i++)
            {
                var beam = new GameObject($"SideBeam_{i}");
                beam.transform.SetParent(transform);
                var mf = beam.AddComponent<MeshFilter>();
                mf.mesh = MeshFactory.CreateBox(new Vector3(0.1f, 60f, 0.1f));
                var mr = beam.AddComponent<MeshRenderer>();
                mr.material = MaterialFactory.CreateUnlitTransparent(beamColors[i]);
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                beam.transform.position = new Vector3(xPositions[i], 25f, -40f);
            }
        }

        private void Update()
        {
            float t = Time.time;

            if (purpleLight != null)
            {
                purpleLight.intensity = 3.5f + Mathf.Sin(t * 0.7f) * 1.2f;
                purpleLight.transform.position = new Vector3(
                    -15f + Mathf.Sin(t * 0.3f) * 4f, 10f,
                    -20f + Mathf.Cos(t * 0.2f) * 6f);
            }

            if (cyanLight != null)
            {
                cyanLight.intensity = 3.5f + Mathf.Cos(t * 0.9f) * 1.2f;
                cyanLight.transform.position = new Vector3(
                    15f + Mathf.Cos(t * 0.3f) * 4f, 10f,
                    -20f + Mathf.Sin(t * 0.25f) * 6f);
            }

            if (pinkSpot != null)
            {
                pinkSpot.intensity = 2.5f + Mathf.Sin(t * 1.2f) * 0.8f;
            }
        }
    }
}
