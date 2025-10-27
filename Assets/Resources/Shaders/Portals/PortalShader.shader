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
            #include "Assets/Resources/Shaders/Chunks/snoise3D.cginc"

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

            float4 _Color;
            float  _Multiplier;

            fixed4 frag( v2f v ) : SV_Target
            {

                // Ddx ddy for nor

                float3 nor = normalize( cross( ddx( v.world ) , ddy( v.world ) ) );

                float3 flatNor = nor;


                nor = v.nor;
                if ( dot( UNITY_MATRIX_V._m20_m21_m22 , nor ) < 0 )
                {
                    nor = -nor;
                }



                // sample the texture
                float3 col = tex2D( _MainTex , v.uv );

                float3 normalTex = tex2D( _NormalMap , v.uv ).xyz * 2 - 1;
                float3 normal    = normalize( mul( v.TBN , normalTex ) );




                float norSizeMult = .01 * _PortalNoiseSize;
                float norAdder    = 1.5;

                float3 eye = v.world - _WorldSpaceCameraPos;
                // normal     = v.nor;



                float3 norOffset =
                    +triNoise3D( v.world * norSizeMult + 10 , 1 , _Time.y ) * float3( 1 , 0 , 0 )
                    + triNoise3D( v.world * norSizeMult + 333 , 1 , _Time.y ) * float3( 0 , 1 , 0 )
                    + triNoise3D( v.world * norSizeMult + 21.2 , 1 , _Time.y ) * float3( 0 , 0 , 1 );

                float speedMult = 4;
                float sizeMult  = .3;

                norOffset = snoise( v.world * sizeMult + float3( _Time.x * .9 * speedMult , 0 , 0 ) ) * float3( 1 , 0 , 0 ) + snoise( v.world * 1.3 * sizeMult + float3( 0 , _Time.x * .8 * speedMult , 0 ) ) * float3( 0 , 1 , 0 ) + snoise( v.world * 1.7 * sizeMult + float3( 0 , 0 , _Time.x * 1.1 * speedMult ) ) * float3( 0 , 0 , 1 );


                norOffset = normalize( norOffset );

                float m = dot( nor , norOffset );

                //norOffset = MapNormal( v.nor , v.t1 , v.t2 , v.t3 , v.uv , 1 ); // )mul( v.TBN , norOffset ); // normal += norOffset * norAdder;
                // normal += norOffset * .1;


                normal = nor;
                normal += norOffset * .2;
                normal = normalize( normal );

                float m2 = dot( -normal , normalize( v.eye ) );


                float3 refrR = normalize( refract( normalize( v.eye ) , normalize( normal ) , .95 ) );
                float3 refrG = normalize( refract( normalize( v.eye ) , normalize( normal ) , .9 ) );
                float3 refrB = normalize( refract( normalize( v.eye ) , normalize( normal ) , .85 ) );


                col   = texCUBE( _OtherWorldCubemap , refrR );
                col.g = texCUBE( _OtherWorldCubemap , refrG ).g;
                col.b = texCUBE( _OtherWorldCubemap , refrB ).b;

                col *= pow( m2 , 10 ) * 20 * v.uv.y * v.uv.y * v.uv.y; //abs( m );

                col *= _Color;
                col *= _Multiplier;

                //   col = nor * .5 + .5;
                //  col = flatNor * .5 + .5;
                // col += pow( length( norOffset ) , 10 ) * .3;

                float distToEdge = ( length( v.world - _BasePosition ) + length( norOffset ) * 10 ) - _PortalAmountShown * _PortalFadeNormalizer;


                /* if ( distToEdge > 0 )
                 {
                     discard; //
                     //col = float3( 1 , 0 , 0 );
                 }
                 else
                 {
                     if ( distToEdge > -1 )
                     {
                         // col *= col * ( 100 * ( 2 - ( -distToEdge / 1 ) ) );
                     }
                 }*/

                if ( v.uv.y < 0 )
                {
                    discard;
                }

                //col *= ( v.uv.y - norOffset.y * .5 ) * 5;


                //col = normal * .5 + .5;

                //  col = v.t1 * .5 + .5;

                // col = normal * .5 + .5;
                //col = normalize( v.eye ) * .5 + .5;

                return float4( col , 1 );
            }
            ENDCG
        }

    }

    Fallback "Diffuse"
}