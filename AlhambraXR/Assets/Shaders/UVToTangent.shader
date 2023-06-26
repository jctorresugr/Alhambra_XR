Shader "Unlit/UVToTangent"
{
    Properties
    {}
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
                float4 tangent: TANGENT;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 tangent : TANGENT;
            };

            v2f vert (appdata v)
            {
                v2f o;
#if UNITY_UV_STARTS_AT_TOP == 0
                o.vertex = float4(2*v.uv.x - 1, 2*v.uv.y - 1, UNITY_NEAR_CLIP_VALUE, 1);
#else
                o.vertex = float4(2*v.uv.x - 1, 1 - 2*v.uv.y, UNITY_NEAR_CLIP_VALUE, 1);
#endif
                o.tangent = v.tangent;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.tangent;// float4(, 0.0f);
            }
            ENDCG
        }
    }
}
