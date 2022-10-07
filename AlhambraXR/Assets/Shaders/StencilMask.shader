Shader "Unlit/StencilMask"
{
    Properties
    {}

        SubShader
    {
        Tags { 
            "RenderType"="Opaque" 
            "Queue" = "Geometry-1"
        }
        LOD 100
        ColorMask 0
        ZWrite off

        Stencil
        {
            Ref  2
            Comp Always
            Pass Replace
        }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = fixed4(v.vertex.x, v.vertex.y, UNITY_NEAR_CLIP_VALUE, v.vertex.w);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                return fixed4(1,1,1,1);
            }
            ENDCG
        }
    }
}
