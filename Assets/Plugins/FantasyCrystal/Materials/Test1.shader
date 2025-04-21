Shader "UI/Test1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Resources/Shaders/Chunks/triNoise3D.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert( appdata v )
            {
                v2f o;
                o.vertex = UnityObjectToClipPos( v.vertex );
                o.uv     = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag( v2f i ) : SV_Target
            {
                fixed4 col = tex2D( _MainTex , i.uv );
                // just invert the colors
                col.rgb = 1 - col.rgb;
                col     = float4( 1 , 0 , 0 , 1 );

                col.x = triNoise3D( float3( i.vertex.x , i.vertex.y , 0 ) * .005 , 2 , _Time.y );
                col.x *= col.x * col.x * 10;
                if ( abs( i.uv.x - .5 ) > .49 )
                {
                    col = 10;
                }

                if ( abs( i.uv.y - .5 ) > .4 )
                {
                    col = 10;
                }

                if ( col.x < ( 1 - i.uv.y ) + pow( abs( i.uv.x - .5 ) * 2 , 4 ) )
                {
                    discard;
                }
                col = 1;

                return col;
            }
            ENDCG
        }
    }
}