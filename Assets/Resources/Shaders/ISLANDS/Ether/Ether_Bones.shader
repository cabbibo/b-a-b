// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Islands/Ether/Bones"
{
    Properties
    {

        _Color ("Color", Color) = (1,1,1,1)
        _Size ("Size", float) = .01
        _Forwards ("Forwards", float) = 1
        _Saturation ("Saturation", float) = .01
    }


    CGINCLUDE
    #include "UnityCG.cginc"
    #include "AutoLight.cginc"

    #include "UnityLightingCommon.cginc"
    #include "Assets/Resources/Shaders/Chunks/hsv.cginc"

    #include "Assets/Resources/Shaders/Chunks/ShadowCasterPos.cginc"
    #include "Assets/Resources/Shaders/Chunks/snoise.cginc"
    ENDCG

    SubShader
    {
        Cull Off

        GrabPass
        {
            "_BackgroundTexture1"
        }

        Tags
        {
            "Queue" = "Geometry+8"
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
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup


            #pragma multi_compile_fogV
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            struct FullTransform
            {
                float4x4 fullMatrix;
                float3   pos;
                float3   vel;
                float2   debug;
            };

            StructuredBuffer<FullTransform> _FinalTransformBuffer;

            //#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            //#endif


            uniform int    _Count;
            uniform float  _Size;
            uniform float  _Forwards;
            uniform float3 _Color;
            uniform float  _Saturation;


            //uniform float4x4 worldMat;

            //A simple input struct for our pixel shader step containing a position.
            struct varyings
            {
                float4 pos : SV_POSITION;
                float3 nor : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 eye : TEXCOORD2;
                float2 uv : TEXCOORD4;
                float  id : TEXCOORD5;
                float4 debug : TEXCOORD6;
                float  hue : TEXCOORD7;
            };


            uniform float4x4 _Transform;
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert( appdata_full v , uint instanceID : SV_InstanceID )
            {

                varyings o;

                FullTransform ft = _FinalTransformBuffer[ instanceID ];

                float4x4 worldMat = ft.fullMatrix;

                float3 worldPos = mul( worldMat , float4( v.vertex.xyz * 1 , 1 ) ).xyz;
                float3 worldNor = mul( worldMat , float4( v.normal.xyz , 0 ) ).xyz;

                if ( instanceID >= _Count )
                {
                    worldPos = 0;
                    worldNor = float3( 0 , 1 , 0 );
                }

                // worldPos = v.vertex.xyz * 10;
                //worldNor = v.normal.xyz;
                // worldPos = v.vertex.xyz * .02 * ( f + u +r) + p;


                o.worldPos = worldPos;
                o.eye      = _WorldSpaceCameraPos - o.worldPos;
                o.nor      = normalize( worldNor ); //v.nor;
                o.uv       = v.texcoord.xy;
                o.id       = instanceID;
                o.pos      = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );
                o.debug.xy = ft.debug;
                o.hue      = 1;




                return o;

            }

            sampler2D _FullColorMap;


            //Pixel function returns a solid color for each point.
            float4 frag( varyings v ) : COLOR
            {

                fixed shadow = 1; //UNITY_SHADOW_ATTENUATION( v , v.worldPos ); //* .5 + .5;
                //    float3 tCol   = tex2D( _MainTex , v.uv );

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

                    float3 fPos = v.worldPos - normalize( v.eye ) * float( i ) * .6;
                    float  v    = ( snoise( fPos * 50 ) + 1 ) / 2;
                    shadowCol += hsv( (float)i / 3 , 1 , v );


                } //


                shadowCol *= shadowCol;
                shadowCol *= shadowCol;
                shadowCol *= shadowCol;
                shadowCol *= shadowCol;



                shadowCol = length( shadowCol ) * ( shadowCol * .8 + .3 ) * 10; //
                // shadowCol += .3;
                shadowCol *= float3( .1 , .3 , .6 );
                shadowCol /= clamp( ( .1 + .1 * length( v.eye ) ) , 1 , 3 );
                col = shadowCol;

                col = shadowStep * col * float3( 1 , .8 , .6 ) * ( length( shadowCol ) + .4 ) * 1 + clamp(
                    ( 1 - shadowStep ) * length( col ) * length( col ) * 10 , 0.05 ,
                    1 ) * shadowCol; // float3(.1,.2,.5);


                col += pow( 1 - clamp( dot( v.nor , normalize( v.eye ) ) , 0 , 1 ) , 10 ) * float3( .3 , .5 , 1 );


                return float4( col , 1 );
            }
            ENDCG

        }
    }

    Fallback "Diffuse"


}