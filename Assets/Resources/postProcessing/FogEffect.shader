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


    float _FogMultiplier;
    float _FogHeightMultiplier;
    float _FogHeightPower;
    float _FogDensityAtFar;
    float _FogDensityAtNear;
    float _FogStepSize;
    float _MaxFogTotal;
    float4 _FogColorNear;
    float4 _FogColorFar;
    float4 _FogColorDistant;
    float _OceanHeight;
    #define _FogSamples 100
    


    float getTerrainHeight(float3 p){
        float2 samplePosition = p.xz;
        float2 uv = (samplePosition + _MapSize.xz/2) / _MapSize.xz;

        float h = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv) * _MapSize.y * 2;

        return h;

    }


    float hash21(float2 p) {
        p = 50.0 * frac(p * 0.3183099 + float2(0.71, 0.113));
        return frac(p.x * p.y * (p.x + p.y));
    }
    float4 Frag(VaryingsDefault v) : SV_Target
    {

        float3 ro = _WorldSpaceCameraPos;
        
        float3 rd = GetRayDirection(v.texcoord);

        float2 uvR = v.texcoord;
        float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvR);
        float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uvR).r;

        float distance = LinearEyeDepth(depth);

        float totalFog = 0;
        float4 totalFogColor=0;

        
        float3 viewVector = mul( _InverseProjection , float4(v.texcoord.x * 2 - 1 , v.texcoord.y * 2 - 1, 0, 1));
        viewVector = mul( _InverseView, viewVector).xyz;



        

        float offset = _FogStepSize * 1 * hash21( v.texcoord + _Time.y );
        bool hasBroke = false;
        for( int i = 0; i < _FogSamples; i++ ){

            float ni = float(i)/ float(_FogSamples);

            float dist = _FogStepSize * float(i) + offset;

            // if its farther than scene depth, break
            if( dist > distance ){
                hasBroke = true;
                break;
            }

            float3 p = ro + viewVector *dist;



            // sample the height map, to see how far away we are from the ground
            // the closer we are to the ground, the more fog we should apply
            float height = getTerrainHeight(p);// SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uvR).r;1

            height = max(height, _OceanHeight);
            float d = p.y - height;

            
            if( d < 0){
                hasBroke = true;

                break;
            }

            float fogValue = clamp( 1/(pow( d, _FogHeightPower) * _FogHeightMultiplier),0,1000) * lerp(_FogDensityAtNear, _FogDensityAtFar, ni);

            totalFog += fogValue;
            
            totalFogColor += lerp( _FogColorNear, _FogColorFar,ni ) * fogValue;
            
            if( totalFog > _MaxFogTotal){
                totalFog = _MaxFogTotal;
                totalFogColor = _FogColorFar;
                hasBroke = true;
                break;
            }

            // totalFog += 1/(d*100);
            
        }

        

        totalFog /= float(_FogSamples);
        totalFog *= _FogMultiplier;
        totalFogColor /= float(_FogSamples);
        totalFogColor *= _FogMultiplier;
        
        if( hasBroke == false){
            totalFog = _MaxFogTotal;
            totalFogColor = _FogColorDistant;
        }   

        // totalFog = distance/ 10000;
        



        //totalFog = normalize(rd);;
        //totalFog = normalize(viewVector);

        //color *= _Intensity;

        

        //color.rgb = lerp( color.rgb , float3(1,1,1), saturate(10*pow((LinearEyeDepth(depth)/20000),1)));
        //color.rgb =   color.rgb + totalFog/1000;

        // color.rgb = lerp( color.rgb , color.rgb * _FogColor.xyz,totalFog/_MaxFogTotal);//float3(1,.1,.5);//getTerrainHeight( float3(v.texcoord.x,0,v.texcoord.y) * 4096);

        color.rgb = totalFogColor.rgb + color.rgb ;;


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