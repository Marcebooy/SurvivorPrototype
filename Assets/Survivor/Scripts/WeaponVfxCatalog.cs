using UnityEngine;
namespace BonkSurvivor
{
    public sealed class WeaponVfxCatalog : ScriptableObject
    {
        [System.Serializable] public sealed class Entry
        {
            public SurvivorGame.Weapon weapon;
            public GameObject impact, flight, zone;
            public string description;
        }
        public Entry[] entries;
        public Texture2D groundTexture;
        public Entry Find(SurvivorGame.Weapon weapon) => entries != null && (int)weapon < entries.Length ? entries[(int)weapon] : null;
    }
}
