Shader "Custom/BlinkSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

//        _MainTex("Texture", 2D) = "white" {}
        _IndexTex("Texture", 2D) = "white" {}
//        _Color("Main Color", Color) = (1,1,1,1)
        _ID("ID",Int) = 254
        _Layer("Layer", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

 //       float4 _MainTex_ST;
 //       float4 _Color;

        sampler2D _IndexTex;
        float4 _IndexTex_ST;
//        float4 _Index;

        fixed _Layer;
        fixed _ID;

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
   //         fixed4 col = tex2D(_MainTex, i.uv);

            fixed indexArray[4] = { index.r, index.g, index.b, index.a };
 //           d = 0.3 * (_SinTime[3] * _SinTime[3]);
            if (_Layer < 4 && ((indexArray[_Layer])*255 == _ID)) { o.Albedo = c.rgb + 0.6 * (_SinTime[3] * _SinTime[3]) * _Color; }

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
