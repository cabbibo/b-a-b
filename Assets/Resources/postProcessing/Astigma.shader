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
            float4    _MainTex_TexelSize;
            sampler2D _CameraDepthTexture;
            sampler2D _CameraGBufferTexture2;
            sampler2D _OcclusionDepthMap;


            int       _UseTexture;
            sampler2D _BokehTex;

            float2 blurAngle;
            float2 blurAngle2;

            struct VertData
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct FragData
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            FragData VertMain( VertData input )
            {
                FragData output;

                output.vertex   = float4( input.vertex.xy , 0.0 , 1.0 );
                output.texcoord = ( input.vertex.xy + 1.0 ) * 0.5;

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

            static const int    kernelSampleCount           = 22;
            static const float2 kernel[ kernelSampleCount ] = {
                float2( 0 , 0 ),
                float2( 0.53333336 , 0 ),
                float2( 0.3325279 , 0.4169768 ),
                float2( -0.11867785 , 0.5199616 ),
                float2( -0.48051673 , 0.2314047 ),
                float2( -0.48051673 , -0.23140468 ),
                float2( -0.11867763 , -0.51996166 ),
                float2( 0.33252785 , -0.4169769 ),
                float2( 1 , 0 ),
                float2( 0.90096885 , 0.43388376 ),
                float2( 0.6234898 , 0.7818315 ),
                float2( 0.22252098 , 0.9749279 ),
                float2( -0.22252095 , 0.9749279 ),
                float2( -0.62349 , 0.7818314 ),
                float2( -0.90096885 , 0.43388382 ),
                float2( -1 , 0 ),
                float2( -0.90096885 , -0.43388376 ),
                float2( -0.6234896 , -0.7818316 ),
                float2( -0.22252055 , -0.974928 ),
                float2( 0.2225215 , -0.9749278 ),
                float2( 0.6234897 , -0.7818316 ),
                float2( 0.90096885 , -0.43388376 ),
            };


            float3 upDownSample( sampler2D tex , float2 uv , float2 offset , float2 texelSize )
            {
                float n = length( float2( sin( uv.x * 100 + _Time.y * 100 ) , sin( uv.y * 100 + _Time.y * 100 ) ) );
                n       = 1;

                float3 totalCol = 0;
                for ( int i = 1; i < int( _NumSamples ); i++ )
                {

                    float multiplier = pow( 1 - ( float( i ) / _NumSamples ) , 2 ); // 1.0 / (float)(i+1);

                    //multiplier *= multiplier;
                    //multiplier *= 4;

                    float2 uvOffset = uv + offset * (float)i * texelSize;
                    float3 col      = tex2D( tex , uvOffset ).rgb;
                    //totalCol += col * col * multiplier;
                    uvOffset = uv - offset * (float)i * texelSize;
                    col      = tex2D( tex , uvOffset ).rgb;


                    if ( length( col ) > _Cutoff )
                    {
                        totalCol += col * col * multiplier * multiplier;
                    }

                }
                return totalCol; // * n;
            }


            float4 _AstigmaColor;

            float4 FragMain( FragData input ) : SV_Target
            {
                float3 color = 0;
                color        = tex2D( _MainTex , input.texcoord ).rgb;


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

                if ( _UseTexture == 0 )
                {

                    for ( int i = 0; i < int( _NumDirections ); i++ )
                    {
                        float  angle = 6.28318530718 * ( _Angle + (float)i / _NumDirections );
                        float2 dir   = float2( cos( angle ) , sin( angle ) );
                        color += _Intensity * upDownSample( _MainTex , input.texcoord , dir , _ScreenParams.zw * _Scale ) * _AstigmaColor;
                    }
                }
                else
                {
                    /*for ( int s = 0; s < _NumDirections; ++s )
                    {
                        for ( int i = 0; i < int( _NumSamples ); i++ )
                        {

                            float  r      = float( i ) / float( _NumSamples );
                            float  a      = s * 6.283185 / float( _NumDirections );
                            float2 offset = float2( cos( a ) , sin( a ) ) * _Scale * r * _MainTex_TexelSize.xy;
                            float4 c      = tex2D( _MainTex , input.texcoord + offset );
                            float2 bUv    = offset; // * 0.5 + 0.5;
                            float4 mask   = tex2D( _BokehTex , bUv );
                            if ( length( c ) > _Cutoff )
                            {
                                color += length( c ) * mask.a / ( 1 + 4 * r );
                            }
                        }
                    }*/
                    float2 uv = input.texcoord;
                    //  int    amount = 5;
                    color.rgb = 0;
                    // color.a   = 1;


                    ///float3 centerPixel = tex2D( _MainTex , uv ).rgb;

                    for ( int k = 0; k < kernelSampleCount; k++ )
                    {
                        float2 o = kernel[ k ];

                        float2 baseUV = o;
                        baseUV *= .5;
                        baseUV += .5;

                        o *= _ScreenParams.zw * _Scale;
                        float3 col = tex2D( _MainTex , uv + o ).rgb;
                        // o *= _MainTex_TexelSize.xy * 8;
                        if ( length( col ) > _Cutoff )
                        {
                            color.xyz += _AstigmaColor * col * ( 1 - tex2D( _BokehTex , baseUV ).r );
                        }

                    }
                    // color.rgb *= 1.0 / kernelSampleCount;


                    /*
                                        for ( int u = -amount; u <= amount; u++ )
                                        {
                                            for ( int v = -amount; v <= amount; v++ )
                                            {
                    
                    
                                                float2 miniUV = float2( u , v );
                    
                    
                                                float2 baseUV = miniUV / float( amount );
                                                baseUV *= .5;
                                                baseUV += .5;
                    
                                                miniUV *= 2;
                    
                                                float2 o = miniUV * _ScreenParams.zw;
                    
                                                float3 col = tex2D( _MainTex , uv + o ).rgb;
                                                if ( length( col ) > 1 )
                                                {
                                                    color.rgb += tex2D( _MainTex , uv + o ).rgb * ( tex2D( _BokehTex , baseUV ).r );
                                                }
                    
                                            }
                                        }
                    
                                        color.rgb *= 1.0 / ( 1 + amount * amount );
                    */
                    //  color.rgb += centerPixel;


                    /*
                    float2 uv = input.texcoord;


                    color             = tex2D( _MainTex , uv );
                    float  brightness = dot( color.rgb , float3( 0.299 , 0.587 , 0.114 ) );
                    float4 bokeh      = tex2D( _BokehTex , uv );
                    color             = lerp( color , bokeh , brightness * _Intensity );

                    */

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


                color = saturate( color );




                // color = tex2D(_AudioMap, input.texcoord.xy);

                //color = sampleAudio(input.texcoord.x,0);

                //color = tex2D(_AudioMap, float2( input.texcoord.x,_Timeline)).rgb;

                return float4( color , 1.0 );
            }
            ENDCG
        }
    }
}