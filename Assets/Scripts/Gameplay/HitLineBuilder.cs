using UnityEngine;
using PulseHighway.Core;
using PulseHighway.Visual;

namespace PulseHighway.Gameplay
{
    public class HitLineBuilder : MonoBehaviour
    {
        private Material hitLineMat;
        private Material glowMat;
        private Material barMat;
        private float pulseTimer;

        public void Build()
        {
            float totalWidth = GameManager.LANE_COUNT * GameManager.LANE_WIDTH + 2f;

            hitLineMat = MaterialFactory.CreateHitLineMaterial();

            // Main hit line — wide bar instead of thin cylinder
            var bar = new GameObject("HitLineBar");
            bar.transform.SetParent(transform);
            var barMF = bar.AddComponent<MeshFilter>();
            barMF.mesh = MeshFactory.CreateBox(new Vector3(totalWidth, 0.15f, 0.5f));
            var barMR = bar.AddComponent<MeshRenderer>();
            barMR.material = hitLineMat;
            barMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            bar.transform.localPosition = new Vector3(0f, 0.08f, GameManager.HIT_LINE_Z);

            // Inner glow line (thin bright core)
            var core = new GameObject("HitLineCore");
            core.transform.SetParent(transform);
            var coreMF = core.AddComponent<MeshFilter>();
            coreMF.mesh = MeshFactory.CreateCylinder(0.06f, totalWidth, 16);
            var coreMat = new Material(MaterialFactory.CreateHitLineMaterial());
            coreMat.SetColor("_EmissionColor", new Color(1f, 1f, 1f) * 10f);
            var coreMR = core.AddComponent<MeshRenderer>();
            coreMR.material = coreMat;
            coreMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            core.transform.localPosition = new Vector3(0f, 0.16f, GameManager.HIT_LINE_Z);
            core.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Outer wide glow
            glowMat = MaterialFactory.CreateUnlitTransparent(new Color(0f, 0.8f, 1f, 0.08f));
            glowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            glowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            glowMat.SetInt("_ZWrite", 0);
            glowMat.renderQueue = 3100;

            var glow = new GameObject("HitLineGlow");
            glow.transform.SetParent(transform);
            var gmf = glow.AddComponent<MeshFilter>();
            gmf.mesh = MeshFactory.CreateBox(new Vector3(totalWidth, 0.05f, 3f));
            var gmr = glow.AddComponent<MeshRenderer>();
            gmr.material = glowMat;
            gmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            glow.transform.localPosition = new Vector3(0f, 0.05f, GameManager.HIT_LINE_Z);

            // Lane-colored segments on the hit line
            float laneStart = -totalWidth * 0.5f + 1f + GameManager.LANE_WIDTH * 0.5f;
            for (int i = 0; i < GameManager.LANE_COUNT; i++)
            {
                Color lc = MaterialFactory.GetLaneColor(i);
                var segMat = MaterialFactory.CreateUnlitTransparent(new Color(lc.r, lc.g, lc.b, 0.15f));
                segMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                segMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                segMat.renderQueue = 3100;

                var seg = new GameObject($"LaneSeg_{i}");
                seg.transform.SetParent(transform);
                var smf = seg.AddComponent<MeshFilter>();
                smf.mesh = MeshFactory.CreateBox(new Vector3(GameManager.LANE_WIDTH * 0.8f, 0.12f, 0.3f));
                var smr = seg.AddComponent<MeshRenderer>();
                smr.material = segMat;
                smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                seg.transform.localPosition = new Vector3(laneStart + i * GameManager.LANE_WIDTH, 0.1f, GameManager.HIT_LINE_Z);
            }
        }

        private void Update()
        {
            pulseTimer += Time.deltaTime;

            if (hitLineMat != null)
            {
                float pulse = Mathf.Sin(pulseTimer * 4f) * 0.3f + 0.7f;
                hitLineMat.SetColor("_EmissionColor", new Color(0f, 0.8f, 1f) * (8f * pulse));
            }

            if (glowMat != null)
            {
                float glowPulse = Mathf.Sin(pulseTimer * 3f) * 0.04f + 0.08f;
                glowMat.color = new Color(0f, 0.8f, 1f, glowPulse);
            }
        }
    }
}
