Shader "Custom/OceanNormal" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM
			#pragma target 5.0
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct vs2ps {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			vs2ps vert(appdata IN) {
				vs2ps OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.uv = IN.uv;				
				return OUT;
			}
			
			fixed4 frag(vs2ps IN) : COLOR {
				fixed4 c = tex2D(_MainTex, IN.uv);
				return 0.5 * (c + 1);
			}
			ENDCG
		}
	} 
}
