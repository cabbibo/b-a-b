   #pragma vertex Vert
    #pragma fragment Frag
    // for VFACE
    #pragma target 3.0
    #pragma multi_compile_fog

    // #pragma enable_d3d11_debug_symbols

    #pragma shader_feature_local _APPLYNORMALMAPPING_ON
    #pragma shader_feature_local _COMPUTEDIRECTIONALLIGHT_ON
    #pragma shader_feature_local _DIRECTIONALLIGHTVARYROUGHNESS_ON
    #pragma shader_feature_local _SUBSURFACESCATTERING_ON
    #pragma shader_feature_local _SUBSURFACESHALLOWCOLOUR_ON
    #pragma shader_feature_local _TRANSPARENCY_ON
    #pragma shader_feature_local _CAUSTICS_ON
    #pragma shader_feature_local _FOAM_ON
    #pragma shader_feature_local _FOAM3DLIGHTING_ON
    #pragma shader_feature_local _PLANARREFLECTIONS_ON
    #pragma shader_feature_local _OVERRIDEREFLECTIONCUBEMAP_ON

    #pragma shader_feature_local _PROCEDURALSKY_ON
    #pragma shader_feature_local _UNDERWATER_ON
    #pragma shader_feature_local _FLOW_ON
    #pragma shader_feature_local _SHADOWS_ON
    #pragma shader_feature_local _CLIPSURFACE_ON
    #pragma shader_feature_local _CLIPUNDERTERRAIN_ON
    #pragma shader_feature_local _ALBEDO_ON

    #pragma shader_feature_local _ _PROJECTION_PERSPECTIVE _PROJECTION_ORTHOGRAPHIC

    #pragma shader_feature_local _DEBUGDISABLESHAPETEXTURES_ON
    #pragma shader_feature_local _DEBUGDISABLESMOOTHLOD_ON
    #pragma shader_feature_local _DEBUGVISUALISE_NONE _DEBUGVISUALISE_SHAPESAMPLE _DEBUGVISUALISE_ANIMATEDWAVES _DEBUGVISUALISE_FLOW _DEBUGVISUALISE_SHADOWS _DEBUGVISUALISE_FOAM

    #pragma multi_compile _ CREST_UNDERWATER_BEFORE_TRANSPARENT

    // Clipping the ocean surface for underwater volumes.
    #pragma multi_compile _ CREST_WATER_VOLUME_2D CREST_WATER_VOLUME_HAS_BACKFACE

    #pragma multi_compile _ CREST_FLOATING_ORIGIN

    #include "UnityCG.cginc"
    #include "Lighting.cginc"

    #include "Helpers/BIRP/Core.hlsl"
    #include "Helpers/BIRP/InputsDriven.hlsl"

    #include "ShaderLibrary/Common.hlsl"

    #include "OceanGlobals.hlsl"
    #include "OceanInputsDriven.hlsl"
    #include "OceanShaderData.hlsl"
    #include "OceanHelpersNew.hlsl"
    #include "OceanVertHelpers.hlsl"
    #include "OceanShaderHelpers.hlsl"
    #include "OceanLightingHelpers.hlsl"

    #include "Helpers/WaterVolume.hlsl"

    #include "OceanEmission.hlsl"
    #include "OceanNormalMapping.hlsl"
    #include "OceanReflection.hlsl"
    #include "OceanFoam.hlsl"


    
    struct Attributes
    {
        // The old unity macros require this name and type.
        float4 vertex : POSITION;

        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        half4 flow_shadow : TEXCOORD1;
        half3 screenPosXYW : TEXCOORD4;
        float4 lodAlpha_worldXZUndisplaced_oceanDepth : TEXCOORD5;
        float3 worldPos : TEXCOORD7;
        #if defined(_DEBUGVISUALISE_SHAPESAMPLE) || defined(_DEBUGVISUALISE_ANIMATEDWAVES)
            half3 debugtint : TEXCOORD8;
        #endif
        half4 grabPos : TEXCOORD9;
        float2 seaLevelDerivs : TEXCOORD10;

        UNITY_FOG_COORDS(3)

        UNITY_VERTEX_OUTPUT_STEREO
    };


    

    void DoVert( Attributes v, out Varyings o){
        
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_OUTPUT(Varyings, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        const CascadeParams cascadeData0 = _CrestCascadeData[_LD_SliceIndex];
        const CascadeParams cascadeData1 = _CrestCascadeData[_LD_SliceIndex + 1];
        const PerCascadeInstanceData instanceData = _CrestPerCascadeInstanceData[_LD_SliceIndex];

        // Move to world space
        o.worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1.0));

        // Vertex snapping and lod transition
        float lodAlpha;
        const float meshScaleLerp = instanceData._meshScaleLerp;
        const float gridSize = instanceData._geoGridWidth;
        SnapAndTransitionVertLayout(meshScaleLerp, cascadeData0, gridSize, o.worldPos, lodAlpha);
        {
            // Scale up by small "epsilon" to solve numerical issues. Expand slightly about tile center.
            // :OceanGridPrecisionErrors
            const float2 tileCenterXZ = UNITY_MATRIX_M._m03_m23;
            const float2 cameraPositionXZ = abs(_WorldSpaceCameraPos.xz);
            // Scale "epsilon" by distance from zero. There is an issue where overlaps can cause SV_IsFrontFace
            // to be flipped (needs to be investigated). Gaps look bad from above surface, and overlaps look bad
            // from below surface. We want to close gaps without introducing overlaps. A fixed "epsilon" will
            // either not solve gaps at large distances or introduce too many overlaps at small distances. Even
            // with scaling, there are still unsolvable overlaps underwater (especially at large distances).
            // 100,000 (0.00001) is the maximum position before Unity warns the user of precision issues.
            o.worldPos.xz = lerp(tileCenterXZ, o.worldPos.xz, lerp(1.0, 1.01, max(cameraPositionXZ.x, cameraPositionXZ.y) * 0.00001));
        }

        o.lodAlpha_worldXZUndisplaced_oceanDepth.x = lodAlpha;
        o.lodAlpha_worldXZUndisplaced_oceanDepth.yz = o.worldPos.xz;

        // sample shape textures - always lerp between 2 LOD scales, so sample two textures
        o.flow_shadow = half4(0.0, 0.0, 0.0, 0.0);

        o.lodAlpha_worldXZUndisplaced_oceanDepth.w = CREST_OCEAN_DEPTH_BASELINE;
        // Sample shape textures - always lerp between 2 LOD scales, so sample two textures

        // Calculate sample weights. params.z allows shape to be faded out (used on last lod to support pop-less scale transitions)
        const float wt_smallerLod = (1. - lodAlpha) * cascadeData0._weight;
        const float wt_biggerLod = (1. - wt_smallerLod) * cascadeData1._weight;
        // Sample displacement textures, add results to current world pos / normal / foam
        const float2 positionWS_XZ_before = o.worldPos.xz;

        // Data that needs to be sampled at the undisplaced position
        if (wt_smallerLod > 0.001)
        {
            const float3 uv_slice_smallerLod = WorldToUV(positionWS_XZ_before, cascadeData0, _LD_SliceIndex);

            #if _DEBUGVISUALISE_ANIMATEDWAVES
                o.debugtint = _LD_TexArray_AnimatedWaves.SampleLevel(LODData_linear_clamp_sampler, uv_slice_smallerLod, 0.0);
            #endif

            #if !_DEBUGDISABLESHAPETEXTURES_ON
                SampleDisplacements(_LD_TexArray_AnimatedWaves, uv_slice_smallerLod, wt_smallerLod, o.worldPos);
            #endif

            #if _FLOW_ON
                SampleFlow(_LD_TexArray_Flow, uv_slice_smallerLod, wt_smallerLod, o.flow_shadow.xy);
            #endif
        }
        if (wt_biggerLod > 0.001)
        {
            const float3 uv_slice_biggerLod = WorldToUV(positionWS_XZ_before, cascadeData1, _LD_SliceIndex + 1);

            #if _DEBUGVISUALISE_ANIMATEDWAVES
                o.debugtint = _LD_TexArray_AnimatedWaves.SampleLevel(LODData_linear_clamp_sampler, uv_slice_biggerLod, 0.0);
            #endif

            #if !_DEBUGDISABLESHAPETEXTURES_ON
                SampleDisplacements(_LD_TexArray_AnimatedWaves, uv_slice_biggerLod, wt_biggerLod, o.worldPos);
            #endif

            #if _FLOW_ON
                SampleFlow(_LD_TexArray_Flow, uv_slice_biggerLod, wt_biggerLod, o.flow_shadow.xy);
            #endif
        }

        // Data that needs to be sampled at the displaced position
        half seaLevelOffset = 0.0;
        o.seaLevelDerivs = 0.0;
        if (wt_smallerLod > 0.0001)
        {
            const float3 uv_slice_smallerLodDisp = WorldToUV(o.worldPos.xz, cascadeData0, _LD_SliceIndex);

            SampleSeaDepth(_LD_TexArray_SeaFloorDepth, uv_slice_smallerLodDisp, wt_smallerLod, o.lodAlpha_worldXZUndisplaced_oceanDepth.w, seaLevelOffset, cascadeData0, o.seaLevelDerivs);

            #if _SHADOWS_ON
                // The minimum sampling weight is lower than others to fix shallow water colour popping.
                if (wt_smallerLod > 0.001)
                {
                    SampleShadow(_LD_TexArray_Shadow, uv_slice_smallerLodDisp, wt_smallerLod, o.flow_shadow.zw);
                }
            #endif
        }
        if (wt_biggerLod > 0.0001)
        {
            const float3 uv_slice_biggerLodDisp = WorldToUV(o.worldPos.xz, cascadeData1, _LD_SliceIndex + 1);

            SampleSeaDepth(_LD_TexArray_SeaFloorDepth, uv_slice_biggerLodDisp, wt_biggerLod, o.lodAlpha_worldXZUndisplaced_oceanDepth.w, seaLevelOffset, cascadeData1, o.seaLevelDerivs);

            #if _SHADOWS_ON
                // The minimum sampling weight is lower than others to fix shallow water colour popping.
                if (wt_biggerLod > 0.001)
                {
                    SampleShadow(_LD_TexArray_Shadow, uv_slice_biggerLodDisp, wt_biggerLod, o.flow_shadow.zw);
                }
            #endif
        }

        o.worldPos.y += seaLevelOffset;


        // Move to world space
        //o.worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1.0));
        //
        // debug tinting to see which shape textures are used
        #if _DEBUGVISUALISE_SHAPESAMPLE
            #define TINT_COUNT (uint)7
            half3 tintCols[TINT_COUNT]; tintCols[0] = half3(1., 0., 0.); tintCols[1] = half3(1., 1., 0.); tintCols[2] = half3(1., 0., 1.); tintCols[3] = half3(0., 1., 1.); tintCols[4] = half3(0., 0., 1.); tintCols[5] = half3(1., 0., 1.); tintCols[6] = half3(.5, .5, 1.);
            o.debugtint = wt_smallerLod * tintCols[_LD_SliceIndex % TINT_COUNT] + wt_biggerLod * tintCols[(_LD_SliceIndex + 1) % TINT_COUNT];
        #endif

        // view-projection
        o.positionCS = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.));

        UNITY_TRANSFER_FOG(o, o.positionCS);

        // unfortunate hoop jumping - this is inputs for refraction. depending on whether HDR is on or off, the grabbed scene
        // colours may or may not come from the backbuffer, which means they may or may not be flipped in y. use these macros
        // to get the right results, every time.
        o.grabPos = ComputeGrabScreenPos(o.positionCS);
        o.screenPosXYW = ComputeScreenPos(o.positionCS).xyw;

    }
