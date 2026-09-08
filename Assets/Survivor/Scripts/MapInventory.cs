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
            _ => "Satunnainen",
        };

        void LoadMapInventory()
        {
            mapItems.Clear();
            mapItems[(MapType.Mosswood, 1)] = PlayerPrefs.GetInt(MapItemKey(MapType.Mosswood, 1), 0);
            mapItems[(MapType.BoneCave, 1)] = PlayerPrefs.GetInt(MapItemKey(MapType.BoneCave, 1), 0);
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

        // 18% boss-kill chance, picked between the two non-procedural map types (Procedural is
        // already free/unlimited so it never drops). Uses the same `notice` banner as other
        // run events (e.g. "ALUE X" on area advance).
        void TryDropMapItem()
        {
            if (Random.value >= .18f) return;
            var type = Random.value < .5f ? MapType.Mosswood : MapType.BoneCave;
            GrantMapItem(type);
            notice = "Löysit Karttaesineen: " + MapTypeName(type) + "!"; noticeUntil = Elapsed + 6;
        }
    }
}
