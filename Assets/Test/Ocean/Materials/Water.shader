Shader "Custom/Water" {
	Properties {
		_BumpMap ("Normal Map", 2D) = "white" {}
		_LightRough ("Light Roubhness", Float) = 1
		_LightPower ("Light Intensity", Float) = 1
		_F0 ("Fresnel 0", Float) = 0.02
		_SkyCube ("Sky", Cube) = "black" {}
		_SkyPower ("Sky Intensity", Float) = 1.0
		_SeaColor ("Sea Color", Color) = (0, 0, 0, 1)
		_Parallax ("Height", Float) = 0.02
		_LocalView ("Local View Position", Vector) = (0, 0, 0, 0)
		_WorldView ("World View Position", Vector) = (0, 0, 0, 0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		//#pragma debug
		#pragma target 5.0
		#pragma surface surf Water vertex:vert
		
		sampler2D _BumpMap;
		float _LightRough;
		float _LightPower;
		float _F0;
		samplerCUBE _SkyCube;
		float _SkyPower;
		float4 _SeaColor;
		float _Parallax;
		float4 _LocalView;
		float4 _WorldView;

		struct Input {
			float2 uv_BumpMap;
			float3 worldRefl;
			float3 worldNormal;
			float3 viewDir;
			float3 worldPos;
			INTERNAL_DATA
		};
		
		float Fresnel(float3 v, float3 n) {
			float c = 1.0 - saturate(dot(v, n));
			return _F0 + (1.0 - _F0) * c * c * c * c * c;
		}
		
		inline fixed4 LightingWater(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten) {
			half3 h = normalize(lightDir + viewDir);
			float nh = max (0, dot (s.Normal, h));
			float spec = Fresnel(viewDir, h) * pow(nh, _LightRough) * _LightPower;
			
			fixed4 c;
			c.rgb = (_LightColor0.rgb  * spec) * (atten * 2);
			c.a = s.Alpha + _LightColor0.a * spec * atten;
			return c;
		}
		
		void vert(inout appdata_full IN, out Input OUT) {
			UNITY_INITIALIZE_OUTPUT(Input, OUT);
			OUT.worldPos = mul(_Object2World, IN.vertex);
		}

		void surf (Input IN, inout SurfaceOutput o) {
			float h = tex2D(_BumpMap, IN.uv_BumpMap).w;
			//IN.viewDir = normalize(IN.worldPos - _WorldView.xyz);
			float2 offset = ParallaxOffset(h, _Parallax, IN.viewDir);
			IN.uv_BumpMap += offset;
			o.Normal = tex2D(_BumpMap, IN.uv_BumpMap).xyz;
			
			//IN.worldRefl = WorldReflectionVector(IN, o.Normal);
			IN.worldNormal = WorldNormalVector(IN, o.Normal);
			IN.worldRefl = reflect(IN.viewDir, IN.worldNormal);
			
			float3 r = normalize(IN.worldRefl);
			float f = Fresnel(r, IN.worldNormal);			
			float4 cRefl = _SkyPower * texCUBE(_SkyCube, r);
			
			//o.Emission =  f * cRefl + (1.0 - f) * _SeaColor.rgb;
			o.Emission = f * _SkyPower;
		}
		ENDCG
	} 
}
