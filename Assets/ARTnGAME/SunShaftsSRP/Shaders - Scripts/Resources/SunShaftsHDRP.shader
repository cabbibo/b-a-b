Shader "Hidden/Custom/SunShaftsHDRP"
{
	HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl" //unity 2018.3
//#include "PostProcessing/Shaders/StdLib.hlsl" //unity 2018.1-2
//#include "UnityCG.cginc"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_ColorBuffer, sampler_ColorBuffer);
	TEXTURE2D_SAMPLER2D(_Skybox, sampler_Skybox);
	float _Blend;

	//sampler2D _MainTex;
	//sampler2D _ColorBuffer;
	//sampler2D _Skybox;
	//sampler2D_float _CameraDepthTexture;
	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
	half4 _CameraDepthTexture_ST;

	half4 _SunThreshold;

	half4 _SunColor;
	uniform half4 _BlurRadius4;
	uniform half4 _SunPosition;
	uniform half4 _MainTex_TexelSize;

#define SAMPLES_FLOAT 6.0f
#define SAMPLES_INT 6

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
#if UNITY_UV_STARTS_AT_TOP
		float2 uv1 : TEXCOORD1;
#endif		
	};

	struct v2f_radial {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 blurVector : TEXCOORD1;
	};

	

	float4 FragGrey(VaryingsDefault i) : SV_Target
	{
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		//float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.0721750));
		//color.rgb = lerp(color.rgb, luminance.xxx, _Blend.xxx);
		return color;
	}

	half4 fragScreen(v2f i) : SV_Target{
		//half4 colorA = tex2D(_MainTex, i.uv.xy);
		half4 colorA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
#if UNITY_UV_STARTS_AT_TOP
		///half4 colorB = tex2D(_ColorBuffer, i.uv1.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv1.xy);
#else
		//half4 colorB = tex2D(_ColorBuffer, i.uv.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);//v1.1
#endif
		half4 depthMask = saturate(colorB * _SunColor);
		return 1.0f - (1.0f - colorA) * (1.0f - depthMask);
	}


		half4 fragAdd(v2f i) : SV_Target{
		//half4 colorA = tex2D(_MainTex, i.uv.xy);
		half4 colorA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
#if UNITY_UV_STARTS_AT_TOP
		//half4 colorB = tex2D(_ColorBuffer, i.uv1.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv1.xy);
#else
		//half4 colorB = tex2D(_ColorBuffer, i.uv.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
#endif
		half4 depthMask = saturate(colorB * _SunColor);
		return colorA + depthMask;
	}


		v2f vert(AttributesDefault v) { //appdata_img v) {
			v2f o;
	//o.pos = UnityObjectToClipPos(v.vertex);
	o.pos = float4(v.vertex.xy, 0.0, 1.0);
	float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
	uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif

	o.uv = uv;// v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
	o.uv1 = uv.xy;
	if (_MainTex_TexelSize.y < 0)
		o.uv1.y = 1 - o.uv1.y;
#endif				

	return o;
}

	v2f_radial vert_radial(AttributesDefault v){ //appdata_img v) {
		v2f_radial o;
////		o.pos = UnityObjectToClipPos(v.vertex);

		//o.pos = float4(v.vertex.xyz,1);
		o.pos = float4(v.vertex.xy, 0.0, 1.0);

		float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

		#if UNITY_UV_STARTS_AT_TOP
				uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
		#endif

				o.uv.xy = uv;//v.texcoord.xy;
		//o.blurVector = (_SunPosition.xy - v.texcoord.xy) * _BlurRadius4.xy;
		o.blurVector = (_SunPosition.xy - uv.xy) * _BlurRadius4.xy;

		return o;
	}

	half4 frag_radial(v2f_radial i) : SV_Target
	{
		half4 color = half4(0,0,0,0);
		for (int j = 0; j < SAMPLES_INT; j++)
		{
			//half4 tmpColor = tex2D(_MainTex, i.uv.xy);
			half4 tmpColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
			color += tmpColor;
			i.uv.xy += i.blurVector;
		}
		return color / SAMPLES_FLOAT;
	}

		half TransformColor(half4 skyboxValue) {
		return dot(max(skyboxValue.rgb - _SunThreshold.rgb, half3(0, 0, 0)), half3(1, 1, 1)); // threshold and convert to greyscale
	}

	half4 frag_depth(v2f i) : SV_Target{
#if UNITY_UV_STARTS_AT_TOP
		//float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv1.xy);
		float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv1.xy));
#else
		//float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
		float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy));
#endif

	//half4 tex = tex2D(_MainTex, i.uv.xy);
	half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
	depthSample = Linear01Depth(depthSample);

	// consider maximum radius
#if UNITY_UV_STARTS_AT_TOP
	half2 vec = _SunPosition.xy - i.uv1.xy;
#else
	half2 vec = _SunPosition.xy - i.uv.xy;
#endif
	half dist = saturate(_SunPosition.w - length(vec.xy));

	half4 outColor = 0;

	// consider shafts blockers
	//if (depthSample > 0.99)
	//if (depthSample > 0.103)
	if (depthSample < 0.018) {
		outColor = TransformColor(tex) * dist;
	}

	return outColor;
	}
		
	inline half Luminance(half3 rgb)
	{
		return dot(rgb, unity_ColorSpaceLuminance.rgb);
	}

	half4 frag_nodepth(v2f i) : SV_Target{
#if UNITY_UV_STARTS_AT_TOP
		//float4 sky = (tex2D(_Skybox, i.uv1.xy));
		float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.uv1.xy);
#else
		//float4 sky = (tex2D(_Skybox, i.uv.xy));
		float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.uv.xy);
#endif

		//float4 tex = (tex2D(_MainTex, i.uv.xy));
		half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
		//sky = float4(0.3, 0.05, 0.05,  1);
		/// consider maximum radius
#if UNITY_UV_STARTS_AT_TOP
		half2 vec = _SunPosition.xy - i.uv1.xy;
#else
		half2 vec = _SunPosition.xy - i.uv.xy;
#endif
		half dist = saturate(_SunPosition.w - length(vec));

		half4 outColor = 0;

		// find unoccluded sky pixels
		// consider pixel values that differ significantly between framebuffer and sky-only buffer as occluded


		if (Luminance(abs(sky.rgb - tex.rgb)) < 0.2) {
			outColor = TransformColor(tex) * dist;
			//outColor = TransformColor(sky) * dist;
		}

		return outColor;
	}

		ENDHLSL

//		SubShader
//	{
//		//Cull Off ZWrite Off ZTest Always
//
//			Pass
//		{
//			HLSLPROGRAM
//
//#pragma vertex VertDefault
//#pragma fragment Frag
//
//			ENDHLSL
//		}
//	}
Subshader {

		Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment fragScreen

			ENDHLSL
		}

			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert_radial
#pragma fragment frag_radial

			ENDHLSL
		}

			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment frag_depth

			ENDHLSL
		}

			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment frag_nodepth

			ENDHLSL
		}

			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment fragAdd

			ENDHLSL
		}


			//PASS5
			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment FragGrey

			ENDHLSL
		}


	}
}