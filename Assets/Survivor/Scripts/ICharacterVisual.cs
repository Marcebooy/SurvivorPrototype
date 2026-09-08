namespace BonkSurvivor
{
    // Shared surface for playable-character visuals (PottuVisual, VelhoVisual) so movement/combat
    // code can drive whichever character is currently selected without knowing its concrete type.
    public interface ICharacterVisual
    {
        void Tick(float dt, float speed, float roll, float size, bool showMeleeWeapon, float healthFraction);
        void ResetPose();
        void Swing(UnityEngine.Vector3 worldAim);
        void Bonk();
        void Block();
        void Shockwave();
    }
}
