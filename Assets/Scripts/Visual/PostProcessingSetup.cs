using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PulseHighway.Visual
{
    public class PostProcessingSetup : MonoBehaviour
    {
        public void Setup()
        {
            // Create a Volume for post-processing
            var volumeGO = new GameObject("PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            volumeGO.layer = 0;

            var volume = volumeGO.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;

            // Bloom
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.value = 3.5f;
            bloom.intensity.overrideState = true;
            bloom.threshold.value = 0.7f;
            bloom.threshold.overrideState = true;
            bloom.scatter.value = 0.6f;
            bloom.scatter.overrideState = true;
            bloom.tint.value = new Color(0.9f, 0.8f, 1f);
            bloom.tint.overrideState = true;

            // Vignette
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.45f;
            vignette.intensity.overrideState = true;
            vignette.smoothness.value = 0.4f;
            vignette.smoothness.overrideState = true;
            vignette.color.value = new Color(0.1f, 0f, 0.15f);
            vignette.color.overrideState = true;

            // Chromatic Aberration
            var chromatic = profile.Add<ChromaticAberration>(true);
            chromatic.intensity.value = 0.15f;
            chromatic.intensity.overrideState = true;

            // Color Adjustments (cyberpunk color grading)
            var colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.postExposure.value = 0.3f;
            colorAdj.postExposure.overrideState = true;
            colorAdj.contrast.value = 15f;
            colorAdj.contrast.overrideState = true;
            colorAdj.saturation.value = 20f;
            colorAdj.saturation.overrideState = true;

            // Film Grain
            var grain = profile.Add<FilmGrain>(true);
            grain.intensity.value = 0.1f;
            grain.intensity.overrideState = true;
            grain.type.value = FilmGrainLookup.Thin1;
            grain.type.overrideState = true;

            // Lift Gamma Gain (push shadows toward purple, highlights toward cyan)
            var lgg = profile.Add<LiftGammaGain>(true);
            lgg.lift.value = new Vector4(0.05f, 0f, 0.08f, 0f);
            lgg.lift.overrideState = true;
            lgg.gain.value = new Vector4(-0.02f, 0.03f, 0.05f, 0f);
            lgg.gain.overrideState = true;

            // Camera setup for post-processing
            var cam = Camera.main;
            if (cam != null)
            {
                var cameraData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (cameraData == null)
                    cameraData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                cameraData.renderPostProcessing = true;
                cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            }
        }
    }
}
