using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BonkSurvivor.Editor
{
    public static class AshWastesBuilder
    {
        public const string Root="Assets/Survivor/Resources/AshWastes";
        [MenuItem("Tools/Survivor/Rebuild Tuhkaeramaa art package")]
        public static void Bake()
        {
            Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Meshes");AssetDatabase.Refresh();
            var scene=EditorSceneManager.NewPreviewScene();
            var go=new GameObject("Tuhkaeramaa - editable environment");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,scene);
            try
            {
                var env=go.AddComponent<AshWastesEnvironment>();
                env.ash=Material("01_AshEarth","#66372C");env.sand=Material("02_OchreDust","#804D32");
                env.basalt=Material("03_Basalt","#34221C");env.rust=Material("04_IronOxide","#703023");
                env.charcoal=Material("05_CharredWood","#241A18");env.ember=Material("06_Ember","#F45B16",2f);
                env.hotCore=Material("07_HotCore","#FFBE55",2.5f);
                env.Build();
                foreach(var mesh in env.generatedMeshes)
                {
                    string path=Root+"/Meshes/"+mesh.name.Replace(" ","_")+".asset";
                    var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if(!saved)AssetDatabase.CreateAsset(mesh,path);
                    else
                    {
                        EditorUtility.CopySerialized(mesh,saved);
                        foreach(var f in go.GetComponentsInChildren<MeshFilter>())if(f.sharedMesh==mesh)f.sharedMesh=saved;
                        Object.DestroyImmediate(mesh);
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(go,Root+"/Tuhkaeramaa_Environment.prefab");
                AssetDatabase.SaveAssets();
                Render(go,scene,new Vector3(0,160,-120),Vector3.zero,105,"PrototypeTools/tuhkaeramaa-overview.png");
                Render(go,scene,new Vector3(42,32,-8),new Vector3(16,0,15),24,"PrototypeTools/tuhkaeramaa-landmark.png");
            }
            finally {EditorSceneManager.ClosePreviewScene(scene);}
            AssetDatabase.Refresh();
        }
        static Material Material(string name,string hex,float glow=0)
        {
            string path=Root+"/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            ColorUtility.TryParseHtmlString(hex,out var color);m.color=color;m.SetFloat("_Smoothness",.08f);
            if(glow>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*glow);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}
            EditorUtility.SetDirty(m);return m;
        }
        static void Render(GameObject root,UnityEngine.SceneManagement.Scene scene,Vector3 position,Vector3 target,float size,string output)
        {
            foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
            var cameraObject=new GameObject("Preview camera",typeof(Camera));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,scene);
            var camera=cameraObject.GetComponent<Camera>();camera.cullingMask=1<<30;
            camera.scene=scene;
            camera.transform.position=position;camera.transform.LookAt(target);camera.orthographic=true;camera.orthographicSize=size;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.035f,.033f);camera.farClipPlane=500;
            var lightObject=new GameObject("Preview sun",typeof(Light));UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObject,scene);
            var light=lightObject.GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.6f;light.cullingMask=1<<30;light.transform.rotation=Quaternion.Euler(55,-35,0);
            var rt=new RenderTexture(1280,960,24);var previous=RenderTexture.active;var texture=new Texture2D(1280,960,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1280,960),0,0);texture.Apply();Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllBytes(output,texture.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(lightObject);}
            foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=0;
        }
    }
}
