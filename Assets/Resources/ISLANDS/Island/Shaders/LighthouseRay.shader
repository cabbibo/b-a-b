Shader "Islands/Island/LighthouseRay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        _Forwardness("Forwardness", Range(0, 20)) = 3

    }
    SubShader
    {

        Cull Off
        Blend One One
        // ZTest Always
        ZWrite Off

        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        LOD 100


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Assets/Resources/Shaders/Chunks/hash.cginc"


            uniform int   _Count;
            uniform float _Length;
            uniform float _Width;

            uniform float _Forwardness;

            uniform float3 _Center;


            //uniform float4x4 worldMat;

            //A simple input struct for our pixel shader step containing a position.
            struct varyings
            {
                float4 pos : SV_POSITION;
                float3 nor : TEXCOORD0;
                float3 world : TEXCOORD1;
                float3 eye : TEXCOORD2;
                float3 debug : TEXCOORD3;
                float2 uv : TEXCOORD4;
                float2 uv2 : TEXCOORD6;
                float  id : TEXCOORD5;
            };

            #include "Assets/Resources/Shaders/Chunks/snoise.cginc"


            float3 randomDirection( int i )
            {
                float x = sin( float( i ) * 103 + .1 );
                float y = sin( float( i ) * 1 + 1.12 );
                float z = ( sin( float( i ) * 140 + 4.12 ) ) + .1;

                return normalize( float3( x , y , z ) );

            }

            float4x4 _LocalToWorld;

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert( uint id : SV_VertexID )
            {

                varyings o;

                int idInTri = id % 3;
                int triID   = id / 3;

                float3 center  = mul( _LocalToWorld , float4( 0 , 0 , 0 , 1 ) ).xyz;
                float3 forward = normalize( mul( _LocalToWorld , float4( 0 , 0 , 1 , 0 ) ).xyz );
                float3 right   = normalize( mul( _LocalToWorld , float4( 1 , 0 , 0 , 0 ) ).xyz );
                float3 up      = normalize( mul( _LocalToWorld , float4( 0 , 1 , 0 , 0 ) ).xyz );

                float3 z = UNITY_MATRIX_V[ 2 ].xyz; //normalize(toLookAt);
                float3 x = UNITY_MATRIX_V[ 0 ].xyz; //normalize(cross(z,UNITY_MATRIX_V[1].xyz));
                float3 y = UNITY_MATRIX_V[ 1 ].xyz; // normalize(cross(z,x));


                float3 lookAt = _Center;

                float3 toLookAt = lookAt - center;


                float3 dir = randomDirection( triID );

                float angle  = hash( float( triID ) ) * 3.14 * 2;
                float radius = hash( float( triID + 1 ) );

                float2 randUV = float2( sin( angle ) , cos( angle ) ) * radius;

                float3 fDir = forward * _Forwardness + right * randUV.x + up * randUV.y;
                fDir        = normalize( fDir );

                // dir = dir.x * x + dir.y * y + dir.z * z * .001;
                //dir = normalize( dir );


                float3 pos = fDir * _Length + center;

                float3 f = UNITY_MATRIX_V[ 2 ].xyz;
                float3 r = normalize( cross( dir ,UNITY_MATRIX_V[ 2 ].xyz ) );
                float3 u = dir;

                float3 p1 = center;
                float3 p2 = center + fDir * _Length + r * _Width;
                float3 p3 = center + fDir * _Length - r * _Width;

                float3 fPos = p1;
                float2 uv   = float2( .5 , 0 );
                if ( idInTri == 1 )
                {
                    fPos = p2;
                    uv   = float2( 1 , 1 );
                }
                if ( idInTri == 2 )
                {
                    fPos = p3;
                    uv   = float2( 0 , 1 );
                }

                o.world = fPos; // mul(_LocalToWorld, float4(fPos,1)).xyz;
                o.pos   = mul( UNITY_MATRIX_VP , float4( o.world.xyz , 1 ) );
                o.uv    = uv;

                return o;
            }


            float4 _Color;

            fixed4 frag( varyings v ) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;

                col *= ( 1 - v.uv.y ) * ( .5 - abs( v.uv.x - .5 ) ); //  * .01;
                col *= min( v.uv.y * 50 * 10 , pow( ( 1 - v.uv.y ) , 20 ) * 2 );
                col.a = 1;

                col = saturate( col );
                // col   = 1 * .1;
                return col;
            }
            ENDCG
        }
    }
}