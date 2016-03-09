Shader "Custom/Ocean" {
	Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
		_HeightMap ("Height Map", 2D) = "black" {}
		_Height ("Height", Float) = 1
		_BumpMap ("Normal Map", 2D) = "white" {}
		_Cube ("Sky", Cube) = "black" {}
		_ICube ("Sky Intensity", Float) = 1.0
		_F0 ("Fresnel 0", Float) = 0.02
		_Absorb ("Translucency", Float) = 1.0
		_SeaColor ("Sea Color", Color) = (0, 0.467, 0.745, 1)
        _Incline ("Incline", Vector) = (0,0,0,0)

		_Tess ("Tessellation Factor", Range(1,32)) = 4
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200 ZWrite Off ZTest LEqual
		Blend One OneMinusSrcAlpha
		ColorMask RGB
		
		CGPROGRAM
		#pragma target 5.0
		#pragma surface surf Lambert addshadow fullforwardshadows vertex:vert tessellate:tess

		sampler2D _CameraDepthTexture;

        sampler2D _MainTex;
		sampler2D _HeightMap;
        float4 _HeightMap_ST;
		float _Height;
		sampler2D _BumpMap;
		samplerCUBE _Cube;
		float _ICube;
		float _F0;
		float _Absorb;
		float4 _SeaColor;
        float4 _Incline;

		float _Tess;

		struct appdata {
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
		};

		struct Input {
            float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldRefl;
			float3 worldNormal;
			float4 screenPos;
			INTERNAL_DATA
		};
		
		float Fresnel(float3 v, float3 n) {
			float c = 1.0 - saturate(dot(v, n));
			return _F0 + (1.0 - _F0) * c * c * c * c * c;
		}

		float4 tess() {
			return _Tess;
		}
		
		void vert(inout appdata v) {
            float2 uv = TRANSFORM_TEX(v.texcoord.xy, _HeightMap);
			float h = tex2Dlod(_HeightMap, float4(uv, 0, 0));
			v.vertex.xyz += _Height * h * v.normal;
            v.vertex.y += dot(uv, _Incline.xy);
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			float3 n = tex2D(_BumpMap, IN.uv_BumpMap).xyz;
			o.Normal = n;
			IN.worldRefl = WorldReflectionVector(IN, o.Normal);
			IN.worldNormal = WorldNormalVector(IN, o.Normal);
			
			float3 r = normalize(IN.worldRefl);
			float f = Fresnel(r, IN.worldNormal);			
			float4 cRefl = _ICube * texCUBE(_Cube, r);
			
			float sceneZ = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, IN.screenPos).r);
			float oceanZ = IN.screenPos.z;
			float dist = max(0.0, sceneZ - oceanZ);
			float absorb = saturate(1.0 - exp(-_Absorb * dist));
			
			o.Emission = lerp(absorb * _SeaColor.rgb, cRefl.rgb, f);
			o.Alpha = absorb;
		}
		ENDCG
	} 
	FallBack Off
}
