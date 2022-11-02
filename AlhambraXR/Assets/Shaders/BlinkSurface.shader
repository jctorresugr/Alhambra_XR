Shader "Custom/BlinkSurface"
{
    Properties
    {
        _Color      ("Color", Color)           = (1,1,1,1)
        _MainTex    ("Albedo (RGB)", 2D)       = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic   ("Metallic", Range(0,1))   = 0.0

        _IndexTex   ("Index Texture", 2D)      = "white" {}
        _IDRight    ("IDRight",Int)            = 254
        _LayerRight ("LayerRight", Int)        = 0
        _IDLeft     ("IDLeft",Int)             = 254
        _LayerLeft  ("LayerLeft", Int)         = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        // Physically based Standard lighting model
        #pragma surface surf Standard

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 3.0

        sampler2D _MainTex;
 //       float4 _Color;

        sampler2D _IndexTex;
        float4    _IndexTex_ST;
//        float4 _Index;

        int _LayerRight;
        int _IDRight;
        int _LayerLeft;
        int _IDLeft;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            // sample the texture
            fixed4 index = tex2D(_IndexTex, IN.uv_MainTex);
            fixed indexArray[4] = { index.r, index.g, index.b, index.a };
            float sinTime = sin(2*_Time[1]);
            if      (_LayerRight < 4 && ((indexArray[_LayerRight]) * 255 == _IDRight)) { o.Albedo = c.rgb + 0.6 * (sinTime * sinTime) * _Color; }
            else if (_LayerLeft  < 4 && ((indexArray[_LayerLeft]) * 255  == _IDLeft))  { o.Albedo = c.rgb + 0.6 * (sinTime * sinTime) * _Color; }

            //if ((indexArray[0]) * 255 > 0) { o.Albedo = c.rgb + 0.6 * (_SinTime[3] * _SinTime[3]) * _Color; }  // Debuging 

            // Metallic and smoothness come from slider variables
            o.Metallic   = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha      = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
