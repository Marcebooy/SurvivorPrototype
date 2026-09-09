using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    public sealed partial class SurvivorGame
    {
        public enum Weapon { Sword, Bow, Lightning, Chunkers, Flamewalker, Bone, Firestaff, Aura, Shotgun, Revolver, Aegis, Bananarang, Axe, SpaceNoodle, Sniper, Rocket, Mines, WirelessDagger, Frostwalker, Tornado, Dexecutioner, BloodMagic, BlackHole, PoisonFlask, Katana, DragonBreath, Dice, HeroSword, CorruptedSword, Scythe }
        public bool Selecting { get => current.Selecting; private set => current.Selecting = value; }
        public int PendingChoices { get => current.PendingChoices; private set => current.PendingChoices = value; }
        public int Area { get; private set; }
        public int Silver { get; private set; }
        public int LegacyHealth { get; private set; }
        public int EarnedSilver { get; private set; }
        public float BestSurvivalSeconds { get; private set; }
        public bool BossDefeated { get; private set; }
        public bool BossActive => boss != null;
        public int WeaponCount => weaponLevels.Count;
        public int TomeCount => tomeLevels.Count;
        public IReadOnlyList<int> Choices => choices;
        public float Size { get => current.Size; private set => current.Size = value; }
        public int Quantity { get => current.Quantity; private set => current.Quantity = value; }
        public float CritChance { get => current.CritChance; private set => current.CritChance = value; }
        public float Armor { get => current.Armor; private set => current.Armor = value; }
        Dictionary<Weapon, int> weaponLevels => current.WeaponLevels;
        Dictionary<int, int> tomeLevels => current.TomeLevels;
        const int MaxWeaponKinds = 5, MaxTomeKinds = 5;
        List<int> choices => current.Choices;
        readonly List<Landmark> landmarks = new List<Landmark>();
        readonly List<Flash> flashes = new List<Flash>();
        Enemy boss;
        float areaStarted, curse = 1, bossAttackTimer, bossDefeatedAt = -1;
        bool bossTimerExpired;
        int bossKills;
        bool rewardPaid;
        bool newRecord;
        string interactionHint = "";
        // Map-mode loadout (interim version - see DrawLoadoutSelect): the player freely picks up
        // to MaxWeaponKinds weapons and MaxTomeKinds Tomes from the full existing roster, since
        // the weapon/Tome variant + fragment unlock system this is meant to gate against doesn't
        // exist yet. mapLoadout stores UpgradeNames ids (weapons via WeaponUpgradeId + Tome ids
        // directly); loadoutWeapons/loadoutTomes are the in-progress staging sets on the picker
        // screen, cleared and copied into mapLoadout on confirm.
        bool awaitingLoadoutChoice;
        bool mapLoadoutActive;
        readonly HashSet<int> mapLoadout = new HashSet<int>();
        readonly HashSet<int> loadoutWeapons = new HashSet<int>();
        readonly HashSet<int> loadoutTomes = new HashSet<int>();
        static readonly int[] TomeIds = { 3, 4, 5, 6, 7, 8, 9, 16, 17 };
        const string SaveKey = "BonkSurvivor.Prototype.v1.";
        sealed class Landmark { public Transform body; public int kind; public bool used; }
        sealed class Flash { public Transform body; public float life; }
        static readonly string[] UpgradeNames = WithAdvancedNames(new string[] {
            "Pannu / BONK", "Bow / Jousi", "Lightning / Salamasauva",
            "Damage Tome", "Cooldown Tome", "Quantity Tome", "Size Tome", "Precision Tome", "Armor Tome", "Movement Tome", "Kiertokivet / Chunkers", "Tulijälki / Flamewalker", "Pomppuluu / Bone", "Tulipallo / Firestaff", "Aura", "Haulikko / Shotgun", "Duration Tome", "Projectile Speed Tome"
        });
        static readonly string[] UpgradeDetails = WithAdvancedDetails(new string[] {
            "Uusi ase tai +1 asetaso. Leveä pannunheilautus. Asetaso kasvattaa pannua.",
            "Uusi ase tai +1 asetaso. Nuolet läpäisevät vihollisia.",
            "Uusi ase tai +1 asetaso. Salama ketjuttuu lähellä oleviin kohteisiin.",
            "+1 % vahinkoa kaikille aseille.", "+1 % hyökkäysnopeutta kaikille aseille.",
            "+1 ammus, kiertokivi, salamakohde tai pannun lisäisku. Max 6. Ei vaikuta auraan tai tulijälkeen.", "+1 % aseiden osuma- ja aluekokoa; kiertokivien kiertorata kasvaa.",
            "+1 prosenttiyksikkö kriittisen osuman mahdollisuuteen (max 70 %).", "+2 panssaria. Vähentää osumavahinkoa.", "+1 % liikkumisnopeutta.",
            "Kivet kiertävät Pottua ja murskaavat. Quantity: lisää kiviä. Size: koko ja kiertorata.",
            "Jättää polttavia alueita. Damage: vahinko. Size: alue. Duration: kesto.",
            "Luu kimpoaa seuraavaan viholliseen. Asetaso lisää vahinkoa ja kimpoiluja. Quantity: lisää luita.",
            "Tulipallo räjähtää aluevahingoksi. Quantity: lisää palloja. Size: suurempi räjähdys.",
            "Vahinkokehä Potun ympärillä. Size: säde. Cooldown: nopeampi syke. Quantity ei vaikuta.",
            "Lyhyen kantaman hauliparvi. Quantity: lisää hauleja. Asetaso kasvattaa vahinkoa.",
            "+1 % tulijälkien, erikoisammusten, miinojen ja alueiden kestoa.", "+1 % ammusten ja kiertokivien nopeutta."
        });

        void ResetProgression()
        {
            foreach (var p in landmarks) if (p.body) Destroy(p.body.gameObject);
            foreach (var f in flashes) if (f.body) Destroy(f.body.gameObject);
            ClearArsenal(true);
            landmarks.Clear(); flashes.Clear(); weaponLevels.Clear(); tomeLevels.Clear(); choices.Clear();
            Silver = PlayerPrefs.GetInt(SaveKey + "Silver", 0);
            LegacyHealth = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey + "Health", 0), 0, 10);
            maxHealth += LegacyHealth * 10; Health = maxHealth;
            Size = 1; Quantity = 1; CritChance = Armor = 0;
            Area = 1; areaStarted = 0; curse = 1; boss = null; bossKills = 0; bossAttackTimer = 3; bossDefeatedAt = -1; bossTimerExpired = false;
            EarnedSilver = 0; rewardPaid = false; accountXpPaidThisRun = false; BossDefeated = false; PendingChoices = 0; Selecting = true; awaitingCharacterChoice = true;
            mapLoadoutActive = false; mapLoadout.Clear(); loadoutWeapons.Clear(); loadoutTomes.Clear();
            // Loadout picker only applies to a real Map-mode run (a fixed Mosswood/Luuluola map,
            // not Survival's always-procedural map) - gates BeginMapRun()/CreateLandmarks() until
            // the player confirms or skips (see DrawLoadoutSelect/ConfirmLoadout/SkipLoadout).
            awaitingLoadoutChoice = selectedMapType != MapType.Procedural;
            if (awaitingLoadoutChoice)
            {
                // Esitäyttö HAHMOT-tabin pysyvästä oletusloadoutista (CharacterShop.cs) - vain
                // lähtökohta, ei lukko: ConfirmLoadout/SkipLoadout eivät koskaan kirjoita takaisin
                // defaultLoadoutiin, joten pelaaja voi vapaasti muokata tätä juuri ennen käynnistystä
                // ilman että se muuttaa pysyvää oletusta.
                foreach (var id in DefaultLoadoutFor(SelectedCharacter))
                {
                    if (!IsUpgradeOwned(SelectedCharacter, id)) continue; // omistus on voinut muuttua tallennuksen jälkeen
                    if (IsTomeId(id)) loadoutTomes.Add(id); else loadoutWeapons.Add(id);
                }
            }
            else { BeginMapRun(); CreateLandmarks(); }
        }

        public bool SelectStarter(int index)
        {
            if (!Selecting || index < 0 || index >= TotalWeaponCount) return false;
            if (mapLoadoutActive && !mapLoadout.Contains(WeaponUpgradeId(index))) return false;
            weaponLevels[(Weapon)index] = 1; Selecting = false;
            notice = "Etsi arkkuja ja pyhäkkö. Portaali avautuu 90 sekunnissa."; noticeUntil = Elapsed + 7;
            return true;
        }

        void ToggleLoadoutWeapon(int upgradeId)
        {
            if (loadoutWeapons.Contains(upgradeId)) loadoutWeapons.Remove(upgradeId);
            else if (loadoutWeapons.Count < MaxWeaponKinds) loadoutWeapons.Add(upgradeId);
        }

        void ToggleLoadoutTome(int id)
        {
            if (loadoutTomes.Contains(id)) loadoutTomes.Remove(id);
            else if (loadoutTomes.Count < MaxTomeKinds) loadoutTomes.Add(id);
        }

        void ConfirmLoadout()
        {
            mapLoadout.Clear();
            foreach (var id in loadoutWeapons) mapLoadout.Add(id);
            foreach (var id in loadoutTomes) mapLoadout.Add(id);
            mapLoadoutActive = mapLoadout.Count > 0; // an empty confirm behaves like Skip - never soft-lock the run on an empty pool
            awaitingLoadoutChoice = false;
            FinishLoadoutSetup();
        }

        void SkipLoadout()
        {
            mapLoadoutActive = false; mapLoadout.Clear();
            awaitingLoadoutChoice = false;
            FinishLoadoutSetup();
        }

        // Character selection deferred finishing Selecting=false for non-Pottu characters until
        // the loadout screen resolves (see SelectCharacter in SurvivorPottu.cs) - do it here.
        void FinishLoadoutSetup()
        {
            BeginMapRun(); CreateLandmarks();
            if (SelectedCharacter != PlayerCharacter.Pottu)
            {
                Selecting = false;
                notice = "Etsi arkkuja ja pyhäkkö. Portaali avautuu 90 sekunnissa."; noticeUntil = Elapsed + 7;
            }
        }

        void CreateLandmarks()
        {
            if (MapLayout != null)
            {
                var rooms = new List<int>();
                for(int i=1;i<MapLayout.Rooms.Count;i++) if(i!=MapLayout.BossRoom) rooms.Add(i);
                for(int i=0;i<3;i++) AddLandmark(0, MapLayout.Center(MapLayout.Rooms[rooms[i]]) + Vector3.up*.8f, eliteMaterial);
                AddLandmark(1, MapLayout.Center(MapLayout.Rooms[rooms[3]]) + Vector3.up, enemyMaterial);
                AddLandmark(2, MapLayout.BossPosition + Vector3.up*1.5f, xpMaterial);
                return;
            }
            float k = arenaRadius / 38f;
            AddLandmark(0, new Vector3(10 * k, .8f, 9 * k), eliteMaterial);
            AddLandmark(0, new Vector3(-18 * k, .8f, 12 * k), eliteMaterial);
            AddLandmark(0, new Vector3(20 * k, .8f, -15 * k), eliteMaterial);
            AddLandmark(1, new Vector3(-12 * k, 1, -12 * k), enemyMaterial);
            AddLandmark(2, new Vector3(0, 1.5f, 25 * k), xpMaterial);
        }

        void AddLandmark(int kind, Vector3 pos, Material material)
        {
            var t = Shape(kind == 0 ? "Arkku" : kind == 1 ? "Riskipyhäkkö" : "Bossiportaali", kind == 2 ? PrimitiveType.Cylinder : PrimitiveType.Cube,
                pos, kind == 2 ? new Vector3(3, 1.5f, 3) : new Vector3(1.5f, 1.5f, 1.5f), material, world);
            landmarks.Add(new Landmark { body = t, kind = kind });
        }

        static bool IsTomeId(int id) => (id >= 3 && id <= 9) || id == 16 || id == 17;
        static Weapon IdToWeapon(int id) => id < 3 ? (Weapon)id : id <= 15 ? (Weapon)(id - 7) : (Weapon)(id - 9);

        // Max 5 distinct weapons and 5 distinct Tomes per run; already-owned kinds can still level up past the cap.
        List<int> BuildUpgradePool()
        {
            var pool = new List<int>();
            for (int i = 0; i < UpgradeNames.Length; i++)
            {
                // Restrict to the chosen Map-mode loadout, but never block leveling up something
                // already owned (e.g. a character's fixed starting weapon picked outside the pool).
                if (mapLoadoutActive && !mapLoadout.Contains(i))
                {
                    bool owned = IsTomeId(i) ? tomeLevels.ContainsKey(i) : weaponLevels.ContainsKey(IdToWeapon(i));
                    if (!owned) continue;
                }
                if (i == 5 && Quantity >= 6) continue;
                if (i == 7 && CritChance >= .699f) continue;
                if (IsTomeId(i)) { if (!tomeLevels.ContainsKey(i) && tomeLevels.Count >= MaxTomeKinds) continue; }
                else { var w = IdToWeapon(i); if (!weaponLevels.ContainsKey(w) && weaponLevels.Count >= MaxWeaponKinds) continue; }
                pool.Add(i);
            }
            return pool;
        }

        void RollChoices()
        {
            choices.Clear(); var pool = BuildUpgradePool();
            for (int i = 0; i < 3; i++) { int n = Random.Range(0, pool.Count); choices.Add(pool[n]); pool.RemoveAt(n); }
        }

        public bool ChooseUpgrade(int slot)
        {
            if (Finished || Selecting || PendingChoices <= 0 || slot < 0 || slot >= choices.Count) return false;
            ApplyUpgrade(choices[slot]); PendingChoices--;
            if (PendingChoices > 0) RollChoices(); else choices.Clear();
            return true;
        }

        void ApplyUpgrade(int id)
        {
            if (id < 0 || id >= UpgradeNames.Length) return;
            if (id >= 18) { var w=(Weapon)(id-9); bool owned=WeaponLevel(w)>0; weaponLevels[w]=WeaponLevel(w)+1; if(owned) GrantWeaponBonus(); }
            else if (id >= 10 && id <= 15) { var w=(Weapon)(id-7); bool alreadyOwned=WeaponLevel(w)>0; weaponLevels[w]=WeaponLevel(w)+1; if (alreadyOwned) GrantWeaponBonus(); }
            else if (id < 3) { var w = (Weapon)id; bool alreadyOwned = weaponLevels.TryGetValue(w, out int level); weaponLevels[w] = alreadyOwned ? level + 1 : 1; if (alreadyOwned) GrantWeaponBonus(); }
            else
            {
                tomeLevels[id] = tomeLevels.TryGetValue(id, out int tomeLevel) ? tomeLevel + 1 : 1;
                switch (id)
                {
                    case 3: damage *= 1.01f; break; case 4: attackRate *= 1.01f; break;
                    case 5: Quantity = Mathf.Min(6, Quantity + 1); break; case 6: Size *= 1.01f; break;
                    case 7: CritChance = Mathf.Min(.7f, CritChance + .01f); break; case 8: Armor += 2; break; case 9: moveSpeed *= 1.01f; break; case 16: EffectDuration *= 1.01f; break; case 17: ProjectileSpeed *= 1.01f; break;
                }
            }
            notice = UpgradeNames[id] + " saatu!"; noticeUntil = Elapsed + 4;
        }

        // Re-picking an already-owned weapon always pays off: damage, size, or (with a bit of luck) both.
        void GrantWeaponBonus()
        {
            bool damageBoost = Random.value < .35f;
            bool sizeBoost = Random.value < .35f;
            if (!damageBoost && !sizeBoost) { if (Random.value < .5f) damageBoost = true; else sizeBoost = true; }
            if (damageBoost) damage *= 1.15f;
            if (sizeBoost) Size *= 1.15f;
        }

        void TickProgression(float dt)
        {
            for (int i = flashes.Count - 1; i >= 0; i--)
            { flashes[i].life -= dt; if (flashes[i].life <= 0) { Destroy(flashes[i].body.gameObject); flashes.RemoveAt(i); } }
            if (!bossTimerExpired && boss == null && Elapsed - areaStarted >= 90)
            {
                bossTimerExpired = true;
                notice = "Uusia vihollisia ei enää tule — tapa loput ja mene portaalille!"; noticeUntil = Elapsed + 6;
            }
            interactionHint = "";
            foreach (var p in landmarks)
            {
                if (p.used) continue;
                if (Vector3.Distance(player.position, p.body.position) > 3.5f) continue;
                interactionHint = LandmarkHint(p.kind);
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                    Interact(p);
                break;
            }
            if (boss != null)
            {
                bossAttackTimer -= dt;
                if (bossAttackTimer <= 0)
                {
                    bossAttackTimer = 2.2f;
                    if (Vector3.Distance(player.position, boss.body.position) < 5)
                    { ReceiveDamage(32); }
                    var ring = Shape("Boss shockwave", PrimitiveType.Cylinder, boss.body.position - Vector3.up * .7f, new Vector3(10,.04f,10), enemyMaterial, world);
                    flashes.Add(new Flash { body = ring, life = .3f });
                }
            }
        }

        string LandmarkHint(int kind)
        {
            if (kind == 0) return "[E] Arkku: 12 kultaa → satunnainen Tome tai asepäivitys";
            if (kind == 1) return "[E] Pyhäkkö: +30 % vahinko, vihollisten HP ja nopeus +20 %";
            if (BossDefeated) return "Seuraava kartta generoidaan...";
            if (boss != null) return "Voita bossi avataksesi portaalin.";
            return Elapsed - areaStarted < 90 ? "Portaali avautuu: " + Mathf.CeilToInt(90 - Elapsed + areaStarted) + " s" : "[E] Kutsu alueen bossi";
        }

        bool Interact(Landmark p)
        {
            if (p.used || Selecting || PendingChoices > 0 || ShopOpen || Finished || Vector3.Distance(player.position, p.body.position) > 3.5f) return false;
            if (p.kind == 0)
            {
                if (Coins < 12) return false;
                Coins -= 12; var pool = BuildUpgradePool(); ApplyUpgrade(pool[Random.Range(0, pool.Count)]); p.used = true;
            }
            else if (p.kind == 1)
            {
                damage *= 1.3f; curse *= 1.2f;
                foreach (var e in enemies) { e.health *= 1.2f; e.speed *= 1.2f; }
                p.used = true; notice = "Pyhäkkö aktivoitu: voima ja vaara kasvavat."; noticeUntil = Elapsed + 5;
            }
            else if (BossDefeated) { AdvanceArea(); return true; }
            else if (boss == null && Elapsed - areaStarted >= 90)
            {
                boss = new Enemy { body = Shape("Vartija " + Area, PrimitiveType.Capsule, ResolveMapPosition((MapLayout == null ? player.position : MapLayout.BossPosition + Vector3.up) + Vector3.forward * 8, 1.8f),
                    new Vector3(2.6f, 2.6f, 2.6f), eliteMaterial, world), health = 900 * Area * curse, speed = 3.2f * curse, elite = true };
                enemies.Add(boss); bossAttackTimer = 3;
                notice = "Kartan " + Area + " vartija ilmestyi! Voitto vie seuraavaan karttaan."; noticeUntil = Elapsed + 6;
            }
            else return false;
            if (p.used) p.body.localScale *= .35f;
            return true;
        }

        void AdvanceArea()
        {
            if (!MapTransitionPending || NetworkClientMode) return;
            ClearMapCombat();
            Area++; areaStarted = Elapsed; BossDefeated = false; bossTimerExpired = false; bossDefeatedAt = -1;
            if (activeMapType != MapType.Procedural) BuildFixedMap(activeMapType);
            else GenerateProceduralMap(NextMapSeed(CurrentMapSeed), true);
            CreateLandmarks(); player.position = MapSpawn + Vector3.up; Health = maxHealth;
            spawnTimer=1.5f; invulnerability=2f; dodgeLeft=dodgeCooldown=0; ShopOpen=false;
            UpdateCamera(true);
            SurvivorNetwork.Instance?.BroadcastMap();
            notice = "ALUE " + Area + " — viholliset vahvistuvat. Buildisi säilyy."; noticeUntil = Elapsed + 6;
        }

        void LoadHighscore() { BestSurvivalSeconds = PlayerPrefs.GetFloat(SaveKey + "BestSurvivalSeconds", 0f); }

        void FinishRun()
        {
            Finished = true; ShopOpen = false;
            newRecord = Elapsed > BestSurvivalSeconds;
            if (newRecord)
            {
                BestSurvivalSeconds = Elapsed;
                PlayerPrefs.SetFloat(SaveKey + "BestSurvivalSeconds", BestSurvivalSeconds); PlayerPrefs.Save();
            }
            GrantAccountAndMasteryXp();
            if (rewardPaid) return;
            EarnedSilver = Kills / 5 + bossKills * 20; Silver += EarnedSilver;
            PlayerPrefs.SetInt(SaveKey + "Silver", Silver); PlayerPrefs.Save(); rewardPaid = true;
        }

        public bool BuyLegacyHealth()
        {
            int cost = 20 + LegacyHealth * 10;
            if ((!Selecting && !Finished) || LegacyHealth >= 10 || Silver < cost) return false;
            Silver -= cost; LegacyHealth++;
            PlayerPrefs.SetInt(SaveKey + "Silver", Silver); PlayerPrefs.SetInt(SaveKey + "Health", LegacyHealth); PlayerPrefs.Save();
            if (Selecting) { maxHealth += 10; Health += 10; }
            return true;
        }

        void AttackWeapons()
        {
            Enemy nearest = null; float best = 24 * 24;
            foreach (var e in enemies) { float d = (e.body.position - player.position).sqrMagnitude; if (d < best) { best = d; nearest = e; } }
            if (nearest == null) return;
            Vector3 aim = (nearest.body.position - player.position).normalized;
            if (weaponLevels.TryGetValue(Weapon.Sword, out int sword))
            {
                float reach = 4 * Size * AffixMultiplier(Weapon.Sword, AffixStat.Size);
                bool twinblade = ActiveVariant(Weapon.Sword) == 1;
                float fullPower = damage * (1 + .25f * (sword - 1)) * (1 + .3f * (Quantity - 1))
                    * AffixMultiplier(Weapon.Sword, AffixStat.Damage) * AffixMultiplier(Weapon.Sword, AffixStat.Quantity);
                if (twinblade) fullPower *= VariantQualityMultiplier(Weapon.Sword);
                float mainPower = twinblade ? fullPower * .65f : fullPower;
                if (best < reach * reach) characterVisual.Swing(aim);
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    var e = enemies[i]; var delta = e.body.position - player.position;
                    if (delta.sqrMagnitude < reach * reach && Vector3.Dot(aim, delta.normalized) > -.25f)
                    { Hit(e, mainPower); SpawnImpact(e.body.position, Weapon.Sword, Size); }
                }
                if (best < reach * reach) { slashTimer = .16f; slash.rotation = Quaternion.LookRotation(aim); }
                // Kaksoisterä-variantti: toinen, pienempi isku laukeaa hetken päästä samaan suuntaan
                // sen sijaan että koko vahinko tulisi yhdellä leveämmällä sivalluksella - ks. AttackWeapons/SwordEchoStrike.
                if (twinblade && best < reach * reach) { swordEchoTimer = .12f; swordEchoAim = aim; swordEchoPower = fullPower * .55f; }
            }
            if (weaponLevels.TryGetValue(Weapon.Bow, out int bow))
            {
                characterVisual.Swing(aim);
                bool ricochet = ActiveVariant(Weapon.Bow) == 1;
                if (ricochet)
                {
                    // Kimmokaari-variantti: yksi vahvempi nuoli joka kimpoaa lähimpään uuteen viholliseen
                    // Quantity+1 kertaa suoran läpäisevän ammusrivistön sijaan - ks. UpdateBolts.
                    var t = Shape("Ricochet arrow", PrimitiveType.Cube, player.position, new Vector3(.15f,.15f,.95f) * Size, boltMaterial, world);
                    t.rotation = Quaternion.LookRotation(aim);
                    float ricochetPower = damage * (1 + .25f * (bow - 1)) * 1.3f * VariantQualityMultiplier(Weapon.Bow)
                        * AffixMultiplier(Weapon.Bow, AffixStat.Damage) * AffixMultiplier(Weapon.Bow, AffixStat.Crit);
                    int ricochetChain = 1 + Quantity + AffixCountBonus(Weapon.Bow, AffixStat.Quantity, Quantity);
                    bolts.Add(new Bolt { body = t, direction = aim, life = 1.6f, power = ricochetPower, chain = true, chainLeft = ricochetChain });
                }
                else
                {
                    int count = Quantity + AffixCountBonus(Weapon.Bow, AffixStat.Quantity, Quantity);
                    float bowPower = damage * (1 + .25f * (bow - 1)) * AffixMultiplier(Weapon.Bow, AffixStat.Damage) * AffixMultiplier(Weapon.Bow, AffixStat.Crit);
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 direction = Quaternion.Euler(0, (i - (count - 1) / 2f) * 9, 0) * aim;
                        var t = Shape("Piercing arrow", PrimitiveType.Cube, player.position, new Vector3(.13f,.13f,.85f) * Size, boltMaterial, world);
                        t.rotation = Quaternion.LookRotation(direction);
                        bolts.Add(new Bolt { body = t, direction = direction, life = 1.3f, power = bowPower });
                    }
                }
            }
            if (weaponLevels.TryGetValue(Weapon.Lightning, out int lightning))
            {
                var candidates = new List<Enemy>(enemies); Vector3 origin = player.position;
                int chainCount = Quantity + AffixCountBonus(Weapon.Lightning, AffixStat.Quantity, Quantity)
                    + AffixCountBonus(Weapon.Lightning, AffixStat.Bounces, Quantity);
                float lightningPower = damage * 1.1f * AffixMultiplier(Weapon.Lightning, AffixStat.Damage);
                for (int i = 0; i < chainCount + 1; i++)
                {
                    Enemy target = null; float distance = (i == 0 ? 16 : 8); distance *= distance;
                    foreach (var e in candidates) { float sq = (e.body.position - origin).sqrMagnitude; if (sq < distance) { distance = sq; target = e; } }
                    if (target == null) break;
                    var end = target.body.position;
                    if (lightningZapEffect) Destroy(Instantiate(lightningZapEffect, end, Quaternion.identity, MapEffectRoot), 2f);
                    candidates.Remove(target); Hit(target, lightningPower * (1 + .25f * (lightning - 1))); origin = end;
                }
            }
        }

        void DrawProgression()
        {
            if (!Selecting && PendingChoices == 0 && !Finished && !ShopOpen)
            {
                string weapons = LoadoutSummary();
                GUI.Label(new Rect(24,192,790,60), "ALUE " + Area + "  •  Aseet " + WeaponCount + "/5  •  Tomet " + TomeCount + "/5  •  " + weapons, textStyle);
                GUI.Label(new Rect(24,254,900,30), "Arkut: keltainen  •  Pyhäkkö: punainen  •  Bossiportaali: sininen kartalla", textStyle);
                DrawDungeonMap();
                GUI.Label(new Rect(360,590,850,55), interactionHint, textStyle);
                if (boss != null) GUI.Label(new Rect(440,80,600,35), "ALUEEN VARTIJA  •  HP " + Mathf.CeilToInt(boss.health), titleStyle);
                else GUI.Label(new Rect(850,190,410,55), BossDefeated ? "Uusi kartta generoidaan..." : "Bossiportaali: " + Mathf.Max(0, Mathf.CeilToInt(90 - Elapsed + areaStarted)) + " s", textStyle);
            }
            if (!Selecting && PendingChoices == 0) return;
            GUI.color = new Color(0,0,0,.93f); GUI.DrawTexture(new Rect(0,0,1280,720), Texture2D.whiteTexture); GUI.color = Color.white;
            if (Selecting)
            {
                if (awaitingCharacterChoice) DrawCharacterSelect();
                else if (awaitingLoadoutChoice) DrawLoadoutSelect();
                else if (SelectedCharacter == PlayerCharacter.Pottu) DrawStarterPages();
                return;
            }
            GUI.Label(new Rect(90,90,1100,55), Selecting ? "VALITSE ALOITUSASE" : "TASO " + Level + " / VALITSE PÄIVITYS", titleStyle);
            GUI.Label(new Rect(90,150,1100,55), Selecting ? "Aloita yhdellä aseella. Kerää XP:tä, kokoa buildi ja voita alueen bossi." : "Peli on tauolla. Valitse yksi kolmesta. Odottavia valintoja: " + PendingChoices, textStyle);
            for (int i = 0; i < 3; i++)
            {
                int id = Selecting ? i : choices[i];
                GUI.Box(new Rect(90+i*370,230,350,250), GUIContent.none);
                GUI.Label(new Rect(110+i*370,250,310,45), UpgradeNames[id], textStyle);
                GUI.Label(new Rect(110+i*370,310,310,95), UpgradeDetails[id], textStyle);
                if (GUI.Button(new Rect(110+i*370,420,310,42), "Valitse", buttonStyle))
                { if (Selecting) SelectStarter(i); else ChooseUpgrade(i); break; }
            }
            if (Selecting) DrawLegacy(90, 540);
        }

        // Interim Map-mode loadout picker: free choice from the whole existing roster (no
        // weapon/Tome variant or fragment unlock system exists yet to gate this against). Only
        // shown for a real fixed-map run (awaitingLoadoutChoice, set in ResetProgression); Survival
        // never sees this screen and keeps its always-full random pool.
        void DrawLoadoutSelect()
        {
            var ownedWeapons = new List<int>();
            var ownedTomes = new List<int>();
            foreach (var id in OwnedUpgradesFor(SelectedCharacter)) { if (IsTomeId(id)) ownedTomes.Add(id); else ownedWeapons.Add(id); }
            ownedWeapons.Sort(); ownedTomes.Sort();

            GUI.Label(new Rect(40,50,1200,40), "MAPPI-TILAN LOADOUT", titleStyle);
            GUI.Label(new Rect(40,92,1200,40), "Väliaikainen versio: valitse vapaasti enintään 5 asetta ja 5 Tomea " + CharacterRosterName(SelectedCharacter) + "n omistamista. Osta lisää HAHMOT-tabista päävalikossa.", textStyle);
            GUI.Label(new Rect(40,140,600,26), "ASEET  (" + loadoutWeapons.Count + "/" + MaxWeaponKinds + ")  •  Laatumateriaali: " + QualityMaterial + " kpl", textStyle);
            const int cols = 6; const float bw = 195, bh = 34, gx = 8, gy = 6, x0 = 40; const float y0 = 168;
            // variantRowH varaa tilan aseen vierestä avattavalle muunnostoggle-napille (ei omaa
            // loadout-paikkaa - sama upgradeId, vain käytöslippu activeVariantByCharacterissa).
            // qualityRowH varaa tilan laatutason parannusnapille (tähdet näkyvät jo togglenapin
            // tekstissä) - varattu aina, vaikka nappia ei näytettäisi (laatu jo 3 tai ei materiaalia).
            const float variantRowH = 20, qualityRowH = 20; const float rowPitch = bh + variantRowH + qualityRowH + gy;
            for (int i = 0; i < ownedWeapons.Count; i++)
            {
                int col = i % cols, row = i / cols;
                int upgradeId = ownedWeapons[i];
                var weapon = IdToWeapon(upgradeId);
                bool selected = loadoutWeapons.Contains(upgradeId);
                float bx = x0+col*(bw+gx), by = y0+row*rowPitch;
                GUI.color = selected ? new Color(.3f,.85f,1) : Color.white;
                if (GUI.Button(new Rect(bx, by, bw, bh), WeaponLabel(weapon), cardTextStyle)) ToggleLoadoutWeapon(upgradeId);
                if (IsVariantUnlocked(weapon, 1))
                {
                    bool variantOn = ActiveVariantFor(SelectedCharacter, weapon) == 1;
                    int quality = VariantQuality(weapon, 1);
                    string stars = new string('★', quality) + new string('☆', 3 - quality);
                    GUI.color = variantOn ? new Color(1f,.75f,.25f) : new Color(.55f,.55f,.55f);
                    if (GUI.Button(new Rect(bx, by+bh+2, bw, variantRowH-2), VariantName(weapon,1) + " " + stars + (variantOn ? " (PÄÄLLÄ)" : ""), cardTextStyle))
                        SetActiveVariant(SelectedCharacter, weapon, variantOn ? 0 : 1);
                    if (quality < 3)
                    {
                        int cost = VariantQualityUpgradeCost(weapon, 1);
                        GUI.color = Color.white; GUI.enabled = QualityMaterial >= cost;
                        if (GUI.Button(new Rect(bx, by+bh+variantRowH+4, bw, qualityRowH-2), "Paranna (" + cost + " kpl)", cardTextStyle))
                            UpgradeVariantQuality(SelectedCharacter, weapon, 1);
                        GUI.enabled = true;
                    }
                }
                GUI.color = Color.white;
            }
            GUI.color = Color.white;
            int weaponRows = Mathf.Max(1, Mathf.CeilToInt(ownedWeapons.Count / (float)cols));
            float tomesY = y0 + weaponRows*rowPitch + 18;
            GUI.Label(new Rect(40,tomesY,600,26), "TOMET  (" + loadoutTomes.Count + "/" + MaxTomeKinds + ")", textStyle);
            const int tcols = 3; const float tbw = 390, tbh = 34;
            for (int i = 0; i < ownedTomes.Count; i++)
            {
                int col = i % tcols, row = i / tcols;
                int id = ownedTomes[i];
                bool selected = loadoutTomes.Contains(id);
                GUI.color = selected ? new Color(.3f,.85f,1) : Color.white;
                if (GUI.Button(new Rect(x0+col*(tbw+gx), tomesY+30+row*(tbh+gy), tbw, tbh), UpgradeNames[id], cardTextStyle)) ToggleLoadoutTome(id);
            }
            GUI.color = Color.white;
            int tomeRows = Mathf.Max(1, Mathf.CeilToInt(ownedTomes.Count / (float)tcols));
            float buttonY = tomesY + 30 + tomeRows*(tbh+gy) + 14;
            bool hasSelection = loadoutWeapons.Count > 0 || loadoutTomes.Count > 0;
            if (GUI.Button(new Rect(x0,buttonY,360,44), hasSelection ? "VAHVISTA VALINTA" : "VAHVISTA (valitse jotain ensin)", buttonStyle) && hasSelection) ConfirmLoadout();
            if (GUI.Button(new Rect(x0+390,buttonY,360,44), "OHITA (käytä koko omistetusta poolista)", buttonStyle)) SkipLoadout();
        }

        void DrawLegacy(float x, float y)
        {
            GUI.Label(new Rect(x,y,900,30), "SILVER " + Silver + "  •  Account Lv. " + AccountLevel + "  •  Pysyvä kestävyys " + LegacyHealth + "/10 (+" + LegacyHealth * 10 + " HP)", textStyle);
            GUI.enabled = Silver >= 20 + LegacyHealth * 10 && LegacyHealth < 10;
            if (GUI.Button(new Rect(x,y+38,510,44), "+10 pysyvää HP:tä / " + (20 + LegacyHealth * 10) + " Silver", buttonStyle)) BuyLegacyHealth();
            GUI.enabled = true;
        }
    }
}

