Shader "Unlit/NavigateLineShader"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _UVRepeat("UVRepeat", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            float _UVRepeat;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.uv = float2(frac(v.uv.x *70.23845f), v.uv.y);
                o.uv = float2(v.uv.x * _UVRepeat,v.uv.y);
                o.color = v.color;
                return o;
            }

            float sqr(float x)
            {
                return x * x;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //TODO: optimize!
                // sample the texture
                float2 uv = float2(frac(i.uv.x),i.uv.y);
                float y0 = abs(uv.y - 0.5f)*1.5f;

                //arrow
                float y1 = (0.5f-uv.x*0.6)*0.6f;
                float y2 = (0.25f - uv.x*0.6)*0.2f;
                y2 *= y2*y2*3000.0f;
                float colorFactor = step(y2, y0) * step(y0, y1); // return 1 if y2<=y0<=y1 else 0
                //float colorFactor = smoothstep(y2-0.15f, y0, y2) * smoothstep(y0, y1, y0+0.2f);
                fixed4 col = colorFactor*0.5f+i.color;
                //col = fixed4(y2,y0, y1, 1.0f);
                return col;
            }
            ENDCG
        }
    }
}
