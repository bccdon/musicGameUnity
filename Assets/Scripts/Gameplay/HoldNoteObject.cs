using UnityEngine;
using PulseHighway.Visual;
using PulseHighway.Core;

namespace PulseHighway.Gameplay
{
    public class HoldNoteObject : NoteObject
    {
        private GameObject beamOuter;
        private GameObject beamInner;
        private MeshRenderer beamOuterRenderer;
        private MeshRenderer beamInnerRenderer;
        private float holdDuration;
        private bool isBeingHeld;
        private bool holdSetUp;

        public void SetupHold(int lane, float duration)
        {
            // Setup base note geometry (only creates once)
            Setup(lane);
            holdDuration = duration;

            float beamLength = duration * GameManager.NOTE_SPEED;

            if (!holdSetUp)
            {
                // Outer glow beam
                beamOuter = new GameObject("BeamOuter");
                beamOuter.transform.SetParent(transform, false);
                var outerMF = beamOuter.AddComponent<MeshFilter>();
                outerMF.mesh = MeshFactory.CreateBox(new Vector3(0.8f, 0.3f, beamLength));
                beamOuterRenderer = beamOuter.AddComponent<MeshRenderer>();
                beamOuterRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                // Inner core beam
                beamInner = new GameObject("BeamInner");
                beamInner.transform.SetParent(transform, false);
                var innerMF = beamInner.AddComponent<MeshFilter>();
                innerMF.mesh = MeshFactory.CreateBox(new Vector3(0.25f, 0.15f, beamLength));
                beamInnerRenderer = beamInner.AddComponent<MeshRenderer>();
                beamInnerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                holdSetUp = true;
            }
            else
            {
                // Reconfigure beam length
                beamOuter.GetComponent<MeshFilter>().mesh = MeshFactory.CreateBox(new Vector3(0.8f, 0.3f, beamLength));
                beamInner.GetComponent<MeshFilter>().mesh = MeshFactory.CreateBox(new Vector3(0.25f, 0.15f, beamLength));
            }

            // Reconfigure materials for this lane
            beamOuterRenderer.material = MaterialFactory.CreateHoldBeamMaterial(lane);
            beamInnerRenderer.material = MaterialFactory.CreateHoldCoreMaterial();

            beamOuter.transform.localPosition = new Vector3(0f, 0f, -beamLength * 0.5f);
            beamInner.transform.localPosition = new Vector3(0f, 0f, -beamLength * 0.5f);
        }

        public void StartHolding()
        {
            isBeingHeld = true;
        }

        public void StopHolding()
        {
            isBeingHeld = false;
        }

        public override void Deactivate()
        {
            isBeingHeld = false;
            base.Deactivate();
        }

        protected override void Update()
        {
            base.Update();

            if (isBeingHeld && beamOuterRenderer != null)
            {
                float pulse = Mathf.Sin(Time.time * 8f) * 0.15f + 0.85f;
                Color c = beamOuterRenderer.material.color;
                beamOuterRenderer.material.color = new Color(c.r, c.g, c.b, 0.4f * pulse);
            }
        }
    }
}
