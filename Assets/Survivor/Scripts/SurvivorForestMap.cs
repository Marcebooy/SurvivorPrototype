using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    public sealed partial class SurvivorGame
    {
        // Convex, asymmetric outline keeps pursuit paths connected across the whole map.
        // All scenery uses a private seed: host and client build identical geometry.
        static readonly Vector2[] ForestOutline = {
            new Vector2(1.65f, -.35f), new Vector2(1.4f, .65f),
            new Vector2(.65f, 1.25f), new Vector2(-.3f, 1.4f),
            new Vector2(-1.25f, .9f), new Vector2(-1.6f, .15f),
            new Vector2(-1.35f, -.8f), new Vector2(-.55f, -1.15f),
            new Vector2(.55f, -1.25f), new Vector2(1.35f, -.95f)
        };
        readonly List<Mesh> mapMeshes = new List<Mesh>();
        readonly List<Vector3> forestObstacles = new List<Vector3>(); // x/z center, y radius

        public Vector3 ClampToMap(Vector3 position, float margin = 1f)
        {
            if (MapLayout != null) return MapLayout.Resolve(position, margin);
            margin += 2.5f; // Keep feet on the inner side of the visible stone rim.
            // Project onto the inward half-planes; margin is in world units.
            var p = new Vector2(position.x, position.z);
            for (int pass = 0; pass < 4; pass++)
                for (int i = 0; i < ForestOutline.Length; i++)
                {
                    var a = ForestOutline[i] * arenaRadius;
                    var edge = (ForestOutline[(i + 1) % ForestOutline.Length] - ForestOutline[i]).normalized;
                    var inward = new Vector2(-edge.y, edge.x);
                    float d = Vector2.Dot(p - a, inward);
                    if (d < margin) p += inward * (margin - d);
                }
            return new Vector3(p.x, position.y, p.y);
        }

        public Vector3 ResolveMapPosition(Vector3 position, float radius = .65f)
        {
            if (MapLayout != null) return MapLayout.Resolve(position, radius);
            position = ClampToMap(position, radius);
            for (int pass = 0; pass < 3; pass++)
                foreach (var obstacle in forestObstacles)
                {
                    var delta = new Vector2(position.x - obstacle.x, position.z - obstacle.z);
                    float clearance = obstacle.y + radius;
                    if (delta.sqrMagnitude >= clearance * clearance) continue;
                    delta = delta.sqrMagnitude < .00001f ? Vector2.right : delta.normalized;
                    position.x = obstacle.x + delta.x * clearance;
                    position.z = obstacle.z + delta.y * clearance;
                }
            return ClampToMap(position, radius);
        }

        public Vector3 MoveOnMap(Vector3 position, Vector3 motion, float radius = .65f)
        {
            if (MapLayout != null) return MapLayout.Move(position, motion, radius);
            // Substeps prevent rolling or a slow frame from tunnelling through trunks.
            int steps = Mathf.Max(1, Mathf.CeilToInt(motion.magnitude / .35f));
            var step = motion / steps;
            for (int i = 0; i < steps; i++)
            {
                var next = position + step;
                foreach (var obstacle in forestObstacles)
                {
                    var away = new Vector3(position.x - obstacle.x, 0, position.z - obstacle.z);
                    float clearance = obstacle.y + radius;
                    if (away.sqrMagnitude > (clearance + .8f) * (clearance + .8f) || Vector3.Dot(step, away) >= 0) continue;
                    var tangent = new Vector3(-away.z, 0, away.x).normalized;
                    if (Vector3.Dot(tangent, step) < -.001f) tangent = -tangent;
                    next += tangent * step.magnitude;
                }
                position = ResolveMapPosition(next, radius);
            }
            return position;
        }

        Mesh FacetedMesh(string label, Vector3[] vertices, int[] triangles)
        {
            var flat = new Vector3[triangles.Length]; var indices = new int[triangles.Length];
            for (int i = 0; i < triangles.Length; i++) { flat[i] = vertices[triangles[i]]; indices[i] = i; }
            var mesh = new Mesh { name = label, vertices = flat, triangles = indices };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); mapMeshes.Add(mesh); return mesh;
        }

        Transform MapMesh(string label, Mesh mesh, Vector3 p, Vector3 scale, Material material, Transform parent)
        {
            var go = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false); go.transform.localPosition = p; go.transform.localScale = scale;
            go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go.transform;
        }

        void BuildForestMap()
        {
            var root = new GameObject("Mosswood - forest map").transform; root.SetParent(world, false); fixedMapRoot = root;
            var earth = MakeMaterial(new Color(.20f, .26f, .13f));
            var grass = MakeMaterial(new Color(.25f, .36f, .17f));
            var moss = MakeMaterial(new Color(.31f, .40f, .20f));
            var path = MakeMaterial(new Color(.38f, .34f, .22f));
            var bark = MakeMaterial(new Color(.25f, .16f, .095f));
            var leaves = new[] { MakeMaterial(new Color(.12f,.29f,.19f)), MakeMaterial(new Color(.19f,.36f,.20f)), MakeMaterial(new Color(.27f,.40f,.20f)) };
            var rocks = new[] { MakeMaterial(new Color(.39f,.43f,.40f)), MakeMaterial(new Color(.48f,.49f,.42f)), MakeMaterial(new Color(.31f,.37f,.34f)) };
            var vertices = new Vector3[ForestOutline.Length + 1];
            var triangles = new int[ForestOutline.Length * 3];
            for (int i = 0; i < ForestOutline.Length; i++)
            {
                vertices[i+1] = new Vector3(ForestOutline[i].x, 0, ForestOutline[i].y) * arenaRadius;
                triangles[i*3] = 0; triangles[i*3+1] = (i+1) % ForestOutline.Length+1; triangles[i*3+2] = i+1;
            }
            var floor = FacetedMesh("Mosswood ground", vertices, triangles);
            MapMesh("Forest floor", floor, Vector3.zero, Vector3.one, grass, root);
            MapMesh("Earth bank", floor, Vector3.down * .6f, new Vector3(1.02f,1,1.02f), earth, root);

            var coneV = new Vector3[8]; coneV[0] = Vector3.up;
            var coneT = new int[21];
            for (int i=0;i<7;i++) { float a=i*Mathf.PI*2/7; coneV[i+1]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)); coneT[i*3]=0; coneT[i*3+1]=(i+1)%7+1; coneT[i*3+2]=i+1; }
            var cone = FacetedMesh("Seven sided pine", coneV, coneT);
            var rock = FacetedMesh("Weathered boulder", new[] { new Vector3(0,1,0),new Vector3(-1,.25f,-.6f),new Vector3(.7f,.15f,-.85f),new Vector3(1,.3f,.55f),new Vector3(-.6f,.15f,.9f),new Vector3(0,-.15f,0) },
                new[] {0,2,1,0,3,2,0,4,3,0,1,4,5,1,2,5,2,3,5,3,4,5,4,1});
            var rng = new System.Random(81427);
            float Next(float lo, float hi) => lo + (float)rng.NextDouble() * (hi-lo);
            // Worn paths lead toward all existing chests, shrine and the portal.
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
                MapMesh("Woodland trail",FacetedMesh("Winding trail",trailVertices,trailTriangles),Vector3.zero,Vector3.one,path,root);
            }
            Shape("Starting clearing",PrimitiveType.Cylinder,Vector3.up*.014f,new Vector3(7,.008f,7),path,root);
            // Widely spaced groves leave generous combat lanes and landmark clearings.
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
                bool tree=rng.NextDouble()<.72;
                float size=Next(.8f,1.3f);
                forestObstacles.Add(new Vector3(p.x,tree ? .55f*size : 1.5f*size,p.z));
                Shape("Moss patch",PrimitiveType.Cylinder,p+Vector3.up*.012f,new Vector3(6*size,.008f,5*size),moss,root);
                if(tree)
                {
                    Shape("Pine trunk",PrimitiveType.Cylinder,p+Vector3.up*1.6f*size,new Vector3(.85f*size,1.6f*size,.85f*size),bark,root);
                    for(int tier=0;tier<3;tier++)
                        MapMesh("Pine crown",cone,p+Vector3.up*(1.7f+tier*1.25f)*size,new Vector3(2.35f-tier*.5f,3,2.35f-tier*.5f)*size,leaves[rng.Next(leaves.Length)],root);
                }
                else MapMesh("Mossy boulder",rock,p,new Vector3(1.8f,2.3f,1.5f)*size,rocks[rng.Next(rocks.Length)],root).Rotate(0,Next(0,360),0);
            }
            // Continuous low stone rim makes the playable boundary visible.
            for(int i=0;i<ForestOutline.Length;i++)
            {
                var a=vertices[i+1]; var b=vertices[(i+1)%ForestOutline.Length+1];
                int count=Mathf.CeilToInt(Vector3.Distance(a,b)/3.2f);
                for(int j=0;j<count;j++)
                {
                    var p=Vector3.Lerp(a,b,j/(float)count);
                    MapMesh("Forest boundary rock",rock,p,new Vector3(Next(2.1f,2.8f),Next(1.2f,2.6f),Next(1.8f,2.5f)),rocks[rng.Next(rocks.Length)],root).Rotate(0,Next(0,360),0);
                    if(j%3==0)
                        for(int tier=0;tier<3;tier++) MapMesh("Boundary pine",cone,p*1.035f+Vector3.up*(1+tier*1.5f),new Vector3(2.7f-tier*.5f,3.5f,2.7f-tier*.5f),leaves[(i+j)%3],root);
                }
            }
        }
    }
}
