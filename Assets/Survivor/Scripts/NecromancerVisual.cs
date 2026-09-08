using UnityEngine;

namespace BonkSurvivor
{
    // Wraps the Blender-built necromancer (Assets/Survivor/Characters/Necromancer/Resources/
    // Necromancer_Glide.fbx) as a playable visual. Same shape as VelhoVisual: one FBX, one looping
    // clip (a hovering "Glide_Slow" - see Unity/Necromancer/README.txt - baked under whatever take
    // name Blender's exporter actually used, so it's picked up dynamically rather than by name).
    // The model carries its bone staff and floating skull permanently, and its starting weapon
    // (Bone) is a ranged auto-attack, so Swing/Bonk/Block stay no-ops like Velho's.
    // Materials/rig import settings are fixed up by Assets/Survivor/Editor/NecromancerImportPostprocessor.cs.
    public sealed class NecromancerVisual : MonoBehaviour, ICharacterVisual
    {
        const string ResourceName = "Necromancer_Glide";
        Transform pose;
        Animation legacyAnimation;
        AnimationState glideState;

        public void Build()
        {
            pose = new GameObject("Necromancer roll pivot").transform; pose.SetParent(transform, false);
            var prefab = Resources.Load<GameObject>(ResourceName);
            if (prefab)
            {
                var instance = Instantiate(prefab, pose, false);
                instance.name = "Necromancer model";
                legacyAnimation = instance.GetComponentInChildren<Animation>();
                if (legacyAnimation)
                {
                    legacyAnimation.wrapMode = WrapMode.Loop;
                    foreach (AnimationState s in legacyAnimation) { glideState = s; break; }
                    if (glideState != null) { glideState.wrapMode = WrapMode.Loop; legacyAnimation.Play(glideState.name); }
                    else Debug.LogWarning("Necromancer_Glide.fbx's Animation component has no clips.");
                }
                else Debug.LogWarning("Necromancer_Glide.fbx has no legacy Animation component - check the model's Rig import setting (should be Legacy).");
            }
            else
            {
                Debug.LogWarning("Necromancer_Glide.fbx not found under a Resources folder - showing a placeholder capsule instead.");
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(pose, false); go.transform.localPosition = new Vector3(0, 1, 0); go.transform.localScale = new Vector3(.8f, 1.3f, .8f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.05f, .04f, .07f) };
                go.GetComponent<Renderer>().sharedMaterial = mat; Destroy(go.GetComponent<Collider>());
            }
            ResetPose();
        }

        public void ResetPose()
        {
            pose.localRotation = Quaternion.identity;
            if (legacyAnimation && glideState != null) { glideState.time = 0; legacyAnimation.Play(glideState.name); glideState.speed = 0; }
        }

        public void Swing(Vector3 worldAim) { }
        public void Bonk() { }
        public void Block() { }
        public void Shockwave() { }

        public void Tick(float dt, float speed, float roll, float size, bool showMeleeWeapon, float healthFraction)
        {
            if (glideState != null) glideState.speed = speed > .02f ? speed * 1.3f : 0f;
            pose.localRotation = roll >= 0 ? Quaternion.Euler(roll * 360, 0, 0) : Quaternion.identity;
        }
    }
}
