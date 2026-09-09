using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    // Map inventory: how many "Karttaesine" (map item) the player owns per (MapType, tier).
    // Tier is fixed at 1 for now (no tier system yet) but the (type, tier) key shape already
    // supports it later without a rewrite. Procedural is free/unlimited and never stored here.
    public sealed partial class SurvivorGame
    {
        readonly Dictionary<(MapType type, int tier), int> mapItems = new Dictionary<(MapType, int), int>();

        static string MapItemKey(MapType type, int tier) => SaveKey + "MapItem_" + type + "_" + tier;

        public int MapItemCount(MapType type, int tier = 1) => mapItems.TryGetValue((type, tier), out int count) ? count : 0;

        public static string MapTypeName(MapType type) => type switch
        {
            MapType.Mosswood => "Mosswood",
            MapType.BoneCave => "Luuluola",
            MapType.AshWastes => "Tuhkaerämaa",
            _ => "Satunnainen",
        };

        void LoadMapInventory()
        {
            mapItems.Clear();
            mapItems[(MapType.Mosswood, 1)] = PlayerPrefs.GetInt(MapItemKey(MapType.Mosswood, 1), 0);
            mapItems[(MapType.BoneCave, 1)] = PlayerPrefs.GetInt(MapItemKey(MapType.BoneCave, 1), 0);
            mapItems[(MapType.AshWastes, 1)] = PlayerPrefs.GetInt(MapItemKey(MapType.AshWastes, 1), 0);
        }

        void GrantMapItem(MapType type, int tier = 1)
        {
            int count = MapItemCount(type, tier) + 1;
            mapItems[(type, tier)] = count;
            PlayerPrefs.SetInt(MapItemKey(type, tier), count); PlayerPrefs.Save();
        }

        // Returns false (and leaves the inventory untouched) if the player owns none.
        bool ConsumeMapItem(MapType type, int tier = 1)
        {
            int count = MapItemCount(type, tier);
            if (count <= 0) return false;
            count--;
            mapItems[(type, tier)] = count;
            PlayerPrefs.SetInt(MapItemKey(type, tier), count); PlayerPrefs.Save();
            return true;
        }

        // Non-procedural map types that can drop as a Karttaesine - Procedural is already
        // free/unlimited so it's excluded. AshWastes joined Mosswood/BoneCave here (kohta 9);
        // adding a fixed map only ever means adding its MapType value to this one array.
        static readonly MapType[] DroppableMapTypes = { MapType.Mosswood, MapType.BoneCave, MapType.AshWastes };

        // 18% boss-kill chance, picked evenly between the non-procedural map types. Tier rolls
        // 1-3, weighted toward the low end (60/30/10%) so higher-Tier maps stay rare. Uses the
        // same `notice` banner as other run events (e.g. "ALUE X" on area advance).
        void TryDropMapItem()
        {
            if (Random.value >= .18f) return;
            var type = DroppableMapTypes[Random.Range(0, DroppableMapTypes.Length)];
            int tier = RollMapTier();
            GrantMapItem(type, tier);
            notice = "Löysit Karttaesineen: " + MapTypeName(type) + " T" + tier + "!"; noticeUntil = Elapsed + 6;
        }

        static int RollMapTier()
        {
            float roll = Random.value;
            if (roll < .60f) return 1;
            if (roll < .90f) return 2;
            return 3;
        }

        // Tiivis yhteenveto HAHMOT-tabin hahmoprofiiliin - ei toista koko MAPIT-tabin taulukkoa.
        public string MapInventorySummary()
        {
            var parts = new List<string>();
            foreach (var type in DroppableMapTypes)
                for (int tier = 1; tier <= 3; tier++)
                {
                    int count = MapItemCount(type, tier);
                    if (count > 0) parts.Add(MapTypeName(type) + " T" + tier + "×" + count);
                }
            return parts.Count > 0 ? string.Join(", ", parts) : "Ei karttoja vielä";
        }
    }
}
