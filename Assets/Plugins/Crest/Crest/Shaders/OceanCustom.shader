// Crest Ocean System

// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

Shader "Crest/OceanCustom"
{
    Properties
    {
        _PainterlyMap("Painterly Map", 2D) = "white" {}


        [Header(Normals)]
        // Strength of the final surface normal (includes both wave normal and normal map)
        _NormalsStrengthOverall( "Overall Normal Strength", Range( 0.0, 1.0 ) ) = 1.0
        // Whether to add normal detail from a texture. Can be used to add visual detail to the water surface
        [Toggle] _ApplyNormalMapping("Use Normal Map", Float) = 1
        // Normal map texture (should be set to Normals type in the properties)
        [NoScaleOffset] _Normals("Normal Map", 2D) = "bump" {}
        // Scale of normal map texture
        _NormalsScale("Normal Map Scale", Range(0.01, 200.0)) = 40.0
        // Strength of normal map influence
        _NormalsStrength("Normal Map Strength", Range(0.01, 2.0)) = 0.36

        // Base light scattering settings which give water colour
        [Header(Scattering)]
        // Base colour when looking straight down into water
        _Diffuse("Scatter Colour Base", Color) = (0.0, 0.0026954073, 0.16981131, 1.0)
        // Base colour when looking into water at shallow/grazing angle
        _DiffuseGrazing("Scatter Colour Grazing", Color) = (0.0, 0.003921569, 0.1686274, 1.0)
        // Changes colour in shadow. Requires 'Create Shadow Data' enabled on OceanRenderer script.
        [Toggle] _Shadows("Shadowing", Float) = 0
        // Base colour in shadow
        _DiffuseShadow("Scatter Colour Shadow", Color) = (0.0, 0.0013477041, 0.084905684, 1.0)

        [Header(Shallow Scattering)]
        // Enable light scattering in shallow water
        [Toggle] _SubSurfaceShallowColour("Enable", Float) = 0
        // Colour in shallow water
        _SubSurfaceShallowCol("Scatter Colour Shallow", Color) = (0.0, 0.003921569, 0.24705884, 1.0)
        // Max depth that is considered 'shallow'
        _SubSurfaceDepthMax("Scatter Colour Shallow Depth Max", Range(0.01, 50.0)) = 10.0
        // Fall off of shallow scattering
        _SubSurfaceDepthPower("Scatter Colour Shallow Depth Falloff", Range(0.01, 10.0)) = 2.5
        // Shallow water colour in shadow (see comment on Shadowing param above)
        _SubSurfaceShallowColShadow("Scatter Colour Shallow Shadow", Color) = (0.0, 0.0053968453, 0.17, 1)

        [Header(Subsurface Scattering)]
        // Whether to to emulate light scattering through the water volume
        [Toggle] _SubSurfaceScattering("Enable", Float) = 1
        // Colour tint for primary light contribution
        _SubSurfaceColour("SSS Tint", Color) = (0.08850684, 0.497, 0.45615074, 1.0)
        // Amount of primary light contribution that always comes in
        _SubSurfaceBase("SSS Intensity Base", Range(0.0, 4.0)) = 0.0
        // Primary light contribution in direction of light to emulate light passing through waves
        _SubSurfaceSun("SSS Intensity Sun", Range(0.0, 10.0)) = 1.7
        // Fall-off for primary light scattering to affect directionality
        _SubSurfaceSunFallOff("SSS Sun Falloff", Range(1.0, 16.0)) = 5.0

        // Reflection properites
        [Header(Reflection Environment)]
        // Strength of specular lighting response
        _Specular("Specular", Range(0.0, 1.0)) = 0.7
        // Controls blurriness of reflection
        _Roughness("Roughness", Range(0.0, 1.0)) = 0.0
        // Controls harshness of Fresnel behaviour
        _FresnelPower("Fresnel Power", Range(1.0, 20.0)) = 5.0
        // Index of refraction of air. Can be increased to almost 1.333 to increase visibility up through water surface.
        _RefractiveIndexOfAir("Refractive Index of Air", Range(1.0, 2.0)) = 1.0
        // Index of refraction of water. Typically left at 1.333.
        _RefractiveIndexOfWater("Refractive Index of Water", Range(1.0, 2.0)) = 1.333
        // Dynamically rendered 'reflection plane' style reflections. Requires OceanPlanarReflection script added to main camera.
        [Toggle] _PlanarReflections("Planar Reflections", Float) = 0
        // How much the water normal affects the planar reflection
        _PlanarReflectionNormalsStrength("Planar Reflections Distortion", Float) = 1
        // Multiplier to adjust the strength of the distortion at a distance.
        _PlanarReflectionDistanceFactor("Planar Reflections Distortion Distance Factor", Range(0.0, 1.0)) = 0.0
        // Multiplier to adjust how intense the reflection is
        _PlanarReflectionIntensity("Planar Reflection Intensity", Range(0.0, 1.0)) = 1.0
        // Whether to use an overridden reflection cubemap (provided in the next property)
        [Toggle] _OverrideReflectionCubemap("Override Reflection Cubemap", Float) = 0
        // Custom environment map to reflect
        [NoScaleOffset] _ReflectionCubemapOverride("Reflection Cubemap Override", CUBE) = "" {}

        [Header(Procedural Skybox)]
        // Enable a simple procedural skybox, not suitable for realistic reflections, but can be useful to give control over reflection colour
        // especially in stylized/non realistic applications
        [Toggle] _ProceduralSky("Enable", Float) = 0
        // Base sky colour
        [HDR] _SkyBase("Base", Color) = (1.0, 1.0, 1.0, 1.0)
        // Colour in sun direction
        [HDR] _SkyTowardsSun("Towards Sun", Color) = (1.0, 1.0, 1.0, 1.0)
        // Direction fall off
        _SkyDirectionality("Directionality", Range(0.0, 0.99)) = 1.0
        // Colour away from sun direction
        [HDR] _SkyAwayFromSun("Away From Sun", Color) = (1.0, 1.0, 1.0, 1.0)

        [Header(Add Directional Light)]
        // Add specular highlights from the the primary light.
        [Toggle] _ComputeDirectionalLight("Enable", Float) = 1
        // Specular highlight intensity.
        _DirectionalLightBoost("Boost", Range(0.0, 512.0)) = 7.0
        // Falloff of the specular highlights from source to camera.
        _DirectionalLightFallOff("Falloff", Range(1.0, 4096.0)) = 275.0
        // Helps to spread out specular highlight in mid-to-background.
        [Toggle] _DirectionalLightVaryRoughness("Vary Falloff Over Distance", Float) = 0
        // Definition of far distance.
        _DirectionalLightFarDistance("Far Distance", Float) = 137.0
        // Same as "Falloff" except only up to "Far Distance".
        _DirectionalLightFallOffFar("Falloff At Far Distance", Range(1.0, 4096.0)) = 42.0

        [Header(Foam)]
        // Enable foam layer on ocean surface
        [Toggle] _Foam("Enable", Float) = 1
        // Foam texture
        [NoScaleOffset] _FoamTexture("Foam", 2D) = "white" {}
        // Foam texture scale
        _FoamScale("Foam Scale", Range(0.01, 50.0)) = 10.0
        // Controls how gradual the transition is from full foam to no foam
        _WaveFoamFeather("Foam Feather", Range(0.001, 1.0)) = 0.4
        // Scale intensity of lighting
        _WaveFoamLightScale("Foam Light Scale", Range(0.0, 2.0)) = 1.35
        // Colour tint for whitecaps / foam on water surface
        _FoamWhiteColor("Foam Tint", Color) = (1.0, 1.0, 1.0, 1.0)
        // Proximity to sea floor where foam starts to get generated
        _ShorelineFoamMinDepth("Shoreline Foam Min Depth", Range(0.01, 5.0)) = 0.27

        [Header(Foam 3D Lighting)]
        // Generates normals for the foam based on foam values/texture and use it for foam lighting
        [Toggle] _Foam3DLighting("Enable", Float) = 1
        // Strength of the generated normals
        _WaveFoamNormalStrength("Foam Normal Strength", Range(0.0, 30.0)) = 3.5
        // Acts like a gloss parameter for specular response
        _WaveFoamSpecularFallOff("Specular Falloff", Range(1.0, 512.0)) = 293.0
        // Strength of specular response
        _WaveFoamSpecularBoost("Specular Boost", Range(0.0, 16.0)) = 0.15

        [Header(Foam Bubbles)]
        // Colour tint bubble foam underneath water surface
        _FoamBubbleColor("Foam Bubbles Color", Color) = (0.64, 0.83, 0.82, 1.0)
        // Parallax for underwater bubbles to give feeling of volume
        _FoamBubbleParallax("Foam Bubbles Parallax", Range(0.0, 0.5)) = 0.14
        // How much underwater bubble foam is generated
        _WaveFoamBubblesCoverage("Foam Bubbles Coverage", Range(0.0, 5.0)) = 1.68

        [Header(Transparency)]
        // Whether light can pass through the water surface
        [Toggle] _Transparency("Enable", Float) = 1
        // Scattering coefficient within water volume, per channel
        _DepthFogDensity("Depth Fog Density", Vector) = (0.9, 0.3, 0.35, 1.0)
        // How strongly light is refracted when passing through water surface
        _RefractionStrength("Refraction Strength", Range(0.0, 2.0)) = 0.5

        [Header(Caustics)]
        // Approximate rays being focused/defocused on underwater surfaces
        [Toggle] _Caustics("Enable", Float) = 1
        // Caustics texture
        [NoScaleOffset] _CausticsTexture("Caustics", 2D) = "black" {}
        // Caustics texture scale
        _CausticsTextureScale("Caustics Scale", Range(0.0, 25.0)) = 5.0
        // The 'mid' value of the caustics texture, around which the caustic texture values are scaled
        _CausticsTextureAverage("Caustics Texture Grey Point", Range(0.0, 1.0)) = 0.07
        // Scaling / intensity
        _CausticsStrength("Caustics Strength", Range(0.0, 10.0)) = 3.2
        // The depth at which the caustics are in focus
        _CausticsFocalDepth("Caustics Focal Depth", Range(0.0, 250.0)) = 2.0
        // The range of depths over which the caustics are in focus
        _CausticsDepthOfField("Caustics Depth of Field", Range(0.01, 1000.0)) = 0.33
        // How much the caustics texture is distorted
        _CausticsDistortionStrength("Caustics Distortion Strength", Range(0.0, 0.25)) = 0.16
        // The scale of the distortion pattern used to distort the caustics
        _CausticsDistortionScale("Caustics Distortion Scale", Range(0.01, 50.0)) = 25.0

        // To use the underwater effect the UnderWaterCurtainGeom and UnderWaterMeniscus prefabs must be parented to the camera.
        [Header(Underwater)]
        // Whether the underwater effect is being used. This enables code that shades the surface correctly from underneath.
        [Toggle] _Underwater("Enable", Float) = 0
        // Ordinarily set this to Back to cull back faces, but set to Off to make sure both sides of the surface draw if the
        // underwater effect is being used.
        [Enum(CullMode)] _CullMode("Cull Mode", Int) = 2

        [Header(Flow)]
        // Flow is horizontal motion in water as demonstrated in the 'whirlpool' example scene. 'Create Flow Sim' must be
        // enabled on the OceanRenderer to generate flow data.
        [Toggle] _Flow("Enable", Float) = 0

        [Header(Clip Surface)]
        // Discards ocean surface pixels. Requires 'Create Clip Surface Data' enabled on OceanRenderer script.
        [Toggle] _ClipSurface("Enable", Float) = 0
        // Clips purely based on water depth
        [Toggle] _ClipUnderTerrain("Clip Below Terrain (Requires depth cache)", Float) = 0

        [Header(Albedo)]
        // Albedo is a colour that is composited onto the surface. Requires 'Create Albedo Data' enabled on OceanRenderer component.
        [Toggle] _Albedo("Enable", Float) = 0

        [Header(Rendering)]
        // What projection modes will this material support? Choosing perspective or orthographic is an optimisation.
        [KeywordEnum(Both, Perspective, Orthographic)] _Projection("Projection Support", Float) = 0.0

        [Header(Debug Options)]
        [Toggle] _DebugDisableShapeTextures("Debug Disable Shape Textures", Float) = 0
        [Toggle] _DebugDisableSmoothLOD("Debug Disable Smooth LOD", Float) = 0
        [KeywordEnum(None, ShapeSample, AnimatedWaves, Flow, Shadows, Foam)] _DebugVisualise("Debug Visualise With Colours", Float) = 0
    }








    SubShader
    {
        Tags
        {
            // Unity treats anything after Geometry+500 as transparent, and will render it in a forward manner and copy
            // out the gbuffer data and do post processing before running it. Discussion of this in issue #53.
            "Queue"="Geometry+20"
            "IgnoreProjector"="True"
            "RenderType"="Opaque"
            "DisableBatching"="True"
        }

        GrabPass
        {
            "_BackgroundTexture"
        }

        Pass
        {
            // Culling user defined - can be inverted for under water
            Cull [_CullMode]

            Tags
            {
                // Tell Unity we're going to render water in forward manner and we're going to do lighting and it will set
                // the appropriate uniforms.
                "LightMode"="ForwardBase"
            }

            CGPROGRAM

            #include "Underwater/CustomShared.hlsl"
            #include "Assets/Resources/Shaders/Chunks/snoise.cginc"
            #include "Assets/Resources/Shaders/Chunks/noise.cginc"

            // Argument name is v because some macros like COMPUTE_EYEDEPTH require it.
            Varyings Vert(Attributes v)
            {
                Varyings o;

                DoVert(v, o);
                
                return o;
            }


            sampler2D _PainterlyMap;

            half4 Frag(const Varyings input, const bool i_isFrontFace : SV_IsFrontFace) : SV_Target
            {
                // We need this when sampling a screenspace texture.
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                








                /*

                CLIP SURFACE 

                */

                //clipsurface means that the ocean surface is clipped by the water depth??? GPT
                
                #if _CLIPSURFACE_ON
                {
                    // Clip surface
                    half clipValue = 0.0;

                    uint slice0; uint slice1; float alpha;
                    // Do not include transition slice to avoid blending as we do a black border instead.
                    PosToSliceIndices(input.worldPos.xz, 0, _SliceCount - 1.0, _CrestCascadeData[0]._scale, slice0, slice1, alpha);

                    const CascadeParams cascadeData0 = _CrestCascadeData[slice0];
                    const CascadeParams cascadeData1 = _CrestCascadeData[slice1];
                    const float weight0 = (1.0 - alpha) * cascadeData0._weight;
                    const float weight1 = (1.0 - weight0) * cascadeData1._weight;

                    bool clear = false;
                    if (weight0 > 0.001)
                    {
                        const float3 uv = WorldToUV(input.worldPos.xz, cascadeData0, slice0);
                        SampleClip(_LD_TexArray_ClipSurface, uv, weight0, clipValue);

                        if ((float)_LD_SliceIndex == _SliceCount - 1.0 && IsOutsideOfUV(uv.xy, cascadeData0._oneOverTextureRes))
                        {
                            clear = true;
                        }
                    }
                    if (weight1 > 0.001)
                    {
                        const float3 uv = WorldToUV(input.worldPos.xz, cascadeData1, slice1);
                        SampleClip(_LD_TexArray_ClipSurface, uv, weight1, clipValue);
                    }

                    if (clear)
                    {
                        clipValue = _CrestClipByDefault;
                    }

                    clipValue = lerp(_CrestClipByDefault, clipValue, weight0 + weight1);
                    // Add 0.5 bias to tighten and smooth clipped edges.
                    clip(-clipValue + 0.5);
                }
                #endif // _CLIPSURFACE_ON








                /*

                Cascade Data

                */


                const CascadeParams cascadeData0 = _CrestCascadeData[_LD_SliceIndex];
                const CascadeParams cascadeData1 = _CrestCascadeData[_LD_SliceIndex + 1];
                const PerCascadeInstanceData instanceData = _CrestPerCascadeInstanceData[_LD_SliceIndex];




                /*

                Checks if underwater
                */


                #if _UNDERWATER_ON
                const bool underwater = IsUnderwater(i_isFrontFace, _CrestForceUnderwater);
                #else
                const bool underwater = false;
                #endif




                // LOD Weights

                const float lodAlpha = input.lodAlpha_worldXZUndisplaced_oceanDepth.x;
                const float2 positionXZWSUndisplaced = input.lodAlpha_worldXZUndisplaced_oceanDepth.yz;
                const float wt_smallerLod = (1.0 - lodAlpha) * cascadeData0._weight;
                const float wt_biggerLod = (1.0 - wt_smallerLod) * cascadeData1._weight;



                // SCreen position
                half3 screenPos = input.screenPosXYW;
                half2 uvDepth = screenPos.xy / screenPos.z;





                #if CREST_WATER_VOLUME
                //ApplyVolumeToOceanSurface(input.positionCS);
                #endif


                #if _CLIPUNDERTERRAIN_ON
                clip(input.lodAlpha_worldXZUndisplaced_oceanDepth.w + 2.0);
                #endif





                // view position
                half3 view = normalize(_WorldSpaceCameraPos - input.worldPos);

                // water surface depth, and underlying scene opaque surface depth
                float pixelZ = CrestLinearEyeDepth(input.positionCS.z);

                // Raw depth is logarithmic for perspective, and linear (0-1) for orthographic.
                float rawDepth = CREST_SAMPLE_SCENE_DEPTH_X(uvDepth);

                // scene depth
                float sceneZ = CrestLinearEyeDepth(rawDepth);

                // getting light direction and color
                float3 lightDir = WaveHarmonic::Crest::WorldSpaceLightDir(input.worldPos);
                half3 lightCol = _LightColor0;
                // Soft shadow, hard shadow
                fixed2 shadow = (fixed2)1.0
                #if _SHADOWS_ON
                - input.flow_shadow.zw
                #endif
                ;

                // Normal - geom + normal mapping. Subsurface scattering.
                float3 dummy = 0.;
                float3 n_pixel = float3(0.0, 1.0, 0.0);
                half sss = 0.;
                #if _FOAM_ON
                float foam = 0.0;
                #endif
                #if _ALBEDO_ON
                half4 albedo = 0.0;
                #endif


                float3 displacement = 0;

                // we have to blend if our two LODS are biggger then zero
                /// this gets us our FOAM  , Albedo and Displacement
                if (wt_smallerLod > 0.001)
                {
                    const float3 uv_slice_smallerLod = WorldToUV(positionXZWSUndisplaced, cascadeData0, _LD_SliceIndex);
                    SampleDisplacementsNormals(_LD_TexArray_AnimatedWaves, uv_slice_smallerLod, wt_smallerLod, cascadeData0._oneOverTextureRes, cascadeData0._texelWidth, dummy, n_pixel.xz, sss);

                    #if _FOAM_ON
                    SampleFoam(_LD_TexArray_Foam, uv_slice_smallerLod, wt_smallerLod, foam);
                    #endif

                    #if _ALBEDO_ON
                    SampleAlbedo(_LD_TexArray_Albedo, uv_slice_smallerLod, wt_smallerLod, albedo);
                    #endif

                    
                    SampleDisplacements(_LD_TexArray_AnimatedWaves, uv_slice_smallerLod, wt_smallerLod, displacement);

                }
                if (wt_biggerLod > 0.001)
                {
                    const float3 uv_slice_biggerLod = WorldToUV(positionXZWSUndisplaced, cascadeData1, _LD_SliceIndex + 1);
                    SampleDisplacementsNormals(_LD_TexArray_AnimatedWaves, uv_slice_biggerLod, wt_biggerLod, cascadeData1._oneOverTextureRes, cascadeData1._texelWidth, dummy, n_pixel.xz, sss);

                    #if _FOAM_ON
                    SampleFoam(_LD_TexArray_Foam, uv_slice_biggerLod, wt_biggerLod, foam);
                    #endif

                    #if _ALBEDO_ON
                    SampleAlbedo(_LD_TexArray_Albedo, uv_slice_biggerLod, wt_biggerLod, albedo);
                    #endif

                    
                    SampleDisplacements(_LD_TexArray_AnimatedWaves, uv_slice_biggerLod, wt_biggerLod, displacement);
                }



                #if _SUBSURFACESCATTERING_ON
                // Extents need the default SSS to avoid popping and not being noticeably different.
                if (_LD_SliceIndex == ((uint)_SliceCount - 1))
                {
                    sss = CREST_SSS_MAXIMUM - CREST_SSS_RANGE;
                }
                #endif



                // normal map
                #if _APPLYNORMALMAPPING_ON
                #if _FLOW_ON
                ApplyNormalMapsWithFlow(_NormalsTiledTexture, positionXZWSUndisplaced, input.flow_shadow.xy, lodAlpha, cascadeData0, instanceData, n_pixel);
                #else
                n_pixel.xz += SampleNormalMaps(_NormalsTiledTexture, positionXZWSUndisplaced, 0.0, lodAlpha, cascadeData0, instanceData);
                #endif
                #endif



                n_pixel.xz += float2(-input.seaLevelDerivs.x, -input.seaLevelDerivs.y);

                // Finalise normal
                n_pixel.xz *= _NormalsStrengthOverall;
                n_pixel = normalize( n_pixel );
                if (underwater) n_pixel = -n_pixel;

                // Foam - underwater bubbles and whitefoam
                half3 bubbleCol = (half3)0.;
                #if _FOAM_ON
                // Foam can saturate.
                foam = saturate(foam);

                half4 whiteFoamCol;
                #if !_FLOW_ON
                ComputeFoam
                (
                _FoamTiledTexture,
                foam,
                input.worldPos.xz,
                positionXZWSUndisplaced,
                0.0, // Flow
                n_pixel,
                pixelZ,
                sceneZ,
                view,
                lightDir,
                shadow.y,
                lodAlpha,
                bubbleCol,
                whiteFoamCol,
                cascadeData0,
                cascadeData1
                );
                #else
                ComputeFoamWithFlow
                (
                _FoamTiledTexture,
                input.flow_shadow.xy,
                foam,
                positionXZWSUndisplaced,
                input.worldPos.xz,
                n_pixel,
                pixelZ,
                sceneZ,
                view,
                lightDir,
                shadow.y,
                lodAlpha,
                bubbleCol,
                whiteFoamCol,
                cascadeData0,
                cascadeData1
                );
                #endif // _FLOW_ON
                #endif // _FOAM_ON

                // Compute color of ocean - in-scattered light + refracted scene
                half3 scatterCol = ScatterColour
                (
                input.lodAlpha_worldXZUndisplaced_oceanDepth.w,
                #if defined(CREST_UNDERWATER_BEFORE_TRANSPARENT) && defined(_SHADOWS_ON)
                underwater ? UnderwaterShadowSSS(_WorldSpaceCameraPos.xz) :
                #endif
                shadow.x,
                sss,
                view,
                #if CREST_UNDERWATER_BEFORE_TRANSPARENT
                underwater ? _CrestAmbientLighting :
                #endif
                WaveHarmonic::Crest::AmbientLight(),
                lightDir,
                lightCol,
                underwater
                );
                half3 col = OceanEmission
                (
                view,
                n_pixel,
                lightDir,
                input.grabPos,
                pixelZ,
                input.positionCS.z,
                uvDepth,
                input.positionCS.xy,
                sceneZ,
                rawDepth,
                bubbleCol,
                underwater,
                scatterCol,
                cascadeData0,
                cascadeData1
                );

                // Light that reflects off water surface

                // Soften reflection at intersections with objects/surfaces
                #if _TRANSPARENCY_ON
                // Above water depth outline is handled in OceanEmission.
                sceneZ = (underwater ? CrestLinearEyeDepth(CREST_MULTISAMPLE_SCENE_DEPTH(uvDepth, rawDepth)) : sceneZ);
                float reflAlpha = saturate((sceneZ  - pixelZ) / 0.2);
                #else
                // This addresses the problem where screenspace depth doesnt work in VR, and so neither will this. In VR people currently
                // disable transparency, so this will always be 1.0.
                float reflAlpha = 1.0;
                #endif

                #if _UNDERWATER_ON
                if (underwater)
                {
                    ApplyReflectionUnderwater(view, n_pixel, lightDir, shadow.y, screenPos.xyzz, scatterCol, reflAlpha, col);
                }
                else
                #endif
                {
                    ApplyReflectionSky(view, n_pixel, lightDir, shadow.y, screenPos.xyzz, pixelZ, reflAlpha, col);
                }

                // Override final result with white foam - bubbles on surface
                #if _FOAM_ON
                col = lerp(col, whiteFoamCol.rgb, whiteFoamCol.a);
                #endif

                // Composite albedo input on top
                #if _ALBEDO_ON
                col = lerp(col, albedo.xyz, albedo.w * reflAlpha);
                #endif
                

                
                #if CREST_UNDERWATER_BEFORE_TRANSPARENT
                else
                {
                    // underwater - do depth fog
                    col = lerp(col, scatterCol, saturate(1. - exp(-_DepthFogDensity.xyz * pixelZ)));
                }
                #endif

                

                //col = n_pixel * .5 + .5;
                //col = foam;
                //col = foam *bubbleCol;

                float3 eye = _WorldSpaceCameraPos - input.worldPos;
                float3 normal = n_pixel;
                float2 uvPosition = input.lodAlpha_worldXZUndisplaced_oceanDepth.yz;

                float3 eyeMatch = dot( normalize(eye),normal);
                
                /* col = 1-eyeMatch;//bubbleCol + whiteFoamCol;
                col = sin(input.positionCS);
                col = 0;
                col.x = sin( input.worldPos.x * 1.0);
                col.z = sin( input.worldPos.z * 1.0);



                //lodAlpha;
                //positionXZWSUndisplaced;
                //wt_smallerLod;
                //wt_biggerLod;


                //  col = lerp(0, 10, whiteFoamCol.a);
                // col = normal * .5 + .5;

                col = normal * .5 + .5;

                col = 1-pow( dot( normal , _WorldSpaceLightPos0),4) * 1000;
                col =pow( dot( normal , _WorldSpaceLightPos0),4);*/



                //                col = albedo;
                
                /* 

                view,
                n_pixel,
                lightDir,
                input.grabPos,
                pixelZ,
                input.positionCS.z,
                uvDepth,
                input.positionCS.xy,
                sceneZ,
                rawDepth,
                bubbleCol,
                underwater,
                scatterCol,
                cascadeData0,
                cascadeData1
                input.seaLevelDerivs
                input.positionCS  float4
                input flow_shadow float4
                input screenPosXYW  float3
                lodAlpha_worldXZUndisplaced_oceanDepth float4
                worldPos float3
                grabPos float4
                seaLevelDerivs float2
                displacement!
                */
                
                col = rawDepth * 100;

                col = bubbleCol;
                col = underwater ? 1 : 0;
                col = scatterCol; // how are we getting this? can we make more expensive but do got rays?
                col = cascadeData0._weight;
                col = 0;
                col.xy= sin(10*uvDepth);
                col = input.grabPos;
                col = lightDir;

                float3 refl = normalize(reflect( _WorldSpaceLightPos0, normal));

                float reflMatch = dot( refl, -normalize(eye));

                col = saturate(pow( reflMatch, 1000) * 4);

                float rim = 1-saturate(dot( normal, normalize(eye)));

                //col += saturate( pow( rim , 30) * 1);

                //                col = foam;
                
                col = 0;
                col.xyz = normalize(input.flow_shadow.xyz );//* 10000;

                col = normal * .5 + .5;
                col *= .3;

                col = 0;// bubbleCol;

                #if _FOAM_ON
                /// col = foam ;
                
                col = lerp(col, whiteFoamCol.rgb, whiteFoamCol.a);
                col = whiteFoamCol * foam;//whiteFoamCol.a;
                col = whiteFoamCol.a * 10;
                col = bubbleCol;
                #endif


                col = pow( reflMatch , 300) * 4;
                //                col = n_pixel - float3(0,.4,0);

                //col = n_pixel;
                //  col = normal * .5 + .5;

                //col = dot( normal, normalize(eye)) < 0;

                //col = normalize(-eye) * .5 + .5;
                /* if( !underwater && dot( normal, normalize(eye)) < 0){
                    col = float3(1,0,0);
                    col = noise( input.worldPos  * .1) + noise( input.worldPos  * .3) * .5 + noise( input.worldPos  * .7) * .25;
                    col *= 1/1.75;
                    col = pow( col,10) * 100;
                }*/

                //col = sin( displacement * .1);

                //if( displacement.y * .8 > abs(displacement.x) && displacement.y * .8 > abs(displacement.z)){
                    //    col= 1;
                //}


                float lightMatch = dot( normal, lightDir);

                col = tex2D(_PainterlyMap, uvPosition * .04).rgb;

                if( lightMatch < .2){
                    col = col.x;
                    }else{
                    col = col.z;
                }

                col += lightMatch;

                //col = normal * .5 + .5;
                
                return half4(col, 1.);
            }

            ENDCG
        }




















        Pass
        {
            Name "SceneSelectionPass"
            Tags { "LightMode" = "SceneSelectionPass" }

            CGPROGRAM
            
            #include "Underwater/UnderwaterMaskShared.hlsl"
            ENDCG
        }


















        
        // shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            Cull Off
            CGPROGRAM
            #pragma multi_compile_shadowcaster



            #include "Underwater/CustomShared.hlsl"

            

            Varyings Vert(Attributes v)
            {
                Varyings o;


                DoVert(v, o);
                /*
                //float4 p = UnityObjectToClipPos(v.vertex);
                float4 p = float4( vert.vertex.xyz, 1);
                // TRANSFER_SHADOW_CASTER_NOPOS(o,o.pos);

                
                UNITY_SETUP_INSTANCE_ID(vert);
                UNITY_TRANSFER_INSTANCE_ID(vert, o); // necessary only if you want to access instanced properties in the fragment Shader.
                
                
                int instanceID = 0;
                #if defined(UNITY_INSTANCING_ENABLED)
                instanceID = UNITY_GET_INSTANCE_ID(vert);
                #endif


                o.nor = normalize(mul( unity_ObjectToWorld,  float4( vert.normal,0)).xyz);
                // o.worldPos = worldPos.xyz;

                float3 wPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;
                float3 windOffset = GetWindOffset( instanceID, wPos );
                
                o.worldPos = wPos + windOffset;//windAmount;
                // o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));


                // o.pos = mul (UNITY_MATRIX_VP, float4(worldPos,1.0f));
                o.eye = _WorldSpaceCameraPos - o.worldPos.xyz;
                //float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 worldNormal  = o.nor;// UnityObjectToWorldNormal( vert.normal);

                float4 opos = 0;
                opos = CustomShadowPos(float4(o.worldPos,1), worldNormal);
                opos = UnityApplyLinearShadowBias(opos);
                o.pos = opos;
                */

                //TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }



            float4 Frag(Varyings i) : SV_Target
            {


                SHADOW_CASTER_FRAGMENT(i);
                

            }
            ENDCG
        }

    }

    // If the above doesn't work then error.
    FallBack "Diffuse"
}
