using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    // Ase-variantit (pilotti: Sword, Bow, Chunkers). Sama ase, vaihtoehtoinen käytös - ei vie
    // omaa loadout-paikkaa (SurvivorProgression.cs: loadoutWeapons/mapLoadout tallentavat vain
    // perusaseen upgradeId:n, ei variantin tilaa). Avataan pysyvästi pelaamalla (bossinkaatodroppi,
    // TryDropWeaponVariant kutsuttuna SurvivorGame.Hit()-metodista TryDropMapItemin vierestä),
    // ei ostamalla Silverillä (CharacterShop.cs, HAHMOT-tabi koskematon). Vain Mappi-tilassa -
    // Selviytymistila (Procedural) ei koskaan aseta tai lue activeVariant-arvoa, joten sen käytös
    // pysyy täysin muuttumattomana.
    public sealed partial class SurvivorGame
    {
        // Pilottivaiheessa yksi muunnos per ase (variantti-indeksi 1). 0 = perusase (aina "omistettu").
        static readonly Weapon[] PilotVariantWeapons = { Weapon.Sword, Weapon.Bow, Weapon.Chunkers };

        static string VariantName(Weapon w, int v) => w switch
        {
            Weapon.Sword => "Kaksoisterä",
            Weapon.Bow => "Kimmokaari",
            Weapon.Chunkers => "Iskukivet",
            _ => "Muunnos " + v,
        };

        readonly HashSet<(Weapon weapon, int variant)> unlockedVariants = new HashSet<(Weapon, int)>();
        // Per hahmo, ei per save-tiedosto - eri hahmot voivat pitää eri variantin päällä samalle aseelle.
        readonly Dictionary<PlayerCharacter, Dictionary<Weapon, int>> activeVariantByCharacter = new Dictionary<PlayerCharacter, Dictionary<Weapon, int>>();

        static string VariantUnlockKey(Weapon w, int v) => SaveKey + "VariantUnlocked_" + w + "_" + v;
        static string ActiveVariantKey(PlayerCharacter c) => SaveKey + "ActiveVariant_" + c;

        public bool IsVariantUnlocked(Weapon w, int v = 1) => unlockedVariants.Contains((w, v));

        // Taisteluikoodi lukee aina nykyistä pelattavaa hahmoa.
        int ActiveVariant(Weapon w) => ActiveVariantFor(SelectedCharacter, w);

        public int ActiveVariantFor(PlayerCharacter c, Weapon w) =>
            activeVariantByCharacter.TryGetValue(c, out var map) && map.TryGetValue(w, out int v) ? v : 0;

        // Ei vie loadout-paikkaa eikä vaadi aseen olevan valittuna loadoutissa juuri nyt - valinta
        // säilyy tallessa vaikka ase poistettaisiin loadoutista, mutta vaikuttaa taisteluun vasta
        // kun weaponLevels sisältää kyseisen aseen jälleen (ActiveVariant-kutsut taisteluikoodissa
        // ovat aina weaponLevels.TryGetValue-tarkistuksen sisällä, joten tämä ei vaadi erillistä sulkua).
        public void SetActiveVariant(PlayerCharacter c, Weapon w, int v)
        {
            if (v != 0 && !IsVariantUnlocked(w, v)) return;
            if (!activeVariantByCharacter.TryGetValue(c, out var map)) activeVariantByCharacter[c] = map = new Dictionary<Weapon, int>();
            if (v == 0) map.Remove(w); else map[w] = v;
            SaveActiveVariants(c);
        }

        void SaveActiveVariants(PlayerCharacter c)
        {
            var parts = new List<string>();
            if (activeVariantByCharacter.TryGetValue(c, out var map))
                foreach (var kv in map) if (kv.Value != 0) parts.Add(kv.Key + "=" + kv.Value);
            PlayerPrefs.SetString(ActiveVariantKey(c), string.Join(";", parts)); PlayerPrefs.Save();
        }

        void LoadWeaponVariants()
        {
            unlockedVariants.Clear();
            foreach (var w in PilotVariantWeapons)
                if (PlayerPrefs.GetInt(VariantUnlockKey(w, 1), 0) == 1) unlockedVariants.Add((w, 1));
            activeVariantByCharacter.Clear();
            foreach (PlayerCharacter c in System.Enum.GetValues(typeof(PlayerCharacter)))
            {
                string saved = PlayerPrefs.GetString(ActiveVariantKey(c), "");
                if (string.IsNullOrEmpty(saved)) continue;
                var map = new Dictionary<Weapon, int>();
                foreach (var part in saved.Split(';'))
                {
                    var kv = part.Split('=');
                    if (kv.Length == 2 && System.Enum.TryParse(kv[0], out Weapon w) && int.TryParse(kv[1], out int v) && v != 0)
                        map[w] = v;
                }
                if (map.Count > 0) activeVariantByCharacter[c] = map;
            }
        }

        void GrantVariant(Weapon w, int v = 1)
        {
            if (!unlockedVariants.Add((w, v))) return;
            PlayerPrefs.SetInt(VariantUnlockKey(w, v), 1); PlayerPrefs.Save();
        }

        // Oma, mapItemistä riippumaton 10 % bossinkaatorulla. Vain Mappi-tilassa (activeMapType !=
        // Procedural) - Selviytymistila ei koskaan kutsu tätä polkua miltään osin. Rajattu aseisiin
        // jotka pelaaja jo omistaa CharacterShopin (HAHMOT-tabi) kautta, jotta droppi ei koskaan
        // avaa muunnosta aseelle jota ei ole vielä edes ostettu peruspelissä.
        void TryDropWeaponVariant()
        {
            if (activeMapType == MapType.Procedural) return;
            if (Random.value >= .10f) return;
            var candidates = new List<Weapon>();
            foreach (var w in PilotVariantWeapons)
                if (!IsVariantUnlocked(w, 1) && IsUpgradeOwned(SelectedCharacter, WeaponUpgradeId((int)w))) candidates.Add(w);
            if (candidates.Count == 0) return;
            var picked = candidates[Random.Range(0, candidates.Count)];
            GrantVariant(picked, 1);
            notice = "Löysit aseen muunnoksen: " + WeaponLabel(picked) + " - " + VariantName(picked, 1) + "!"; noticeUntil = Elapsed + 6;
        }

        // Variantin laatutaso (1-3 tähteä) - parantaa jo avattua varianttia, EI avaa uutta.
        // Globaali kuten unlockedVariants (ei per hahmo, koska kyse on itse esineen ominaisuudesta
        // eikä pelaajan sen-hetkisestä valinnasta). Laatu alkaa tähdestä 1 heti kun variantti on
        // avattu - erillistä "tähti 0" -tilaa ei ole.
        readonly Dictionary<(Weapon weapon, int variant), int> variantQuality = new Dictionary<(Weapon, int), int>();
        int qualityMaterial;

        static string VariantQualityKey(Weapon w, int v) => SaveKey + "VariantQuality_" + w + "_" + v;
        const string QualityMaterialKey = SaveKey + "QualityMaterial";

        public int VariantQuality(Weapon w, int v = 1) => variantQuality.TryGetValue((w, v), out int q) ? q : 1;
        public int QualityMaterial => qualityMaterial;

        // Tähti 1=1.0x, 2=1.1x, 3=1.2x - maltillinen, ei tuplaa varianttikohtaista tehoa.
        float VariantQualityMultiplier(Weapon w, int v = 1) => 1 + .1f * (VariantQuality(w, v) - 1);

        // Kasvava hinta kuten UpgradePricessa: 1 kpl 1->2, 2 kpl 2->3.
        public int VariantQualityUpgradeCost(Weapon w, int v = 1) => VariantQuality(w, v);

        public bool UpgradeVariantQuality(PlayerCharacter c, Weapon w, int v = 1)
        {
            if (!IsUpgradeOwned(c, WeaponUpgradeId((int)w)) || !IsVariantUnlocked(w, v)) return false;
            int quality = VariantQuality(w, v);
            if (quality >= 3) return false;
            int cost = VariantQualityUpgradeCost(w, v);
            if (qualityMaterial < cost) return false;
            qualityMaterial -= cost;
            variantQuality[(w, v)] = quality + 1;
            PlayerPrefs.SetInt(QualityMaterialKey, qualityMaterial);
            PlayerPrefs.SetInt(VariantQualityKey(w, v), quality + 1);
            PlayerPrefs.Save();
            return true;
        }

        void LoadVariantQuality()
        {
            variantQuality.Clear();
            foreach (var w in PilotVariantWeapons)
            {
                int saved = PlayerPrefs.GetInt(VariantQualityKey(w, 1), 1);
                if (saved > 1) variantQuality[(w, 1)] = Mathf.Clamp(saved, 1, 3);
            }
            qualityMaterial = PlayerPrefs.GetInt(QualityMaterialKey, 0);
        }

        // Kolmas, mapItemistä ja TryDropWeaponVariantista täysin riippumaton 15 % bossinkaatorulla.
        // Vain Mappi-tilassa. Droppaa vain jos jollain omistetulla ja jo avatulla variantilla on
        // vielä tilaa parantua (laatu < 3) - muuten materiaali olisi hetkessä hyödytöntä.
        void TryDropQualityMaterial()
        {
            if (activeMapType == MapType.Procedural) return;
            if (Random.value >= .15f) return;
            bool anyUpgradable = false;
            foreach (var w in PilotVariantWeapons)
                if (IsVariantUnlocked(w, 1) && IsUpgradeOwned(SelectedCharacter, WeaponUpgradeId((int)w)) && VariantQuality(w, 1) < 3) { anyUpgradable = true; break; }
            if (!anyUpgradable) return;
            qualityMaterial++;
            PlayerPrefs.SetInt(QualityMaterialKey, qualityMaterial); PlayerPrefs.Save();
            notice = "Löysit laatumateriaalia! (" + qualityMaterial + " kpl)"; noticeUntil = Elapsed + 6;
        }
    }
}
