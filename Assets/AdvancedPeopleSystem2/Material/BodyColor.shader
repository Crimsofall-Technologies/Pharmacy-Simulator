Shader "BodyColor - APPack"
{
	Properties
	{
		_SkinColor("SkinColor", Color) = (0,0,0,0)
		_EyeColor("EyeColor", Color) = (0,0,0,0)
		_HairColor("HairColor", Color) = (0,0,0,0)
		_UnderpantsColor("UnderPantsColor", Color) = (0,0,0,0)
		_OralCavityColor("OralCavityColor", Color) = (0,0,0,0)
		_TeethColor("TeethColor", Color) = (0,0,0,0)
		_Glossiness("Smoothness", Range(0,1)) = 0.0
		_Saturation("Saturation", Range(0,2)) = 1.0
		[Toggle]_UseGradient("UseGradient", Float) = 1
		[HideInInspector] _texcoord("", 2D) = "white" {}
		[HideInInspector] __dirty("", Int) = 1
	}

		SubShader
		{
			Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
			Cull Back

			CGPROGRAM
			#pragma target 3.0 multi_compile_instancing
			#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
			struct Input
			{
				float2 uv_texcoord;
			};

			uniform float4 _SkinColor;
			uniform float4 _EyeColor;
			uniform float4 _HairColor;
			uniform float4 _UnderpantsColor;
			uniform float4 _OralCavityColor;
			uniform float4 _TeethColor;
			uniform half _Glossiness;
			uniform half _Saturation;
			uniform float _UseGradient;

			float4 GetColor(float4 SkinColor , float4 EyeColor , float4 HairColor , float4 UnderPantsColor, float4 OralCavityColor, float4 TeethColor, float XOffset, float YOffset)
			{
				if (XOffset > 0.75) {
					if (YOffset < 0.05)
						return OralCavityColor;
					else if (YOffset < 0.1)
						return TeethColor;
					else
						return (_UseGradient) ? (SkinColor * YOffset) : SkinColor;;
				}
				else if (XOffset < 0.75 && XOffset > 0.5)
				return (_UseGradient) ? (EyeColor * YOffset) : EyeColor;
				else if (XOffset < 0.5 && XOffset > 0.25)
				return (_UseGradient) ? (HairColor * YOffset) : HairColor;
				else
				return (_UseGradient) ? (UnderPantsColor * YOffset) : UnderPantsColor;;
			}

			void Unity_Saturation_float(float3 In, float Saturation, out float3 Out)
			{
				float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
				Out = luma.xxx + Saturation.xxx * (In - luma.xxx);
			}

			#pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input i , inout SurfaceOutputStandard o)
			{
				float XOffset = (i.uv_texcoord).x;
				float YOffset = (i.uv_texcoord).y;

				float3 localGetColor = GetColor(_SkinColor, _EyeColor, _HairColor, _UnderpantsColor, _OralCavityColor, _TeethColor, XOffset , YOffset);

				//localGetColor = (_UseGradient) ? localGetColor * YOffset : localGetColor;

				Unity_Saturation_float(localGetColor, _Saturation, localGetColor);

				o.Albedo = localGetColor.xyz;
				o.Smoothness = _Glossiness;
				o.Alpha = 1;
			}

			ENDCG
		}
		Fallback "Diffuse"
}