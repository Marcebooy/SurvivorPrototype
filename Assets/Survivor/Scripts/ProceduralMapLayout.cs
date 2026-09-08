using System;
using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    // Version the algorithm when changing seed semantics. No UnityEngine.Random or scene state.
    public sealed class ProceduralMapLayout
    {
        public const int GeneratorVersion = 1, GridSize = 65;
        public const float CellSize = 4f;
        const int Origin = GridSize / 2, RoomSpacing = 11;
        public int Seed { get; }
        public readonly List<RectInt> Rooms = new List<RectInt>();
        public readonly List<Vector2Int> FloorCells = new List<Vector2Int>();
        public int BossRoom { get; private set; }
        public Vector3 Spawn => Center(Rooms[0]);
        public Vector3 BossPosition => Center(Rooms[BossRoom]);
        readonly bool[,] floor = new bool[GridSize, GridSize];
        readonly int[,] distance = new int[GridSize, GridSize];
        readonly Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Vector2Int targetCell = new Vector2Int(-1, -1);
        static readonly Vector2Int[] Directions = { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };

        struct SeedRandom
        {
            uint state;
            public SeedRandom(int seed) { state = unchecked((uint)seed) ^ 0xA3C59AC3u; if(state==0) state=1; }
            public int Next(int min, int max) { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return min + (int)(state % (uint)(max-min)); }
        }

        public ProceduralMapLayout(int seed)
        {
            Seed = seed;
            var rng = new SeedRandom(seed);
            int roomCount = rng.Next(8, 13);
            var slots = new List<Vector2Int> { Vector2Int.zero };
            var links = new List<Vector2Int>();
            // Grow a spanning tree on coarse room slots. Every added room has a parent.
            while (slots.Count < roomCount)
            {
                var candidates = new List<Vector2Int>();
                for (int parent=0;parent<slots.Count;parent++)
                    for (int d=0;d<4;d++)
                    {
                        var slot=slots[parent]+Directions[d];
                        if(Mathf.Abs(slot.x)<=2 && Mathf.Abs(slot.y)<=2 && !slots.Contains(slot)) candidates.Add(new Vector2Int(parent,d));
                    }
                var edge=candidates[rng.Next(0,candidates.Count)];
                slots.Add(slots[edge.x]+Directions[edge.y]); links.Add(new Vector2Int(edge.x,slots.Count-1));
            }
            foreach(var slot in slots)
            {
                int halfX=rng.Next(3,5), halfZ=rng.Next(3,5);
                var center=slot*RoomSpacing+new Vector2Int(Origin,Origin);
                var room=new RectInt(center.x-halfX,center.y-halfZ,halfX*2+1,halfZ*2+1);
                Rooms.Add(room);
                Carve(room);
            }
            // Optional loops offer alternate routes without compromising connectivity.
            for(int a=0;a<slots.Count;a++)
                for(int b=a+1;b<slots.Count;b++)
                    if((slots[a]-slots[b]).sqrMagnitude==1 && rng.Next(0,3)==0) links.Add(new Vector2Int(a,b));
            foreach(var link in links)
            {
                var a=ToCell(Center(Rooms[link.x])); var b=ToCell(Center(Rooms[link.y]));
                Carve(new RectInt(Mathf.Min(a.x,b.x)-1,Mathf.Min(a.y,b.y)-1,Mathf.Abs(a.x-b.x)+3,Mathf.Abs(a.y-b.y)+3));
            }
            for(int x=0;x<GridSize;x++) for(int z=0;z<GridSize;z++) if(floor[x,z]) FloorCells.Add(new Vector2Int(x,z));
            UpdateNavigation(Spawn);
            int farthest=-1;
            for(int i=1;i<Rooms.Count;i++)
            {
                var cell=ToCell(Center(Rooms[i]));
                if(distance[cell.x,cell.y]>farthest) { farthest=distance[cell.x,cell.y]; BossRoom=i; }
            }
        }

        void Carve(RectInt r) { for(int x=r.xMin;x<r.xMax;x++) for(int z=r.yMin;z<r.yMax;z++) floor[x,z]=true; }
        public bool IsFloor(int x,int z) => x>=0 && z>=0 && x<GridSize && z<GridSize && floor[x,z];
        public Vector2Int ToCell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x/CellSize+.5f)+Origin,Mathf.FloorToInt(p.z/CellSize+.5f)+Origin);
        public Vector3 ToWorld(Vector2Int c) => new Vector3((c.x-Origin)*CellSize,0,(c.y-Origin)*CellSize);
        public Vector3 Center(RectInt r) => ToWorld(new Vector2Int(r.xMin+r.width/2,r.yMin+r.height/2));

        public bool IsWalkable(Vector3 p,float radius=.65f)
        {
            var lo=ToCell(p-new Vector3(radius,0,radius)); var hi=ToCell(p+new Vector3(radius,0,radius));
            for(int x=lo.x;x<=hi.x;x++) for(int z=lo.y;z<=hi.y;z++)
            {
                if(IsFloor(x,z)) continue;
                var c=ToWorld(new Vector2Int(x,z));
                float dx=Mathf.Max(0,Mathf.Abs(p.x-c.x)-CellSize*.5f), dz=Mathf.Max(0,Mathf.Abs(p.z-c.z)-CellSize*.5f);
                if(dx*dx+dz*dz<=radius*radius+.0001f) return false;
            }
            return IsFloor(ToCell(p).x,ToCell(p).y);
        }

        public Vector3 Resolve(Vector3 p,float radius=.65f)
        {
            if(IsWalkable(p,radius)) return p;
            var best=Spawn; float sq=float.MaxValue;
            foreach(var cell in FloorCells)
            {
                var candidate=ToWorld(cell); candidate.y=p.y;
                float d=(candidate-p).sqrMagnitude;
                if(d<sq && IsWalkable(candidate,radius)) { sq=d; best=candidate; }
            }
            best.y=p.y; return best;
        }

        public Vector3 Move(Vector3 p,Vector3 motion,float radius=.65f)
        {
            if(!IsWalkable(p,radius)) p=Resolve(p,radius);
            motion.y=0;
            int steps=Mathf.Max(1,Mathf.CeilToInt(motion.magnitude/.4f)); var step=motion/steps;
            for(int i=0;i<steps;i++)
            {
                if(IsWalkable(p+step,radius)) p+=step;
                else
                {
                    var x=new Vector3(step.x,0,0); var z=new Vector3(0,0,step.z);
                    if(IsWalkable(p+x,radius)) p+=x;
                    if(IsWalkable(p+z,radius)) p+=z;
                }
            }
            return p;
        }

        public void UpdateNavigation(Vector3 target)
        {
            var cell=ToCell(Resolve(target)); if(cell==targetCell) return;
            targetCell=cell; queue.Clear();
            for(int x=0;x<GridSize;x++) for(int z=0;z<GridSize;z++) distance[x,z]=-1;
            distance[cell.x,cell.y]=0; queue.Enqueue(cell);
            while(queue.Count>0)
            {
                var c=queue.Dequeue();
                foreach(var d in Directions)
                {
                    var n=c+d; if(!IsFloor(n.x,n.y) || distance[n.x,n.y]>=0) continue;
                    distance[n.x,n.y]=distance[c.x,c.y]+1; queue.Enqueue(n);
                }
            }
        }

        public bool HasClearPath(Vector3 from,Vector3 to,float radius=.7f)
        {
            int steps=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(from,to)/1.5f));
            for(int i=0;i<=steps;i++) if(!IsWalkable(Vector3.Lerp(from,to,i/(float)steps),radius)) return false;
            return true;
        }

        public Vector3 PursuitDirection(Vector3 from,Vector3 target,float radius=.7f)
        {
            var delta=target-from; delta.y=0;
            if(HasClearPath(from,target,radius)) return delta.normalized;
            var cell=ToCell(from); if(!IsFloor(cell.x,cell.y)) return (Resolve(from,radius)-from).normalized;
            var best=cell; int score=distance[cell.x,cell.y];
            foreach(var d in Directions)
            {
                var n=cell+d; if(!IsFloor(n.x,n.y) || distance[n.x,n.y]<0) continue;
                if(distance[n.x,n.y]<score) { score=distance[n.x,n.y]; best=n; }
            }
            var waypoint=ToWorld(best); waypoint.y=from.y;
            // Center first if a diagonal shortcut would cut the inside of a wall corner.
            if(!HasClearPath(from,waypoint,radius)) { waypoint=ToWorld(cell); waypoint.y=from.y; }
            return (waypoint-from).normalized;
        }

        public int ReachableCellCount { get { int count=0; foreach(var c in FloorCells) if(distance[c.x,c.y]>=0) count++; return count; } }
    }
}
