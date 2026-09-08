using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BonkSurvivor.Editor
{
    // Fixes up the import of the four Paladin_*.fbx files under
    // Assets/Survivor/Characters/Paladin/Resources/ (Paladin_Walk_Steady carries the mesh that
    // gets instantiated; Attack_CleanSlash, Block_Holy and Divine_Beam exist only so PaladinVisual
    // can lift their AnimationClip out - see Unity/Paladin/README.txt). Legacy rig on all four;
    // only the walk clip loops. Materials are recreated as URP/Lit from the exact colors
    // create_paladin.py assigned in Blender (the FBX carries no textures).
    public sealed class PaladinImportPostprocessor : AssetPostprocessor
    {
        static readonly Dictionary<string, (Color color, float metallic, float smoothness, float emission)> Materials = new Dictionary<string, (Color, float, float, float)>
        {
            { "Blue black enamel", (new Color(.015f, .023f, .038f), .72f, .68f, 0f) },
            { "Crimson enamel", (new Color(.23f, .009f, .016f), .58f, .67f, 0f) },
            { "Deep red velvet", (new Color(.20f, .008f, .016f), 0f, .35f, 0f) },
            { "Polished antique gold", (new Color(.72f, .40f, .075f), .78f, .74f, 0f) },
            { "Pale gold edges", (new Color(.88f, .65f, .23f), .75f, .76f, 0f) },
            { "Silver steel", (new Color(.38f, .45f, .53f), .82f, .7f, 0f) },
            { "Black leather", (new Color(.012f, .012f, .015f), 0f, .35f, 0f) },
            { "Ruby settings", (new Color(.43f, .008f, .018f), .42f, .8f, 0f) },
            { "Warm face", (new Color(.43f, .23f, .15f), 0f, .35f, 0f) },
            { "Face shadow", (new Color(.009f, .005f, .006f), 0f, .35f, 0f) },
            { "Golden white sacred light", (new Color(1f, .63f, .10f), 0f, .35f, 3f) },
            { "White hot core", (new Color(1f, .88f, .48f), 0f, .35f, 4f) },
        };

        bool IsPaladinModel => assetPath.Replace('\\', '/').Contains("/Paladin_");

        void OnPreprocessModel()
        {
            if (!IsPaladinModel) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!IsPaladinModel) return;
            var importer = (ModelImporter)assetImporter;
            bool loop = assetPath.Contains("Walk_Steady");
            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++) { var c = clips[i]; c.loopTime = loop; c.loopPose = loop; clips[i] = c; }
            importer.clipAnimations = clips;
        }

        void OnPostprocessMaterial(Material material)
        {
            if (!IsPaladinModel) return;
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
