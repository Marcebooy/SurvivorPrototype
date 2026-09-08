using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BonkSurvivor.Editor
{
    // Fixes up the import of the three Golem_*.fbx files under
    // Assets/Survivor/Characters/Golem/Resources/ (Golem_Walk_Massive carries the mesh that gets
    // instantiated; Attack_Crush and Stoneform_Shockwave exist only so GolemVisual can lift their
    // AnimationClip out - see Unity/Golem/README.txt). Legacy rig on all three; only the walk
    // clip loops. Materials are recreated as URP/Lit from the exact colors create_golem.py
    // assigned in Blender (the FBX carries no textures).
    public sealed class GolemImportPostprocessor : AssetPostprocessor
    {
        static readonly Dictionary<string, (Color color, float metallic, float smoothness, float emission)> Materials = new Dictionary<string, (Color, float, float, float)>
        {
            { "Ancient grey granite", (new Color(.20f, .23f, .24f), 0f, .12f, 0f) },
            { "Worn stone edges", (new Color(.34f, .37f, .36f), 0f, .14f, 0f) },
            { "Deep basalt joints", (new Color(.043f, .052f, .055f), 0f, .35f, 0f) },
            { "Old moss", (new Color(.074f, .11f, .045f), 0f, .35f, 0f) },
            { "Oxidized bronze", (new Color(.22f, .17f, .08f), .65f, .35f, 0f) },
            { "Amber heart light", (new Color(1f, .19f, .008f), 0f, .35f, 3f) },
            { "Core white hot center", (new Color(1f, .56f, .08f), 0f, .35f, 4f) },
        };

        bool IsGolemModel => assetPath.Replace('\\', '/').Contains("/Golem_");

        void OnPreprocessModel()
        {
            if (!IsGolemModel) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!IsGolemModel) return;
            var importer = (ModelImporter)assetImporter;
            bool loop = assetPath.Contains("Walk_Massive");
            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++) { var c = clips[i]; c.loopTime = loop; c.loopPose = loop; clips[i] = c; }
            importer.clipAnimations = clips;
        }

        void OnPostprocessMaterial(Material material)
        {
            if (!IsGolemModel) return;
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
