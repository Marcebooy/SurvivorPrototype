using UnityEngine;

namespace BonkSurvivor
{
    // Wraps the Blender-built paladin (Assets/Survivor/Characters/Paladin/Resources/*.fbx) as a
    // playable visual. Same trick as RitariVisual: only Paladin_Walk_Steady carries the mesh we
    // instantiate, Attack_CleanSlash and Block_Holy are lifted out of their own FBX and added onto
    // that Animation component (see Unity/Paladin/README.txt). The starting weapon is Aura (a
    // continuous passive ring with no attack animation of its own, so it deals guaranteed damage
    // from the first second rather than only reacting to being hit - the same reasoning that put
    // Sword ahead of Aegis for Ritari); Swing/Block still play Attack_CleanSlash/Block_Holy the
    // moment the player picks up a Sword-family weapon or Aegis later, which is exactly how a
    // paladin's kit is expected to round out. Divine_Beam has no gameplay hook yet - nothing in
    // the current weapon set represents a channelled beam - so it stays unused for now, same as
    // several of this model's other decorative parts.
    public sealed class PaladinVisual : MonoBehaviour, ICharacterVisual
    {
        const string WalkResource = "Paladin_Walk_Steady";
        const string AttackResource = "Paladin_Attack_CleanSlash";
        const string BlockResource = "Paladin_Block_Holy";
        Transform pose, modelRoot;

        public Transform GetSocket(AttachmentSocket socket) => modelRoot ? WeaponAttachmentPoint.FindDriver(modelRoot, SurvivorGame.PlayerCharacter.Paladin, socket) : null;
        Animation legacyAnimation;
        float actionTimer;

        public void Build()
        {
            pose = new GameObject("Paladin roll pivot").transform; pose.SetParent(transform, false);
            var prefab = Resources.Load<GameObject>(WalkResource);
            if (prefab)
            {
                var instance = Instantiate(prefab, pose, false);
                instance.name = "Paladin model";
                modelRoot = instance.transform;
                legacyAnimation = instance.GetComponentInChildren<Animation>();
                if (legacyAnimation)
                {
                    legacyAnimation.wrapMode = WrapMode.Loop;
                    AddNamedClip(legacyAnimation, WalkResource, "Walk", true);
                    AddNamedClip(legacyAnimation, AttackResource, "Attack", false);
                    AddNamedClip(legacyAnimation, BlockResource, "Block", false);
                }
                else Debug.LogWarning("Paladin_Walk_Steady.fbx has no legacy Animation component - check the model's Rig import setting (should be Legacy).");
            }
            else
            {
                Debug.LogWarning("Paladin_Walk_Steady.fbx not found under a Resources folder - showing a placeholder capsule instead.");
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(pose, false); go.transform.localPosition = new Vector3(0, 1, 0); go.transform.localScale = new Vector3(1f, 1.3f, 1f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.2f, .008f, .016f) };
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
        public void Bonk() { }
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
                if (legacyAnimation["Walk"] != null) legacyAnimation.Play("Walk");
            }
            var walk = legacyAnimation["Walk"];
            if (walk != null) walk.speed = speed > .02f ? speed * 1.1f : 0f;
        }
    }
}
