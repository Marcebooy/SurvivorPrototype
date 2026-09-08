using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BonkSurvivor
{
    // Self-contained first playable. Only runs in the dedicated prototype scene.
    public sealed partial class SurvivorGame : MonoBehaviour
    {
        [Header("Starting balance")]
        public float moveSpeed = 8f;
        public float damage = 18f;
        public float attackRate = 1.3f;
        public float pickupRadius = 3.5f;
        // Runs now end on death; portals advance between areas.
        public float arenaRadius = 60f;
        public float maxHealth = 100f;
        public float Health { get; private set; }
        public int Level { get; private set; } = 1;
        public int Coins { get; private set; }
        public int Kills { get; private set; }
        public int Experience { get; private set; }
        public int NextLevel => 8 + (Level - 1) * 5;
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
        Transform player, world, slash;
        Camera followCamera;
        Material enemyMaterial, eliteMaterial, xpMaterial, boltMaterial;
        float spawnTimer, attackTimer, invulnerability, slashTimer;
        string notice = "Selviä viisi minuuttia!";
        float noticeUntil = 5f;
        GUIStyle titleStyle, textStyle, buttonStyle, centerTitleStyle, centerTextStyle, centerButtonStyle;
        sealed class Enemy { public Transform body; public float health, speed; public bool elite; }
        sealed class Drop { public Transform body; public int value; }
        sealed class Bolt { public Transform body; public Vector3 direction; public float life, power; public HashSet<Enemy> hit = new HashSet<Enemy>(); }

        void Start() { LoadHighscore(); BuildWorld(); }

        void StartGame() { AtMainMenu = false; ResetRun(); }
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
            var ground = MakeMaterial(new Color(.09f, .18f, .21f));
            var stone = MakeMaterial(new Color(.17f, .3f, .33f));
            var cyan = MakeMaterial(new Color(.2f, .95f, .85f));
            enemyMaterial = MakeMaterial(new Color(.95f, .28f, .28f));
            eliteMaterial = MakeMaterial(new Color(1f, .6f, .16f));
            xpMaterial = MakeMaterial(new Color(.3f, .8f, 1f));
            boltMaterial = MakeMaterial(new Color(1f, .9f, .35f));
            Shape("Arena", PrimitiveType.Cylinder, new Vector3(0, -.3f, 0), new Vector3(arenaRadius * 2 + 4, .3f, arenaRadius * 2 + 4), ground, world);
            int pillarCount = Mathf.RoundToInt(48f * arenaRadius / 38f);
            for (int i = 0; i < pillarCount; i++)
            {
                float a = i * Mathf.PI * 2 / pillarCount;
                var p = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * (arenaRadius + 1.2f);
                Shape("Boundary pillar", PrimitiveType.Cube, p + Vector3.up, new Vector3(.7f, 2, .7f), i % 4 == 0 ? cyan : stone, world);
            }
            int floorExtent = Mathf.CeilToInt(arenaRadius);
            for (int x = -floorExtent; x <= floorExtent; x += 6)
            for (int z = -floorExtent; z <= floorExtent; z += 6)
                if (new Vector2(x,z).magnitude < arenaRadius - 2)
                    Shape("Floor marker", PrimitiveType.Cube, new Vector3(x, .015f, z), new Vector3(.14f, .025f, .14f), stone, world);
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
            foreach (var e in enemies) Destroy(e.body.gameObject);
            foreach (var d in drops) Destroy(d.body.gameObject);
            foreach (var b in bolts) Destroy(b.body.gameObject);
            enemies.Clear(); drops.Clear(); bolts.Clear();
            System.Array.Clear(purchases, 0, purchases.Length);
            moveSpeed = 8; damage = 18; attackRate = 1.3f; pickupRadius = 3.5f; maxHealth = 100;
            Health = maxHealth; Level = 1; Coins = Kills = Experience = 0; Elapsed = 0;
            ShopOpen = Finished = false; spawnTimer = attackTimer = invulnerability = slashTimer = 0;
            player.position = Vector3.up; slash.gameObject.SetActive(false);
            ResetProgression(); ResetPottu(); notice = "Valitse aloitusase"; noticeUntil = 5; UpdateCamera(true);
        }

        void Update()
        {
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
            if (ShopOpen || Selecting || PendingChoices > 0) return;
            float dt = Time.deltaTime; Elapsed += dt;

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
            if (boss == null && Elapsed - areaStarted < 90 && spawnTimer <= 0 && enemies.Count < 260)
            {
                SpawnEnemy(); spawnTimer = Mathf.Max(.09f, .8f - Elapsed / 200f);
            }
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var e = enemies[i]; var delta = player.position - e.body.position; delta.y = 0;
                e.body.position += delta.normalized * (e.speed * dt);
                if (delta.sqrMagnitude < 1.7f && invulnerability <= 0)
                {
                    ReceiveDamage(e.elite ? 26 : 13);
                    e.body.position -= delta.normalized * 1.5f;
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
                if (distance < 1.1f) { GrantExperience(d.value); Coins += d.value; Destroy(d.body.gameObject); drops.RemoveAt(i); }
            }
        }

        void LateUpdate() { if (followCamera) UpdateCamera(false); }
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
            p = Vector3.ClampMagnitude(p, arenaRadius); p.y = 1;
            bool elite = Elapsed > 20 && Random.value < .24f;
            enemies.Add(new Enemy {
                body = Shape(elite ? "Brute" : "Chaser", PrimitiveType.Capsule, p, Vector3.one * (elite ? 1.3f : .8f), elite ? eliteMaterial : enemyMaterial, world),
                health = (elite ? 130 : 34) * (1 + Elapsed / 200) * curse * (1 + .3f * (Area - 1)), speed = ((elite ? 3.4f : 4.6f) + Mathf.Min(3, Elapsed / 70)) * curse, elite = elite
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
                    { b.hit.Add(e); Hit(e, b.power); }
                }
                if (b.life <= 0) { Destroy(b.body.gameObject); bolts.RemoveAt(i); }
            }
        }

        void Hit(Enemy e, float amount)
        {
            bool critical = Random.value < CritChance; if (critical) amount *= 2; HitFeedback(e, amount, critical); e.health -= amount;
            if (e.health > 0) { e.body.position += (e.body.position - player.position).normalized * .6f; return; }
            if (e == boss) { boss = null; BossDefeated = true; bossDefeatedAt = Elapsed; bossKills++; notice = "Bossi voitettu! Seuraava alue avautuu..."; noticeUntil = Elapsed + 8; }
            int value = e.elite ? 5 : 1;
            // Merge nearby drops when the arena is crowded, preserving all XP and coins.
            if (drops.Count >= 250) drops[0].value += value;
            else drops.Add(new Drop { body = Shape("XP + gold", PrimitiveType.Cube, new Vector3(e.body.position.x, .5f, e.body.position.z), Vector3.one * .4f, xpMaterial, world), value = value });
            enemies.Remove(e); Destroy(e.body.gameObject); Kills++;
        }

        public void GrantExperience(int amount)
        {
            if (amount <= 0 || Selecting || Finished || boss != null) return;
            Experience += amount;
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
            }
            titleStyle.normal.textColor = Color.white; textStyle.normal.textColor = Color.white;
            buttonStyle.normal.textColor = Color.white; buttonStyle.hover.textColor = Color.white;
            centerTitleStyle.normal.textColor = Color.white; centerTextStyle.normal.textColor = Color.white;
            centerButtonStyle.normal.textColor = Color.white; centerButtonStyle.hover.textColor = Color.white;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
            if (AtMainMenu) { DrawMainMenu(); return; }
            bool overlayOpen = ShopOpen || Finished || Selecting || PendingChoices > 0;
            if (!overlayOpen)
            {
                GUI.Box(new Rect(20,20,330,144), GUIContent.none);
                GUI.Label(new Rect(36,28,310,40), "POTTU / BONK", titleStyle);
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
            GUI.color = new Color(0,0,0,.85f); GUI.DrawTexture(new Rect(0,0,1280,720), Texture2D.whiteTexture); GUI.color = Color.white;
            GUI.Label(new Rect(240,150,800,60), "POTTU / BONK", centerTitleStyle);
            GUI.Label(new Rect(240,220,800,30), "Selviydy hengissä niin pitkään kuin pystyt", centerTextStyle);
            GUI.Label(new Rect(240,270,800,34), "PARAS SELVIYTYMISAIKA  " + FormatTime(BestSurvivalSeconds), centerTextStyle);
            if (GUI.Button(new Rect(490,340,300,56), "PELAA  [ENTER]", centerButtonStyle)) StartGame();
            if (GUI.Button(new Rect(490,406,300,50), "LOPETA", centerButtonStyle)) QuitGame();
            GUI.Label(new Rect(240,650,800,30), "WASD liiku   •   SPACE kierähdä   •   E tutki   •   TAB kauppa", centerTextStyle);
        }

        void OnDestroy() { if (swipeMesh) Destroy(swipeMesh); foreach (var m in materials) if (m) Destroy(m); }
    }
}






