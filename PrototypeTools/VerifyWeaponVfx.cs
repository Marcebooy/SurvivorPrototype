var g=UnityEngine.Object.FindAnyObjectByType<BonkSurvivor.SurvivorGame>();g.enabled=false;
g.StartSeededRun(58214);g.SelectCharacter(BonkSurvivor.SurvivorGame.PlayerCharacter.Pottu);g.SelectStarter(0);
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var t=g.GetType(); object Call(string name,params object[] args)=>t.GetMethod(name,flags).Invoke(g,args);
var list=(System.Collections.IList)t.GetField("enemies",flags).GetValue(g);
var player=(Transform)t.GetProperty("player",flags).GetValue(g);
for(int i=0;i<10;i++) {Call("SpawnEnemy");var e=list[list.Count-1];var et=e.GetType();et.GetField("health").SetValue(e,100000f);var body=(Transform)et.GetField("body").GetValue(e);body.position=player.position+Quaternion.Euler(0,i*36,0)*Vector3.forward*(i%2==0?3f:7f);}
var levels=(System.Collections.IDictionary)t.GetProperty("weaponLevels",flags).GetValue(g);
var tested=new System.Collections.Generic.List<string>();
for(int i=0;i<30;i++) {
 var w=(BonkSurvivor.SurvivorGame.Weapon)i;levels.Clear();levels[w]=1;
 if(i<3)Call("AttackWeapons");else if(i<9)Call("CastArsenal",w,1);else Call("CastAdvanced",w,1);
 if(i==3)Call("TickOrbits",.3f);
 if(i==10)Call("BlockWithAegis");
 for(int step=0;step<8;step++) {Call("TickShots",.05f);Call("TickAdvancedShots",.05f);Call("TickFlames",.05f);Call("TickAdvancedZones",.05f);Call("UpdateBolts",.05f);}
 tested.Add(w.ToString());
}
var catalog=Resources.Load<BonkSurvivor.WeaponVfxCatalog>("WeaponVfxCatalog");
var prefabs=catalog.entries.SelectMany(e=>new[]{e.impact,e.flight,e.zone}).Where(p=>p).ToArray();
var pool=UnityEngine.Object.FindAnyObjectByType<BonkSurvivor.WeaponVfxPool>();
for(int i=0;i<200;i++)pool.Play(catalog.entries[i%30].impact,player.position,Quaternion.identity,1);
if(pool.ActiveCount>BonkSurvivor.WeaponVfxPool.Capacity)throw new System.Exception("VFX capacity exceeded");
return new {tested,active=pool.ActiveCount,projectiles=g.ArsenalProjectileCount+g.AdvancedProjectileCount,zones=g.AdvancedZoneCount,missingMaterials=prefabs.SelectMany(p=>p.GetComponentsInChildren<Renderer>(true)).Count(r=>!r.sharedMaterial || !r.sharedMaterial.shader.isSupported),badScripts=prefabs.SelectMany(p=>p.GetComponentsInChildren<MonoBehaviour>(true)).Count()};
