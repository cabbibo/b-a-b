﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Battlehub/Wireframe"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" { }
		_Color1("Color1", Color) = (0.2, 0.2, 0.2, 1)
		_Color2("Color2", Color) = (0.2, 0.2, 0.2, 1)
		_ThicknessFront("Thickness Font", Float) = 1.0
		_ThicknessBack("Thickness Back", Float) = 1.0
		[HideInInspector]_ZTest("__zt", Float) = 0.0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Cull Back
			//ZTest[_ZTest]
			//ZWrite Off
		//	Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 baryc : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color1;
			fixed4 _Color2;
			fixed4 _SelectedColor;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _ThicknessBack;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.baryc = v.color;
				o.color.a = v.color.a;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float edgeFactor(float3 barycentric) {
				float3 d = fwidth(barycentric);
				float3 a3 = smoothstep(float3(0, 0, 0), d * _ThicknessBack, barycentric);
				return min(min(a3.x, a3.y), a3.z);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 color = i.color;
				color = lerp( _Color1 , _Color2 ,1.0 - edgeFactor(i.baryc));
								return color ;
			}
			ENDCG
		}
	}
}
