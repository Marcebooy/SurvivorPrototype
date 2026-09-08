using UnityEngine;

namespace BonkSurvivor
{
    // Wraps the Blender-built hunter (Assets/Survivor/Characters/Hunter/Resources/*.fbx) as a
    // playable visual. Only Hunter_Run_Agile carries the mesh we instantiate; Bow_ChargeRelease is
    // lifted out of its own FBX and added onto that Animation component (see
    // Unity/Hunter/README.txt). Swing plays the bow draw-and-release - the Bow weapon itself has
    // no character hook elsewhere in the codebase, so one was added to AttackWeapons() alongside
    // this character.
    public sealed class HunterVisual : MonoBehaviour, ICharacterVisual
    {
        const string RunResource = "Hunter_Run_Agile";
        const string ShootResource = "Hunter_Bow_ChargeRelease";
        Transform pose;
        Animation legacyAnimation;
        float actionTimer;

        public void Build()
        {
            pose = new GameObject("Hunter roll pivot").transform; pose.SetParent(transform, false);
            var prefab = Resources.Load<GameObject>(RunResource);
            if (prefab)
            {
                var instance = Instantiate(prefab, pose, false);
                instance.name = "Hunter model";
                legacyAnimation = instance.GetComponentInChildren<Animation>();
                if (legacyAnimation)
                {
                    legacyAnimation.wrapMode = WrapMode.Loop;
                    AddNamedClip(legacyAnimation, RunResource, "Run", true);
                    AddNamedClip(legacyAnimation, ShootResource, "Shoot", false);
                }
                else Debug.LogWarning("Hunter_Run_Agile.fbx has no legacy Animation component - check the model's Rig import setting (should be Legacy).");
            }
            else
            {
                Debug.LogWarning("Hunter_Run_Agile.fbx not found under a Resources folder - showing a placeholder capsule instead.");
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(pose, false); go.transform.localPosition = new Vector3(0, 1, 0); go.transform.localScale = new Vector3(.75f, 1.25f, .75f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.035f, .095f, .049f) };
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
            if (legacyAnimation && legacyAnimation["Run"] != null) { legacyAnimation["Run"].time = 0; legacyAnimation.Play("Run"); legacyAnimation["Run"].speed = 0; }
        }

        public void Swing(Vector3 worldAim)
        {
            if (!legacyAnimation || legacyAnimation["Shoot"] == null) return;
            legacyAnimation.Play("Shoot");
            actionTimer = legacyAnimation["Shoot"].length;
        }
        public void Bonk() { }
        public void Block() { }
        public void Shockwave() { }

        public void Tick(float dt, float speed, float roll, float size, bool showMeleeWeapon, float healthFraction)
        {
            pose.localRotation = roll >= 0 ? Quaternion.Euler(roll * 360, 0, 0) : Quaternion.identity;
            if (!legacyAnimation) return;
            if (actionTimer > 0)
            {
                actionTimer -= dt;
                if (actionTimer > 0) return;
                if (legacyAnimation["Run"] != null) legacyAnimation.Play("Run");
            }
            var run = legacyAnimation["Run"];
            if (run != null) run.speed = speed > .02f ? speed * 1.3f : 0f;
        }
    }
}
