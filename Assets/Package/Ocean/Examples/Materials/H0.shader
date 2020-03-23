// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/H0" {
	Properties {
		_Height ("Height Power", Float) = 1
		_H0Tex ("Height Map", 2D) = "black" {}
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
			sampler2D _H0Tex;

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
				float4 h = _Height * tex2D (_H0Tex, IN.uv);
				return 0.5 * (h + 1);
			}
			ENDCG
		}
	} 
}
