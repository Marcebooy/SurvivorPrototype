using UnityEngine;

namespace BonkSurvivor
{
    // Wraps the Blender-built berserker (Assets/Survivor/Characters/Berserker/Resources/*.fbx) as
    // a playable visual. Same three-FBX-one-rig trick as RitariVisual: only Berserker_Walk_Heavy
    // carries the mesh we instantiate, and Attack_Double / Rage_LowHP are lifted out of their own
    // FBX and added onto that Animation component (see Unity/Berserker/README.txt). Swing plays
    // the double-axe swing used by the Corrupted Sword starting weapon, whose damage already scales
    // with missing HP - Rage_LowHP is a stationary "enraged" loop (heavy breathing, no footwork)
    // that replaces Walk once health drops below a threshold, matching the model's own baked
    // Rage value (.18 walking, .92-1 critical) and the "vahinko kasvaa kun HP laskee" passive idea.
    public sealed class BerserkerVisual : MonoBehaviour, ICharacterVisual
    {
        const string WalkResource = "Berserker_Walk_Heavy";
        const string AttackResource = "Berserker_Attack_Double";
        const string RageResource = "Berserker_Rage_LowHP";
        const float RageThreshold = .3f;
        Transform pose;
        Animation legacyAnimation;
        float actionTimer;
        bool raging;

        public void Build()
        {
            pose = new GameObject("Berserker roll pivot").transform; pose.SetParent(transform, false);
            var prefab = Resources.Load<GameObject>(WalkResource);
            if (prefab)
            {
                var instance = Instantiate(prefab, pose, false);
                instance.name = "Berserker model";
                legacyAnimation = instance.GetComponentInChildren<Animation>();
                if (legacyAnimation)
                {
                    legacyAnimation.wrapMode = WrapMode.Loop;
                    AddNamedClip(legacyAnimation, WalkResource, "Walk", true);
                    AddNamedClip(legacyAnimation, AttackResource, "Attack", false);
                    AddNamedClip(legacyAnimation, RageResource, "Rage", true);
                }
                else Debug.LogWarning("Berserker_Walk_Heavy.fbx has no legacy Animation component - check the model's Rig import setting (should be Legacy).");
            }
            else
            {
                Debug.LogWarning("Berserker_Walk_Heavy.fbx not found under a Resources folder - showing a placeholder capsule instead.");
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(pose, false); go.transform.localPosition = new Vector3(0, 1, 0); go.transform.localScale = new Vector3(1f, 1.3f, 1f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.3f, .1f, .07f) };
                go.GetComponent<Renderer>().sharedMaterial = mat; Destroy(go.GetComponent<Collider>());
            }
            ResetPose();
        }

        static void AddNamedClip(Animation anim, string resource, string stateName, bool loop)
        {
            AnimationClip found = null;
            foreach (var obj in Resources.LoadAll(resource)) if (obj is AnimationClip c) { found = c; break; }
            if (!found) return;
            found.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            anim.AddClip(found, stateName);
        }

        public void ResetPose()
        {
            pose.localRotation = Quaternion.identity; actionTimer = 0; raging = false;
            if (legacyAnimation && legacyAnimation["Walk"] != null) { legacyAnimation["Walk"].time = 0; legacyAnimation.Play("Walk"); legacyAnimation["Walk"].speed = 0; }
        }

        public void Swing(Vector3 worldAim) => PlayAction("Attack");
        public void Bonk() { }
        public void Block() { }
        public void Shockwave() { }

        void PlayAction(string state)
        {
            if (!legacyAnimation || legacyAnimation[state] == null) return;
            legacyAnimation.Play(state);
            actionTimer = legacyAnimation[state].length;
        }

        public void Tick(float dt, float speed, float roll, float size, bool showMeleeWeapon, float healthFraction)
        {
            pose.localRotation = roll >= 0 ? Quaternion.Euler(roll * 360, 0, 0) : Quaternion.identity;
            if (!legacyAnimation) return;
            if (actionTimer > 0)
            {
                actionTimer -= dt;
                if (actionTimer > 0) return;
            }
            bool wantsRage = healthFraction < RageThreshold;
            if (wantsRage != raging)
            {
                raging = wantsRage;
                string state = raging ? "Rage" : "Walk";
                if (legacyAnimation[state] != null) { legacyAnimation[state].time = 0; legacyAnimation.Play(state); }
            }
            if (raging) return; // heavy-breathing loop plays at its own pace, independent of movement
            var walk = legacyAnimation["Walk"];
            if (walk != null) walk.speed = speed > .02f ? speed * 1.2f : 0f;
        }
    }
}
