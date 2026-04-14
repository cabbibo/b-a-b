Shader "Debug/ShardToggleGroupDebug"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue" ="Geometry"
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
            #include "Assets/Resources/Shaders/Chunks/hsv.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 world : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float  lightWeight : TEXCOORD2;
                float3 closestLight : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;


            #include  "Assets/Resources/Shaders/Chunks/ShardToggleGroup.cginc"

            v2f vert( appdata v )
            {

                v2f o;
                o.vertex       = UnityObjectToClipPos( v.vertex );
                o.world        = mul( unity_ObjectToWorld , float4( v.vertex.xyz , 1 ) ).xyz;
                o.lightWeight  = getVertexLightWeight( o.world , 10 , 2 );
                o.closestLight = getClosestOnLight( o.world );
                o.uv           = v.uv;
                return o;
            }

            float _AmountFilled;

            fixed4 frag( v2f v ) : SV_Target
            {
                // sample the texture
                float3 col = 0;

                uint ids[ 16 ];
                GetClosestShardIDs16( v.world , ids );

                for ( int i = 0; i < 16; i++ )
                {

                    int    id   = ids[ i ];
                    float4 data = _ShardBuffer[ id ];

                    col += hsv( float( id ) / float( _ShardBuffer_COUNT ) , 1 , 1 ) / ( 10 * pow( length( data.xyz - v.world ) , 2 ) );

                }

                //  col = v.lightWeight;

                //col = 1 / ( 10 * pow( length( v.closestLight.xyz - v.world ) , 2 ) );



                return float4( col , 1 );
            }
            ENDCG
        }
    }
}