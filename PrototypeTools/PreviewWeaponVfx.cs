UnityEngine.Object.FindAnyObjectByType<BonkSurvivor.SurvivorGame>().enabled=false;
var catalog=Resources.Load<BonkSurvivor.WeaponVfxCatalog>("WeaponVfxCatalog");
var root=new GameObject("Weapon VFX gallery"); root.transform.position=new Vector3(1000,0,1000);
for(int i=0;i<30;i++) {
 var e=catalog.entries[i]; var pos=root.transform.position+new Vector3((i%6-2.5f)*4,0,(2-i/6)*4);
 var fx=UnityEngine.Object.Instantiate(e.impact,pos,Quaternion.identity,root.transform);
 foreach(var ps in fx.GetComponentsInChildren<ParticleSystem>()) {ps.Simulate(.22f,false,true,true);ps.Pause(false);}
 var label=new GameObject(e.weapon.ToString(),typeof(TextMesh));label.transform.SetParent(root.transform);label.transform.position=pos+new Vector3(-1.4f,0,-1.4f);label.transform.rotation=Quaternion.Euler(70,0,0);
 var text=label.GetComponent<TextMesh>();text.text=i+" "+e.weapon;text.characterSize=.105f;text.fontSize=40;text.color=Color.white;
}
var cam=Camera.main;cam.transform.position=root.transform.position+new Vector3(0,26,-10);cam.transform.LookAt(root.transform.position);cam.orthographic=true;cam.orthographicSize=11;cam.backgroundColor=new Color(.025f,.03f,.04f);
return "Gallery ready";
