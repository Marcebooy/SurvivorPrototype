// Run in Play Mode: unity command eval_file --file PrototypeTools/VerifyForestMap.cs
var g = UnityEngine.Object.FindFirstObjectByType<BonkSurvivor.SurvivorGame>();
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
object Field(string name) => g.GetType().GetField(name, flags).GetValue(g);
object Call(string name, params object[] args) => g.GetType().GetMethod(name, flags).Invoke(g, args);
void Set(string name, object value) => g.GetType().GetProperty(name).GetSetMethod(true).Invoke(g, new[] { value });
var checks = new System.Collections.Generic.List<string>();
void Check(bool ok, string message) { if (!ok) throw new System.Exception(message); checks.Add(message); }
var state = UnityEngine.Random.state;
try
{
    g.ResetRun();
    var player = (UnityEngine.Transform)Field("player");
    var outline = (UnityEngine.Vector2[])g.GetType().GetField("ForestOutline", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);
    float area = 0;
    for (int i=0;i<outline.Length;i++) { var a=outline[i]; var b=outline[(i+1)%outline.Length]; area += a.x*b.y-b.x*a.y; }
    Check(area*.5f > UnityEngine.Mathf.PI*1.8f, "Playable footprint is over 1.8 times the old arena");
    var random = new System.Random(42);
    for (int i=0;i<2000;i++)
    {
        var p = g.ResolveMapPosition(new UnityEngine.Vector3((float)(random.NextDouble()-.5)*800, 1, (float)(random.NextDouble()-.5)*800));
        for (int j=0;j<outline.Length;j++)
        {
            var edge=(outline[(j+1)%outline.Length]-outline[j]).normalized;
            var delta=new UnityEngine.Vector2(p.x,p.z)-outline[j]*g.arenaRadius;
            if (UnityEngine.Vector2.Dot(delta,new UnityEngine.Vector2(-edge.y,edge.x)) < 3.14f)
                throw new System.Exception("Boundary projection outside polygon at "+p);
        }
    }
    checks.Add("2000 out-of-bounds positions project inside the stone rim");
    var obstacles=(System.Collections.Generic.List<UnityEngine.Vector3>)Field("forestObstacles");
    Check(obstacles.Count>=60,"Forest has at least 60 trees/boulders with movement obstacles");
    foreach(var o in obstacles)
    {
        var p=new UnityEngine.Vector3(o.x-5,1,o.z);
        for(int step=0;step<100;step++)
        {
            p=g.MoveOnMap(p,UnityEngine.Vector3.right*.15f);
            var delta=new UnityEngine.Vector2(p.x-o.x,p.z-o.z);
            if(delta.magnitude<o.y+.64f) throw new System.Exception("Movement entered a trunk/rock");
        }
        if(p.x<o.x+3) throw new System.Exception("Movement got stuck behind obstacle");
    }
    checks.Add("Head-on movement slides past every forest obstacle without penetration");
    foreach(var landmark in (System.Collections.IList)Field("landmarks"))
    {
        var t=(UnityEngine.Transform)landmark.GetType().GetField("body").GetValue(landmark);
        Check((g.ResolveMapPosition(t.position,3)-t.position).sqrMagnitude<.001f,"Landmark clearing is accessible: "+t.name);
    }
    for(int i=0;i<outline.Length;i++)
    {
        player.position=g.ResolveMapPosition(new UnityEngine.Vector3(outline[i].x,0,outline[i].y)*g.arenaRadius)+UnityEngine.Vector3.up;
        for(int j=0;j<12;j++) Call("SpawnEnemy");
    }
    foreach(var enemy in (System.Collections.IList)Field("enemies"))
    {
        var body=(UnityEngine.Transform)enemy.GetType().GetField("body").GetValue(enemy);
        if((g.ResolveMapPosition(body.position,1)-body.position).sqrMagnitude>.001f) throw new System.Exception("Invalid enemy spawn");
    }
    checks.Add("120 enemy spawns at map corners remain inside map and clear of obstacles");
    g.ResetRun(); g.SelectStarter(0); Set("Elapsed",91f);
    var portal=((System.Collections.IList)Field("landmarks"))[4];
    player.position=((UnityEngine.Transform)portal.GetType().GetField("body").GetValue(portal)).position;
    Check((bool)Call("Interact",portal) && g.BossActive,"Portal summons boss on new map");
    var boss=Field("boss"); var bossBody=(UnityEngine.Transform)boss.GetType().GetField("body").GetValue(boss);
    Check((g.ResolveMapPosition(bossBody.position,1.8f)-bossBody.position).sqrMagnitude<.001f,"Boss spawn is clear of scenery");
    return checks;
}
finally { g.ResetRun(); UnityEngine.Random.state=state; }
