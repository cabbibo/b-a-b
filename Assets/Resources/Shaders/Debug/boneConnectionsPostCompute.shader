// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/BonesConnectionPostCompute"
{
    Properties
    {

        _Color ("Color", Color) = (1,1,1,1)
        _Size ("Size", float) = .01
        _Forwards ("Forwards", float) = 1
    }


    SubShader
    {
        Cull Off
        Pass
        {

            CGPROGRAM
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "UnityCG.cginc"
            #include "Assets/Resources/Shaders/Chunks/hsv.cginc"


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



                return o;

            }


            //Pixel function returns a solid color for each point.
            float4 frag( varyings v ) : COLOR
            {


                float3 col = 1;
                col        = v.nor * .5 + .5;
                col        = v.debug.xyz;
                return float4( col , 1 );
            }
            ENDCG

        }
    }

    Fallback Off


}