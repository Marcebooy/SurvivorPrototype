var g=UnityEngine.Object.FindFirstObjectByType<BonkSurvivor.SurvivorGame>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
object Call(string name,params object[] args)=>g.GetType().GetMethod(name,flags).Invoke(g,args);
object Field(string name)=>g.GetType().GetField(name,flags).GetValue(g);
void Set(string name,object value)=>g.GetType().GetProperty(name).GetSetMethod(true).Invoke(g,new object[]{value});
var checks=new System.Collections.Generic.List<string>();
void Check(bool ok,string name){if(!ok)throw new System.Exception(name);checks.Add(name);}
try
{
 g.ResetRun();
 Check(!g.TryDodge(UnityEngine.Vector3.forward),"Cannot dodge in selection");
 g.SelectStarter(0);
 var p=(UnityEngine.Transform)Field("player"); var visual=p.GetComponent<BonkSurvivor.PottuVisual>();
 Check(visual!=null && p.Find("Pottu roll pivot/Wobbly potato/Oversized helmet")!=null,"Potato model and helmet created");
 Check(g.TryDodge(UnityEngine.Vector3.right) && g.IsDodging && !g.TryDodge(UnityEngine.Vector3.right),"Dodge starts and prevents spam");
 float hp=g.Health; Check(!(bool)Call("ReceiveDamage",22f) && g.Health==hp,"Dodge blocks boss-size damage");
 var before=p.position; Call("MovePottu",UnityEngine.Vector3.zero,.2f,false);
 Check(p.position.x>before.x+3 && g.IsDodging,"Roll moves in locked direction");
 Call("MovePottu",UnityEngine.Vector3.zero,.2f,false);
 Check(!g.IsDodging && g.DodgeCooldown>0 && !g.TryDodge(UnityEngine.Vector3.forward),"Dodge expires with cooldown remaining");
 Check((bool)Call("ReceiveDamage",9f) && g.Health<hp,"Damage resumes after roll");
 Call("MovePottu",UnityEngine.Vector3.zero,2f,false);
 Check(g.TryDodge(UnityEngine.Vector3.zero),"Dodge recharges and idle dodge uses facing");
 p.position=new UnityEngine.Vector3(38,1,0); Call("MovePottu",UnityEngine.Vector3.right,.4f,false);
 Check(new UnityEngine.Vector2(p.position.x,p.position.z).magnitude<=g.arenaRadius-1+.001f,"Roll respects arena boundary");
 g.ResetRun(); g.SelectStarter(0); Set("ShopOpen",true);
 Check(!g.TryDodge(UnityEngine.Vector3.forward),"Shop blocks dodge"); Set("ShopOpen",false);
 g.GrantExperience(8); Check(!g.TryDodge(UnityEngine.Vector3.forward),"Level choices block dodge");
 g.ChooseUpgrade(0); Call("ApplyUpgrade",6); Call("ApplyUpgrade",0);
 Call("MovePottu",UnityEngine.Vector3.forward,.1f,false);
 Check(visual.PanScale>1.2f,"Size and weapon levels grow pan");
 var source=p.GetComponent<UnityEngine.AudioSource>();
 var clip=(UnityEngine.AudioClip)typeof(BonkSurvivor.PottuVisual).GetField("bonk",flags).GetValue(visual);
 var data=new float[clip.samples]; clip.GetData(data,0); float peak=0; foreach(float f in data)peak=UnityEngine.Mathf.Max(peak,UnityEngine.Mathf.Abs(f));
 Check(source!=null && peak>.1f && peak<=1,"Synthesized BONK audio is non-silent and unclipped");
 g.ResetRun(); Check(!g.IsDodging && g.DodgeCooldown==0,"Restart clears dodge state");
}
finally {g.ResetRun();}
return checks;
