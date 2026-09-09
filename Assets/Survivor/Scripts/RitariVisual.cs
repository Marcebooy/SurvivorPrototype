using UnityEngine;

namespace BonkSurvivor
{
    // Wraps the Blender-built knight (Assets/Survivor/Characters/Ritari/Resources/*.fbx) as a
    // playable visual. Unlike Velho, the model/sword/shield are exported as three separate
    // per-clip FBX files sharing the identical 12-bone rig (see Unity/Knight/README.txt): only
    // Ritari_Walk_Heavy carries the mesh we instantiate, and the Attack_Slash / Block clips are
    // lifted out of their own FBX and added onto that same Animation component at runtime
    // (legacy animation only needs matching bone paths, and all three exports share them).
    // Import settings/materials are fixed up by Assets/Survivor/Editor/RitariImportPostprocessor.cs.
    // Swing plays the built-in sword slash; Block plays the shield-raise + spark flash used by
    // the Aegis starting weapon.
    public sealed class RitariVisual : MonoBehaviour, ICharacterVisual
    {
        const string WalkResource = "Ritari_Walk_Heavy";
        const string AttackResource = "Ritari_Attack_Slash";
        const string BlockResource = "Ritari_Block";
        Transform pose, modelRoot;

        public Transform GetSocket(AttachmentSocket socket) => modelRoot ? WeaponAttachmentPoint.FindDriver(modelRoot, SurvivorGame.PlayerCharacter.Ritari, socket) : null;
        Animation legacyAnimation;
        float actionTimer;

        public void Build()
        {
            pose = new GameObject("Ritari roll pivot").transform; pose.SetParent(transform, false);
            var prefab = Resources.Load<GameObject>(WalkResource);
            if (prefab)
            {
                var instance = Instantiate(prefab, pose, false);
                instance.name = "Ritari model";
                modelRoot = instance.transform;
                legacyAnimation = instance.GetComponentInChildren<Animation>();
                if (legacyAnimation)
                {
                    legacyAnimation.wrapMode = WrapMode.Loop;
                    AddNamedClip(legacyAnimation, WalkResource, "Walk", true);
                    AddNamedClip(legacyAnimation, AttackResource, "Attack", false);
                    AddNamedClip(legacyAnimation, BlockResource, "Block", false);
                }
                else Debug.LogWarning("Ritari_Walk_Heavy.fbx has no legacy Animation component - check the model's Rig import setting (should be Legacy).");
            }
            else
            {
                Debug.LogWarning("Ritari_Walk_Heavy.fbx not found under a Resources folder - showing a placeholder capsule instead.");
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(pose, false); go.transform.localPosition = new Vector3(0, 1, 0); go.transform.localScale = new Vector3(1f, 1.3f, 1f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.09f, .12f, .17f) };
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
        public void Block() => PlayAction("Block");
        public void Shockwave() { }
        public void Bonk() { }

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
                if (actionTimer <= 0 && legacyAnimation["Walk"] != null) legacyAnimation.Play("Walk");
                else return;
            }
            var walk = legacyAnimation["Walk"];
            if (walk != null) walk.speed = speed > .02f ? speed * 1.2f : 0f;
        }
    }
}
