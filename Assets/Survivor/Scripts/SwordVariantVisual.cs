using UnityEngine;

namespace BonkSurvivor
{
    // Purely visual: shows the Kaksoiskajo Sword model when the base Sword weapon is equipped,
    // and swaps to the Kaksoisterä model when WeaponVariants' variant toggle is active
    // (ActiveVariantFor) - no damage/stat/gameplay logic here, all of that already lives in
    // WeaponVariants.cs. Character-independent by design: the Mappi-tilan omistuspohjainen
    // loadout lets any character equip Sword, so attaching this to one specific character's
    // visual (e.g. only Ritari) would miss the others.
    //
    // Attaches to the equipped character's real RightHand socket via WeaponAttachmentPoint (the
    // WeaponArtPilot delivery, Assets/WeaponArtPilot/Documentation) instead of the old fixed
    // body-relative offset. Re-attached whenever the selected character changes, since the driver
    // bone lives inside characterVisual's own model and gets destroyed/rebuilt with it
    // (BuildCharacterVisual). Kaksoiskajo and its Kaksoisterä variant keep their own separate
    // mesh/material - they are not colour variants of a WeaponArtPilot base family
    // (CODE_HANDOFF.md #7) - only the placement math is shared.
    //
    // KNOWN LIMITATION unchanged from before this system existed: Ritari's Sword is baked into his
    // rigged FBX mesh with no separate transform to hide, so his model and the Kaksoiskajo model
    // render at once for him - not fixable without giving Ritari a real detachable sword part
    // (out of scope here; WeaponArtPilot's pilot did not touch existing character FBX meshes).
    public sealed partial class SurvivorGame
    {
        Transform swordVariantModel, kaksoisteraVariantModel;
        PlayerCharacter swordVariantAttachedTo;
        bool swordVariantAttached;

        void EnsureSwordVariantVisual()
        {
            if (swordVariantModel || kaksoisteraVariantModel || !player) return;
            swordVariantModel = InstantiateSwordModel("Kaksoiskajo_Sword");
            kaksoisteraVariantModel = InstantiateSwordModel("Kaksoiskajo_Kaksoistera");
        }

        Transform InstantiateSwordModel(string resourceName)
        {
            var prefab = Resources.Load<GameObject>(resourceName);
            if (!prefab)
            {
                Debug.LogWarning(resourceName + ".fbx not found under a Resources folder - Sword variant visual skipped for it.");
                return null;
            }
            var instance = Instantiate(prefab, player);
            instance.name = resourceName + " (variant visual)";
            instance.SetActive(false);
            return instance.transform;
        }

        // Re-parents both models onto the current character's RightHand driver. Called once per
        // character build and re-tried every tick while it hasn't succeeded yet (e.g. characterVisual
        // was still loading its FBX the first frame) - WeaponAttachmentPoint.Attach logs an error and
        // returns false rather than leaving a model at a silent (0,0,0), so this keeps retrying
        // instead of giving up on a transient miss.
        void EnsureSwordVariantAttached()
        {
            if (swordVariantAttached && swordVariantAttachedTo == SelectedCharacter) return;
            if (characterVisual == null) return;
            bool ok = true;
            if (swordVariantModel) ok &= WeaponAttachmentPoint.Attach(swordVariantModel, characterVisual, SelectedCharacter, AttachmentSocket.RightHand);
            if (kaksoisteraVariantModel) ok &= WeaponAttachmentPoint.Attach(kaksoisteraVariantModel, characterVisual, SelectedCharacter, AttachmentSocket.RightHand);
            swordVariantAttachedTo = SelectedCharacter;
            swordVariantAttached = ok;
        }

        // Ticked every frame from Update() - before MovePottu/characterVisual.Tick, so
        // HasSwordVariantVisual below already reflects this frame's state (not last frame's) when
        // PottuVisual decides whether to also show its own procedural pan.
        void TickSwordVariantVisual()
        {
            EnsureSwordVariantVisual();
            if (!swordVariantModel && !kaksoisteraVariantModel) return;
            EnsureSwordVariantAttached();
            bool hasSword = weaponLevels.ContainsKey(Weapon.Sword);
            bool kaksoisteraActive = hasSword && ActiveVariant(Weapon.Sword) == 1;
            if (swordVariantModel) swordVariantModel.gameObject.SetActive(hasSword && !kaksoisteraActive);
            if (kaksoisteraVariantModel) kaksoisteraVariantModel.gameObject.SetActive(kaksoisteraActive);
        }

        // True once this shows a Kaksoiskajo model for Sword (base or Kaksoisterä) - used to hide
        // a character's own built-in/procedural Sword visual so the same weapon doesn't render
        // twice. Only meaningful for characters whose weapon visual is actually toggleable
        // (Pottu's frying pan, via PottuVisual's showMeleeWeapon parameter - see SurvivorPottu.cs).
        // KNOWN LIMITATION: Ritari's Sword is baked into his rigged FBX mesh with no separate
        // transform to hide - his RitariVisual.Tick() doesn't use showMeleeWeapon at all, so his
        // model and the Kaksoiskajo model will render at once for him. Not fixable without giving
        // Ritari a real detachable sword part, which is out of scope here.
        public bool HasSwordVariantVisual =>
            (swordVariantModel && swordVariantModel.gameObject.activeSelf) || (kaksoisteraVariantModel && kaksoisteraVariantModel.gameObject.activeSelf);
    }
}
