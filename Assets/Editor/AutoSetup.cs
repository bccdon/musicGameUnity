using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;

namespace PulseHighway.Editor
{
    [InitializeOnLoad]
    public static class AutoSetup
    {
        static AutoSetup()
        {
            EditorApplication.delayCall += RunSetup;
        }

        private static void RunSetup()
        {
            // Check if URP is already configured
            if (GraphicsSettings.defaultRenderPipeline != null)
                return;

            Debug.Log("[PulseHighway] Auto-configuring URP render pipeline...");

            // Create URP Renderer
            string rendererPath = "Assets/Settings/URP-Renderer.asset";
            string pipelinePath = "Assets/Settings/URP-Pipeline.asset";

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                AssetDatabase.CreateFolder("Assets", "Settings");

            // Create renderer asset if needed
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }

            // Create pipeline asset if needed
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                // Use reflection to create since the constructor needs a renderer
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }

            // Configure pipeline settings
            pipeline.supportsCameraOpaqueTexture = true;
            pipeline.supportsCameraDepthTexture = true;
            pipeline.msaaSampleCount = 4;
            pipeline.supportsHDR = true;
            pipeline.renderScale = 1.0f;

            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();

            // Assign to graphics settings
            GraphicsSettings.defaultRenderPipeline = pipeline;

            // Assign to all quality levels
            var qualityLevels = QualitySettings.names;
            for (int i = 0; i < qualityLevels.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            // Set back to highest quality
            QualitySettings.SetQualityLevel(qualityLevels.Length - 1, true);

            // Set color space to Linear
            PlayerSettings.colorSpace = ColorSpace.Linear;

            AssetDatabase.SaveAssets();
            Debug.Log("[PulseHighway] URP render pipeline configured successfully!");
        }
    }
}
