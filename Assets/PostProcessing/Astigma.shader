Shader "VertexFragment/Astigma"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

            #pragma vertex VertMain
            #pragma fragment FragMain

            #include "UnityCG.cginc"
            #include "Assets/Resources/Shaders/Chunks/BibPit.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            sampler2D _CameraGBufferTexture2;
            sampler2D _OcclusionDepthMap;


            

            float2 blurAngle;
            float2 blurAngle2;

            struct VertData
            {
                float4 vertex : POSITION;
                float4 uv     : TEXCOORD0;
            };

            struct FragData
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            FragData VertMain(VertData input)
            {
                FragData output;

                output.vertex = float4(input.vertex.xy, 0.0, 1.0);
                output.texcoord = (input.vertex.xy + 1.0) * 0.5;

                // For Direct3D Build
                output.texcoord.y = 1.0 - output.texcoord.y;

                // For Open/WebGL build
                //output.texcoord.y = output.texcoord.y;

                return output;
                
            }

            float _Intensity;
            float _Scale;
            float _Cutoff;
            float _AspectRatio;
            float _NumSamples;
            float _NumDirections;
            float _Angle;

           // float4 _ScreenParams;


            float3 upDownSample( sampler2D tex, float2 uv, float2 offset, float2 texelSize )
            {
                float n = length(float2( sin( uv.x * 100+_Time.y * 100), sin(uv.y * 100 +_Time.y * 100)));
                n = 1;
            
                float3 totalCol = 0;
                for( int i = 1; i < int(_NumSamples); i++ ){

                    float multiplier = pow( 1-(float(i)/_NumSamples),2);// 1.0 / (float)(i+1);

                    //multiplier *= multiplier;
                    //multiplier *= 4;

                    float2 uvOffset = uv + offset  * (float)i * texelSize;
                    float3 col = tex2D(tex, uvOffset).rgb;
                    //totalCol += col * col * multiplier;
                    uvOffset = uv - offset  * (float)i * texelSize;
                    col = tex2D(tex, uvOffset).rgb;

                    if( length(col) > _Cutoff ){
                        totalCol += col * col  * multiplier * multiplier;
                    }

                }
                return totalCol;// * n;
            }


            float4 FragMain(FragData input) : SV_Target
            {
                float3 color = 0;
                color = tex2D(_MainTex, input.texcoord).rgb;


                /* float3 grit = tex2D(_GritTexture, input.texcoord).rgb;
                float3 grit2 = tex2D(_GritTexture2, input.texcoord.xy).rgb;
                float3 grit3 = tex2D(_GritTexture3, input.texcoord.xy).rgb;
                float3 grit4 = tex2D(_GritTexture4, input.texcoord.xy).rgb;

                //color *= 2 * grit;


                
                color =10* saturate(((upDownSample( _MainTex, input.texcoord, float2(0,1), .005 )*upDownSample( _MainTex, input.texcoord, float2(1,0), .005 )) - .5*color));*/
                //color = color * .5 + color *.5* grit2;
                // color *= float3(.8,.5,1);

                //color *= grit * float3(1,.8,.9) + grit2 *  float3(.9,.8,1);

                // color *= grit2;

                // color = length(color);

                for( int i = 0; i < int(_NumDirections); i++){
                    float angle = 6.28318530718 *(_Angle + (float)i / _NumDirections);
                    float2 dir = float2(cos(angle), sin(angle));
                    color += _Intensity * upDownSample( _MainTex, input.texcoord, dir, _ScreenParams.zw * _Scale );
                }

                /*  color += grit2 * tex2D(_AudioMap, float2(length(grit2) * .2,_Timeline)) * 1;
                color += grit * grit * grit * tex2D(_AudioMap, float2(length(grit) * .2,_Timeline)) * 1;
                color += grit3 * grit3 * grit3 * tex2D(_AudioMap, float2(length(grit3) * .2,_Timeline)) * 2;
                //color += grit4 * grit4 * grit4 * tex2D(_AudioMap, float2(length(grit4) * .2,_Timeline)) * 3;



                float3 gritSparkle = upDownSample( _GritTexture4, input.texcoord, float2(1,1), .001 );
                gritSparkle  += upDownSample( _GritTexture4, input.texcoord, float2(1,-1), .001 );

                //color = grit4;//ritSparkle;
                color += gritSparkle * gritSparkle * 1 * tex2D(_AudioMap, float2(length(gritSparkle*gritSparkle) * .1,_Timeline)) * 1; ;// * gritSparkle;

                */
                

                /* float3 sparkle = upDownSample( _MainTex, input.texcoord, float2(1,0), .005 );
                sparkle += upDownSample( _MainTex, input.texcoord, float2(0,-1), .005 );

                color.xyz = length(sparkle.xyz)* sparkle.xyz * 10;// - tex2D(_MainTex, input.texcoord).rgb) *tex2D(_MainTex, input.texcoord).rgb ;
                */
              //  color *= _Amount;


                color = saturate(color);


                // color = tex2D(_AudioMap, input.texcoord.xy);

                //color = sampleAudio(input.texcoord.x,0);

                //color = tex2D(_AudioMap, float2( input.texcoord.x,_Timeline)).rgb;

                return float4(color, 1.0);
            }

            ENDCG
        }
    }
}