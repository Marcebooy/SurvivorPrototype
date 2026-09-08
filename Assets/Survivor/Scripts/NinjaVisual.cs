using UnityEngine;

namespace BonkSurvivor
{
    // Wraps the Blender-built ninja (Assets/Survivor/Characters/Ninja/Resources/*.fbx) as a
    // playable visual. Same three-FBX-one-rig trick as RitariVisual: only Ninja_Run_Shadow carries
    // the mesh we instantiate, Attack_CrossCut and Shadowstep are lifted out of their own FBX and
    // added onto that Animation component (see Unity/Ninja/README.txt). Swing plays the
    // dual-blade cross-cut used by the Katana starting weapon (no character hook existed for
    // Katana before this - added alongside this character). Shadowstep replaces the generic
    // dodge-roll pose: instead of the potato-style barrel roll every other character uses, a ninja
    // plays its own teleport-flicker animation for the same iframe window (driven by the shared
    // `roll` parameter every character's Tick already receives, so no new interface hook needed).
    public sealed class NinjaVisual : MonoBehaviour, ICharacterVisual
    {
        const string RunResource = "Ninja_Run_Shadow";
        const string AttackResource = "Ninja_Attack_CrossCut";
        const string DashResource = "Ninja_Shadowstep";
        Transform pose;
        Animation legacyAnimation;
        float actionTimer;
        bool wasRolling;

        public void Build()
        {
            pose = new GameObject("Ninja roll pivot").transform; pose.SetParent(transform, false);
            var prefab = Resources.Load<GameObject>(RunResource);
            if (prefab)
            {
                var instance = Instantiate(prefab, pose, false);
                instance.name = "Ninja model";
                legacyAnimation = instance.GetComponentInChildren<Animation>();
                if (legacyAnimation)
                {
                    legacyAnimation.wrapMode = WrapMode.Loop;
                    AddNamedClip(legacyAnimation, RunResource, "Run", true);
                    AddNamedClip(legacyAnimation, AttackResource, "Attack", false);
                    AddNamedClip(legacyAnimation, DashResource, "Dash", false);
                }
                else Debug.LogWarning("Ninja_Run_Shadow.fbx has no legacy Animation component - check the model's Rig import setting (should be Legacy).");
            }
            else
            {
                Debug.LogWarning("Ninja_Run_Shadow.fbx not found under a Resources folder - showing a placeholder capsule instead.");
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(pose, false); go.transform.localPosition = new Vector3(0, 1, 0); go.transform.localScale = new Vector3(.7f, 1.2f, .7f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.012f, .021f, .055f) };
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
            pose.localRotation = Quaternion.identity; actionTimer = 0; wasRolling = false;
            if (legacyAnimation && legacyAnimation["Run"] != null) { legacyAnimation["Run"].time = 0; legacyAnimation.Play("Run"); legacyAnimation["Run"].speed = 0; }
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
            bool rolling = roll >= 0;
            if (rolling && !wasRolling) PlayAction("Dash");
            wasRolling = rolling;
            if (!legacyAnimation) return;
            if (actionTimer > 0)
            {
                actionTimer -= dt;
                if (actionTimer > 0) return;
                if (legacyAnimation["Run"] != null) legacyAnimation.Play("Run");
            }
            var run = legacyAnimation["Run"];
            if (run != null) run.speed = speed > .02f ? speed * 1.4f : 0f;
        }
    }
}
