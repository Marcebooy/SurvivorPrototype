using System.IO;
using UnityEditor;
using UnityEngine;

namespace BonkSurvivor.Editor
{
    // Fixes up the import of Kaksoiskajo_Sword.fbx / Kaksoiskajo_Kaksoistera.fbx under
    // Assets/Survivor/Weapons/Kaksoiskajo/Resources/ (source asset + build notes in
    // Unity/Kaksoiskajo/README.md). Static, unrigged geometry - Scale Factor 1, no animation.
    // Remaps the FBX's per-material submeshes (SilverSteel, AntiqueGold, NavyLeather,
    // TurquoiseCrystal, Lightning) to URP/Lit using the matching *_BaseColor/_MetallicSmoothness/
    // _Emission PNGs imported alongside in the sibling Textures/ folder (MetallicSmoothness maps
    // already imported with sRGB off - R=metallic, A=smoothness, "Metallic Alpha" source per the
    // source package's README).
    //
    // Uses OnPostprocessModel (walks the imported hierarchy's renderers directly) rather than
    // OnPostprocessMaterial - confirmed via a debug marker that OnPostprocessMaterial never fires
    // for this FBX (likely because Unity's own texture-name search already resolves the material
    // before that hook runs), while OnPostprocessModel/OnPreprocessModel reliably do.
    public sealed class KaksoiskajoImportPostprocessor : AssetPostprocessor
    {
        static readonly string[] MaterialNames = { "SilverSteel", "AntiqueGold", "NavyLeather", "TurquoiseCrystal", "Lightning" };

        // HDR-emissiokerroin lähdepaketin READMEn ohjearvoista (kristalli ~2.2, salama ~5);
        // muut materiaalit eivät hehku.
        static float EmissionIntensity(string materialName) => materialName switch
        {
            "Lightning" => 5f,
            "TurquoiseCrystal" => 2.2f,
            _ => 0f,
        };

        bool IsKaksoiskajoModel => assetPath.Replace('\\', '/').Contains("/Kaksoiskajo_");

        void OnPreprocessModel()
        {
            if (!IsKaksoiskajoModel) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.useFileScale = true;
            importer.globalScale = 1f;
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!IsKaksoiskajoModel) return;
            string texturesFolder = Path.GetDirectoryName(assetPath).Replace('\\', '/') + "/Textures/";
            var fixedUp = new System.Collections.Generic.HashSet<Material>();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var material = mats[i];
                    if (material == null || !fixedUp.Add(material)) continue;
                    ApplyKaksoiskajoMaterial(material, texturesFolder);
                }
            }
        }

        static void ApplyKaksoiskajoMaterial(Material material, string texturesFolder)
        {
            bool known = false;
            foreach (var name in MaterialNames) if (name == material.name) { known = true; break; }
            if (!known) return;

            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(texturesFolder + material.name + "_BaseColor.png");
            var metallicMap = AssetDatabase.LoadAssetAtPath<Texture2D>(texturesFolder + material.name + "_MetallicSmoothness.png");
            var emissionMap = AssetDatabase.LoadAssetAtPath<Texture2D>(texturesFolder + material.name + "_Emission.png");

            material.shader = Shader.Find("Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", Color.white);
            if (baseMap) material.SetTexture("_BaseMap", baseMap);

            if (metallicMap)
            {
                material.SetTexture("_MetallicGlossMap", metallicMap);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.SetFloat("_Metallic", 1f);
                material.SetFloat("_Smoothness", 1f);
                material.SetFloat("_SmoothnessTextureChannel", 0f); // 0 = Metallic map alpha (matches "Metallic Alpha" source)
            }

            float intensity = EmissionIntensity(material.name);
            if (emissionMap && intensity > 0f)
            {
                material.SetTexture("_EmissionMap", emissionMap);
                material.SetColor("_EmissionColor", Color.white * intensity);
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
        }
    }
}
