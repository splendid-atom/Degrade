Shader "Custom/BrushStroke"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _BrushTexture ("Brush Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0, 0, 0, 1)
        _EdgeBlur ("Edge Blur", Range(0, 1)) = 0.5
        _BrushScale ("Brush Scale", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _BrushTexture;
            float4 _Color;
            float _EdgeBlur;
            float _BrushScale;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // 获取基础颜色
                half4 col = tex2D(_MainTex, i.uv) * _Color;

                // 获取毛笔纹理
                half4 brush = tex2D(_BrushTexture, i.uv * _BrushScale);

                // 增加边缘模糊效果
                float edge = smoothstep(0.4, 0.5, brush.a);
                col.rgb *= edge;
                col.a *= edge;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
