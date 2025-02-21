Shader "Custom/SortingPBRShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}   // 颜色贴图
        _BumpMap ("Normal Map", 2D) = "bump" {}      // 法线贴图
        _Metallic ("Metallic", Range(0,1)) = 0.5     // 金属度
        _Smoothness ("Smoothness", Range(0,1)) = 0.5 // 光滑度
    }

    SubShader
    {
        Tags { "Queue"="Transparent+1" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade

        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _Metallic;
        half _Smoothness;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Alpha = c.a;  // 透明度
        }
        ENDCG
    }

    FallBack "Standard"
}
