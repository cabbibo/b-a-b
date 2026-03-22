Shader "Post/LensFlare"
{

    Properties
    {
        _DogMultiplier ("Dog Multiplier", Float) = 20
        _FlareMultiplier ("Flare Multiplier", Float) = 20
        _GhostMultiplier ("Ghost Multiplier", Float) = 20
        _HexTexture("Hex",2D) = "white"{}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
            Name "FullscreenPass"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            TEXTURE2D (_BlitTexture);
            SAMPLER (sampler_BlitTexture);
            float4 _BlitTexture_TexelSize;


            TEXTURE2D (_HexTexture);
            SAMPLER (sampler_HexTexture);
            float4 _HexTexture_TexelSize;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float3 _SunDirection;
            float3 _SunPosition;


            Varyings Vert( Attributes v )
            {
                float2   uv = float2( ( v.vertexID << 1 ) & 2 , v.vertexID & 2 );
                Varyings o;
                o.uv = float2( uv.x , 1 - uv.y );

                o.positionCS = float4( uv * 2.0 - 1.0 , 0 , 1 );
                return o;
            }

            float _GhostMultiplier;
            float _FlareMultiplier;
            float _DogMultiplier;

            #include "Assets/Resources/Shaders/Chunks/zucconi.cginc"
            #include "Assets/Resources/Shaders/Chunks/noise.cginc"

            float3 lensflare( float2 uv , float2 pos )
            {
                float2 main = uv - pos;
                float2 uvd  = uv * ( length( uv ) );

                float ang  = atan2( main.x , main.y );
                float dist = length( main );
                dist       = pow( dist , .1 );
                float n    = noise( float3( ang * 16.0 , dist * 32.0 , 0 ) );

                float f0 = 1.0 / ( length( uv - pos ) * 16.0 + 1.0 );

                f0 = f0 + f0 * ( sin( noise( sin( ang * 2. + pos.x ) * 4.0 - cos( ang * 3. + pos.y ) ) * 16. ) * .1 + dist * .1 + .8 );

                float f1 = max( 0.01 - pow( length( uv + 1.2 * pos ) , 1.9 ) , .0 ) * 7.0;

                float f2  = max( 1.0 / ( 1.0 + 32.0 * pow( length( uvd + 0.8 * pos ) , 2.0 ) ) , .0 ) * 00.25;
                float f22 = max( 1.0 / ( 1.0 + 32.0 * pow( length( uvd + 0.85 * pos ) , 2.0 ) ) , .0 ) * 00.23;
                float f23 = max( 1.0 / ( 1.0 + 32.0 * pow( length( uvd + 0.9 * pos ) , 2.0 ) ) , .0 ) * 00.21;

                float2 uvx = lerp( uv , uvd , -0.5 );

                float f4  = max( 0.01 - pow( length( uvx + 0.4 * pos ) , 2.4 ) , .0 ) * 6.0;
                float f42 = max( 0.01 - pow( length( uvx + 0.45 * pos ) , 2.4 ) , .0 ) * 5.0;
                float f43 = max( 0.01 - pow( length( uvx + 0.5 * pos ) , 2.4 ) , .0 ) * 3.0;

                uvx = lerp( uv , uvd , -.4 );

                float f5  = max( 0.01 - pow( length( uvx + 0.2 * pos ) , 5.5 ) , .0 ) * 2.0;
                float f52 = max( 0.01 - pow( length( uvx + 0.4 * pos ) , 5.5 ) , .0 ) * 2.0;
                float f53 = max( 0.01 - pow( length( uvx + 0.6 * pos ) , 5.5 ) , .0 ) * 2.0;

                uvx = lerp( uv , uvd , -0.5 );

                float f6  = max( 0.01 - pow( length( uvx - 0.3 * pos ) , 1.6 ) , .0 ) * 6.0;
                float f62 = max( 0.01 - pow( length( uvx - 0.325 * pos ) , 1.6 ) , .0 ) * 3.0;
                float f63 = max( 0.01 - pow( length( uvx - 0.35 * pos ) , 1.6 ) , .0 ) * 5.0;

                float3 c = 0;;

                c.r += f2 + f4 + f5 + f6;
                c.g += f22 + f42 + f52 + f62;
                c.b += f23 + f43 + f53 + f63;
                c = c * 1.3 - length( uvd ) * .02;
                c *= c * _GhostMultiplier;
                c += f0 * f0 * _FlareMultiplier;

                return c;
            }


            float2 brightness( float2 uv , float tightness , float radius )
            {





                float val = 0;



                val = ( .01 / ( .001 + abs( uv.y ) ) + .01 / ( .001 + abs( uv.x ) ) ) * .001 / ( .0001 + pow( ( 1. * ( length( uv ) - radius ) ) , 2. ) ) - 1. * pow( length( uv ) , 2. );

                float outwards = clamp( tightness * ( length( uv ) - radius * .9 ) , 0. , 1. );
                val *= outwards; //clamp(10. * (length(uv)-.3),0.,-1.);


                return float2( val , outwards );


            }


            float3 sunDog( float2 uv )
            {


                float tightness = 100.;
                tightness *= abs( uv.y );
                tightness *= pow( abs( uv.x ) , .4 );
                float fullVal = length( uv ) * tightness - tightness * .3;

                float midBright = ( .5 - abs( fullVal - .01 ) ) * 2.;



                float3 col = 0;
                // col.x = length( uv) * 2. - .3;

                // col.y = clamp(col.x +1.,0.,1.);


                //col.x = midBright;// * midBright;
                float val = midBright; //* fullVal * 10.;

                float secondTight = 2.0;
                val *= length( uv ) * secondTight - secondTight * .2;
                val /= 5. * pow( length( uv ) , 2. );

                //col.y = fullVal;

                col = zucconi( clamp( 1.2 - 1. * abs( fullVal * 2 + .5 ) + .2 , 0 , .8 ) ) * val * 3.;

                //col = 

                if ( fullVal <= 0. || fullVal >= 1. )
                {
                    //col = vec3(0.);
                }

                return col;




            }

            // #include "Assets/Resources/Shaders/Chunks/de"


            float4 Frag( Varyings v ) : SV_Target
            {


                float4 color    = 0;
                float3 camPos   = _WorldSpaceCameraPos;
                float3 lightPos = camPos - _SunDirection * 1000;


                float2 fUV = v.uv - .5;
                fUV *= 2;
                fUV.x *= _ScreenParams.x / _ScreenParams.y;



                float4 col = SAMPLE_TEXTURE2D_X( _BlitTexture , sampler_BlitTexture , v.uv );

                float4 clipPos   = TransformWorldToHClip( _SunPosition );
                float4 screenPos = ComputeScreenPos( clipPos );
                float2 sunUV     = ( screenPos.xy / screenPos.w - 0.5 ) * 2.0;


                sunUV.y = -sunUV.y;
                #if UNITY_UV_STARTS_AT_TOP
sunUV.y = 1.0 - sunUV.y;
                #endif
                bool isBehind = clipPos.w <= 0.0;

                float3 lens = .4 * lensflare( fUV , sunUV );

                if ( !isBehind )
                {
                    col.rgb += max( lens , 0 );
                }

                float2 localSunUV = fUV - sunUV;

                float match = saturate( dot( normalize( localSunUV ) , normalize( fUV ) ) );

                match *= match;



                float ring = length( localSunUV * pow( abs( localSunUV.y ) , .2 ) );
                ring       = sin( ring * 100 ) * max( 1 - 10 * abs( ring - .5 ) , 0 );
                //ring 
                //ring -= .1*pow(  saturate(1-abs(  localSunUV.y ) ),10);

                //float sunDog = saturate( ring * 4 + 1 );


                //sunDog = zucconi( sunDog );
                //col.xyz		 += _DogMultiplier*match* (.5-abs(  sunDog -.5))* zucconi( 1-sunDog );

                if ( !isBehind )
                {
                    float2 vals = brightness( localSunUV , 5.0 , .6 );
                    col.xyz += ( match + .1 ) * _DogMultiplier * pow( saturate( vals.x ) , 1 ) * ( zucconi( 1 - vals.y + match * .1 ) * .7 + .3 ); // (match*.4+.2)* _DogMultiplier*saturate(  sunDog(localSunUV * .5));


                    for ( int i = 0; i < 10; i++ )
                    {

                        float  v     = ( sin( float( i ) * 1332. ) + sin( float( i ) * 14.14 ) ) * 5;
                        float2 hexUV = saturate( sunUV + localSunUV * v + fUV * ( 2 + sin( float( i ) * 12.31 ) ) * 2 + .5 );
                        col.xyz += ( 1 - SAMPLE_TEXTURE2D_X( _HexTexture , sampler_HexTexture , hexUV ) ).x * .1 * ( zucconi( abs( sin( float( i ) * 145.55 + length( localSunUV ) * .2 ) ) ) * .5 + .5 );
                        //col *= match;
                    }
                }
                //col.xyz= _SunPosition;
                //   col.xy = _ScreenParams.xy;
                // col.z = 0;
                return col; // example effect
            }
            ENDHLSL
        }
    }

}