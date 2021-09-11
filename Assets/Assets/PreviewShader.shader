Shader "Custom/Preview" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	_Rotation("Rotation", Float) = 0.0
	}
		SubShader{
			Tags { 
				"RenderType" = "Opaque"
				"Preview" = "True"
		}
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows vertex:vert

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
		float _Rotation;

		void vert(inout appdata_full v) {
		  v.texcoord.xy -= 0.5;
		  float s = sin(_Rotation);
		  float c = cos(_Rotation);
		  float2x2 rotationMatrix = float2x2(c, -s, s, c);
		  rotationMatrix *= 0.5;
		  rotationMatrix += 0.5;
		  rotationMatrix = rotationMatrix * 2 - 1;
		  v.texcoord.xy = mul(v.texcoord.xy, rotationMatrix);
		  v.texcoord.xy += 0.5;
		}

			void surf(Input IN, inout SurfaceOutputStandard o) {
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}
