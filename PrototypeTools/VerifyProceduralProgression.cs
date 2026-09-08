// Play Mode integration test. Restores seed preferences and returns to the main menu.
var g=UnityEngine.Object.FindFirstObjectByType<BonkSurvivor.SurvivorGame>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
object Field(string n)=>g.GetType().GetField(n,flags).GetValue(g);
object Call(string n,params object[] a)=>g.GetType().GetMethod(n,flags).Invoke(g,a);
void Set(string n,object v)=>g.GetType().GetProperty(n).GetSetMethod(true).Invoke(g,new[]{v});
void SF(string n,object v)=>g.GetType().GetField(n,flags).SetValue(g,v);
var checks=new System.Collections.Generic.List<string>();
void Check(bool ok,string name) { if(!ok)throw new System.Exception(name);checks.Add(name); }
const string prefix="BonkSurvivor.Prototype.v1.";
var intKeys=new[]{"LastMapSeed","LastMapNumber"};
var saved=new System.Collections.Generic.Dictionary<string,int>();
foreach(var key in intKeys) if(UnityEngine.PlayerPrefs.HasKey(prefix+key))saved[key]=UnityEngine.PlayerPrefs.GetInt(prefix+key);
bool hadHistory=UnityEngine.PlayerPrefs.HasKey(prefix+"MapSeedHistory");
string history=UnityEngine.PlayerPrefs.GetString(prefix+"MapSeedHistory","");
var state=UnityEngine.Random.state;
try
{
    g.enabled=false;g.StartSeededRun(12345);g.SelectStarter(0);
    Check(g.Area==1 && g.CurrentMapSeed==12345 && g.MapSeeds.Count==1,"Seeded run starts in map 1");
    Call("ApplyUpgrade",3);Call("ApplyUpgrade",6);Call("ApplyUpgrade",16);
    float damage=g.damage,size=g.Size,duration=g.EffectDuration;
    int materials=((System.Collections.ICollection)Field("materials")).Count;
    var player=(UnityEngine.Transform)Field("player");
    float lastBossHealth=0;
    for(int cycle=0;cycle<3;cycle++)
    {
        int oldArea=g.Area, oldSeed=g.CurrentMapSeed;
        Set("PendingChoices",0);Set("ShopOpen",false);
        for(int i=0;i<5;i++)Call("SpawnEnemy");
        Set("Elapsed",(float)Field("areaStarted")+91f);
        var portal=((System.Collections.IList)Field("landmarks"))[4];
        player.position=((UnityEngine.Transform)portal.GetType().GetField("body").GetValue(portal)).position;
        Check((bool)Call("Interact",portal) && g.BossActive,"New boss summons on map "+oldArea);
        var boss=Field("boss");float hp=(float)boss.GetType().GetField("health").GetValue(boss);
        Check(hp>lastBossHealth,"Boss health increases on map "+oldArea);lastBossHealth=hp;
        Call("Hit",boss,100000f);
        Check(g.MapTransitionPending,"Boss death schedules transition "+oldArea);
        g.GrantExperience(50);int pending=g.PendingChoices,level=g.Level;
        Set("ShopOpen",true);SF("transitionLeft",0f);
        Call("Update");
        Check(g.Area==oldArea+1 && g.CurrentMapSeed!=oldSeed && !g.MapTransitionPending,"Automatic Update transition completes despite shop/upgrade pause "+oldArea);
        Check(g.WeaponCount==1 && g.WeaponLevel(BonkSurvivor.SurvivorGame.Weapon.Sword)==1 && g.damage==damage && g.Size==size && g.EffectDuration==duration && g.Level==level && g.PendingChoices==pending,"Build and queued upgrades survive transition "+oldArea);
        Check(g.EnemyCount==0 && g.ArsenalProjectileCount==0 && g.FlameCount==0 && ((System.Collections.ICollection)Field("drops")).Count==0,"Old combat state cleared "+oldArea);
        Check(g.MapLayout.IsWalkable(g.PlayerPosition) && (g.PlayerPosition-g.MapSpawn-UnityEngine.Vector3.up).sqrMagnitude<.001f,"Player moved to new starting room "+oldArea);
        Check(((System.Collections.ICollection)Field("materials")).Count==materials && ((System.Collections.ICollection)Field("mapMeshes")).Count==4,"Map materials and meshes do not accumulate "+oldArea);
        Call("AdvanceArea"); Check(g.Area==oldArea+1,"Transition cannot advance twice "+oldArea);
    }
    Check(g.MapSeeds.Count==4 && UnityEngine.PlayerPrefs.GetInt(prefix+"LastMapSeed")==g.CurrentMapSeed,"Four map seeds persisted");
    var loaded=UnityEngine.JsonUtility.FromJson<BonkSurvivor.SurvivorGame.MapSeedHistory>(UnityEngine.PlayerPrefs.GetString(prefix+"MapSeedHistory"));
    Check(loaded.maps.Count==4 && loaded.maps[3].seed==g.CurrentMapSeed && loaded.maps[3].generatorVersion==BonkSurvivor.ProceduralMapLayout.GeneratorVersion,"Seed history round-trips through saved JSON");
    int replaySeed=g.CurrentMapSeed;var previous=g.MapLayout.FloorCells.ToArray();
    g.StartSeededRun(replaySeed);
    Check(System.Linq.Enumerable.SequenceEqual(previous,g.MapLayout.FloorCells),"A later map seed reproduces the exact layout as a new run");
    return checks;
}
catch(System.Exception error) { return "Checks completed: "+string.Join("; ",checks)+" ERROR: "+error.ToString(); }
finally
{
    Set("AtMainMenu",true);g.enabled=true;UnityEngine.Random.state=state;
    foreach(var key in intKeys) { if(saved.ContainsKey(key))UnityEngine.PlayerPrefs.SetInt(prefix+key,saved[key]);else UnityEngine.PlayerPrefs.DeleteKey(prefix+key); }
    if(hadHistory)UnityEngine.PlayerPrefs.SetString(prefix+"MapSeedHistory",history);else UnityEngine.PlayerPrefs.DeleteKey(prefix+"MapSeedHistory");
    UnityEngine.PlayerPrefs.Save();
}
