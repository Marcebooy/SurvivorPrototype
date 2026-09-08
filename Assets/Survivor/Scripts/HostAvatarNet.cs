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
    // The host's OWN local copy hides its renderers, since the host already sees their real body -
    // only other peers (the friend) need to see this mirror.
    public sealed class HostAvatarNet : NetworkBehaviour
    {
        public readonly NetworkVariable<int> SelectedCharacter = new NetworkVariable<int>();
        Transform visualRoot;
        ICharacterVisual visual;
        int builtFor = -1;
        Vector3 lastPos;
        bool hasLastPos;
        const float SpeedReference = 8f;

        public ICharacterVisual Visual => visual;

        public override void OnNetworkSpawn()
        {
            lastPos = transform.position; hasLastPos = true;
            SelectedCharacter.OnValueChanged += (_, __) => RebuildVisual();
            RebuildVisual();
        }

        void RebuildVisual()
        {
            if (builtFor == SelectedCharacter.Value && visualRoot) return;
            if (visualRoot) Destroy(visualRoot.gameObject);
            visual = SurvivorGame.BuildCharacterVisualOn(transform, (SurvivorGame.PlayerCharacter)SelectedCharacter.Value, out visualRoot);
            builtFor = SelectedCharacter.Value; hasLastPos = false;
            if (IsServer) SetRenderersEnabled(visualRoot, false);
        }

        static void SetRenderersEnabled(Transform root, bool enabled)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.enabled = enabled;
        }

        void Update()
        {
            if (visual == null) return;
            float dt = Time.deltaTime;
            float speed = 0;
            if (hasLastPos && dt > 0) speed = Mathf.Clamp01(Vector3.Distance(transform.position, lastPos) / (dt * SpeedReference));
            lastPos = transform.position; hasLastPos = true;
            visual.Tick(dt, speed, -1, 1, true, 1);
        }

        // Host-only: called every frame from SurvivorGame while a session is hosted.
        public void MirrorHost(Vector3 position, Quaternion rotation, SurvivorGame.PlayerCharacter character)
        {
            transform.SetPositionAndRotation(position, rotation);
            if (SelectedCharacter.Value != (int)character) SelectedCharacter.Value = (int)character;
        }

        // Wraps the host's own real visual (used on the host's own screen) so its discrete
        // animation triggers also replay on every other peer's copy of this mirror.
        public ICharacterVisual WrapForHost(ICharacterVisual hostOwnVisual) =>
            new TriggerRelayVisual(hostOwnVisual, (kind, aim) => PlayTriggerRpc(kind, aim));

        [Rpc(SendTo.NotServer)]
        void PlayTriggerRpc(int kind, Vector3 aim) { if (visual != null) TriggerRelayVisual.Apply(visual, kind, aim); }
    }
}
