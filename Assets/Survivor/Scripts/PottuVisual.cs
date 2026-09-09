using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    // Procedural low-poly character; its visual pose never changes the gameplay root.
    public sealed class PottuVisual : MonoBehaviour, ICharacterVisual
    {
        Transform pose, torso, helmet, leftFoot, rightFoot, arm, pan;
        Material skin, dark, steel, rim, white, leather;
        readonly List<Material> owned = new List<Material>();
        Mesh roundMesh, domeMesh;
        AudioSource audioSource;
        AudioClip bonk;
        float phase, swing, soundCooldown;
        Quaternion swingFacing = Quaternion.identity;
        public float PanScale => pan ? pan.localScale.x : 0;

        // WeaponAttachmentPoint driver: RightHand rides the swing pivot (unscaled), everything else
        // rides the potato body directly. WeaponRigidFollower (not this Transform's own localScale)
        // is what keeps the weapon out of torso's animated squash/stretch and pan's Size-stat scale.
        public Transform GetSocket(AttachmentSocket socket) => socket == AttachmentSocket.RightHand ? arm : torso;

        Material Mat(Color color)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color = color;
            owned.Add(m); return m;
        }

        Transform Part(string label, PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material material, bool dome = false)
        {
            var go = GameObject.CreatePrimitive(type); go.name = label;
            go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material; Destroy(go.GetComponent<Collider>());
            if (type == PrimitiveType.Sphere) go.GetComponent<MeshFilter>().sharedMesh = dome ? domeMesh : roundMesh;
            return go.transform;
        }

        static Mesh Faceted(bool hemisphere)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            int rings = hemisphere ? 4 : 8, sides = 12;
            Vector3 Point(int r, int s)
            {
                float a = r * Mathf.PI / 8, b = s * Mathf.PI * 2 / sides;
                return new Vector3(Mathf.Sin(a)*Mathf.Cos(b), Mathf.Cos(a), Mathf.Sin(a)*Mathf.Sin(b)) * .5f;
            }
            void Triangle(Vector3 a, Vector3 b, Vector3 c)
            { int n=vertices.Count; vertices.Add(a); vertices.Add(b); vertices.Add(c); triangles.Add(n); triangles.Add(n+1); triangles.Add(n+2); }
            for(int r=0;r<rings;r++) for(int s=0;s<sides;s++)
            { var a=Point(r,s); var b=Point(r+1,s); var c=Point(r+1,s+1); var d=Point(r,s+1); Triangle(a,d,b); Triangle(d,c,b); }
            var mesh = new Mesh { name = hemisphere ? "Pottu helmet dome" : "Pottu faceted mesh" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        public void Build()
        {
            roundMesh = Faceted(false); domeMesh = Faceted(true);
            skin = Mat(new Color(.73f,.43f,.18f)); dark = Mat(new Color(.045f,.065f,.075f));
            steel = Mat(new Color(.35f,.51f,.59f)); steel.SetFloat("_Metallic", .65f);
            rim = Mat(new Color(.68f,.79f,.8f)); rim.SetFloat("_Metallic",.7f);
            white = Mat(new Color(1,.96f,.8f)); leather = Mat(new Color(.22f,.10f,.055f));
            pose = new GameObject("Pottu roll pivot").transform; pose.SetParent(transform,false);
            torso = new GameObject("Wobbly potato").transform; torso.SetParent(pose,false);
            Part("Potato body", PrimitiveType.Sphere, torso, new Vector3(0,.05f,0), new Vector3(1.4f,1.8f,1.1f), skin);
            for(int i=0;i<9;i++)
            {
                float a=i*2.4f;
                Part("Potato dimple",PrimitiveType.Sphere,torso,new Vector3(Mathf.Sin(a)*.56f,-.45f+i*.105f,Mathf.Cos(a)*.44f),Vector3.one*.085f,leather);
            }
            Part("Nose",PrimitiveType.Sphere,torso,new Vector3(0,.08f,.59f),new Vector3(.3f,.25f,.25f),skin);
            for(int side=-1;side<=1;side+=2)
            {
                Part("Eye",PrimitiveType.Sphere,torso,new Vector3(side*.26f,.3f,.49f),new Vector3(.34f,.32f,.14f),white);
                Part("Determined pupil",PrimitiveType.Sphere,torso,new Vector3(side*.24f,.3f,.565f),new Vector3(.12f,.18f,.06f),dark);
                var brow=Part("Angry eyebrow",PrimitiveType.Cube,torso,new Vector3(side*.26f,.47f,.57f),new Vector3(.4f,.085f,.1f),dark);
                brow.localRotation=Quaternion.Euler(0,0,side*18);
            }
            Part("Grumpy mouth",PrimitiveType.Cube,torso,new Vector3(0,-.2f,.535f),new Vector3(.35f,.06f,.07f),dark);
            helmet=new GameObject("Oversized helmet").transform; helmet.SetParent(torso,false); helmet.localPosition=new Vector3(0,.65f,0);
            Part("Helmet shell",PrimitiveType.Sphere,helmet,Vector3.zero,new Vector3(1.95f,1.5f,1.65f),steel,true);
            Part("Helmet brim",PrimitiveType.Cylinder,helmet,Vector3.zero,new Vector3(2.05f,.07f,1.8f),rim);
            Part("Helmet spike",PrimitiveType.Sphere,helmet,new Vector3(0,.8f,0),new Vector3(.22f,.42f,.22f),rim);
            var strap=Part("Crooked chin strap",PrimitiveType.Cube,torso,new Vector3(.52f,-.03f,.4f),new Vector3(.09f,.75f,.12f),leather); strap.localRotation=Quaternion.Euler(0,0,-15);
            leftFoot=Part("Left boot",PrimitiveType.Sphere,pose,new Vector3(-.38f,-.8f,.08f),new Vector3(.42f,.4f,.62f),leather);
            rightFoot=Part("Right boot",PrimitiveType.Sphere,pose,new Vector3(.38f,-.8f,.08f),new Vector3(.42f,.4f,.62f),leather);
            Part("Left mitten",PrimitiveType.Sphere,torso,new Vector3(-.76f,-.15f,0),new Vector3(.35f,.45f,.35f),skin);
            arm=new GameObject("Pan swing pivot").transform; arm.SetParent(pose,false);
            pan=new GameObject("Growing frying pan").transform; pan.SetParent(arm,false);
            pan.localPosition=new Vector3(.75f,-.08f,.1f);
            Part("Right mitten",PrimitiveType.Sphere,pan,Vector3.zero,Vector3.one*.32f,skin);
            Part("Pan handle",PrimitiveType.Cube,pan,new Vector3(0,0,.43f),new Vector3(.16f,.15f,.95f),leather);
            Part("Pan outer rim",PrimitiveType.Cylinder,pan,new Vector3(0,0,1.27f),new Vector3(1.13f,.095f,1.13f),rim);
            Part("Cast iron pan",PrimitiveType.Cylinder,pan,new Vector3(0,.075f,1.27f),new Vector3(.99f,.04f,.99f),dark);
            audioSource=gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake=false; audioSource.spatialBlend=0; audioSource.volume=.3f;
            const int sampleRate=22050; var samples=new float[(int)(sampleRate*.28f)];
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/sampleRate;
                samples[i]=Mathf.Exp(-t*19)*(Mathf.Sin(2*Mathf.PI*310*t)*.48f+Mathf.Sin(2*Mathf.PI*827*t)*.26f+Mathf.Sin(2*Mathf.PI*1463*t)*.15f);
            }
            bonk=AudioClip.Create("BONK - synthesized cast iron",samples.Length,1,sampleRate,false); bonk.SetData(samples,0);
            ResetPose();
        }

        public void ResetPose()
        {
            phase=swing=soundCooldown=0; pose.localRotation=Quaternion.identity; torso.localScale=Vector3.one;
            torso.localPosition=Vector3.zero; torso.localRotation=Quaternion.identity; pan.localScale=Vector3.one;
            arm.localRotation=Quaternion.identity; if(audioSource) audioSource.Stop();
        }

        public void Swing(Vector3 worldAim)
        {
            swing=.3f; var local=transform.InverseTransformDirection(worldAim); local.y=0;
            swingFacing=local.sqrMagnitude>.001f ? Quaternion.LookRotation(local) : Quaternion.identity;
        }
        public void Bonk()
        {
            if(soundCooldown>0) return;
            audioSource.pitch=Random.Range(.88f,1.12f); audioSource.PlayOneShot(bonk); soundCooldown=.055f;
        }
        public void Block() { }
        public void Shockwave() { }
        public void Tick(float dt, float speed, float roll, float size, bool showPan, float healthFraction)
        {
            phase+=dt*16*speed; swing=Mathf.Max(0,swing-dt); soundCooldown=Mathf.Max(0,soundCooldown-dt);
            float step=Mathf.Sin(phase)*speed;
            leftFoot.localPosition=new Vector3(-.38f,-.8f+Mathf.Max(0,step)*.18f,.08f+step*.2f);
            rightFoot.localPosition=new Vector3(.38f,-.8f+Mathf.Max(0,-step)*.18f,.08f-step*.2f);
            torso.localPosition=new Vector3(0,Mathf.Abs(step)*.09f,0);
            torso.localRotation=Quaternion.Euler(step*3,0,step*7);
            torso.localScale=new Vector3(1+Mathf.Abs(step)*.045f,1-Mathf.Abs(step)*.045f,1);
            helmet.localRotation=Quaternion.Euler(step*6,0,Mathf.Sin(phase-.5f)*speed*9-6);
            pose.localRotation=roll>=0 ? Quaternion.Euler(roll*360,0,0) : Quaternion.identity;
            pan.gameObject.SetActive(showPan); pan.localScale=Vector3.one*Mathf.Clamp(size,1,3.5f);
            arm.localRotation=swing>0 ? swingFacing*Quaternion.Euler(0,Mathf.Lerp(-100,100,1-swing/.3f),-15) : Quaternion.Euler(0,15,0);
        }
        void OnDestroy()
        {
            foreach(var m in owned) if(m) Destroy(m);
            if(roundMesh) Destroy(roundMesh); if(domeMesh) Destroy(domeMesh); if(bonk) Destroy(bonk);
        }
    }
}
