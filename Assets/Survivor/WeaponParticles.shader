Shader "BonkSurvivor/WeaponParticles"
{
 Properties { _MainTex("Particle",2D)="white"{} _Tint("Tint",Color)=(1,1,1,1) }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
 Pass {
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
 CBUFFER_START(UnityPerMaterial)
 half4 _Tint;
 CBUFFER_END
 struct Attributes { float4 positionOS:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
 struct Varyings { float4 positionCS:SV_POSITION; half4 color:COLOR; float2 uv:TEXCOORD0; };
 Varyings vert(Attributes i) { Varyings o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.color=i.color; o.uv=i.uv; return o; }
 half4 frag(Varyings i):SV_Target { half4 tex=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv); tex.a*=max(tex.r,max(tex.g,tex.b)); return tex*i.color*_Tint; }
 ENDHLSL
 }
 }
}
