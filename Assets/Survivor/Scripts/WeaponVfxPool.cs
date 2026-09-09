using System.Collections.Generic;
using UnityEngine;
namespace BonkSurvivor
{
    // A map-local, bounded pool. Attached visuals follow in world space so thin projectile
    // and ground-disc transforms cannot squash the particle systems.
    public sealed class WeaponVfxPool : MonoBehaviour
    {
        sealed class Slot {
            public GameObject prefab, instance; public ParticleSystem[] particles;
            public Transform follow; public bool attached, active; public float remaining;
        }
        readonly List<Slot> slots = new List<Slot>();
        public int ActiveCount { get { int n=0; foreach(var s in slots) if(s.active) n++; return n; } }
        public const int Capacity=80;
        public GameObject Play(GameObject prefab,Vector3 position,Quaternion rotation,float scale,Transform follow=null)
        {
            if(!prefab) return null;
            int active=0,bursts=0; Slot slot=null;
            foreach(var s in slots) {
                if(s.active) {active++; if(!s.attached) bursts++;}
                else if(s.prefab==prefab) slot=s;
            }
            if(active>=Capacity || (!follow && bursts>=32)) return null;
            if(slot==null) {
                if(slots.Count>=128) return null;
                var go=Instantiate(prefab,transform); go.SetActive(false);
                slot=new Slot {prefab=prefab,instance=go,particles=go.GetComponentsInChildren<ParticleSystem>(true)};
                slots.Add(slot);
            }
            slot.active=true; slot.attached=follow; slot.follow=follow; slot.remaining=1.6f;
            slot.instance.transform.SetPositionAndRotation(position,rotation);
            slot.instance.transform.localScale=Vector3.one*Mathf.Clamp(scale,.15f,5f);
            slot.instance.SetActive(true);
            foreach(var p in slot.particles) {p.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear); p.Play(false);}
            return slot.instance;
        }
        void LateUpdate()
        {
            foreach(var s in slots) {
                if(!s.active) continue;
                if(s.attached && s.follow) s.instance.transform.SetPositionAndRotation(s.follow.position,s.follow.rotation);
                s.remaining-=Time.deltaTime;
                if((s.attached && !s.follow) || (!s.attached && s.remaining<=0)) {
                    foreach(var p in s.particles) p.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
                    s.instance.SetActive(false); s.active=false; s.follow=null;
                }
            }
        }
    }
    public sealed partial class SurvivorGame
    {
        WeaponVfxCatalog weaponVfxCatalog;
        WeaponVfxPool weaponVfxPool;
        readonly Dictionary<Weapon,Material> weaponZoneMaterials=new Dictionary<Weapon,Material>();
        Material WeaponZoneMaterial(Weapon w) {
            if(weaponZoneMaterials.TryGetValue(w,out var material) && material) return material;
            VfxFor(w);
            EnsureArsenalMaterials(); EnsureAdvancedMaterials();
            material=new Material(Shader.Find("BonkSurvivor/WeaponParticles"));
            material.mainTexture=weaponVfxCatalog.groundTexture;
            Color color=(w==Weapon.Flamewalker ? fireMat : AdvancedMaterial(w)).color; color.a=.38f; material.SetColor("_Tint",color);
            materials.Add(material);weaponZoneMaterials[w]=material;return material;
        }
        WeaponVfxCatalog.Entry VfxFor(Weapon w) {
            if(!weaponVfxCatalog) weaponVfxCatalog=Resources.Load<WeaponVfxCatalog>("WeaponVfxCatalog");
            return weaponVfxCatalog ? weaponVfxCatalog.Find(w) : null;
        }
        WeaponVfxPool VfxPool {
            get { if(!weaponVfxPool) weaponVfxPool=MapEffectRoot.gameObject.AddComponent<WeaponVfxPool>(); return weaponVfxPool; }
        }
        void SpawnImpact(Vector3 pos,Weapon w,float scale=1f) {
            var entry=VfxFor(w); if(entry==null) return;
            VfxPool.Play(entry.impact,pos,Quaternion.identity,Mathf.Min(scale,2.5f));
        }
        void AttachWeaponVfx(Transform body,Weapon w,float scale,bool zone=false) {
            var entry=VfxFor(w); if(entry==null) return;
            VfxPool.Play(zone?entry.zone:entry.flight,body.position,body.rotation,scale,body);
        }
    }
}
