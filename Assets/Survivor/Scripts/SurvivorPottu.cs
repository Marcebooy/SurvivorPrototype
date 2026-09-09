using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BonkSurvivor
{
    public sealed partial class SurvivorGame
    {
        public enum PlayerCharacter { Pottu, Velho, Ritari, Necromancer, Berserker, Golem, Hunter, Ninja, Paladin }
        public PlayerCharacter SelectedCharacter { get => current.SelectedCharacter; private set => current.SelectedCharacter = value; }
        bool awaitingCharacterChoice { get => current.AwaitingCharacterChoice; set => current.AwaitingCharacterChoice = value; }
        ICharacterVisual characterVisual { get => current.CharacterVisual; set => current.CharacterVisual = value; }
        Transform visualRoot { get => current.VisualRoot; set => current.VisualRoot = value; }
        // The host's own real visual, unwrapped - TickNetworkCoop wraps it (once a HostAvatarNet
        // mirror exists) so its Swing/Bonk/Block/Shockwave calls also replay for a connected friend.
        ICharacterVisual hostOwnVisual;
        float dodgeLeft, dodgeCooldown, hurtFlash;
        Vector3 dodgeDirection;
        public bool IsDodging => dodgeLeft > 0;
        public float DodgeCooldown => dodgeCooldown;
        readonly List<DamageLabel> damageLabels = new List<DamageLabel>();
        sealed class DamageLabel { public Vector3 position; public string text; public float life; public bool crit; }
        readonly Dictionary<Renderer,float> hitFlashes=new Dictionary<Renderer,float>();
        readonly List<Renderer> flashKeys=new List<Renderer>();
        MaterialPropertyBlock flashBlock;
        Mesh swipeMesh;

        void BuildSwipe()
        {
            var v=new List<Vector3>(); var t=new List<int>();
            for(int i=0;i<=24;i++)
            {
                float a=Mathf.Lerp(-104,104,i/24f)*Mathf.Deg2Rad;
                var d=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a)); v.Add(d*.465f); v.Add(d*.5f);
                if(i<24) { int n=i*2; t.Add(n);t.Add(n+1);t.Add(n+2);t.Add(n+1);t.Add(n+3);t.Add(n+2); }
            }
            swipeMesh=new Mesh { name="Pottu pan swipe arc" }; swipeMesh.SetVertices(v); swipeMesh.SetTriangles(t,0); swipeMesh.RecalculateNormals(); swipeMesh.RecalculateBounds();
            slash.GetComponent<MeshFilter>().sharedMesh=swipeMesh;
        }

        void BuildPottu()
        {
            hostState.Body=new GameObject("Player").transform; hostState.Body.SetParent(world,false); hostState.Body.position=Vector3.up;
            BuildCharacterVisual(PlayerCharacter.Pottu);
        }
        void BuildCharacterVisual(PlayerCharacter character)
        {
            if(visualRoot) Destroy(visualRoot.gameObject);
            SelectedCharacter=character;
            var v = BuildCharacterVisualOn(player, character, out var root);
            characterVisual = v; visualRoot = root;
            if (current == hostState) { hostOwnVisual = v; swordVariantAttached = false; weaponVisualsAttached = false; }
        }

        // Standalone (not routed through `current`) so any peer can build the SAME deterministic
        // body locally under an arbitrary transform - used to show a connected friend's own body
        // and the host's body on every machine without networking the mesh itself. See
        // RemotePlayerNet.cs and HostAvatarNet.cs.
        public static ICharacterVisual BuildCharacterVisualOn(Transform parent, PlayerCharacter character, out Transform root)
        {
            root=new GameObject(character+" visual").transform; root.SetParent(parent,false);
            if(character==PlayerCharacter.Velho) { var v=root.gameObject.AddComponent<VelhoVisual>(); v.Build(); return v; }
            if(character==PlayerCharacter.Ritari) { var v=root.gameObject.AddComponent<RitariVisual>(); v.Build(); return v; }
            if(character==PlayerCharacter.Necromancer) { var v=root.gameObject.AddComponent<NecromancerVisual>(); v.Build(); return v; }
            if(character==PlayerCharacter.Berserker) { var v=root.gameObject.AddComponent<BerserkerVisual>(); v.Build(); return v; }
            if(character==PlayerCharacter.Golem) { var v=root.gameObject.AddComponent<GolemVisual>(); v.Build(); return v; }
            if(character==PlayerCharacter.Hunter) { var v=root.gameObject.AddComponent<HunterVisual>(); v.Build(); return v; }
            if(character==PlayerCharacter.Ninja) { var v=root.gameObject.AddComponent<NinjaVisual>(); v.Build(); return v; }
            if(character==PlayerCharacter.Paladin) { var v=root.gameObject.AddComponent<PaladinVisual>(); v.Build(); return v; }
            var pottu=root.gameObject.AddComponent<PottuVisual>(); pottu.Build(); return pottu;
        }
        public bool SelectCharacter(PlayerCharacter character)
        {
            if(!Selecting || !awaitingCharacterChoice) return false;
            // For a friend Combatant, RemotePlayerNet already built (and wired in) the mesh so every
            // peer shows the same one - building another here would leave two overlapping bodies.
            if(current == hostState) BuildCharacterVisual(character); else SelectedCharacter = character;
            ResetPottu(); awaitingCharacterChoice=false;
            if(character==PlayerCharacter.Velho) weaponLevels[Weapon.Lightning]=1;
            else if(character==PlayerCharacter.Ritari) weaponLevels[Weapon.Sword]=1;
            else if(character==PlayerCharacter.Necromancer) weaponLevels[Weapon.Bone]=1;
            else if(character==PlayerCharacter.Berserker) weaponLevels[Weapon.CorruptedSword]=1;
            else if(character==PlayerCharacter.Golem) weaponLevels[Weapon.Chunkers]=1;
            else if(character==PlayerCharacter.Hunter) weaponLevels[Weapon.Bow]=1;
            else if(character==PlayerCharacter.Ninja) weaponLevels[Weapon.Katana]=1;
            else if(character==PlayerCharacter.Paladin) weaponLevels[Weapon.Aura]=1;
            if(character!=PlayerCharacter.Pottu && !awaitingLoadoutChoice)
            {
                Selecting=false;
                notice="Etsi arkkuja ja pyhäkkö. Portaali avautuu 90 sekunnissa."; noticeUntil=Elapsed+7;
            }
            return true;
        }
        static readonly (PlayerCharacter character, string name, string description)[] CharacterRoster =
        {
            (PlayerCharacter.Pottu, "POTTU", "Tasapainoinen lähitaistelija.\nAloitusase: valitse itse (30 asetta).\nPelityyli: luotettava lähivahinko, kohtuullinen HP."),
            (PlayerCharacter.Velho, "VELHO", "Area damage ja elementtiefektit.\nAloitusase: Salamasauva - sähköisku, joka ketjuttuu lähellä oleviin vihollisiin.\nPelityyli: suuri vahinko ja alue, heikompi kestävyys."),
            (PlayerCharacter.Ritari, "RITARI", "Tankki ja lähitaistelija.\nAloitusase: Sword - miekaniskut lähellä oleviin vihollisiin.\nPelityyli: kestävä lähitaistelu; kilpi (Aegis) täydentää buildia hyvin myöhemmin."),
            (PlayerCharacter.Necromancer, "NECROMANCER", "Kutsuihin ja tappojen ketjuttamiseen erikoistunut.\nAloitusase: Pomppuluu - kimpoaa vihollisesta toiseen.\nPelityyli: heikompi suora vahinko, mutta ketjuttuvat osumat."),
            (PlayerCharacter.Berserker, "BERSERKERI", "Aggressiivinen lähitaistelija, joka hyötyy matalasta HP:stä.\nAloitusase: Corrupted Sword - vahvistuu puuttuvan HP:n mukaan.\nPelityyli: suuri vahinko, puolustus heikkenee taistelun aikana."),
            (PlayerCharacter.Golem, "GOLEM", "Hidas tankki ja alueen hallitsija.\nAloitusase: Chunkers - kiertävät kivet murskaavat lähellä olevia.\nPelityyli: erittäin suuri HP ja koko; jokainen osuma laukaisee shokkiaallon."),
            (PlayerCharacter.Hunter, "METSÄSTÄJÄ", "Etäisyysvahinko ja kriittiset osumat.\nAloitusase: Bow - nuolet läpäisevät vihollisia.\nPelityyli: suuri kantama ja nopeus, heikompi HP ja Armor."),
            (PlayerCharacter.Ninja, "NINJA", "Nopea glass cannon ja väistöihin perustuva hahmo.\nAloitusase: Katana - nopeat automaattiset iskut.\nPelityyli: erittäin suuri liikkumisnopeus, matala HP; väistö on oma varjoaskel-animaatio."),
            (PlayerCharacter.Paladin, "PALADIINI", "Puolustava hybridihahmo.\nAloitusase: Aura - jatkuva vahinkokehä pelaajan ympärillä.\nPelityyli: Armor ja HP; kilpi (Aegis) täydentää buildia hyvin myöhemmin."),
        };
        void DrawCharacterSelect()
        {
            GUI.Label(new Rect(90,74,1100,40),"VALITSE HAHMO",titleStyle);
            GUI.Label(new Rect(90,116,1100,30),"Jokaisella hahmolla on oma pelityyli ja aloitusase.",textStyle);
            const float x0=40, x1=1240, gapX=16, gapY=14, top=156, cardHeight=210;
            int columns=CharacterRoster.Length<=5 ? CharacterRoster.Length : Mathf.CeilToInt(CharacterRoster.Length/2f);
            float width=(x1-x0-(columns-1)*gapX)/columns;
            for(int i=0;i<CharacterRoster.Length;i++)
            {
                var entry=CharacterRoster[i];
                int col=i%columns, row=i/columns;
                DrawCharacterCard(x0+col*(width+gapX),top+row*(cardHeight+gapY),width,cardHeight,entry.character,entry.name,entry.description);
            }
            DrawLegacy(90,596);
        }
        void DrawCharacterCard(float x, float y, float width, float height, PlayerCharacter character, string name, string description)
        {
            GUI.Box(new Rect(x,y,width,height),GUIContent.none);
            GUI.Label(new Rect(x+14,y+10,width-84,30),name,cardNameStyle);
            int masteryLevel = MasteryLevelFor(character);
            if (masteryLevel > 0) GUI.Label(new Rect(x+width-84,y+14,70,22),"M " + masteryLevel,badgeStyle);
            GUI.Label(new Rect(x+14,y+46,width-28,height-92),description,cardTextStyle);
            if(GUI.Button(new Rect(x+14,y+height-38,width-28,32),"Valitse",buttonStyle)) SelectCharacter(character);
        }
        static string CharacterTitle(PlayerCharacter character) => character switch
        {
            PlayerCharacter.Velho => "VELHO / ZAP",
            PlayerCharacter.Ritari => "RITARI / MIEKKA",
            PlayerCharacter.Necromancer => "NECROMANCER / LUU",
            PlayerCharacter.Berserker => "BERSERKERI / RAIVO",
            PlayerCharacter.Golem => "GOLEM / MURSKA",
            PlayerCharacter.Hunter => "METSÄSTÄJÄ / JOUSI",
            PlayerCharacter.Ninja => "NINJA / VARJO",
            PlayerCharacter.Paladin => "PALADIINI / AURA",
            _ => "POTTU / BONK",
        };
        void ResetPottu()
        {
            dodgeLeft=dodgeCooldown=hurtFlash=0; damageLabels.Clear();
            foreach(var pair in hitFlashes) if(pair.Key) pair.Key.SetPropertyBlock(null);
            hitFlashes.Clear(); player.rotation=Quaternion.Euler(0,180,0); characterVisual.ResetPose();
        }
        public bool TryDodge(Vector3 direction)
        {
            if(Selecting || ShopOpen || PendingChoices>0 || Finished || dodgeCooldown>0 || IsDodging) return false;
            direction.y=0; dodgeDirection=direction.sqrMagnitude>.001f ? direction.normalized : player.forward;
            dodgeLeft=.38f; dodgeCooldown=1.6f; player.rotation=Quaternion.LookRotation(dodgeDirection); return true;
        }
        void MovePottu(Vector3 move, float dt, bool dodgePressed)
        {
            dodgeCooldown=Mathf.Max(0,dodgeCooldown-dt);
            if(dodgePressed) TryDodge(move);
            bool rolling=IsDodging;
            var motion=rolling ? dodgeDirection*(18f*Mathf.Min(dt,dodgeLeft)) : move*(moveSpeed*dt);
            player.position=MoveOnMap(player.position, motion, .65f); player.position=new Vector3(player.position.x,1,player.position.z);
            if(!rolling && move.sqrMagnitude>.01f) player.rotation=Quaternion.Slerp(player.rotation,Quaternion.LookRotation(move),dt*14);
            dodgeLeft=Mathf.Max(0,dodgeLeft-dt);
            float roll=IsDodging ? 1-dodgeLeft/.38f : -1;
            float panSize=Size*(1+(weaponLevels.TryGetValue(Weapon.Sword,out int level) ? level-1 : 0)*.13f);
            // showMeleeWeapon is false when SwordVariantVisual (Kaksoiskajo) is already showing a
            // model for Sword, so Potun oma pannu ei näy päällekkäin sen kanssa - ks. SwordVariantVisual.cs.
            bool showMeleeWeapon=weaponLevels.ContainsKey(Weapon.Sword) && !HasSwordVariantVisual;
            characterVisual.Tick(dt,rolling ? 0 : move.magnitude,roll,panSize,showMeleeWeapon,Health/Mathf.Max(1,maxHealth));
            TickFeedback(dt);
        }
        bool ReceiveDamage(float amount)
        {
            if(IsDodging || invulnerability>0 || Finished) return false;
            if(BlockWithAegis()) return false;
            Health=Mathf.Max(0,Health-Mathf.Max(1,amount-Armor)); invulnerability=.45f; hurtFlash=.25f;
            characterVisual.Shockwave();
            if(Health<=0) FinishRun(); return true;
        }
        void HitFeedback(Enemy enemy, float amount, bool crit)
        {
            if(damageLabels.Count>=60) damageLabels.RemoveAt(0);
            damageLabels.Add(new DamageLabel { position=enemy.body.position+Vector3.up*1.25f, text=(crit ? "BONK! " : "")+Mathf.CeilToInt(amount),life=.7f,crit=crit });
            var renderer=enemy.body.GetComponent<Renderer>();
            if(renderer) { if(flashBlock==null) flashBlock=new MaterialPropertyBlock(); flashBlock.SetColor("_BaseColor",Color.white); renderer.SetPropertyBlock(flashBlock); hitFlashes[renderer]=.1f; }
            characterVisual.Bonk();
        }
        void TickFeedback(float dt)
        {
            hurtFlash=Mathf.Max(0,hurtFlash-dt);
            for(int i=damageLabels.Count-1;i>=0;i--) { var label=damageLabels[i]; label.life-=dt; label.position+=Vector3.up*dt*1.6f; if(label.life<=0) damageLabels.RemoveAt(i); }
            flashKeys.Clear(); flashKeys.AddRange(hitFlashes.Keys);
            foreach(var r in flashKeys)
            { if(!r) { hitFlashes.Remove(r); continue; } float left=hitFlashes[r]-dt; if(left<=0) { r.SetPropertyBlock(null); hitFlashes.Remove(r); } else hitFlashes[r]=left; }
        }
        void DrawPottuHUD()
        {
            if(Selecting || PendingChoices>0 || ShopOpen || Finished) return;
            DrawAdvancedStatus();
            GUI.Label(new Rect(24,600,330,32),IsDodging ? "VÄISTÖSSÄ!" : dodgeCooldown>0 ? "Väistö: "+dodgeCooldown.ToString("0.0")+" s" : "Väistö valmis [SPACE]",textStyle);
            foreach(var label in damageLabels)
            {
                var p=followCamera.WorldToViewportPoint(label.position); if(p.z<=0) continue;
                GUI.color=label.crit ? new Color(1,.85f,.2f,Mathf.Clamp01(label.life*3)) : new Color(1,1,1,Mathf.Clamp01(label.life*3));
                GUI.Label(new Rect(p.x*1280-35,(1-p.y)*720-16,180,36),label.text,textStyle);
            }
            if(hurtFlash>0)
            { GUI.color=new Color(1,.15f,.1f,hurtFlash*.65f); GUI.DrawTexture(new Rect(0,0,1280,8),Texture2D.whiteTexture); GUI.DrawTexture(new Rect(0,712,1280,8),Texture2D.whiteTexture); }
            GUI.color=Color.white;
        }
    }
}
