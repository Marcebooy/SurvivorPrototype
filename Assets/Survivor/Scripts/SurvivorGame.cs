using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BonkSurvivor
{
    // Self-contained first playable. Only runs in the dedicated prototype scene.
    public sealed partial class SurvivorGame : MonoBehaviour
    {
        [Header("Enemy model")]
        public GameObject undeadEnemyPrefab;
        public GameObject dropPrefab; // networked XP/gold/health pickup - see DropNet.cs
        [Header("VFX")]
        public GameObject lightningZapEffect;
        public GameObject auraEffectPrefab;
        public GameObject slashEffect;        // FX_Orange_Slash_1 - terä-/lähitaisteluaseet
        public GameObject fireImpactEffect;   // FX_Fireball - räjähtävät/tuliaseet
        public GameObject poisonImpactEffect; // FX_Green_Hit - myrkky
        public GameObject frostImpactEffect;  // VFX_Zap_06_White - jää
        public GameObject voidImpactEffect;   // VFX_Zap_07_Black - pimeys/tyhjiö
        public GameObject boltImpactEffect;   // VFX_Zap_05_Purple - yleinen taika-/ammusosuma
        [Header("Starting balance")]
        // These per-player stats now live on `Combatant` (see Combatant.cs) so a connected friend
        // can have their own independent copy; these properties forward to whichever Combatant is
        // "current" (always hostState during the host's own code, briefly swapped to a friend's
        // Combatant while TickNetworkCoop runs their weapons/progression - see SurvivorGameNetwork.cs).
        public float moveSpeed { get => current.MoveSpeed; set => current.MoveSpeed = value; }
        public float damage { get => current.Damage; set => current.Damage = value; }
        public float attackRate { get => current.AttackRate; set => current.AttackRate = value; }
        public float pickupRadius { get => current.PickupRadius; set => current.PickupRadius = value; }
        // Runs now end on death; portals advance between areas.
        public float arenaRadius = 60f;
        public float maxHealth { get => current.MaxHealth; set => current.MaxHealth = value; }
        public float Health { get => current.Health; private set => current.Health = value; }
        public int Level { get => current.Level; private set => current.Level = value; }
        public int Coins { get => current.Coins; private set => current.Coins = value; }
        public int Kills { get => current.Kills; private set => current.Kills = value; }
        public int Experience { get => current.Experience; private set => current.Experience = value; }
        public int NextLevel => 8 + (Level - 1) * 5;
        readonly Combatant hostState = new Combatant();
        Combatant current;
        void Awake() { current = hostState; }
        public float Elapsed { get; private set; }
        public bool ShopOpen { get; private set; }
        public bool Finished { get; private set; }
        public bool AtMainMenu { get; private set; } = true;
        public int EnemyCount => enemies.Count;
        public Vector3 PlayerPosition => player.position;
        readonly List<Enemy> enemies = new List<Enemy>();
        readonly List<Drop> drops = new List<Drop>();
        readonly List<Bolt> bolts = new List<Bolt>();
        readonly List<Material> materials = new List<Material>();
        readonly int[] purchases = new int[5];
        Transform player => current.Body;
        Transform world, slash;
        Camera followCamera;
        Material enemyMaterial, eliteMaterial, xpMaterial, boltMaterial, healthMaterial;
        float spawnTimer, attackTimer, invulnerability, slashTimer;
        string notice = "Selviä viisi minuuttia!";
        float noticeUntil = 5f;
        GUIStyle titleStyle, textStyle, buttonStyle, centerTitleStyle, centerTextStyle, centerButtonStyle, hudNameStyle, cardTextStyle;
        bool settingsOpen;
        int fpsCap = 60;
        float masterVolume = 1f;
        string joinCodeInputField = "";
        internal sealed class Enemy { public Transform body; public float health, speed; public bool elite; public Animator animator; }
        sealed class Drop { public Transform body; public int value; public bool isHealth; }
        sealed class Bolt { public Transform body; public Vector3 direction; public float life, power; public HashSet<Enemy> hit = new HashSet<Enemy>(); }

        void Start() { LoadHighscore(); LoadSettings(); BuildWorld(); }

        void LoadSettings()
        {
            fpsCap = PlayerPrefs.GetInt(SaveKey + "FpsCap", 60);
            masterVolume = PlayerPrefs.GetFloat(SaveKey + "MasterVolume", 1f);
            ApplyFpsCap(fpsCap);
            AudioListener.volume = masterVolume;
        }

        void ApplyFpsCap(int fps)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fps;
        }

        void SetFpsCap(int fps)
        {
            fpsCap = fps; ApplyFpsCap(fps);
            PlayerPrefs.SetInt(SaveKey + "FpsCap", fps); PlayerPrefs.Save();
        }

        void SetVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume); AudioListener.volume = masterVolume;
            PlayerPrefs.SetFloat(SaveKey + "MasterVolume", masterVolume); PlayerPrefs.Save();
        }

        void StartGame() { if(!string.IsNullOrWhiteSpace(mapSeedInput) && !int.TryParse(mapSeedInput,out _)) return; AtMainMenu = false; ResetRun(); }
        void ExitToMainMenu() { FinishRun(); AtMainMenu = true; ShopOpen = false; }

        static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        Material MakeMaterial(Color color)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color = color; materials.Add(m); return m;
        }

        Transform Shape(string label, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var go = GameObject.CreatePrimitive(type); go.name = label;
            go.transform.SetParent(parent); go.transform.position = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(go.GetComponent<Collider>());
            return go.transform;
        }

        void BuildWorld()
        {
            world = new GameObject("Runtime arena").transform; world.SetParent(transform);
            enemyMaterial = MakeMaterial(new Color(.95f, .28f, .28f));
            eliteMaterial = MakeMaterial(new Color(1f, .6f, .16f));
            xpMaterial = MakeMaterial(new Color(.3f, .8f, 1f));
            healthMaterial = MakeMaterial(new Color(1f, .25f, .4f));
            boltMaterial = MakeMaterial(new Color(1f, .9f, .35f));
            GenerateProceduralMap(81427, false);
            BuildPottu();
            slash = Shape("Auto melee pulse", PrimitiveType.Cylinder, Vector3.zero, new Vector3(6, .035f, 6), boltMaterial, world);
            BuildSwipe(); slash.gameObject.SetActive(false);
            followCamera = new GameObject("Survivor Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            followCamera.transform.SetParent(world); followCamera.tag = "MainCamera";
            followCamera.fieldOfView = 52; followCamera.farClipPlane = 180;
            followCamera.clearFlags = CameraClearFlags.SolidColor; followCamera.backgroundColor = new Color(.035f,.06f,.1f);
            var sun = new GameObject("Sun", typeof(Light)).GetComponent<Light>();
            sun.transform.SetParent(world); sun.type = LightType.Directional; sun.intensity = 1.6f;
            sun.transform.rotation = Quaternion.Euler(50, -35, 0); sun.shadows = LightShadows.Soft;
            RenderSettings.ambientLight = new Color(.45f, .55f, .65f);
            UpdateCamera(true);
        }

        public void ResetRun()
        {
            if (NetworkClientMode) return;
            ClearMapCombat();
            foreach (var e in enemies) Destroy(e.body.gameObject);
            foreach (var d in drops) Destroy(d.body.gameObject);
            foreach (var b in bolts) Destroy(b.body.gameObject);
            enemies.Clear(); drops.Clear(); bolts.Clear();
            System.Array.Clear(purchases, 0, purchases.Length);
            moveSpeed = 8; damage = 18; attackRate = 1.3f; pickupRadius = 3.5f; maxHealth = 100;
            Health = maxHealth; Level = 1; Coins = Kills = Experience = 0; Elapsed = 0;
            ShopOpen = Finished = false; spawnTimer = attackTimer = invulnerability = slashTimer = 0;
            player.position = Vector3.up; slash.gameObject.SetActive(false);
            ResetProgression(); ResetPottu(); player.position = MapSpawn + Vector3.up;
            notice = "Valitse aloitusase"; noticeUntil = 5; UpdateCamera(true);
            SurvivorNetwork.Instance?.BroadcastMap();
        }

        void Update()
        {
            if (NetworkClientMode) { TickNetworkClient(Time.deltaTime); return; }
            var k = Keyboard.current;
            if (AtMainMenu)
            {
                if (k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame)) StartGame();
                return;
            }
            if (k != null && (k.tabKey.wasPressedThisFrame || k.escapeKey.wasPressedThisFrame) && !Finished && !Selecting && PendingChoices == 0) ShopOpen = !ShopOpen;
            if (Finished)
            {
                if (k != null)
                {
                    if (k.rKey.wasPressedThisFrame) ResetRun();
                    if (k.mKey.wasPressedThisFrame) AtMainMenu = true;
                }
                return;
            }
            if (MapTransitionPending) { TickMapTransition(Time.deltaTime); return; }
            if (ShopOpen || Selecting || PendingChoices > 0) return;
            float dt = Time.deltaTime; Elapsed += dt;
            TickNetworkCoop(dt);

            Vector2 input = Vector2.zero;
            if (k != null)
            {
                input.x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
                input.y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
            }
            if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > .05f) input = Gamepad.current.leftStick.ReadValue();
            input = Vector2.ClampMagnitude(input, 1);
            var move = new Vector3(input.x, 0, input.y);
            bool dodgePressed = (k != null && k.spaceKey.wasPressedThisFrame) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
            MovePottu(move, dt, dodgePressed);
            TickProgression(dt); if (Finished) return;
            invulnerability -= dt;
            spawnTimer -= dt;
            int enemyCap = 260 + (Area - 1) * 30;
            if (boss == null && Elapsed - areaStarted < 90 && spawnTimer <= 0 && enemies.Count < enemyCap)
            {
                SpawnEnemy(); spawnTimer = Mathf.Max(.09f, .8f - Elapsed / 200f - (Area - 1) * .05f);
            }
            MapLayout?.UpdateNavigation(player.position);
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if(i>=enemies.Count) continue;
                var e = enemies[i];
                var targetPos = NearestCombatantPosition(e.body.position);
                var delta = targetPos - e.body.position; delta.y = 0;
                float bodyRadius = e == boss ? 1.3f : .65f;
                var direction = MapLayout == null ? delta.normalized : MapLayout.PursuitDirection(e.body.position, targetPos, bodyRadius);
                e.body.position = MoveOnMap(e.body.position, direction * (e.speed * EnemyMoveMultiplier(e) * dt), bodyRadius);
                if (delta.sqrMagnitude > .01f) e.body.rotation = Quaternion.Slerp(e.body.rotation, Quaternion.LookRotation(delta.normalized), dt * 10);
                if (e.animator) e.animator.SetFloat("Speed", e.speed);
                var hostDelta = player.position - e.body.position; hostDelta.y = 0;
                if (hostDelta.sqrMagnitude < 1.7f && invulnerability <= 0)
                {
                    ReceiveDamage(e.elite ? 26 : 13);
                    if(!enemies.Contains(e)) continue;
                    if (e.animator) e.animator.SetTrigger("Attack");
                    e.body.position = MoveOnMap(e.body.position, -hostDelta.normalized * 1.5f, bodyRadius);
                    if (Health <= 0) { FinishRun(); return; }
                }
            }
            attackTimer -= dt;
            if (attackTimer <= 0) { AttackWeapons(); attackTimer = 1 / attackRate; }
            slashTimer -= dt;
            slash.gameObject.SetActive(slashTimer > 0);
            slash.position = new Vector3(player.position.x, .15f, player.position.z);
            slash.localScale = new Vector3(8 * Size, .025f, 8 * Size) * (1 - Mathf.Clamp01(slashTimer / .16f) * .3f);
            TickArsenal(dt); UpdateBolts(dt);
            for (int i = drops.Count - 1; i >= 0; i--)
            {
                var d = drops[i]; float distance = Vector3.Distance(d.body.position, player.position);
                d.body.Rotate(0, 100 * dt, 0);
                if (distance < pickupRadius) d.body.position = Vector3.MoveTowards(d.body.position, player.position, 15 * dt);
                if (distance < 1.1f)
                {
                    if (d.isHealth) { Health = Mathf.Min(maxHealth, Health + d.value); notice = "+" + d.value + " HP"; noticeUntil = Elapsed + 2; }
                    else { GrantExperience(d.value); Coins += d.value; }
                    Destroy(d.body.gameObject); drops.RemoveAt(i);
                }
            }
        }

        void LateUpdate() { if (NetworkClientMode) return; if (followCamera) UpdateCamera(false); }
        void UpdateCamera(bool snap)
        {
            var target = player.position + new Vector3(0, 23, -19);
            followCamera.transform.position = snap ? target : Vector3.Lerp(followCamera.transform.position, target, 1 - Mathf.Exp(-8 * Time.deltaTime));
            followCamera.transform.rotation = Quaternion.Euler(48, 0, 0);
        }

        void SpawnEnemy()
        {
            var offset = Random.insideUnitCircle.normalized * Random.Range(18f, 25f);
            var p = player.position + new Vector3(offset.x, 0, offset.y); p.y = 0;
            p = ResolveMapPosition(p, 1f); p.y = 0;
            // Near a corner an outward spawn can project back onto the player.
            for (int attempt = 0; attempt < 12 && (p - player.position).sqrMagnitude < 16f * 16f; attempt++)
            {
                offset = Random.insideUnitCircle.normalized * Random.Range(18f, 25f);
                var candidate = ResolveMapPosition(player.position + new Vector3(offset.x, 0, offset.y), 1f);
                candidate.y = 0;
                if ((candidate - player.position).sqrMagnitude > (p - player.position).sqrMagnitude) p = candidate;
            }
            bool elite = Elapsed > 20 && Random.value < .24f;
            Transform body; Animator animator = null;
            if (undeadEnemyPrefab)
            {
                var instance = Instantiate(undeadEnemyPrefab, p, Quaternion.identity, world);
                instance.name = elite ? "Brute" : "Chaser";
                instance.transform.localScale = Vector3.one * (elite ? 2.8f : 2f);
                animator = instance.GetComponent<Animator>();
                if (animator) animator.applyRootMotion = false;
                if (elite)
                {
                    var renderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (renderer)
                    {
                        var tint = new MaterialPropertyBlock();
                        tint.SetColor("_BaseColor", new Color(1f, .5f, .15f));
                        renderer.SetPropertyBlock(tint);
                    }
                }
                body = instance.transform;
                if (IsNetworkHost)
                {
                    var netObj = instance.GetComponent<NetworkObject>();
                    if (netObj) netObj.Spawn();
                }
            }
            else body = Shape(elite ? "Brute" : "Chaser", PrimitiveType.Capsule, p + Vector3.up, Vector3.one * (elite ? 1.3f : .8f), elite ? eliteMaterial : enemyMaterial, world);
            enemies.Add(new Enemy {
                body = body, animator = animator,
                health = (elite ? 130 : 34) * (1 + Elapsed / 200) * curse * (1 + .45f * (Area - 1)), speed = ((elite ? 3.4f : 4.6f) + Mathf.Min(3, Elapsed / 70)) * curse, elite = elite
            });
        }

        void UpdateBolts(float dt)
        {
            for (int i = bolts.Count - 1; i >= 0; i--)
            {
                var b = bolts[i]; var start = b.body.position; var end = start + b.direction * (24 * ProjectileSpeed * dt);
                b.life -= dt; b.body.position = end;
                foreach (var e in new List<Enemy>(enemies))
                {
                    var segment = end - start;
                    float t = Mathf.Clamp01(Vector3.Dot(e.body.position - start, segment) / Mathf.Max(.0001f, segment.sqrMagnitude));
                    if (!b.hit.Contains(e) && (e.body.position - (start + segment * t)).sqrMagnitude < (e.elite ? 1.2f : .7f) * Size * Size)
                    { b.hit.Add(e); Hit(e, b.power); SpawnImpact(e.body.position, Weapon.Bow, Size); }
                }
                if (b.life <= 0) { Destroy(b.body.gameObject); bolts.RemoveAt(i); }
            }
        }

        void Hit(Enemy e, float amount)
        {
            bool critical = Random.value < CritChance; if (critical) amount *= 2; HitFeedback(e, amount, critical); e.health -= amount;
            if (e.health > 0) { e.body.position = MoveOnMap(e.body.position, (e.body.position - player.position).normalized * .6f, e == boss ? 1.3f : .65f); return; }
            AdvancedKill(e);
            if (e == boss) { boss = null; BossDefeated = true; bossDefeatedAt = Elapsed; bossKills++; BeginMapTransition(); }
            int value = e.elite ? 5 : 1;
            // Merge nearby drops when the arena is crowded, preserving all XP and coins.
            if (drops.Count >= 250) drops[0].value += value;
            else drops.Add(new Drop { body = SpawnDrop(new Vector3(e.body.position.x, .5f, e.body.position.z), Vector3.one * .4f, PrimitiveType.Cube, xpMaterial, false), value = value });
            if (Random.value < .01f)
                drops.Add(new Drop { body = SpawnDrop(new Vector3(e.body.position.x, .5f, e.body.position.z), Vector3.one * .45f, PrimitiveType.Sphere, healthMaterial, true), value = 25, isHealth = true });
            enemies.Remove(e); Destroy(e.body.gameObject); Kills++;
        }

        // Networked when hosting a multiplayer session (so a connected friend can see loot on the
        // ground too - DropNet.cs), otherwise the same plain primitive as always.
        Transform SpawnDrop(Vector3 pos, Vector3 scale, PrimitiveType shape, Material material, bool isHealth)
        {
            // Only networked when actually hosting - DropNet's color depends on OnNetworkSpawn
            // running, so offline this stays the plain Shape() primitive exactly as before.
            if (dropPrefab && IsNetworkHost)
            {
                var instance = Instantiate(dropPrefab, pos, Quaternion.identity, world);
                instance.transform.localScale = scale;
                var dn = instance.GetComponent<DropNet>();
                if (dn) dn.IsHealth.Value = isHealth;
                var netObj = instance.GetComponent<NetworkObject>();
                if (netObj) netObj.Spawn();
                return instance.transform;
            }
            return Shape(isHealth ? "Health pickup" : "XP + gold", shape, pos, scale, material, world);
        }

        public void GrantExperience(int amount)
        {
            if (amount <= 0 || Selecting || Finished || boss != null) return;
            float xpFalloff = Mathf.Max(.35f, 1f - (Level - 1) * .05f);
            Experience += Mathf.Max(1, Mathf.RoundToInt(amount * xpFalloff));
            while (Experience >= NextLevel)
            {
                Experience -= NextLevel; Level++; PendingChoices++;
                if (choices.Count == 0) RollChoices();
            }
        }

        public int UpgradeCost(int index) => index < 0 || index >= 5 ? int.MaxValue : (index == 4 ? 10 : 8) + purchases[index] * 6;
        public bool BuyUpgrade(int index)
        {
            if (!ShopOpen || Finished || Selecting || PendingChoices > 0 || index < 0 || index >= 5 || Coins < UpgradeCost(index) || (index == 4 && Health >= maxHealth)) return false;
            Coins -= UpgradeCost(index); purchases[index]++;
            switch (index) { case 0: attackRate += .25f; break; case 1: damage += 7; break; case 2: moveSpeed += .7f; break; case 3: pickupRadius += 1.2f; break; case 4: Health = Mathf.Min(maxHealth, Health + 45); break; }
            return true;
        }

        void OnGUI()
        {
            if (!player) return;
            GUI.color = Color.white; GUI.contentColor = Color.white; GUI.backgroundColor = Color.white;
            if (titleStyle == null || titleStyle.fontSize != 28)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
                textStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 17, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(18,12,10,10) };
                centerTitleStyle = new GUIStyle(titleStyle) { alignment = TextAnchor.MiddleCenter };
                centerTextStyle = new GUIStyle(textStyle) { alignment = TextAnchor.MiddleCenter };
                centerButtonStyle = new GUIStyle(buttonStyle) { alignment = TextAnchor.MiddleCenter };
                hudNameStyle = new GUIStyle(titleStyle) { fontSize = 21 };
                cardTextStyle = new GUIStyle(textStyle) { fontSize = 13 };
            }
            titleStyle.normal.textColor = Color.white; textStyle.normal.textColor = Color.white;
            buttonStyle.normal.textColor = Color.white; buttonStyle.hover.textColor = Color.white;
            centerTitleStyle.normal.textColor = Color.white; centerTextStyle.normal.textColor = Color.white;
            centerButtonStyle.normal.textColor = Color.white; centerButtonStyle.hover.textColor = Color.white;
            hudNameStyle.normal.textColor = Color.white; cardTextStyle.normal.textColor = Color.white;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
            if (NetworkClientMode) { DrawNetworkClientHud(); return; }
            if (AtMainMenu) { DrawMainMenu(); return; }
            bool overlayOpen = ShopOpen || Finished || Selecting || PendingChoices > 0;
            if (!overlayOpen)
            {
                GUI.Box(new Rect(20,20,330,144), GUIContent.none);
                GUI.Label(new Rect(36,28,310,40), CharacterTitle(SelectedCharacter), hudNameStyle);
                GUI.Label(new Rect(36,72,300,30), "TASO " + Level + "   •   " + Coins + " kultaa   •   " + Kills + " kaatoa", textStyle);
                Bar(new Rect(36,108,290,16), Health / maxHealth, new Color(.2f,.9f,.7f));
                GUI.Label(new Rect(36,128,290,28), "HP " + Mathf.CeilToInt(Health) + "/" + maxHealth + "    XP " + Experience + "/" + NextLevel, textStyle);
                Bar(new Rect(20,174,330,6), (float)Experience / NextLevel, new Color(.35f,.65f,1));
                GUI.Label(new Rect(1070,28,200,40), Mathf.FloorToInt(Elapsed / 60).ToString("00") + ":" + Mathf.FloorToInt(Elapsed % 60).ToString("00") + " / RUN", titleStyle);
                GUI.Label(new Rect(24,664,1000,35), "WASD  Liiku   •   SPACE  Kierähdä   •   E  Tutki   •   TAB  Kauppa", textStyle);
                if (GUI.Button(new Rect(1070,650,185,44), "Kauppa [TAB]", buttonStyle) && !Finished && !Selecting && PendingChoices == 0) ShopOpen = !ShopOpen;
                if (Elapsed < noticeUntil) GUI.Label(new Rect(370,28,690,65), notice, textStyle);
            }
            if ((ShopOpen || Finished) && !Selecting && PendingChoices == 0)
            {
                GUI.color = new Color(0,0,0,.85f); GUI.DrawTexture(new Rect(0,0,1280,720), Texture2D.whiteTexture); GUI.color = Color.white;
                GUI.Box(new Rect(350,110,580,510), GUIContent.none);
                GUI.Label(new Rect(380,132,520,45), Finished ? (Health > 0 ? "SELVIYDYIT!" : "KIERROS PÄÄTTYI") : "KAUPPA  /  " + Coins + " KULTAA", titleStyle);
                if (Finished)
                {
                    GUI.Label(new Rect(380,205,500,120),
                        "Taso " + Level + "  •  " + Kills + " kaatoa\n" +
                        "Selviytymisaika " + Mathf.FloorToInt(Elapsed) + " sekuntia" + (newRecord ? "  —  UUSI ENNÄTYS!" : "") + "\n" +
                        "Paras aika " + FormatTime(BestSurvivalSeconds), textStyle);
                    GUI.Label(new Rect(380,330,520,30), "+" + EarnedSilver + " Silver tältä kierrokselta", textStyle);
                    if (GUI.Button(new Rect(380,380,255,50), "Uusi kierros [R]", buttonStyle)) ResetRun();
                    if (GUI.Button(new Rect(645,380,255,50), "Päävalikko [M]", buttonStyle)) AtMainMenu = true;
                    DrawLegacy(380, 460);
                }
                else
                {
                    GUI.Label(new Rect(380,183,520,50), "Peli on tauolla. Parannukset ovat voimassa tämän kierroksen.", textStyle);
                    string[] labels = { "Hyökkäysnopeus  +0.25 iskua/s", "Vahinko  +7", "Liikenopeus  +0.7", "Keräyssäde  +1.2 m", "Parannus  +45 HP" };
                    for (int i = 0; i < 5; i++)
                    {
                        GUI.enabled = Coins >= UpgradeCost(i) && (i != 4 || Health < maxHealth);
                        if (GUI.Button(new Rect(380,243+i*53,520,45), labels[i] + "     " + UpgradeCost(i) + " kultaa", buttonStyle)) BuyUpgrade(i);
                    }
                    GUI.enabled = true;
                    GUI.Label(new Rect(380,513,520,34), "Vahinko " + damage + "  •  " + attackRate.ToString("0.00") + " iskua/s  •  Nopeus " + moveSpeed.ToString("0.0"), textStyle);
                    if (GUI.Button(new Rect(380,558,250,42), "Jatka [TAB]", buttonStyle)) ShopOpen = false;
                    if (GUI.Button(new Rect(650,558,250,42), "Poistu päävalikkoon", buttonStyle)) ExitToMainMenu();
                }
            }
            DrawPottuHUD();
            if (!Finished) DrawProgression();
        }

        static void Bar(Rect rect, float fraction, Color color)
        {
            GUI.color = new Color(.07f,.09f,.13f); GUI.DrawTexture(rect, Texture2D.whiteTexture);
            rect.width *= Mathf.Clamp01(fraction); GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white;
        }

        static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(Mathf.Max(0, seconds));
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        void DrawMainMenu()
        {
            if (settingsOpen) { DrawSettings(); return; }
            GUI.color = new Color(0,0,0,.85f); GUI.DrawTexture(new Rect(0,0,1280,720), Texture2D.whiteTexture); GUI.color = Color.white;
            GUI.Label(new Rect(240,150,800,60), "POTTU / BONK", centerTitleStyle);
            GUI.Label(new Rect(240,220,800,30), "Selviydy hengissä niin pitkään kuin pystyt", centerTextStyle);
            GUI.Label(new Rect(240,270,800,34), "PARAS SELVIYTYMISAIKA  " + FormatTime(BestSurvivalSeconds), centerTextStyle);
            if (GUI.Button(new Rect(490,340,300,56), "PELAA  [ENTER]", centerButtonStyle)) StartGame();
            if (GUI.Button(new Rect(490,406,300,50), "ASETUKSET", centerButtonStyle)) settingsOpen = true;
            if (GUI.Button(new Rect(490,466,300,50), "LOPETA", centerButtonStyle)) QuitGame();
            DrawMapSeedMenu();
            DrawMultiplayerPanel();
            GUI.Label(new Rect(240,650,800,30), "WASD liiku   •   SPACE kierähdä   •   E tutki   •   TAB kauppa", centerTextStyle);
        }

        void DrawMultiplayerPanel()
        {
            var net = SurvivorNetwork.Instance;
            if (!net) return;
            GUI.Label(new Rect(240,520,800,26), "MONINPELI", centerTextStyle);
            switch (net.CurrentState)
            {
                case SurvivorNetwork.State.Connected when net.JoinCode != "":
                    GUI.Label(new Rect(240,548,800,30), "Liittymiskoodi: " + net.JoinCode +
                        (net.ConnectedFriendCount > 0 ? "   (kaveri pelissä!)" : "   (odotetaan kaveria...)"), centerTextStyle);
                    if (GUI.Button(new Rect(490,580,300,40), "Kopioi koodi", centerButtonStyle)) GUIUtility.systemCopyBuffer = net.JoinCode;
                    break;
                case SurvivorNetwork.State.Connected:
                case SurvivorNetwork.State.SigningIn:
                case SurvivorNetwork.State.Hosting:
                case SurvivorNetwork.State.Joining:
                    GUI.Label(new Rect(240,548,800,30), net.StatusMessage, centerTextStyle);
                    break;
                default:
                    if (net.CurrentState == SurvivorNetwork.State.Error) GUI.Label(new Rect(240,548,800,26), net.StatusMessage, centerTextStyle);
                    if (GUI.Button(new Rect(340,580,240,40), "ISÄNNÖI KAVERILLE", centerButtonStyle)) net.HostGame();
                    joinCodeInputField = GUI.TextField(new Rect(590,580,180,40), joinCodeInputField, 12);
                    if (GUI.Button(new Rect(780,580,180,40), "LIITY KOODILLA", centerButtonStyle)) net.JoinGame(joinCodeInputField);
                    break;
            }
        }

        void DrawSettings()
        {
            GUI.color = new Color(0,0,0,.85f); GUI.DrawTexture(new Rect(0,0,1280,720), Texture2D.whiteTexture); GUI.color = Color.white;
            GUI.Label(new Rect(240,140,800,60), "ASETUKSET", centerTitleStyle);

            GUI.Label(new Rect(390,250,500,30), "Ruudunpäivitysnopeus", centerTextStyle);
            GUI.color = fpsCap == 60 ? new Color(.3f,.85f,1) : Color.white;
            if (GUI.Button(new Rect(390,290,235,50), "60 FPS", centerButtonStyle)) SetFpsCap(60);
            GUI.color = fpsCap == 120 ? new Color(.3f,.85f,1) : Color.white;
            if (GUI.Button(new Rect(655,290,235,50), "120 FPS", centerButtonStyle)) SetFpsCap(120);
            GUI.color = Color.white;

            GUI.Label(new Rect(390,380,500,30), "Äänenvoimakkuus  " + Mathf.RoundToInt(masterVolume * 100) + " %", centerTextStyle);
            float newVolume = GUI.HorizontalSlider(new Rect(390,420,500,24), masterVolume, 0f, 1f);
            if (!Mathf.Approximately(newVolume, masterVolume)) SetVolume(newVolume);

            if (GUI.Button(new Rect(490,500,300,50), "TAKAISIN", centerButtonStyle)) settingsOpen = false;
        }

        void OnDestroy() { if (swipeMesh) Destroy(swipeMesh); foreach (var mesh in mapMeshes) if (mesh) Destroy(mesh); foreach (var m in materials) if (m) Destroy(m); }
    }
}



