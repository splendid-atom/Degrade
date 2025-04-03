// Made with Amplify Shader Editor v1.9.7.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "/_Vefects_/SH_Vefects_Zap"
{
	Properties
	{
		[Space(33)][Header(Zap)][Space(13)]_ZapTexture("Zap Texture", 2D) = "white" {}
		_EmissiveIntensity("Emissive Intensity", Float) = 1
		_ErosionSmoothness("Erosion Smoothness", Float) = 0.01
		_TextureMultiply("Texture Multiply", Float) = 0
		[Space(33)][Header(LUT)][Space(13)]_LUT("LUT", 2D) = "white" {}
		_LUTAmplitude("LUT Amplitude", Float) = 1
		_LUTOffset("LUT Offset", Float) = 0
		_LUTErosion("LUT Erosion", Float) = 0
		_LUTErosionOffset("LUT Erosion Offset", Float) = 0
		_LUTErosionSmoothness("LUT Erosion Smoothness", Float) = 0.3
		[Space(33)][Header(AR)][Space(13)]_Cull("Cull", Float) = 2
		_Src("Src", Float) = 5
		_Dst("Dst", Float) = 10
		_ZWrite("ZWrite", Float) = 0
		_ZTest("ZTest", Float) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull [_Cull]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		Blend [_Src] [_Dst]
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#define ASE_VERSION 19701
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _Src;
		uniform float _Dst;
		uniform float _ZWrite;
		uniform float _ZTest;
		uniform float _Cull;
		uniform sampler2D _LUT;
		uniform float _LUTErosionOffset;
		uniform float _LUTErosionSmoothness;
		uniform sampler2D _ZapTexture;
		uniform float4 _ZapTexture_ST;
		uniform float _ErosionSmoothness;
		uniform float _LUTErosion;
		uniform float _LUTAmplitude;
		uniform float _LUTOffset;
		uniform float _TextureMultiply;
		uniform float _EmissiveIntensity;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float temp_output_46_0 = ( i.uv_texcoord.z + _LUTErosionOffset );
			float2 uv_ZapTexture = i.uv_texcoord * _ZapTexture_ST.xy + _ZapTexture_ST.zw;
			float4 tex2DNode10 = tex2D( _ZapTexture, uv_ZapTexture );
			float smoothstepResult45 = smoothstep( temp_output_46_0 , ( temp_output_46_0 + _LUTErosionSmoothness ) , tex2DNode10.g);
			float smoothstepResult27 = smoothstep( i.uv_texcoord.z , ( i.uv_texcoord.z + _ErosionSmoothness ) , tex2DNode10.g);
			float temp_output_33_0 = saturate( smoothstepResult27 );
			float lerpResult41 = lerp( smoothstepResult45 , temp_output_33_0 , _LUTErosion);
			float2 temp_cast_0 = (( ( lerpResult41 * _LUTAmplitude ) + _LUTOffset )).xx;
			float3 temp_output_35_0 = ( tex2D( _LUT, temp_cast_0 ).rgb * (i.vertexColor).rgb );
			float2 temp_cast_1 = (( ( lerpResult41 * _LUTAmplitude ) + _LUTOffset )).xx;
			float3 lerpResult39 = lerp( temp_output_35_0 , ( temp_output_35_0 * temp_output_33_0 ) , _TextureMultiply);
			o.Emission = ( lerpResult39 * _EmissiveIntensity );
			o.Alpha = saturate( ( temp_output_33_0 * i.vertexColor.a ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xyzw = customInputData.uv_texcoord;
				o.customPack1.xyzw = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xyzw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.vertexColor = IN.color;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19701
Node;AmplifyShaderEditor.TexCoordVertexDataNode;26;-1920,768;Inherit;False;0;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;30;-1408,1024;Inherit;False;Property;_ErosionSmoothness;Erosion Smoothness;3;0;Create;True;0;0;0;False;0;False;0.01;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-1664,1408;Inherit;False;Property;_LUTErosionOffset;LUT Erosion Offset;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-1408,896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;10;-1920,384;Inherit;True;Property;_ZapTexture;Zap Texture;1;0;Create;True;0;0;0;False;3;Space(33);Header(Zap);Space(13);False;-1;d3fe6c8361bf4bf49bf597008abb96c8;d3fe6c8361bf4bf49bf597008abb96c8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-1664,1280;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1408,1536;Inherit;False;Property;_LUTErosionSmoothness;LUT Erosion Smoothness;10;0;Create;True;0;0;0;False;0;False;0.3;0.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;27;-1408,768;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;-1408,1408;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;33;-1152,768;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;45;-1408,1280;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-1408,512;Inherit;False;Property;_LUTErosion;LUT Erosion;8;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-2432,128;Inherit;False;Property;_LUTAmplitude;LUT Amplitude;6;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;41;-1408,384;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-2432,0;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-2176,128;Inherit;False;Property;_LUTOffset;LUT Offset;7;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;25;-1920,-512;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-2176,0;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;36;-1664,-512;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;11;-1920,0;Inherit;True;Property;_LUT;LUT;5;0;Create;True;0;0;0;False;3;Space(33);Header(LUT);Space(13);False;-1;f4179423434cbd74e85a336802d71fb6;f4179423434cbd74e85a336802d71fb6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-1152,0;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-912,-304;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-896,128;Inherit;False;Property;_TextureMultiply;Texture Multiply;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;24;974,-50;Inherit;False;1252;162.95;Lush was here! <3;5;20;21;22;23;19;Lush was here! <3;0,0,0,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-768,768;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-512,128;Inherit;False;Property;_EmissiveIntensity;Emissive Intensity;2;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;39;-896,0;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;20;1280,0;Inherit;False;Property;_Src;Src;12;0;Create;True;0;0;0;True;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;1536,0;Inherit;False;Property;_Dst;Dst;13;0;Create;True;0;0;0;True;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;1792,0;Inherit;False;Property;_ZWrite;ZWrite;14;0;Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;2048,0;Inherit;False;Property;_ZTest;ZTest;15;0;Create;True;0;0;0;True;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;1024,0;Inherit;False;Property;_Cull;Cull;11;0;Create;True;0;0;0;True;3;Space(33);Header(AR);Space(13);False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;37;-512,768;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-512,0;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;50;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;/_Vefects_/SH_Vefects_Zap;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;True;_ZWrite;0;True;_ZTest;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;True;_Src;10;True;_Dst;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;True;_Cull;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;28;0;26;3
WireConnection;28;1;30;0
WireConnection;46;0;26;3
WireConnection;46;1;48;0
WireConnection;27;0;10;2
WireConnection;27;1;26;3
WireConnection;27;2;28;0
WireConnection;44;0;46;0
WireConnection;44;1;43;0
WireConnection;33;0;27;0
WireConnection;45;0;10;2
WireConnection;45;1;46;0
WireConnection;45;2;44;0
WireConnection;41;0;45;0
WireConnection;41;1;33;0
WireConnection;41;2;42;0
WireConnection;12;0;41;0
WireConnection;12;1;15;0
WireConnection;13;0;12;0
WireConnection;13;1;16;0
WireConnection;36;0;25;0
WireConnection;11;1;13;0
WireConnection;35;0;11;5
WireConnection;35;1;36;0
WireConnection;38;0;35;0
WireConnection;38;1;33;0
WireConnection;34;0;33;0
WireConnection;34;1;25;4
WireConnection;39;0;35;0
WireConnection;39;1;38;0
WireConnection;39;2;40;0
WireConnection;37;0;34;0
WireConnection;17;0;39;0
WireConnection;17;1;18;0
WireConnection;50;2;17;0
WireConnection;50;9;37;0
ASEEND*/
//CHKSM=E1D88E267F03FD0F59D811C370D8E4A78943CB66