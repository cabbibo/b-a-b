Shader "Unlit/SkyboxTest"
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS( 1 )
                float4 vertex : SV_POSITION;
                float3 world : TEXCOORD2;
                float3 normal : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;

            samplerCUBE _Skybox;

            v2f vert( appdata v )
            {
                v2f o;
                o.vertex = UnityObjectToClipPos( v.vertex );
                o.world  = mul( unity_ObjectToWorld , v.vertex ).xyz;
                o.uv     = TRANSFORM_TEX( v.uv , _MainTex );
                o.normal = v.normal;
                UNITY_TRANSFER_FOG( o , o.vertex );
                return o;
            }

            fixed4 frag( v2f i ) : SV_Target
            {

                float3 dir = i.world - _WorldSpaceCameraPos;
                // sample the texture
                fixed4 col = texCUBE( _Skybox , normalize( i.normal ) );
                //   col        = 1;
                //col.xyz    = 1; // i.normal;
                // apply fog
                UNITY_APPLY_FOG( i.fogCoord , col );
                return col;
            }
            ENDCG
        }
    }
}