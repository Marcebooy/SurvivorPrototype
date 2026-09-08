using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BonkSurvivor.Editor
{
    // Fixes up the import of the two Hunter_*.fbx files under
    // Assets/Survivor/Characters/Hunter/Resources/ (Hunter_Run_Agile carries the mesh that gets
    // instantiated; Hunter_Bow_ChargeRelease exists only so HunterVisual can lift its
    // AnimationClip out - see Unity/Hunter/README.txt). Legacy rig on both; only the run clip
    // loops. Materials are recreated as URP/Lit from the exact colors create_hunter.py assigned
    // in Blender (the FBX carries no textures).
    public sealed class HunterImportPostprocessor : AssetPostprocessor
    {
        static readonly Dictionary<string, (Color color, float metallic, float smoothness, float emission)> Materials = new Dictionary<string, (Color, float, float, float)>
        {
            { "Forest green leather", (new Color(.035f, .095f, .049f), 0f, .35f, 0f) },
            { "Weathered chestnut leather", (new Color(.105f, .051f, .024f), 0f, .35f, 0f) },
            { "Dark leather", (new Color(.016f, .023f, .024f), 0f, .35f, 0f) },
            { "Deep forest cloak", (new Color(.023f, .058f, .038f), 0f, .35f, 0f) },
            { "Antique gold runes", (new Color(.64f, .40f, .105f), .7f, .68f, 0f) },
            { "Blue grey metal", (new Color(.14f, .23f, .30f), .8f, .7f, 0f) },
            { "Silver blade edges", (new Color(.45f, .56f, .61f), .8f, .72f, 0f) },
            { "Midnight bow wood", (new Color(.014f, .03f, .045f), .25f, .35f, 0f) },
            { "Warm face and fingers", (new Color(.43f, .26f, .17f), 0f, .35f, 0f) },
            { "Shadow", (new Color(.002f, .004f, .006f), 0f, .35f, 0f) },
            { "Azure rune light", (new Color(.018f, .55f, 1f), 0f, .35f, 3f) },
            { "White blue arrow core", (new Color(.34f, .86f, 1f), 0f, .35f, 4f) },
            { "Linen bindings", (new Color(.37f, .34f, .24f), 0f, .35f, 0f) },
            { "Arrow feathers", (new Color(.16f, .22f, .16f), 0f, .35f, 0f) },
        };

        bool IsHunterModel => assetPath.Replace('\\', '/').Contains("/Hunter_");

        void OnPreprocessModel()
        {
            if (!IsHunterModel) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!IsHunterModel) return;
            var importer = (ModelImporter)assetImporter;
            bool loop = assetPath.Contains("Run_Agile");
            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++) { var c = clips[i]; c.loopTime = loop; c.loopPose = loop; clips[i] = c; }
            importer.clipAnimations = clips;
        }

        void OnPostprocessMaterial(Material material)
        {
            if (!IsHunterModel) return;
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
