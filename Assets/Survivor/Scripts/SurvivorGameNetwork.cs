using Unity.Netcode;
using UnityEngine;

namespace BonkSurvivor
{
    // Multiplayer hooks: the host keeps simulating exactly as in single-player and additionally
    // simulates each connected friend's own Combatant (character, weapons, XP/level) through the
    // same casting/progression code the host uses, briefly making it "current" (see the `current`
    // field and Combatant.cs). A connected client renders the host's world instead of running its
    // own simulation, and picks its character/weapons/upgrades via RemotePlayerNet RPCs.
    // See the multiplayer plan in AGENTS.md.
    public sealed partial class SurvivorGame
    {
        bool NetworkActive => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        bool IsNetworkHost => NetworkActive && NetworkManager.Singleton.IsServer;
        bool NetworkClientMode => NetworkActive && !NetworkManager.Singleton.IsServer;
        int clientStarterPage;

        public void ApplyNetworkMap(int seed, int area, int runSeed)
        {
            if(!NetworkClientMode) return;
            ClearMapCombat(); Area=Mathf.Max(1,area); RunSeed=runSeed;
            GenerateProceduralMap(seed,false); CreateLandmarks();
            if(RemotePlayerNet.Local) RemotePlayerNet.Local.transform.position=MapSpawn;
        }

        // Host-side: create a fresh Combatant for a just-connected friend and give them the same
        // default look the host starts with (BuildPottu does this for the host at Start()).
        public Combatant CreateFriendCombatant(Transform body)
        {
            var c = new Combatant { Body = body, Selecting = true, AwaitingCharacterChoice = true };
            var prev = current; current = c;
            try { BuildCharacterVisual(PlayerCharacter.Pottu); } finally { current = prev; }
            return c;
        }

        public bool FriendSelectCharacter(Combatant c, PlayerCharacter character)
        {
            var prev = current; current = c;
            try { return SelectCharacter(character); } finally { current = prev; }
        }

        public bool FriendSelectStarter(Combatant c, int index)
        {
            var prev = current; current = c;
            try { return SelectStarter(index); } finally { current = prev; }
        }

        public bool FriendChooseUpgrade(Combatant c, int slot)
        {
            var prev = current; current = c;
            try { return ChooseUpgrade(slot); } finally { current = prev; }
        }

        // Vihollisten liikkeen kohde: lähin elossa oleva pelaaja (isäntä tai kaveri).
        // Kontaktivahinko isäntään lasketaan silti aina erikseen tuoreesta player.position-etäisyydestä,
        // ja kaverin kontaktivahinko/lähitaistelu hoidetaan TickNetworkCoopissa - tämä vaikuttaa vain
        // siihen, minne päin vihollinen kävelee/kääntyy.
        Vector3 NearestCombatantPosition(Vector3 from)
        {
            Vector3 best = player.position; float bestSqr = (best - from).sqrMagnitude;
            var remotes = RemotePlayerNet.Active;
            for (int i = 0; i < remotes.Count; i++)
            {
                var rp = remotes[i];
                if (!rp || !rp.IsAlive) continue;
                float sqr = (rp.Position - from).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = rp.Position; }
            }
            return best;
        }

