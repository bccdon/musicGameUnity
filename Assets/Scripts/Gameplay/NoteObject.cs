using UnityEngine;
using PulseHighway.Visual;

namespace PulseHighway.Gameplay
{
    public class NoteObject : MonoBehaviour
    {
        public int NoteIndex { get; set; } = -1;
        public int Lane { get; set; }
        public bool IsActive { get; private set; }
        public bool IsSetUp { get; private set; }

        protected MeshRenderer headRenderer;
        protected MeshRenderer coreRenderer;
        protected Material headMaterial;
        protected TrailRenderer trail;
        protected Transform headTransform;
        protected Transform coreTransform;

        private static Mesh octahedronMesh;
        private static Mesh coreMesh;
        private static Material coreMaterial;

        public virtual void Setup(int lane)
        {
            Lane = lane;

            // Only create geometry once
            if (!IsSetUp)
            {
                if (octahedronMesh == null)
                    octahedronMesh = MeshFactory.CreateOctahedron(0.6f);
                if (coreMesh == null)
                    coreMesh = MeshFactory.CreateOctahedron(0.25f);
                if (coreMaterial == null)
                    coreMaterial = MaterialFactory.CreateNoteCoreMaterial();

                // Head (outer diamond)
                var headGO = new GameObject("Head");
                headGO.transform.SetParent(transform, false);
                var headMF = headGO.AddComponent<MeshFilter>();
                headMF.mesh = octahedronMesh;
                headRenderer = headGO.AddComponent<MeshRenderer>();
                headRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                headTransform = headGO.transform;

                // Core (inner white diamond)
                var coreGO = new GameObject("Core");
                coreGO.transform.SetParent(transform, false);
                var coreMF = coreGO.AddComponent<MeshFilter>();
                coreMF.mesh = coreMesh;
                coreRenderer = coreGO.AddComponent<MeshRenderer>();
                coreRenderer.material = coreMaterial;
                coreRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                coreTransform = coreGO.transform;

                // Trail renderer
                trail = gameObject.AddComponent<TrailRenderer>();
                trail.startWidth = 0.3f;
                trail.endWidth = 0f;
                trail.time = 0.3f;
                trail.minVertexDistance = 0.1f;
                trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                trail.receiveShadows = false;

                IsSetUp = true;
            }

            // Reconfigure for this lane (safe to call multiple times)
            headMaterial = MaterialFactory.CreateNoteMaterial(lane);
            headRenderer.material = headMaterial;

            Color lc = MaterialFactory.GetLaneColor(lane);
            var trailMat = MaterialFactory.CreateTrailMaterial(lane);
            trail.material = trailMat;
            trail.startColor = new Color(lc.r, lc.g, lc.b, 0.5f);
            trail.endColor = new Color(lc.r, lc.g, lc.b, 0f);

            Deactivate();
        }

        public virtual void Activate(int noteIndex)
        {
            NoteIndex = noteIndex;
            IsActive = true;
            gameObject.SetActive(true);
            if (trail != null) trail.Clear();
        }

        public virtual void Deactivate()
        {
            IsActive = false;
            NoteIndex = -1;
            gameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            if (!IsActive) return;

            // Spin the diamond
            if (headTransform != null)
                headTransform.Rotate(0f, 180f * Time.deltaTime, 72f * Time.deltaTime, Space.Self);
            if (coreTransform != null)
                coreTransform.Rotate(0f, -120f * Time.deltaTime, 0f, Space.Self);
        }
    }
}
