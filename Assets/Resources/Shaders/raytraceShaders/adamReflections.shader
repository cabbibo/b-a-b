Shader "Adam/PlaneReflections Standard (Specular setup)"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		_GlossMapScale("Smoothness Factor", Range(0.0, 1.0)) = 1.0
		[Enum(Specular Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

		_SpecColor("Specular", Color) = (0.2,0.2,0.2)
		_SpecGlossMap("Specular", 2D) = "white" {}
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}

		_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
		_ParallaxMap ("Height Map", 2D) = "black" {}

		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}

		_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
		
		_DetailMask("Detail Mask", 2D) = "white" {}

		_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapScale("Scale", Float) = 1.0
		_DetailNormalMap("Normal Map", 2D) = "bump" {}

		[Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


		// Blending state
		[HideInInspector] _Mode ("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0

//adam-begin:
		_PlaneReflectionIntensityScale("Plane Reflection Intensity Scale", Range(0.0, 4.0)) = 1.0
		_PlaneReflectionBumpScale("Plane Reflection Bump Scale", Range(0.0, 1.0)) = 0.4
		_PlaneReflectionBumpClamp("Plane Reflection Bump Clamp", Range(0.0, 0.15)) = 0.05
//adam-end:
	}

	CGINCLUDE
		#define UNITY_SETUP_BRDF_INPUT SpecularSetup
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks"="False" "Special"="PlaneReflection" }
		LOD 300
	

		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD" 
			Tags { "LightMode" = "ForwardBase" }

			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM
//adam-begin: 5.0 / d3d11 only (includes d3d12)
			#pragma target 5.0
			#pragma only_renderers d3d11
//adam-end:

			// -------------------------------------

			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _EMISSION
			#pragma shader_feature _SPECGLOSSMAP
			#pragma shader_feature ___ _DETAIL_MULX2
			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
			#pragma shader_feature _PARALLAXMAP
			
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog

//adam-begin:
			#pragma multi_compile _ PLANE_REFLECTION
			#pragma multi_compile _ PLANE_REFLECTION_USER_CLIPPLANE
			uniform float4 _PlaneReflectionClipPlane;
//adam-end:

//adam-begin: Skip the CoreForward proxy for vertex, use custom fragment
			#pragma vertex vertForwardBase
			#pragma fragment fragForwardBasePR
			#ifndef UNITY_STANDARD_CORE_INCLUDED
#define UNITY_STANDARD_CORE_INCLUDED

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityInstancing.cginc"
#include "UnityStandardConfig.cginc"
// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

#ifndef UNITY_STANDARD_INPUT_INCLUDED
#define UNITY_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityInstancing.cginc"
#include "UnityStandardConfig.cginc"
//adam-begin:
//moved later: #include "UnityPBSLighting.cginc" // TBD: remove
//adam-end:
#include "UnityStandardUtils.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || DIRLIGHTMAP_SEPARATE || !DIRLIGHTMAP_OFF || _PARALLAXMAP)
	#define _TANGENT_TO_WORLD 1 
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
	#define _DETAIL 1
#endif

//---------------------------------------
half4		_Color;
half		_Cutoff;

sampler2D	_MainTex;
float4		_MainTex_ST;

sampler2D	_DetailAlbedoMap;
float4		_DetailAlbedoMap_ST;

sampler2D	_BumpMap;
half		_BumpScale;

sampler2D	_DetailMask;
sampler2D	_DetailNormalMap;
half		_DetailNormalMapScale;

sampler2D	_SpecGlossMap;
sampler2D	_MetallicGlossMap;
half		_Metallic;
half		_Glossiness;
half		_GlossMapScale;

sampler2D	_OcclusionMap;
half		_OcclusionStrength;
//adam-begin:
float		_VertexOcclusionPower;
float		_LightProbeScale;
float		_ReflectionProbeBoost;
//adam-end:

sampler2D	_ParallaxMap;
half		_Parallax;
half		_UVSec;

half4 		_EmissionColor;
sampler2D	_EmissionMap;

//adam-begin:
	#include "UnityPBSLighting.cginc" // TBD: remove
//adam-end:

//-------------------------------------------------------------------------------------
// Input functions

struct VertexInput
{
	float4 vertex	: POSITION;
	half3 normal	: NORMAL;
	float2 uv0		: TEXCOORD0;
	float2 uv1		: TEXCOORD1;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	float2 uv2		: TEXCOORD2;
#endif
#ifdef _TANGENT_TO_WORLD
	half4 tangent	: TANGENT;
#endif
//adam-begin: Vertex color (e.g. for vertex AO)
	half4 color		: COLOR;
//adam-end:
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 TexCoords(VertexInput v)
{
	float4 texcoord;
	texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
	texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
	return texcoord;
}		

half DetailMask(float2 uv)
{
	return tex2D (_DetailMask, uv).a;
}

half3 Albedo(float4 texcoords)
{
	half3 albedo = _Color.rgb * tex2D (_MainTex, texcoords.xy).rgb;
#if _DETAIL
	#if (SHADER_TARGET < 30)
		// SM20: instruction count limitation
		// SM20: no detail mask
		half mask = 1; 
	#else
		half mask = DetailMask(texcoords.xy);
	#endif
	half3 detailAlbedo = tex2D (_DetailAlbedoMap, texcoords.zw).rgb;
	#if _DETAIL_MULX2
		albedo *= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);
	#elif _DETAIL_MUL
		albedo *= LerpWhiteTo (detailAlbedo, mask);
	#elif _DETAIL_ADD
		albedo += detailAlbedo * mask;
	#elif _DETAIL_LERP
		albedo = lerp (albedo, detailAlbedo, mask);
	#endif
#endif
	return albedo;
}

half Alpha(float2 uv)
{
#if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
	return _Color.a;
#else
	return tex2D(_MainTex, uv).a * _Color.a;
#endif
}		

//adam-begin: use min of texture and vertex occlusion
half Occlusion(float2 uv, half vertexOcclusion)
{
#if (SHADER_TARGET < 30)
	// SM20: instruction count limitation
	// SM20: simpler occlusion
	return tex2D(_OcclusionMap, uv).g;
#else
	half occ = min(pow(vertexOcclusion, _VertexOcclusionPower), tex2D(_OcclusionMap, uv).g);
	return LerpOneTo (occ, _OcclusionStrength);
#endif
}
//adam-end:

half4 SpecularGloss(float2 uv)
{
	half4 sg;
#ifdef _SPECGLOSSMAP
	#if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
		sg.rgb = tex2D(_SpecGlossMap, uv).rgb;
		sg.a = tex2D(_MainTex, uv).a;
	#else
		sg = tex2D(_SpecGlossMap, uv);
	#endif
	sg.a *= _GlossMapScale;
#else
	sg.rgb = _SpecColor.rgb;
	#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		sg.a = tex2D(_MainTex, uv).a * _GlossMapScale;
	#else
		sg.a = _Glossiness;
	#endif
#endif
	return sg;
}

half2 MetallicGloss(float2 uv)
{
	half2 mg;
	
#ifdef _METALLICGLOSSMAP
	#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		mg.r = tex2D(_MetallicGlossMap, uv).r;
		mg.g = tex2D(_MainTex, uv).a;
	#else
		mg = tex2D(_MetallicGlossMap, uv).ra;
	#endif
	mg.g *= _GlossMapScale;
#else
	mg.r = _Metallic;
	#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		mg.g = tex2D(_MainTex, uv).a * _GlossMapScale;
	#else
		mg.g = _Glossiness;
	#endif
#endif
	return mg;
}

half3 Emission(float2 uv)
{
#ifndef _EMISSION
	return 0;
#else
	return tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
#endif
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords)
{
	half3 normalTangent = UnpackScaleNormal(tex2D (_BumpMap, texcoords.xy), _BumpScale);
	// SM20: instruction count limitation
	// SM20: no detail normalmaps
#if _DETAIL && !defined(SHADER_API_MOBILE) && (SHADER_TARGET >= 30) 
	half mask = DetailMask(texcoords.xy);
	half3 detailNormalTangent = UnpackScaleNormal(tex2D (_DetailNormalMap, texcoords.zw), _DetailNormalMapScale);
	#if _DETAIL_LERP
		normalTangent = lerp(
			normalTangent,
			detailNormalTangent,
			mask);
	#else				
		normalTangent = lerp(
			normalTangent,
			BlendNormals(normalTangent, detailNormalTangent),
			mask);
	#endif
#endif
	return normalTangent;
}
#endif

float4 Parallax (float4 texcoords, half3 viewDir)
{
// D3D9/SM30 supports up to 16 samplers, skip the parallax map in case we exceed the limit
#define EXCEEDS_D3D9_SM3_MAX_SAMPLER_COUNT	(defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_SEPARATE) && defined(SHADOWS_SCREEN) && defined(_NORMALMAP) && \
											 defined(_EMISSION) && defined(_DETAIL) && (defined(_METALLICGLOSSMAP) || defined(_SPECGLOSSMAP)))

#if !defined(_PARALLAXMAP) || (SHADER_TARGET < 30) || (defined(SHADER_API_D3D9) && EXCEEDS_D3D9_SM3_MAX_SAMPLER_COUNT)
	// SM20: instruction count limitation
	// SM20: no parallax
	return texcoords;
#else
	half h = tex2D (_ParallaxMap, texcoords.xy).g;
	float2 offset = ParallaxOffset1Step (h, _Parallax, viewDir);
	return float4(texcoords.xy + offset, texcoords.zw + offset);
#endif

#undef EXCEEDS_D3D9_SM3_MAX_SAMPLER_COUNT
}
			
