Shader "Islands/Island/Model"
{
    Properties
    {

        _HighLightColor("HighLightColor", Color) = (1,1,1,1)
        _LowLightColor("LowLightColor", Color) = (0,0,0,1)
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




    }









    CGINCLUDE
    #include "Assets/Resources/Shaders/Chunks/QuillShaderIncludes.cginc"
    ENDCG





    SubShader
    {

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
            #pragma vertex SetVaryings
            #pragma fragment frag
            #include "Assets/Resources/Shaders/Chunks/SelfShadowingVertPragmas.cginc"

            float4 frag( varyings v ) : COLOR
            {


                LightingData lightingData;
                float3       col;

                fixed shadow = UNITY_SHADOW_ATTENUATION( v , v.worldPos ); // * .5 + .5;

                shadow = shadow * _ShadowStrength + ( 1 - _ShadowStrength );

                GetLightingData( v.worldPos , v.eye , v.nor , _WorldSpaceLightPos0.xyz , lightingData );

                float3 fNor         = normalize( lerp( lightingData.flatNormal , v.nor , 1 ) );
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



                col = lerp( 1 , lerp( painterlyColor , painterlyColor2 , 0 ) , _PainterlyLightImportance );

                col += shadowCol * .2 * ( 1 - shadow );
                col *= v.color * 2.;


                col *= lerp( float3( .1 , .1 , .2 ) , float3( 1 , .9 , .9 ) , shadow * ( .4 + floor( lightMatch * 5 ) / 5 ) );

                // col *= _LightColor0;
                col *= _OverallMultiplier;
                //col = traceCol;

                if ( lightingData.eyeMatch - length( traceCol ) * .2 < .3 )
                {
                    discard;
                }

                DoWrenDiscard( v.worldPos );

                return float4( col , 1 );
            }
            ENDCG
        }



        // shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass
        {


            Cull OFF
            ZWrite ON
            ZTest ON

            // Here is where we set the values 
            // so the outline will only show *outside* 
            // the object
            /* Stencil
             {
                 Ref [_StencilMask]
                 Comp notequal
                 Fail keep
                 Pass replace
             }*/

            CGPROGRAM
            #pragma vertex vert2
            #pragma fragment frag2
            #pragma multi_compile_shadowcaster

            float  _OutlineAmount;
            float4 _OutlineColor;

            varyings vert2( inputData vert )
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

                o.eye          = _WorldSpaceCameraPos - wPos;
                o.nor          = normalize( mul( unity_ObjectToWorld , float4( vert.normal , 0 ) ).xyz );
                o.worldPos     = wPos + windOffset + o.nor * _OutlineAmount - 10 * normalize( o.eye ) * _OutlineAmount; //windAmount;
                o.pos          = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );
                o.eye          = _WorldSpaceCameraPos - o.worldPos;
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

            /*
struct LightingData
{
    float3 flatNormal;
    float  flatNormalMatch;
    float  normalMatch;
    float  lightMatch;
    float  flatLightMatch;
    float  eyeMatch;
    float  reflectionMatch;
};
*/


            float4 frag2( varyings v ) : SV_Target
            {
                LightingData lightingData;
                GetLightingData( v.worldPos , v.eye , v.nor , _WorldSpaceLightPos0.xyz , lightingData );

                float3 traceCol = 0;
                float3 eye      = v.eye;
                for ( int i = 0; i < 3; i++ )
                {
                    float  ni   = (float)i / 3;
                    float3 fPos = v.worldPos - normalize( eye ) * float( i ) * 1.3;
                    float  v    = snoise( fPos * ( ni * 10 + 2 ) );
                    traceCol += v;
                }
                if ( lightingData.eyeMatch - length( traceCol ) * .2 < .3 )
                {
                    discard;
                }

                // float4 col = float4( _OutlineColor.xyz * v.color * 6 * ( 1 - lightingData.eyeMatch ) , 1 );
                float4 col = float4( _OutlineColor.xyz * v.color , 1 );
                col.xyz    = generic_desaturate( col , 2 );
                return col;

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
            #pragma vertex SetShadowVaryings
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

            float4 frag( varyings i ) : SV_Target
            {
                LightingData lightingData;
                GetLightingData( i.worldPos , i.eye , i.nor , _WorldSpaceLightPos0.xyz , lightingData );
                SHADOW_CASTER_FRAGMENT( i );


            }
            ENDCG
        }





    }

}