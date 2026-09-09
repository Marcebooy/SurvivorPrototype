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
        Vector2 characterTabScroll;

        // Pysyvä oletusloadout per hahmo - sama id-muoto kuin SurvivorProgression.cs:n mapLoadout
        // (aseet WeaponUpgradeId:n kautta, Tomet suoraan). Esitäyttää DrawLoadoutSelectin
        // loadoutWeapons/loadoutTomes-staging-setit ResetProgressionissa, mutta on vain lähtökohta -
        // ConfirmLoadout/SkipLoadout eivät koskaan kirjoita tänne takaisin, joten run-kohtainen
        // muokkaus Mappi-runin alkuruudussa ei muuta tätä pysyvää oletusta.
        readonly Dictionary<PlayerCharacter, HashSet<int>> defaultLoadout = new Dictionary<PlayerCharacter, HashSet<int>>();

        static string DefaultLoadoutKey(PlayerCharacter c) => SaveKey + "DefaultLoadout_" + c;

        public IReadOnlyCollection<int> DefaultLoadoutFor(PlayerCharacter c) => defaultLoadout.TryGetValue(c, out var set) ? set : System.Array.Empty<int>();

        bool IsInDefaultLoadout(PlayerCharacter c, int id) => defaultLoadout.TryGetValue(c, out var set) && set.Contains(id);

        void ToggleDefaultLoadout(PlayerCharacter c, int id)
        {
            if (!defaultLoadout.TryGetValue(c, out var set)) defaultLoadout[c] = set = new HashSet<int>();
            if (set.Contains(id)) set.Remove(id);
            else
            {
                int cap = IsTomeId(id) ? MaxTomeKinds : MaxWeaponKinds;
                int currentCount = 0;
                foreach (var existing in set) if (IsTomeId(existing) == IsTomeId(id)) currentCount++;
                if (currentCount >= cap) return;
                set.Add(id);
            }
            SaveDefaultLoadout(c);
        }

        void SaveDefaultLoadout(PlayerCharacter c)
        {
            var set = defaultLoadout.TryGetValue(c, out var s) ? s : null;
            PlayerPrefs.SetString(DefaultLoadoutKey(c), set == null ? "" : string.Join(",", set));
            PlayerPrefs.Save();
        }

        void LoadDefaultLoadout()
        {
            defaultLoadout.Clear();
            foreach (PlayerCharacter c in System.Enum.GetValues(typeof(PlayerCharacter)))
            {
                string saved = PlayerPrefs.GetString(DefaultLoadoutKey(c), "");
                if (string.IsNullOrEmpty(saved)) continue;
                var set = new HashSet<int>();
                foreach (var part in saved.Split(',')) if (int.TryParse(part, out int id)) set.Add(id);
                if (set.Count > 0) defaultLoadout[c] = set;
            }
        }

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
            // Sisältö kasvoi kaupan lisäksi karttavarasto-yhteenvedolla ja oletusloadout-osiolla,
            // eikä enää mahdu yhdelle 720px-korkealle ruudulle - koko välilehti on nyt vieritettävä.
            // ScrollView-position on koko näytön kokoinen (0,0,1280,720), joten kaikki alkuperäiset
            // absoluuttiset koordinaatit (150, 192, 226, ... y0=326 jne.) pysyvät muuttumattomina;
            // DrawMenuTabs() (yläpalkin välilehdet) piirretään erikseen tämän ulkopuolella, joten
            // ScrollView ei vaikuta niihin.
            var viewport = new Rect(0, 0, 1280, 720);
            var content = new Rect(0, 0, 1260, 2000);
            characterTabScroll = GUI.BeginScrollView(viewport, characterTabScroll, content);

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
            int tomeRows = Mathf.CeilToInt(TomeIds.Length / (float)tcols);
            float profileY = tomesY + 26 + tomeRows*(tbh+gy) + 24;

            GUI.Label(new Rect(40, profileY, 1200, 24), "KARTTAVARASTO: " + MapInventorySummary(), textStyle);
            profileY += 34;

            int defaultWeaponCount = 0, defaultTomeCount = 0;
            foreach (var id in DefaultLoadoutFor(shopCharacter)) { if (IsTomeId(id)) defaultTomeCount++; else defaultWeaponCount++; }
            GUI.Label(new Rect(40, profileY, 1200, 24),
                "OLETUSLOADOUT (" + defaultWeaponCount + "/" + MaxWeaponKinds + " asetta, " + defaultTomeCount + "/" + MaxTomeKinds + " Tomea - esitäyttää Mappi-runin loadout-ruudun, muokattavissa vielä siellä)",
                textStyle);
            profileY += 30;
            DrawDefaultLoadoutSection(40, profileY);

            GUI.EndScrollView();
        }

        // Sama grid-tyyli kuin DrawLoadoutSelectissa (SurvivorProgression.cs), mutta kirjoittaa
        // pysyvään defaultLoadoutiin loadoutWeapons/loadoutTomes-staging-setin sijaan, ja käyttää
        // shopCharacteria (HAHMOT-tabin valinta) SelectedCharacterin (aktiivinen run) sijaan.
        void DrawDefaultLoadoutSection(float x0, float y0)
        {
            var ownedWeapons = new List<int>();
            var ownedTomes = new List<int>();
            foreach (var id in OwnedUpgradesFor(shopCharacter)) { if (IsTomeId(id)) ownedTomes.Add(id); else ownedWeapons.Add(id); }
            ownedWeapons.Sort(); ownedTomes.Sort();

            const int cols = 6; const float bw = 195, bh = 34, gx = 8, gy = 6;
            const float variantRowH = 20, qualityRowH = 20; const float affixInfoRowH = 18, affixButtonRowH = 20;
            const float rowPitch = bh + variantRowH + qualityRowH + affixInfoRowH + affixButtonRowH + gy;
            for (int i = 0; i < ownedWeapons.Count; i++)
            {
                int col = i % cols, row = i / cols;
                int upgradeId = ownedWeapons[i];
                var weapon = IdToWeapon(upgradeId);
                bool selected = IsInDefaultLoadout(shopCharacter, upgradeId);
                float bx = x0+col*(bw+gx), by = y0+row*rowPitch;
                GUI.color = selected ? new Color(.3f,.85f,1) : Color.white;
                if (GUI.Button(new Rect(bx, by, bw, bh), WeaponLabel(weapon), cardTextStyle)) ToggleDefaultLoadout(shopCharacter, upgradeId);
                if (IsVariantUnlocked(weapon, 1))
                {
                    bool variantOn = ActiveVariantFor(shopCharacter, weapon) == 1;
                    int quality = VariantQuality(weapon, 1);
                    string stars = new string('★', quality) + new string('☆', 3 - quality);
                    GUI.color = variantOn ? new Color(1f,.75f,.25f) : new Color(.55f,.55f,.55f);
                    if (GUI.Button(new Rect(bx, by+bh+2, bw, variantRowH-2), VariantName(weapon,1) + " " + stars + (variantOn ? " (PÄÄLLÄ)" : ""), cardTextStyle))
                        SetActiveVariant(shopCharacter, weapon, variantOn ? 0 : 1);
                    if (quality < 3)
                    {
                        int cost = VariantQualityUpgradeCost(weapon, 1);
                        GUI.color = Color.white; GUI.enabled = QualityMaterial >= cost;
                        if (GUI.Button(new Rect(bx, by+bh+variantRowH+4, bw, qualityRowH-2), "Paranna (" + cost + " kpl)", cardTextStyle))
                            UpgradeVariantQuality(shopCharacter, weapon, 1);
                        GUI.enabled = true;
                    }
                }
                if (AffixPool.ContainsKey(weapon))
                {
                    var affixes = AffixesFor(shopCharacter, weapon);
                    string affixText = "Affiksit: ";
                    for (int a = 0; a < affixes.Count; a++)
                        affixText += (a > 0 ? "  " : "") + AffixStatLabel(affixes[a].stat) + " +" + Mathf.RoundToInt(affixes[a].percent) + "%";
                    if (affixes.Count == 0) affixText += "-";
                    GUI.color = Color.white;
                    float infoY = by + bh + variantRowH + qualityRowH + 2;
                    GUI.Label(new Rect(bx, infoY, bw, affixInfoRowH), affixText, cardTextStyle);
                    float aby = infoY + affixInfoRowH;
                    bool canAdd = affixAddMaterial > 0 && affixes.Count < MaxAffixSlots;
                    GUI.color = canAdd ? Color.white : new Color(.5f,.5f,.5f);
                    GUI.enabled = canAdd;
                    if (GUI.Button(new Rect(bx, aby, bw/2f-2, affixButtonRowH-2), "Lisää (" + affixAddMaterial + ")", cardTextStyle))
                        TryAddAffix(shopCharacter, weapon);
                    bool canReroll = affixRerollMaterial > 0;
                    GUI.color = canReroll ? Color.white : new Color(.5f,.5f,.5f);
                    GUI.enabled = canReroll;
                    if (GUI.Button(new Rect(bx+bw/2f+2, aby, bw/2f-2, affixButtonRowH-2), "Rulla (" + affixRerollMaterial + ")", cardTextStyle))
                        TryRerollAffixes(shopCharacter, weapon);
                    GUI.enabled = true;
                }
                GUI.color = Color.white;
            }
            int weaponRows = Mathf.Max(1, Mathf.CeilToInt(ownedWeapons.Count / (float)cols));
            float tomesY = y0 + weaponRows*rowPitch + 14;

            const int tcols = 3; const float tbw = 390, tbh = 34;
            for (int i = 0; i < ownedTomes.Count; i++)
            {
                int col = i % tcols, row = i / tcols;
                int id = ownedTomes[i];
                bool selected = IsInDefaultLoadout(shopCharacter, id);
                GUI.color = selected ? new Color(.3f,.85f,1) : Color.white;
                if (GUI.Button(new Rect(x0+col*(tbw+gx), tomesY+row*(tbh+gy), tbw, tbh), UpgradeNames[id], cardTextStyle)) ToggleDefaultLoadout(shopCharacter, id);
            }
            GUI.color = Color.white;
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
