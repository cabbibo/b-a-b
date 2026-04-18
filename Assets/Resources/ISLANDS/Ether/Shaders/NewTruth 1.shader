Shader "Islands/Ether/NewTruth1"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Size ("Size", float) = .01
        _Saturation ("Saturation", float) = .01
        _ColorMap("_ColorMap",2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    #include "AutoLight.cginc"
    #include "UnityLightingCommon.cginc"

    #include "Assets/Resources/Shaders/Chunks/hsv.cginc"
    #include "Assets/Resources/Shaders/Chunks/snoise.cginc"
    #include "Assets/Resources/Shaders/Chunks/triNoise3D.cginc"
    #include "Assets/Resources/Shaders/Chunks/InverseMatrix.cginc"
    #include "Assets/Resources/Shaders/Chunks/generic_desaturate.cginc"
    ENDCG

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry+8"
        }
        Cull Off

        GrabPass
        {
            "_BackgroundTexture1"
        }

        Pass
        {
            Tags
            {
                "LightMode" = "ForwardBase"
            }
            LOD 100
            Cull Off

            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            struct Particle
            {
                float3 pos;
                float3 vel;
                float3 nor;
                float3 tan;
                float2 uv;
                float2 debug;
            };

            StructuredBuffer<Particle> _FormBuffer;

            float _Size;
            float _Saturation;

            sampler2D _ColorMap;

            struct varyings
            {
                float4 pos : SV_POSITION;
                float3 nor : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 eye : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float  id : TEXCOORD4;
                float3 localCam : TEXCOORD7;
                float3 localPos : TEXCOORD5;
                float3 localRD : TEXCOORD6;
                float2 debug :TEXCOORD8;
                float  isComplete : TEXCOORD9;
                float  rowIsComplete :TEXCOORD10;
                float  inActiveRow : TEXCOORD11;
            };

            /*float4x4 WorldMatrix(
                float3 position ,
                float3 x ,
                float3 y ,
                float3 z ,
                float3 scale
            )
            {


                return float4x4(
                    float4( x * scale.x , position.x ) ,
                    float4( y * scale.y , position.y ) ,
                    float4( z * scale.z , position.z ) ,
                    float4( 0 , 0 , 0 , 1 )
                );
            }*/

            float4x4 WorldMatrix(
                float3 position ,
                float3 x ,
                float3 y ,
                float3 z ,
                float3 scale
            )
            {
                x *= scale.x;
                y *= scale.y;
                z *= scale.z;

                return float4x4(
                    x.x , y.x , z.x , position.x ,
                    x.y , y.y , z.y , position.y ,
                    x.z , y.z , z.z , position.z ,
                    0 , 0 , 0 , 1
                );
            }


            float _CrystalPercentage;
            int   _CrystalsForComplete;
            int   _MaxRows;


            varyings vert( appdata_full v , uint instanceID : SV_InstanceID )
            {
                varyings o;

                Particle p = _FormBuffer[ instanceID ];

                o.rowIsComplete = ( p.debug.x < floor( _CrystalPercentage ) ) ? 1 : 0;
                o.inActiveRow   = p.debug.x - floor( _CrystalPercentage ) == 0 ? 1 : 0;

                bool isCollected = floor( _CrystalPercentage * float( _CrystalsForComplete ) ) > instanceID;
                o.isComplete     = isCollected ? 1 : 0;

                float3 up      = float3( 0 , 0 , 1 );
                float3 forward = normalize( cross( p.vel , float3( 0 , 1 , 0 ) ) );


                float4x4 m = WorldMatrix(
                    p.pos ,
                    p.nor ,
                    p.vel ,
                    p.tan ,
                    _Size * p.uv.x
                );

                float3 localPos = v.vertex.xyz - float3( 0 , .3 , 0 );

                //localPos        = localPos.;
                float3 worldPos = mul( m , float4( localPos * float3( 1 , 1 , 1 ) , 1 ) ).xyz;

                o.worldPos = worldPos;
                o.nor      = normalize( mul( m , float4( v.normal , 0 ) ).xyz );
                o.eye      = _WorldSpaceCameraPos - worldPos;
                o.pos      = mul( UNITY_MATRIX_VP , float4( worldPos , 1 ) );
                o.uv       = v.texcoord;
                o.id       = instanceID;

                o.debug = p.debug;

                float4x4 worldToLocal = InverseMatrix( m );

                o.localCam = mul( worldToLocal , float4( _WorldSpaceCameraPos , 1 ) ).xyz;
                o.localRD  = normalize( o.localCam - localPos );

                o.localPos = localPos;



                return o;
            }

            sampler2D _BackgroundTexture1;

            float4 frag( varyings v ) : SV_Target
            {
                float shadow = 1;

                float m  = dot( UNITY_MATRIX_V[ 2 ].xyz , v.nor );
                float m2 = dot( float3( 0 , 1 , 0 ) , v.nor );

                float3 col = hsv( m2 * .4 , _Saturation , 1 );

                float lightness = saturate( m ) * ( shadow * .5 + .5 );
                lightness       = floor( lightness * 2 ) / 2;

                col *= lightness + .1;

                float shadowStep = floor( shadow * 3 ) / 3;

                float3 shadowCol = 0;

                for ( int i = 0; i < 3; i++ )
                {
                    float3 fPos = v.worldPos - normalize( v.eye ) * float( i ) * 1.3;
                    float  nv   = ( snoise( fPos * 10 ) + 1 ) * .5;
                    shadowCol += hsv( (float)i / 3 , 1 , nv );
                }

                shadowCol *= shadowCol;
                shadowCol *= shadowCol;
                shadowCol *= shadowCol;
                shadowCol *= shadowCol;

                shadowCol = length( shadowCol ) * ( shadowCol * .8 + .3 ) * 10;
                shadowCol += .3;
                shadowCol *= float3( .1 , .3 , .6 );
                shadowCol /= clamp( .1 + .1 * length( v.eye ) , 1 , 3 );

                col = shadowStep * col * float3( 1 , .8 , .6 ) * ( length( shadowCol ) + .4 )
                    + clamp( ( 1 - shadowStep ) * length( col ) * length( col ) * 10 , 0.05 , 1 ) * shadowCol;

                float3 eyeDir    = normalize( v.eye );
                float3 refracted = refract( eyeDir , v.nor , 1.0 / 1.33 );

                float3 newPos  = v.worldPos + refracted * .3;
                float4 grabPos = ComputeGrabScreenPos( mul( UNITY_MATRIX_VP , float4( newPos , 1 ) ) );

                float3 bgCol = tex2Dproj( _BackgroundTexture1 , grabPos ).xyz;

                float3 ro = v.localPos;
                float3 rd = v.localRD;

                float3 fog = 0;

                for ( int i = 0; i < 30; i++ )
                {
                    float3 fPos = ro - rd * float( i ) * .01;
                    //fPos += float3( 0 , 0.03 , .25 );

                    float nv = triNoise3D( fPos * 3 + v.id , 1 , _Time.x );
                    nv *= nv * nv * 10;

                    if ( length( fPos ) < .05 + nv * .08 ) nv += 1;
                    nv /= 40;

                    //  fog += hsv( float( i ) / 50 - .2 + v.debug.y / 8 , .6 , nv * nv );


                    fog += normalize( tex2D( _ColorMap , float2( float( i ) / 50 - .2 + v.debug.y / 8 , 0 ) ) ) * nv * nv; // , .6 , nv * nv );

                }

                col = 200 * fog; ///generic_desaturate( 100 * fog , 1 );

                col = generic_desaturate( col , 1 - v.inActiveRow );

                col *= normalize( tex2D( _ColorMap , float2( v.debug.y / 8 , 0 ) ) ) * 1.3 + .6;
                col *= v.isComplete + .1;

                if ( v.isComplete < 0.5 && v.inActiveRow < .5 )
                {
                    discard;
                }



                // col *= v.isComplete;

                // col *= v.inActiveRow;

                //col *= generic_desaturate( tex2D( _ColorMap , float2( v.debug.y / 8 , 0 ) ).xyz , .5 ); //.// )hsv( v.debug.y / 8 , v.debug.x , 1 );

                return float4( col , 1 );
            }
            ENDCG
        }
    }
}