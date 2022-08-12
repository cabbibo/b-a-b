
Shader "Volumetric/CloudsOnShape"
{
    
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorMap ("Color Map", 2D) = "white" {}
        _HueStart("_HueStart",float) = 1
        _HueSize("_HueSize",float) = 0
        _MaxViewLength("_MaxViewLength",float) = 0
        _FogColor("_FogColor",Color) =  (0, 0, 0, 1)
        _SaturationStart("_SaturationStart",float) = 1
        _SaturationSize("_SaturationSize",float) =1
        _LightnessStart("_LightnessStart",float) = 1
        _LightnessSize("_LightnessSize",float) = 0

    }
    SubShader
    {
        // inside SubShader
Tags { "Queue"="Transparent+1" "RenderType"="Transparent" "IgnoreProjector"="True" }
        // No culling or depth
        
        //Blend One One
        Blend SrcAlpha OneMinusSrcAlpha 
        //ZWrite Off ZTest Always


           // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }


    CGINCLUDE


float4 _FogColor;
float _MaxViewLength;
      // Textures
            Texture3D<float4> NoiseTex;
            Texture3D<float4> DetailNoiseTex;
            Texture2D<float4> BlueNoise;
            
            SamplerState samplerNoiseTex;
            SamplerState samplerDetailNoiseTex;
            SamplerState samplerBlueNoise;

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            // Shape settings
            int3 mapSize;
            float densityMultiplier;
            float densityOffset;
            float scale;
            float detailNoiseScale;
            float detailNoiseWeight;
            float3 detailWeights;
            float4 shapeNoiseWeights;
            float4 phaseParams;

            // March settings
            int numStepsLight;
            float rayOffsetStrength;

            float3 boundsMin;
            float3 boundsMax;

            float3 shapeOffset;
            float3 detailOffset;

            // Light settings
            float lightAbsorptionTowardSun;
            float lightAbsorptionThroughCloud;
            float darknessThreshold;
            float4 _LightColor0;
            float4 colA;
            float4 colB;

            // Animation settings
            float timeScale;
            float baseSpeed;
            float detailSpeed;

            // Debug settings:
            int debugViewMode; // 0 = off; 1 = shape tex; 2 = detail tex; 3 = weathermap
            int debugGreyscale;
            int debugShowAllChannels;
            float debugNoiseSliceDepth;
            float4 debugChannelWeight;
            float debugTileAmount;
            float viewerSize;


            

            float3 _Scale;
            
            float remap(float v, float minOld, float maxOld, float minNew, float maxNew) {
                return minNew + (v-minOld) * (maxNew - minNew) / (maxOld-minOld);
            }

            float2 squareUV(float2 uv) {
                float width = _ScreenParams.x;
                float height =_ScreenParams.y;
                //float minDim = min(width, height);
                float scale = 1000;
                float x = uv.x * width;
                float y = uv.y * height;
                return float2 (x/scale, y/scale);
            }



            // Returns (dstToBox, dstInsideBox). If ray misses box, dstInsideBox will be zero
            float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir) {
                // Adapted from: http://jcgt.org/published/0007/03/04/
                float3 t0 = (boundsMin - rayOrigin) * invRaydir;
                float3 t1 = (boundsMax - rayOrigin) * invRaydir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                
                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(tmax.x, min(tmax.y, tmax.z));

                // CASE 1: ray intersects box from outside (0 <= dstA <= dstB)
                // dstA is dst to nearest intersection, dstB dst to far intersection

                // CASE 2: ray intersects box from inside (dstA < 0 < dstB)
                // dstA is the dst to intersection behind the ray, dstB is dst to forward intersection

                // CASE 3: ray misses box (dstA > dstB)

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }

            // Henyey-Greenstein
            float hg(float a, float g) {
                float g2 = g*g;
                return (1-g2) / (4*3.1415*pow(1+g2-2*g*(a), 1.5));
            }

            float phase(float a) {
                float blend = .5;
                float hgBlend = hg(a,phaseParams.x) * (1-blend) + hg(a,-phaseParams.y) * blend;
                return phaseParams.z + hgBlend*phaseParams.w;
            }

            float beer(float d) {
                float beer = exp(-d);
                return beer;
            }

            float remap01(float v, float low, float high) {
                return (v-low)/(high-low);
            }
