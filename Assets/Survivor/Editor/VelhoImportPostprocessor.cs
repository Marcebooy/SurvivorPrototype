using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BonkSurvivor.Editor
{
    // Fixes up the import of Assets/Survivor/Characters/Velho/Resources/Velho_Walk.fbx so the
    // wizard works out of the box: Legacy rig (so a plain Animation component + Play("Walk_Quick")
    // is enough at runtime, no hand-built Animator Controller needed), looping clip, and URP/Lit
    // materials recreated from the exact colors create_wizard.py assigned in Blender (the FBX
    // itself carries no textures - see Unity/Wizard/README.txt).
    public sealed class VelhoImportPostprocessor : AssetPostprocessor
    {
        static readonly Dictionary<string, (Color color, float metallic, float emission)> Materials = new Dictionary<string, (Color, float, float)>
        {
            { "Robe | midnight plum", (new Color(.105f, .025f, .22f), 0f, 0f) },
            { "Hood | violet facets", (new Color(.19f, .055f, .32f), 0f, 0f) },
            { "Azure embroidery", (new Color(.035f, .27f, .8f), 0f, .3f) },
            { "Antique gold", (new Color(.8f, .4f, .08f), .45f, 0f) },
            { "Electric cyan", (new Color(.02f, .8f, 1f), 0f, 3f) },
            { "Ember runes", (new Color(1f, .19f, .015f), 0f, 1f) },
            { "Hood interior", (new Color(.008f, .004f, .019f), 0f, 0f) },
            { "Golden eyes", (new Color(1f, .7f, .04f), 0f, 4f) },
            { "Twisted dark wood", (new Color(.15f, .065f, .055f), 0f, 0f) },
            { "Boots and gloves", (new Color(.045f, .025f, .07f), 0f, 0f) },
            { "Scroll parchment", (new Color(.83f, .64f, .36f), 0f, 0f) },
            { "Magenta potion", (new Color(.65f, .035f, .3f), 0f, .5f) },
        };

        bool IsVelhoModel => assetPath.Replace('\\', '/').Contains("Velho_Walk");

        void OnPreprocessModel()
        {
            if (!IsVelhoModel) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!IsVelhoModel) return;
            var importer = (ModelImporter)assetImporter;
            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++) { var c = clips[i]; c.loopTime = true; c.loopPose = true; clips[i] = c; }
            importer.clipAnimations = clips;
        }

        void OnPostprocessMaterial(Material material)
        {
            if (!IsVelhoModel) return;
            if (!Materials.TryGetValue(material.name, out var def)) return;
            material.shader = Shader.Find("Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", def.color);
            material.SetFloat("_Metallic", def.metallic);
            material.SetFloat("_Smoothness", .28f);
            if (def.emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                material.SetColor("_EmissionColor", def.color * def.emission);
            }
        }
    }
}
