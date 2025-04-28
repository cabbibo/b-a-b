Shader "Unlit/OutlineSphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,1,1)
        _LastHitLocation ("_LastHitLocation", Vector) = (.5,.5,0,0)

        _Active("_Active",float) = 0
        _CurrentScore("_CurrentScore",float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue" ="Transparent"
        }
        LOD 100

        Blend One One
        Cull Front
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //   #pragma multi_compile_instancing
            // make fog work

            #include "UnityCG.cginc"


            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float3 localPos : TEXCOORD3;
                float2 wrenPos : TEXCOORD4;
                float3 nor : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;

            #include "../Chunks/terrainData.cginc"
            #include "../Chunks/noise.cginc"
            #include "../Chunks/snoise.cginc"
            #include "../Chunks/hsv.cginc"

            float3    _WrenPos;
            sampler2D _FullColorMap;
            sampler2D _AudioMap;


            float _CurrentScore;

            float distanceToLine( float2 pt1 , float2 pt2 , float2 testPt )
            {
                float2 lineDir  = pt2 - pt1;
                float2 perpDir  = float2( lineDir.y , -lineDir.x );
                float2 dirToPt1 = pt1 - testPt;
                return abs( dot( normalize( perpDir ) , dirToPt1 ) );
            }


            v2f vert( appdata_full v )
            {
                v2f o;

                o.worldPos = mul( unity_ObjectToWorld , float4( v.vertex.xyz , 1 ) ).xyz;
                o.pos      = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );


                o.localPos = mul( unity_WorldToObject , float4( _WrenPos , 1 ) ).xyz;
                o.nor      = normalize( mul( unity_ObjectToWorld , float4( v.normal.xyz , 0 ) ).xyz );

                o.uv = v.texcoord; //TRANSFORM_TEX(v.texcoord, _MainTex);


                return o;
            }

            float4 _Color;
            float  _Active;

            float2 _LastHitLocation;
            fixed4 frag( v2f v ) : SV_Target
            {

                float r = length( v.uv - .5 ) * 2;

                float2 nV = v.uv - .5;
                float  a  = atan2( nV.y , nV.x );



                //  float2 delta

                float nScore = saturate( _CurrentScore / 1000 );

                float n = noise( v.worldPos );
                float h = terrainHeight( v.worldPos );

                float3 fwd = normalize( mul( unity_ObjectToWorld , float4( 0 , 0 , 1 , 0 ) ).xyz );

                if ( h < 0 )
                {
                    // discard;
                }

                float speed = _Time.y * ( _Active * 3 + 1 );
                // sample the texture
                fixed3 col = ( sin( a * 10 + n + speed ) + 1 ) / 2; //tex2D(_MainTex, v.uv);

                col = 0; //.5;

                bool circle = length( ( v.uv.xy - .5 ) - v.localPos.xy ) > .1 / ( 1 + abs( v.localPos.z ) );
                bool rim    = length( v.uv - .5 ) > .47 && length( v.uv - .5 ) < .5;

                float aVal = ( length( v.uv - .5 ) - .47 ) / .03;

                //  col.xyz = tex2D( _AudioMap , float2(aVal,0)).xyz;




                //  col = _CurrentScore;



                // if( length( nV ) < .05 ){
                // rim = true;



                float3 eye = _WorldSpaceCameraPos - v.worldPos;
                for ( int k = 0; k < 3; k++ )
                {

                    float3 fPos = v.worldPos - normalize( eye ) * float( k ) * .7;
                    fPos        = mul( unity_WorldToObject , float4( fPos , 1 ) ).xyz;
                    float2 fUV  = float2( fPos.x , fPos.y );
                    //col.r += 1/(1+abs(length(fPos.xy)-.5) * 100);
                    //col.g += 1/(1+abs(length(fPos.xy)-.49) * 150);
                    //col.b += 1/(1+abs(length(fPos.xy)-.48) * 200);
                    float uvL = length( fUV );

                    uvL += noise( fPos * 2 ) * .1;


                    if ( uvL > .5 )
                    {
                        //  col -= .1;//discard;
                    }

                    float3 colMultiplier = float3( 1 , 0 , 0 );
                    if ( k == 1 ) { colMultiplier = float3( 0 , 1 , 0 ); }
                    if ( k == 2 ) { colMultiplier = float3( 0 , 0 , 1 ); }

                    col += colMultiplier * .8 / ( 1 + pow( abs( uvL - .49 ) * 10 , 4 ) * 100 );


                    col += colMultiplier * 1 / ( 1 + uvL * 1000 );
                    col += colMultiplier * 1 / ( 1 + abs( uvL - .05 ) * 1000 );


                    float2 delta = _LastHitLocation - fUV;

                    float aDelta = ( dot( normalize( _LastHitLocation ) , normalize( fUV ) ) + 1 ) / 2;
                    col += nScore * 4 * colMultiplier * pow( aDelta , 100 * nScore ) * saturate(
                        ( length( _LastHitLocation ) - uvL ) * 100 );
                    col += nScore * 4 * colMultiplier / ( 1 + pow( length( fUV - _LastHitLocation ) * 10 , 4 ) );
                    //pow( aDelta , 100) * saturate( ( length(_LastHitLocation) - uvL) * 100);
                    col += nScore * 4 * colMultiplier / ( .1 + pow( length( fUV - _LastHitLocation ) * 30 ,
                                    14 * nScore + 1 ) );
                    //pow( aDelta , 100) * saturate( ( length(_LastHitLocation) - uvL) * 100);



                    float wrenDist = length( fUV - v.localPos.xy );
                    col += abs( v.localPos.z ) / ( .2 + 100 * pow( wrenDist , 1 / abs( v.localPos.z ) ) );


                }
                //  }





                float3 shadowCol = 0;
                for ( int j = 0; j < 3; j++ )
                {

                    float3 fPos = v.worldPos - normalize( eye ) * float( j ) * .7;
                    float  v    = ( ( snoise( fPos * 10 ) ) + 1 ) / 2;
                    shadowCol += hsv( (float)j / 3 , 1 , v );


                } //
                shadowCol = pow( shadowCol , 40 );
                col *= ( length( shadowCol ) * 100 + .5 ) * ( shadowCol * .5 + .5 );


                float fNV = length( nV );

                float nVal = noise( v.worldPos * 2 + fwd * _Time.y ) * .1;
                fNV += noise( v.worldPos * 2 + fwd * _Time.y ) * .1;
                if ( fNV > .5 )
                {
                    //  discard;
                }




                float ringFade = .45;
                if ( fNV > ringFade )
                {
                    col *= pow( 1 - ( fNV - ringFade ) / ( .5 - ringFade ) , 2 );
                }


                //col = 1;


                float eyeM = dot( normalize( eye ) , v.nor );
                eyeM += nVal * 2;
                col = pow( 1 + eyeM , 20 );

                if ( length( col ) < .1 )
                {
                    discard;
                }
                else
                {

                    col *= length( shadowCol * 100 );
                }

                // col *= pow( length(shadowCol * .3) + length(col * .3) , 10) * 100;// * shadowCol;
                //  col =  dot( _LastHitLocation , nV);//atan2( delta.y , delta.x)/6.28; //length(_LastHitLocation-nV);
                //  col = ((a + 3.14  * 1.5)%6.28) / 6.28;

                // apply fog
                //UNITY_APPLY_FOG(v.fogCoord, col);

                //col = float3(1,0,1);;

                col = saturate( col * .3 ) / .3;

                return float4( col , 1 );
            }
            ENDCG
        }
    }
}