var g=UnityEngine.Object.FindAnyObjectByType<BonkSurvivor.SurvivorGame>();
var gallery=GameObject.Find("Weapon VFX gallery"); if(gallery)UnityEngine.Object.Destroy(gallery);
g.StartSeededRun(58214);g.SelectCharacter(BonkSurvivor.SurvivorGame.PlayerCharacter.Pottu);g.SelectStarter(0);
var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var t=g.GetType();
var levels=(System.Collections.IDictionary)t.GetProperty("weaponLevels",f).GetValue(g); levels.Clear();
foreach(var w in new[]{BonkSurvivor.SurvivorGame.Weapon.Firestaff,BonkSurvivor.SurvivorGame.Weapon.PoisonFlask,BonkSurvivor.SurvivorGame.Weapon.Frostwalker,BonkSurvivor.SurvivorGame.Weapon.Lightning,BonkSurvivor.SurvivorGame.Weapon.Bone}) levels[w]=1;
var list=(System.Collections.IList)t.GetField("enemies",f).GetValue(g);var player=(Transform)t.GetProperty("player",f).GetValue(g);
for(int i=0;i<12;i++){t.GetMethod("SpawnEnemy",f).Invoke(g,null);var e=list[list.Count-1];e.GetType().GetField("health").SetValue(e,5000f);((Transform)e.GetType().GetField("body").GetValue(e)).position=player.position+Quaternion.Euler(0,i*30,0)*Vector3.forward*7;}
Camera.main.orthographic=false;t.GetMethod("UpdateCamera",f).Invoke(g,new object[]{true});g.enabled=true;
return "Five-weapon gameplay running";
