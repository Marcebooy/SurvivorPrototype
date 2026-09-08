using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BonkSurvivor
{
    // Stage 1 multiplayer: the joining friend's body. Simple placeholder capsule, fixed
    // (non-leveling) melee, no shop/loadout of its own yet - see the multiplayer plan in AGENTS.md.
    public sealed class RemotePlayerNet : NetworkBehaviour
    {
        public const float MaxHealth = 80f;
        const float MoveSpeed = 8f;
        const float MeleeRadius = 3.5f;
        const float MeleeDamage = 16f;
        const float MeleeInterval = 1f;
        const float HitInvulnerability = .45f;

        public static readonly List<RemotePlayerNet> Active = new List<RemotePlayerNet>();
        public static RemotePlayerNet Local { get; private set; }

        public readonly NetworkVariable<float> Health = new NetworkVariable<float>(MaxHealth);
        public Vector3 Position => transform.position;
        public bool IsAlive => Health.Value > 0;
        public float MeleeRadiusSqr => MeleeRadius * MeleeRadius;
        public float MeleeDamageAmount => MeleeDamage;

        float meleeTimer, invulnerability;
        Material bodyMat;

        public override void OnNetworkSpawn()
        {
            BuildVisual();
            Active.Add(this);
            if (IsOwner) Local = this;
        }

        public override void OnNetworkDespawn()
        {
            Active.Remove(this);
            if (Local == this) Local = null;
        }

        void BuildVisual()
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Kaverin hahmo";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = Vector3.up;
            Object.Destroy(body.GetComponent<Collider>());
            bodyMat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.3f, .95f, .55f) };
            body.GetComponent<Renderer>().sharedMaterial = bodyMat;
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
            transform.position += move * (MoveSpeed * Time.deltaTime);
            if (move.sqrMagnitude > .01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), Time.deltaTime * 14);
        }

        public override void OnDestroy() { if (bodyMat) Object.Destroy(bodyMat); base.OnDestroy(); }

        // Host-only bookkeeping below - driven from SurvivorGame's network coop tick.
        public void TickServer(float dt)
        {
            if (invulnerability > 0) invulnerability -= dt;
            meleeTimer -= dt;
        }

        public bool TryConsumeMeleeTick()
        {
            if (meleeTimer > 0) return false;
            meleeTimer = MeleeInterval; return true;
        }

        public void ApplyDamage(float amount)
        {
            if (!IsServer || !IsAlive || invulnerability > 0) return;
            Health.Value = Mathf.Max(0, Health.Value - amount);
            invulnerability = HitInvulnerability;
        }
    }
}
