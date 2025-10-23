Shader "Islands/Mountain/ShardTrail"
{
    Properties
    {

        _Color ("Color", Color) = (1,1,1,1)
        _Color2 ("Color2", Color) = (1,1,1,1)
        _Color3 ("Color3", Color) = (1,1,1,1)

        _Size ("Size", float) = .01
        _LifeDivider ("_LifeDivider", float) = 10
    }

    CGINCLUDE
    uniform int    _Count;
    uniform float  _Size;
    uniform float3 _Color;

    struct MeshVert
    {
        float3 pos;
        float3 nor;
        float2 uv;
    };


    struct Vert
    {
        float3 pos;
        float3 vel;
        float3 nor;
        float3 ogPos;
        float  life;
        float  type;
        float2 debug;
    };

    StructuredBuffer<Vert>     _ShardBuffer;
    StructuredBuffer<MeshVert> _VertBuffer;
    StructuredBuffer<int>      _TriBuffer;

    int _TriCount;
    int _VertCount;


    #include "UnityCG.cginc"
    #include "UnityLightingCommon.cginc"
    #include "AutoLight.cginc"
    //uniform float4x4 worldMat;

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
        float3 barycentric : TEXCOORD10;
        float  id : TEXCOORD5;
        float  life : TEXCOORD7;
        float  type : TEXCOORD8;
        UNITY_SHADOW_COORDS( 9 )
    };


    #include "Assets/Resources/Shaders/Chunks/rotationMatrix.cginc"
    #include "Assets/Resources/Shaders/Chunks/translationMatrix.cginc"
    #include "Assets/Resources/Shaders/Chunks/scaleMatrix.cginc"
    #include "Assets/Resources/Shaders/Chunks/Matrix.cginc"


    //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
    //which we transform with the view-projection matrix before passing to the pixel program.
    varyings vert( uint id : SV_VertexID )
    {

        varyings o;

        int base      = id / _TriCount;
        int alternate = id % _TriCount;



        if ( base < _Count )
        {

            Vert   v     = _ShardBuffer[ base % _Count ];
            float3 extra = float3( 0 , 0 , 0 );

            float3 l = UNITY_MATRIX_V[ 0 ].xyz;
            float3 u = UNITY_MATRIX_V[ 1 ].xyz;

            float2 uv = float2( 0 , 0 );

            MeshVert vert = _VertBuffer[ _TriBuffer[ alternate ] ];

            float life = v.life;
            float type = v.type;

            float4x4 t = translationMatrix( v.pos );
            float4x4 r = look_at_matrix( normalize( v.vel ) ,
            normalize( cross(
                normalize( v.vel ) , float3( 1 , 0 , 0 ) ) ) );
            float4x4 s = scaleMatrix( 1 );

            float4x4 rts = mul( t , mul( r , s ) );

            float3 fwd = float3( 0 , 1 , 0 );
            if ( length( v.vel ) > .001 )
            {
                fwd = normalize( v.vel );
            }
            float3 up    = normalize( cross( fwd , float3( 1 , 0 , 0 ) ) );
            float3 right = normalize( cross( up , fwd ) );
            // o.worldPos =   v.pos + vert.pos * _Size;//

            //
            o.worldPos = mul( rts , float4( vert.pos.xzy * _Size * pow( life , .5 ) , 1 ) ).xyz;
            o.nor      = normalize( mul( rts , float4( vert.nor.xzy , 0 ) ).xyz );
            o.eye      = _WorldSpaceCameraPos - o.worldPos;
            //  o.nor =;
            o.uv   = vert.uv; //float2( v.life , v.type );
            o.uv2  = uv;
            o.id   = base;
            o.life = life;
            o.type = type;
            o.pos  = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );

            UNITY_TRANSFER_SHADOW( o , o.worldPos );

        }



        return o;

    }


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
        LOD 100
        Cull Off

        Tags
        {
            "Queue" = "Geometry+10"
            "LightMode" = "ForwardBase"
        }


        GrabPass
        {
            "_BackgroundTexture"
        }

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 4.5
            // make fog work
            #pragma multi_compile_fogV
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight


            //  #include "../../Chunks/Struct16.cginc"
            #include "Assets/Resources/Shaders/Chunks/hsv.cginc"
            #include "Assets/Resources/Shaders/Chunks/snoise.cginc"
            #include "Assets/Resources/Shaders/Chunks/CalcSparkles.cginc"

            sampler2D _BackgroundTexture;

            float       _LifeDivider;
            samplerCUBE _Skybox;

            float4 _Color2;
            float4 _Color3;
            //Pixel function returns a solid color for each point.
            float4 frag( varyings v ) : COLOR
            {


                float3 flatNormal = normalize( cross( ddx( v.worldPos ) , ddy( v.worldPos ) ) );
                //if( length( v.uv2 -.5) > .5 ){ discard;}


                //col = v.normal * .5  + .5;

                float3 col = float4( _Color.xyz , 1 ); // v.debug.x * 10;

                //color.xyz = hsv(v.life / _LifeDivider,1,1).xyz;

                col = v.nor; // * .5  + .5;

                float3 eye       = _WorldSpaceCameraPos - v.worldPos;
                float3 eyeDir    = normalize( eye );
                float3 refracted = refract( eyeDir , v.nor , 1.0 / 1.33 );
                refracted        = eyeDir;

                float3 newPos = v.worldPos + refracted * .3;

                float4 mvpPos = mul( UNITY_MATRIX_VP , float4( newPos , 1.0f ) );

                float4 grabPos = ComputeGrabScreenPos( mvpPos );

                float4 bgCol = tex2Dproj( _BackgroundTexture , grabPos );

                col = dot( _WorldSpaceLightPos0 , v.nor );
                col *= _LightColor0;

                col = hsv( v.type / 7 , 1 , 1 );

                if ( v.type > 6.5 )
                {
                    col = 1;
                }
                fixed shadow = UNITY_SHADOW_ATTENUATION( v , v.worldPos ); //* .5 + .5;

                //col.xy = v.nor;
                // col.z  = 0;
                col = texCUBE( _Skybox , normalize( v.nor ) );

                col = shadow * .3;

                col       = calcSparkles( v.worldPos * .1 , refracted );
                float val = length( col );
                //col = lerp( _Color , _Color2 , length( col ) );

                col *= _Color3 * _LightColor0 * pow( saturate( 1 - dot( v.nor , normalize( v.eye ) ) ) , 10 ) * .5 * ( val + 3 );
                col += _Color * _LightColor0 * saturate( ( dot( _WorldSpaceLightPos0 , v.nor ) + 1 ) ) * ( val + .5 );
                col += _Color2 * _LightColor0 * pow( ( saturate( dot( _WorldSpaceLightPos0 , reflect( -normalize( v.eye ) , v.nor ) ) ) ) , 100 ) * 2 * ( val + .5 );

                // col *= ( dot( _WorldSpaceLightPos0 , v.nor ) + 1 );
                //  col *= col;
                //col *= .5;
                //col *= _LightColor0;
                // col
                //col = bgCol;
                // col = tex2D( _MainTex , v.barycentric.xy );
                // col = bgCol;;
                // col = v.nor;

                //  col = v.type / 7;
                // col = bgCol.xyz + col * col * col * col * 10;

                // col = flatNormal;

                return float4( col , 1 );
            }
            ENDCG

        }




        // SHADOW PASS

        Pass
        {
            Tags
            {
                "Queue" = "Geometry+10"
                "LightMode" = "ShadowCaster"
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
            #pragma vertex vert2
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "Assets/Resources/Shaders/Chunks/ShadowCasterPos.cginc"

            struct varyings2
            {
                V2F_SHADOW_CASTER;
                float3 nor : NORMAL;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float4 data1 : TEXCOORD2;
            };


            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings2 vert2( uint id : SV_VertexID )
            {

                varyings2 o;

                int base      = id / _TriCount;
                int alternate = id % _TriCount;

                if ( base < _Count )
                {

                    Vert   v     = _ShardBuffer[ base % _Count ];
                    float3 extra = float3( 0 , 0 , 0 );

                    float3 l = UNITY_MATRIX_V[ 0 ].xyz;
                    float3 u = UNITY_MATRIX_V[ 1 ].xyz;

                    float2 uv = float2( 0 , 0 );

                    MeshVert vert = _VertBuffer[ _TriBuffer[ alternate ] ];

                    float life = v.life;
                    float type = v.type;

                    float4x4 t = translationMatrix( v.pos );
                    float4x4 r = look_at_matrix( normalize( v.vel ) ,
                                      normalize( cross(
                                          normalize( v.vel ) , float3( 1 , 0 , 0 ) ) ) );
                    float4x4 s = scaleMatrix( 1 );

                    float4x4 rts = mul( t , mul( r , s ) );


                    float3 fwd = float3( 0 , 1 , 0 );
                    if ( length( v.vel ) > .001 )
                    {
                        fwd = normalize( v.vel );
                    }

                    //
                    o.worldPos = mul( rts , float4( vert.pos.xzy * _Size * pow( life , .5 ) , 1 ) ).xyz;
                    o.nor      = normalize( mul( rts , float4( vert.nor.xzy , 0 ) ).xyz );

                    float4 position = ShadowCasterPos( o.worldPos , o.nor );
                    o.pos           = UnityApplyLinearShadowBias( position );



                }

                return o;

            }

            float4 frag( varyings i ) : COLOR
            {
                SHADOW_CASTER_FRAGMENT( i )
            }
            ENDCG
        }



    }





}
