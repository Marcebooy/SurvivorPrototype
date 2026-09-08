using Unity.Netcode;
using UnityEngine;

namespace BonkSurvivor
{
    // Networked XP/gold/health pickup so a connected friend can see loot on the ground too
    // (previously these were plain non-networked primitives, invisible to a client). Position is
    // replicated via NetworkTransform on the prefab; only the look (color) needs its own sync.
    public sealed class DropNet : NetworkBehaviour
    {
        public readonly NetworkVariable<bool> IsHealth = new NetworkVariable<bool>();
        Material mat;

        public override void OnNetworkSpawn()
        {
            var rend = GetComponentInChildren<Renderer>();
            if (!rend) return;
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = IsHealth.Value ? new Color(1f, .25f, .4f) : new Color(.3f, .8f, 1f);
            rend.sharedMaterial = mat;
        }

        public override void OnNetworkDespawn() { if (mat) Destroy(mat); }
    }
}
