Shader "Custom/OceanUnlitTex" {
	Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
		_HeightMap ("Height Map", 2D) = "black" {}
		_Height ("Height", Float) = 1
        _Incline ("Incline", Vector) = (0,0,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		ZWrite On ZTest LEqual
		ColorMask RGB

        Pass {
    		CGPROGRAM
    		#pragma target 5.0
    		#pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
    		sampler2D _HeightMap;
            float4 _HeightMap_ST;
    		float _Height;
            float4 _Incline;

    		struct appdata {
    			float4 vertex : POSITION;
    			float4 tangent : TANGENT;
    			float3 normal : NORMAL;
    			float4 texcoord : TEXCOORD0;
    		};

    		struct v2f {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uvBottom : TEXCOORD1;
    		};
    		
    		v2f vert(appdata v) {
                float2 uvBottom = v.texcoord.xy;
                if (_MainTex_TexelSize.y < 0)
                    uvBottom.y = 1 - uvBottom.y;

    			float h = tex2Dlod(_HeightMap, float4(TRANSFORM_TEX(uvBottom, _HeightMap), 0, 0));
    			v.vertex.xyz += _Height * h * v.normal;
                v.vertex.y += dot(uvBottom, _Incline.xy);

                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 1));
                o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                o.uvBottom = uvBottom;
                return o;
    		}
    		
    		float4 frag(v2f IN) : COLOR {
                float4 cmain = tex2D(_MainTex, IN.uv);
                return cmain;
    		}
    		ENDCG
        }
	} 
	FallBack Off
}