        void TickNetworkClient(float dt)
        {
            var rp = RemotePlayerNet.Local;
            if (rp && followCamera)
            {
                var target = rp.transform.position + new Vector3(0, 23, -19);
                followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, target, 1 - Mathf.Exp(-8 * dt));
                followCamera.transform.rotation = Quaternion.Euler(48, 0, 0);
            }
        }

        void TickNetworkCoop(float dt)
        {
            if (!IsNetworkHost) return;
            var remotes = RemotePlayerNet.Active;
            for (int r = 0; r < remotes.Count; r++)
            {
                var rp = remotes[r];
                var c = rp ? rp.FriendState : null;
                if (!rp || c == null || !c.IsAlive) continue;
                var prev = current; current = c;
                try
                {
                    if (c.Invulnerability > 0) c.Invulnerability -= dt;

                    if (!c.Selecting)
                    {
                        c.AttackTimer -= dt;
                        if (c.AttackTimer <= 0) { AttackWeapons(); c.AttackTimer = 1 / Mathf.Max(.1f, attackRate); }
                        TickArsenal(dt);

                        // Enemy contact damage, Aegis-aware (mirrors ReceiveDamage's block check).
                        for (int i = enemies.Count - 1; i >= 0; i--)
                        {
                            var e = enemies[i];
                            var delta = c.Body.position - e.body.position; delta.y = 0;
                            if (delta.sqrMagnitude < 1.7f * 1.7f && c.Invulnerability <= 0)
                            {
                                if (!BlockWithAegis())
                                {
                                    c.Health = Mathf.Max(0, c.Health - Mathf.Max(1, (e.elite ? 26f : 13f) - Armor));
                                    c.Invulnerability = .45f;
                                }
                            }
                        }

                        // Drops: friend collects XP/gold/health orbs near them, same rules as the host.
                        for (int i = drops.Count - 1; i >= 0; i--)
                        {
                            var d = drops[i]; float distance = Vector3.Distance(d.body.position, c.Body.position);
                            if (distance < pickupRadius) d.body.position = Vector3.MoveTowards(d.body.position, c.Body.position, 15 * dt);
                            if (distance < 1.1f)
                            {
                                if (d.isHealth) c.Health = Mathf.Min(c.MaxHealth, c.Health + d.value);
                                else { GrantExperience(d.value); c.Coins += d.value; }
                                Destroy(d.body.gameObject); drops.RemoveAt(i);
                            }
                        }
                    }
                }
                finally { current = prev; }

                // Mirror the bits the client's HUD needs onto replicated NetworkVariables.
                rp.Health.Value = c.Health; rp.MaxHealthNet.Value = c.MaxHealth;
                rp.Level.Value = c.Level; rp.Coins.Value = c.Coins;
                rp.PendingChoices.Value = c.PendingChoices;
                rp.Choice0.Value = c.Choices.Count > 0 ? c.Choices[0] : -1;
                rp.Choice1.Value = c.Choices.Count > 1 ? c.Choices[1] : -1;
                rp.Choice2.Value = c.Choices.Count > 2 ? c.Choices[2] : -1;
                rp.Selecting.Value = c.Selecting; rp.AwaitingCharacterChoice.Value = c.AwaitingCharacterChoice;
                rp.SelectedCharacter.Value = (int)c.SelectedCharacter;
            }
        }

        void DrawNetworkClientHud()
        {
            var rp = RemotePlayerNet.Local;
            if (!rp) return;
            if (rp.AwaitingCharacterChoice.Value) { DrawFriendCharacterSelect(rp); return; }
            if (rp.Selecting.Value && (PlayerCharacter)rp.SelectedCharacter.Value == PlayerCharacter.Pottu) { DrawFriendStarterPages(rp); return; }
            if (rp.PendingChoices.Value > 0) { DrawFriendUpgradeChoice(rp); return; }

            GUI.Box(new Rect(20, 20, 330, 110), GUIContent.none);
            GUI.Label(new Rect(36, 28, 310, 32), "KAVERINA ISÄNNÄN PELISSÄ", hudNameStyle);
            float hp = rp.Health.Value, maxHp = Mathf.Max(1, rp.MaxHealthNet.Value);
            Bar(new Rect(36, 66, 290, 16), hp / maxHp, new Color(.3f, .95f, .55f));
            GUI.Label(new Rect(36, 84, 290, 24), "HP " + Mathf.CeilToInt(hp) + "/" + Mathf.CeilToInt(maxHp), textStyle);
            GUI.Label(new Rect(36, 106, 290, 24), "TASO " + rp.Level.Value + "   •   " + rp.Coins.Value + " kultaa", textStyle);
            GUI.Label(new Rect(36, 140, 450, 30), "KARTTA " + Area, textStyle);
            DrawDungeonMap();
            if (GUI.Button(new Rect(1070, 650, 185, 44), "Katkaise yhteys", buttonStyle) && SurvivorNetwork.Instance)
                SurvivorNetwork.Instance.Disconnect();
        }

        void DrawFriendCharacterSelect(RemotePlayerNet rp)
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
                float x=x0+col*(width+gapX), y=top+row*(cardHeight+gapY);
                GUI.Box(new Rect(x,y,width,cardHeight),GUIContent.none);
                GUI.Label(new Rect(x+14,y+10,width-28,30),entry.name,hudNameStyle);
                GUI.Label(new Rect(x+14,y+46,width-28,cardHeight-92),entry.description,cardTextStyle);
                if(GUI.Button(new Rect(x+14,y+cardHeight-38,width-28,32),"Valitse",buttonStyle)) rp.RequestCharacterRpc((int)entry.character);
            }
        }

        void DrawFriendStarterPages(RemotePlayerNet rp)
        {
            int pageCount = (TotalWeaponCount+2)/3;
            GUI.Label(new Rect(90,90,1100,50),"POTUN ASEKAAPPI  /  "+(clientStarterPage+1)+" / " + pageCount,titleStyle);
            GUI.Label(new Rect(90,150,1100,55),"Valitse yksi aloitusase. Kaikki 30 asetta löytyvät myös tasopäivityksistä ja arkuista.",textStyle);
            for(int i=0;i<3;i++)
            {
                int weapon=clientStarterPage*3+i, id=WeaponUpgradeId(weapon);
                GUI.Box(new Rect(90+i*370,230,350,250),GUIContent.none);
                GUI.Label(new Rect(110+i*370,250,310,40),UpgradeNames[id],textStyle);
                GUI.Label(new Rect(110+i*370,305,310,110),UpgradeDetails[id],textStyle);
                if(GUI.Button(new Rect(110+i*370,428,310,42),"Aloita tällä",buttonStyle)) rp.RequestStarterRpc(weapon);
            }
            if(GUI.Button(new Rect(90,494,220,38),"← Edelliset",buttonStyle)) clientStarterPage=(clientStarterPage+pageCount-1)%pageCount;
            if(GUI.Button(new Rect(980,494,220,38),"Seuraavat →",buttonStyle)) clientStarterPage=(clientStarterPage+1)%pageCount;
        }

        void DrawFriendUpgradeChoice(RemotePlayerNet rp)
        {
            GUI.Label(new Rect(90,90,1100,55), "TASO " + rp.Level.Value + " / VALITSE PÄIVITYS", titleStyle);
            GUI.Label(new Rect(90,150,1100,55), "Peli jatkuu taustalla. Valitse yksi kolmesta.", textStyle);
            int[] choiceIds = { rp.Choice0.Value, rp.Choice1.Value, rp.Choice2.Value };
            for (int i = 0; i < 3; i++)
            {
                int id = choiceIds[i]; if (id < 0) continue;
                GUI.Box(new Rect(90+i*370,230,350,250), GUIContent.none);
                GUI.Label(new Rect(110+i*370,250,310,45), UpgradeNames[id], textStyle);
                GUI.Label(new Rect(110+i*370,310,310,95), UpgradeDetails[id], textStyle);
                if (GUI.Button(new Rect(110+i*370,420,310,42), "Valitse", buttonStyle)) rp.RequestUpgradeRpc(i);
            }
        }
    }
}
