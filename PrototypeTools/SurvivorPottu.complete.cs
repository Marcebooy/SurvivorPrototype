using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BonkSurvivor
{
    public sealed partial class SurvivorGame
    {
        PottuVisual pottu;
        float dodgeLeft, dodgeCooldown, hurtFlash;
        Vector3 dodgeDirection;
        public bool IsDodging => dodgeLeft > 0;
        public float DodgeCooldown => dodgeCooldown;
        readonly List<DamageLabel> damageLabels = new List<DamageLabel>();
        sealed class DamageLabel { public Vector3 position; public string text; public float life; public bool crit; }
        readonly Dictionary<Renderer,float> hitFlashes=new Dictionary<Renderer,float>();
        readonly List<Renderer> flashKeys=new List<Renderer>();
        MaterialPropertyBlock flashBlock;
        Mesh swipeMesh;

        void BuildSwipe()
        {
            var v=new List<Vector3>(); var t=new List<int>();
            for(int i=0;i<=24;i++)
            {
                float a=Mathf.Lerp(-104,104,i/24f)*Mathf.Deg2Rad;
                var d=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a)); v.Add(d*.465f); v.Add(d*.5f);
                if(i<24) { int n=i*2; t.Add(n);t.Add(n+1);t.Add(n+2);t.Add(n+1);t.Add(n+3);t.Add(n+2); }
            }
            swipeMesh=new Mesh { name="Pottu pan swipe arc" }; swipeMesh.SetVertices(v); swipeMesh.SetTriangles(t,0); swipeMesh.RecalculateNormals(); swipeMesh.RecalculateBounds();
            slash.GetComponent<MeshFilter>().sharedMesh=swipeMesh;
        }

        void BuildPottu()
        {
            player=new GameObject("Pottu").transform; player.SetParent(world,false); player.position=Vector3.up;
            pottu=player.gameObject.AddComponent<PottuVisual>(); pottu.Build();
        }
        void ResetPottu()
        {
            dodgeLeft=dodgeCooldown=hurtFlash=0; damageLabels.Clear();
            foreach(var pair in hitFlashes) if(pair.Key) pair.Key.SetPropertyBlock(null);
            hitFlashes.Clear(); player.rotation=Quaternion.Euler(0,180,0); pottu.ResetPose();
        }
        public bool TryDodge(Vector3 direction)
        {
            if(Selecting || ShopOpen || PendingChoices>0 || Finished || dodgeCooldown>0 || IsDodging) return false;
            direction.y=0; dodgeDirection=direction.sqrMagnitude>.001f ? direction.normalized : player.forward;
            dodgeLeft=.38f; dodgeCooldown=1.6f; player.rotation=Quaternion.LookRotation(dodgeDirection); return true;
        }
        void MovePottu(Vector3 move, float dt, bool dodgePressed)
        {
            dodgeCooldown=Mathf.Max(0,dodgeCooldown-dt);
            if(dodgePressed) TryDodge(move);
            bool rolling=IsDodging;
            var motion=rolling ? dodgeDirection*(18f*Mathf.Min(dt,dodgeLeft)) : move*(moveSpeed*dt);
            var next=player.position+motion; next.y=0;
            player.position=Vector3.ClampMagnitude(next,arenaRadius-1)+Vector3.up;
            if(!rolling && move.sqrMagnitude>.01f) player.rotation=Quaternion.Slerp(player.rotation,Quaternion.LookRotation(move),dt*14);
            dodgeLeft=Mathf.Max(0,dodgeLeft-dt);
            float roll=IsDodging ? 1-dodgeLeft/.38f : -1;
            float panSize=Size*(1+(weaponLevels.TryGetValue(Weapon.Sword,out int level) ? level-1 : 0)*.13f);
            pottu.Tick(dt,rolling ? 0 : move.magnitude,roll,panSize,weaponLevels.ContainsKey(Weapon.Sword));
            TickFeedback(dt);
        }
        bool ReceiveDamage(float amount)
        {
            if(IsDodging || invulnerability>0 || Finished) return false;
            if(BlockWithAegis()) return false;
            Health=Mathf.Max(0,Health-Mathf.Max(1,amount-Armor)); invulnerability=.45f; hurtFlash=.25f;
            if(Health<=0) FinishRun(); return true;
        }
        void HitFeedback(Enemy enemy, float amount, bool crit)
        {
            if(damageLabels.Count>=60) damageLabels.RemoveAt(0);
            damageLabels.Add(new DamageLabel { position=enemy.body.position+Vector3.up*1.25f, text=(crit ? "BONK! " : "")+Mathf.CeilToInt(amount),life=.7f,crit=crit });
            var renderer=enemy.body.GetComponent<Renderer>();
            if(renderer) { if(flashBlock==null) flashBlock=new MaterialPropertyBlock(); flashBlock.SetColor("_BaseColor",Color.white); renderer.SetPropertyBlock(flashBlock); hitFlashes[renderer]=.1f; }
            pottu.Bonk();
        }
        void TickFeedback(float dt)
        {
            hurtFlash=Mathf.Max(0,hurtFlash-dt);
            for(int i=damageLabels.Count-1;i>=0;i--) { var label=damageLabels[i]; label.life-=dt; label.position+=Vector3.up*dt*1.6f; if(label.life<=0) damageLabels.RemoveAt(i); }
            flashKeys.Clear(); flashKeys.AddRange(hitFlashes.Keys);
            foreach(var r in flashKeys)
            { if(!r) { hitFlashes.Remove(r); continue; } float left=hitFlashes[r]-dt; if(left<=0) { r.SetPropertyBlock(null); hitFlashes.Remove(r); } else hitFlashes[r]=left; }
        }
        void DrawPottuHUD()
        {
            if(Selecting || PendingChoices>0 || ShopOpen || Finished) return;
            DrawAdvancedStatus();
            GUI.Label(new Rect(24,600,330,32),IsDodging ? "POTTU PYÖRII!" : dodgeCooldown>0 ? "Väistö: "+dodgeCooldown.ToString("0.0")+" s" : "Väistö valmis [SPACE]",textStyle);
            foreach(var label in damageLabels)
            {
                var p=followCamera.WorldToViewportPoint(label.position); if(p.z<=0) continue;
                GUI.color=label.crit ? new Color(1,.85f,.2f,Mathf.Clamp01(label.life*3)) : new Color(1,1,1,Mathf.Clamp01(label.life*3));
                GUI.Label(new Rect(p.x*1280-35,(1-p.y)*720-16,180,36),label.text,textStyle);
            }
            if(hurtFlash>0)
            { GUI.color=new Color(1,.15f,.1f,hurtFlash*.65f); GUI.DrawTexture(new Rect(0,0,1280,8),Texture2D.whiteTexture); GUI.DrawTexture(new Rect(0,712,1280,8),Texture2D.whiteTexture); }
            GUI.color=Color.white;
        }
    }
}