#endif // UNITY_STANDARD_INPUT_INCLUDED
#include "UnityPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityStandardBRDF.cginc"

#include "AutoLight.cginc"


//-------------------------------------------------------------------------------------
// counterpart for NormalizePerPixelNormal
// skips normalization per-vertex and expects normalization to happen per-pixel
half3 NormalizePerVertexNormal (float3 n) // takes float to avoid overflow
{
    #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
        return normalize(n);
    #else
        return n; // will normalize per-pixel instead
    #endif
}

half3 NormalizePerPixelNormal (half3 n)
{
	#if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
		return n;
	#else
		return normalize(n);
	#endif
}

//-------------------------------------------------------------------------------------
UnityLight MainLight (half3 normalWorld)
{
	UnityLight l;
	#ifdef LIGHTMAP_OFF
		
		l.color = _LightColor0.rgb;
		l.dir = _WorldSpaceLightPos0.xyz;
		l.ndotl = LambertTerm (normalWorld, l.dir);
	#else
		// no light specified by the engine
		// analytical light might be extracted from Lightmap data later on in the shader depending on the Lightmap type
		l.color = half3(0.f, 0.f, 0.f);
		l.ndotl  = 0.f;
		l.dir = half3(0.f, 0.f, 0.f);
	#endif

	return l;
}

