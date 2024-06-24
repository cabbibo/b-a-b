Shader "PostProcessing/FogEffect"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    TEXTURE2D_SAMPLER2D(_DepthTex, sampler_DepthTex);
    TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
    

    //samplerCube _Cubemap;
    float _Intensity;

    float _StartDistance;
    float _EndDistance;

    float4x4 _InverseProjection;
    float4x4 _InverseView;

    float4x4 _InverseViewProjection;

    /* transform uv -> NDC

    then create clipspacePos =[NDCx, NDCy, -1, 1]

    viewPos = invPorjectMtarix * clipspacePos
    worldPos = invViewMatrix * viewPos

    rayDir = worldPos-cameraPos*/

    float3 GetRayDirection(float2 texcoord)
    {
        // Convert texcoord to NDC
        float2 ndc = texcoord * 2.0 - 1.0;



        // Create a clip space position with z=1 and w=1 (for far plane)
        float4 clipSpacePos = float4(ndc, 1.0, 1.0);

        // Transform clip space position to camera space
        float4 viewPos = mul(_InverseProjection, clipSpacePos);
        viewPos /= viewPos.w;

        float3 worldPos = mul(_InverseView, viewPos).xyz;
        //   cameraSpaceDir.w = 0;


        // Normalize the direction
        float3 rayDirection =  normalize(worldPos.xyz - _WorldSpaceCameraPos);


        //rayDirection = normalize(mul(_InverseViewProjection, float4(ndc.x, ndc.y,-1, 0)).xyz);
        return rayDirection;
    }

    TEXTURE2D_SAMPLER2D(_HeightMap, sampler_HeightMap);
    float3 _MapSize;


    float getTerrainHeight(float3 p){
        float2 samplePosition = p.xz;
        float2 uv = (samplePosition + _MapSize.xz/2) / _MapSize.xz;

        float h = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv) * _MapSize.y * 2;

        return h;

    }



    float4 Frag(VaryingsDefault i) : SV_Target
    {

        float3 ro = _WorldSpaceCameraPos;
        float3 rd = GetRayDirection(i.texcoord);

        float2 uvR = i.texcoord;
        float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvR);
        float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uvR).r;

        float distance = LinearEyeDepth(depth);

        float3 totalFog = 0;

        for( int i = 0; i < 10; i++ ){
            float3 p = ro + rd * distance;

            // sample the height map, to see how far away we are from the ground
            // the closer we are to the ground, the more fog we should apply

            float height = getTerrainHeight(p);// SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uvR).r;1

            float d = p.y - height;

            totalFog += d;;
            
        }


        totalFog = normalize(rd);;

        //color *= _Intensity;

        

        //color.rgb = lerp( color.rgb , float3(1,1,1), saturate(10*pow((LinearEyeDepth(depth)/20000),1)));
        color.rgb =color.rgb + normalize(totalFog);

        return color;
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex VertDefault
            #pragma fragment Frag

            ENDHLSL
        }
    }
}