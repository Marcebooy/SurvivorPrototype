using UnityEngine;

namespace BonkSurvivor
{
    // Wraps the Blender-built golem (Assets/Survivor/Characters/Golem/Resources/*.fbx) as a
    // playable visual. Same three-FBX-one-rig trick as RitariVisual: only Golem_Walk_Massive
    // carries the mesh we instantiate, Attack_Crush and Stoneform_Shockwave are lifted out of
    // their own FBX and added onto that Animation component (see Unity/Golem/README.txt).
    // Golem's starting weapon (Chunkers) is a passive orbit with no attack hook of its own, so
    // Swing mostly sits unused unless a Sword-family weapon is picked up later; the interesting
    // hook is Shockwave, wired to every hit the golem takes (matching the README's own suggestion
    // that "pinojen tayttyminen kaynnistaa shokkiaallon" and AGENTS.md's "vahingon saaminen lataa
    // maahan iskeytyvän shokkiaallon" passive) and the model's orbiting-rocks pose doubles as a
    // nice nod to Chunkers itself.
    public sealed class GolemVisual : MonoBehaviour, ICharacterVisual
    {
        const string WalkResource = "Golem_Walk_Massive";
        const string AttackResource = "Golem_Attack_Crush";
        const string ShockResource = "Golem_Stoneform_Shockwave";
        Transform pose;
        Animation legacyAnimation;
        float actionTimer;

        public void Build()
        {
            pose = new GameObject("Golem roll pivot").transform; pose.SetParent(transform, false);
            var prefab = Resources.Load<GameObject>(WalkResource);
            if (prefab)
            {
                var instance = Instantiate(prefab, pose, false);
                instance.name = "Golem model";
                legacyAnimation = instance.GetComponentInChildren<Animation>();
                if (legacyAnimation)
                {
                    legacyAnimation.wrapMode = WrapMode.Loop;
                    AddNamedClip(legacyAnimation, WalkResource, "Walk", true);
                    AddNamedClip(legacyAnimation, AttackResource, "Attack", false);
                    AddNamedClip(legacyAnimation, ShockResource, "Shock", false);
                }
                else Debug.LogWarning("Golem_Walk_Massive.fbx has no legacy Animation component - check the model's Rig import setting (should be Legacy).");
            }
            else
            {
                Debug.LogWarning("Golem_Walk_Massive.fbx not found under a Resources folder - showing a placeholder capsule instead.");
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(pose, false); go.transform.localPosition = new Vector3(0, 1.2f, 0); go.transform.localScale = new Vector3(1.3f, 1.5f, 1.3f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.2f, .23f, .24f) };
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
            pose.localRotation = Quaternion.identity; actionTimer = 0;
            if (legacyAnimation && legacyAnimation["Walk"] != null) { legacyAnimation["Walk"].time = 0; legacyAnimation.Play("Walk"); legacyAnimation["Walk"].speed = 0; }
        }

        public void Swing(Vector3 worldAim) => PlayAction("Attack");
        public void Shockwave() => PlayAction("Shock");
        public void Bonk() { }
        public void Block() { }

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
                if (legacyAnimation["Walk"] != null) legacyAnimation.Play("Walk");
            }
            var walk = legacyAnimation["Walk"];
            if (walk != null) walk.speed = speed > .02f ? speed * .9f : 0f;
        }
    }
}
