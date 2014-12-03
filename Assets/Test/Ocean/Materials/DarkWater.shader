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
		
		CGPROGRAM
		#pragma target 5.0
		#pragma surface surf Water vertex:vert
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

		struct Input {
			float2 uv_BumpMap;
			float3 viewDirForLight;

			fixed4 _TtoW0;
			fixed4 _TtoW1;
			fixed4 _TtoW2;
		};
		
		float Fresnel(float3 v, float3 n) {
			float c = 1.0 - saturate(dot(v, n));
			return _F0 + (1.0 - _F0) * c * c * c * c * c;
		}
		
		inline fixed4 LightingWater(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten) {
			//viewDir = s.Albedo;
			half3 h = normalize(lightDir + viewDir);
			float nh = max (0, dot (s.Normal, h));
			float spec = Fresnel(viewDir, h) * pow(nh, _LightRough) * _LightPower;
			
			fixed4 c;
			c.rgb = (_LightColor0.rgb  * spec) * (atten * 2);
			c.a = s.Alpha + _LightColor0.a * spec * atten;
			return c;
		}
		
		inline float3 Custom_ObjSpaceViewDir( in float4 v ) {
			float3 objSpaceCameraPos = mul(_World2Object, float4(_WorldViewPos.xyz, 1)).xyz * unity_Scale.w;
			return objSpaceCameraPos - v.xyz;
		}
		
		void vert(inout appdata_full v, out Input OUT) {
			TANGENT_SPACE_ROTATION;
			UNITY_INITIALIZE_OUTPUT(Input, OUT);
			
			float3 viewDir = ObjSpaceViewDir(v.vertex);
			OUT.viewDirForLight = mul (rotation, viewDir);
			float3 worldViewDir = mul ((float3x3)_Object2World, -viewDir);
			OUT._TtoW0 = float4(mul(rotation, _Object2World[0].xyz), worldViewDir.x)*unity_Scale.w;
			OUT._TtoW1 = float4(mul(rotation, _Object2World[1].xyz), worldViewDir.y)*unity_Scale.w;
			OUT._TtoW2 = float4(mul(rotation, _Object2World[2].xyz), worldViewDir.z)*unity_Scale.w;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			float h = tex2D(_BumpMap, IN.uv_BumpMap).w;
			float2 offset = ParallaxOffset(h, _Parallax, IN.viewDirForLight);
			IN.uv_BumpMap += offset;
			o.Normal = tex2D(_BumpMap, IN.uv_BumpMap).xyz;
			
			float3 worldViewDir = float3(IN._TtoW0.w, IN._TtoW1.w, IN._TtoW2.w);
			float3 worldNormal = Custom_WorldNormalVector(IN, o.Normal);
			float3 worldRefl = reflect(worldViewDir, worldNormal);
			
			float3 r = normalize(worldRefl);
			float f = Fresnel(r, worldNormal);			
			//float4 cRefl = _SkyPower * texCUBE(_SkyCube, r);			
			//o.Emission =  f * cRefl + (1.0 - f) * _SeaColor.rgb;
			o.Emission = f * _SkyPower;
		}
		ENDCG
	} 
}
