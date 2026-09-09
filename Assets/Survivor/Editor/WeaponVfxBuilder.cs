using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace BonkSurvivor.Editor
{
    public static class WeaponVfxBuilder
    {
        const string Output="Assets/Survivor/WeaponVfx";
        const string Toon="Assets/Magic Pig Games (Infinity PBR)/HOVL - Toon Projectiles 2/";
        const string Unique="Assets/Magic Pig Games (Infinity PBR)/Unique Projectiles Volume 2/";
        static readonly Color Steel=new Color(.82f,.9f,1), Gold=new Color(1,.72f,.2f), Fire=new Color(1,.28f,.045f), Ice=new Color(.22f,.8f,1), Poison=new Color(.35f,1,.12f), Void=new Color(.55f,.16f,.95f), Blood=new Color(1,.08f,.14f);
        static Material fallback;
        static string Impact(string name)=>Toon+"Particles/Impacts/Toon Projectiles 2 - "+name+" Impact.prefab";
        static string Flight(string name)=>Toon+"Projectiles/Toon Projectiles 2 - "+name+".prefab";
        static string Orb(string name)=>Unique+"Projectiles/Unique Projectiles Vol 2 - "+name+".prefab";
        public static string Build()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before building VFX");
            if(!AssetDatabase.IsValidFolder(Output)) AssetDatabase.CreateFolder("Assets/Survivor","WeaponVfx");
            fallback=AssetDatabase.LoadAssetAtPath<Material>(Output+"/RestoredParticles.mat");
            if(!fallback) { fallback=new Material(Shader.Find("BonkSurvivor/WeaponParticles")); AssetDatabase.CreateAsset(fallback,Output+"/RestoredParticles.mat"); }
            fallback.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Eric VFX Studio/Resource/Textures/Radial Glow.png"); EditorUtility.SetDirty(fallback);
            var catalog=AssetDatabase.LoadAssetAtPath<WeaponVfxCatalog>("Assets/Survivor/Resources/WeaponVfxCatalog.asset");
            if(!catalog) {catalog=ScriptableObject.CreateInstance<WeaponVfxCatalog>();AssetDatabase.CreateAsset(catalog,"Assets/Survivor/Resources/WeaponVfxCatalog.asset");}
            catalog.entries=new WeaponVfxCatalog.Entry[SurvivorGame.TotalWeaponCount];
            catalog.groundTexture=(Texture2D)fallback.mainTexture;
            void Add(SurvivorGame.Weapon w,string impact,Color color,string description,string flight=null,string zone=null) {
                catalog.entries[(int)w]=new WeaponVfxCatalog.Entry {weapon=w,description=description,
                    impact=Convert(impact,w+"Impact",color,false,2),
                    flight=flight==null?null:Convert(flight,w+"Flight",color,true,.9f),
                    zone=zone==null?null:Convert(zone,w+"Zone",color,true,2)};
            }
            const string slash="Assets/Eric VFX Studio/Free Game VFX/Prefab/FX_Orange_Slash_1.prefab";
            Add(SurvivorGame.Weapon.Sword,slash,Gold,"Warm melee slash");
            Add(SurvivorGame.Weapon.Bow,Impact("Arrow"),Steel,"Arrow trail and pale impact",Flight("Arrow"));
            Add(SurvivorGame.Weapon.Lightning,"Assets/Vefects/Zap VFX URP/VFX/Zap/Particles/VFX_Zap_02_Blue.prefab",Ice,"Blue lightning strike");
            Add(SurvivorGame.Weapon.Chunkers,Impact("Magic Stone"),new Color(.65f,.5f,.3f),"Stone dust");
            Add(SurvivorGame.Weapon.Flamewalker,Impact("Explode"),Fire,"Small persistent ground flames",null,Orb("Fireball03 Orange"));
            Add(SurvivorGame.Weapon.Bone,Impact("Magic Stone"),new Color(.9f,.85f,.65f),"Bone dust and pale trail",Flight("Magic Stone"));
            Add(SurvivorGame.Weapon.Firestaff,Impact("Explode"),Fire,"Fireball flight and separate explosion",Orb("Fireball03 Orange"));
            Add(SurvivorGame.Weapon.Aura,Impact("Energy Orb"),Gold,"Holy aura motes",null,Flight("Energy Orb"));
            Add(SurvivorGame.Weapon.Shotgun,Impact("Bullet"),Gold,"Compact pellet sparks",Flight("Bullet"));
            Add(SurvivorGame.Weapon.Revolver,Impact("Bullet"),Gold,"Ricochet sparks",Flight("Bullet"));
            Add(SurvivorGame.Weapon.Aegis,Impact("Energy Orb"),Gold,"Golden shield counterattack");
            Add(SurvivorGame.Weapon.Bananarang,Impact("Arrow"),new Color(1,.85f,.08f),"Yellow returning trail",Flight("Wind"));
            Add(SurvivorGame.Weapon.Axe,slash,Steel,"Steel spinning cut",Flight("Wind"));
            Add(SurvivorGame.Weapon.SpaceNoodle,Impact("Cosmic"),Void,"Violet link impact");
            Add(SurvivorGame.Weapon.Sniper,Impact("Bullet"),Steel,"Sharp pale shot impact");
            Add(SurvivorGame.Weapon.Rocket,Impact("Explode"),Fire,"Rocket exhaust and explosion",Orb("Fireball03 Orange"));
            Add(SurvivorGame.Weapon.Mines,Impact("Explode"),Fire,"Explosion only after mine triggers");
            Add(SurvivorGame.Weapon.WirelessDagger,Impact("Magic Arrow"),Ice,"Blue homing blade",Flight("Magic Arrow"));
            Add(SurvivorGame.Weapon.Frostwalker,Impact("Ice"),Ice,"Icy ground motes",null,Flight("Ice"));
            Add(SurvivorGame.Weapon.Tornado,Impact("Wind"),new Color(.6f,.9f,.9f),"Wind swirl",null,Flight("Wind"));
            Add(SurvivorGame.Weapon.Dexecutioner,slash,Blood,"Red execution slash");
            Add(SurvivorGame.Weapon.BloodMagic,Impact("Red"),Blood,"Crimson blood burst");
            Add(SurvivorGame.Weapon.BlackHole,Impact("Cosmic"),Void,"Violet gravity core",null,Orb("BlackHole01 Purple"));
            Add(SurvivorGame.Weapon.PoisonFlask,Impact("Forest"),Poison,"Green lingering poison",null,Orb("Orb02 Green"));
            Add(SurvivorGame.Weapon.Katana,slash,Steel,"Fast pale cut");
            Add(SurvivorGame.Weapon.DragonBreath,Impact("Explode"),Fire,"Orange cone-end flame bursts");
            Add(SurvivorGame.Weapon.Dice,Impact("Cube"),new Color(1,.3f,.85f),"Pink dice burst");
            Add(SurvivorGame.Weapon.HeroSword,slash,Gold,"Golden slash wave",Flight("Magic Arrow"));
            Add(SurvivorGame.Weapon.CorruptedSword,slash,Void,"Corrupted violet cut");
            Add(SurvivorGame.Weapon.Scythe,slash,new Color(.5f,1,.65f),"Spectral green harvest slash");
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();return "Mapped "+catalog.entries.Length+" weapons";
        }
        static GameObject Convert(string path,string name,Color tint,bool loop,float diameter)
        {
            if(!AssetDatabase.LoadAssetAtPath<GameObject>(path)) throw new InvalidOperationException("Missing VFX: "+path);
            var contents=PrefabUtility.LoadPrefabContents(path);
            var root=new GameObject(name); UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,contents.scene); contents.transform.SetParent(root.transform,false);
            // Imported projectile prefabs carry positions from their vendor demo scenes.
            contents.transform.localPosition=Vector3.zero; contents.transform.localRotation=Quaternion.identity;
            try {
                foreach(var tr in contents.GetComponentsInChildren<Transform>(true)) { tr.gameObject.layer=0; GameObjectUtility.RemoveMonoBehavioursWithMissingScript(tr.gameObject); }
                foreach(var script in contents.GetComponentsInChildren<MonoBehaviour>(true)) UnityEngine.Object.DestroyImmediate(script);
                foreach(var light in contents.GetComponentsInChildren<Light>(true)) UnityEngine.Object.DestroyImmediate(light);
                foreach(var collider in contents.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
                foreach(var rb in contents.GetComponentsInChildren<Rigidbody>(true)) UnityEngine.Object.DestroyImmediate(rb);
                foreach(var audio in contents.GetComponentsInChildren<AudioSource>(true)) UnityEngine.Object.DestroyImmediate(audio);
                foreach(var animator in contents.GetComponentsInChildren<Animator>(true)) UnityEngine.Object.DestroyImmediate(animator);
                foreach(var ps in contents.GetComponentsInChildren<ParticleSystem>(true)) {
                    ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
                    var main=ps.main;main.loop=loop;main.playOnAwake=true;main.stopAction=ParticleSystemStopAction.None;main.scalingMode=ParticleSystemScalingMode.Hierarchy;main.maxParticles=Mathf.Min(main.maxParticles,40);
                    main.startColor=new Color(tint.r,tint.g,tint.b,loop?.55f:.85f);
                    main.startLifetimeMultiplier=Mathf.Min(main.startLifetimeMultiplier,loop?1.2f:.8f);
                    var collision=ps.collision;collision.enabled=false;var triggers=ps.trigger;triggers.enabled=false;
                    var r=ps.GetComponent<ParticleSystemRenderer>();
                    if(r) {
                        if(!r.sharedMaterial || !r.sharedMaterial.shader.isSupported || r.sharedMaterial.shader.name.StartsWith("Legacy")) {
                            r.sharedMaterial=fallback;
                            var sheet=ps.textureSheetAnimation; sheet.enabled=false;
                        }
                        if(!r.trailMaterial) r.trailMaterial=fallback;
                        if(r.renderMode==ParticleSystemRenderMode.Mesh && !r.mesh) r.renderMode=ParticleSystemRenderMode.Billboard;
                        r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
                    }
                    var shape=ps.shape;
                    if(shape.enabled && (shape.shapeType==ParticleSystemShapeType.Mesh || shape.shapeType==ParticleSystemShapeType.MeshRenderer || shape.shapeType==ParticleSystemShapeType.SkinnedMeshRenderer)) shape.enabled=false;
                    ps.Simulate(.3f,false,true,true);
                }
                var renderers=contents.GetComponentsInChildren<Renderer>(true);
                foreach(var r in renderers) if(!r.sharedMaterial || !r.sharedMaterial.shader.isSupported) r.sharedMaterial=fallback;
                var bounds=new Bounds(root.transform.position,Vector3.one*.1f);
                foreach(var r in renderers) if(r.enabled) bounds.Encapsulate(r.bounds);
                float span=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
                contents.transform.localScale*=Mathf.Clamp(diameter/Mathf.Max(.1f,span),.015f,2f);
                // Zap has a tall sky bolt; fitting its full height into a two-unit impact
                // would make the ground flash invisible at the gameplay camera distance.
                if(name=="LightningImpact") contents.transform.localScale=Vector3.one*.6f;
                foreach(var ps in contents.GetComponentsInChildren<ParticleSystem>(true)) ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
                return PrefabUtility.SaveAsPrefabAsset(root,Output+"/"+name+".prefab");
            }
            finally { contents.transform.SetParent(null);PrefabUtility.UnloadPrefabContents(contents);UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
