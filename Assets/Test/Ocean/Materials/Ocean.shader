Shader "Custom/Ocean" {
	Properties {
		_BumpMap ("Normal Map", 2D) = "white" {}
		_Cube ("Sky", Cube) = "black" {}
		_ICube ("Sky Intensity", Float) = 1.0
		_F0 ("Fresnel 0", Float) = 0.02
		_Absorb ("Translucency", Float) = 1.0
		_SeaColor ("Sea Color", Color) = (0, 0.467, 0.745, 1)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200 ZWrite On ZTest LEqual
		Blend One OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 5.0
		#pragma surface surf Lambert vertex:vert

		sampler2D _CameraDepthTexture;

		sampler2D _BumpMap;
		samplerCUBE _Cube;
		float _ICube;
		float _F0;
		float _Absorb;
		float4 _SeaColor;

		struct Input {
			float2 uv_BumpMap;
			float3 worldRefl; INTERNAL_DATA
			float3 worldNormal;
			float4 screenPos;
		};
		
		float Fresnel(float3 v, float3 n) {
			float c = 1.0 - saturate(dot(v, n));
			return _F0 + (1.0 - _F0) * c * c * c * c * c;
		}
		
		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			float4 projPos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.screenPos = ComputeScreenPos(projPos);
			COMPUTE_EYEDEPTH(o.screenPos.z);
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			float3 n = tex2D(_BumpMap, IN.uv_BumpMap).xyz;
			
			o.Normal = n;
			IN.worldRefl = WorldReflectionVector(IN, o.Normal);
			IN.worldNormal = WorldNormalVector(IN, o.Normal);
			
			float3 r = normalize(IN.worldRefl);
			float f = Fresnel(r, IN.worldNormal);			
			float4 cRefl = _ICube * texCUBE(_Cube, r);
			
			float sceneZ = DECODE_EYEDEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)).r);
			float oceanZ = IN.screenPos.z;
			float dist = max(0.0, sceneZ - oceanZ);
			float absorb = saturate(1.0 - exp(-_Absorb * dist));
			
			o.Emission = f * cRefl.rgb + (1.0 - f) * absorb * _SeaColor.rgb;
			o.Alpha = absorb;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
