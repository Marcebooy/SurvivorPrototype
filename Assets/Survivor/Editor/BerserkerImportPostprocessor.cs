using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BonkSurvivor.Editor
{
    // Fixes up the import of the three Berserker_*.fbx files under
    // Assets/Survivor/Characters/Berserker/Resources/ (Berserker_Walk_Heavy carries the mesh that
    // gets instantiated; Berserker_Attack_Double and Berserker_Rage_LowHP exist only so
    // BerserkerVisual can lift their AnimationClip out and add it onto the walking model's
    // Animation component - see Unity/Berserker/README.txt). Legacy rig on all three; walk and
    // rage loop, attack does not. Materials are recreated as URP/Lit from the exact colors
    // create_berserker.py assigned in Blender (the FBX carries no textures). The "Rage energy" /
    // "Hot orange sparks" materials use a Blender driver tied to a custom HP property that FBX
    // export can't carry, so their emission here is just the script's own base intensity rather
    // than whatever got baked into any one file - the RageFX bone's animated scale is what
    // actually grows during the Rage_LowHP clip.
    public sealed class BerserkerImportPostprocessor : AssetPostprocessor
    {
        static readonly Dictionary<string, (Color color, float metallic, float smoothness, float emission)> Materials = new Dictionary<string, (Color, float, float, float)>
        {
            { "Warm battle worn skin", (new Color(.44f, .20f, .125f), 0f, .42f, 0f) },
            { "Healed scars", (new Color(.61f, .31f, .23f), 0f, .35f, 0f) },
            { "Worn dark steel", (new Color(.13f, .17f, .19f), .82f, .62f, 0f) },
            { "Sharpened axe steel", (new Color(.43f, .5f, .53f), .88f, .71f, 0f) },
            { "Blackened brown leather", (new Color(.035f, .022f, .015f), 0f, .35f, 0f) },
            { "Leather grain", (new Color(.12f, .067f, .029f), 0f, .35f, 0f) },
            { "Dirty bronze", (new Color(.4f, .23f, .07f), .73f, .35f, 0f) },
            { "Crimson fabric", (new Color(.28f, .008f, .012f), 0f, .35f, 0f) },
            { "Dirty linen wraps", (new Color(.42f, .36f, .24f), 0f, .35f, 0f) },
            { "Dark auburn hair", (new Color(.055f, .023f, .012f), 0f, .35f, 0f) },
            { "Old horn", (new Color(.39f, .33f, .22f), 0f, .35f, 0f) },
            { "Ivory teeth", (new Color(.7f, .62f, .43f), 0f, .35f, 0f) },
            { "Facial recesses", (new Color(.012f, .004f, .002f), 0f, .35f, 0f) },
            { "Enraged red eyes", (new Color(1f, .013f, .003f), 0f, .35f, 3f) },
            { "Rage energy", (new Color(1f, .023f, .003f), 0f, .35f, 4f) },
            { "Hot orange sparks", (new Color(1f, .13f, .006f), 0f, .35f, 5f) },
        };

        bool IsBerserkerModel => assetPath.Replace('\\', '/').Contains("/Berserker_");

        void OnPreprocessModel()
        {
            if (!IsBerserkerModel) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!IsBerserkerModel) return;
            var importer = (ModelImporter)assetImporter;
            bool loop = !assetPath.Contains("Attack_Double");
            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++) { var c = clips[i]; c.loopTime = loop; c.loopPose = loop; clips[i] = c; }
            importer.clipAnimations = clips;
        }

        void OnPostprocessMaterial(Material material)
        {
            if (!IsBerserkerModel) return;
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
