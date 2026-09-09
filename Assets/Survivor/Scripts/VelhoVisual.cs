using UnityEngine;

namespace BonkSurvivor
{
    // Wraps the Blender-built wizard (Assets/Survivor/Characters/Velho/Resources/Velho_Walk.fbx)
    // as a playable visual. The FBX carries a 9-bone rig and a single looping walk clip (named
    // "Walk_Quick" as a Blender action - see Unity/Wizard/README.txt - but Blender's FBX exporter
    // bakes it into the file under the scene's name instead, so the clip is picked up by whatever
    // it's actually called rather than a hardcoded name). Materials/rig import settings are fixed
    // up on import by Assets/Survivor/Editor/VelhoImportPostprocessor.cs. No attack animation
    // exists yet, so Swing/Bonk are no-ops - Velho's starting weapon (Lightning) has no melee visual hook.
    public sealed class VelhoVisual : MonoBehaviour, ICharacterVisual
    {
        const string ResourceName = "Velho_Walk";
        Transform pose, modelRoot;
        Animation legacyAnimation;
        AnimationState walkState;

        public Transform GetSocket(AttachmentSocket socket) => modelRoot ? WeaponAttachmentPoint.FindDriver(modelRoot, SurvivorGame.PlayerCharacter.Velho, socket) : null;

        public void Build()
        {
            pose = new GameObject("Velho roll pivot").transform; pose.SetParent(transform, false);
            var prefab = Resources.Load<GameObject>(ResourceName);
            if (prefab)
            {
                var instance = Instantiate(prefab, pose, false);
                instance.name = "Velho model";
                modelRoot = instance.transform;
                legacyAnimation = instance.GetComponentInChildren<Animation>();
                if (legacyAnimation)
                {
                    legacyAnimation.wrapMode = WrapMode.Loop;
                    foreach (AnimationState s in legacyAnimation) { walkState = s; break; }
                    if (walkState != null) { walkState.wrapMode = WrapMode.Loop; legacyAnimation.Play(walkState.name); }
                    else Debug.LogWarning("Velho_Walk.fbx's Animation component has no clips.");
                }
                else Debug.LogWarning("Velho_Walk.fbx has no legacy Animation component - check the model's Rig import setting (should be Legacy).");
            }
            else
            {
                Debug.LogWarning("Velho_Walk.fbx not found under a Resources folder - showing a placeholder capsule instead.");
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(pose, false); go.transform.localPosition = new Vector3(0, 1, 0); go.transform.localScale = new Vector3(.8f, 1.3f, .8f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.11f, .03f, .22f) };
                go.GetComponent<Renderer>().sharedMaterial = mat; Destroy(go.GetComponent<Collider>());
            }
            ResetPose();
        }

        public void ResetPose()
        {
            pose.localRotation = Quaternion.identity;
            if (legacyAnimation && walkState != null) { walkState.time = 0; legacyAnimation.Play(walkState.name); walkState.speed = 0; }
        }

        public void Swing(Vector3 worldAim) { }
        public void Bonk() { }
        public void Block() { }
        public void Shockwave() { }

        public void Tick(float dt, float speed, float roll, float size, bool showMeleeWeapon, float healthFraction)
        {
            if (walkState != null) walkState.speed = speed > .02f ? speed * 1.4f : 0f;
            pose.localRotation = roll >= 0 ? Quaternion.Euler(roll * 360, 0, 0) : Quaternion.identity;
        }
    }
}
