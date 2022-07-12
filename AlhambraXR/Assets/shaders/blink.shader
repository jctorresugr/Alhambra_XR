Shader "Unlit/blink"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _IndexTex("Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (1,1,1,1)
        _ID("ID",Int) = 254
        _Layer("Layer", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            sampler2D _IndexTex;
            float4 _IndexTex_ST;
            float4 _Index;

            fixed _Layer;
            fixed _ID;

            //float4 _SinTime;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float d;

                // sample the texture
                fixed4 index = tex2D(_IndexTex, i.uv);
                fixed4 col = tex2D(_MainTex, i.uv);
                
                //   d = 0.3 * (_SinTime[3] * _SinTime[3]);
                //   if (index[0] + index[1] + index[2]) > 0.0) {
                //       col[0] = col[0] + d * (1 - col[0]);
                //       col[1] = col[1] + d * (1 - col[1]);
                //   }
                
                fixed indexArray[4] = { index.r, index.g, index.b, index.a };
                d = 0.3*( _SinTime[3] * _SinTime[3]);
                if (_Layer < 4 && (indexArray[_Layer] == _ID / 255.0)) { col = col + 0.3 * (_SinTime[3] * _SinTime[3]); }
        //        if (index[layer] == id) { col[layer] = col[layer] + 0.3 * (_SinTime[3] * _SinTime[3]); }
    //                if (index[1] > 0) { col[1] = col[1] + 0.3 * (_CosTime[3] * _CosTime[3]); }
    //                if (index[2] > 0) { col[2] = col[2] + 0.3 * (_CosTime[3] * _SinTime[3]); }
    
                // apply fog

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

