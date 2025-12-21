Shader "IMMAT/Basic/Wind"
{
    Properties
    {

        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        LOD 100

        Cull Off
        Blend One One
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"


            struct Vert
            {
                float3 pos;
                float3 vel;
                float3 nor;
                float3 tan;
                float2 uv;
                float2 debug;
            };


            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 nor : NORMAL;
                float3 world : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float2 debug : TEXCOORD2;
                float3 vel : TEXCOORD3;
            };

            float4 _Color;

            StructuredBuffer<Vert> _VertBuffer;
            StructuredBuffer<int>  _TriBuffer;

            v2f vert( uint vid : SV_VertexID )
            {
                v2f  o;
                Vert v  = _VertBuffer[ _TriBuffer[ vid ] ];
                o.pos   = mul( UNITY_MATRIX_VP , float4( v.pos , 1.0f ) );
                o.nor   = v.nor;
                o.world = v.pos;
                o.uv    = v.uv;
                o.debug = v.debug;
                o.vel   = v.vel;


                return o;
            }

            sampler2D _MainTex;

            fixed4 frag( v2f v ) : SV_Target
            {

                float3 nor = -cross( normalize( ddx( v.world ) ) , normalize( ddy( v.world ) ) );
                // sample the texture
                fixed4 col = tex2D( _MainTex , v.uv );



                float life = v.debug.y;




                float fade = saturate( min( life , 1 - life ) * 10 );

                fade *= ( .5 - abs( v.uv.x - .5 ) ) * 2;
                fade *= ( .5 - abs( v.uv.y - .5 ) ) * 2;

                col.xyz *= normalize( v.vel ) * .5 + 1;


                col *= fade;

                if ( length( col.xyz ) < .1 )
                {
                    discard;
                }

                return col;
            }
            ENDCG
        }
    }
}