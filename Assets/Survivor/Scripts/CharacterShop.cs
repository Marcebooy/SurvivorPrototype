using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    // Per-character weapon/Tome ownership + shop (Silver, main menu HAHMOT tab). Feeds the
    // Map-mode loadout picker (DrawLoadoutSelect, SurvivorProgression.cs) - Survival mode never
    // reads any of this and keeps offering the full roster regardless of ownership.
    public sealed partial class SurvivorGame
    {
        readonly Dictionary<PlayerCharacter, HashSet<int>> ownedUpgrades = new Dictionary<PlayerCharacter, HashSet<int>>();
        PlayerCharacter shopCharacter = PlayerCharacter.Pottu;

        // Every character's fixed fights-with-this weapon (matches SelectCharacter's auto-assign
        // in SurvivorPottu.cs) is owned for free from the start. Pottu has no fixed weapon in
        // Survival (30-weapon free pick) so Sword is used as a sane starting default here.
        static int DefaultWeaponIndex(PlayerCharacter character) => character switch
        {
            PlayerCharacter.Velho => (int)Weapon.Lightning,
            PlayerCharacter.Ritari => (int)Weapon.Sword,
            PlayerCharacter.Necromancer => (int)Weapon.Bone,
            PlayerCharacter.Berserker => (int)Weapon.CorruptedSword,
            PlayerCharacter.Golem => (int)Weapon.Chunkers,
            PlayerCharacter.Hunter => (int)Weapon.Bow,
            PlayerCharacter.Ninja => (int)Weapon.Katana,
            PlayerCharacter.Paladin => (int)Weapon.Aura,
            _ => (int)Weapon.Sword,
        };

        static string OwnedKey(PlayerCharacter character) => SaveKey + "Owned_" + character;

        void LoadCharacterShop()
        {
            Silver = PlayerPrefs.GetInt(SaveKey + "Silver", 0); // menu (HAHMOT tab) needs this before any run has called ResetProgression this session
            ownedUpgrades.Clear();
            foreach (PlayerCharacter character in System.Enum.GetValues(typeof(PlayerCharacter)))
            {
                var set = new HashSet<int>();
                string saved = PlayerPrefs.GetString(OwnedKey(character), "");
                if (!string.IsNullOrEmpty(saved))
                    foreach (var part in saved.Split(',')) if (int.TryParse(part, out int id)) set.Add(id);
                set.Add(WeaponUpgradeId(DefaultWeaponIndex(character))); // always-owned default starter, never lost
                ownedUpgrades[character] = set;
            }
        }

        public bool IsUpgradeOwned(PlayerCharacter character, int id) => ownedUpgrades.TryGetValue(character, out var set) && set.Contains(id);

        public IReadOnlyCollection<int> OwnedUpgradesFor(PlayerCharacter character) => ownedUpgrades.TryGetValue(character, out var set) ? set : System.Array.Empty<int>();

        // Increasing Silver price per already-owned item for that character, same style as BuyLegacyHealth's "20 + LegacyHealth*10".
        public int UpgradePrice(PlayerCharacter character) => 25 + (ownedUpgrades.TryGetValue(character, out var set) ? set.Count : 0) * 20;

        public bool BuyCharacterUpgrade(PlayerCharacter character, int id)
        {
            if (IsUpgradeOwned(character, id)) return false;
            int price = UpgradePrice(character);
            if (Silver < price) return false;
            Silver -= price;
            ownedUpgrades[character].Add(id);
            PlayerPrefs.SetInt(SaveKey + "Silver", Silver);
            PlayerPrefs.SetString(OwnedKey(character), string.Join(",", ownedUpgrades[character]));
            PlayerPrefs.Save();
            return true;
        }

        void DrawCharacterTab()
        {
            GUI.Label(new Rect(40,150,1200,40), "HAHMOT", titleStyle);
            GUI.Label(new Rect(40,192,1200,26), "Osta lisää aseita ja Tomeja Silverillä hahmokohtaisesti - Mappi-tilan loadout kootaan vain siitä mitä hahmo omistaa.", textStyle);
            const float cx0 = 40, cw = 138, ch = 32, cgap = 6;
            for (int i = 0; i < CharacterRoster.Length; i++)
            {
                var entry = CharacterRoster[i];
                bool selected = shopCharacter == entry.character;
                GUI.color = selected ? new Color(.3f,.85f,1) : Color.white;
                if (GUI.Button(new Rect(cx0+i*(cw+cgap), 226, cw, ch), entry.name, cardTextStyle)) shopCharacter = entry.character;
            }
            GUI.color = Color.white;
            GUI.Label(new Rect(40,266,900,26), CharacterRosterName(shopCharacter) + "  •  Mastery " + MasteryLevelFor(shopCharacter) + "  •  SILVER " + Silver, textStyle);

            GUI.Label(new Rect(40,300,600,24), "ASEET", textStyle);
            const int cols = 6; const float bw = 195, bh = 32, gx = 8, gy = 6, x0 = 40; const float y0 = 326;
            for (int i = 0; i < TotalWeaponCount; i++)
            {
                int col = i % cols, row = i / cols;
                DrawShopSlot(new Rect(x0+col*(bw+gx), y0+row*(bh+gy), bw, bh), WeaponUpgradeId(i), WeaponLabel((Weapon)i));
            }
            float tomesY = y0 + 5*(bh+gy) + 16;
            GUI.Label(new Rect(40,tomesY,600,24), "TOMET", textStyle);
            const int tcols = 3; const float tbw = 390, tbh = 32;
            for (int i = 0; i < TomeIds.Length; i++)
            {
                int col = i % tcols, row = i / tcols;
                int id = TomeIds[i];
                DrawShopSlot(new Rect(x0+col*(tbw+gx), tomesY+26+row*(tbh+gy), tbw, tbh), id, UpgradeNames[id]);
            }
        }

        static string CharacterRosterName(PlayerCharacter character)
        {
            foreach (var entry in CharacterRoster) if (entry.character == character) return entry.name;
            return character.ToString();
        }

        void DrawShopSlot(Rect rect, int id, string label)
        {
            if (IsUpgradeOwned(shopCharacter, id))
            {
                GUI.enabled = false;
                GUI.Button(rect, label + "  (omistettu)", cardTextStyle);
                GUI.enabled = true;
                return;
            }
            int price = UpgradePrice(shopCharacter);
            GUI.enabled = Silver >= price;
            if (GUI.Button(rect, label + "  " + price + "g", cardTextStyle)) BuyCharacterUpgrade(shopCharacter, id);
            GUI.enabled = true;
        }
    }
}