UnityLight AdditiveLight (half3 normalWorld, half3 lightDir, half atten)
{
	UnityLight l;

	l.color = _LightColor0.rgb;
	l.dir = lightDir;
	#ifndef USING_DIRECTIONAL_LIGHT
		l.dir = NormalizePerPixelNormal(l.dir);
	#endif
	l.ndotl = LambertTerm (normalWorld, l.dir);

	// shadow the light
	l.color *= atten;
	return l;
}

UnityLight DummyLight (half3 normalWorld)
{
	UnityLight l;
	l.color = 0;
	l.dir = half3 (0,1,0);
	l.ndotl = LambertTerm (normalWorld, l.dir);
	return l;
}

UnityIndirect ZeroIndirect ()
{
	UnityIndirect ind;
	ind.diffuse = 0;
	ind.specular = 0;
	return ind;
}

//-------------------------------------------------------------------------------------
// Common fragment setup

// deprecated
half3 WorldNormal(half4 tan2world[3])
{
	return normalize(tan2world[2].xyz);
}

// deprecated
#ifdef _TANGENT_TO_WORLD
	half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
	{
		half3 t = tan2world[0].xyz;
		half3 b = tan2world[1].xyz;
		half3 n = tan2world[2].xyz;

	#if UNITY_TANGENT_ORTHONORMALIZE
		n = NormalizePerPixelNormal(n);

		// ortho-normalize Tangent
		t = normalize (t - n * dot(t, n));

		// recalculate Binormal
		half3 newB = cross(n, t);
		b = newB * sign (dot (newB, b));
	#endif

		return half3x3(t, b, n);
	}
#else
	half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
	{
		return half3x3(0,0,0,0,0,0,0,0,0);
	}
#endif

