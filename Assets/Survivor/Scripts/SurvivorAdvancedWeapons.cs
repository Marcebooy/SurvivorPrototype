using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    public sealed partial class SurvivorGame
    {
        sealed class WeaponEntry
        {
            public string name, description; public float interval;
            public WeaponEntry(string n,string d,float t) {name=n;description=d;interval=t;}
        }
        // Existing enum values and upgrade IDs remain stable. New IDs begin at 18.
        static class AdvancedCatalog
        {
            public static readonly WeaponEntry[] Entries = {
                new WeaponEntry("Revolver", "Kolmen luodin sarja. Luodit kimpoavat; asetaso lisää kimpoiluja. Quantity lisää luoteja.",1.25f),
                new WeaponEntry("Aegis / Kilpi", "Latautuva kilpi torjuu yhden osuman ja tekee aluevastaiskun. Cooldown nopeuttaa latausta.",4.5f),
                new WeaponEntry("Bananarang", "Banaani lentää ulos ja palaa Pottuun. Voi osua samaan kohteeseen kerran kumpaankin suuntaan.",1.4f),
                new WeaponEntry("Axe / Kirves", "Pyörivä kirves läpäisee viholliset leveältä alueelta. Quantity lisää kirveitä.",1.6f),
                new WeaponEntry("Space Noodle", "Linkki lähimpään viholliseen. Vahingoittaa myös linkin välissä olevia; Duration pidentää linkkiä.",2.4f),
                new WeaponEntry("Sniper Rifle", "Hidas, erittäin vahva läpäisevä laukaus. Size leventää osumalinjaa.",2.8f),
                new WeaponEntry("Slutty Rocket / Raketti", "Hakeutuva raketti räjähtää alueelle. Quantity lisää raketteja, Size räjähdystä.",1.9f),
                new WeaponEntry("Mines / Miinat", "Jättää viiveellä virittyvän miinan. Kosketus räjäyttää; Duration lisää odotusaikaa.",1.5f),
                new WeaponEntry("Wireless Dagger", "Hakeutuva tikari kimpoaa seuraavaan kohteeseen. Asetaso lisää kimpoiluja.",1.1f),
                new WeaponEntry("Frostwalker", "Jättää jäätä: hidastaa, jäädyttää lyhyesti ja tekee vahinkoa. Bossi vain hidastuu.",.9f),
                new WeaponEntry("Tornado", "Liikkuva pyörre työntää ja vahingoittaa. Duration pidentää, Size leventää pyörrettä.",2.2f),
                new WeaponEntry("Dexecutioner", "Lähiterä voi teloittaa tavallisen vihollisen. Elitejä ja bosseja ei voi teloittaa.",1.25f),
                new WeaponEntry("Blood Magic", "Verenimuisku. Joka 10. tappo lisää kierroksen Max HP:tä yhdellä (max +100). Ei tallennu.",1.35f),
                new WeaponEntry("Black Hole", "Vetää vihollisia kasaan ja vahingoittaa. Bossiin heikompi veto. Duration pidentää kestoa.",3.2f),
                new WeaponEntry("Poison Flask", "Heittää lähimmän vihollisen kohdalle myrkkylammikon. Size ja Duration kasvattavat aluetta ja kestoa.",2.1f),
                new WeaponEntry("Katana", "Nopea automaattinen isku lähimpään viholliseen. Quantity lisää erillisiä kohteita.",.38f),
                new WeaponEntry("Dragon's Breath", "Lyhyt automaattisesti suunnattu tulikartio. Size kasvattaa kantamaa.",.55f),
                new WeaponEntry("Dice / Noppa", "Heittää 1–6: vahinko riippuu luvusta. Kuutonen lisää crit-mahdollisuutta 1 prosenttiyksikön.",1.2f),
                new WeaponEntry("Hero Sword", "Lähisivallus ja läpäisevä etäviilto. Quantity lisää viiltoja.",1.4f),
                new WeaponEntry("Corrupted Sword", "Lähisivallus vahvistuu puuttuvan HP:n mukaan, enintään kolminkertaiseksi.",1.05f),
                new WeaponEntry("Scythe / Viikate", "360 asteen isku. Kolme elite-tappoa lataa seuraavasta iskusta kolminkertaisen.",1.8f)
            };
        }
        static string[] WithAdvancedNames(string[] original)
        {var all=new List<string>(original);foreach(var e in AdvancedCatalog.Entries)all.Add(e.name);return all.ToArray();}
        static string[] WithAdvancedDetails(string[] original)
        {var all=new List<string>(original);foreach(var e in AdvancedCatalog.Entries)all.Add(e.description);return all.ToArray();}
        public const int TotalWeaponCount=30;
        public int ShieldCharges => shieldCharges;
        public int ScytheCharge => scytheCharge;
        public int BloodHealthGained => bloodHealthGained;
        public int AdvancedProjectileCount => advancedShots.Count;
        public int AdvancedZoneCount => advancedZones.Count;
        public int LastDiceRoll { get => current.LastDiceRoll; private set => current.LastDiceRoll = value; }
        List<AdvancedShot> advancedShots => current.AdvancedShots;
        List<AdvancedZone> advancedZones => current.AdvancedZones;
        Dictionary<Enemy,Chill> chilled => current.Chilled;
        readonly List<Enemy> chillKeys=new List<Enemy>();
        int shieldCharges { get => current.ShieldCharges; set => current.ShieldCharges = value; }
        int scytheCharge { get => current.ScytheCharge; set => current.ScytheCharge = value; }
        int bloodKills { get => current.BloodKills; set => current.BloodKills = value; }
        int bloodHealthGained { get => current.BloodHealthGained; set => current.BloodHealthGained = value; }
        Material advancedGold,advancedIce,advancedPoison,advancedVoid,advancedSteel;
        internal sealed class Chill {public float slow,freeze;}
        internal sealed class AdvancedShot
        {
            public Weapon kind;public Transform body;public Vector3 direction;public Enemy target;
            public float power,speed,radius,age,life;public int bounces;public bool returning;
            public HashSet<Enemy> hit=new HashSet<Enemy>();
        }
        internal sealed class AdvancedZone
        {
            public Weapon kind;public Transform body;public Enemy target;public Vector3 direction;
            public float power,radius,life,age,tick;
        }
        void EnsureAdvancedMaterials()
        {
            if(advancedGold)return;
            advancedGold=MakeMaterial(new Color(1,.74f,.12f));advancedIce=MakeMaterial(new Color(.3f,.85f,1));
            advancedPoison=MakeMaterial(new Color(.5f,.85f,.08f));advancedVoid=MakeMaterial(new Color(.35f,.09f,.55f));
            advancedSteel=MakeMaterial(new Color(.65f,.8f,.85f));
        }
        Material AdvancedMaterial(Weapon w)
        {
            if(w==Weapon.Frostwalker)return advancedIce;
            if(w==Weapon.PoisonFlask)return advancedPoison;
            if(w==Weapon.BlackHole || w==Weapon.CorruptedSword || w==Weapon.SpaceNoodle)return advancedVoid;
            if(w==Weapon.Rocket || w==Weapon.DragonBreath || w==Weapon.BloodMagic)return fireMat;
            return w==Weapon.Bananarang || w==Weapon.Dice || w==Weapon.Aegis ? advancedGold : advancedSteel;
        }
        void ClearAdvanced(bool resetRun)
        {
            foreach(var s in advancedShots)if(s.body)Destroy(s.body.gameObject);advancedShots.Clear();
            foreach(var z in advancedZones)if(z.body)Destroy(z.body.gameObject);advancedZones.Clear();chilled.Clear();
            // Yhtenäinen ajastintaulukko (weaponSlotTimers) tyhjennetään yhdessä ClearArsenal:issa,
            // joka aina kutsuu tätä metodia - kattaa siis myös nämä 21 asetta.
            shieldCharges=0;
            if(resetRun){scytheCharge=bloodKills=bloodHealthGained=0;LastDiceRoll=0;}
        }
        void TickAdvanced(float dt)
        {
            EnsureAdvancedMaterials();
            chillKeys.Clear();chillKeys.AddRange(chilled.Keys);
            foreach(var e in chillKeys)
            {var c=chilled[e];c.freeze-=dt;c.slow-=dt;if(!e.body || c.slow<=0)chilled.Remove(e);}
            for(int i=0;i<AdvancedCatalog.Entries.Length;i++)
            {
                var w=(Weapon)(9+i);int level=WeaponLevel(w);if(level==0)continue;
                weaponSlotTimers[(int)w]-=dt;if(weaponSlotTimers[(int)w]>0)continue;
                CastAdvanced(w,level);
                // Näillä aseilla on oma slotti yhtenäisessä ajastintaulussa (kuten Flamewalkerilla),
                // joten Cooldown-affiksi/-stat voi nopeuttaa juuri tätä asetta erikseen muihin vaikuttamatta.
                weaponSlotTimers[(int)w]=WeaponInterval(w)/(Mathf.Max(.1f,attackRate)*GetWeaponStats(w).Cooldown);
            }
            TickAdvancedShots(dt);TickAdvancedZones(dt);
        }
        float EnemyMoveMultiplier(Enemy e)
        {if(!chilled.TryGetValue(e,out var c))return 1;return c.freeze>0 && e!=boss ? 0 : c.slow>0 ? .45f : 1;}
        void ChillEnemy(Enemy e)
        {if(!chilled.TryGetValue(e,out var c)){c=new Chill();chilled[e]=c;}c.slow=1.2f*EffectDuration;c.freeze=e==boss ? 0 : .32f*EffectDuration;}
        bool BlockWithAegis()
        {
            if(shieldCharges<=0)return false;shieldCharges--;invulnerability=.45f;
            AreaHit(player.position,4*Size,damage*(2+.3f*WeaponLevel(Weapon.Aegis)));
            AdvancedPulse(player.position,4*Size,Weapon.Aegis);characterVisual.Block();return true;
        }
        void AdvancedKill(Enemy e)
        {
            chilled.Remove(e);
            if(e.elite && WeaponLevel(Weapon.Scythe)>0)scytheCharge=Mathf.Min(3,scytheCharge+1);
            if(WeaponLevel(Weapon.BloodMagic)>0 && bloodHealthGained<100)
            {bloodKills++;if(bloodKills%10==0){bloodHealthGained++;maxHealth++;Health=Mathf.Min(maxHealth,Health+1);}}
        }
        void AdvancedPulse(Vector3 center,float radius,Weapon w)
        {
            if(flashes.Count>=160)return;

            SpawnImpact(new Vector3(center.x,.6f,center.z),w,Mathf.Max(1f,radius*.45f));
        }
        void AdvancedBeam(Vector3 start,Vector3 end,float width,Weapon w)
        {
            if(flashes.Count>=160 || (end-start).sqrMagnitude<.001f)return;
            var t=Shape(WeaponLabel(w)+" strike",PrimitiveType.Cube,(start+end)/2,new Vector3(width,width,Vector3.Distance(start,end)),AdvancedMaterial(w),world);
            t.rotation=Quaternion.LookRotation(end-start);flashes.Add(new Flash{body=t,life=.12f});
            SpawnImpact(end,w,Size);
        }
        void LineHit(Vector3 a,Vector3 b,float radius,float power,Weapon kind=Weapon.Sniper)
        {
            var delta=b-a;
            for(int i=enemies.Count-1;i>=0;i--)
            {var e=enemies[i];float t=Mathf.Clamp01(Vector3.Dot(e.body.position-a,delta)/Mathf.Max(.001f,delta.sqrMagnitude));if((e.body.position-a-delta*t).sqrMagnitude<radius*radius) { SpawnImpact(e.body.position,kind,Size); Hit(e,power); }}
        }
        bool CanExecute(Enemy e) => e!=boss && !e.elite;
        void CastAdvanced(Weapon w,int level)
        {
            EnsureArsenalMaterials();EnsureAdvancedMaterials();
            var stats=GetWeaponStats(w);
            float power=damage*(1+.25f*(level-1))*stats.Damage;
            var nearest=Nearest(player.position,40);var aim=nearest!=null ? (nearest.body.position-player.position).normalized : player.forward;
            if(w==Weapon.Aegis){shieldCharges=1;return;}
            if(w==Weapon.Mines || w==Weapon.Frostwalker)
            {AddAdvancedZone(w,player.position,aim,null,power,(w==Weapon.Mines ? 2.8f*Size : 2*Size)*stats.Size,(w==Weapon.Mines ? 10 : 3)*EffectDuration*stats.Duration);return;}
            if(nearest==null)return;
            if(w==Weapon.SpaceNoodle || w==Weapon.BlackHole || w==Weapon.PoisonFlask || w==Weapon.Tornado)
            {
                Vector3 pos=w==Weapon.Tornado || w==Weapon.SpaceNoodle ? player.position : nearest.body.position;
                AddAdvancedZone(w,pos,aim,nearest,power,(w==Weapon.SpaceNoodle ? .65f*Size : 3*Size)*stats.Size,(w==Weapon.SpaceNoodle ? 1.8f : 3)*EffectDuration*stats.Duration);return;
            }
            if(w==Weapon.Sniper)
            {var end=player.position+aim*45;LineHit(player.position,end,.8f*Size,power*5);AdvancedBeam(player.position,end,.08f*Size,w);return;}
            if(w==Weapon.Katana || w==Weapon.Dexecutioner || w==Weapon.BloodMagic || w==Weapon.Dice)
            {
                characterVisual.Swing(aim);
                float range=w==Weapon.Dice ? 20 : w==Weapon.BloodMagic ? 10 : 6*Size;
                var used=new HashSet<Enemy>();
                int baseCount=w==Weapon.Katana ? Quantity : 1;
                int count=baseCount+Mathf.RoundToInt(baseCount*(stats.Quantity-1));
                for(int i=0;i<count;i++)
                {
                    var e=Nearest(player.position,range,used);if(e==null)break;used.Add(e);
                    float hit=power*(w==Weapon.Katana ? .8f : 1.25f)*stats.Crit;
                    if(w==Weapon.Dexecutioner && CanExecute(e) && Random.value<Mathf.Min(.5f,.15f+.02f*level))hit=e.health+1;
                    if(w==Weapon.Dice){LastDiceRoll=Random.Range(1,7);hit=power*LastDiceRoll*.4f;if(LastDiceRoll==6)CritChance=Mathf.Min(.7f,CritChance+.01f);}
                    AdvancedBeam(player.position,e.body.position,.18f*Size,w);Hit(e,hit);
                    if(w==Weapon.BloodMagic)Health=Mathf.Min(maxHealth,Health+1);
                }
                return;
            }
            if(w==Weapon.DragonBreath || w==Weapon.HeroSword || w==Weapon.CorruptedSword || w==Weapon.Scythe)
            {
                float range=(w==Weapon.DragonBreath ? 7 : 4.5f)*Size;
                float multiplier=w==Weapon.CorruptedSword ? 1+2*(1-Health/Mathf.Max(1,maxHealth)) : 1;
                if(w==Weapon.Scythe && scytheCharge>=3){multiplier=3;scytheCharge=0;}
                for(int i=enemies.Count-1;i>=0;i--)
                {
                    var e=enemies[i];var delta=e.body.position-player.position;
                    if(delta.sqrMagnitude<=range*range && (w==Weapon.Scythe || Vector3.Dot(aim,delta.normalized)>(w==Weapon.DragonBreath ? .72f : -.2f)))Hit(e,power*multiplier);
                }
                if(w==Weapon.DragonBreath){AdvancedBeam(player.position,player.position+aim*range,.3f*Size,w);AdvancedBeam(player.position,player.position+Quaternion.Euler(0,25,0)*aim*range,.12f,w);AdvancedBeam(player.position,player.position+Quaternion.Euler(0,-25,0)*aim*range,.12f,w);}
                else {AdvancedPulse(player.position,range,w);characterVisual.Swing(aim);}
                if(w!=Weapon.HeroSword)return;
            }
            int baseProjectiles=w==Weapon.Revolver ? Quantity+2 : Quantity;
            int projectiles=baseProjectiles+Mathf.RoundToInt(baseProjectiles*(stats.Quantity-1));
            for(int i=0;i<projectiles && advancedShots.Count<200;i++)
            {
                Vector3 direction=Quaternion.Euler(0,(i-(projectiles-1)*.5f)*8,0)*aim;
                AddAdvancedShot(w,level,direction,nearest,power,stats);
            }
        }
        void AddAdvancedShot(Weapon w,int level,Vector3 direction,Enemy target,float power,WeaponStats stats)
        {
            float affixSize=stats.Size;
            var scale=w==Weapon.Bananarang ? new Vector3(.75f,.18f,.35f) : w==Weapon.Axe ? new Vector3(.8f,.16f,.8f) : w==Weapon.Rocket ? new Vector3(.3f,.3f,.9f) : w==Weapon.HeroSword ? new Vector3(2*Size,.12f,.22f) : new Vector3(.18f,.18f,.65f);
            var body=Shape(WeaponLabel(w),PrimitiveType.Cube,player.position,scale*Size*affixSize,AdvancedMaterial(w),world);body.rotation=Quaternion.LookRotation(direction);
            if(w==Weapon.Axe) {var handle=Shape("Axe handle",PrimitiveType.Cube,body.position,new Vector3(.13f,.13f,1.4f),boneMat,world);handle.SetParent(body,true);}
            AttachWeaponVfx(body,w,Size);
            float speed=(w==Weapon.Revolver ? 30 : w==Weapon.Rocket ? 12 : w==Weapon.Axe ? 8 : 18)*ProjectileSpeed;
            int baseBounces=1+level/2;
            advancedShots.Add(new AdvancedShot{kind=w,body=body,direction=direction,target=target,power=power*(w==Weapon.Rocket ? 2 : w==Weapon.Axe ? 1.5f : 1),speed=speed,radius=(w==Weapon.HeroSword ? 1.1f : .55f)*Size*affixSize,life=(w==Weapon.Bananarang ? 3 : 2.5f)*EffectDuration,bounces=baseBounces+Mathf.RoundToInt(baseBounces*(stats.Bounces-1))});
        }
        void TickAdvancedShots(float dt)
        {
            for(int i=advancedShots.Count-1;i>=0;i--)
            {
                var s=advancedShots[i];s.age+=dt;
                if(s.kind==Weapon.Bananarang && s.age>=.55f && !s.returning){s.returning=true;s.hit.Clear();}
                if(s.returning)s.direction=(player.position-s.body.position).normalized;
                if(s.kind==Weapon.Rocket || s.kind==Weapon.WirelessDagger)
                {
                    if(s.target==null || !enemies.Contains(s.target))s.target=Nearest(s.body.position,30,s.hit);
                    if(s.target!=null)s.direction=(s.target.body.position-s.body.position).normalized;
                }
                Vector3 start=s.body.position,end=start+s.direction*s.speed*Mathf.Min(dt,s.life),segment=end-start;
                bool pierce=s.kind==Weapon.Axe || s.kind==Weapon.HeroSword || s.kind==Weapon.Bananarang;
                Enemy first=null;float firstT=2;var contacts=new List<Enemy>();
                foreach(var e in enemies)
                {
                    if(s.hit.Contains(e))continue;
                    float t=Mathf.Clamp01(Vector3.Dot(e.body.position-start,segment)/Mathf.Max(.001f,segment.sqrMagnitude));
                    if((e.body.position-start-segment*t).sqrMagnitude>Mathf.Pow(s.radius+(e.elite ? 1 : .5f),2))continue;
                    if(pierce)contacts.Add(e);else if(t<firstT){first=e;firstT=t;}
                }
                s.body.position=end;s.life-=dt;
                if(pierce)foreach(var e in contacts){s.hit.Add(e);Hit(e,s.power);SpawnImpact(e.body.position,s.kind,Size);}
                else if(first!=null)
                {
                    s.body.position=start+segment*firstT;s.hit.Add(first);
                    if(s.kind==Weapon.Rocket){AreaHit(s.body.position,3*Size,s.power);AdvancedPulse(s.body.position,3*Size,s.kind);s.life=0;}
                    else
                    {
                        Hit(first,s.power);SpawnImpact(s.body.position,s.kind,Size);s.target=s.bounces>0 ? Nearest(s.body.position,12,s.hit) : null;
                        if(s.target==null)s.life=0;else{s.bounces--;s.direction=(s.target.body.position-s.body.position).normalized;}
                    }
                }
                if(s.kind==Weapon.Axe || s.kind==Weapon.Bananarang)s.body.Rotate(0,dt*600*ProjectileSpeed,0);
                else if(s.direction.sqrMagnitude>.001f)s.body.rotation=Quaternion.LookRotation(s.direction);
                if(s.returning)
                {
                    float t=Mathf.Clamp01(Vector3.Dot(player.position-start,segment)/Mathf.Max(.001f,segment.sqrMagnitude));
                    if((player.position-start-segment*t).sqrMagnitude<.5f)s.life=0;
                }
                if(s.life<=0){Destroy(s.body.gameObject);advancedShots.RemoveAt(i);}
            }
        }
        void AddAdvancedZone(Weapon w,Vector3 pos,Vector3 dir,Enemy target,float power,float radius,float life)
        {
            if(advancedZones.Count>=48){Destroy(advancedZones[0].body.gameObject);advancedZones.RemoveAt(0);}
            bool beam=w==Weapon.SpaceNoodle;
            var body=Shape(WeaponLabel(w),beam ? PrimitiveType.Cube : PrimitiveType.Cylinder,new Vector3(pos.x,beam ? 1 : .12f,pos.z),beam ? Vector3.one*.1f : new Vector3(radius*2,w==Weapon.Tornado ? 1.3f : .035f,radius*2),AdvancedMaterial(w),world);
            if(!beam && w!=Weapon.Mines) body.GetComponent<Renderer>().sharedMaterial=WeaponZoneMaterial(w);
            advancedZones.Add(new AdvancedZone{kind=w,body=body,target=target,direction=dir,power=power,radius=radius,life=life});
            if(!beam) AttachWeaponVfx(body,w,radius,true);
        }
        void TickAdvancedZones(float dt)
        {
            for(int i=advancedZones.Count-1;i>=0;i--)
            {
                var z=advancedZones[i];z.age+=dt;z.life-=dt;z.tick-=dt;
                if(z.kind==Weapon.SpaceNoodle)
                {
                    if(z.target==null || !enemies.Contains(z.target) || Vector3.Distance(player.position,z.target.body.position)>30)z.life=0;
                    else
                    {
                        var end=z.target.body.position;z.body.position=(player.position+end)/2;z.body.localScale=new Vector3(.12f,.12f,Vector3.Distance(player.position,end));
                        if((end-player.position).sqrMagnitude>.001f)z.body.rotation=Quaternion.LookRotation(end-player.position);
                        if(z.tick<=0 && z.life>0){LineHit(player.position,end,z.radius,z.power*.35f,z.kind);z.tick=.3f;}
                    }
                }
                else if(z.life>0)
                {
                    if(z.kind==Weapon.Tornado){z.body.position+=z.direction*dt*3*ProjectileSpeed;z.body.Rotate(0,300*dt,0);}
                    if(z.kind==Weapon.Mines)
                    {
                        if(z.age>=.4f && Nearest(z.body.position,1.8f*Size)!=null){AreaHit(z.body.position,z.radius,z.power*3);AdvancedPulse(z.body.position,z.radius,z.kind);z.life=0;}
                    }
                    else
                    {
                        for(int j=enemies.Count-1;j>=0;j--)
                        {
                            var e=enemies[j];var delta=e.body.position-z.body.position;delta.y=0;
                            if(delta.sqrMagnitude>z.radius*z.radius)continue;
                            if(z.kind==Weapon.BlackHole || z.kind==Weapon.Tornado)
                                e.body.position=MoveOnMap(e.body.position,delta.normalized*(z.kind==Weapon.BlackHole ? -1 : 1)*dt*4*(e==boss ? .2f : 1),e==boss ? 1.3f : .65f);
                            if(z.tick<=0)
                            {if(z.kind==Weapon.Frostwalker)ChillEnemy(e);Hit(e,z.power*(z.kind==Weapon.Frostwalker ? .18f : .35f));}
                        }
                        if(z.tick<=0)z.tick=.4f;
                    }
                }
                if(z.life<=0){Destroy(z.body.gameObject);advancedZones.RemoveAt(i);}
            }
        }
        string LoadoutSummary()
        {
            var parts=new List<string>();foreach(var w in weaponLevels){if(parts.Count==5)break;parts.Add(WeaponLabel(w.Key)+" "+w.Value);}
            return string.Join("  •  ",parts)+(weaponLevels.Count>5 ? "  +"+(weaponLevels.Count-5)+" muuta" : "");
        }
        void DrawAdvancedStatus()
        {
            if(AtMainMenu || Selecting || ShopOpen || Finished || PendingChoices>0)return;
            var lines=new List<string>();
            if(WeaponLevel(Weapon.Aegis)>0)lines.Add(shieldCharges>0 ? "AEGIS: kilpi valmis" : "AEGIS: latautuu");
            if(WeaponLevel(Weapon.Scythe)>0)lines.Add("VIIKATE: "+scytheCharge+" / 3 eliteä");
            if(WeaponLevel(Weapon.BloodMagic)>0)lines.Add("VERIMAGIA: +"+bloodHealthGained+" HP tällä kierroksella");
            if(WeaponLevel(Weapon.Dice)>0)lines.Add("NOPPA: "+(LastDiceRoll==0 ? "odottaa" : LastDiceRoll.ToString()));
            GUI.Label(new Rect(24,440,335,145),string.Join("\n",lines),textStyle);
        }
    }
}
