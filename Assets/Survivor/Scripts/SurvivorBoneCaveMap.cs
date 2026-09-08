using UnityEngine;

namespace BonkSurvivor
{
    public sealed partial class SurvivorGame
    {
        // Fixed Map Type candidate "Luuluola" (bone cave) - mirrors SurvivorForestMap.cs's
        // BuildForestMap(): same outline, same ClampToMap/ResolveMapPosition/MoveOnMap and
        // forestObstacles list (they are generic to any fixed, non-procedural map), only the
        // visuals and obstacle shapes differ. Uses a fixed rng seed so the layout is frozen -
        // identical every run, unlike the procedural dungeon (ProceduralMapLayout) used by default.
        void BuildBoneCaveMap()
        {
            var root = new GameObject("Luuluola - bone cave map").transform; root.SetParent(world, false);
            var earth = MakeMaterial(new Color(.16f, .15f, .16f));
            var floor = MakeMaterial(new Color(.62f, .60f, .55f));
            var dust = MakeMaterial(new Color(.72f, .70f, .63f));
            var path = MakeMaterial(new Color(.58f, .54f, .47f));
            var bone = new[] { MakeMaterial(new Color(.86f, .84f, .77f)), MakeMaterial(new Color(.78f, .76f, .68f)), MakeMaterial(new Color(.70f, .68f, .61f)) };
            var vertices = new Vector3[ForestOutline.Length + 1];
            var triangles = new int[ForestOutline.Length * 3];
            for (int i = 0; i < ForestOutline.Length; i++)
            {
                vertices[i+1] = new Vector3(ForestOutline[i].x, 0, ForestOutline[i].y) * arenaRadius;
                triangles[i*3] = 0; triangles[i*3+1] = (i+1) % ForestOutline.Length+1; triangles[i*3+2] = i+1;
            }
            var caveFloor = FacetedMesh("Luuluola ground", vertices, triangles);
            MapMesh("Cave floor", caveFloor, Vector3.zero, Vector3.one, floor, root);
            MapMesh("Earth bank", caveFloor, Vector3.down * .6f, new Vector3(1.02f,1,1.02f), earth, root);

            // Slim, slightly peaked slab - reads as a headstone/gravemarker from any angle.
            var headstone = FacetedMesh("Headstone slab",
                new[] {
                    new Vector3(0,1,0),
                    new Vector3(-.4f,.75f,-.12f), new Vector3(.4f,.75f,-.12f), new Vector3(.4f,.75f,.12f), new Vector3(-.4f,.75f,.12f),
                    new Vector3(-.5f,0,-.15f), new Vector3(.5f,0,-.15f), new Vector3(.5f,0,.15f), new Vector3(-.5f,0,.15f)
                },
                new[] {
                    0,2,1, 0,3,2, 0,4,3, 0,1,4,
                    1,2,6,1,6,5, 2,3,7,2,7,6, 3,4,8,3,8,7, 4,1,5,4,5,8
                });
            var rng = new System.Random(50301);
            float Next(float lo, float hi) => lo + (float)rng.NextDouble() * (hi-lo);
            // Same worn-path destinations as Mosswood: chests, shrine and portal always stay reachable.
            var destinations = new[] { new Vector3(10,0,9),new Vector3(-18,0,12),new Vector3(20,0,-15),new Vector3(-12,0,-12),new Vector3(0,0,25) };
            foreach (var destination in destinations)
            {
                var end = destination * (arenaRadius/38f);
                int count = Mathf.CeilToInt(end.magnitude / 1.5f);
                var trailVertices = new Vector3[(count+1)*2];
                var trailTriangles = new int[count*6];
                for (int j=0;j<=count;j++)
                {
                    float t=j/(float)count; var p=end*t;
                    p.x += Mathf.Sin(t*Mathf.PI*2)*1.2f; p.y=.018f;
                    var tangent = end + Vector3.right * (Mathf.Cos(t*Mathf.PI*2)*Mathf.PI*2*1.2f);
                    var side = new Vector3(-tangent.z,0,tangent.x).normalized * (1.6f+.18f*Mathf.Sin(t*17));
                    trailVertices[j*2]=p-side; trailVertices[j*2+1]=p+side;
                    if(j==count) continue;
                    int v=j*2, k=j*6;
                    trailTriangles[k]=v; trailTriangles[k+1]=v+1; trailTriangles[k+2]=v+2;
                    trailTriangles[k+3]=v+1; trailTriangles[k+4]=v+3; trailTriangles[k+5]=v+2;
                }
                MapMesh("Bone-dust trail",FacetedMesh("Winding trail",trailVertices,trailTriangles),Vector3.zero,Vector3.one,path,root);
            }
            Shape("Starting clearing",PrimitiveType.Cylinder,Vector3.up*.014f,new Vector3(7,.008f,7),path,root);
            // Widely spaced headstones leave the same generous combat lanes as Mosswood's groves.
            for (int attempt=0;attempt<650 && forestObstacles.Count<95;attempt++)
            {
                var p=new Vector3(Next(-1.5f,1.6f)*arenaRadius,0,Next(-1.2f,1.35f)*arenaRadius);
                if ((ClampToMap(p,8)-p).sqrMagnitude>.01f || p.magnitude<14) continue;
                bool clear=true;
                foreach (var d in destinations)
                {
                    var end=d*(arenaRadius/38f); float t=Mathf.Clamp01(Vector3.Dot(p,end)/end.sqrMagnitude);
                    if ((p-end*t).magnitude<6) { clear=false; break; }
                }
                foreach(var o in forestObstacles) if(new Vector2(p.x-o.x,p.z-o.z).magnitude<10) { clear=false; break; }
                if(!clear) continue;
                float size=Next(.85f,1.4f);
                forestObstacles.Add(new Vector3(p.x,.55f*size,p.z));
                Shape("Bone dust patch",PrimitiveType.Cylinder,p+Vector3.up*.012f,new Vector3(6*size,.008f,5*size),dust,root);
                MapMesh("Headstone",headstone,p,new Vector3(1f,1.4f,1f)*size,bone[rng.Next(bone.Length)],root).Rotate(Next(-6,6),Next(0,360),Next(-6,6));
            }
            // Continuous low headstone rim makes the playable boundary visible, same as Mosswood's stone rim.
            for(int i=0;i<ForestOutline.Length;i++)
            {
                var a=vertices[i+1]; var b=vertices[(i+1)%ForestOutline.Length+1];
                int count=Mathf.CeilToInt(Vector3.Distance(a,b)/3.2f);
                for(int j=0;j<count;j++)
                {
                    var p=Vector3.Lerp(a,b,j/(float)count);
                    MapMesh("Bone cave boundary marker",headstone,p,new Vector3(Next(1.1f,1.5f),Next(1.6f,2.4f),Next(1.1f,1.5f)),bone[rng.Next(bone.Length)],root).Rotate(Next(-4,4),Next(0,360),Next(-4,4));
                }
            }
        }
    }
}
