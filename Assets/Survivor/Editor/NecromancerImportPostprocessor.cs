using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BonkSurvivor.Editor
{
    // Fixes up the import of Assets/Survivor/Characters/Necromancer/Resources/Necromancer_Glide.fbx:
    // Legacy rig (a plain Animation component is enough at runtime), looping clip, and URP/Lit
    // materials recreated from the exact colors create_necromancer.py assigned in Blender (the FBX
    // carries no textures - see Unity/Necromancer/README.txt).
    public sealed class NecromancerImportPostprocessor : AssetPostprocessor
    {
        static readonly Dictionary<string, (Color color, float metallic, float smoothness, float emission)> Materials = new Dictionary<string, (Color, float, float, float)>
        {
            { "Obsidian violet cloth", (new Color(.024f, .016f, .043f), 0f, .35f, 0f) },
            { "Muted amethyst lining", (new Color(.082f, .041f, .112f), 0f, .35f, 0f) },
            { "Tarnished silver embroidery", (new Color(.24f, .27f, .26f), .6f, .35f, 0f) },
            { "Aged ivory", (new Color(.61f, .52f, .32f), 0f, .43f, 0f) },
            { "Old bone joints", (new Color(.27f, .23f, .14f), 0f, .35f, 0f) },
            { "Absolute hood shadow", (new Color(.0015f, .002f, .003f), 0f, .35f, 0f) },
            { "Black leather", (new Color(.027f, .024f, .024f), 0f, .35f, 0f) },
            { "Soul green", (new Color(.08f, 1f, .19f), 0f, .35f, 3f) },
            { "Verdigris ornaments", (new Color(.025f, .19f, .12f), .55f, .35f, 0f) },
            { "Old parchment", (new Color(.44f, .37f, .23f), 0f, .35f, 0f) },
            { "Opaque poison glass", (new Color(.018f, .25f, .10f), .25f, .8f, .35f) },
        };

        bool IsNecromancerModel => assetPath.Replace('\\', '/').Contains("Necromancer_Glide");

        void OnPreprocessModel()
        {
            if (!IsNecromancerModel) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!IsNecromancerModel) return;
            var importer = (ModelImporter)assetImporter;
            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++) { var c = clips[i]; c.loopTime = true; c.loopPose = true; clips[i] = c; }
            importer.clipAnimations = clips;
        }

        void OnPostprocessMaterial(Material material)
        {
            if (!IsNecromancerModel) return;
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