half3 PerPixelWorldNormal(float4 i_tex, half4 tangentToWorld[3])
{
#ifdef _NORMALMAP
	half3 tangent = tangentToWorld[0].xyz;
	half3 binormal = tangentToWorld[1].xyz;
	half3 normal = tangentToWorld[2].xyz;

	#if UNITY_TANGENT_ORTHONORMALIZE
		normal = NormalizePerPixelNormal(normal);

		// ortho-normalize Tangent
		tangent = normalize (tangent - normal * dot(tangent, normal));

		// recalculate Binormal
		half3 newB = cross(normal, tangent);
		binormal = newB * sign (dot (newB, binormal));
	#endif

	half3 normalTangent = NormalInTangentSpace(i_tex);
	half3 normalWorld = NormalizePerPixelNormal(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
#else
	half3 normalWorld = normalize(tangentToWorld[2].xyz);
#endif
	return normalWorld;
}

#ifdef _PARALLAXMAP
	#define IN_VIEWDIR4PARALLAX(i) NormalizePerPixelNormal(half3(i.tangentToWorldAndParallax[0].w,i.tangentToWorldAndParallax[1].w,i.tangentToWorldAndParallax[2].w))
	#define IN_VIEWDIR4PARALLAX_FWDADD(i) NormalizePerPixelNormal(i.viewDirForParallax.xyz)
#else
	#define IN_VIEWDIR4PARALLAX(i) half3(0,0,0)
	#define IN_VIEWDIR4PARALLAX_FWDADD(i) half3(0,0,0)
#endif

//adam-begin:
#if 1 //UNITY_SPECCUBE_BOX_PROJECTION || UNITY_LIGHT_PROBE_PROXY_VOLUME || defined(_SKINNED_MESH)
//adam-end:
	#define IN_WORLDPOS(i) i.posWorld
#else
	#define IN_WORLDPOS(i) half3(0,0,0)
#endif

#define IN_LIGHTDIR_FWDADD(i) half3(i.tangentToWorldAndLightDir[0].w, i.tangentToWorldAndLightDir[1].w, i.tangentToWorldAndLightDir[2].w)

#define FRAGMENT_SETUP(x) FragmentCommonData x = \
	FragmentSetup(i.tex, i.eyeVec, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndParallax, IN_WORLDPOS(i));

#define FRAGMENT_SETUP_FWDADD(x) FragmentCommonData x = \
	FragmentSetup(i.tex, i.eyeVec, IN_VIEWDIR4PARALLAX_FWDADD(i), i.tangentToWorldAndLightDir, half3(0,0,0));

struct FragmentCommonData
{
	half3 diffColor, specColor;
	// Note: oneMinusRoughness & oneMinusReflectivity for optimization purposes, mostly for DX9 SM2.0 level.
	// Most of the math is being done on these (1-x) values, and that saves a few precious ALU slots.
	half oneMinusReflectivity, oneMinusRoughness;
	half3 normalWorld, eyeVec, posWorld;
	half alpha;

#if UNITY_OPTIMIZE_TEXCUBELOD || UNITY_STANDARD_SIMPLE
	half3 reflUVW;
#endif

#if UNITY_STANDARD_SIMPLE
	half3 tangentSpaceNormal;
#endif
};

#ifndef UNITY_SETUP_BRDF_INPUT
	#define UNITY_SETUP_BRDF_INPUT SpecularSetup
#endif

inline FragmentCommonData SpecularSetup (float4 i_tex)
{
	half4 specGloss = SpecularGloss(i_tex.xy);
	half3 specColor = specGloss.rgb;
	half oneMinusRoughness = specGloss.a;

	half oneMinusReflectivity;
	half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular (Albedo(i_tex), specColor, /*out*/ oneMinusReflectivity);
	
	FragmentCommonData o = (FragmentCommonData)0;
	o.diffColor = diffColor;
	o.specColor = specColor;
	o.oneMinusReflectivity = oneMinusReflectivity;
	o.oneMinusRoughness = oneMinusRoughness;
	return o;
}

//inline FragmentCommonData MetallicSetup (float4 i_tex)
//{
//	half2 metallicGloss = MetallicGloss(i_tex.xy);
//	half metallic = metallicGloss.x;
//	half oneMinusRoughness = metallicGloss.y;		// this is 1 minus the square root of real roughness m.
//
//	half oneMinusReflectivity;
//	half3 specColor;
//	half3 diffColor = DiffuseAndSpecularFromMetallic (Albedo(i_tex), metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);
//
//	FragmentCommonData o = (FragmentCommonData)0;
//	o.diffColor = diffColor;
//	o.specColor = specColor;
//	o.oneMinusReflectivity = oneMinusReflectivity;
//	o.oneMinusRoughness = oneMinusRoughness;
//	return o;
//} 

inline FragmentCommonData FragmentSetup (float4 i_tex, half3 i_eyeVec, half3 i_viewDirForParallax, half4 tangentToWorld[3], half3 i_posWorld)
{
	i_tex = Parallax(i_tex, i_viewDirForParallax);

	half alpha = Alpha(i_tex.xy);
	#if defined(_ALPHATEST_ON)
		clip (alpha - _Cutoff);
	#endif

	FragmentCommonData o = UNITY_SETUP_BRDF_INPUT (i_tex);
	o.normalWorld = PerPixelWorldNormal(i_tex, tangentToWorld);
	o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
	o.posWorld = i_posWorld;

	// NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	o.diffColor = PreMultiplyAlpha (o.diffColor, alpha, o.oneMinusReflectivity, /*out*/ o.alpha);
	return o;
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, bool reflections)
{
	UnityGIInput d;
	d.light = light;
	d.worldPos = s.posWorld;
	d.worldViewDir = -s.eyeVec;
	d.atten = atten;
	#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
		d.ambient = 0;
		d.lightmapUV = i_ambientOrLightmapUV;
	#else
		d.ambient = i_ambientOrLightmapUV.rgb;
		d.lightmapUV = 0;
	#endif
	d.boxMax[0] = unity_SpecCube0_BoxMax;
	d.boxMin[0] = unity_SpecCube0_BoxMin;
	d.probePosition[0] = unity_SpecCube0_ProbePosition;
	d.probeHDR[0] = unity_SpecCube0_HDR;

	d.boxMax[1] = unity_SpecCube1_BoxMax;
	d.boxMin[1] = unity_SpecCube1_BoxMin;
	d.probePosition[1] = unity_SpecCube1_ProbePosition;
	d.probeHDR[1] = unity_SpecCube1_HDR;

	if(reflections)
	{
		Unity_GlossyEnvironmentData g;
		g.roughness		= 1 - s.oneMinusRoughness;
	#if UNITY_OPTIMIZE_TEXCUBELOD || UNITY_STANDARD_SIMPLE
		g.reflUVW 		= s.reflUVW;
	#else
		g.reflUVW		= reflect(s.eyeVec, s.normalWorld);
	#endif

		return UnityGlobalIllumination (d, occlusion, s.normalWorld, g);
	}
	else
	{
		return UnityGlobalIllumination (d, occlusion, s.normalWorld);
	}
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
{
	return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, true);
}


//-------------------------------------------------------------------------------------
half4 OutputForward (half4 output, half alphaFromSurface)
{
	#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
		output.a = alphaFromSurface;
	#else
		UNITY_OPAQUE_ALPHA(output.a);
	#endif
	return output;
}

inline half4 VertexGIForward(VertexInput v, float3 posWorld, half3 normalWorld)
{
	half4 ambientOrLightmapUV = 0;
	// Static lightmaps
	#ifndef LIGHTMAP_OFF
		ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		ambientOrLightmapUV.zw = 0;
	// Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
	#elif UNITY_SHOULD_SAMPLE_SH
		#ifdef VERTEXLIGHT_ON
			// Approximated illumination from non-important point lights
			ambientOrLightmapUV.rgb = Shade4PointLights (
				unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
				unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
				unity_4LightAtten0, posWorld, normalWorld);
		#endif

		ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, ambientOrLightmapUV.rgb);		
	#endif

	#ifdef DYNAMICLIGHTMAP_ON
		ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif

	return ambientOrLightmapUV;
}

