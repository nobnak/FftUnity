Shader "Custom/Color" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _Color;

			struct appdata {
				float4 vertex : POSITION;
			};
			
			float4 vert(appdata IN) : POSITION {
				return mul(UNITY_MATRIX_MVP, IN.vertex);
			}
			fixed4 frag(float4 vertex : POSITION) : COLOR {
				return _Color;
			}
			ENDCG
		}
	} 
}
