Shader "Festival/Toon Particles"
{
    Properties
    {
        _MainTex ("Particle texture", 2D) = "white" {}
        _TintColor ("Tint", Color) = (0.5,0.5,0.5,0.5)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination blend", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend [_SrcBlend] [_DstBlend]
        Cull Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _TintColor;
            struct appdata { float4 vertex:POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; };
            v2f vert(appdata v)
            {
                v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.color=v.color;
                o.uv=TRANSFORM_TEX(v.uv,_MainTex); return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 color=2*i.color*_TintColor*tex2D(_MainTex,i.uv);
                color.a=saturate(color.a); return color;
            }
            ENDCG
        }
    }
    Fallback Off
}
