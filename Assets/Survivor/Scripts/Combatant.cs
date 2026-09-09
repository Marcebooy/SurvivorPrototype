using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    public sealed partial class SurvivorGame
    {
        // All per-player mutable state (stats, weapon ownership, weapon runtime pools, XP/level).
        // SurvivorGame keeps exactly one of these "current" at a time (see `current` field below) -
        // the host's own single-player code is completely unchanged (it always runs with
        // current == hostState); a connected friend gets a second Combatant that the host
        // temporarily makes current while ticking the friend's weapons (see TickNetworkCoop).
        // Nested here (not top-level) so it can freely use the private Enemy/ArsenalShot/etc. types.
        public sealed class Combatant
        {
            public Transform Body;
            public ICharacterVisual CharacterVisual;
            public Transform VisualRoot;
            public PlayerCharacter SelectedCharacter = PlayerCharacter.Pottu;
            public bool AwaitingCharacterChoice;
            public bool Selecting;
            public int StarterPage;

            public float Health = 100f, MaxHealth = 100f;
            public int Level = 1, Coins, Kills, Experience;
            public int PendingChoices;
            public readonly List<int> Choices = new List<int>();

            public float Damage = 18f, AttackRate = 1.3f, MoveSpeed = 8f, PickupRadius = 3.5f;
            public float Size = 1f, CritChance, Armor, EffectDuration = 1f, ProjectileSpeed = 1f;
            public int Quantity = 1;
            public float Invulnerability;
            public float AttackTimer;

            public readonly Dictionary<Weapon, int> WeaponLevels = new Dictionary<Weapon, int>();
            public readonly Dictionary<int, int> TomeLevels = new Dictionary<int, int>();

            internal readonly List<ArsenalShot> Shots = new List<ArsenalShot>();
            internal readonly List<FlamePatch> Flames = new List<FlamePatch>();
            public readonly List<Transform> Rocks = new List<Transform>();
            public LineRenderer AuraRing;
            public Transform AuraEffect;
            public float OrbitAngle, OrbitTick;

            // Yhtenäinen ajastintaulukko (pelaaja-progressio-ja-ranked-suunnitelma.md, kohta 10.5) -
            // yksi slotti per Weapon-enumin arvo (indeksi = (int)Weapon). Korvasi vaiheessa 5 kolme
            // rinnakkaista mallia (attackTimer-skalaari, WeaponTimers[6], AdvancedTimers[21]).
            // Sword/Bow/Lightning jakavat yhä tarkoituksella yhden slotin (Sword-indeksi) - ks.
            // AttackWeapons/SharedAttackTimerAffixMultiplier.
            public readonly float[] AttackTimerSlots = new float[30];
            internal readonly List<AdvancedShot> AdvancedShots = new List<AdvancedShot>();
            internal readonly List<AdvancedZone> AdvancedZones = new List<AdvancedZone>();
            internal readonly Dictionary<Enemy, Chill> Chilled = new Dictionary<Enemy, Chill>();
            public int ShieldCharges, ScytheCharge, BloodKills, BloodHealthGained;
            public int LastDiceRoll;

            public bool IsAlive => Health > 0;
        }
    }
}
