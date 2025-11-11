Shader "Unlit/CrystalFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS( 1 )
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;

            float _FillAmount;

            v2f vert( appdata v )
            {
                v2f o;
                o.vertex = UnityObjectToClipPos( v.vertex );
                o.uv     = TRANSFORM_TEX( v.uv , _MainTex );
                UNITY_TRANSFER_FOG( o , o.vertex );
                return o;
            }

            fixed4 frag( v2f i ) : SV_Target
            {
                // sample the texture
                fixed4 col = 1;



                col = 1;

                if ( abs( i.uv.y - .5 ) < .45 && i.uv.x > _FillAmount && abs( i.uv.x - .5 ) < .49 )
                {
                    discard;
                }

                return col;
            }
            ENDCG
        }
    }
}