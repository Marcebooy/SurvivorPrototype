using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BonkSurvivor.Editor
{
    // Fixes up the import of the three Ninja_*.fbx files under
    // Assets/Survivor/Characters/Ninja/Resources/ (Ninja_Run_Shadow carries the mesh that gets
    // instantiated; Attack_CrossCut and Shadowstep exist only so NinjaVisual can lift their
    // AnimationClip out - see Unity/Ninja/README.txt). Legacy rig on all three; only the run clip
    // loops. Materials are recreated as URP/Lit from the exact colors create_ninja.py assigned in
    // Blender (the FBX carries no textures).
    public sealed class NinjaImportPostprocessor : AssetPostprocessor
    {
        static readonly Dictionary<string, (Color color, float metallic, float smoothness, float emission)> Materials = new Dictionary<string, (Color, float, float, float)>
        {
            { "Midnight woven fabric", (new Color(.012f, .021f, .055f), 0f, .35f, 0f) },
            { "Charcoal cloth", (new Color(.009f, .012f, .019f), 0f, .35f, 0f) },
            { "Soft charcoal guards", (new Color(.06f, .07f, .085f), 0f, .35f, 0f) },
            { "Violet silk accents", (new Color(.11f, .019f, .19f), 0f, .35f, 0f) },
            { "Dark polished blade steel", (new Color(.15f, .19f, .25f), .75f, .68f, 0f) },
            { "Blade cutting edge", (new Color(.39f, .45f, .58f), .7f, .73f, 0f) },
            { "Electric blue eyes", (new Color(.018f, .52f, 1f), 0f, .35f, 3f) },
            { "Violet shadow cuts", (new Color(.43f, .016f, 1f), 0f, .35f, 3f) },
            { "Black violet shadow smoke", (new Color(.014f, .005f, .026f), 0f, .35f, 0f) },
            { "Spectral violet silhouettes", (new Color(.12f, .012f, .28f), 0f, .35f, .4f) },
        };

        bool IsNinjaModel => assetPath.Replace('\\', '/').Contains("/Ninja_");

        void OnPreprocessModel()
        {
            if (!IsNinjaModel) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!IsNinjaModel) return;
            var importer = (ModelImporter)assetImporter;
            bool loop = assetPath.Contains("Run_Shadow");
            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++) { var c = clips[i]; c.loopTime = loop; c.loopPose = loop; clips[i] = c; }
            importer.clipAnimations = clips;
        }

        void OnPostprocessMaterial(Material material)
        {
            if (!IsNinjaModel) return;
            if (!Materials.TryGetValue(material.name, out var def)) return;
            material.shader = Shader.Find("Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", def.color);
            material.SetFloat("_Metallic", def.metallic);
            material.SetFloat("_Smoothness", def.smoothness);
            if (def.emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                material.SetColor("_EmissionColor", def.color * def.emission);
            }
        }
    }
}