// ------------------------------------------------------------------
//  Base forward pass (directional light, emission, lightmaps, ...)

struct VertexOutputForwardBase
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndParallax[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
	half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UV
	SHADOW_COORDS(6)
	UNITY_FOG_COORDS(7)

	// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
//adam-begin:
	#if 1 //UNITY_SPECCUBE_BOX_PROJECTION || UNITY_LIGHT_PROBE_PROXY_VOLUME
//adam-end:
		float3 posWorld					: TEXCOORD8;
	#endif

	#if UNITY_OPTIMIZE_TEXCUBELOD
		#if UNITY_SPECCUBE_BOX_PROJECTION
			half3 reflUVW				: TEXCOORD9;
		#else
			half3 reflUVW				: TEXCOORD8;
		#endif
	#endif

//adam-begin: shader additions
	half vertexOcclusion					: TEXCOORD10;
#if PLANE_REFLECTION_USER_CLIPPLANE
	float clipDistance					: SV_ClipDistance;
#endif
//adam-end:
};

VertexOutputForwardBase vertForwardBase (VertexInput v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutputForwardBase o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase, o);

	float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
//adam-begin:
	#if 1 //UNITY_SPECCUBE_BOX_PROJECTION || UNITY_LIGHT_PROBE_PROXY_VOLUME
		o.posWorld = posWorld.xyz;
	#endif	
