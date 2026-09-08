using Unity.Netcode;
using UnityEngine;

namespace BonkSurvivor
{
    // Stage 1 multiplayer hooks: host keeps simulating exactly as in single-player and
    // additionally lets connected RemotePlayerNet friends take/deal contact damage; a
    // connected client renders the host's world instead of running its own simulation.
    // See the multiplayer plan in AGENTS.md.
    public sealed partial class SurvivorGame
    {
        bool NetworkActive => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        bool IsNetworkHost => NetworkActive && NetworkManager.Singleton.IsServer;
        bool NetworkClientMode => NetworkActive && !NetworkManager.Singleton.IsServer;

        void TickNetworkClient(float dt)
        {
            var rp = RemotePlayerNet.Local;
            if (rp && followCamera)
            {
                var target = rp.transform.position + new Vector3(0, 23, -19);
                followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, target, 1 - Mathf.Exp(-8 * dt));
                followCamera.transform.rotation = Quaternion.Euler(48, 0, 0);
            }
        }

        void TickNetworkCoop(float dt)
        {
            if (!IsNetworkHost) return;
            var remotes = RemotePlayerNet.Active;
            for (int r = 0; r < remotes.Count; r++)
            {
                var rp = remotes[r];
                if (!rp || !rp.IsAlive) continue;
                rp.TickServer(dt);
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    var e = enemies[i];
                    var delta = rp.Position - e.body.position; delta.y = 0;
                    if (delta.sqrMagnitude < 1.7f * 1.7f) rp.ApplyDamage(e.elite ? 26f : 13f);
                }
                if (rp.TryConsumeMeleeTick())
                    for (int i = enemies.Count - 1; i >= 0; i--)
                    {
                        var e = enemies[i];
                        if ((e.body.position - rp.Position).sqrMagnitude < rp.MeleeRadiusSqr) Hit(e, rp.MeleeDamageAmount);
                    }
            }
        }

        void DrawNetworkClientHud()
        {
            var rp = RemotePlayerNet.Local;
            GUI.Box(new Rect(20, 20, 330, 90), GUIContent.none);
            GUI.Label(new Rect(36, 28, 310, 32), "KAVERINA ISÄNNÄN PELISSÄ", hudNameStyle);
            float hp = rp ? rp.Health.Value : 0; float maxHp = RemotePlayerNet.MaxHealth;
            Bar(new Rect(36, 66, 290, 16), hp / maxHp, new Color(.3f, .95f, .55f));
            GUI.Label(new Rect(36, 84, 290, 24), "HP " + Mathf.CeilToInt(hp) + "/" + Mathf.CeilToInt(maxHp), textStyle);
            if (GUI.Button(new Rect(1070, 650, 185, 44), "Katkaise yhteys", buttonStyle) && SurvivorNetwork.Instance)
                SurvivorNetwork.Instance.Disconnect();
        }
    }
}
