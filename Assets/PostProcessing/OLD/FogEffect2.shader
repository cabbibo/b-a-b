Shader "PostProcessing/FogEffect2"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
    
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    
    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    float4x4 UNITY_MATRIX_M;
    //#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    //#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
    
    float3 _WorldSpaceCameraMainPos;
    matrix _InvCamProjMatrix;

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        float2 uv : TEXCOORD0;
        //UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        float3 viewVector : TEXCOORD1;
        float2 uv   : TEXCOORD2;   
    };


    float4x4 _InverseProjection;
    float4x4 _InverseView;

    float4x4 _InverseViewProjection;

    Varyings Vert(Attributes input)
    {
        Varyings output;
        // UNITY_SETUP_INSTANCE_ID(input);
        //  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        output.uv = input.uv;
        output.viewVector = mul( _InverseProjection, float4(input.uv * 2 - 1, 0, -1));
        output.viewVector = float3(input.uv * 2 - 1, 0);
        // output.viewVector = mul(UNITY_MATRIX_M, float4(output.viewVector,0));
        return output;
    }

    /*float LinearEyeDepth(float z)
    {
        return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
    }

    static const float maxFloat = 0;//3.402823466e+38;
    

    // Returns vector (dstToSphere, dstThroughSphere)
    // If ray origin is inside sphere, dstToSphere = 0
    // If ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
    float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir) {
        float3 offset = rayOrigin - sphereCentre;
        float a = dot(rayDir, rayDir);// if rayDir might not be normalized
        float b = 2 * dot(offset, rayDir);
        float c = dot(offset, offset) - sphereRadius * sphereRadius;
        float d = (b * b) - (4 * a * c); // Discriminant from quadratic formula

        // Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
        if (d > 0) {
            float s = sqrt(d);
            float dstToSphereNear = -b - s;
            float dstToSphereFar = -b + s;

            // Ignore intersections that occur behind the ray
            if (dstToSphereFar < 0) {
                dstToSphereFar = -1;
            }

            if (dstToSphereNear < 0) {
                dstToSphereNear = -1;   
            }

            dstToSphereNear = dstToSphereNear / (2 * a);
            dstToSphereFar = dstToSphereFar / (2 * a);

            return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);

        }
        // Ray did not intersect sphere
        return float2(-1, 0);
    }*/

    // List of properties to control your post process effect
    float _Intensity;
    //TEXTURE2D_X(_InputTexture);

    float3 planetCentre;
    float atmosphereRadius;
    float planetRadius;

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        // scenedepth
        /* float sceneDepthNonLinear = SampleCameraDepth(input.uv);
        float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(input.viewVector);

        // viewdirection
        PositionInputs posInput = GetPositionInput(input.positionCS.xy, _ScreenSize.zw, sceneDepth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float3 viewDirection = GetWorldSpaceNormalizeViewDir(posInput.positionWS);//posInput.positionWS);
        
        // rayorigin
        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float  deviceDepth = LoadCameraDepth(positionSS);
        float2 positionNDC = positionSS * _ScreenSize.zw + (0.5 * _ScreenSize.zw);
        float3 positionWS = ComputeWorldSpacePosition(positionNDC, deviceDepth, UNITY_MATRIX_I_VP);
        
        // raysphere                                  
        float3 rayOrigin = GetAbsolutePositionWS(positionWS);
        float3 rayDir = normalize(viewDirection);

        float2 hitInfo = raySphere(planetCentre, atmosphereRadius, rayOrigin, rayDir);
        float dstToAtmosphere = hitInfo.x;
        float dstThroughAtmosphere = min(hitInfo.y, sceneDepth - dstToAtmosphere);
        
        // add to color
        float4 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS);                    
        return outColor + (dstThroughAtmosphere / (atmosphereRadius * 2));*/



        return float4(input.viewVector,1);
    }
    

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Atmosphere"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
            #pragma fragment CustomPostProcess
            #pragma vertex Vert
            
            ENDHLSL
        }
    }
    Fallback Off
}