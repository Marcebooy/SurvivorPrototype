using System.Collections.Generic;

namespace BonkSurvivor
{
    // Yhtenäinen WeaponStats-pipeline (pelaaja-progressio-ja-ranked-suunnitelma.md, kohta 10.5).
    // Data (arkkityyppi + applicableStats per ase), GetWeaponStats()-keskitetty laskenta ja
    // yhtenäinen ajastintaulukko (AttackTimerSlots) on migratoitu käyttöön kaikilla 30 aseella
    // (CastArsenal/TickArsenal, CastAdvanced/TickAdvanced, AttackWeapons) - ei muuta pelattavaa
    // käytöstä, koska aseet joilla ei ole WeaponMetadata-merkintää saavat aina Identity-kertoimet
    // eikä perusintervalleja muutettu. Sword/Bow/Lightning jakavat yhä tarkoituksella yhden
    // ajastinslotin (ks. AttackWeapons) - jaetun ajastimen korjaus on erillinen, myöhemmin
    // hyväksyttäväksi jätetty tehtävä, samoin "Crit yleiseksi" (globaali CritChance/CritMultiplier).
    public sealed partial class SurvivorGame
    {
        // Käytöstyyppi ohjaa mm. sitä mistä kohdasta CastArsenal/CastAdvanced-koodia ase
        // lukee GetWeaponStats()-tulostaan (vaihe 3) ja mikä ajastinmalli sillä on (vaihe 4).
        public enum WeaponArchetype
        {
            Melee,           // lähitähtäys lähimpään/useampaan kohteeseen kantaman sisällä (Sword, Katana, Dice, ...)
            RangedProjectile,// fyysinen ammus joka lentää/kimpoaa/läpäisee (Bow, Firestaff, Bone, Axe, ...)
            Orbit,           // hahmon ympärillä kiertävä jatkuva este/isku (Chunkers)
            AoE,             // paikallaan pysyvä tai leviävä alue-/vyöhykevaikutus (Flamewalker, Frostwalker, Aura, ...)
            Beam,            // linjamainen/ketjuuntuva välitön osuma (Lightning, Sniper, Space Noodle)
        }

        public struct WeaponMeta
        {
            public WeaponArchetype archetype;
            public AffixStat[] applicableStats; // statit jotka ovat mekaanisesti mielekkäitä tälle aseelle
        }

