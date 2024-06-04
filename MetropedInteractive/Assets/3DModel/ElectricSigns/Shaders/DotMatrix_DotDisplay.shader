Shader "DotMatrix/DotMatrix DotDisplay" {

	Properties {
		_MainTex("Dot Texture", 2D) = "white" {}
		_BackgroundColor("Background Color", Color) = (0,0,0,1)
		_DotSize("Dot Size", Range(0.25, 1.5)) = 1.0
		_Roundness("Roundness", Range(0.0, 1.0)) = 1.0
		_Softness("Softness", Range(0.0, 1.0)) = 0.2
	}

	SubShader {

		Pass {

			CGPROGRAM

			#pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			fixed4 _BackgroundColor;
			float _DotSize;
			float _Roundness;
			float _Softness;

			fixed4 frag(v2f_img input) : SV_Target {

				float gsizeX = 1.0 / _MainTex_TexelSize.z;
				float gsizey = 1.0 / _MainTex_TexelSize.w;

				float centerX = floor(input.uv.x * _MainTex_TexelSize.z) * gsizeX + gsizeX / 2.0;
				float centerY = floor(input.uv.y * _MainTex_TexelSize.w) * gsizey + gsizey / 2.0;
				float distX = abs(input.uv.x - centerX);
				float distY = abs(input.uv.y - centerY) / (_MainTex_TexelSize.z / _MainTex_TexelSize.w);

				fixed power = 10.0 - _Roundness * 8.0;
				float dist = pow((pow(distX, power) + pow(distY, power)), 1.0 / power);
				float diam = gsizeX * _DotSize / 2.0;
				float multip = max(1.0 - dist / diam, 0.0);

				float soft = multip / (_Softness * 0.66 + 0.01);
				multip = min(soft,sign(multip));

				fixed4 color = tex2D(_MainTex, input.uv);
				color = lerp(_BackgroundColor,color,multip);

				return color;

			}

			ENDCG

		}

	}

}
