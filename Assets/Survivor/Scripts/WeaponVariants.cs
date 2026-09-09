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
    }
}
