Shader "Islands/Forest/Terrain"
{
    Properties
    {
        _LowLightColor ("LowLightColor", Color) = (0,0,0,1)
        _HighLightColor ("HighLightColor", Color) = (1,1,1,1)
        _EdgeNoiseColor ("EdgeNoiseColor", Color) = (1,1,1,1)
        _Color ("Color", Color) = (1,1,1,1)
        _BackfaceColor("BackfaceColor", Color )= (1,1,1,1)
        _Size ("Size", float) = .01
        _Fade ("Fade", float) = 1
        _FadeLocation ("_FadeLocation",Vector) = (0,0,0)
        _WindDirection ("_WindDirection",Vector) = (1,0,0)
        _WindAmount ("_WindAmount",float) = 1
        _WindChangeSpeed ("_WindChangeSpeed",float) = 1
        _WindChangeSize ("_WindChangeSize",float) = 1


        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}

        _NormalMap ("Normal Map", 2D) = "bump" {}
        _TriplanarMultiplier ("TriplanarMultiplier", Vector) = (1,1,1)
        _TriplanarSharpness ("TriplanarSharpness", float) = 1
        _TriplanarNormalWeight ("TriplanarNormalWeight", float) = 1

        _TextureShadingWeights( "Texture Shading Weight" , Vector ) = ( 0,1,2,3)

        _PainterlyLightMap("PainterlyLightMap", 2D) = "white" {}


        _OutlineAmount("OutlineAmount", float) = .1
        _OutlineColor("OutlineColor", Color) = (0,0,0,1)

        _ShadowStrength("ShadowStrength", float) = 1

        _PainterlyLightImportance("PainterlyLightImportance", float) = 1

        _OverallMultiplier("OverallMultiplier", float) = 1


        _StencilMask ("Stencil Mask", Int) = 9

        _FullNormalMap ("FullNormalMap", 2D) = "white" {}
        _FullNormalStrength ("FullNormalStrength", float) = 1



    }









    CGINCLUDE
    float4 _EdgeNoiseColor;

    samplerCUBE _Skybox;
    #include "Assets/Resources/Shaders/Chunks/QuillShaderIncludes.cginc"
    ENDCG





    SubShader
    {

        Pass
        {

            // Giving our selves stencil info 
            // for our outline shader to use
            /* Stencil
             {
                 Ref [_StencilMask]
                 Comp always
                 Pass replace
                 ZFail keep
             }*/

            Tags
            {
                "RenderType"="Opaque" "LightMode" = "ForwardBase"
            }
            LOD 100
            Cull Off


            CGPROGRAM
            #pragma vertex SetVaryings_UNITY
            #pragma fragment frag

            #include "Assets/Resources/Shaders/Chunks/SelfShadowingVertPragmas.cginc"

            sampler2D _FullNormalMap;
            float     _FullNormalStrength;


            float4 frag( FullVaryingData v ) : COLOR
            {


                LightingData lightingData;
                float3       col;

                fixed shadow = UNITY_SHADOW_ATTENUATION( v , v.worldPos ); // * .5 + .5;

                shadow = shadow * _ShadowStrength + ( 1 - _ShadowStrength );

                GetLightingData( v.worldPos , v.eye , v.nor , _WorldSpaceLightPos0.xyz , lightingData );

                float3 fNor       = normalize( lerp( lightingData.flatNormal , v.nor , 1 ) );
                float3 fullNormal = tex2D( _FullNormalMap , v.uv ).xyz;
                float  ao         = tex2D( _FullNormalMap , v.uv ).w;
                fNor += ( fullNormal.x * float3( 1 , 0 , 0 ) + fullNormal.y * float3( 0 , 0 , 1 ) + fullNormal.z * float3( 0 , 1 , 0 ) ) * _FullNormalStrength;
                fNor                = normalize( fNor );
                float3 triplanarNor = triplanarNormal( v.worldPos , fNor , v.tspace0 , v.tspace1 , v.tspace2 , v.offsetAmount * .1 );



                float3 traceCol = 1;
                float3 eye      = v.eye;
                for ( int i = 0; i < 3; i++ )
                {
                    float  ni   = (float)i / 3;
                    float3 fPos = v.worldPos - normalize( eye ) * float( i + 1 ) * 2.3;
                    float  v    = snoise( fPos * 2 + ( ni * 30 + 2 ) );
                    traceCol *= v;
                }

                float3 shadowCol = pow( traceCol , 6 ) * .1; //* traceCol * traceCol * .1;
                shadowCol        = length( shadowCol ) * v.color;

                shadowCol = lerp( shadowCol , 0 , saturate( length( v.eye ) * .01 ) );

                col = v.color; //lerp( v.color * (shadowCol * .5+.5) ,shadowCol,1-shadow);

                float3 reflection = reflect( -_WorldSpaceLightPos0 , triplanarNor );

                float  lightMatch      = saturate( dot( _WorldSpaceLightPos0 , triplanarNor ) );
                float  normalMatch     = saturate( dot( v.eye , triplanarNor ) );
                float  reflectionMatch = saturate( dot( reflection , normalize( v.eye ) ) );
                float  scale           = length( ObjectScale() );
                float4 painterlyColor  = PainterlyColor(
                    v.worldPos ,
                    triplanarNor ,
                    ( lightMatch * shadow + reflectionMatch * 3 + pow( normalMatch , 10 ) * .4 ) ,
                    ( GetXYInLightSpace( v.worldPos ) * _TriplanarMultiplier.xy )
                );

                col = painterlyColor;


                col += shadowCol * .2 * ( 1 - shadow );


                col *= lerp( float3( .1 , .1 , .2 ) , float3( 1 , .9 , .9 ) , shadow * ( .4 + floor( lightMatch * 5 ) / 5 ) );

                // col *= _LightColor0;
                col *= _OverallMultiplier;
                col *= tex2D( _MainTex , v.uv + ( 1 - ao ) * triplanarNor.x * .003 );

                if ( lightingData.eyeMatch - length( traceCol ) * .2 < .5 )
                {
                    //  col *= _EdgeNoiseColor;

                }

                if ( lightingData.eyeMatch - length( traceCol ) * .2 < .1 )
                {
                    // col = 0;
                    col = _EdgeNoiseColor; //discard;
                }

                float lightMatch2 = saturate( dot( _WorldSpaceLightPos0 , floor( ( triplanarNor * .2 + v.nor ) * 3 ) / 3 ) );
                lightMatch2 *= shadow * shadow * shadow;

                // lightMatch2 += tex2D( _PainterlyLightMap , v.uv * 100 ).x * .1;

                //  lightMatch2 += tex2D( _PainterlyLightMap , float2( v.uv.y , -v.uv.x ) * 100 ).x * .2;

                lightMatch2 += length( pow( traceCol , 4 ) * 100 ) * .01;


                float3 colorValue = lerp( _LowLightColor , _HighLightColor , lightMatch2 );
                col               = colorValue;

                col += pow( traceCol , 4 ) * 100 * colorValue;

                //col *= shadow * shadow * shadow;



                //  col = generic_desaturate( texCUBElod( _Skybox , float4( fNor , 6 ) ) , .6 ) * 2;]

                //col *= tex2D( _MainTex , v.uv );


                DoWrenDiscard( v.worldPos );

                return float4( col , 1 );
            }
            ENDCG
        }











        // shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass
        {
            Tags
            {
                "LightMode"="ShadowCaster"
            }

            Cull Off
            CGPROGRAM
            #pragma vertex SetShadowVaryings_UNITY
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

            float4 frag( FullVaryingData i ) : SV_Target
            {
                LightingData lightingData;
                GetLightingData( i.worldPos , i.eye , i.nor , _WorldSpaceLightPos0.xyz , lightingData );
                SHADOW_CASTER_FRAGMENT( i );


            }
            ENDCG
        }





    }

}