        // Lähde: kohdan 10.5 kartoitus (AGENTS.md:n statshyötylistat 13:lle jo toteutetulle aseelle,
        // loput 17 päätelty aseen CastAdvanced/CastArsenal-mekaniikasta samalla "mikä fyysisesti
        // vaikuttaa tähän aseeseen" -periaatteella). Nykyisten 13 aseen applicableStats on
        // tarkoituksella identtinen niiden jo tuotannossa olevan AffixPool-listan kanssa
        // (WeaponAffixes.cs) - tämä taulukko ei muuta niiden käytöstä miltään osin, se on vasta
        // tuleva yhden totuuden lähde jota AffixPool tulee myöhemmin lukemaan tämän sijaan.
        static readonly Dictionary<Weapon, WeaponMeta> WeaponMetadata = new Dictionary<Weapon, WeaponMeta>
        {
            // --- Aloitusaseet (7/7 affiksit toteutettu - applicableStats == nykyinen AffixPool) ---
            { Weapon.Sword, new WeaponMeta { archetype = WeaponArchetype.Melee,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Quantity, AffixStat.Cooldown } } },
            { Weapon.Bow, new WeaponMeta { archetype = WeaponArchetype.RangedProjectile,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Quantity, AffixStat.Crit, AffixStat.ProjectileSpeed } } },
            { Weapon.Lightning, new WeaponMeta { archetype = WeaponArchetype.Beam,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Quantity, AffixStat.Bounces, AffixStat.Cooldown } } },
            { Weapon.Firestaff, new WeaponMeta { archetype = WeaponArchetype.RangedProjectile,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Quantity, AffixStat.Size, AffixStat.ProjectileSpeed } } },
            { Weapon.Chunkers, new WeaponMeta { archetype = WeaponArchetype.Orbit,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Quantity, AffixStat.Size, AffixStat.ProjectileSpeed } } },
            { Weapon.Bone, new WeaponMeta { archetype = WeaponArchetype.RangedProjectile,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Quantity, AffixStat.Bounces, AffixStat.ProjectileSpeed } } },
            { Weapon.Flamewalker, new WeaponMeta { archetype = WeaponArchetype.AoE,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Duration, AffixStat.Size, AffixStat.Cooldown } } },

            // --- Avattavat aseet, affiksit toteutettu (6/23 - applicableStats == nykyinen AffixPool) ---
            { Weapon.Revolver, new WeaponMeta { archetype = WeaponArchetype.RangedProjectile,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Bounces, AffixStat.Quantity, AffixStat.Cooldown } } },
            { Weapon.Axe, new WeaponMeta { archetype = WeaponArchetype.RangedProjectile,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Quantity, AffixStat.Cooldown } } },
            { Weapon.Katana, new WeaponMeta { archetype = WeaponArchetype.Melee,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Cooldown, AffixStat.Crit, AffixStat.Quantity } } },
            { Weapon.Shotgun, new WeaponMeta { archetype = WeaponArchetype.RangedProjectile,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Quantity, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.Frostwalker, new WeaponMeta { archetype = WeaponArchetype.AoE,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Duration, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.BlackHole, new WeaponMeta { archetype = WeaponArchetype.AoE,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Duration, AffixStat.Cooldown } } },

            // --- Avattavat aseet, ei vielä affikseja (17/23) - applicableStats päätelty
            // CastAdvanced/CastArsenal-mekaniikasta, ei vielä käytössä missään (affiksien
            // laajennus lopuille aseille tulee vasta tämän pipelinen jälkeen, kohta 10.3). ---
            { Weapon.Aegis, new WeaponMeta { archetype = WeaponArchetype.AoE,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.Bananarang, new WeaponMeta { archetype = WeaponArchetype.RangedProjectile,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Quantity, AffixStat.Size, AffixStat.Duration } } },
            { Weapon.Aura, new WeaponMeta { archetype = WeaponArchetype.AoE,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.SpaceNoodle, new WeaponMeta { archetype = WeaponArchetype.Beam,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Duration, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.Sniper, new WeaponMeta { archetype = WeaponArchetype.Beam,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Cooldown, AffixStat.Crit } } },
            { Weapon.Rocket, new WeaponMeta { archetype = WeaponArchetype.RangedProjectile,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Quantity, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.Mines, new WeaponMeta { archetype = WeaponArchetype.AoE,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Duration, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.WirelessDagger, new WeaponMeta { archetype = WeaponArchetype.RangedProjectile,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Bounces, AffixStat.Quantity, AffixStat.Cooldown } } },
            { Weapon.Tornado, new WeaponMeta { archetype = WeaponArchetype.AoE,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Duration, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.Dexecutioner, new WeaponMeta { archetype = WeaponArchetype.Melee,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Cooldown, AffixStat.Crit } } },
            { Weapon.BloodMagic, new WeaponMeta { archetype = WeaponArchetype.Melee,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.PoisonFlask, new WeaponMeta { archetype = WeaponArchetype.AoE,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Duration, AffixStat.Cooldown } } },
            { Weapon.DragonBreath, new WeaponMeta { archetype = WeaponArchetype.AoE,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.Dice, new WeaponMeta { archetype = WeaponArchetype.Melee,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Cooldown, AffixStat.Crit } } },
            { Weapon.HeroSword, new WeaponMeta { archetype = WeaponArchetype.Melee,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Quantity, AffixStat.Cooldown } } },
            { Weapon.CorruptedSword, new WeaponMeta { archetype = WeaponArchetype.Melee,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Cooldown } } },
            { Weapon.Scythe, new WeaponMeta { archetype = WeaponArchetype.Melee,
                applicableStats = new[] { AffixStat.Damage, AffixStat.Size, AffixStat.Cooldown } } },
        };

        // Kaikkien kahdeksan universaalin statin AFFIKSIKERTOIMET yhdelle aseelle koottuna yhteen
        // hakuun - täsmälleen samat arvot kuin nykyiset hajanaiset AffixMultiplier(w, X)-kutsut
        // palauttaisivat kukin erikseen, ei siis mikään uusi kerroin eikä valmiiksi laskettu
        // lopputulos. Kaikki kentät ovat kertoimia (1 = ei affiksia), oletus 1 jokaiselle. Sama
        // "kerrotaan aseen omaan kaavaan" -käyttötapa säilyy ennallaan migraatiossa (vaihe 5):
        // esim. Quantity/Bounces eivät ole tässä valmiita lukumääriä, koska aseiden omat
        // peruslukumääräkaavat poikkeavat toisistaan (Quantity, Quantity+2, Quantity+1, 1, ...) -
        // kutsuja laskee edelleen itse `baseCount + Mathf.RoundToInt(baseCount*(stats.X-1))`-
        // tyylisesti, mutta lukee kertoimen tästä yhdestä struct:ista AffixMultiplier(w,stat)-
        // kutsujen sijaan.
        // VariantQualityMultiplier(w) EI sisälly tähän - se on jo nykyisin ehdollinen (vain
        // varianttikohtaisissa haaroissa, esim. Sword vain twinblade-iskussa, ei perussivalluksessa),
        // joten sen leipominen tähän muuttaisi käytöstä kaikkien aseiden ei-variantti-haaroissa.
        // Pysyy siis omana, erikseen kutsuttavana funktionaan kuten tähänkin asti.
        public struct WeaponStats
        {
            public float Damage;
            public float Cooldown;
            public float Size;
            public float Quantity;
            public float Duration;
            public float ProjectileSpeed;
            public float Bounces;
            public float Crit; // paikallinen vahinkokerroin (Bow/Katana-mallin mukaisesti), EI globaali CritChance-lisäys

            public static WeaponStats Identity => new WeaponStats
            { Damage = 1, Cooldown = 1, Size = 1, Quantity = 1, Duration = 1, ProjectileSpeed = 1, Bounces = 1, Crit = 1 };
        }

        // Taisteluikoodin lukema versio - lukee aina nykyistä pelattavaa hahmoa ja palauttaa
        // aina Identity Selviytymistilassa, samaan tapaan kuin AffixMultiplier(Weapon,AffixStat).
        WeaponStats GetWeaponStats(Weapon w) =>
            activeMapType == MapType.Procedural ? WeaponStats.Identity : GetWeaponStatsFor(SelectedCharacter, w);

        // Ungated versio (UI/testaus) - sama malli kuin AffixMultiplierFor. Aseille joilla ei ole
        // vielä WeaponMetadata-merkintää (17 vielä-affiksitonta) palauttaa Identityn, koska
        // applicableStats-lista puuttuu eikä statin mielekkyyttä voi silloin päätellä.
        public WeaponStats GetWeaponStatsFor(PlayerCharacter c, Weapon w)
        {
            var stats = WeaponStats.Identity;
            if (!WeaponMetadata.TryGetValue(w, out var meta)) return stats;
            foreach (var stat in meta.applicableStats)
            {
                float mult = AffixMultiplierFor(c, w, stat);
                switch (stat)
                {
                    case AffixStat.Damage: stats.Damage = mult; break;
                    case AffixStat.Cooldown: stats.Cooldown = mult; break;
                    case AffixStat.Size: stats.Size = mult; break;
                    case AffixStat.Quantity: stats.Quantity = mult; break;
                    case AffixStat.Duration: stats.Duration = mult; break;
                    case AffixStat.ProjectileSpeed: stats.ProjectileSpeed = mult; break;
                    case AffixStat.Bounces: stats.Bounces = mult; break;
                    case AffixStat.Crit: stats.Crit = mult; break;
                }
            }
            return stats;
        }

        // Yhtenäinen ajastintaulukko - yksi slotti per Weapon-enumin arvo, käytössä AttackWeapons/
        // TickArsenal/TickAdvanced:issa kaikilla 30 aseella (Sword/Bow/Lightning jakavat
        // tarkoituksella Sword-indeksin, ks. AttackWeapons).
        float[] weaponSlotTimers => current.AttackTimerSlots;

        // Perusintervalli (sekunteina attackRate=1:llä, ennen Cooldown-affiksia) yhdelle aseelle -
        // yksi totuuden lähde kolmen nykyisen hajanaisen lähteen sijaan: Sword/Bow/Lightning
        // (implisiittisesti 1, AttackWeapons-metodin jaettu attackTimer), TickArsenal:in ternääri
        // (Flamewalker/Aura/Firestaff/Shotgun/oletus - kattaa myös Chunkers/Bonen, joilla samat
        // lukemat) ja AdvancedCatalog.Entries[w-9] (Revolver...Scythe). Lukemat kopioitu suoraan
        // näistä kolmesta lähteestä sellaisenaan, ei muutettu.
        static float WeaponInterval(Weapon w)
        {
            if (w == Weapon.Sword || w == Weapon.Bow || w == Weapon.Lightning) return 1f;
            if ((int)w >= 3 && (int)w <= 8)
            {
                return w == Weapon.Flamewalker ? .65f : w == Weapon.Aura ? .55f : w == Weapon.Firestaff ? 1.5f : w == Weapon.Shotgun ? 1.3f : .95f;
            }
            if ((int)w >= 9) return AdvancedCatalog.Entries[(int)w - 9].interval;
            return 1f;
        }
    }
}
