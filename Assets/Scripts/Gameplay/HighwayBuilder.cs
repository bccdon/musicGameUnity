using UnityEngine;
using PulseHighway.Core;
using PulseHighway.Visual;

namespace PulseHighway.Gameplay
{
    public class HighwayBuilder : MonoBehaviour
    {
        private Material[] laneFlashMaterials = new Material[5];
        private float[] flashTimers = new float[5];

        public void Build()
        {
            float totalWidth = GameManager.LANE_COUNT * GameManager.LANE_WIDTH;
            float startX = -totalWidth * 0.5f + GameManager.LANE_WIDTH * 0.5f;

            for (int lane = 0; lane < GameManager.LANE_COUNT; lane++)
            {
                float x = startX + lane * GameManager.LANE_WIDTH;
                float laneZ = -GameManager.HIGHWAY_LENGTH * 0.5f + GameManager.HIT_LINE_Z;

                // Lane surface — dark reflective
                var laneGO = CreateMeshObject($"Lane_{lane}",
                    MeshFactory.CreateBox(new Vector3(GameManager.LANE_WIDTH * 0.95f, 0.05f, GameManager.HIGHWAY_LENGTH)),
                    MaterialFactory.CreateHighwayMaterial());
                laneGO.transform.SetParent(transform);
                laneGO.transform.localPosition = new Vector3(x, 0f, laneZ);

                // Left edge — glowing lane color
                var edgeLeft = CreateMeshObject($"Edge_{lane}_L",
                    MeshFactory.CreateBox(new Vector3(0.06f, 0.2f, GameManager.HIGHWAY_LENGTH)),
                    MaterialFactory.CreateEdgeMaterial(lane));
                edgeLeft.transform.SetParent(transform);
                edgeLeft.transform.localPosition = new Vector3(x - GameManager.LANE_WIDTH * 0.48f, 0.1f, laneZ);

                // Right edge for last lane
                if (lane == GameManager.LANE_COUNT - 1)
                {
                    var edgeRight = CreateMeshObject($"Edge_{lane}_R",
                        MeshFactory.CreateBox(new Vector3(0.06f, 0.2f, GameManager.HIGHWAY_LENGTH)),
                        MaterialFactory.CreateEdgeMaterial(lane));
                    edgeRight.transform.SetParent(transform);
                    edgeRight.transform.localPosition = new Vector3(x + GameManager.LANE_WIDTH * 0.48f, 0.1f, laneZ);
                }

                // Center guide line — subtle
                var centerMat = MaterialFactory.CreateUnlitTransparent(new Color(1f, 1f, 1f, 0.04f));
                var centerLine = CreateMeshObject($"CenterLine_{lane}",
                    MeshFactory.CreateBox(new Vector3(0.02f, 0.01f, GameManager.HIGHWAY_LENGTH)),
                    centerMat);
                centerLine.transform.SetParent(transform);
                centerLine.transform.localPosition = new Vector3(x, 0.03f, laneZ);

                // Lane flash overlay
                laneFlashMaterials[lane] = MaterialFactory.CreateLaneFlashMaterial(lane);
                var flashGO = CreateMeshObject($"Flash_{lane}",
                    MeshFactory.CreateBox(new Vector3(GameManager.LANE_WIDTH * 0.9f, 0.02f, 12f)),
                    laneFlashMaterials[lane]);
                flashGO.transform.SetParent(transform);
                flashGO.transform.localPosition = new Vector3(x, 0.06f, GameManager.HIT_LINE_Z - 4f);
            }
        }

        public void FlashLane(int lane)
        {
            if (lane < 0 || lane >= 5) return;
            flashTimers[lane] = 0.12f;
        }

        private void Update()
        {
            for (int i = 0; i < 5; i++)
            {
                if (flashTimers[i] > 0)
                {
                    flashTimers[i] -= Time.deltaTime;
                    float alpha = Mathf.Clamp01(flashTimers[i] / 0.12f) * 0.5f;
                    Color c = MaterialFactory.GetLaneColor(i);
                    laneFlashMaterials[i].color = new Color(c.r, c.g, c.b, alpha);
                }
            }
        }

        private GameObject CreateMeshObject(string name, Mesh mesh, Material material)
        {
            var go = new GameObject(name);
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = material;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            return go;
        }
    }
}
