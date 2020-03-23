// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Height" {
	Properties {
		_Height ("Height Power", Float) = 1
		_HeightMap ("Height Map", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			float _Height;
			sampler2D _HeightMap;

			struct Input {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			Input vert(Input IN) {
				Input OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				return OUT;
			}
			float4 frag(Input IN) : COLOR {
				float h = _Height * tex2D (_HeightMap, IN.uv).r;
				return 0.5 * (h + 1);
			}
			ENDCG
		}
	} 
}
