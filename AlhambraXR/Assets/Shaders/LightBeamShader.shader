Shader "Unlit/LightBeamShader"
{
    Properties
    {
        _Color("Color (RGBA)", Color) = (1, 1, 1, 1) 
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            /*
            * A simple light beam shader
            * TODO: make it looks better! (A better model, noise and texture)
            * TODO List:
            *   Shrink when the user comes near by
            *   some particle effect, moving/variation light effect
            *   Gradient: ground -> top (fade out)
            *   Model: ground -> top (big -> small, like a cone, but bend inside)
            */
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 normal: NORMAL;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(v.normal); // do not trust unity normal, it is not normalized :( (waste 2 hours)
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float normalFactor = i.normal.z;
                normalFactor *= normalFactor;
                normalFactor *= normalFactor;
                float timeFactor = 0.5f + 0.25f * sin(_Time * 40.0f);
                fixed4 col = _Color* normalFactor* timeFactor;
                //col = float4(i.normal * 0.5 + 0.5,0.5f);
                return col;
            }
            ENDCG
        }
    }
}
