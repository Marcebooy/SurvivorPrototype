// Unity CLI eval_file. Pure layout test: no scene or save mutation.
var hashes=new System.Collections.Generic.HashSet<string>();
string Fingerprint(BonkSurvivor.ProceduralMapLayout map)
{
    var b=new System.Text.StringBuilder();
    foreach(var cell in map.FloorCells) b.Append(cell.x).Append(',').Append(cell.y).Append(';');
    return b.ToString();
}
var seeds=new System.Collections.Generic.List<int>{0,1,-1,int.MinValue,int.MaxValue,81427,12345};
for(int i=0;i<200;i++) seeds.Add(unchecked(i*17439283+91243));
int routes=0;
foreach(int seed in seeds)
{
    var map=new BonkSurvivor.ProceduralMapLayout(seed);
    if(map.Rooms.Count<8 || map.Rooms.Count>12 || map.BossRoom==0) throw new System.Exception("Invalid room roles "+seed);
    if(map.ReachableCellCount!=map.FloorCells.Count) throw new System.Exception("Disconnected seed "+seed);
    for(int x=0;x<BonkSurvivor.ProceduralMapLayout.GridSize;x++)
    {
        bool entered=false,exited=false;
        for(int z=0;z<BonkSurvivor.ProceduralMapLayout.GridSize;z++)
        {
            if(map.IsFloor(x,z)) { if(exited)throw new System.Exception("Internal gap in open map "+seed); entered=true; }
            else if(entered) exited=true;
        }
    }
    for(int x=-12;x<=12;x++)for(int z=-12;z<=12;z++)
        if(!map.IsWalkable(new UnityEngine.Vector3(x*4,0,z*4)))throw new System.Exception("Central combat space blocked "+seed);
    string hash=Fingerprint(map); hashes.Add(hash);
    if(hash!=Fingerprint(new BonkSurvivor.ProceduralMapLayout(seed))) throw new System.Exception("Non-deterministic seed "+seed);
    if(!map.IsWalkable(map.Spawn,1.3f) || !map.IsWalkable(map.BossPosition,1.8f)) throw new System.Exception("Invalid spawn "+seed);
    // Exercise pursuit through corridor bends with the larger boss radius too.
    if(routes<220)
    {
        foreach(var room in map.Rooms)
        {
            var p=map.Center(room); var goal=map.Spawn;
            map.UpdateNavigation(goal);
            int steps=0;
            while((p-goal).sqrMagnitude>1 && steps++<1500)
            {
                p=map.Move(p,map.PursuitDirection(p,goal,1.3f)*.8f,1.3f);
                if(!map.IsWalkable(p,1.3f)) throw new System.Exception("Path left floor "+seed);
            }
            if(steps>=1500) throw new System.Exception("Pursuit stuck, seed="+seed+", room="+room+", p="+p);
            routes++;
        }
    }
    // A long dodge must stop at a wall, never jump to a disconnected part of the footprint.
    var origin=map.Spawn;
    var moved=map.Move(origin,UnityEngine.Vector3.right*400);
    if(!map.IsWalkable(moved) || !map.HasClearPath(origin,moved,.65f)) throw new System.Exception("Wall tunnelling "+seed);
}
if(hashes.Count<200) throw new System.Exception("Insufficient layout variation");
return new { seeds=seeds.Count, uniqueLayouts=hashes.Count, connected=true, deterministic=true, pursuitRoutes=routes, wallTests=seeds.Count };
