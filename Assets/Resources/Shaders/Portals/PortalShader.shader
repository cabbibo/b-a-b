Shader "AcrossUniverse/PortalShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OtherWorldCubemap ("Other World Cubemap", Cube) = "_Skybox" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _PortalFadeNormalizer ("Portal Fade Normalizer", float) = 0.5
        _PortalAmountShown ("Portal Amount Shown", float) = 0.5
        _PortalNoiseSize ("Portal Noise Size", float ) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100
        Cull Off


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog


            #include "UnityCG.cginc"
            #include "Assets/Resources/Shaders/Chunks/triNoise3D.cginc"

            samplerCUBE _OtherWorldCubemap;
            sampler2D   _NormalMap;

            float _PortalFadeNormalizer;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2   uv : TEXCOORD0;
                float3   world : TEXCOORD1;
                float3   eye : TEXCOORD2;
                float3   nor : TEXCOORD3;
                float4   vertex : SV_POSITION;
                float3   t1 : TEXCOORD4;
                float3   t2 : TEXCOORD5;
                float3   t3 : TEXCOORD6;
                float3x3 TBN : TEXCOORD7;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;

            float3 _BasePosition;


            float _PortalAmountShown;
            float _PortalNoiseSize;

            v2f vert( appdata v )
            {
                v2f o;
                o.world  = mul( unity_ObjectToWorld , v.vertex ).xyz;
                o.eye    = o.world - _WorldSpaceCameraPos;
                o.nor    = normalize( mul( unity_ObjectToWorld , float4( v.normal , 0 ) ).xyz );
                o.vertex = UnityObjectToClipPos( v.vertex );
                o.uv     = TRANSFORM_TEX( v.uv , _MainTex );

                //o.local = v.vertex.xyz;

                if ( dot( normalize( o.eye ) , o.nor ) < 0 )
                {
                    // o.nor = -o.nor;
                }

                float3 fNor = normalize( o.nor );
                float3 fBi  = normalize( cross( fNor , float3( 0 , 1 , 0 ) ) );
                float3 fTan = normalize( cross( fNor , fBi ) );



                // output the tangent space matrix
                o.t1  = float3( fTan.x , fBi.x , fNor.x );
                o.t2  = float3( fTan.y , fBi.y , fNor.y );
                o.t3  = float3( fTan.z , fBi.z , fNor.z );
                o.TBN = float3x3( fTan , fBi , fNor );

                UNITY_TRANSFER_FOG( o , o.vertex );
                return o;
            }


            float3 MapNormal( float3 nor , float3 t1 , float3 t2 , float3 t3 , float2 uv , float val )
            {
                float3 tnormal = UnpackNormal( tex2D( _NormalMap , uv ) );
                // transform normal from tangent to world space
                float3 n;
                n.x = dot( t1 , tnormal );
                n.y = dot( t2 , tnormal );
                n.z = dot( t3 , tnormal );

                return normalize( lerp( nor , normalize( n ) , val ) );
            }

            fixed4 frag( v2f v ) : SV_Target
            {
                // sample the texture
                float3 col = tex2D( _MainTex , v.uv );

                float3 normalTex = tex2D( _NormalMap , v.uv ).xyz * 2 - 1;
                float3 normal    = normalize( mul( v.TBN , normalTex ) );

                normal = lerp( normal , v.nor , 0.95 );

                float norSizeMult = .01 * _PortalNoiseSize;
                float norAdder    = .1;

                float3 eye = v.world - _WorldSpaceCameraPos;
                normal     = v.nor;

                if ( dot( UNITY_MATRIX_V._m20_m21_m22 , normal ) < 0 )
                {
                    normal = -normal;
                }



                float3 norOffset =
                    +triNoise3D( v.world * norSizeMult , 1 , _Time.y ) * float3( 1 , 0 , 0 )
                    + triNoise3D( v.world * norSizeMult , 2 , _Time.y ) * float3( 0 , 1 , 0 )
                    + triNoise3D( v.world * norSizeMult , 3 , _Time.y ) * float3( 0 , 0 , 1 );

                normal += norOffset * norAdder;



                float3 refr = normalize( refract( normalize( v.eye ) , normalize( normal ) , 1 ) );


                col = texCUBE( _OtherWorldCubemap , refr );
                col += pow( length( norOffset ) , 10 ) * .3;

                float distToEdge = ( length( v.world - _BasePosition ) + length( norOffset ) * 10 ) - _PortalAmountShown * _PortalFadeNormalizer;

                if ( distToEdge > 0 )
                {
                    discard; //
                    //col = float3( 1 , 0 , 0 );
                }
                else
                {
                    if ( distToEdge > -1 )
                    {
                        col *= col * ( 100 * ( 2 - ( -distToEdge / 1 ) ) );
                    }
                }


                //col = normal * .5 + .5;

                //  col = v.t1 * .5 + .5;

                // col = normal * .5 + .5;
                //col = normalize( v.eye ) * .5 + .5;
                return float4( col , 1 );
            }
            ENDCG
        }
    }
}