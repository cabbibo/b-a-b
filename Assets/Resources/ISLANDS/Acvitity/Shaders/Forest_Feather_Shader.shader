// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Islands/Forest/Feathers"
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
    #pragma skip_variants SHADOWS_CUBE SHADOWS_DEPTH
    #include "AutoLight.cginc"
    #include "UnityLightingCommon.cginc"
    #include "Assets/Resources/Shaders/Chunks/hsv.cginc"

    //A simple input struct for our pixel shader step containing a position.
    struct varyings
    {
        float4 pos : SV_POSITION;
        float3 nor : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
        float3 eye : TEXCOORD2;
        float3 debug : TEXCOORD3;
        float2 uv : TEXCOORD4;
        float2 uv2 : TEXCOORD6;
        float  id : TEXCOORD5;
        float  randID : TEXCOORD13;
        float  hue : TEXCOORD10;
        float  offset : TEXCOORD11;
        float  baseHue : TEXCOORD12;
        int    feather:TEXCOORD7;
        float4 data1:TEXCOORD9;
        float  collectionType:TEXCOORD14;
        float3 barycentric : TEXCOORD15;
        UNITY_SHADOW_COORDS( 8 )
    };


    [maxvertexcount(3)]
    void geom( triangle varyings input[ 3 ] , inout TriangleStream<varyings> triStream )
    {
        varyings o;
        //  float3 normal = normalize(cross(input[1].vertex - input[0].vertex, input[2].vertex - input[0].vertex));

        float3 normal = float3( 0 , 1 , 0 );


        o             = input[ 0 ];
        o.barycentric = float3( 1 , 0 , 0 );
        triStream.Append( o );

        o             = input[ 1 ];
        o.barycentric = float3( 0 , 1 , 0 );
        triStream.Append( o );

        o             = input[ 2 ];
        o.barycentric = float3( 0 , 0 , 1 );
        triStream.Append( o );


        triStream.RestartStrip();
    }


    float getGrid( float3 barys , float size , float offset )
    {

        float val = max( max( sin( barys.x * size ) , sin( barys.y * size ) ) , sin( barys.z * size ) );
        val -= offset;
        val /= ( 1 - offset );
        val = clamp( val , 0 , 1 );
        return val;
    }
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

            #include "UnityCG.cginc"


            uniform int    _Count;
            uniform float  _Size;
            uniform float3 _Color;

            float _Saturation;

            uniform int _TrisPerMesh;

            struct Vert
            {
                float3 pos;
                float3 nor;
                float2 uv;
            };


            #include "Assets/Resources/Shaders/Chunks/FeatherStruct.cginc"

            StructuredBuffer<Vert>    _VertBuffer;
            StructuredBuffer<int>     _TriBuffer;
            StructuredBuffer<Feather> _FeatherBuffer;


            uniform float _BodyShardRendered;

            bool GetShown( int id )
            {

            }


            //uniform float4x4 worldMat;

            sampler2D _MainTex;


            #include "Assets/Resources/Shaders/Chunks/hash.cginc"
            uniform float4x4 _Transform;
            uniform int      _NumberMeshes;

            float _Hue1;
            float _Hue2;
            float _Hue3;
            float _Hue4;

            float _IsBody;


            float _TotalShardsInBody;
            float _NumShards;
            float _TmpNumShards;
            float _ONumShards;

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert( uint id : SV_VertexID )
            {

                varyings o;

                int     base      = id / _TrisPerMesh;
                int     alternate = id % _TrisPerMesh;
                Feather feather   = _FeatherBuffer[ base ];

                int whichMesh = int( feather.featherType );
                //int(floor(hash(float(base)) * float(_NumberMeshes)));// %4;


                float4x4 baseMatrix = feather.ltw;
                Vert     v          = _VertBuffer[ _TriBuffer[ alternate + whichMesh * _TrisPerMesh ] ];


                float3 pos = v.pos;

                if ( feather.id > _NumShards )
                {
                    //pos *= 0;
                }




                // o.data1 = feather.newData1;
                o.worldPos = mul( baseMatrix , float4( pos , 1 ) ).xyz; //extra;
                o.id       = float( base );
                o.feather  = whichMesh;

                o.baseHue = _Hue1;

                o.hue    = _Hue1;
                o.randID = feather.id;



                if ( whichMesh == 1 ) { o.hue = _Hue2; }
                if ( whichMesh == 2 ) { o.hue = _Hue3; }
                if ( whichMesh == 3 ) { o.hue = _Hue4; }
                if ( whichMesh == 4 ) { o.hue = _Hue4; }

                o.collectionType = feather.type;





                //o.data1 = feather.newData1;
                o.nor = normalize( mul( baseMatrix , float4( v.nor , 0 ) ).xyz );
                o.pos = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );
                o.uv  = v.uv;
                o.eye = _WorldSpaceCameraPos - o.worldPos;
                UNITY_TRANSFER_SHADOW( o , o.worldPos );


                return o;

            }

            sampler2D _FullColorMap;
            #include "Assets/Resources/Shaders/Chunks/snoise.cginc"


            sampler2D   _BackgroundTexture1;
            samplerCUBE _Skybox;


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
                col = shadowStep * col * float3( 1 , .8 , .6 ) * ( length( shadowCol ) + .4 ) * 1 + clamp(
                    ( 1 - shadowStep ) * length( col ) * length( col ) * 10 , 0.05 ,
                    1 ) * shadowCol; // float3(.1,.2,.5);


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

                //col = lerp( 1 , 0 , saturate( minBary * 10 ) );
                //  col *= hsv( v.collectionType / 7 , 1 , 1 );


                //col = bgCol.xyz + col*col *col*col * 10;


                //col = bgCol;

                col *= tex2D( _MainTex , v.uv );
                col *= texCUBElod( _Skybox , float4( normalize( v.nor ) , 6 ) ).xyz;
                col *= 3;

                //col = shadow;
                //col = dot( _WorldSpaceLightPos0 , v.nor );
                // col += ( 1 - shadow ) * float3( 1 , 0 , 0 );

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


            struct Vert
            {
                float3 pos;
                float3 nor;
                float2 uv;
            };

            #include "Assets/Resources/Shaders/Chunks/FeatherStruct.cginc"
            int                       _TrisPerMesh;
            StructuredBuffer<Vert>    _VertBuffer;
            StructuredBuffer<Feather> _FeatherBuffer;
            StructuredBuffer<int>     _TriBuffer;

            struct v2f
            {
                V2F_SHADOW_CASTER;
                float3 nor : NORMAL;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float4 data1 : TEXCOORD2;
            };

            float _TotalShardsInBody;
            float _NumShards;
            float _TmpNumShards;
            float _ONumShards;


            v2f vert( appdata_base input , uint id : SV_VertexID )
            {
                v2f o;


                //             UNITY_INITIALIZE_OUTPUT(v2f, o);


                int base      = id / _TrisPerMesh;
                int alternate = id % _TrisPerMesh;


                Feather feather = _FeatherBuffer[ base ];


                int whichMesh = int( feather.featherType );
                //int(floor(hash(float(base)) * float(_NumberMeshes)));// %4;


                float4x4 baseMatrix = feather.ltw;
                Vert     v          = _VertBuffer[ _TriBuffer[ alternate + whichMesh * _TrisPerMesh ] ];

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