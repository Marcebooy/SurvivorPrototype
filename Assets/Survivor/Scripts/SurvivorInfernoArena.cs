using UnityEngine;
namespace BonkSurvivor
{
 public sealed partial class SurvivorGame
 {
  Enemy CreateNecromancerBoss(Vector3 position)
  {
   var assets = Resources.Load<InfernoBossAssets>("InfernoBossAssets");
   if (!assets || !assets.boss) throw new System.InvalidOperationException("Inferno boss assets missing");
   var body = Instantiate(assets.boss, position, Quaternion.identity, world).transform;
   body.name = "Skeleton Necromancer - area " + Area;
   return new Enemy { body=body, animator=body.GetComponentInChildren<Animator>(), health=900*Area*curse, speed=3.2f*curse, elite=true };
  }
  void BuildInfernoBossArena()
  {
   var a = Resources.Load<InfernoBossAssets>("InfernoBossAssets");
   if (!a) throw new System.InvalidOperationException("Inferno arena assets missing");
   var root = new GameObject("Inferno - Necromancer arena").transform;
   root.SetParent(proceduralRoot,false); root.position=MapLayout.BossPosition;
   // Match the navigation plane; retain wide entrances between decorative groups.
   Shape("Basalt arena floor",PrimitiveType.Cylinder,root.position+Vector3.down*.1f,new Vector3(36,.15f,36),MakeMaterial(new Color(.12f,.105f,.10f)),root);
   var platform=PlaceInferno(a.platform,root,Vector3.zero,1.8f,0);
   platform.transform.localScale=new Vector3(1.8f,.08f,1.8f);
   platform.transform.position += Vector3.up*(.22f-InfernoBounds(platform).max.y);
   // Decorative lava inlay, level with the floor and crossed by four stone entrances.
   var vertices=new System.Collections.Generic.List<Vector3>();
   var triangles=new System.Collections.Generic.List<int>();
   for(int segment=0;segment<64;segment++) {
    if(segment%16<2 || segment%16>13) continue;
    float angle=segment*Mathf.PI*2/64, next=(segment+1)*Mathf.PI*2/64;
    int n=vertices.Count;
    vertices.Add(new Vector3(Mathf.Sin(angle)*17.2f,.07f,Mathf.Cos(angle)*17.2f));
    vertices.Add(new Vector3(Mathf.Sin(angle)*17.8f,.07f,Mathf.Cos(angle)*17.8f));
    vertices.Add(new Vector3(Mathf.Sin(next)*17.8f,.07f,Mathf.Cos(next)*17.8f));
    vertices.Add(new Vector3(Mathf.Sin(next)*17.2f,.07f,Mathf.Cos(next)*17.2f));
    triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
   }
   MapMesh("Lava inlay",FacetedMesh("Lava ring",vertices.ToArray(),triangles.ToArray()),Vector3.zero,Vector3.one,a.lava,root);
   for(int i=0;i<8;i++) {
    float angle=(22.5f+i*45)*Mathf.Deg2Rad;
    var radial=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));
    PlaceInferno(i%2==0?a.column:a.brokenColumn,root,radial*19,.65f,-i*45);
    PlaceInferno(a.brazier,root,radial*16.8f,.8f,0);
    PlaceInferno(a.rock,root,radial*22,.65f,i*71);
   }
   PlaceInferno(a.statue,root,new Vector3(-10,0,21),.18f,180);
   PlaceInferno(a.statue,root,new Vector3(10,0,21),.18f,180);
  }
  static Bounds InfernoBounds(GameObject go) {
   var rs=go.GetComponentsInChildren<Renderer>(); var b=rs[0].bounds;
   foreach(var r in rs) b.Encapsulate(r.bounds); return b;
  }
  GameObject PlaceInferno(GameObject prefab,Transform parent,Vector3 position,float scale,float yaw) {
   var go=Instantiate(prefab,parent,false);
   go.transform.localScale=Vector3.one*scale; go.transform.localRotation=Quaternion.Euler(0,yaw,0); go.transform.localPosition=position;
   var b=InfernoBounds(go);
   go.transform.position+=new Vector3(parent.position.x+position.x-b.center.x,-b.min.y,parent.position.z+position.z-b.center.z);
   foreach(var c in go.GetComponentsInChildren<Collider>()) c.enabled=false;
   return go;
  }
 }
}
