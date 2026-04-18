// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Islands/Ether/Feathers"
{
    Properties
    {

        _Color ("Color", Color) = (1,1,1,1)
        _Size ("Size", float) = .01
        _Saturation ("Saturation", float) = .01



        _IsBody("Is body" , float ) = 0

        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }


    CGINCLUDE
    #include "Assets/Resources/Shaders/Chunks/FeatherCommon.cginc"
    ENDCG






    SubShader
    {

        Tags
        {
            "Queue" = "Geometry+8"
        }
        GrabPass
        {
            "_BackgroundTexture1"
        }

        Pass
        {

            LOD 100
            Cull Off
            Tags
            {
                "LightMode" = "ForwardBase"
            }



            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 4.5
            // make fog work
            #pragma multi_compile_fogV
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight


            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert( uint id : SV_VertexID )
            {

                return SetUpOutputValues( id );

            }


            //Pixel function returns a solid color for each point.
            float4 frag( varyings v ) : COLOR
            {
                fixed  shadow = UNITY_SHADOW_ATTENUATION( v , v.worldPos ); //* .5 + .5;
                float3 tCol   = tex2D( _MainTex , v.uv );

                float  m         = dot( UNITY_MATRIX_V[ 2 ].xyz , v.nor );
                float3 m2        = dot( float3( 0 , 1 , 0 ) , v.nor );
                float  hueOffset = sin( v.id * 15.91 ) * .04 + sin( v.id * 14.1445 ) * .06;




                // float3 col= float3(v.data1.x,v.data1.y,1.);//(1-tCol.x) * hsv(m * .3 + v.feather * .2, 1,1) * shadow;
                float3 col       = hsv( v.hue + m2 * .4 , _Saturation , 1 ); // * lerp(1,tCol ,1-shadow);
                float  lightness = saturate( m ) * ( shadow * .5 + .5 );
                lightness        = floor( lightness * 2 ) / 2;


                col *= lightness + .1;



                //col *= col * col * col * 10;




                float shadowStep = floor( shadow * 3 ) / 3;

                //float 

                float3 shadowCol = 0;

                for ( int i = 0; i < 3; i++ )
                {

                    float3 fPos = v.worldPos - normalize( v.eye ) * float( i ) * 1.3;
                    float  v    = ( snoise( fPos * 10 ) + 1 ) / 2;
                    shadowCol += hsv( (float)i / 3 , 1 , v );


                } //


                shadowCol *= shadowCol;
                shadowCol *= shadowCol;
                shadowCol *= shadowCol;
                shadowCol *= shadowCol;

                shadowCol = length( shadowCol ) * ( shadowCol * .8 + .3 ) * 10; //
                shadowCol += .3;
                shadowCol *= float3( .1 , .3 , .6 );
                shadowCol /= clamp( ( .1 + .1 * length( v.eye ) ) , 1 , 3 );
                col = shadowStep * col * float3( 1 , .8 , .6 ) * ( length( shadowCol ) + .4 ) * 1 + clamp( ( 1 - shadowStep ) * length( col ) * length( col ) * 10 , 0.05 , 1 ) * shadowCol; // float3(.1,.2,.5);


                float b = length( col );


                tCol = tex2D( _FullColorMap , float2( -m * .3 + v.feather * .3 , v.baseHue ) ).xyz;

                col.xyz *= ( tCol * 1 + 1.4 ); //normalize( col*col) * b * b * 4;
                //col = saturate(col/.8)*.8;


                col = pow( length( col ) , 2 ) * col * m * m;

                col = tCol;
                col = 1 * m;

                col = saturate( col );


                col = hsv( .5 * ( v.randID / _TotalShardsInBody ) , 1 , 1 );

                col = hsv( v.collectionType / 7 , 1 , 1 );

                float3 eye       = _WorldSpaceCameraPos - v.worldPos;
                float3 eyeDir    = normalize( eye );
                float3 refracted = refract( eyeDir , v.nor , 1.0 / 1.33 );

                float3 newPos = v.worldPos + refracted * .3;

                float4 mvpPos = mul( UNITY_MATRIX_VP , float4( newPos , 1.0f ) );

                float4 grabPos = ComputeGrabScreenPos( mvpPos );

                float4 bgCol = tex2Dproj( _BackgroundTexture1 , grabPos );


                col += dot( _WorldSpaceLightPos0 , v.nor );
                col *= _LightColor0;


                float3 barys;
                barys.xy = v.barycentric;
                barys.z  = 1 - barys.x - barys.y;

                float minBary = min( barys.x , min( barys.y , barys.z ) );

                col = lerp( 1 , 0 , saturate( minBary * 10 ) );
                //col = bgCol.xyz + col*col *col*col * 10;



                float3 ro = v.localPos;
                float3 rd = v.localRD;

                float3 fog = 0;



                float id = v.id;
                id       = v.randID;

                float3 localNor = normalize( cross(
                    ddy( v.localPos ) ,
                    ddx( v.localPos )
                ) );

                rd = refract( rd , localNor , .8 );


                for ( int i = 0; i < 30; i++ )
                {


                    float3 fPos = ro - rd * float( i ) * .01f;
                    // fPos *= 10;

                    fPos += float3( 0 , 0.03 , .25 );
                    //fPos += 1000; 

                    // fPos %= .03;
                    //fPos -= .015;
                    /*fPos *= float3(1,1,1);
                    fPos %= .1;*/
                    float v = triNoise3D( fPos * 3 + id , 1 , _Time.x );

                    v *= v * v * 10;

                    if ( length( fPos ) < .04 + v * .08 )
                    {
                        v += 1;
                    }

                    v /= 40;

                    if ( v > .48 )
                    {
                        //  fog += hsv(0,0,1);
                    }


                    /*if( v > 0.3/40 ){
                      fog = hsv(float(i)/10,1,1);
                      break;
                    }*/

                    fog += hsv( float( i ) / 30 , 1 , v * v );

                }

                col = 50 * fog;

                //col *= hsv( v.hue + sin(id) * .1,.4,1);

                if ( minBary < .001 )
                {
                    // col = 1;// bgCol.xyz;
                }

                //  col = localNor * .5 +.5;
                // col *= hsv(v.hue,.5,1);//fog;

                //col += pow(1-m,10);

                // col += normalize(v.localRD)* .5 + .5;

                // col = fog / 30;


                //col = bgCol;


                //col = v.nor * .5 +.5;
                return float4( col , 1 );
            }
            ENDCG

        }







































        // SHADOW PASS

        Pass
        {
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            Tags
            {
                "Queue" = "Geometry+100"
            }

            Fog
            {
                Mode Off
            }
            ZWrite On
            ZTest LEqual
            Cull Off
            Offset 1, 1
            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"
            #include "Assets/Resources/Shaders/Chunks/ShadowCasterPos.cginc"


            struct v2f
            {
                V2F_SHADOW_CASTER;
                float3 nor : NORMAL;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float4 data1 : TEXCOORD2;
            };


            v2f vert( appdata_base input , uint id : SV_VertexID )
            {
                v2f o;


                //             UNITY_INITIALIZE_OUTPUT(v2f, o);


                int base      = id / _TrisPerMesh;
                int alternate = id % _TrisPerMesh;


                Feather feather = _FeatherBuffer[ base ];


                int whichMesh = int( feather.featherType ); //int(floor(hash(float(base)) * float(_NumberMeshes)));// %4;


                float4x4 baseMatrix = feather.ltw;
                Vert     v          = _VertBuffer[ _TriBuffer[ alternate + whichMesh * _TrisPerMesh ] ];

                float4x4 worldToLocal = transpose( baseMatrix );

                o.worldPos = mul( baseMatrix , float4( v.pos , 1 ) ).xyz; //extra;

                o.nor = normalize( mul( baseMatrix , float4( v.nor , 0 ) ).xyz );

                if ( feather.id > _NumShards )
                {
                    o.worldPos *= 0;
                }

                o.pos = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );



                float4 position = ShadowCasterPos( o.worldPos , o.nor );
                o.pos           = UnityApplyLinearShadowBias( position );


                // UNITY_TRANSFER_SHADOW(o,o.worldPos);

                return o;

            }

            float4 frag( v2f i ) : COLOR
            {
                SHADOW_CASTER_FRAGMENT( i )
            }
            ENDCG
        }






    }



}