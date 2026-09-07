var g = UnityEngine.Object.FindFirstObjectByType<BonkSurvivor.SurvivorGame>();
var type = g.GetType();
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var results = new System.Collections.Generic.List<string>();
void Check(bool ok, string label) { if (!ok) throw new System.Exception(label); results.Add(label); }
void Set(string p, object v) { type.GetProperty(p).GetSetMethod(true).Invoke(g, new object[]{v}); }
object Call(string method, params object[] args) => type.GetMethod(method, flags).Invoke(g, args);
object Field(string name) => type.GetField(name, flags).GetValue(g);
var keys = new [] { "BonkSurvivor.Prototype.v1.Silver", "BonkSurvivor.Prototype.v1.Health" };
var existed = new bool[2]; var saved = new int[2];
for(int i=0;i<2;i++) { existed[i] = UnityEngine.PlayerPrefs.HasKey(keys[i]); saved[i] = UnityEngine.PlayerPrefs.GetInt(keys[i]); }
try
{
    UnityEngine.PlayerPrefs.SetInt(keys[0], 0); UnityEngine.PlayerPrefs.SetInt(keys[1], 0);
    g.ResetRun();
    Check(g.Selecting && g.WeaponCount == 0, "Starts at weapon selection");
    g.SendMessage("Update"); Check(g.Elapsed == 0, "Selection pauses simulation");
    Check(!g.SelectStarter(3) && g.SelectStarter(1) && g.WeaponCount == 1 && !g.SelectStarter(0), "Exactly one valid starter");
    g.GrantExperience(21); Check(g.Level == 3 && g.PendingChoices == 2 && g.Experience == 0, "Multi-level XP queues choices");
    Check(g.Choices.Count == 3 && new System.Collections.Generic.HashSet<int>(g.Choices).Count == 3, "Three distinct random upgrades");
    g.SendMessage("Update"); Check(g.Elapsed == 0, "Level choice pauses simulation");
    Check(!g.ChooseUpgrade(-1) && g.ChooseUpgrade(0) && g.PendingChoices == 1 && g.ChooseUpgrade(2) && g.PendingChoices == 0, "Choices consume once and drain queue");
    var player = (UnityEngine.Transform)Field("player");
    var landmarks = (System.Collections.IList)Field("landmarks");
    var chest = landmarks[0]; var chestTransform = (UnityEngine.Transform)chest.GetType().GetField("body").GetValue(chest);
    Set("Coins", 30); Check(!(bool)Call("Interact", chest), "Chest requires proximity");
    player.position = chestTransform.position;
    Check((bool)Call("Interact", chest) && g.Coins == 18 && !(bool)Call("Interact", chest), "Chest costs gold and is one-use");
    var shrine = landmarks[3]; player.position = ((UnityEngine.Transform)shrine.GetType().GetField("body").GetValue(shrine)).position;
    float before = g.damage;
    Check((bool)Call("Interact", shrine) && g.damage > before && (float)Field("curse") > 1 && !(bool)Call("Interact", shrine), "Shrine applies reward and persistent risk once");
    var portal = landmarks[4]; player.position = ((UnityEngine.Transform)portal.GetType().GetField("body").GetValue(portal)).position;
    Check(!(bool)Call("Interact", portal) && !g.BossActive, "Portal locked before timer");
    Set("Elapsed", 91f); Check((bool)Call("Interact", portal) && g.BossActive, "Portal summons boss");
    Check(!(bool)Call("Interact", portal) && g.Area == 1, "Cannot skip active boss");
    Call("Hit", Field("boss"), 100000f); Check(g.BossDefeated && !g.BossActive, "Boss death unlocks portal");
    int weapons = g.WeaponCount;
    Check((bool)Call("Interact", portal) && g.Area == 2 && g.WeaponCount == weapons && !g.BossDefeated, "Next area keeps build and resets boss gate");
    Set("Kills", 50); Call("FinishRun"); int silver = g.Silver;
    Check(g.Finished && silver == 30 && g.EarnedSilver == 30, "Death awards kill and boss Silver");
    Call("FinishRun"); Check(g.Silver == silver, "Rewards paid only once");
    Check(g.BuyLegacyHealth() && g.Silver == 10 && g.LegacyHealth == 1, "Persistent HP purchase debits Silver");
    g.ResetRun(); Check(g.Silver == 10 && g.maxHealth == 110 && g.Health == 110 && g.Selecting, "Persistence reload and fresh run");
    // Check each actual attack, independent of random level-up offers.
    for(int weapon=0;weapon<3;weapon++)
    {
        g.ResetRun(); g.SelectStarter(weapon); player.position = UnityEngine.Vector3.up;
        Call("SpawnEnemy"); var enemies = (System.Collections.IList)Field("enemies"); var enemy = enemies[0];
        var et = enemy.GetType(); ((UnityEngine.Transform)et.GetField("body").GetValue(enemy)).position = player.position + UnityEngine.Vector3.forward * 2;
        et.GetField("health").SetValue(enemy, 1f);
        Call("AttackWeapons"); if(weapon == 1) Call("UpdateBolts", .1f);
        Check(g.Kills == 1, "Weapon " + weapon + " kills target");
    }
    g.ResetRun(); g.SelectStarter(1); Call("SpawnEnemy"); Call("SpawnEnemy");
    var es = (System.Collections.IList)Field("enemies");
    for(int i=0;i<es.Count;i++) { var e=es[i]; var t=e.GetType(); ((UnityEngine.Transform)t.GetField("body").GetValue(e)).position = player.position + UnityEngine.Vector3.forward * (2+i); t.GetField("health").SetValue(e,1f); }
    Call("AttackWeapons"); Call("UpdateBolts", .2f); Check(g.Kills == 2, "Bow pierces multiple enemies in one step");
}
finally
{
    for(int i=0;i<2;i++) { if(existed[i]) UnityEngine.PlayerPrefs.SetInt(keys[i],saved[i]); else UnityEngine.PlayerPrefs.DeleteKey(keys[i]); }
    UnityEngine.PlayerPrefs.Save(); g.ResetRun();
}
return results;
