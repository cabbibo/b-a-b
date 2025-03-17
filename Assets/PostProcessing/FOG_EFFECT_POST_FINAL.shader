Shader "PostProcessing/FogEffect"
{



    CGINCLUDE


    sampler2D _MainTex;
    sampler2D _DepthTex;
    sampler2D _CameraDepthTexture;
    sampler2D _HeightMap;

    

//#include "UnityLightingCommon.cginc"



//samplerCube _Cubemap;
float _Intensity;

float _StartDistance;
float _EndDistance;

float4x4 _InverseProjection;
float4x4 _InverseView;

float4x4 _InverseViewProjection;

float4 _LightColor0;

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

float3 _MapSize;
float3 _MapOffset;


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

float _LightColorImportance;
#define _FogSamples 40

const float e = 2.7182818284590452353602874713527;

float staticNoise(float2 texCoord)
{
    //float G = e + (_Time.y * 0.00001+1000);
    float G = e + (  0.00001+10);
    float2 r = (G * sin(G * texCoord.xy));
    return (frac(r.x * r.y * (1.0 + texCoord.x)));
}



// TODO NEED OFFSET!
float getTerrainHeight(float3 p){
    float2 samplePosition = p.xz- _MapOffset.xz;
    float2 uv = (samplePosition + _MapSize.xz/2) / _MapSize.xz;

    float h = tex2D(_HeightMap,  uv) * _MapSize.y * 2;

    return h;

}


float hash21(float2 p) {
    p = 50.0 * frac(p * 0.3183099 + float2(0.71, 0.113));
    return frac(p.x * p.y * (p.x + p.y));
}



float LinearEyeDepth(float z)
{
    return rcp(_ZBufferParams.z * z + _ZBufferParams.w);
}



      #include "Assets/Resources/Shaders/Chunks/SunShadows.cginc"
      #include "Assets/Resources/Shaders/Chunks/snoise.cginc"
      #include "Assets/Resources/Shaders/Chunks/noise.cginc"

    struct AttributesDefault
    {
        float3 vertex : POSITION;
    };
    
    struct VaryingsDefault
    {
        float4 vertex : SV_POSITION;
        float2 texcoord : TEXCOORD0;
        float4 sceenPos : TEXCOORD1;

    };

    // Vertex manipulation
float2 TransformTriangleVertexToUV(float2 vertex)
{
    float2 uv = (vertex + 1.0) * 0.5;
    return uv;
}


    VaryingsDefault VertDefault(AttributesDefault v)
{
    VaryingsDefault o;
    o.vertex = float4(v.vertex.xy, 0.0, 1.0);
    o.texcoord = (v.vertex.xy + 1.0) * 0.5;

#if UNITY_UV_STARTS_AT_TOP
    o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif


    return o;
}


    float4 Frag(VaryingsDefault v) : SV_Target
    {

        float3 ro = _WorldSpaceCameraPos;
        
        float3 rd = GetRayDirection(v.texcoord);

        float2 uvR = v.texcoord;
        float4 color = tex2D(_MainTex,  uvR);
        float4 bgCol = color;
        float depth = tex2D(_CameraDepthTexture, uvR).r;

        float3 viewVector = mul( _InverseProjection , float4(v.texcoord.x * 2 - 1 , v.texcoord.y * 2 - 1, 0, 1));
        viewVector = mul( _InverseView, viewVector).xyz;

        float distance = LinearEyeDepth(depth);

        float totalFog = 0;
        float4 totalFogColor=0;





        float4 worldPos = float4(ro + rd * distance, 1);
        float shadowAttenuation = GetSunShadowsAttenuation_PCF5x5(worldPos, 1, -1).x;	

        float offset = _FogStepSize * staticNoise( v.texcoord + _Time.y%1 );

     ///   float offsetN = staticNoise( v.texcoord + _Time.y%1 );

       //  ro -= _FogStepSize *normalize(viewVector) * offset;
        bool hasBroke = false;
        float offsetN = staticNoise( v.texcoord + _Time.y%1 );
        float currentDistance=0;
        float currentStepSize=0;

        float oSVal = 1;

        for( int i = 0; i < _FogSamples; i++ ){

            float ni = float(i)/ float(_FogSamples);
            offsetN = staticNoise( v.texcoord + ((floor(_Time.y*20)/20) * .01 %.1) + 100 + float(i) * .1); // different noise each step?

            currentStepSize = _FogStepSize * (.2 + ni*2);
            currentDistance += currentStepSize - offsetN * currentStepSize; // alwa

            float dist = currentDistance;

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

            //float n = snoise(p * .1);

            fixed4 cascadeWeights = GET_CASCADE_WEIGHTS(p.xyz, 0);

            float sVal = unity_sampleShadowmap(GET_SHADOW_COORDINATES(float4(p.xyz, 1), cascadeWeights));
            float fogValue = sVal;//clamp( 1/(pow( d, _FogHeightPower) * _FogHeightMultiplier),0,1000) * lerp(_FogDensityAtNear, _FogDensityAtFar, ni);


            float deltaSVal = sVal - oSVal;

            oSVal = sVal;
           // fogValue *= n * .5 + .5;
          //  totalFog += fogValue;
            
            float fogDensity = lerp(_FogDensityAtNear, _FogDensityAtFar, ni);
          //  totalFogColor += lerp( _FogColorNear, _FogColorFar,ni ) * fogValue;

          //totalFog += sVal *(noise(p * .01)+1)* 1/(pow( d+3, _FogHeightPower));// GetSunShadowsAttenuation_PCF5x5(p,1,0);
        //  totalFog += ni*sVal * .1;//* _FogHeightMultiplier/(pow( d+2, _FogHeightPower))* lerp(_FogDensityAtNear, _FogDensityAtFar, ni);;// GetSunShadowsAttenuation_PCF5x5(p,1,0);
           float fogAmountThisStep = clamp(deltaSVal,0,1) * (3/(ni*2+1)) *_FogHeightMultiplier/(pow( d+2, _FogHeightPower));
              fogAmountThisStep +=   fogDensity*sVal  *_FogHeightMultiplier/(pow( d+2, _FogHeightPower));
                fogAmountThisStep += .1 * offsetN*_FogHeightMultiplier/(pow( d+2, _FogHeightPower));


            totalFog += fogAmountThisStep;//* fogDensity;
            totalFogColor += lerp( _FogColorNear, _FogColorFar,ni ) *fogDensity;







            if( totalFog > _MaxFogTotal){
                totalFog = _MaxFogTotal;
                totalFogColor = _FogColorFar;
                hasBroke = true;
                break;
            }

            // totalFog += 1/(d*100);
            
        }

        if( hasBroke == false ){
           // totalFog = _MaxFogTotal;
        }


        totalFogColor = lerp(totalFogColor, _LightColor0*3*totalFog/10, _LightColorImportance);

        color =bgCol+totalFogColor;// lerp(min(bgCol,totalFogColor) ,  bgCol+totalFogColor ,3*totalFog/10);//
      //  color = totalFogColor;
       // color = lerp(0, _LightColor0 ,3*totalFog/10);//
        

        /*if( hasBroke == false ){
            float height = getTerrainHeight(worldPos.xyz);// SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uvR).r;1

            height = max(height, _OceanHeight);
            float d = worldPos.y - height;
             color =lerp(min(bgCol,_LightColor0) ,  bgCol+_LightColor0 ,clamp(_FogHeightMultiplier/pow( d+2, _FogHeightPower),0,1000));//

             color = 
        }*/

       // maxStep = 


        color = saturate(color);
       // color = shadowAttenuation;

        fixed4 cascadeWeights = GET_CASCADE_WEIGHTS(worldPos.xyz, 0);

        //color =  unity_sampleShadowmap(GET_SHADOW_COORDINATES(float4(worldPos.xyz, 1), cascadeWeights));
        
        //color = color + float4(.4,0,0,1) *(1-shadowAttenuation);//distance/1000;
       //float4 color = float4(1,0,0,0);

        return color;
    }

    ENDCG






















    

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

            #pragma vertex VertDefault
            #pragma fragment Frag

            ENDCG
        }
    }













}