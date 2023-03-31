Shader "Unlit/UVToNormal"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
#if UNITY_UV_STARTS_AT_TOP == 0
                o.vertex = float4(2 * v.uv.x - 1, 2 * v.uv.y - 1, UNITY_NEAR_CLIP_VALUE, 1);
#else
                o.vertex = float4(2 * v.uv.x - 1, 1 - 2 * v.uv.y, UNITY_NEAR_CLIP_VALUE, 1);
#endif
                o.normal = v.normal;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(i.normal, 0.0f);
            }
            ENDCG
        }
    }
}
