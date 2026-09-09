using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    // Purely visual: shows Astra's WeaponArtPilot base model for a weapon while it's equipped,
    // attached via WeaponAttachmentPoint (RightHand) exactly like SwordVariantVisual.cs's
    // Kaksoiskajo model. Character-independent by design - Mappi-tilan omistuspohjainen loadout
    // lets any character equip any weapon. One instance per weapon (not per character); the
    // instance is re-parented onto whichever character currently has it equipped.
    //
    // Covers the 17 weapons whose catalog.json "role" unambiguously matches a Weapon enum value
    // and whose base prefab Astra already delivered (Assets/WeaponArtPilot/Documentation/
    // catalog.json, 2026-09-09 delivery) - moved from Library/Prefabs/ into Library/Prefabs/
    // Resources/ so Resources.Load can find them (they were not under a Resources folder before).
    // See AGENTS.md's Muutosloki for the full list of skipped/ambiguous/missing weapons and why.
    //
    // Two-handed Heavy_ prefabs (only Scythe so far - WarAxe/Hammer have no matching Weapon enum
    // value yet) attach the same way: single RightHand parent, no double-parenting. Their
    // SupportGrip child transform rides along inert - support-hand IK is explicitly out of scope
    // here (CODE_HANDOFF.md #4/#6).
    //
    // KNOWN LIMITATION shared with SwordVariantVisual: a handful of characters carry their
    // starting weapon baked into their rigged FBX mesh with no separate transform to hide
    // (Necromancer's bone staff for Bone, and potentially others found during Play Mode testing -
    // see AGENTS.md) - for those characters the baked mesh and this model render at once. Not
    // fixable without giving them a real detachable part (out of scope here).
    public sealed partial class SurvivorGame
    {
        struct WeaponVisualDef { public Weapon weapon; public string resource; }

        static readonly WeaponVisualDef[] WeaponVisualDefs =
        {
            new WeaponVisualDef { weapon = Weapon.Katana, resource = "Blade_Katana" },
            new WeaponVisualDef { weapon = Weapon.CorruptedSword, resource = "Blade_CorruptedSword" },
            new WeaponVisualDef { weapon = Weapon.Lightning, resource = "Staff_Lightning" },
            new WeaponVisualDef { weapon = Weapon.Firestaff, resource = "Staff_Firestaff" },
            new WeaponVisualDef { weapon = Weapon.Revolver, resource = "Gun_Revolver" },
            new WeaponVisualDef { weapon = Weapon.Shotgun, resource = "Gun_Shotgun" },
            new WeaponVisualDef { weapon = Weapon.Sniper, resource = "Gun_Sniper" },
            new WeaponVisualDef { weapon = Weapon.Rocket, resource = "Gun_Rocket" },
            new WeaponVisualDef { weapon = Weapon.Chunkers, resource = "Relic_Chunkers" },
            new WeaponVisualDef { weapon = Weapon.Dice, resource = "Relic_Dice" },
            new WeaponVisualDef { weapon = Weapon.Bone, resource = "Kit_Bone" },
            new WeaponVisualDef { weapon = Weapon.PoisonFlask, resource = "Kit_PoisonFlask" },
            new WeaponVisualDef { weapon = Weapon.Mines, resource = "Kit_Mine" },
            new WeaponVisualDef { weapon = Weapon.Bananarang, resource = "Kit_Bananarang" },
            new WeaponVisualDef { weapon = Weapon.Axe, resource = "Kit_ThrowingAxe" },
            new WeaponVisualDef { weapon = Weapon.Bow, resource = "Bow_Recurve" },
            new WeaponVisualDef { weapon = Weapon.Scythe, resource = "Heavy_Scythe" },
        };

        readonly Dictionary<Weapon, Transform> weaponVisualModels = new Dictionary<Weapon, Transform>();
        bool weaponVisualsBuilt;
        bool weaponVisualsAttached;
        PlayerCharacter weaponVisualsAttachedTo;

        void EnsureWeaponVisuals()
        {
            if (weaponVisualsBuilt || !player) return;
            weaponVisualsBuilt = true;
            foreach (var def in WeaponVisualDefs)
            {
                var prefab = Resources.Load<GameObject>(def.resource);
                if (!prefab)
                {
                    Debug.LogWarning(def.resource + " not found under a Resources folder - weapon visual skipped for " + def.weapon + ".");
                    continue;
                }
                var instance = Instantiate(prefab, player);
                instance.name = def.resource + " (weapon visual)";
                instance.SetActive(false);
                weaponVisualModels[def.weapon] = instance.transform;
            }
        }

        // Re-parents every loaded model onto the current character's RightHand driver. Same
        // retry-until-it-succeeds shape as SwordVariantVisual.EnsureSwordVariantAttached - a
        // transient miss (characterVisual still loading its FBX) is retried next tick rather than
        // giving up, since WeaponAttachmentPoint.Attach never silently places a model at (0,0,0).
        void EnsureWeaponVisualsAttached()
        {
            if (weaponVisualsAttached && weaponVisualsAttachedTo == SelectedCharacter) return;
            if (characterVisual == null) return;
            bool ok = true;
            foreach (var kv in weaponVisualModels)
                ok &= WeaponAttachmentPoint.Attach(kv.Value, characterVisual, SelectedCharacter, AttachmentSocket.RightHand);
            weaponVisualsAttachedTo = SelectedCharacter;
            weaponVisualsAttached = ok;
        }

        // Ticked every frame from Update(), alongside TickSwordVariantVisual.
        void TickWeaponVisuals()
        {
            EnsureWeaponVisuals();
            if (weaponVisualModels.Count == 0) return;
            EnsureWeaponVisualsAttached();
            foreach (var kv in weaponVisualModels)
                kv.Value.gameObject.SetActive(weaponLevels.ContainsKey(kv.Key));
        }
    }
}
