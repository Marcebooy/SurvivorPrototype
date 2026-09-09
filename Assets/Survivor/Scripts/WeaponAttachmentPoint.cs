using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    // The four carry points delivered by Astra's WeaponArtPilot pilot (Assets/WeaponArtPilot/Documentation,
    // 2026-09-09) - see CHARACTER_SOCKET_TABLE.md for the per-character rest pose this encodes.
    public enum AttachmentSocket { RightHand, LeftHand, Back, Belt }

    // Generalized weapon-carry system from the WeaponArtPilot delivery. Replaces SwordVariantVisual's old
    // fixed body-relative offset with real per-character socket data (CONVENTIONS.md + attachment_profiles.json):
    //   worldPosition = driver.TransformPoint(profile.localPosition)
    //   worldRotation = driver.rotation * profile.localRotation
    //   weaponWorldScale = characterUniformScale * weaponVisualScale
    // For the 8 rigged FBX characters this is just Transform parenting under the resolved driver bone -
    // profile.localScale already bakes in the 0.01 compensation for their 100x-scaled bones (CONVENTIONS.md
    // "Tuonnissa todetut kaksi poikkeamaa" #2; the FBX's own import scale is never touched). Pottu has no FBX
    // rig and his body has an animated non-uniform squash/stretch scale plus a Size-stat-driven pan scale -
    // neither may leak into the weapon (CONVENTIONS.md #3) - so he uses WeaponRigidFollower instead, which
    // reads the driver's position/rotation only and ignores its scale entirely.
    //
    // A missing driver path is an import/data error, not a silent (0,0,0) placement (CODE_HANDOFF.md #1) -
    // Attach() logs an error and returns false rather than guessing a position.
    public static class WeaponAttachmentPoint
    {
        public struct Profile
        {
            public string driverPath;
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;
            public Profile(string path, Vector3 pos, Quaternion rot, Vector3 scale)
            { driverPath = path; localPosition = pos; localRotation = rot; localScale = scale; }
        }

        struct CharacterProfiles
        {
            public float defaultVisualScale;
            public Profile RightHand, LeftHand, Back, Belt;
        }

        static readonly Vector3 RiggedCompensatedScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Values copied verbatim from Assets/WeaponArtPilot/Documentation/attachment_profiles.json
        // (schemaVersion 1, units metres, quaternion order xyzw). defaultVisualScale is Astra's design
        // intent per character (0.85-1.45), not a measured bone scale - CONVENTIONS.md "Ajurien
        // seuraaminen ja skaala".
        static readonly Dictionary<SurvivorGame.PlayerCharacter, CharacterProfiles> Table = new Dictionary<SurvivorGame.PlayerCharacter, CharacterProfiles>
        {
            [SurvivorGame.PlayerCharacter.Velho] = new CharacterProfiles
            {
                defaultVisualScale = 1.0f,
                RightHand = new Profile("Velho_Rig/Root/Body/Arm.L", new Vector3(0.0046f, -0.0057f, 0.0018f), new Quaternion(0.048963f, -0.0108624f, -0.216311f, 0.9750355f), RiggedCompensatedScale),
                LeftHand = new Profile("Velho_Rig/Root/Body/Arm.R", new Vector3(-0.0046f, -0.0057f, 0.0018f), new Quaternion(0.048963f, 0.0108624f, 0.216311f, 0.9750355f), RiggedCompensatedScale),
                Back = new Profile("Velho_Rig/Root/Body", new Vector3(0f, 0.0062f, -0.0046f), new Quaternion(0.9848185f, 0.1735871f, 0f, -1e-07f), RiggedCompensatedScale),
                Belt = new Profile("Velho_Rig/Root/Body", new Vector3(0.0051f, 0f, -0.0005f), new Quaternion(0.7071068f, 0f, 0.7071068f, 0f), RiggedCompensatedScale),
            },
            [SurvivorGame.PlayerCharacter.Ritari] = new CharacterProfiles
            {
                defaultVisualScale = 1.0f,
                RightHand = new Profile("Ritari_Rig/Root/Body/Arm.L", new Vector3(0.0028f, -0.0051f, 0.001f), new Quaternion(0.048963f, -0.0108624f, -0.216311f, 0.9750355f), RiggedCompensatedScale),
                LeftHand = new Profile("Ritari_Rig/Root/Body/Arm.R", new Vector3(-0.0028f, -0.0051f, 0.001f), new Quaternion(0.048963f, 0.0108624f, 0.216311f, 0.9750355f), RiggedCompensatedScale),
                Back = new Profile("Ritari_Rig/Root/Body", new Vector3(-0.0018f, 0.0068f, -0.0062f), new Quaternion(0.9848185f, 0.1735871f, 0f, -1e-07f), RiggedCompensatedScale),
                Belt = new Profile("Ritari_Rig/Root/Body", new Vector3(0.0058f, 0.0004f, -0.0008f), new Quaternion(0.7071068f, 0f, 0.7071068f, 0f), RiggedCompensatedScale),
            },
            [SurvivorGame.PlayerCharacter.Necromancer] = new CharacterProfiles
            {
                defaultVisualScale = 0.95f,
                RightHand = new Profile("Necromancer_Rig/Root/Body/Arm.L", new Vector3(0.0032f, -0.0052f, 0.0028f), new Quaternion(0.048963f, -0.0108624f, -0.216311f, 0.9750355f), RiggedCompensatedScale),
                LeftHand = new Profile("Necromancer_Rig/Root/Body/Arm.R", new Vector3(-0.0032f, -0.0052f, 0.0028f), new Quaternion(0.048963f, 0.0108624f, 0.216311f, 0.9750355f), RiggedCompensatedScale),
                Back = new Profile("Necromancer_Rig/Root/Body", new Vector3(0f, 0.0042f, -0.0048f), new Quaternion(0.9848185f, 0.1735871f, 0f, -1e-07f), RiggedCompensatedScale),
                Belt = new Profile("Necromancer_Rig/Root/Body", new Vector3(0.0036f, -0.0011f, -0.0008f), new Quaternion(0.7071068f, 0f, 0.7071068f, 0f), RiggedCompensatedScale),
            },
            [SurvivorGame.PlayerCharacter.Berserker] = new CharacterProfiles
            {
                defaultVisualScale = 1.1f,
                RightHand = new Profile("Berserker_Rig/Root/Body/UpperArm.L/Forearm.L", new Vector3(0.00165f, -0.005f, 0.00115f), new Quaternion(0f, 1e-07f, -0.9926551f, 0.1209787f), RiggedCompensatedScale),
                LeftHand = new Profile("Berserker_Rig/Root/Body/UpperArm.R/Forearm.R", new Vector3(-0.00165f, -0.005f, 0.00115f), new Quaternion(0f, -1e-07f, 0.9926551f, 0.1209787f), RiggedCompensatedScale),
                Back = new Profile("Berserker_Rig/Root/Body", new Vector3(0f, 0.0077f, -0.0045f), new Quaternion(0.9848185f, 0.1735871f, 0f, -1e-07f), RiggedCompensatedScale),
                Belt = new Profile("Berserker_Rig/Root/Body", new Vector3(0.0052f, -0.0002f, -0.001f), new Quaternion(0.7071068f, 0f, 0.7071068f, 0f), RiggedCompensatedScale),
            },
            [SurvivorGame.PlayerCharacter.Golem] = new CharacterProfiles
            {
                defaultVisualScale = 1.45f,
                RightHand = new Profile("Golem_Rig/Root/Body/Arm.L/Forearm.L", new Vector3(0.0016f, -0.0067f, 0.0018f), new Quaternion(0.048963f, -0.0108624f, -0.216311f, 0.9750355f), RiggedCompensatedScale),
                LeftHand = new Profile("Golem_Rig/Root/Body/Arm.R/Forearm.R", new Vector3(-0.0016f, -0.0067f, 0.0018f), new Quaternion(0.048963f, 0.0108624f, 0.216311f, 0.9750355f), RiggedCompensatedScale),
                Back = new Profile("Golem_Rig/Root/Body", new Vector3(0f, 0.0095f, -0.0064f), new Quaternion(0.9848185f, 0.1735871f, 0f, -1e-07f), RiggedCompensatedScale),
                Belt = new Profile("Golem_Rig/Root/Body", new Vector3(0.008f, -0.0002f, -0.001f), new Quaternion(0.7071068f, 0f, 0.7071068f, 0f), RiggedCompensatedScale),
            },
            [SurvivorGame.PlayerCharacter.Hunter] = new CharacterProfiles
            {
                defaultVisualScale = 0.9f,
                RightHand = new Profile("Hunter_Rig/Root/Body/Arm.L/Forearm.L", new Vector3(0.0018f, -0.0027f, 0.00175f), new Quaternion(0.048963f, -0.0108624f, -0.216311f, 0.9750355f), RiggedCompensatedScale),
                LeftHand = new Profile("Hunter_Rig/Root/Body/Arm.R/Forearm.R", new Vector3(-0.0018f, -0.0027f, 0.00175f), new Quaternion(0.048963f, 0.0108624f, 0.216311f, 0.9750355f), RiggedCompensatedScale),
                Back = new Profile("Hunter_Rig/Root/Body", new Vector3(0.0017f, 0.0053f, -0.0056f), new Quaternion(0.9848185f, 0.1735871f, 0f, -1e-07f), RiggedCompensatedScale),
                Belt = new Profile("Hunter_Rig/Root/Body", new Vector3(0.0036f, -0.0003f, -0.0009f), new Quaternion(0.7071068f, 0f, 0.7071068f, 0f), RiggedCompensatedScale),
            },
            [SurvivorGame.PlayerCharacter.Ninja] = new CharacterProfiles
            {
                defaultVisualScale = 0.85f,
                RightHand = new Profile("Ninja_Rig/Root/Body/Arm.L/Forearm.L", new Vector3(0.0013f, -0.0033f, 0.00115f), new Quaternion(0f, 1e-07f, -0.9926551f, 0.1209787f), RiggedCompensatedScale),
                LeftHand = new Profile("Ninja_Rig/Root/Body/Arm.R/Forearm.R", new Vector3(-0.0013f, -0.0033f, 0.00115f), new Quaternion(0f, -1e-07f, 0.9926551f, 0.1209787f), RiggedCompensatedScale),
                Back = new Profile("Ninja_Rig/Root/Body", new Vector3(0f, 0.0043f, -0.0033f), new Quaternion(0.9848185f, 0.1735871f, 0f, -1e-07f), RiggedCompensatedScale),
                Belt = new Profile("Ninja_Rig/Root/Body", new Vector3(0.0031f, 0f, -0.00075f), new Quaternion(0.7071068f, 0f, 0.7071068f, 0f), RiggedCompensatedScale),
            },
            [SurvivorGame.PlayerCharacter.Paladin] = new CharacterProfiles
            {
                defaultVisualScale = 1.05f,
                RightHand = new Profile("Paladin_Rig/Root/Body/Arm.L", new Vector3(0.0037f, -0.0088f, 0.0012f), new Quaternion(0.048963f, -0.0108624f, -0.216311f, 0.9750355f), RiggedCompensatedScale),
                LeftHand = new Profile("Paladin_Rig/Root/Body/Arm.R", new Vector3(-0.0037f, -0.0088f, 0.0012f), new Quaternion(0.048963f, 0.0108624f, 0.216311f, 0.9750355f), RiggedCompensatedScale),
                Back = new Profile("Paladin_Rig/Root/Body", new Vector3(0f, 0.0061f, -0.0052f), new Quaternion(0.9848185f, 0.1735871f, 0f, -1e-07f), RiggedCompensatedScale),
                Belt = new Profile("Paladin_Rig/Root/Body", new Vector3(0.0041f, -0.0003f, -0.001f), new Quaternion(0.7071068f, 0f, 0.7071068f, 0f), RiggedCompensatedScale),
            },
            // Pottu is procedural (no FBX rig) - localPosition/localRotation here are in the driver's
            // OWN local space (metres, unscaled) per WeaponRigidFollower, not a child Transform's
            // localPosition/localScale - his driverPath/localScale fields are unused (see Attach()).
            [SurvivorGame.PlayerCharacter.Pottu] = new CharacterProfiles
            {
                defaultVisualScale = 1.0f,
                RightHand = new Profile(null, new Vector3(0.75f, -0.08f, 0.1f), new Quaternion(0f, 0.7071068f, 0.7071068f, 0f), Vector3.one),
                LeftHand = new Profile(null, new Vector3(-0.76f, -0.15f, 0f), Quaternion.identity, Vector3.one),
                Back = new Profile(null, new Vector3(0f, 0.4f, -0.68f), new Quaternion(0.9848185f, 0.1735871f, 0f, 0f), Vector3.one),
                Belt = new Profile(null, new Vector3(0.76f, -0.37f, -0.12f), new Quaternion(0.7071068f, 0f, 0.7071068f, 0f), Vector3.one),
            },
        };

        static Profile Get(SurvivorGame.PlayerCharacter character, AttachmentSocket socket)
        {
            var profiles = Table[character];
            switch (socket)
            {
                case AttachmentSocket.RightHand: return profiles.RightHand;
                case AttachmentSocket.LeftHand: return profiles.LeftHand;
                case AttachmentSocket.Back: return profiles.Back;
                default: return profiles.Belt;
            }
        }

        public static float DefaultVisualScale(SurvivorGame.PlayerCharacter character) =>
            Table.TryGetValue(character, out var p) ? p.defaultVisualScale : 1f;

        // Resolves the driver bone for a rigged (non-Pottu) character's socket under its instantiated
        // FBX model root, by the bind-pose path recorded in attachment_profiles.json.
        public static Transform FindDriver(Transform modelRoot, SurvivorGame.PlayerCharacter character, AttachmentSocket socket)
        {
            var profile = Get(character, socket);
            var driver = modelRoot ? modelRoot.Find(profile.driverPath) : null;
            if (!driver)
                Debug.LogError($"WeaponAttachmentPoint: driver '{profile.driverPath}' not found under {(modelRoot ? modelRoot.name : "<null model root>")} for {character}/{socket} - check the FBX rig hierarchy (attachment_profiles.json may be out of date with the imported model).");
            return driver;
        }

        // Attaches `weapon` to `character`'s `socket`, using whatever driver Transform
        // `visual.GetSocket(socket)` resolves (a rigged bone for the 8 FBX characters, or Pottu's own
        // roll/pan pivot). `scaleMultiplier` composes with the character's defaultVisualScale (e.g. for
        // a weapon's own Size-stat driven visual growth) on top of the bind-pose placement. Returns
        // false (after logging) if the driver could not be resolved - never silently placed at origin.
        public static bool Attach(Transform weapon, ICharacterVisual visual, SurvivorGame.PlayerCharacter character, AttachmentSocket socket, float scaleMultiplier = 1f)
        {
            var driver = visual.GetSocket(socket);
            if (!driver)
            {
                Debug.LogError($"WeaponAttachmentPoint: {character} has no driver for socket {socket}.");
                return false;
            }
            var profile = Get(character, socket);
            float scale = DefaultVisualScale(character) * scaleMultiplier;
            if (character == SurvivorGame.PlayerCharacter.Pottu)
            {
                var follower = weapon.GetComponent<WeaponRigidFollower>();
                if (!follower) follower = weapon.gameObject.AddComponent<WeaponRigidFollower>();
                weapon.SetParent(((MonoBehaviour)visual).transform, false);
                follower.driver = driver;
                follower.localPosition = profile.localPosition;
                follower.localRotation = profile.localRotation;
                follower.worldScale = scale;
            }
            else
            {
                var stale = weapon.GetComponent<WeaponRigidFollower>();
                if (stale) Object.Destroy(stale);
                weapon.SetParent(driver, false);
                weapon.localPosition = profile.localPosition;
                weapon.localRotation = profile.localRotation;
                weapon.localScale = profile.localScale * scale;
            }
            return true;
        }
    }

    // Rigid follower used only for Pottu (WeaponAttachmentPoint.Attach): reads the driver's world
    // position/rotation every LateUpdate but deliberately ignores the driver's scale, so neither his
    // body's animated squash/stretch nor the Size-stat-driven pan scale (PottuVisual.Tick's
    // "pan.localScale=Vector3.one*Mathf.Clamp(size,1,3.5f)") leaks into the weapon
    // (CONVENTIONS.md "Ajurien seuraaminen ja skaala"). Assumes its own parent (the PottuVisual root)
    // is never itself scaled, which holds today - Pottu's roll/walk/size effects all live on child
    // transforms (pose/torso/pan), never on the visual root.
    public sealed class WeaponRigidFollower : MonoBehaviour
    {
        public Transform driver;
        public Vector3 localPosition;
        public Quaternion localRotation = Quaternion.identity;
        public float worldScale = 1f;

        void LateUpdate()
        {
            if (!driver) return;
            transform.position = driver.position + driver.rotation * localPosition;
            transform.rotation = driver.rotation * localRotation;
            transform.localScale = Vector3.one * worldScale;
        }
    }
}
