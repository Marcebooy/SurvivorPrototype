using System;
using UnityEngine;

namespace BonkSurvivor
{
    // Combat code (AttackWeapons, CastArsenal, CastAdvanced, ...) only ever runs on the host, so
    // discrete animation triggers (Swing/Bonk/Block/Shockwave) only ever fire on whichever local
    // ICharacterVisual instance the host happens to be looking at - never on a remote peer's own
    // copy of the same body. This wraps a locally-built visual so those calls also broadcast (via
    // the owning NetworkBehaviour's RPC) and replay on every other peer's own local copy.
    // Continuous Tick()/ResetPose() are NOT relayed - each peer drives its own copy's walk-cycle
    // locally from observed movement (see RemotePlayerNet/HostAvatarNet's own Update()).
    public sealed class TriggerRelayVisual : ICharacterVisual
    {
        readonly ICharacterVisual local;
        readonly Action<int, Vector3> broadcast;

        public TriggerRelayVisual(ICharacterVisual local, Action<int, Vector3> broadcast)
        {
            this.local = local; this.broadcast = broadcast;
        }

        public void Tick(float dt, float speed, float roll, float size, bool showMeleeWeapon, float healthFraction) { }
        public void ResetPose() => local.ResetPose();
        public void Swing(Vector3 worldAim) { local.Swing(worldAim); broadcast(0, worldAim); }
        public void Bonk() { local.Bonk(); broadcast(1, default); }
        public void Block() { local.Block(); broadcast(2, default); }
        public void Shockwave() { local.Shockwave(); broadcast(3, default); }
        public Transform GetSocket(AttachmentSocket socket) => local.GetSocket(socket);

        public static void Apply(ICharacterVisual visual, int kind, Vector3 aim)
        {
            switch (kind)
            {
                case 0: visual.Swing(aim); break;
                case 1: visual.Bonk(); break;
                case 2: visual.Block(); break;
                case 3: visual.Shockwave(); break;
            }
        }
    }
}
