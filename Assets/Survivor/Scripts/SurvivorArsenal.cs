using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    public sealed partial class SurvivorGame
    {
        public float EffectDuration { get => current.EffectDuration; private set => current.EffectDuration = value; }
        public float ProjectileSpeed { get => current.ProjectileSpeed; private set => current.ProjectileSpeed = value; }
        public int ArsenalProjectileCount => shots.Count;
        public int FlameCount => flames.Count;
        public int OrbitCount => rocks.Count;
        List<ArsenalShot> shots => current.Shots;
        List<FlamePatch> flames => current.Flames;
        List<Transform> rocks => current.Rocks;
        float[] weaponTimers => current.WeaponTimers;
        Material fireMat, boneMat, rockMat, auraMat;
        LineRenderer auraRing { get => current.AuraRing; set => current.AuraRing = value; }
        Transform auraEffect { get => current.AuraEffect; set => current.AuraEffect = value; }
        float orbitAngle { get => current.OrbitAngle; set => current.OrbitAngle = value; }
        float orbitTick { get => current.OrbitTick; set => current.OrbitTick = value; }
        int starterPage { get => current.StarterPage; set => current.StarterPage = value; }
        internal sealed class ArsenalShot
        {
            public Transform body; public Weapon kind; public Vector3 direction;
            public float life,power,speed,radius; public int bounces;
            public HashSet<Enemy> hit=new HashSet<Enemy>();
        }
        internal sealed class FlamePatch { public Transform body; public float life,tick,power,radius; }

        static int WeaponUpgradeId(int index) => index<3 ? index : index<9 ? index+7 : index+9;
        static string WeaponLabel(Weapon w) => UpgradeNames[WeaponUpgradeId((int)w)].Split('/')[0].Trim();
        public int WeaponLevel(Weapon w) => weaponLevels.TryGetValue(w,out int n) ? n : 0;

        void ClearArsenal(bool resetStats)
        {
            ClearAdvanced(resetStats);
            foreach(var s in shots) if(s.body) Destroy(s.body.gameObject); shots.Clear();
            foreach(var f in flames) if(f.body) Destroy(f.body.gameObject); flames.Clear();
            foreach(var r in rocks) if(r) Destroy(r.gameObject); rocks.Clear();
            if(auraRing) Destroy(auraRing.gameObject); auraRing=null;
            if(auraEffect) Destroy(auraEffect.gameObject); auraEffect=null;
            System.Array.Clear(weaponTimers,0,weaponTimers.Length); orbitAngle=orbitTick=0; chunkersBlastTimer=0;
            if(resetStats) { EffectDuration=ProjectileSpeed=1; starterPage=0; }
        }

        void EnsureArsenalMaterials()
        {
            if(fireMat) return;
            fireMat=MakeMaterial(new Color(1,.32f,.04f)); boneMat=MakeMaterial(new Color(.94f,.9f,.72f));
            rockMat=MakeMaterial(new Color(.52f,.39f,.72f)); auraMat=MakeMaterial(new Color(.35f,1,.57f));
        }

        Enemy Nearest(Vector3 from,float range,HashSet<Enemy> excluded=null)
        {
            Enemy result=null; float best=range*range;
            foreach(var e in enemies)
            { if(excluded!=null && excluded.Contains(e)) continue; float sq=(e.body.position-from).sqrMagnitude; if(sq<best) { result=e;best=sq; } }
            return result;
        }

        void TickArsenal(float dt)
        {
            EnsureArsenalMaterials();
            for(int i=0;i<6;i++)
            {
                var w=(Weapon)(i+3); int level=WeaponLevel(w); if(level==0) continue;
                weaponTimers[i]-=dt;
                if(weaponTimers[i]<=0)
                {
                    CastArsenal(w,level);
                    float interval=w==Weapon.Flamewalker ? .65f : w==Weapon.Aura ? .55f : w==Weapon.Firestaff ? 1.5f : w==Weapon.Shotgun ? 1.3f : .95f;
                    // Näillä aseilla on oma weaponTimers[i]-ajastin (toisin kuin Sword/Lightning), joten
                    // Cooldown-affiksi voi nopeuttaa juuri tätä asetta erikseen muihin vaikuttamatta.
                    weaponTimers[i]=interval/(Mathf.Max(.1f,attackRate)*AffixMultiplier(w,AffixStat.Cooldown));
                }
            }
            TickOrbits(dt); TickShots(dt); TickFlames(dt); TickAdvanced(dt);
        }

        void CastArsenal(Weapon w,int level)
        {
            EnsureArsenalMaterials(); float power=damage*(1+.25f*(level-1))*AffixMultiplier(w,AffixStat.Damage);
            if(w==Weapon.Flamewalker)
            {
                if(flames.Count>=80) { Destroy(flames[0].body.gameObject); flames.RemoveAt(0); }
                float radius=1.4f*Size*AffixMultiplier(w,AffixStat.Size);
                var t=Shape("Flame trail",PrimitiveType.Cylinder,new Vector3(player.position.x,.09f,player.position.z),new Vector3(radius*2,.055f,radius*2),fireMat,world);
                flames.Add(new FlamePatch {body=t,life=(2.5f+.3f*level)*EffectDuration*AffixMultiplier(w,AffixStat.Duration),power=power*.32f,radius=radius}); return;
            }
            if(w==Weapon.Aura)
            {
                if(!auraRing)
                {
                    auraRing=new GameObject("Healing green? No: damage aura",typeof(LineRenderer)).GetComponent<LineRenderer>();
                    auraRing.transform.SetParent(world,false); auraRing.sharedMaterial=auraMat; auraRing.loop=true; auraRing.positionCount=48; auraRing.widthMultiplier=.08f;
                }
                if(!auraEffect && auraEffectPrefab) auraEffect=Instantiate(auraEffectPrefab,world).transform;
                AreaHit(player.position,3.2f*Size,power*.42f); return;
            }
            if(w==Weapon.Chunkers) return; // Continuous orbit has its own contact cadence.
            var target=Nearest(player.position,w==Weapon.Shotgun ? 10 : 23); if(target==null) return;
            var aim=(target.body.position-player.position).normalized;
            int baseCount=w==Weapon.Shotgun ? 5+Quantity-1 : Quantity;
            int count=baseCount+AffixCountBonus(w,AffixStat.Quantity,baseCount);
            float affixSize=AffixMultiplier(w,AffixStat.Size);
            for(int i=0;i<count && shots.Count<180;i++)
            {
                var direction=Quaternion.Euler(0,(i-(count-1)*.5f)*(w==Weapon.Shotgun ? 7 : 10),0)*aim;
                var mat=w==Weapon.Firestaff ? fireMat : w==Weapon.Bone ? boneMat : boltMaterial;
                var size=w==Weapon.Bone ? new Vector3(.2f,.2f,.75f) : Vector3.one*(w==Weapon.Firestaff ? .65f : .18f);
                var body=Shape(WeaponLabel(w),w==Weapon.Bone ? PrimitiveType.Cube : PrimitiveType.Sphere,player.position,size*Size*affixSize,mat,world);
                body.rotation=Quaternion.LookRotation(direction);
                if(w==Weapon.Bone)
                {
                    for(int side=-1;side<=1;side+=2)
                    { var end=Shape("Bone end",PrimitiveType.Sphere,body.position,Vector3.one*.3f,boneMat,world); end.SetParent(body,true); end.localPosition=new Vector3(0,0,side*.5f); }
                }
                // Fireball VFX is a looping flame/aura effect, not a one-shot burst - parent it to the
                // projectile so it's visible for the whole flight, not just at the impact explosion.
                if(w==Weapon.Firestaff && fireImpactEffect)
                {
                    var trail=Instantiate(fireImpactEffect,body.position,Quaternion.identity,body);
                    trail.transform.localScale=Vector3.one*ImpactBaseScale(Weapon.Firestaff)*Size;
                }
                float speed=(w==Weapon.Firestaff ? 14 : w==Weapon.Bone ? 18 : 27)*ProjectileSpeed*AffixMultiplier(w,AffixStat.ProjectileSpeed);
                int baseBounces=1+level/2;
                shots.Add(new ArsenalShot { body=body,kind=w,direction=direction,speed=speed,
                    life=w==Weapon.Shotgun ? 9/speed : 2.5f, power=power*(w==Weapon.Firestaff ? 1.4f : w==Weapon.Shotgun ? .42f : .85f),
                    radius=(w==Weapon.Firestaff ? 2.5f*Size : .35f*Size)*affixSize,bounces=baseBounces+AffixCountBonus(w,AffixStat.Bounces,baseBounces) });
            }
        }

        void AreaHit(Vector3 center,float radius,float power)
        {
            for(int i=enemies.Count-1;i>=0;i--)
            { var e=enemies[i]; var delta=e.body.position-center;delta.y=0; if(delta.sqrMagnitude<=radius*radius) Hit(e,power); }
        }

        void TickOrbits(float dt)
        {
            int level=WeaponLevel(Weapon.Chunkers);
            if(level>0)
            {
                float chunkersSize=AffixMultiplier(Weapon.Chunkers,AffixStat.Size);
                int baseOrbitCount=Quantity+1;
                int count=Mathf.Min(8,baseOrbitCount+AffixCountBonus(Weapon.Chunkers,AffixStat.Quantity,baseOrbitCount));
                while(rocks.Count<count) rocks.Add(Shape("Orbiting potato rock",PrimitiveType.Cube,player.position,Vector3.one*.65f*Size*chunkersSize,rockMat,world));
                orbitAngle+=dt*2.7f*ProjectileSpeed*AffixMultiplier(Weapon.Chunkers,AffixStat.ProjectileSpeed);
                for(int i=0;i<rocks.Count;i++)
                {
                    float a=orbitAngle+i*Mathf.PI*2/rocks.Count;
                    rocks[i].position=player.position+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*2.5f*Size;
                    rocks[i].localScale=Vector3.one*.65f*Size*chunkersSize; rocks[i].Rotate(45*dt,70*dt,35*dt);
                }
                // Iskukivet-variantti: jatkuvan kosketusvahingon sijaan kivet iskevät ajoittain
                // räjähtävän aluevahingon lähimpään viholliseen (painottaa Cooldownia/Sizea
                // Quantitya enemmän) - ks. WeaponVariants.cs.
                if (ActiveVariant(Weapon.Chunkers) == 1)
                {
                    chunkersBlastTimer -= dt;
                    if (chunkersBlastTimer <= 0)
                    {
                        chunkersBlastTimer = 1.1f / (Mathf.Max(.1f, attackRate) * AffixMultiplier(Weapon.Chunkers, AffixStat.Cooldown));
                        var target = Nearest(player.position, 9 * Size);
                        if (target != null)
                        {
                            SpawnImpact(target.body.position, Weapon.Chunkers, Size * 1.3f);
                            AreaHit(target.body.position, 1.4f * Size * chunkersSize, damage * .9f * (1 + .25f * (level - 1)) * VariantQualityMultiplier(Weapon.Chunkers) * AffixMultiplier(Weapon.Chunkers, AffixStat.Damage));
                        }
                    }
                }
                else
                {
                    orbitTick-=dt;
                    if(orbitTick<=0)
                    {
                        orbitTick=.28f/(Mathf.Max(.1f,attackRate)*AffixMultiplier(Weapon.Chunkers,AffixStat.Cooldown));
                        for(int i=enemies.Count-1;i>=0;i--)
                        {
                            var e=enemies[i]; foreach(var rock in rocks)
                            { if((e.body.position-rock.position).sqrMagnitude<Mathf.Pow(.9f*Size*chunkersSize,2)) {Hit(e,damage*.7f*(1+.25f*(level-1))*AffixMultiplier(Weapon.Chunkers,AffixStat.Damage));break;} }
                        }
                    }
                }
            }
            if(auraRing)
                for(int i=0;i<48;i++) { float a=i*Mathf.PI*2/48; auraRing.SetPosition(i, new Vector3(player.position.x+Mathf.Cos(a)*3.2f*Size,.16f,player.position.z+Mathf.Sin(a)*3.2f*Size)); }
            if(auraEffect)
            {
                auraEffect.position=new Vector3(player.position.x,.02f,player.position.z);
                float ringScale=4.5f*Size; auraEffect.localScale=new Vector3(ringScale,1f,ringScale);
            }
        }

        void TickShots(float dt)
        {
            for(int i=shots.Count-1;i>=0;i--)
            {
                var s=shots[i]; var start=s.body.position; var end=start+s.direction*s.speed*Mathf.Min(dt,s.life);
                var segment=end-start; Enemy contact=null; float closest=2;
                foreach(var e in enemies)
                {
                    if(s.hit.Contains(e)) continue;
                    float t=Mathf.Clamp01(Vector3.Dot(e.body.position-start,segment)/Mathf.Max(.0001f,segment.sqrMagnitude));
                    float hitRadius=(e.elite ? 1.1f : .6f)+(s.kind==Weapon.Firestaff ? .32f*Size : s.radius);
                    if(t<closest && (e.body.position-(start+t*segment)).sqrMagnitude<hitRadius*hitRadius) { contact=e;closest=t; }
                }
                s.life-=dt; s.body.position=end;
                if(contact!=null)
                {
                    s.body.position=start+segment*closest; s.hit.Add(contact);
                    if(s.kind==Weapon.Firestaff)
                    {
                        AreaHit(s.body.position,s.radius,s.power);
                        var pulse=Shape("Fireball explosion",PrimitiveType.Sphere,s.body.position,Vector3.one*s.radius,fireMat,world);
                        flashes.Add(new Flash {body=pulse,life=.14f}); s.life=0;
                        SpawnImpact(s.body.position,Weapon.Firestaff,s.radius);
                    }
                    else
                    {
                        Hit(contact,s.power);
                        var next=s.kind==Weapon.Bone && s.bounces>0 ? Nearest(s.body.position,10,s.hit) : null;
                        if(next!=null) { s.bounces--;s.direction=(next.body.position-s.body.position).normalized;s.body.rotation=Quaternion.LookRotation(s.direction); }
                        else s.life=0;
                    }
                }
                if(s.life<=0) {Destroy(s.body.gameObject);shots.RemoveAt(i);}
            }
        }

        void TickFlames(float dt)
        {
            for(int i=flames.Count-1;i>=0;i--)
            {
                var f=flames[i]; f.life-=dt; f.tick-=dt;
                if(f.life<=0) {Destroy(f.body.gameObject);flames.RemoveAt(i);continue;}
                if(f.tick<=0) {AreaHit(f.body.position,f.radius,f.power);f.tick=.4f;}
            }
        }

        void DrawStarterPages()
        {
            var allowed = new List<int>();
            for (int w = 0; w < TotalWeaponCount; w++) if (!mapLoadoutActive || mapLoadout.Contains(WeaponUpgradeId(w))) allowed.Add(w);
            int pageCount = Mathf.Max(1, (allowed.Count+2)/3);
            starterPage = ((starterPage % pageCount) + pageCount) % pageCount;
            GUI.Label(new Rect(90,90,1100,50),"POTUN ASEKAAPPI  /  "+(starterPage+1)+" / " + pageCount,titleStyle);
            GUI.Label(new Rect(90,150,1100,55), mapLoadoutActive ? "Valitse yksi aloitusase Mappi-tilan loadoutistasi." : "Valitse yksi aloitusase. Kaikki 30 asetta löytyvät myös tasopäivityksistä ja arkuista.",textStyle);
            for(int i=0;i<3;i++)
            {
                int slot = starterPage*3+i;
                if (slot >= allowed.Count) continue;
                int weapon=allowed[slot], id=WeaponUpgradeId(weapon);
                GUI.Box(new Rect(90+i*370,230,350,250),GUIContent.none);
                GUI.Label(new Rect(110+i*370,250,310,40),UpgradeNames[id],textStyle);
                GUI.Label(new Rect(110+i*370,305,310,110),UpgradeDetails[id],textStyle);
                if(GUI.Button(new Rect(110+i*370,428,310,42),"Aloita tällä",buttonStyle)) {SelectStarter(weapon);break;}
            }
            if(GUI.Button(new Rect(90,494,220,38),"← Edelliset",buttonStyle)) starterPage=(starterPage+pageCount-1)%pageCount;
            if(GUI.Button(new Rect(980,494,220,38),"Seuraavat →",buttonStyle)) starterPage=(starterPage+1)%pageCount;
            DrawLegacy(90,550);
        }
    }
}

