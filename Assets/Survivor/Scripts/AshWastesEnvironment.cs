using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    // Authoring source. The editor baker stores generated meshes and materials in a reusable prefab.
    // No automatic regeneration: hand-edited prefab children remain editable.
    public sealed class AshWastesEnvironment : MonoBehaviour
    {
        public Material ash, sand, basalt, rust, charcoal, ember, hotCore;
        public List<Vector3> obstacles = new List<Vector3>(); // x/z position, y footprint radius
        public readonly List<Mesh> generatedMeshes = new List<Mesh>();
        static readonly Vector2[] Outline = {
            new Vector2(1.65f,-.35f),new Vector2(1.4f,.65f),new Vector2(.65f,1.25f),new Vector2(-.3f,1.4f),
            new Vector2(-1.25f,.9f),new Vector2(-1.6f,.15f),new Vector2(-1.35f,-.8f),new Vector2(-.55f,-1.15f),new Vector2(.55f,-1.25f),new Vector2(1.35f,-.95f)
        };
        Mesh rock, branch;
        Transform Group(string label, Vector3 p, Transform parent)
        { var g=new GameObject(label).transform;g.SetParent(parent,false);g.localPosition=p;return g; }
        Mesh Mesh(string label,List<Vector3> v,List<int> indices)
        {
            var flat=new Vector3[indices.Count];var t=new int[indices.Count];
            for(int i=0;i<t.Length;i++){flat[i]=v[indices[i]];t[i]=i;}
            var m=new Mesh{name=label,vertices=flat,triangles=t};m.RecalculateNormals();m.RecalculateBounds();generatedMeshes.Add(m);return m;
        }
        Transform Part(string label,Mesh mesh,Vector3 p,Vector3 size,Material mat,Transform parent)
        {
            var t=Group(label,p,parent);t.localScale=size;
            t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;return t;
        }
        void Boulder(Vector3 p,Vector3 scale,Transform parent,bool block=true)
        {
            Part("Wind-carved basalt",rock,p,scale,basalt,parent).localRotation=Quaternion.Euler(0,p.x*17+p.z*11,0);
            if(block){var w=parent.TransformPoint(p);obstacles.Add(new Vector3(w.x,Mathf.Max(scale.x,scale.z),w.z));}
        }
        void Limb(Vector3 a,Vector3 b,float width,Transform parent)
        {
            var t=Part("Charred branch",branch,a,new Vector3(width,Vector3.Distance(a,b),width),charcoal,parent);
            t.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
        }
        void Tree(Vector3 p,float s,Transform parent)
        {
            var t=Group("Burnt sentinel",p,parent);t.localScale=Vector3.one*s;
            Limb(Vector3.zero,new Vector3(.25f,4.6f,0),.6f,t);
            Limb(new Vector3(.15f,2.3f,0),new Vector3(-1.7f,3.8f,.15f),.35f,t);
            Limb(new Vector3(-1.7f,3.8f,.15f),new Vector3(-1.9f,4.8f,.25f),.18f,t);
            Limb(new Vector3(.2f,3.2f,0),new Vector3(1.6f,4.4f,-.3f),.3f,t);
            Limb(new Vector3(1.6f,4.4f,-.3f),new Vector3(2.2f,4.5f,.4f),.15f,t);
            var w=parent.TransformPoint(p);obstacles.Add(new Vector3(w.x,.75f*s,w.z));
        }
        void Crack(string name,Vector3[] points,float width,Transform parent)
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<points.Length;i++)
            {
                var d=points[Mathf.Min(i+1,points.Length-1)]-points[Mathf.Max(0,i-1)];
                var side=new Vector3(-d.z,0,d.x).normalized*width;
                v.Add(points[i]-side);v.Add(points[i]+side);
                if(i==points.Length-1)continue;
                int n=i*2;t.AddRange(new[]{n,n+1,n+2,n+1,n+3,n+2});
            }
            var m=Mesh(name,v,t);
            Part(name,m,Vector3.up*.025f,Vector3.one,ember,parent);
            for(int i=0;i<v.Count;i+=2){var center=(v[i]+v[i+1])*.5f;v[i]=Vector3.Lerp(center,v[i],.3f);v[i+1]=Vector3.Lerp(center,v[i+1],.3f);}
            Part(name+" hot seam",Mesh(name+" Core",v,t),Vector3.up*.035f,Vector3.one,hotCore,parent);
        }
        void Anchor(string name,Vector3 p,Transform parent) => Group(name,p,parent);

        public void Build()
        {
            var rv=new List<Vector3>{new Vector3(0,1,0),new Vector3(-1,.2f,-.6f),new Vector3(.6f,.12f,-.9f),new Vector3(1,.2f,.5f),new Vector3(-.7f,.1f,.9f),new Vector3(0,-.15f,0)};
            rock=Mesh("Ash_Basalt",rv,new List<int>{0,2,1,0,3,2,0,4,3,0,1,4,5,1,2,5,2,3,5,3,4,5,4,1});
            var bv=new List<Vector3>();var bt=new List<int>();
            for(int i=0;i<6;i++){float a=i*Mathf.PI/3;bv.Add(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));bv.Add(new Vector3(Mathf.Cos(a)*.7f,1,Mathf.Sin(a)*.7f));}
            for(int i=0;i<6;i++){int n=i*2,j=((i+1)%6)*2;bt.AddRange(new[]{n,n+1,j,j,n+1,j+1});}
            branch=Mesh("Ash_HexagonalBranch",bv,bt);
            var floorV=new List<Vector3>{Vector3.zero};var floorT=new List<int>();
            foreach(var p in Outline)floorV.Add(new Vector3(p.x*60,0,p.y*60));
            for(int i=0;i<Outline.Length;i++)floorT.AddRange(new[]{0,(i+1)%Outline.Length+1,i+1});
            Part("Tuhkaeramaa terrain",Mesh("Ash_Floor",floorV,floorT),Vector3.zero,Vector3.one,ash,transform);
            var landmarks=Group("LANDMARKS - editable",Vector3.zero,transform);
            float k=60f/38;
            var split=Group("01 Split monolith - chest",new Vector3(10*k,0,9*k),landmarks);
            Boulder(new Vector3(-5,0,1),new Vector3(2.4f,7,2.4f),split);
            Boulder(new Vector3(5,0,2),new Vector3(2.2f,5.6f,2.2f),split);
            Crack("Monolith fault",new[]{new Vector3(-7,0,5),new Vector3(-2,0,4),new Vector3(1,0,6),new Vector3(7,0,4)},.22f,split);
            Anchor("Chest_01",Vector3.up*.8f,split);
            var grove=Group("02 Cinder grove - chest",new Vector3(-18*k,0,12*k),landmarks);
            Tree(new Vector3(-5,0,2),1.5f,grove);Tree(new Vector3(5,0,4),.95f,grove);Anchor("Chest_02",Vector3.up*.8f,grove);
            var basin=Group("03 Ember basin - chest",new Vector3(20*k,0,-15*k),landmarks);
            for(int i=0;i<5;i++){float a=i*Mathf.PI/4;Boulder(new Vector3(Mathf.Cos(a)*7,0,Mathf.Sin(a)*7+2),new Vector3(1.2f,1.7f,1.2f),basin);}
            Crack("Basin seam",new[]{new Vector3(-5,0,3),new Vector3(-2,0,5),new Vector3(2,0,4),new Vector3(5,0,6)},.3f,basin);Anchor("Chest_03",Vector3.up*.8f,basin);
            var shrine=Group("04 Burnt sentinel - shrine",new Vector3(-12*k,0,-12*k),landmarks);
            Tree(new Vector3(0,0,7),2f,shrine);Boulder(new Vector3(-6,0,4),new Vector3(1.5f,2,1.5f),shrine);Anchor("Shrine",Vector3.up,shrine);
            var crown=Group("05 Ember crown - portal",new Vector3(0,0,25*k),landmarks);
            for(int i=0;i<5;i++){float a=i*Mathf.PI/4;Boulder(new Vector3(Mathf.Cos(a)*9,0,Mathf.Sin(a)*9+2),new Vector3(1.6f,3+i%2*2,1.6f),crown);}
            Crack("Crown fault",new[]{new Vector3(-9,0,5),new Vector3(-5,0,7),new Vector3(0,0,6),new Vector3(4,0,8),new Vector3(9,0,5)},.32f,crown);Anchor("Portal",Vector3.up*1.5f,crown);
            Anchor("PlayerSpawn",Vector3.up,transform);
            var border=Group("OUTER RIM",Vector3.zero,transform);
            for(int i=0;i<Outline.Length;i++)
            {
                var a=floorV[i+1];var b=floorV[(i+1)%Outline.Length+1];int count=Mathf.CeilToInt(Vector3.Distance(a,b)/3.5f);
                for(int j=0;j<count;j++){var p=Vector3.Lerp(a,b,j/(float)count);Boulder(p,new Vector3(2.2f,1.6f+(j%3)*.45f,2.2f),border,false);}
            }
            // Warm windblown patches lie flat and never create collision or choke points.
            var dv=new List<Vector3>{Vector3.zero};var dt=new List<int>();
            for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;dv.Add(new Vector3(Mathf.Cos(a)*(1f+i%2*.2f),0,Mathf.Sin(a)));dt.AddRange(new[]{0,(i+1)%7+1,i+1});}
            var drift=Mesh("Ash_WindblownPatch",dv,dt);
            var rng=new System.Random(90521);
            for(int i=0;i<42;i++)
            {
                var p=new Vector3((float)rng.NextDouble()*120-60,0,(float)rng.NextDouble()*94-47);
                bool clear=true;foreach(Transform landmark in landmarks)if(Vector3.Distance(p,landmark.localPosition)<13){clear=false;break;}
                if(!clear)continue;
                Part("Rust sand drift",drift,p+Vector3.up*.012f,new Vector3(4+i%4,1,2+i%3),i%3==0 ? rust : sand,transform);
            }
        }
    }
}
