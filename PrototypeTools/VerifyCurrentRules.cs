var g=UnityEngine.Object.FindFirstObjectByType<BonkSurvivor.SurvivorGame>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
object Call(string n,params object[] a)=>g.GetType().GetMethod(n,flags).Invoke(g,a);
object Field(string n)=>g.GetType().GetField(n,flags).GetValue(g);
void Set(string n,object v)=>g.GetType().GetProperty(n).GetSetMethod(true).Invoke(g,new object[]{v});
void SF(string n,object v)=>g.GetType().GetField(n,flags).SetValue(g,v);
var checks=new System.Collections.Generic.List<string>();
void Check(bool ok,string s){if(!ok)throw new System.Exception(s);checks.Add(s);}
var state=UnityEngine.Random.state;
try
{
 Call("StartGame");g.SelectStarter(0);g.GrantExperience(8);Check(g.Level==2&&g.PendingChoices==1,"XP choice works");g.ChooseUpgrade(0);
 int xp=g.Experience;g.GrantExperience(20);Check(g.Experience!=xp,"XP still accrues outside bosses");
 g.ResetRun();g.SelectStarter(0);
 Set("Elapsed",91f);g.SendMessage("Update");Check(g.EnemyCount==0,"Spawning stops after 90 seconds");
 var landmarks=(System.Collections.IList)Field("landmarks");var portal=landmarks[4];
 var player=(UnityEngine.Transform)Field("player");player.position=((UnityEngine.Transform)portal.GetType().GetField("body").GetValue(portal)).position;
 Check((bool)Call("Interact",portal)&&g.BossActive,"Existing boss summon still works");xp=g.Experience;g.GrantExperience(100);Check(g.Experience==xp,"Boss blocks XP");
 var boss=Field("boss");Check((float)boss.GetType().GetField("health").GetValue(boss)==900,"Boss balance preserved");
 Call("Hit",boss,100000f);Set("Elapsed",94f);Call("TickProgression",.1f);Check(g.Area==2&&!g.BossActive,"Boss death auto-advances area after delay");
 float d=g.damage,s=g.Size,m=g.moveSpeed,a=g.attackRate,p=g.ProjectileSpeed,t=g.EffectDuration,c=g.CritChance;
 foreach(int id in new[]{3,4,6,7,9,16,17})Call("ApplyUpgrade",id);
 Check(UnityEngine.Mathf.Abs(g.damage-d*1.01f)<.001f&&UnityEngine.Mathf.Abs(g.Size-s*1.01f)<.001f&&UnityEngine.Mathf.Abs(g.moveSpeed-m*1.01f)<.001f&&UnityEngine.Mathf.Abs(g.attackRate-a*1.01f)<.001f&&UnityEngine.Mathf.Abs(g.ProjectileSpeed-p*1.01f)<.001f&&UnityEngine.Mathf.Abs(g.EffectDuration-t*1.01f)<.001f&&UnityEngine.Mathf.Abs(g.CritChance-c-.01f)<.001f,"All current 1% Tome rules preserved");
 g.ResetRun();g.SelectStarter(0);
 foreach(int id in new[]{1,2,10,11,12,13,14,15})Call("ApplyUpgrade",id);
 for(int id=18;id<39;id++)Call("ApplyUpgrade",id);
 Check(g.WeaponCount==30,"All 30 weapons equipped together");Set("Quantity",6);
 for(int i=0;i<40;i++)
 {
   Call("SpawnEnemy");var es=(System.Collections.IList)Field("enemies");var e=es[es.Count-1];float angle=i*UnityEngine.Mathf.PI*2/40;
   ((UnityEngine.Transform)e.GetType().GetField("body").GetValue(e)).position=g.PlayerPosition+new UnityEngine.Vector3(UnityEngine.Mathf.Sin(angle),0,UnityEngine.Mathf.Cos(angle))*6;
   e.GetType().GetField("health").SetValue(e,100000f);e.GetType().GetField("elite").SetValue(e,true);
 }
 for(int i=0;i<200;i++){Call("TickArsenal",.05f);Call("AttackWeapons");Call("UpdateBolts",.05f);Call("TickProgression",.05f);}
 Check(g.AdvancedProjectileCount<=200&&g.ArsenalProjectileCount<=180&&g.AdvancedZoneCount<=48&&g.FlameCount<=80,"Ten seconds of maximum-quantity mixed arsenal stay within caps");
 Set("ShopOpen",true);float elapsed=g.Elapsed;g.SendMessage("Update");Check(g.Elapsed==elapsed,"Shop pauses current game loop");
 g.ResetRun();Check(g.WeaponCount==0&&g.AdvancedZoneCount==0&&g.AdvancedProjectileCount==0&&g.FlameCount==0&&g.OrbitCount==0,"Reset clears combined arsenal");
}
finally {UnityEngine.Random.state=state;SF("boss",null);g.ResetRun();}
return checks;
