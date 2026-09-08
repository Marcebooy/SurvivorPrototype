var g=UnityEngine.Object.FindFirstObjectByType<BonkSurvivor.SurvivorGame>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
object Call(string n,params object[] a)=>g.GetType().GetMethod(n,flags).Invoke(g,a);
object Field(string n)=>g.GetType().GetField(n,flags).GetValue(g);
void Set(string n,object v)=>g.GetType().GetProperty(n).GetSetMethod(true).Invoke(g,new object[]{v});
void SetField(string n,object v)=>g.GetType().GetField(n,flags).SetValue(g,v);
var results=new System.Collections.Generic.List<string>();
void Check(bool ok,string label){if(!ok)throw new System.Exception(label);results.Add(label);}
void Fresh(int w){g.ResetRun();Check(g.SelectStarter(w),"Select "+w);Set("CritChance",0f);}
object Spawn(float x,float z,float hp=1,bool elite=false)
{
 Call("SpawnEnemy");var list=(System.Collections.IList)Field("enemies");var e=list[list.Count-1];var t=e.GetType();
 ((UnityEngine.Transform)t.GetField("body").GetValue(e)).position=g.PlayerPosition+new UnityEngine.Vector3(x,0,z);
 t.GetField("health").SetValue(e,hp);t.GetField("elite").SetValue(e,elite);return e;
}
float HP(object e)=>(float)e.GetType().GetField("health").GetValue(e);
UnityEngine.Transform Body(object e)=>(UnityEngine.Transform)e.GetType().GetField("body").GetValue(e);
void Cast(int w)=>Call("CastAdvanced",(BonkSurvivor.SurvivorGame.Weapon)w,g.WeaponLevel((BonkSurvivor.SurvivorGame.Weapon)w));
try
{
 Call("StartGame");
 var names=(string[])g.GetType().GetField("UpgradeNames",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
 Check(names.Length==39,"39 upgrade entries: 30 weapons and 9 Tomes");
 for(int w=0;w<30;w++){g.ResetRun();Check(g.SelectStarter(w)&&g.WeaponCount==1,"Starter "+w+" works");}
 g.ResetRun();Check(!g.SelectStarter(30)&&!g.SelectStarter(-1),"Starter bounds");
 g.SelectStarter(0);for(int id=18;id<39;id++)Call("ApplyUpgrade",id);
 Check(g.WeaponCount==22,"All 21 new upgrade IDs grant correct weapons");
 float dmg=g.damage,size=g.Size;Call("ApplyUpgrade",18);
 Check(g.WeaponLevel(BonkSurvivor.SurvivorGame.Weapon.Revolver)==2&&(g.damage>dmg||g.Size>size),"Owned weapon grants existing bonus rule");
 Fresh(9);Spawn(0,3);Spawn(2,3);Cast(9);Call("TickAdvancedShots",.1f);Call("TickAdvancedShots",.15f);Check(g.Kills==2,"Revolver burst bounces");
 Fresh(10);Spawn(0,2);Cast(10);float hp=g.Health;
 Check(!(bool)Call("ReceiveDamage",32f)&&g.Health==hp&&g.ShieldCharges==0&&g.Kills==1,"Aegis blocks and retaliates");
 SetField("invulnerability",0f);Check((bool)Call("ReceiveDamage",13f)&&g.Health<hp,"Consumed shield does not block forever");
 Call("TickAdvanced",.01f);Check(g.ShieldCharges==1,"Aegis recharge");
 Fresh(11);var e=Spawn(0,3,1000);Cast(11);Call("TickAdvancedShots",.2f);float h=HP(e);Call("TickAdvancedShots",.4f);
 Check(h<1000&&HP(e)<h,"Bananarang hits outbound and returning");
 Call("TickAdvancedShots",5f);Check(g.AdvancedProjectileCount==0,"Bananarang cleans up on return or timeout");
 Fresh(12);Spawn(0,2);Spawn(0,3);Cast(12);Call("TickAdvancedShots",.5f);Check(g.Kills==2,"Axe pierces multiple enemies");
 Fresh(13);Spawn(0,3);var end=Spawn(0,6);Call("AddAdvancedZone",BonkSurvivor.SurvivorGame.Weapon.SpaceNoodle,g.PlayerPosition,UnityEngine.Vector3.forward,end,g.damage,.65f,2f);
 Call("TickAdvancedZones",.1f);Check(g.Kills==2,"Space Noodle hits link and endpoint");Call("TickAdvancedZones",.1f);Check(g.AdvancedZoneCount==0,"Noodle cleans up dead target");
 Fresh(14);Spawn(0,3);Spawn(0,12);Cast(14);Check(g.Kills==2,"Sniper penetrates targets");
 Fresh(15);e=Spawn(0,5);Spawn(1,5);Cast(15);Body(e).position+=UnityEngine.Vector3.right;
 Call("TickAdvancedShots",.2f);Call("TickAdvancedShots",.4f);Check(g.Kills==2,"Rocket homes and explodes");
 Fresh(16);Spawn(0,1);Spawn(1,1);Cast(16);Call("TickAdvancedZones",.1f);Check(g.Kills==0,"Mine arming delay");Call("TickAdvancedZones",.4f);Check(g.Kills==2&&g.AdvancedZoneCount==0,"Mine proximity explosion is one-shot");
 Fresh(17);Spawn(0,3);Spawn(2,3);Cast(17);Call("TickAdvancedShots",.2f);Call("TickAdvancedShots",.2f);Check(g.Kills==2,"Wireless dagger homes and bounces");
 Fresh(18);e=Spawn(0,1,1000);Cast(18);Call("TickAdvancedZones",.1f);
 Check(HP(e)<1000&&(float)Call("EnemyMoveMultiplier",e)==0,"Frostwalker damages and freezes");
 var bossTarget=Spawn(1,1,1000,true);SetField("boss",bossTarget);Call("ChillEnemy",bossTarget);
 Check((float)Call("EnemyMoveMultiplier",bossTarget)>.0f&&(float)Call("EnemyMoveMultiplier",bossTarget)<1,"Boss slows without freezing");SetField("boss",null);
 Call("ClearAdvanced",false);Check((float)Call("EnemyMoveMultiplier",e)==1,"Cleanup removes slow status");
 Fresh(19);e=Spawn(2,2,1000);Cast(19);var before=Body(e).position;Call("TickAdvancedZones",.1f);
 Check(HP(e)<1000&&Body(e).position.x>before.x,"Tornado damages and pushes");
 Fresh(20);e=Spawn(0,2,10000);var elite=Spawn(1,2,10000,true);
 Check((bool)Call("CanExecute",e)&&!(bool)Call("CanExecute",elite),"Execution excludes elites");SetField("boss",e);Check(!(bool)Call("CanExecute",e),"Execution excludes boss even without elite flag");SetField("boss",null);
 Cast(20);Check(HP(e)<10000,"Dexecutioner attacks");
 Fresh(21);Set("Health",50f);e=Spawn(0,3,100);Cast(21);Check(HP(e)<100&&g.Health==51,"Blood Magic drains and heals");
 float max=g.maxHealth;for(int i=0;i<10;i++){var victim=Spawn(3,3);Call("Hit",victim,10000f);}
 Check(g.maxHealth==max+1&&g.BloodHealthGained==1,"Ten Blood Magic kills grant run HP");
 Fresh(22);e=Spawn(2,3,1000);Cast(22);Body(e).position+=UnityEngine.Vector3.right;float x=Body(e).position.x;Call("TickAdvancedZones",.1f);
 // Hit also applies the existing knockback; use a non-damage tick to isolate attraction.
 x=Body(e).position.x;Call("TickAdvancedZones",.1f);Check(HP(e)<1000&&Body(e).position.x<x,"Black Hole pulls and damages");
 Fresh(23);e=Spawn(0,3,1000);Cast(23);Call("TickAdvancedZones",.1f);h=HP(e);Call("TickAdvancedZones",.5f);Check(h<1000&&HP(e)<h,"Poison ticks repeatedly");
 Fresh(24);e=Spawn(0,3,1000);Cast(24);h=HP(e);Cast(24);Check(h<1000&&HP(e)<h,"Katana repeat strike");
 Fresh(25);e=Spawn(0,3);var behind=Spawn(0,-4,1000);Cast(25);Check(g.Kills==1&&HP(behind)==1000,"Dragon cone excludes rear target");
 Fresh(26);e=Spawn(0,3,100000);bool six=false;var rng=UnityEngine.Random.state;UnityEngine.Random.InitState(125);
 for(int i=0;i<80;i++){Body(e).position=g.PlayerPosition+UnityEngine.Vector3.forward*3;Cast(26);Check(g.LastDiceRoll>=1&&g.LastDiceRoll<=6,"Dice range "+i);if(g.LastDiceRoll==6)six=true;}
 UnityEngine.Random.state=rng;Check(six&&g.CritChance>0,"Sixes increase crit");
 Fresh(27);Spawn(0,3);Spawn(0,9);Cast(27);Call("TickAdvancedShots",.55f);Check(g.Kills==2,"Hero Sword melee and ranged wave");
 Fresh(28);e=Spawn(0,3,1000);Cast(28);float fullDamage=1000-HP(e);Body(e).position=g.PlayerPosition+UnityEngine.Vector3.forward*3;Set("Health",g.maxHealth*.2f);h=HP(e);Cast(28);Check(h-HP(e)>fullDamage*2,"Corrupted Sword scales with missing HP");
 Fresh(29);for(int i=0;i<3;i++){var victim=Spawn(0,8,1,true);Call("Hit",victim,100f);}Check(g.ScytheCharge==3,"Elite kills charge Scythe");
 e=Spawn(0,-3,1000);Cast(29);Check(HP(e)<=1000-g.damage*3&&g.ScytheCharge==0,"Charged Scythe hits behind and consumes charge");
 // Respect the user's current balance and boss progression.
 Fresh(0);dmg=g.damage;Call("ApplyUpgrade",3);Check(UnityEngine.Mathf.Abs(g.damage-dmg*1.01f)<.001f,"One-percent Tome balance preserved");
 for(int id=18;id<39;id++)Call("ApplyUpgrade",id);Spawn(0,3,100000);Call("TickArsenal",.01f);
 Check(g.AdvancedProjectileCount>0&&g.AdvancedZoneCount>0,"Full arsenal runs concurrently");
 Set("ShopOpen",true);int count=g.AdvancedProjectileCount;g.SendMessage("Update");Check(count==g.AdvancedProjectileCount,"Shop pauses advanced arsenal");Set("ShopOpen",false);
 Call("AdvanceArea");Check(g.WeaponCount==22&&g.AdvancedProjectileCount==0&&g.AdvancedZoneCount==0,"Area transition keeps build and clears effects");
 g.ResetRun();Check(g.BloodHealthGained==0&&g.ScytheCharge==0&&g.ShieldCharges==0&&g.AdvancedZoneCount==0,"New run resets advanced state");
 Check(g.arenaRadius==60,"Larger arena retained");
}
finally {SetField("boss",null);g.ResetRun();}
return new {count=results.Count,checks=results};
