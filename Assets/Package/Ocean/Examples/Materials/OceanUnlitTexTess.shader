Shader "Custom/OceanUnlitTexTess" {
	Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _HeightMap ("Height Map", 2D) = "black" {}
        _Height ("Height", Float) = 1
        _Incline ("Incline", Vector) = (0,0,0,0)
        _BumpMap ("Normal Map", 2D) = "white" {}

		_Tess ("Tessellation Factor", Range(1,32)) = 4
        _Phong ("Phong Tess", Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="AlphaTest" "Queue"="AlphaTest" }
		LOD 200 ZWrite On ZTest LEqual
		Blend One OneMinusSrcAlpha
		ColorMask RGB
		
		CGPROGRAM
		#pragma target 5.0
		#pragma surface surf Lambert noshadow vertex:vert tessellate:tess tessphong:_Phong
      
        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        sampler2D _HeightMap;
        float4 _HeightMap_ST;
        float _Height;
        float4 _Incline;
        sampler2D _BumpMap;
        float4 _BumpMap_ST;

		float _Tess;
        float _Phong;

		struct appdata {
			float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 tangent : TANGENT;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            float4 texcoord2 : TEXCOORD2;
		};

		struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
		};

		float4 tess() {
			return _Tess;
		}
		
		void vert(inout appdata v) {
            float2 uvBottom = v.texcoord.xy;
            if (_MainTex_TexelSize.y < 0)
                uvBottom.y = 1 - uvBottom.y;

			float h = tex2Dlod(_HeightMap, float4(TRANSFORM_TEX(uvBottom, _HeightMap), 0, 0));
			v.vertex.xyz += _Height * h * v.normal;
            v.vertex.y += dot(uvBottom, _Incline.xy);

            float3 binormal = cross(v.normal.xyz, v.tangent.xyz);
            float3 n = tex2Dlod(_BumpMap, float4(TRANSFORM_TEX(uvBottom, _BumpMap), 0, 0)).xyz;
            v.normal = normalize(v.tangent.xyz * n.x + v.normal * n.z + binormal * n.y);
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
            float4 cmain = tex2D(_MainTex, IN.uv_MainTex);
            o.Emission = cmain.rgb;
            return;
		}
		ENDCG
	} 
	FallBack Off
}
