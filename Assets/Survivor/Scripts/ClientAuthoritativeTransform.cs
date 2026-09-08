namespace BonkSurvivor
{
    // The joining player's own machine drives this transform so their movement feels
    // instant instead of waiting on a host round-trip; NetworkTransform interpolates it
    // for everyone else. Host-spawned objects (enemies) keep the default server-authoritative transform.
    public sealed class ClientAuthoritativeTransform : Unity.Netcode.Components.NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
