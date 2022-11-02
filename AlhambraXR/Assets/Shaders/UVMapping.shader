Shader "Unlit/UVMapping"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Stencil
        {
            Ref 2
            Comp Equal
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct fragOutput {
                float4 uv    : SV_Target0;
                fixed4 color : SV_Target1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

#if UNITY_UV_STARTS_AT_TOP == 1
                o.uv     = v.uv;
#else
                o.uv     = 1-v.uv;
#endif
                return o;
            }

            fragOutput frag(v2f i)
            {
                fragOutput o;
                o.uv    = float4(i.uv, 0.0, 1.0);
                o.color = tex2D(_MainTex, i.uv);
                return o;
            }
            ENDCG
        }
    }
}
