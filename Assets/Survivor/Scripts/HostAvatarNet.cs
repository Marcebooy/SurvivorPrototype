using Unity.Netcode;
using UnityEngine;

namespace BonkSurvivor
{
    // A replicated stand-in for the host's own player body. The host's real `player` Transform
    // (built procedurally in BuildPottu, driven by the full single-player movement/dodge/camera
    // code) is never itself a NetworkObject - it exists before any network session does, and
    // making it one would risk the host's own gameplay. Instead SurvivorNetwork spawns one of
    // these when hosting starts, and the host mirrors its own position/character onto it every
    // frame; NetworkTransform (server-authoritative, the default - fine since only the host ever
    // writes to it) replicates the position, and the SelectedCharacter NetworkVariable lets every
    // peer build the same deterministic body locally (see RebuildVisual, same trick as RemotePlayerNet).
    public sealed class HostAvatarNet : NetworkBehaviour
    {
        public readonly NetworkVariable<int> SelectedCharacter = new NetworkVariable<int>();
        Transform visualRoot;
        int builtFor = -1;

        public override void OnNetworkSpawn()
        {
            SelectedCharacter.OnValueChanged += (_, __) => RebuildVisual();
            RebuildVisual();
        }

        void RebuildVisual()
        {
            if (builtFor == SelectedCharacter.Value && visualRoot) return;
            if (visualRoot) Destroy(visualRoot.gameObject);
            SurvivorGame.BuildCharacterVisualOn(transform, (SurvivorGame.PlayerCharacter)SelectedCharacter.Value, out visualRoot);
            builtFor = SelectedCharacter.Value;
        }

        // Host-only: called every frame from SurvivorGame while a session is hosted.
        public void MirrorHost(Vector3 position, Quaternion rotation, SurvivorGame.PlayerCharacter character)
        {
            transform.SetPositionAndRotation(position, rotation);
            if (SelectedCharacter.Value != (int)character) SelectedCharacter.Value = (int)character;
        }
    }
}