/*
        Pass
        {

            // Giving our selves stencil info 
            // for our outline shader to use
            Stencil
            {
                Ref [_StencilMask]
                Comp always
                Pass replace
                ZFail keep
            }
            Tags
            {
                "RenderType"="Opaque" "LightMode" = "ForwardBase"
            }
            LOD 100
            Cull Off


            CGPROGRAM
            #pragma vertex SetVaryings2
            #pragma fragment frag
            #include "Assets/Resources/Shaders/Chunks/SelfShadowingVertPragmas.cginc"

            sampler2D _FullNormalMap;
            float     _FullNormalStrength;
            float     _OutlineAmount;


            varyings SetVaryings2( inputData vert )
            {
                varyings o;

                UNITY_SETUP_INSTANCE_ID( vert );
                UNITY_TRANSFER_INSTANCE_ID( vert , o ); // necessary only if you want to access instanced properties in the fragment Shader.



                int instanceID = 0;
                #if defined(UNITY_INSTANCING_ENABLED)
                    instanceID = UNITY_GET_INSTANCE_ID(vert);
                #endif

                float3 wPos       = mul( unity_ObjectToWorld , float4( vert.vertex.xyz , 1 ) ).xyz;
                float3 windOffset = GetWindOffset( instanceID , wPos );

                o.nor = normalize( mul( unity_ObjectToWorld , float4( vert.normal , 0 ) ).xyz );

                o.worldPos = wPos + windOffset; //windAmount;
                o.worldPos += o.nor * _OutlineAmount;
                o.worldPos -= 40 * normalize( _WorldSpaceCameraPos - o.worldPos );

                o.pos          = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );
                o.eye          = _WorldSpaceCameraPos - o.worldPos;
                o.nor          = normalize( mul( unity_ObjectToWorld , float4( vert.normal , 0 ) ).xyz );
                o.uv           = vert.texcoord.xy;
                o.color        = vert.color;
                o.tangent      = vert.tangent.xyz * vert.tangent.w;
                o.offsetAmount = length( windOffset );

                half3 wNormal  = o.nor;
                half3 wTangent = mul( unity_ObjectToWorld , float4( vert.tangent.xyz , 0 ) ).xyz * vert.tangent.w;
                // compute bitangent from cross product of normal and tangent
                //half tangentSign = tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross( wNormal , wTangent ); // * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3( wTangent.x , wBitangent.x , wNormal.x );
                o.tspace1 = half3( wTangent.y , wBitangent.y , wNormal.y );
                o.tspace2 = half3( wTangent.z , wBitangent.z , wNormal.z );


                UNITY_TRANSFER_SHADOW( o , o.worldPos );
                UNITY_TRANSFER_FOG( o , o.pos );

                return o;

            }


            float4 frag( varyings v ) : COLOR
            {

                LightingData lightingData;
                float3       col;

                fixed shadow = UNITY_SHADOW_ATTENUATION( v , v.worldPos ); // * .5 + .5;

                shadow = shadow * _ShadowStrength + ( 1 - _ShadowStrength );

                GetLightingData( v.worldPos , v.eye , v.nor , _WorldSpaceLightPos0.xyz , lightingData );

                float3 fNor       = normalize( lerp( lightingData.flatNormal , v.nor , 1 ) );
                float3 fullNormal = tex2D( _FullNormalMap , v.uv ).xyz;
                float  ao         = tex2D( _FullNormalMap , v.uv ).w;
                fNor += ( fullNormal.x * float3( 1 , 0 , 0 ) + fullNormal.y * float3( 0 , 0 , 1 ) + fullNormal.z * float3( 0 , 1 , 0 ) ) * _FullNormalStrength;
                fNor                = normalize( fNor );
                float3 triplanarNor = triplanarNormal( v.worldPos , fNor , v.tspace0 , v.tspace1 , v.tspace2 , v.offsetAmount * .1 );



                float3 traceCol = 0;
                float3 eye      = v.eye;
                for ( int i = 0; i < 3; i++ )
                {
                    float  ni   = (float)i / 3;
                    float3 fPos = v.worldPos - normalize( eye ) * float( i ) * 1.3;
                    float  v    = snoise( fPos * ( ni * 10 + 2 ) );
                    traceCol += v;
                }

                float3 shadowCol = pow( traceCol , 6 ) * .1; //* traceCol * traceCol * .1;
                shadowCol        = length( shadowCol ) * v.color;

                shadowCol = lerp( shadowCol , 0 , saturate( length( v.eye ) * .01 ) );

                col = v.color; //lerp( v.color * (shadowCol * .5+.5) ,shadowCol,1-shadow);

                float  lightMatch      = saturate( dot( _WorldSpaceLightPos0 , triplanarNor ) );
                float  normalMatch     = saturate( dot( v.eye , triplanarNor ) );
                float  reflectionMatch = saturate( dot( reflect( -_WorldSpaceLightPos0 , triplanarNor ) , normalize( v.eye ) ) );
                float  scale           = length( ObjectScale() );
                float4 painterlyColor  = PainterlyColor(
                    v.worldPos ,
                    triplanarNor ,
                    ( lightMatch * shadow + reflectionMatch * 3 + pow( normalMatch , 10 ) * .4 ) ,
                    ( GetXYInLightSpace( v.worldPos ) * _TriplanarMultiplier.xy )
                );

                float4 painterlyColor2 = PainterlyColor(
                    v.worldPos ,
                    triplanarNor ,
                    ( lightMatch * shadow + reflectionMatch * 3 * shadow + pow( normalMatch , 10 ) * .4 ) ,
                    ( GetXYInLightSpace( v.worldPos ) * _TriplanarMultiplier.xy ).yx
                );



                col = lerp( 1 , lerp( painterlyColor , painterlyColor2 , 1 ) , _PainterlyLightImportance );

                col = lerp( float3( .2 , 0.2 , .4 ) , float3( 0 , .8 , 1 ) , painterlyColor.x );

                col += shadowCol * .2 * ( 1 - shadow );
                col *= v.color * 2.;


                col *= lerp( float3( .1 , .1 , .2 ) , float3( 1 , .9 , .9 ) , shadow * ( .4 + floor( lightMatch * 5 ) / 5 ) );

                // col *= _LightColor0;
                col *= _OverallMultiplier;
                col = generic_desaturate( col , 1.4 );
                col = tex2D( _MainTex , v.uv + ( 1 - ao ) * triplanarNor.x * .003 ).xyz;
                //col = tex2D( _MainTex , v.uv ).xyz;

                //col = floor( col * 20 ) / 20;

                //  col = triplanarNor;
                //col = traceCol;


                if ( lightingData.eyeMatch - length( traceCol ) * .2 < .5 )
                {
                    col *= float3( 0.3 , 0.5 , .6 );

                }

                if ( lightingData.eyeMatch - length( traceCol ) * .2 < .1 )
                {
                    // col = 0;
                    // discard;
                }

                // col *= 3;
                DoWrenDiscard( v.worldPos );


                return float4( col , 1 );
            }
            ENDCG
        }
*/
