using UnityEngine;

namespace PulseHighway.Visual
{
    public static class MaterialFactory
    {
        private static readonly Color[] LaneColors = new Color[]
        {
            new Color(1.0f, 0.0f, 0.333f),   // Lane 0: Red   #ff0055
            new Color(1.0f, 0.6f, 0.0f),      // Lane 1: Orange #ff9900
            new Color(1.0f, 1.0f, 0.0f),      // Lane 2: Yellow #ffff00
            new Color(0.0f, 1.0f, 0.6f),      // Lane 3: Green  #00ff99
            new Color(0.0f, 0.8f, 1.0f)       // Lane 4: Cyan   #00ccff
        };

        // Cached materials per lane to avoid leaks
        private static Material[] cachedNoteMaterials = new Material[5];
        private static Material[] cachedTrailMaterials = new Material[5];
        private static Material[] cachedEdgeMaterials = new Material[5];
        private static Material[] cachedHoldBeamMaterials = new Material[5];
        private static Material[] cachedLaneFlashMaterials = new Material[5];
        private static Material cachedCoreMaterial;
        private static Material cachedHoldCoreMaterial;

        public static Color GetLaneColor(int lane)
        {
            if (lane < 0 || lane >= LaneColors.Length) return Color.white;
            return LaneColors[lane];
        }

        public static Material CreateHighwayMaterial()
        {
            var mat = new Material(GetLitShader());
            mat.name = "Highway";
            mat.color = new Color(0.015f, 0.015f, 0.04f, 1f);
            mat.SetFloat("_Smoothness", 0.95f);
            mat.SetFloat("_Metallic", 0.95f);
            return mat;
        }

        public static Material CreateEdgeMaterial(int lane)
        {
            if (cachedEdgeMaterials[lane] != null) return cachedEdgeMaterials[lane];
            var mat = new Material(GetUnlitShader());
            mat.name = $"Edge_Lane{lane}";
            Color c = GetLaneColor(lane);
            mat.color = new Color(c.r, c.g, c.b, 0.7f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", c * 1.5f);
            SetupTransparency(mat);
            cachedEdgeMaterials[lane] = mat;
            return mat;
        }

        public static Material CreateHitLineMaterial()
        {
            var mat = new Material(GetLitShader());
            mat.name = "HitLine";
            mat.color = Color.white;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0f, 0.8f, 1f) * 4f);
            return mat;
        }

        public static Material CreateNoteMaterial(int lane)
        {
            if (cachedNoteMaterials[lane] != null) return cachedNoteMaterials[lane];
            var mat = new Material(GetLitShader());
            mat.name = $"Note_Lane{lane}";
            Color c = GetLaneColor(lane);
            mat.color = c;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", c * 3f);
            mat.SetFloat("_Smoothness", 0.9f);
            mat.SetFloat("_Metallic", 0.8f);
            cachedNoteMaterials[lane] = mat;
            return mat;
        }

        public static Material CreateNoteCoreMaterial()
        {
            if (cachedCoreMaterial != null) return cachedCoreMaterial;
            var mat = new Material(GetLitShader());
            mat.name = "NoteCore";
            mat.color = Color.white;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.white * 2f);
            cachedCoreMaterial = mat;
            return mat;
        }

        public static Material CreateHoldBeamMaterial(int lane)
        {
            if (cachedHoldBeamMaterials[lane] != null) return cachedHoldBeamMaterials[lane];
            var mat = new Material(GetUnlitShader());
            mat.name = $"HoldBeam_Lane{lane}";
            Color c = GetLaneColor(lane);
            mat.color = new Color(c.r, c.g, c.b, 0.5f);
            SetupTransparency(mat);
            SetupAdditive(mat);
            cachedHoldBeamMaterials[lane] = mat;
            return mat;
        }

        public static Material CreateHoldCoreMaterial()
        {
            if (cachedHoldCoreMaterial != null) return cachedHoldCoreMaterial;
            var mat = new Material(GetUnlitShader());
            mat.name = "HoldCore";
            mat.color = new Color(1f, 1f, 1f, 0.9f);
            SetupTransparency(mat);
            cachedHoldCoreMaterial = mat;
            return mat;
        }

        public static Material CreateGridMaterial()
        {
            var mat = new Material(GetUnlitShader());
            mat.name = "Grid";
            mat.color = new Color(0.5f, 0.05f, 0.4f, 0.25f);
            SetupTransparency(mat);
            return mat;
        }

        public static Material CreateStarfieldMaterial()
        {
            var mat = new Material(GetParticleShader());
            mat.name = "Starfield";
            mat.color = Color.white;
            SetupAdditive(mat);
            return mat;
        }

        public static Material CreateParticleMaterial()
        {
            var mat = new Material(GetParticleShader());
            mat.name = "Particle";
            mat.color = Color.white;
            SetupAdditive(mat);
            return mat;
        }

        public static Material CreateLaneFlashMaterial(int lane)
        {
            if (cachedLaneFlashMaterials[lane] != null) return cachedLaneFlashMaterials[lane];
            var mat = new Material(GetUnlitShader());
            mat.name = $"LaneFlash_{lane}";
            Color c = GetLaneColor(lane);
            mat.color = new Color(c.r, c.g, c.b, 0f);
            SetupTransparency(mat);
            SetupAdditive(mat);
            cachedLaneFlashMaterials[lane] = mat;
            return mat;
        }

        public static Material CreateTrailMaterial(int lane)
        {
            if (cachedTrailMaterials[lane] != null) return cachedTrailMaterials[lane];
            var mat = new Material(GetUnlitShader());
            mat.name = $"Trail_Lane{lane}";
            Color c = GetLaneColor(lane);
            mat.color = new Color(c.r, c.g, c.b, 0.6f);
            SetupTransparency(mat);
            SetupAdditive(mat);
            cachedTrailMaterials[lane] = mat;
            return mat;
        }

        public static Material CreateUnlitTransparent(Color color)
        {
            var mat = new Material(GetUnlitShader());
            mat.color = color;
            SetupTransparency(mat);
            return mat;
        }

        private static Shader GetLitShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            return shader;
        }

        private static Shader GetUnlitShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            return shader;
        }

        private static Shader GetParticleShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            return shader;
        }

        private static void SetupTransparency(Material mat)
        {
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        private static void SetupAdditive(Material mat)
        {
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3100;
        }
    }
}
