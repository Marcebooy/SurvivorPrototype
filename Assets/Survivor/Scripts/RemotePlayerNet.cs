using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BonkSurvivor
{
    // Stage 2 multiplayer: the joining friend's body. Has its own real Combatant (character,
    // weapons, XP/level) simulated host-side exactly like the host's own player - see
    // SurvivorGame.TickNetworkCoop and the FriendXxx() helpers in SurvivorGameNetwork.cs.
    public sealed class RemotePlayerNet : NetworkBehaviour
    {
        const float MoveSpeed = 8f;

        public static readonly List<RemotePlayerNet> Active = new List<RemotePlayerNet>();
        public static RemotePlayerNet Local { get; private set; }

        // Host-only: the friend's actual game state (weapons, XP, level, stats). Not networked
        // directly - the NetworkVariables below mirror just the numbers the client's HUD needs.
        [System.NonSerialized] public SurvivorGame.Combatant FriendState;

        public readonly NetworkVariable<float> Health = new NetworkVariable<float>(100f);
        public readonly NetworkVariable<float> MaxHealthNet = new NetworkVariable<float>(100f);
        public readonly NetworkVariable<int> Level = new NetworkVariable<int>(1);
        public readonly NetworkVariable<int> Coins = new NetworkVariable<int>();
        public readonly NetworkVariable<int> PendingChoices = new NetworkVariable<int>();
        public readonly NetworkVariable<int> Choice0 = new NetworkVariable<int>(-1);
        public readonly NetworkVariable<int> Choice1 = new NetworkVariable<int>(-1);
        public readonly NetworkVariable<int> Choice2 = new NetworkVariable<int>(-1);
        public readonly NetworkVariable<bool> Selecting = new NetworkVariable<bool>(true);
        public readonly NetworkVariable<bool> AwaitingCharacterChoice = new NetworkVariable<bool>(true);
        public readonly NetworkVariable<int> SelectedCharacter = new NetworkVariable<int>();

        public Vector3 Position => transform.position;
        public bool IsAlive => Health.Value > 0;

        SurvivorGame map;
        Transform visualRoot;
        ICharacterVisual visual;
        Vector3 lastPos;
        bool hasLastPos;

        public override void OnNetworkSpawn()
        {
            map = Object.FindFirstObjectByType<SurvivorGame>();
            Active.Add(this);
            if (IsOwner) { Local = this; if (map) transform.position = map.MapSpawn; }
            lastPos = transform.position; hasLastPos = true;
            // The body mesh is built independently (not networked) on every peer that can see this
            // object - host, the owning friend, and anyone else - so everyone shows the same
            // deterministic model without replicating the mesh itself. On the host, RequestCharacterRpc
            // already builds it synchronously (and wires it into FriendState for combat animations);
            // this reactive path is only for OTHER peers (the friend's own client, spectators) where
            // FriendState is null, so we don't rebuild a redundant second copy on the host itself.
            AwaitingCharacterChoice.OnValueChanged += (_, awaiting) => { if (FriendState == null && !awaiting) RebuildVisual((SurvivorGame.PlayerCharacter)SelectedCharacter.Value); };
            SelectedCharacter.OnValueChanged += (_, val) => { if (FriendState == null && !AwaitingCharacterChoice.Value) RebuildVisual((SurvivorGame.PlayerCharacter)val); };
            if (FriendState == null && !AwaitingCharacterChoice.Value) RebuildVisual((SurvivorGame.PlayerCharacter)SelectedCharacter.Value);
        }

        // Builds the body mesh fresh under this transform. On the host (FriendState != null) this
        // also wires a trigger-relaying wrapper into the Combatant so combat code (Swing/Bonk/Block)
        // both animates the host's own local copy AND replays the same trigger on every other peer
        // (via RPC) - see SelectCharacter's `current == hostState` check in SurvivorPottu.cs, which
        // skips its own mesh-building for a friend Combatant so there's only ever one mesh per peer.
        public void RebuildVisual(SurvivorGame.PlayerCharacter character)
        {
            if (visualRoot) Destroy(visualRoot.gameObject);
            visual = SurvivorGame.BuildCharacterVisualOn(transform, character, out visualRoot);
            hasLastPos = false;
            if (FriendState != null)
            {
                FriendState.CharacterVisual = new TriggerRelayVisual(visual, (kind, aim) => PlayTriggerRpc(kind, aim));
                FriendState.VisualRoot = visualRoot; FriendState.SelectedCharacter = character;
            }
        }

        public override void OnNetworkDespawn()
        {
            Active.Remove(this);
            if (Local == this) Local = null;
        }

        void Update()
        {
            // Everyone (owner, host, any spectator) drives their own local copy's walk-cycle blend
            // from locally observed movement - cheaper and simpler than networking animation floats,
            // and works whether or not this particular peer is the one actually moving the body.
            if (visual != null)
            {
                float dt = Time.deltaTime;
                float speed = 0;
                if (hasLastPos && dt > 0) speed = Mathf.Clamp01(Vector3.Distance(transform.position, lastPos) / (dt * MoveSpeed));
                lastPos = transform.position; hasLastPos = true;
                visual.Tick(dt, speed, -1, 1, true, 1);
            }

            if (!IsOwner) return;
            Vector2 input = Vector2.zero;
            var k = Keyboard.current;
            if (k != null)
            {
                input.x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
                input.y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
            }
            if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > .05f) input = Gamepad.current.leftStick.ReadValue();
            input = Vector2.ClampMagnitude(input, 1);
            var move = new Vector3(input.x, 0, input.y);
            var motion = move * (MoveSpeed * Time.deltaTime);
            transform.position = map ? map.MoveOnMap(transform.position, motion) : transform.position + motion;
            if (move.sqrMagnitude > .01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), Time.deltaTime * 14);
        }

        // Host -> everyone else: replay a discrete animation trigger (see TriggerRelayVisual). The
        // host already played it locally before calling this, so skip it there.
        [Rpc(SendTo.NotServer)]
        void PlayTriggerRpc(int kind, Vector3 aim) { if (visual != null) TriggerRelayVisual.Apply(visual, kind, aim); }

        // Client -> host: menu choices. The client renders the picker UI from shared static data
        // (character roster / upgrade names) - only the pick itself needs to cross the network.
        [Rpc(SendTo.Server)]
        public void RequestCharacterRpc(int character)
        {
            if (!map || FriendState == null) return;
            // Build the mesh first (synchronously, host-local) so it already exists when
            // SelectCharacter's ResetPottu() calls into it a moment later.
            RebuildVisual((SurvivorGame.PlayerCharacter)character);
            map.FriendSelectCharacter(FriendState, (SurvivorGame.PlayerCharacter)character);
        }

        [Rpc(SendTo.Server)]
        public void RequestStarterRpc(int weaponIndex)
        {
            if (map && FriendState != null) map.FriendSelectStarter(FriendState, weaponIndex);
        }

        [Rpc(SendTo.Server)]
        public void RequestUpgradeRpc(int slot)
        {
            if (map && FriendState != null) map.FriendChooseUpgrade(FriendState, slot);
        }
    }
}
