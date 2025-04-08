// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/MeshPointerInterface"
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


            //#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            StructuredBuffer<float2>   _ConnectionBuffer;
            StructuredBuffer<float4x4> _TransformBuffer;
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
            };


            StructuredBuffer<float3> _PositionBuffer;
            StructuredBuffer<float>  _FadeBuffer;
            StructuredBuffer<float>  _TypeBuffer;
            StructuredBuffer<float4> _ExtraDataBuffer;


            float4x4 createTransformationMatrix( float3 forward , float3 right , float3 up , float3 position ,
                                       float3           scale )
            {
                // Normalize the direction vectors to ensure they are unit vectors
                forward = normalize( forward );
                right   = normalize( right );
                up      = normalize( up );

                // Return the transformation matrix by filling in the values directly
                return float4x4(
                    scale.x * right.x , scale.x * right.y , scale.x * right.z , 1.0 , // First row: scaled right vector
                    scale.y * up.x , scale.y * up.y , scale.y * up.z , 1.0 , // Second row: scaled up vector
                    scale.z * forward.x , scale.z * forward.y , scale.z * forward.z , 1.0 ,
                    // Third row: scaled negative forward vector
                    position.x , position.y , position.z , 1.0 // Fourth row: position (translation)
                );
            }

            float3 _WrenPos;

            uniform float4x4 _Transform;
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert( appdata_full v , uint instanceID : SV_InstanceID )
            {

                varyings o;

                float3 center    = _PositionBuffer[ instanceID ];
                float3 centerPos = _WrenPos;
                float3 fwd       = centerPos - _WorldSpaceCameraPos;

                float scale = _Size;

                float3 f = normalize( fwd );
                float3 r = normalize( cross( f , float3( 0 , 1 , 0 ) ) );
                float3 u = normalize( cross( r , f ) );

                f = float3( 0 , 0 , 1 );
                r = float3( 1 , 0 , 0 );
                u = float3( 0 , 1 , 0 );

                float3 p = centerPos + normalize( fwd ) * 5 * _Size; //.5 * scale;

                float scl = _Size;


                float4x4 worldMat = float4x4(
                    scl * r.x , scl * u.x , scl * f.x , p.x ,
                    scl * r.y , scl * u.y , scl * f.y , p.y ,
                    scl * r.z , scl * u.z , scl * f.z , p.z ,
                    0 , 0 , 0 , 1 );

                float3 worldPos = mul( worldMat , float4( v.vertex.xyz * _Size * 1 , 1 ) ).xyz;
                float3 worldNor = mul( worldMat , float4( v.normal.xyz , 0 ) ).xyz;

                o.worldPos = worldPos;

                o.eye = _WorldSpaceCameraPos - o.worldPos;
                o.nor = worldNor; //v.nor;
                o.uv  = v.texcoord.xy;
                o.id  = instanceID;
                o.pos = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );



                return o;

            }


            //Pixel function returns a solid color for each point.
            float4 frag( varyings v ) : COLOR
            {


                float3 col = float3( 1 , 0 , 0 );
                return float4( col , 1 );
            }
            ENDCG

        }
    }

    Fallback Off


}