using UnityEngine;
using PulseHighway.Core;
using PulseHighway.Visual;

namespace PulseHighway.Gameplay
{
    public class HitLineBuilder : MonoBehaviour
    {
        private Material hitLineMat;
        private Material glowMat;
        private float pulseTimer;

        public void Build()
        {
            float totalWidth = GameManager.LANE_COUNT * GameManager.LANE_WIDTH + 1f;

            hitLineMat = MaterialFactory.CreateHitLineMaterial();

            // Main hit line
            var cylinder = new GameObject("HitLineCylinder");
            cylinder.transform.SetParent(transform);
            var mf = cylinder.AddComponent<MeshFilter>();
            mf.mesh = MeshFactory.CreateCylinder(0.08f, totalWidth, 24);
            var mr = cylinder.AddComponent<MeshRenderer>();
            mr.material = hitLineMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            cylinder.transform.localPosition = new Vector3(0f, 0.15f, GameManager.HIT_LINE_Z);
            cylinder.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Outer glow cylinder
            glowMat = MaterialFactory.CreateUnlitTransparent(new Color(0f, 0.8f, 1f, 0.12f));
            glowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            glowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            glowMat.SetInt("_ZWrite", 0);
            glowMat.renderQueue = 3100;

            var glow = new GameObject("HitLineGlow");
            glow.transform.SetParent(transform);
            var gmf = glow.AddComponent<MeshFilter>();
            gmf.mesh = MeshFactory.CreateCylinder(0.25f, totalWidth, 24);
            var gmr = glow.AddComponent<MeshRenderer>();
            gmr.material = glowMat;
            gmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            glow.transform.localPosition = new Vector3(0f, 0.15f, GameManager.HIT_LINE_Z);
            glow.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        private void Update()
        {
            pulseTimer += Time.deltaTime;

            if (hitLineMat != null)
            {
                float pulse = Mathf.Sin(pulseTimer * 4f) * 0.4f + 0.6f;
                hitLineMat.SetColor("_EmissionColor", new Color(0f, 0.8f, 1f) * (4f * pulse));
            }

            if (glowMat != null)
            {
                float glowPulse = Mathf.Sin(pulseTimer * 3f) * 0.06f + 0.1f;
                glowMat.color = new Color(0f, 0.8f, 1f, glowPulse);
            }
        }
    }
}
