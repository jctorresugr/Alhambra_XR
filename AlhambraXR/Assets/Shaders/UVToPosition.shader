Shader "Unlit/UVToPosition"
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
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 color  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
#if UNITY_UV_STARTS_AT_TOP == 0
                o.vertex = float4(2*v.uv.x - 1, 2*v.uv.y - 1, UNITY_NEAR_CLIP_VALUE, 1);
#else
                o.vertex = float4(2*v.uv.x - 1, 1 - 2*v.uv.y, UNITY_NEAR_CLIP_VALUE, 1);
#endif
                o.color = v.vertex.xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                //return float4(1.0, 1.0, 1.0, 1.0);
                return float4(i.color, 1.0);
            }
            ENDCG
        }
    }
}
