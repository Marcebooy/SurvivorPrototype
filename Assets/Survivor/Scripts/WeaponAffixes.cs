using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    // Ase-affiksit: per (hahmo, ase) rullatut, pysyvät bonukset yhdelle statille kerrallaan,
    // max 2 slottia per ase. AGENTS.md:n dokumentoima statshyötylista per ase - aluksi vain
    // seitsemälle aloitusaseelle (pilotti), laajennettu kuudelle avattavalle aseelle (Revolver/
    // Axe/Katana/Shotgun/Frostwalker/BlackHole), ja nyt (kohta 10.5:n jälkeen, kohta 10.3/11)
    // kuudelle lisää (Mines/Tornado/PoisonFlask/SpaceNoodle/Rocket/WirelessDagger). Vain
    // Mappi-tilassa (droppi + käyttö) - Selviytymistila (Procedural) ei koskaan aseta, lue tai
    // kuluta affiksidataa miltään osin.
    public sealed partial class SurvivorGame
    {
        public enum AffixStat { Damage, Size, Quantity, Cooldown, Duration, ProjectileSpeed, Bounces, Crit }

        public struct WeaponAffix
        {
            public AffixStat stat; public float percent; // percent: +15..+35
        }

        const int MaxAffixSlots = 2;

        // Affiksit käytössä näille 19 aseelle - kunkin pooli LUETAAN WeaponStats.cs:n
        // WeaponMetadata[w].applicableStats:sta (kohta 10.5), ei kirjoiteta erikseen käsin: uuden
        // aseen affiksien käyttöönotto vaatii siis enää vain sen lisäämisen tähän listaan, kunhan
        // AGENTS.md:ssä on sille dokumentoitu statshyötylista jota applicableStats-arvo vastaa.
        // Ensimmäiset 13 (7 pilotti + 6 laajennus) - committoitu (44ee574/1fc7f1b, 1669638/146c923).
        // Seuraavat 6 (kohta 10.3/11, 2026-09-09) - Mines/Tornado/PoisonFlask/SpaceNoodle/Rocket/
        // WirelessDagger valittu koska niiden CastAdvanced/AddAdvancedShot/AddAdvancedZone-koodi
        // jo käytti stats.Size/stats.Duration/stats.Quantity/stats.Bounces-kertoimia identtisellä
        // kaavalla kuin BlackHole (jolla affiksit jo toimivat) - Cooldown toimii kaikille
        // TickAdvanced-ajastimen kautta yleisesti. Loput 11 (Dice/CorruptedSword/BloodMagic/
        // Scythe/Dexecutioner/Sniper/Aegis/Bananarang/HeroSword/Aura/DragonBreath) jätetty
        // tarkoituksella tämän tehtävän ulkopuolelle - osalla (esim. Sniper) applicableStats-listan
        // Size-affiksi ei vielä vaikuttaisi CastAdvanced-koodissa mitenkään (LineHit ei lue
        // stats.Size:a), ja se korjaus kuuluu vasta niiden omaan affiksien käyttöönottotehtävään.
        static readonly Weapon[] AffixEnabledWeapons =
        {
            Weapon.Sword, Weapon.Flamewalker, Weapon.Lightning, Weapon.Firestaff, Weapon.Chunkers, Weapon.Bone, Weapon.Bow,
            Weapon.Revolver, Weapon.Axe, Weapon.Katana, Weapon.Shotgun, Weapon.Frostwalker, Weapon.BlackHole,
            Weapon.Mines, Weapon.Tornado, Weapon.PoisonFlask, Weapon.SpaceNoodle, Weapon.Rocket, Weapon.WirelessDagger,
        };

        // Lazily built (not a static-initializer field) because WeaponMetadata lives in a
        // different partial-class file (WeaponStats.cs) - relying on static field init order
        // across partial-class files would be fragile. First access builds it once and caches it.
        static Dictionary<Weapon, AffixStat[]> affixPoolCache;
        static Dictionary<Weapon, AffixStat[]> AffixPool
        {
            get
            {
                if (affixPoolCache == null)
                {
                    affixPoolCache = new Dictionary<Weapon, AffixStat[]>();
                    foreach (var w in AffixEnabledWeapons) affixPoolCache[w] = WeaponMetadata[w].applicableStats;
                }
                return affixPoolCache;
            }
        }

        static string AffixStatLabel(AffixStat s) => s switch
        {
            AffixStat.Damage => "Dmg", AffixStat.Size => "Size", AffixStat.Quantity => "Qty",
            AffixStat.Cooldown => "CD", AffixStat.Duration => "Dur", AffixStat.ProjectileSpeed => "Speed",
            AffixStat.Bounces => "Bounce", AffixStat.Crit => "Crit", _ => s.ToString(),
        };

        readonly Dictionary<(PlayerCharacter character, Weapon weapon), List<WeaponAffix>> weaponAffixes =
            new Dictionary<(PlayerCharacter, Weapon), List<WeaponAffix>>();

        int affixAddMaterial, affixRerollMaterial;

        static string AffixSaveKey(PlayerCharacter c) => SaveKey + "Affixes_" + c;
        const string AffixAddMaterialKey = SaveKey + "AffixAddMaterial";
        const string AffixRerollMaterialKey = SaveKey + "AffixRerollMaterial";

        public int AffixAddMaterial => affixAddMaterial;
        public int AffixRerollMaterial => affixRerollMaterial;

        public IReadOnlyList<WeaponAffix> AffixesFor(PlayerCharacter c, Weapon w) =>
            weaponAffixes.TryGetValue((c, w), out var list) ? list : (IReadOnlyList<WeaponAffix>)System.Array.Empty<WeaponAffix>();

        // Taisteluikoodi lukee aina nykyistä pelattavaa hahmoa (kuten ActiveVariant). Affiksit voidaan
        // tallentaa hahmolle riippumatta siitä missä tilassa seuraava run pelataan (kuten variantit/
        // laatu), mutta niiden VAIKUTUS taisteluun on rajattu Mappi-tilaan - Selviytymistila (Procedural)
        // pysyy siis täysin muuttumattomana vaikka pelaajalla olisi affikseja tallessa.
        float AffixMultiplier(Weapon w, AffixStat stat) =>
            activeMapType == MapType.Procedural ? 1f : AffixMultiplierFor(SelectedCharacter, w, stat);

        // Sword ja Lightning jakavat yhä (WeaponStats-pipeline, kohta 10.5 vaihe 5 - tarkoituksella,
        // ks. AttackWeapons) yhden ajastinslotin, joten niiden Cooldown-affiksi nopeuttaa koko
        // jaettua sykettä sen ajan kun ase on weaponLevelsissa. Jaetun ajastimen korjaus (erilliset
        // slotit, täsmällinen per-ase Cooldown) on omaksi, myöhemmin erikseen hyväksyttäväksi ja
        // testattavaksi tehtäväksi jätetty muutos - ei osa tätä refaktorointia.
        float SharedAttackTimerAffixMultiplier()
        {
            float mult = 1;
            if (weaponLevels.ContainsKey(Weapon.Sword)) mult *= AffixMultiplier(Weapon.Sword, AffixStat.Cooldown);
            if (weaponLevels.ContainsKey(Weapon.Lightning)) mult *= AffixMultiplier(Weapon.Lightning, AffixStat.Cooldown);
            return mult;
        }

        public float AffixMultiplierFor(PlayerCharacter c, Weapon w, AffixStat stat)
        {
            float mult = 1;
            if (weaponAffixes.TryGetValue((c, w), out var list))
                foreach (var a in list) if (a.stat == stat) mult *= 1 + a.percent / 100f;
            return mult;
        }

        static WeaponAffix RollAffix(Weapon w, List<WeaponAffix> existing)
        {
            var pool = AffixPool[w];
            var choices = new List<AffixStat>();
            foreach (var s in pool) if (existing == null || !existing.Exists(a => a.stat == s)) choices.Add(s);
            if (choices.Count == 0) choices.AddRange(pool); // pooli täynnä jo valituilla - ei tapahdu 2-slottisella poolilla joka on >=2 aina
            var stat = choices[Random.Range(0, choices.Count)];
            return new WeaponAffix { stat = stat, percent = Random.Range(15f, 35f) };
        }

        // Käytetään HAHMOT-tabissa (päävalikko), ei kesken Mappi-runin - kuten UpgradeVariantQuality,
        // ei siis activeMapType-tarkistusta täällä (se koskee vain materiaalien droppausta kartalla).
        // Rajaus: vain omistetulle aseelle.
        public bool TryAddAffix(PlayerCharacter c, Weapon w)
        {
            if (!AffixPool.ContainsKey(w) || !IsUpgradeOwned(c, WeaponUpgradeId((int)w))) return false;
            if (affixAddMaterial <= 0) return false;
            if (!weaponAffixes.TryGetValue((c, w), out var list)) weaponAffixes[(c, w)] = list = new List<WeaponAffix>();
            if (list.Count >= MaxAffixSlots) return false;
            list.Add(RollAffix(w, list));
            affixAddMaterial--;
            PlayerPrefs.SetInt(AffixAddMaterialKey, affixAddMaterial);
            SaveAffixes(c, w);
            PlayerPrefs.Save();
            return true;
        }

        public bool TryRerollAffixes(PlayerCharacter c, Weapon w)
        {
            if (!AffixPool.ContainsKey(w) || !IsUpgradeOwned(c, WeaponUpgradeId((int)w))) return false;
            if (affixRerollMaterial <= 0) return false;
            var list = new List<WeaponAffix>();
            for (int i = 0; i < MaxAffixSlots; i++) list.Add(RollAffix(w, list));
            weaponAffixes[(c, w)] = list;
            affixRerollMaterial--;
            PlayerPrefs.SetInt(AffixRerollMaterialKey, affixRerollMaterial);
            SaveAffixes(c, w);
            PlayerPrefs.Save();
            return true;
        }

        void SaveAffixes(PlayerCharacter c, Weapon changedWeapon)
        {
            var parts = new List<string>();
            foreach (var w in AffixPool.Keys)
            {
                if (!weaponAffixes.TryGetValue((c, w), out var list) || list.Count == 0) continue;
                var affixParts = new List<string>();
                foreach (var a in list) affixParts.Add(a.stat + "=" + a.percent.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
                parts.Add(w + ":" + string.Join(",", affixParts));
            }
            PlayerPrefs.SetString(AffixSaveKey(c), string.Join(";", parts));
        }

        void LoadWeaponAffixes()
        {
            weaponAffixes.Clear();
            affixAddMaterial = PlayerPrefs.GetInt(AffixAddMaterialKey, 0);
            affixRerollMaterial = PlayerPrefs.GetInt(AffixRerollMaterialKey, 0);
            foreach (PlayerCharacter c in System.Enum.GetValues(typeof(PlayerCharacter)))
            {
                string saved = PlayerPrefs.GetString(AffixSaveKey(c), "");
                if (string.IsNullOrEmpty(saved)) continue;
                foreach (var weaponPart in saved.Split(';'))
                {
                    var split = weaponPart.Split(':');
                    if (split.Length != 2 || !System.Enum.TryParse(split[0], out Weapon w)) continue;
                    var list = new List<WeaponAffix>();
                    foreach (var affixPart in split[1].Split(','))
                    {
                        var kv = affixPart.Split('=');
                        if (kv.Length == 2 && System.Enum.TryParse(kv[0], out AffixStat stat) &&
                            float.TryParse(kv[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float percent))
                            list.Add(new WeaponAffix { stat = stat, percent = percent });
                    }
                    if (list.Count > 0) weaponAffixes[(c, w)] = list;
                }
            }
        }

        // Oma, mapItemistä ja variantti-/laatudropeista täysin riippumaton 12 % bossinkaatorulla.
        // Vain Mappi-tilassa. Rajattu aseisiin jotka pelaaja jo omistaa.
        void TryDropWeaponAffixMaterial()
        {
            if (activeMapType == MapType.Procedural) return;
            if (Random.value >= .12f) return;
            bool anyOwned = false;
            foreach (var w in AffixPool.Keys) if (IsUpgradeOwned(SelectedCharacter, WeaponUpgradeId((int)w))) { anyOwned = true; break; }
            if (!anyOwned) return;
            affixAddMaterial++;
            PlayerPrefs.SetInt(AffixAddMaterialKey, affixAddMaterial); PlayerPrefs.Save();
            notice = "Löysit affiksimateriaalia! (" + affixAddMaterial + " kpl)"; noticeUntil = Elapsed + 6;
        }

        // Toinen, edellisestä riippumaton 12 % bossinkaatorulla samoin rajauksin.
        void TryDropAffixRerollMaterial()
        {
            if (activeMapType == MapType.Procedural) return;
            if (Random.value >= .12f) return;
            bool anyOwned = false;
            foreach (var w in AffixPool.Keys) if (IsUpgradeOwned(SelectedCharacter, WeaponUpgradeId((int)w))) { anyOwned = true; break; }
            if (!anyOwned) return;
            affixRerollMaterial++;
            PlayerPrefs.SetInt(AffixRerollMaterialKey, affixRerollMaterial); PlayerPrefs.Save();
            notice = "Löysit affiksien uudelleenrullausmateriaalia! (" + affixRerollMaterial + " kpl)"; noticeUntil = Elapsed + 6;
        }
    }
}