float sdBox( float3 p, float3 b )
{
  float3 d = abs(p) - b;
  return min(max(d.x,max(d.y,d.z)),0.0) +
         length(max(d,0.0));
}
            float sampleDensity(float3 rayPos) {
                // Constants:
                const int mipLevel = 0;
                const float baseScale = 1;
                const float offsetSpeed = 1/1.0;

                // Calculate texture sample positions
                float time = _Time.x * timeScale;
                float3 size = 1;//boundsMax - boundsMin;
                float3 boundsCentre = 0;//(boundsMin+boundsMax) * .5;
                float3 uvw = (size * .5 + rayPos * 4 * _Scale) * scale ;// * baseScale * scale;
                float3 shapeSamplePos = uvw * .1;//+ shapeOffset * offsetSpeed + float3(time,time*0.1,time*0.2) * baseSpeed;

                // Calculate falloff at along x/z edges of the cloud container
                const float containerEdgeFadeDst = 1;
                float dstFromEdgeX = min(containerEdgeFadeDst, min(rayPos.x - -1, 1 - rayPos.x));
                float dstFromEdgeZ = min(containerEdgeFadeDst, min(rayPos.z - -1, 1 - rayPos.z));
                float edgeWeight = min(dstFromEdgeZ,dstFromEdgeX)/containerEdgeFadeDst;
                

                edgeWeight = sdBox( rayPos , .3 );
                // Calculate height gradient from weather map
                //float2 weatherUV = (size.xz * .5 + (rayPos.xz-boundsCentre.xz)) / max(size.x,size.z);
                //float weatherMap = WeatherMap.SampleLevel(samplerWeatherMap, weatherUV, mipLevel).x;
                float gMin = .2;
                float gMax = .7;
                float heightPercent = (rayPos.y - boundsMin.y) / size.y;
                float heightGradient = saturate(remap(heightPercent, 0.0, gMin, 0, 1)) * saturate(remap(heightPercent, 1, gMax, 0, 1));
                heightGradient *= edgeWeight;
                heightGradient = 1;//clamp(edgeWeight,1,1);//1.4;saturate(-edgeWeight+.5);

                // Calculate base shape density
                float4 shapeNoise = NoiseTex.SampleLevel(samplerNoiseTex, shapeSamplePos, mipLevel);
                float4 normalizedShapeWeights = shapeNoiseWeights / dot(shapeNoiseWeights, 1);
                float shapeFBM = dot(shapeNoise, normalizedShapeWeights) * saturate(1-4*sdBox(rayPos,.4));// * heightGradient;
                float baseShapeDensity = shapeFBM + densityOffset * .01;

                // Save sampling from detail tex if shape density <= 0
                if (baseShapeDensity > 0) {
                    
                    // Sample detail noise
                    float3 detailSamplePos = uvw*detailNoiseScale + detailOffset * offsetSpeed + float3(time*.4,-time,time*0.1)*detailSpeed;
                    float4 detailNoise = DetailNoiseTex.SampleLevel(samplerDetailNoiseTex, detailSamplePos, mipLevel);
                    float3 normalizedDetailWeights = detailWeights / dot(detailWeights, 1);
                    float detailFBM = dot(detailNoise, normalizedDetailWeights);

                    // Subtract detail noise from base shape (weighted by inverse density so that edges get eroded more than centre)
                    float oneMinusShape = 1 - shapeFBM;
                    float detailErodeWeight = oneMinusShape * oneMinusShape * oneMinusShape;
                    float cloudDensity = baseShapeDensity - (1-detailFBM) * detailErodeWeight * detailNoiseWeight;
    
                    return cloudDensity * densityMultiplier;
                }


                return 0;
            }

            // Calculate proportion of light that reaches the given point from the lightsource
            float lightmarch(float3 position) {
                float3 dirToLight = normalize(mul(unity_WorldToObject , float4(_WorldSpaceLightPos0.xyz,0)).xyz);
                float dstInsideBox = rayBoxDst(-1, 1, position, 1/dirToLight).y;
                
                float stepSize =.1;// dstInsideBox/numStepsLight;
                float totalDensity = 0;

                for (int step = 0; step < numStepsLight; step ++) {
                    position += dirToLight * stepSize;
                    totalDensity += max(0, sampleDensity(position) * stepSize);
                }

                float transmittance = exp(-totalDensity * lightAbsorptionTowardSun );
                return darknessThreshold + transmittance * (1-darknessThreshold);
            }

           
            sampler2D _ColorMap;
            sampler2D _BackgroundTexture;
            

        float _HueStart; 
        float _HueSize; 
        float _SaturationStart;
        float _SaturationSize;
        float _LightnessStart; 
        float _LightnessSize;
 float3 hsv(float h, float s, float v)
{
  return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
    h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
}      

