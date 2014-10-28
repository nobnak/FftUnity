Shader "Custom/Ocean" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "white" {}
		_Cube ("Sky", Cube) = "black" {}
		_ICube ("Sky Intensity", Float) = 1.0
		_F0 ("Fresnel 0", Float) = 0.02
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200 ZWrite On ZTest Always
		Blend One OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _BumpMap;
		samplerCUBE _Cube;
		float _ICube;
		float _F0;

		struct Input {
			float2 uv_MainTex;
			float3 worldRefl; INTERNAL_DATA
			float3 worldNormal;
		};
		
		float Fresnel(float3 v, float3 n) {
			float c = 1.0 - saturate(dot(v, n));
			return _F0 + (1.0 - _F0) * c * c * c * c * c;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			float4 c = tex2D(_MainTex, IN.uv_MainTex);
			float3 n = tex2D(_BumpMap, IN.uv_MainTex).xyz;
			
			o.Normal = n;
			IN.worldRefl = WorldReflectionVector(IN, o.Normal);
			IN.worldNormal = WorldNormalVector(IN, o.Normal);
			
			float3 r = normalize(IN.worldRefl);
			float f = Fresnel(r, IN.worldNormal);			
			float4 cRefl = _ICube * texCUBE(_Cube, r);
			
			o.Emission = f * cRefl.rgb;
			o.Alpha = f;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
