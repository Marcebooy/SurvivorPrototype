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

        public override void OnNetworkSpawn()
        {
            map = Object.FindFirstObjectByType<SurvivorGame>();
            Active.Add(this);
            if (IsOwner) { Local = this; if (map) transform.position = map.MapSpawn; }
        }

        public override void OnNetworkDespawn()
        {
            Active.Remove(this);
            if (Local == this) Local = null;
        }

        void Update()
        {
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

        // Client -> host: menu choices. The client renders the picker UI from shared static data
        // (character roster / upgrade names) - only the pick itself needs to cross the network.
        [Rpc(SendTo.Server)]
        public void RequestCharacterRpc(int character)
        {
            if (map && FriendState != null) map.FriendSelectCharacter(FriendState, (SurvivorGame.PlayerCharacter)character);
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