ENDCG


























        Pass
        {

            Cull Back 
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "CloudDebug.cginc"

            // vertex input: position, UV
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 grabPos : TEXCOORD3;
                float worldLength : TEXCOORD4;
            };
            
            v2f vert (appdata v) {
                v2f output;
                output.pos = UnityObjectToClipPos(v.vertex);
                output.uv = v.uv;
                output.worldPos = v.vertex;//mul(unity_ObjectToWorld, v.vertex).xyz;
                
                output.grabPos = ComputeGrabScreenPos(output.pos);

                float l = length(mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos);

                output.worldLength = (l*l) / (_MaxViewLength*_MaxViewLength);
                
                //float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                output.viewVector = mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1)).xyz;// - v.vertex.xyz);
                return output;
            }


            float4 frag (v2f i) : SV_Target
            {
              
                // Create ray
                float3 rayPos = _WorldSpaceCameraPos;
                float viewLength = length(i.viewVector- i.worldPos);
                float3 rayDir = -normalize(i.viewVector - i.worldPos);// / viewLength;
                
            
                // point of intersection with the cloud container
                float3 entryPoint = i.worldPos;//rayPos + rayDir * dstToBox;

                // random starting offset (makes low-res results noisy rather than jagged/glitchy, which is nicer)
                float randomOffset = BlueNoise.SampleLevel(samplerBlueNoise, squareUV(i.grabPos.xy*1), 0);
                randomOffset *= rayOffsetStrength;
                
                // Phase function makes clouds brighter around sun
                float cosAngle = dot(-rayDir, _WorldSpaceLightPos0.xyz);
                float phaseVal = phase(cosAngle);


                float2 rayToContainerInfo = rayBoxDst(-1, 1, rayPos, 1/rayDir);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;

                float dstTravelled =  randomOffset;
                float dstLimit = 1;//dstInsideBox;//min(0-dstToBox, dstInsideBox);
                
                
                
                const float stepSize = .07;

                // March through volume:
                float transmittance = 1;
                float3 lightEnergy = 0;

                while (dstTravelled < dstLimit) {
                    rayPos = entryPoint + rayDir * dstTravelled;
                    float density = sampleDensity(rayPos);
                    
                    if (density > 0) {
                        float lightTransmittance = lightmarch(rayPos);
                        lightEnergy += density * stepSize * transmittance * lightTransmittance * phaseVal;
                        transmittance *= exp(-density * stepSize * lightAbsorptionThroughCloud);
                    
                        // Exit early if T is close to zero as further samples won't affect the result much
                        if (transmittance < 0.01) {
                            break;
                        }
                    }
                    dstTravelled += stepSize;
                }

                // Add clouds to background
                float3 backgroundCol  = tex2Dproj(_BackgroundTexture, i.grabPos).xyz;//tex2D(_MainTex,i.uv);
                lightEnergy = 1-lightEnergy;
                float3 cloudCol = hsv( _HueStart + _HueSize * lightEnergy , _SaturationStart + _SaturationSize * lightEnergy,_LightnessStart + _LightnessSize * (1-lightEnergy));// tex2D( _ColorMap , clamp( 1-lightEnergy * 1,0,1) )  ;//lerp( float3(0,0,1) * 0, float3(1,0,0) * 1 , lightEnergy);//* _LightColor0;//hsv(lightEnergy * 2,1,lightEnergy);// * _LightColor0;
                float3 col =  cloudCol;// * _LightColor0 ;//backgroundCol * transmittance + cloudCol;
                if( transmittance > .99 ){
                   // discard;
                }

/*
                if( abs(i.worldPos.x) > .49 && abs(i.worldPos.x) < .49999 ){
                    col = 1;
                    transmittance = 0;
                }

                 if( abs(i.worldPos.y) > .49 && abs(i.worldPos.y) < .49999 ){
                    col = 1;
                    transmittance = 0;
                }

                 if( abs(i.worldPos.z) > .49 && abs(i.worldPos.z) < .49999 ){
                    col = 1;
                    transmittance = 0;
                }*/

                
               // col = lerp( col , _FogColor , i.worldLength);// / (_MaxViewLength*_MaxViewLength) );
                //col = lightEnergy;//NoiseTex.SampleLevel(samplerNoiseTex, shapeSamplePos, mipLevel);;
                //transmittance = 0;
                return float4(col,1-transmittance);

            }

            ENDCG
        }


         Pass
        {

            Cull Front
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "CloudDebug.cginc"

            // vertex input: position, UV
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 grabPos : TEXCOORD3;
            };
            
            v2f vert (appdata v) {
                v2f output;
                output.pos = UnityObjectToClipPos(v.vertex);
                output.uv = v.uv;
                output.worldPos = v.vertex;//mul(unity_ObjectToWorld, v.vertex).xyz;
                
                output.grabPos = ComputeGrabScreenPos(output.pos);
                
                //float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                output.viewVector = mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1)).xyz;// - v.vertex.xyz);
                return output;
            }


            float4 frag (v2f i) : SV_Target
            {
              
                // Create ray
                float3 rayPos = _WorldSpaceCameraPos;
                
                float viewLength = length(i.viewVector- i.worldPos);
                float3 rayDir = -normalize(i.viewVector - i.worldPos);// / viewLength;
                
            
                // point of intersection with the cloud container
                float3 entryPoint = i.viewVector;//rayPos + rayDir * dstToBox;

                // random starting offset (makes low-res results noisy rather than jagged/glitchy, which is nicer)
                float randomOffset = BlueNoise.SampleLevel(samplerBlueNoise, squareUV(i.grabPos.xy*1), 0);
                randomOffset *= rayOffsetStrength;
                
                // Phase function makes clouds brighter around sun
                float cosAngle = dot(-rayDir, _WorldSpaceLightPos0.xyz);
                float phaseVal = phase(cosAngle);


                float2 rayToContainerInfo = rayBoxDst(-1, 1, rayPos, 1/rayDir);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;

                float dstTravelled =  randomOffset;
                float dstLimit = 1;//dstInsideBox;//min(0-dstToBox, dstInsideBox);
                
                
                
                const float stepSize = .07;

                // March through volume:
                float transmittance = 1;
                float3 lightEnergy = 0;

                while (dstTravelled < dstLimit) {
                    rayPos = entryPoint + rayDir * dstTravelled;
                    float density = sampleDensity(rayPos);
                    
                    if (density > 0) {
                        float lightTransmittance = lightmarch(rayPos);
                        lightEnergy += density * stepSize * transmittance * lightTransmittance * phaseVal;
                        transmittance *= exp(-density * stepSize * lightAbsorptionThroughCloud);
                    
                        // Exit early if T is close to zero as further samples won't affect the result much
                        if (transmittance < 0.01) {
                            break;
                        }
                    }
                    dstTravelled += stepSize;
                }

                // Add clouds to background
                float3 backgroundCol  = tex2Dproj(_BackgroundTexture, i.grabPos).xyz;//tex2D(_MainTex,i.uv);
                lightEnergy = 1-lightEnergy;
                float3 cloudCol = hsv( _HueStart + _HueSize * lightEnergy , _SaturationStart + _SaturationSize * lightEnergy,_LightnessStart + _LightnessSize * (1-lightEnergy));// tex2D( _ColorMap , clamp( 1-lightEnergy * 1,0,1) )  ;//lerp( float3(0,0,1) * 0, float3(1,0,0) * 1 , lightEnergy);//* _LightColor0;//hsv(lightEnergy * 2,1,lightEnergy);// * _LightColor0;
               
                float3 col =  cloudCol  ;//backgroundCol * transmittance + cloudCol;
                if( transmittance > .99 ){
                   // discard;
                }

                /*if( abs(i.worldPos.x) > .49 && abs(i.worldPos.x) < .49999 ){
                    col = 1;
                    transmittance = 0;
                }

                 if( abs(i.worldPos.y) > .49 && abs(i.worldPos.y) < .49999 ){
                    col = 1;
                    transmittance = 0;
                }

                 if( abs(i.worldPos.z) > .49 && abs(i.worldPos.z) < .49999 ){
                    col = 1;
                    transmittance = 0;
                }*/


                //col = lerp( col , _FogColor , (viewLength * viewLength) / (_MaxViewLength*_MaxViewLength) );

                return float4(col,1-transmittance);
                //return float4(col,1-transmittance);

            }

            ENDCG
        }
    }
}