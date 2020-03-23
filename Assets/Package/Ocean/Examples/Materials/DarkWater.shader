// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

Shader "Custom/DakrWater" {
	Properties {
		_BumpMap ("Normal Map", 2D) = "white" {}
		_LightRough ("Light Roubhness", Float) = 1
		_LightPower ("Light Intensity", Float) = 1
		_F0 ("Fresnel 0", Float) = 0.02
		_SkyCube ("Sky", Cube) = "black" {}
		_SkyPower ("Sky Intensity", Float) = 1.0
		_SeaColor ("Sea Color", Color) = (0, 0, 0, 1)
		_Parallax ("Height", Float) = 0.02
		_WorldViewPos ("World View Position", Vector) = (0, 10, 0, 1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGINCLUDE
		#pragma target 5.0
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"
		#define Custom_WorldNormalVector(data,normal) fixed3(dot(data._TtoW0,normal), dot(data._TtoW1,normal), dot(data._TtoW2,normal))
		
		sampler2D _BumpMap;
		float _LightRough;
		float _LightPower;
		float _F0;
		samplerCUBE _SkyCube;
		float _SkyPower;
		float4 _SeaColor;
		float _Parallax;
		float4 _WorldViewPos;
		float4 _LightColor0;

		struct Input {
			float4 vertex : POSITION;
			float2 uv_BumpMap : TEXCOORD0;
			float3 viewDirForLight : TEXCOORD1;
			float3 lightDir : TEXCOORD2;

			fixed4 _TtoW0 : TEXCOORD3;
			fixed4 _TtoW1 : TEXCOORD4;
			fixed4 _TtoW2 : TEXCOORD5;
			
			LIGHTING_COORDS(6,7)
		};
		
		float Fresnel(float3 v, float3 n) {
			float c = 1.0 - saturate(dot(v, n));
			return _F0 + (1.0 - _F0) * c * c * c * c * c;
		}
		
		inline fixed4 LightingWater(float3 normal, fixed3 lightDir, half3 viewDir, fixed atten) {
			half3 h = normalize(lightDir + viewDir);
			float nh = max (0, dot (normal, h));
			float spec = Fresnel(viewDir, h) * pow(nh, _LightRough) * _LightPower;
			
			fixed4 c;
			c.rgb = (_LightColor0.rgb  * spec) * (atten * 2);
			c.a = _LightColor0.a * spec * atten;
			return c;
		}
		
		inline float3 Custom_ObjSpaceViewDir( in float4 v ) {
			float3 objSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldViewPos.xyz, 1)).xyz * 1.0;
			return objSpaceCameraPos - v.xyz;
		}
		
		Input vert(appdata_full v) {
			TANGENT_SPACE_ROTATION;
			//UNITY_INITIALIZE_OUTPUT(Input, OUT);
			Input OUT;
			
			OUT.vertex = UnityObjectToClipPos(v.vertex);
			float3 viewDir = Custom_ObjSpaceViewDir(v.vertex);
			OUT.viewDirForLight = mul (rotation, viewDir);
			float3 worldViewDir = mul ((float3x3)unity_ObjectToWorld, -viewDir);
			OUT._TtoW0 = float4(mul(rotation, unity_ObjectToWorld[0].xyz), worldViewDir.x)*1.0;
			OUT._TtoW1 = float4(mul(rotation, unity_ObjectToWorld[1].xyz), worldViewDir.y)*1.0;
			OUT._TtoW2 = float4(mul(rotation, unity_ObjectToWorld[2].xyz), worldViewDir.z)*1.0;
			OUT.lightDir = mul (rotation, ObjSpaceLightDir(v.vertex));
			OUT.uv_BumpMap = v.texcoord;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			return OUT;
		}

		float4 frag(Input IN) : COLOR {
			float h = tex2D(_BumpMap, IN.uv_BumpMap).w;
			float2 offset = ParallaxOffset(h, _Parallax, IN.viewDirForLight);
			IN.uv_BumpMap += offset;
			float3 normal = tex2D(_BumpMap, IN.uv_BumpMap).xyz;
			
			float3 worldViewDir = float3(IN._TtoW0.w, IN._TtoW1.w, IN._TtoW2.w);
			float3 worldNormal = Custom_WorldNormalVector(IN, normal);
			float3 worldRefl = reflect(worldViewDir, worldNormal);
			
			float3 r = normalize(worldRefl);
			float f = Fresnel(r, worldNormal);			
			//float4 cRefl = _SkyPower * texCUBE(_SkyCube, r);			
			//o.Emission =  f * cRefl + (1.0 - f) * _SeaColor.rgb;
			float4 c = f * _SkyPower;
			float atten = LIGHT_ATTENUATION(IN);
			c += LightingWater (normal, IN.lightDir, normalize(IN.viewDirForLight), atten);
			return c;
		}
		ENDCG
		
		Pass {
			Tags{"LightMode" = "ForwardBase"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
		
		Pass {
			Tags{"LightMode" = "ForwardAdd"}
			Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	} 
}
