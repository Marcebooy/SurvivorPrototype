var g=UnityEngine.Object.FindFirstObjectByType<BonkSurvivor.SurvivorGame>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
object Call(string name,params object[] args)=>g.GetType().GetMethod(name,flags).Invoke(g,args);
object Field(string name)=>g.GetType().GetField(name,flags).GetValue(g);
var checks=new System.Collections.Generic.List<string>();
void Check(bool ok,string label){if(!ok)throw new System.Exception(label);checks.Add(label);}
void Spawn(float x,float z,float hp=1)
{
 Call("SpawnEnemy");var es=(System.Collections.IList)Field("enemies");var e=es[es.Count-1];var t=e.GetType();
 ((UnityEngine.Transform)t.GetField("body").GetValue(e)).position=g.PlayerPosition+new UnityEngine.Vector3(x,0,z);t.GetField("health").SetValue(e,hp);
}
try
{
 for(int w=0;w<9;w++){g.ResetRun();Check(g.SelectStarter(w)&&g.WeaponCount==1,"Starter "+w+" available");}
 g.ResetRun();Check(!g.SelectStarter(9)&&!g.SelectStarter(-1),"Out of range starter rejected");
 g.SelectStarter(0);for(int id=10;id<=15;id++)Call("ApplyUpgrade",id);
 Check(g.WeaponCount==7,"All six weapons can join a build");
 Call("ApplyUpgrade",10);Check(g.WeaponLevel(BonkSurvivor.SurvivorGame.Weapon.Chunkers)==2,"New weapon upgrade increases level");
 g.ResetRun();g.SelectStarter(3);Spawn(2.5f,0);Call("TickArsenal",0f);
 Check(g.Kills==1&&g.OrbitCount==2,"Orbiting rocks deal contact damage");
 Call("ApplyUpgrade",5);Call("TickArsenal",.01f);Check(g.OrbitCount==3,"Quantity adds orbiting rock");
 g.ResetRun();g.SelectStarter(4);Spawn(0,0);Call("TickArsenal",.01f);
 Check(g.Kills==1&&g.FlameCount==1,"Flame trail burns enemies");
 var fireList=(System.Collections.IList)Field("flames");var patch=fireList[0];float initialLife=(float)patch.GetType().GetField("life").GetValue(patch);
 Call("TickFlames",.2f);Check((float)patch.GetType().GetField("life").GetValue(patch)<initialLife,"Flames count down");
 Call("TickFlames",10f);Check(g.FlameCount==0,"Flames expire");
 Call("ApplyUpgrade",16);Call("CastArsenal",BonkSurvivor.SurvivorGame.Weapon.Flamewalker,1);patch=fireList[0];
 Check((float)patch.GetType().GetField("life").GetValue(patch)>initialLife,"Duration Tome extends new flames");
 g.ResetRun();g.SelectStarter(5);Spawn(0,3);Spawn(2,3);Call("TickArsenal",.2f);Call("TickShots",.2f);
 Check(g.Kills==2,"Bone bounces to second target");
 g.ResetRun();g.SelectStarter(6);Spawn(0,3);Spawn(1,3);Call("TickArsenal",.2f);
 Check(g.Kills==2,"Fireball explodes into nearby target");
 g.ResetRun();g.SelectStarter(7);Spawn(0,2);Call("TickArsenal",.01f);Check(g.Kills==1,"Aura damages nearby target");
 Spawn(0,5);Call("TickArsenal",1f);Check(g.Kills==1,"Aura respects radius");
 g.ResetRun();g.SelectStarter(8);Spawn(0,3);Call("TickArsenal",.1f);Check(g.Kills>=1,"Shotgun pellets hit");
 Call("TickShots",2f);Check(g.ArsenalProjectileCount==0,"Pellets expire at short range");
 Call("ApplyUpgrade",17);Check(g.ProjectileSpeed>1,"Projectile Speed Tome applies");
 g.ResetRun();g.SelectStarter(0);for(int id=10;id<=15;id++)Call("ApplyUpgrade",id);Spawn(0,7,10000);Call("TickArsenal",.01f);
 Check(g.FlameCount>0&&g.OrbitCount>0&&g.ArsenalProjectileCount>0,"Mixed arsenal runs concurrently");
 int shots=g.ArsenalProjectileCount; g.GetType().GetProperty("ShopOpen").GetSetMethod(true).Invoke(g,new object[]{true});g.SendMessage("Update");
 Check(g.ArsenalProjectileCount==shots,"Shop pauses arsenal simulation");
 Call("AdvanceArea");Check(g.FlameCount==0&&g.OrbitCount==0&&g.ArsenalProjectileCount==0&&g.WeaponCount==7,"Area clears effects and retains weapons");
 g.ResetRun();Check(g.FlameCount==0&&g.OrbitCount==0&&g.ArsenalProjectileCount==0&&g.WeaponCount==0&&g.ProjectileSpeed==1&&g.EffectDuration==1,"Restart clears arsenal and Tome stats");
}
finally{g.ResetRun();}
return checks;
