using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BonkSurvivor.Editor
{
    // Fixes up the import of the three Ritari_*.fbx files under
    // Assets/Survivor/Characters/Ritari/Resources/ (Ritari_Walk_Heavy carries the mesh that gets
    // instantiated; Ritari_Attack_Slash and Ritari_Block exist only so RitariVisual can lift their
    // AnimationClip out and add it onto the walking model's Animation component - see
    // Unity/Knight/README.txt). Legacy rig on all three (so a plain Animation component is enough
    // at runtime, no hand-built Animator Controller needed); only the walk clip loops. Materials
    // are recreated as URP/Lit from the exact colors create_knight.py assigned in Blender (the FBX
    // carries no textures).
    public sealed class RitariImportPostprocessor : AssetPostprocessor
    {
        static readonly Dictionary<string, (Color color, float metallic, float emission)> Materials = new Dictionary<string, (Color, float, float)>
        {
            { "Dark forged steel", (new Color(.085f, .12f, .17f), .72f, 0f) },
            { "Steel bevels", (new Color(.26f, .34f, .43f), .75f, 0f) },
            { "Recesses and joints", (new Color(.012f, .018f, .03f), 0f, 0f) },
            { "Royal blue enamel", (new Color(.018f, .07f, .30f), .3f, 0f) },
            { "Royal blue cape", (new Color(.022f, .045f, .19f), 0f, 0f) },
            { "Old gold", (new Color(.7f, .39f, .065f), .65f, 0f) },
            { "Sword polished steel", (new Color(.52f, .65f, .76f), .8f, 0f) },
            { "Ice blue visor", (new Color(.12f, .7f, 1f), .2f, 3f) },
            { "Blue white block flash", (new Color(.34f, .8f, 1f), 0f, 5f) },
        };

        bool IsRitariModel => assetPath.Replace('\\', '/').Contains("/Ritari_");

        void OnPreprocessModel()
        {
            if (!IsRitariModel) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!IsRitariModel) return;
            var importer = (ModelImporter)assetImporter;
            bool loop = assetPath.Contains("Walk_Heavy");
            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++) { var c = clips[i]; c.loopTime = loop; c.loopPose = loop; clips[i] = c; }
            importer.clipAnimations = clips;
        }

        void OnPostprocessMaterial(Material material)
        {
            if (!IsRitariModel) return;
            if (!Materials.TryGetValue(material.name, out var def)) return;
            material.shader = Shader.Find("Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", def.color);
            material.SetFloat("_Metallic", def.metallic);
            material.SetFloat("_Smoothness", .54f);
            if (def.emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                material.SetColor("_EmissionColor", def.color * def.emission);
            }
        }
    }
}