//adam-end:
	o.pos = UnityObjectToClipPos(v.vertex);
	o.tex = TexCoords(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndParallax[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndParallax[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndParallax[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndParallax[0].xyz = 0;
		o.tangentToWorldAndParallax[1].xyz = 0;
		o.tangentToWorldAndParallax[2].xyz = normalWorld;
	#endif
	//We need this for shadow receving
	TRANSFER_SHADOW(o);

	o.ambientOrLightmapUV = VertexGIForward(v, posWorld, normalWorld);
	
	#ifdef _PARALLAXMAP
		TANGENT_SPACE_ROTATION;
		half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
		o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
		o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
		o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
	#endif

	#if UNITY_OPTIMIZE_TEXCUBELOD
		o.reflUVW 		= reflect(o.eyeVec, normalWorld);
	#endif

//adam-begin: shader additions
		o.vertexOcclusion = v.color.a;
#if PLANE_REFLECTION_USER_CLIPPLANE
		o.clipDistance = dot(posWorld, _PlaneReflectionClipPlane);
#endif
//adam-end:

	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

half4 fragForwardBaseInternal (VertexOutputForwardBase i)
{
	FRAGMENT_SETUP(s)
#if UNITY_OPTIMIZE_TEXCUBELOD
	s.reflUVW		= i.reflUVW;
#endif

	UnityLight mainLight = MainLight (s.normalWorld);
	half atten = SHADOW_ATTENUATION(i);

//adam-begin: pass vertex occlusion to occlusion input function
	half occlusion = Occlusion(i.tex.xy, i.vertexOcclusion);
//adam-end:

	UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, mainLight);

	half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
	c.rgb += UNITY_BRDF_GI (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, occlusion, gi);
	c.rgb += Emission(i.tex.xy);

	UNITY_APPLY_FOG(i.fogCoord, c.rgb);
	return OutputForward (c, s.alpha);
}

half4 fragForwardBase (VertexOutputForwardBase i) : SV_Target	// backward compatibility (this used to be the fragment entry function)
{
	return fragForwardBaseInternal(i);
}

// ------------------------------------------------------------------
//  Additive forward pass (one light per pass)

struct VertexOutputForwardAdd
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndLightDir[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:lightDir]
	LIGHTING_COORDS(5,6)
	UNITY_FOG_COORDS(7)

	// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
#if defined(_PARALLAXMAP)
	half3 viewDirForParallax			: TEXCOORD8;
#endif

//adam-begin:
#if PLANE_REFLECTION_USER_CLIPPLANE
	float clipDistance					: SV_ClipDistance;
#endif
//adam-end:
};

VertexOutputForwardAdd vertForwardAdd (VertexInput v)
{
	VertexOutputForwardAdd o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputForwardAdd, o);

	float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
	o.pos = UnityObjectToClipPos(v.vertex);
	o.tex = TexCoords(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndLightDir[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndLightDir[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndLightDir[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndLightDir[0].xyz = 0;
		o.tangentToWorldAndLightDir[1].xyz = 0;
		o.tangentToWorldAndLightDir[2].xyz = normalWorld;
	#endif
	//We need this for shadow receiving
	TRANSFER_VERTEX_TO_FRAGMENT(o);

	float3 lightDir = _WorldSpaceLightPos0.xyz - posWorld.xyz * _WorldSpaceLightPos0.w;
	#ifndef USING_DIRECTIONAL_LIGHT
		lightDir = NormalizePerVertexNormal(lightDir);
	#endif
	o.tangentToWorldAndLightDir[0].w = lightDir.x;
	o.tangentToWorldAndLightDir[1].w = lightDir.y;
	o.tangentToWorldAndLightDir[2].w = lightDir.z;

	#ifdef _PARALLAXMAP
		TANGENT_SPACE_ROTATION;
		o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
	#endif

//adam-begin:
#if PLANE_REFLECTION_USER_CLIPPLANE
		o.clipDistance = dot(posWorld, _PlaneReflectionClipPlane);
#endif
//adam-end:

	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

half4 fragForwardAddInternal (VertexOutputForwardAdd i)
{
	FRAGMENT_SETUP_FWDADD(s)

	UnityLight light = AdditiveLight (s.normalWorld, IN_LIGHTDIR_FWDADD(i), LIGHT_ATTENUATION(i));
	UnityIndirect noIndirect = ZeroIndirect ();

	half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, light, noIndirect);
	
	UNITY_APPLY_FOG_COLOR(i.fogCoord, c.rgb, half4(0,0,0,0)); // fog towards black in additive pass
	return OutputForward (c, s.alpha);
}

half4 fragForwardAdd (VertexOutputForwardAdd i) : SV_Target		// backward compatibility (this used to be the fragment entry function)
{
	return fragForwardAddInternal(i);
}

// ------------------------------------------------------------------
//  Deferred pass

struct VertexOutputDeferred
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndParallax[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
	half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UVs			
//adam-begin:
	#if 1 //UNITY_SPECCUBE_BOX_PROJECTION || UNITY_LIGHT_PROBE_PROXY_VOLUME
//adam-end:
		float3 posWorld						: TEXCOORD6;
	#endif
	#if UNITY_OPTIMIZE_TEXCUBELOD
		#if UNITY_SPECCUBE_BOX_PROJECTION
			half3 reflUVW				: TEXCOORD7;
		#else
			half3 reflUVW				: TEXCOORD6;
		#endif
	#endif

//adam-begin:
	half vertexOcclusion				: TEXCOORD10;
#if PLANE_REFLECTION_USER_CLIPPLANE
	float clipDistance					: SV_ClipDistance;
#endif
//adam-end:
};



VertexOutputDeferred vertDeferred(VertexInput v)
{
	VertexOutputDeferred o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputDeferred, o);
//adam-begin:
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	float4 posWorld = mul(unity_ObjectToWorld, v.vertex);

//adam-begin:
	#if 1 //UNITY_SPECCUBE_BOX_PROJECTION || UNITY_LIGHT_PROBE_PROXY_VOLUME || defined(_SKINNED_MESH)
//adam-end:
		o.posWorld = posWorld;
	#endif
//adam-begin:
	o.pos = mul(UNITY_MATRIX_VP, posWorld);
//adam-end:

	o.tex = TexCoords(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndParallax[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndParallax[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndParallax[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndParallax[0].xyz = 0;
		o.tangentToWorldAndParallax[1].xyz = 0;
		o.tangentToWorldAndParallax[2].xyz = normalWorld;
	#endif

	o.ambientOrLightmapUV = 0;
	#ifndef LIGHTMAP_OFF
		o.ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	#elif UNITY_SHOULD_SAMPLE_SH
		o.ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, o.ambientOrLightmapUV.rgb);
	#endif
	#ifdef DYNAMICLIGHTMAP_ON
		o.ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif
	
	#ifdef _PARALLAXMAP
		TANGENT_SPACE_ROTATION;
		half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
		o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
		o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
		o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
	#endif

	#if UNITY_OPTIMIZE_TEXCUBELOD
		o.reflUVW		= reflect(o.eyeVec, normalWorld);
	#endif

//adam-begin:
		o.vertexOcclusion = v.color.a;
#if PLANE_REFLECTION_USER_CLIPPLANE
		o.clipDistance = dot(posWorld, _PlaneReflectionClipPlane);
#endif
//adam-end:

	return o;
}

void fragDeferred (
	VertexOutputDeferred i,
	out half4 outDiffuse : SV_Target0,			// RT0: diffuse color (rgb), occlusion (a)
	out half4 outSpecSmoothness : SV_Target1,	// RT1: spec color (rgb), smoothness (a)
	out half4 outNormal : SV_Target2,			// RT2: normal (rgb), --unused, very low precision-- (a) 
	out half4 outEmission : SV_Target3			// RT3: emission (rgb), --unused-- (a)
)
{
	#if (SHADER_TARGET < 30)
		outDiffuse = 1;
		outSpecSmoothness = 1;
		outNormal = 0;
		outEmission = 0;
		return;
	#endif

	FRAGMENT_SETUP(s)
#if UNITY_OPTIMIZE_TEXCUBELOD
	s.reflUVW		= i.reflUVW;
#endif

	// no analytic lights in this pass
	UnityLight dummyLight = DummyLight (s.normalWorld);
	half atten = 1;

	// only GI
//adam-begin: pass vertex occlusion to occlusion input function
	half occlusion = Occlusion(i.tex.xy, i.vertexOcclusion);
//adam-end:
#if UNITY_ENABLE_REFLECTION_BUFFERS
	bool sampleReflectionsInDeferred = false;
#else
	bool sampleReflectionsInDeferred = true;
#endif

	UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, dummyLight, sampleReflectionsInDeferred);

	half3 color = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect).rgb;
	color += UNITY_BRDF_GI (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, occlusion, gi);

	#ifdef _EMISSION
		color += Emission (i.tex.xy);
	#endif

	#ifndef UNITY_HDR_ON
		color.rgb = exp2(-color.rgb);
	#endif

	outDiffuse = half4(s.diffColor, occlusion);
	outSpecSmoothness = half4(s.specColor, s.oneMinusRoughness);
	outNormal = half4(s.normalWorld*0.5+0.5,1);
	outEmission = half4(color, 1);
}


//
// Old FragmentGI signature. Kept only for backward compatibility and will be removed soon
//

inline UnityGI FragmentGI(
	float3 posWorld,
	half occlusion, half4 i_ambientOrLightmapUV, half atten, half oneMinusRoughness, half3 normalWorld, half3 eyeVec,
	UnityLight light,
	bool reflections)
{
	// we init only fields actually used
	FragmentCommonData s = (FragmentCommonData)0;
	s.oneMinusRoughness = oneMinusRoughness;
	s.normalWorld = normalWorld;
	s.eyeVec = eyeVec;
	s.posWorld = posWorld;
#if UNITY_OPTIMIZE_TEXCUBELOD
	s.reflUVW = reflect(eyeVec, normalWorld);
#endif
	return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, reflections);
}
inline UnityGI FragmentGI (
	float3 posWorld,
	half occlusion, half4 i_ambientOrLightmapUV, half atten, half oneMinusRoughness, half3 normalWorld, half3 eyeVec,
	UnityLight light)
{
	return FragmentGI (posWorld, occlusion, i_ambientOrLightmapUV, atten, oneMinusRoughness, normalWorld, eyeVec, light, true);
}

#endif // UNITY_STANDARD_CORE_INCLUDED
//adam-end:

//adam-begin: Custom fragment
			uniform sampler2D	_PlaneReflection;
			uniform float		_PlaneReflectionIntensityScale;
			uniform float		_PlaneReflectionBumpScale;
			uniform float		_PlaneReflectionBumpClamp;
			uniform float		_PlaneReflectionLodSteps;

			half4 fragForwardBasePR(VertexOutputForwardBase i) : SV_Target
			{
//adam-begin:
				FRAGMENT_SETUP(s)
//adam-end:
#if UNITY_OPTIMIZE_TEXCUBELOD
				s.reflUVW = i.reflUVW;
#endif

				UnityLight mainLight = MainLight(s.normalWorld);
				half atten = SHADOW_ATTENUATION(i);

//adam-begin:
				half occlusion = Occlusion(i.tex.xy, i.vertexOcclusion);
//adam-end:
				UnityGI gi = FragmentGI(s, occlusion, i.ambientOrLightmapUV, atten, mainLight);

//adam-begin:
#if PLANE_REFLECTION
				float mip = pow(1.f - s.oneMinusRoughness, 3.0 / 4.0) * _PlaneReflectionLodSteps;
				float2 vpos = i.pos / _ScreenParams.xy;

			#ifdef _NORMALMAP
				half3 tanNormal = NormalInTangentSpace(i.tex);
				vpos.xy += clamp(tanNormal.xy * _PlaneReflectionBumpScale /* * float2(-1.f, -1.f)*/, -_PlaneReflectionBumpClamp, _PlaneReflectionBumpClamp);
			#endif
				float4 lookup = float4(vpos.x, vpos.y, 0.f, mip);
				float4 hdrRefl = tex2Dlod(_PlaneReflection, lookup);
				gi.indirect.specular = hdrRefl.rgb * _PlaneReflectionIntensityScale * occlusion;
#endif
//adam-end:

				half4 c = UNITY_BRDF_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
				c.rgb += UNITY_BRDF_GI(s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, occlusion, gi);
				c.rgb += Emission(i.tex.xy);

				UNITY_APPLY_FOG(i.fogCoord, c.rgb);
				return OutputForward(c, s.alpha);
			}

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Additive forward pass (one light per pass)
		Pass
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }
			Blend [_SrcBlend] One
			Fog { Color (0,0,0,0) } // in additive pass fog should be black
			ZWrite Off
			ZTest LEqual

			CGPROGRAM
//adam-begin: 5.0 / d3d11 only (includes d3d12)
			#pragma target 5.0
			#pragma only_renderers d3d11
//adam-end:

			// -------------------------------------

			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _SPECGLOSSMAP
			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature ___ _DETAIL_MULX2
			#pragma shader_feature _PARALLAXMAP
			
//adam-begin:
			#pragma multi_compile _ PLANE_REFLECTION_USER_CLIPPLANE
			uniform float4 _PlaneReflectionClipPlane;
//adam-end:

			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog

			#pragma vertex vertAdd
			#pragma fragment fragAdd
			#include "UnityStandardCoreForward.cginc"

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Shadow rendering pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
//adam-begin: 5.0 / d3d11 only (includes d3d12)
			#pragma target 5.0
			#pragma only_renderers d3d11
//adam-end:
			
			// -------------------------------------

//adam-begin:
			#pragma multi_compile _ PLANE_REFLECTION_USER_CLIPPLANE
			uniform float4 _PlaneReflectionClipPlane;
//adam-end:

			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}

		// ------------------------------------------------------------------
		// Extracts information for lightmapping, GI (emission, albedo, ...)
		// This pass it not used during regular rendering.
		Pass
		{
			Name "META" 
			Tags { "LightMode"="Meta" }

			Cull Off

			CGPROGRAM
//adam-begin: 5.0 / d3d11 only (includes d3d12)
			#pragma target 5.0
			#pragma only_renderers d3d11
//adam-end:

			#pragma vertex vert_meta
			#pragma fragment frag_meta

			#pragma shader_feature _EMISSION
			#pragma shader_feature _SPECGLOSSMAP
			#pragma shader_feature ___ _DETAIL_MULX2

			#include "UnityStandardMeta.cginc"
			ENDCG
		}
	}


//adam-begin-end: Intentionally left out fallback sub shader until later stage.

//adam-begin:
	FallBack "Adam/Standard (Specular setup)"
	CustomEditor "AdamStandardShaderGUI"
//adam-end:
}