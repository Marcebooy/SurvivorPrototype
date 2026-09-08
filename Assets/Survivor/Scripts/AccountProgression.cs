using System;
using UnityEngine;

namespace BonkSurvivor
{
    // Account Level (koko tilin XP, riippumaton hahmosta) + per-hahmo Mastery (1-100).
    // EI KOSKAAN pelimekaanista bonusta — vain seurantaa palkintoja/kosmetiikkaa varten.
    // Katso HannuBonk-projektin "Pelaajataso, Ranked & Leaderboard -suunnitelma" -dokumentti.
    public sealed partial class SurvivorGame
    {
        public int AccountXP { get; private set; }
        public int AccountLevel { get; private set; }
        public int LastRunAccountXp { get; private set; }
        bool accountXpPaidThisRun;

        // Account Level: tasaisesti kasvava kaava, ei ylärajaa.
        static int AccountXpForLevel(int level) => 100 + level * 40;

        // Mastery: nopea alku (1-10, palkinto joka tasolla), keskitiheä (11-30),
        // harva ja iso loppupäässä (31-100) — taso 100 on aidosti pitkän pelaamisen merkki.
        static int MasteryXpForLevel(int level)
        {
            if (level < 10) return 60 + level * 20;
            if (level < 30) return 400 + (level - 10) * 70;
            return 2000 + (level - 30) * 260;
        }

        static int LevelFromXp(int xp, Func<int, int> costForLevel, int cap)
        {
            int level = 0, spent = 0;
            while (level < cap)
            {
                int cost = costForLevel(level);
                if (spent + cost > xp) break;
                spent += cost; level++;
            }
            return level;
        }

        void LoadAccountProgression()
        {
            AccountXP = PlayerPrefs.GetInt(SaveKey + "AccountXP", 0);
            AccountLevel = LevelFromXp(AccountXP, AccountXpForLevel, int.MaxValue);
        }

        public int MasteryXpFor(PlayerCharacter character) => PlayerPrefs.GetInt(SaveKey + "MasteryXP_" + character, 0);

        public int MasteryLevelFor(PlayerCharacter character) => LevelFromXp(MasteryXpFor(character), MasteryXpForLevel, 100);

        // Kutsutaan FinishRunista (SurvivorProgression.cs) kerran per run — sekä kuoleman
        // että "Poistu päävalikkoon" -napin jälkeen, koska molemmat kutsuvat FinishRunia.
        void GrantAccountAndMasteryXp()
        {
            if (accountXpPaidThisRun) return;
            accountXpPaidThisRun = true;

            int runXp = Kills + bossKills * 50 + Mathf.FloorToInt(Elapsed / 10f);
            runXp = Mathf.Max(runXp, 10); // pieni osallistumisbonus ettei nopea kuolema anna nollaa

            LastRunAccountXp = runXp;

            AccountXP += runXp;
            PlayerPrefs.SetInt(SaveKey + "AccountXP", AccountXP);
            AccountLevel = LevelFromXp(AccountXP, AccountXpForLevel, int.MaxValue);

            int masteryXp = MasteryXpFor(SelectedCharacter) + runXp;
            PlayerPrefs.SetInt(SaveKey + "MasteryXP_" + SelectedCharacter, masteryXp);

            PlayerPrefs.Save();
        }
    }
}
