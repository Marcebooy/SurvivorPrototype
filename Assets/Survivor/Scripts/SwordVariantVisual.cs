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
    // KNOWN LIMITATION: no playable character has a real hand/weapon socket yet (Pottu's melee
    // weapon is a procedural frying pan, Ritari's Sword is baked into his rigged FBX mesh) -
    // both models are parented directly to the player body at a fixed approximate offset instead.
    // Replace SwordAttachLocalPosition/Rotation with a proper socket transform once characters
    // gain a real weapon-attachment point.
    public sealed partial class SurvivorGame
    {
        static readonly Vector3 SwordAttachLocalPosition = new Vector3(.55f, .95f, .25f);
        static readonly Quaternion SwordAttachLocalRotation = Quaternion.Euler(0, 25, 70);

        Transform swordVariantModel, kaksoisteraVariantModel;

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
            instance.transform.localPosition = SwordAttachLocalPosition;
            instance.transform.localRotation = SwordAttachLocalRotation;
            instance.SetActive(false);
            return instance.transform;
        }

        // Ticked every frame from Update() alongside the rest of the run simulation - two
        // SetActive calls, no allocation, independent of which character/visual is active.
        void TickSwordVariantVisual()
        {
            EnsureSwordVariantVisual();
            if (!swordVariantModel && !kaksoisteraVariantModel) return;
            bool hasSword = weaponLevels.ContainsKey(Weapon.Sword);
            bool kaksoisteraActive = hasSword && ActiveVariant(Weapon.Sword) == 1;
            if (swordVariantModel) swordVariantModel.gameObject.SetActive(hasSword && !kaksoisteraActive);
            if (kaksoisteraVariantModel) kaksoisteraVariantModel.gameObject.SetActive(kaksoisteraActive);
        }
    }
}
