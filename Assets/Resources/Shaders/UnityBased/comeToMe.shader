Shader "Unlit/ComeToMe2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
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


            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal :NORMAL;
                float3 color :COLOR;
                float2 uv : TEXCOORD0;
                uint id : SV_VERTEXID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float id : TEXCOORD2;
                float3 nor : NORMAL;
                float3 world : TEXCOORD3;
                float3 col : TEXCOORD4;
            };

            sampler2D _MainTex;
            float3 _Color;
            float4 _MainTex_ST;

            #include "../chunks/noise.cginc"
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul( unity_ObjectToWorld , float4( v.vertex.xyz,1)).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.nor = normalize(mul( unity_ObjectToWorld , float4( v.normal,0)));
                o.col = v.color;
                int id = v.id / 4;

                o.id = float(id);

                o.uv = (v.uv * 1/6) + float2( floor(hash( o.id * 12 ) * 6 ) /6, floor(hash( o.id  * 30) * 6 ) /6 );


                return o;
            }

            #include "../chunks/hsv.cginc"

            fixed4 frag (v2f v) : SV_Target
            {

                float3 col = 1;

                /*float l = length(v.uv-.5);

                l += noise( float3(v.uv * 10,_Time.x*2) ) * .1;
                float4 col = 1;
                
                if( l > .5 ){
                   discard;
                }

                if( l < .2 ){
                    discard;
                }

                float inOuterRing = saturate((l - .4) * 10);

                inOuterRing *= inOuterRing;
                inOuterRing *= 10;

                

                float ring = floor( inOuterRing );
                inOuterRing %= 1;
                if( inOuterRing < .3 ){ discard;}

                float lightness = .5;
                float whichRing =9-(floor( ((_Time.y * .5) * 10) % 10));
                
                
                if( ring == whichRing ){
                    lightness = 1;
                }

                
*/

float3 eye = _WorldSpaceCameraPos - v.world;

float3 shadowCol = 0;
 for( int i = 0; i < 3; i++){

      float3 fPos = v.world - normalize(eye) * float(i) * 1.3;
      float v = (noise(fPos * 40));
      shadowCol += hsv((float)i/3,1,v);

    
    }//

        shadowCol = pow( shadowCol,5);

        shadowCol = length(shadowCol) * (shadowCol* .8 + .2)  * 10 * float3(1, .8,.3);


                    half3 worldViewDir = normalize(UnityWorldSpaceViewDir(v.world));
         half3 worldRefl = reflect(-worldViewDir, v.nor);
         half4 skyData =UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
         half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);

                float4 tCol = tex2D(_MainTex, v.uv);
                //col.xyz = hsv( l - .4 , 1,lightness );

                col.xyz = v.col;// (v.nor * .5 +.5) * hsv( v.id / 100, 1,1 );

                col += v.col * shadowCol ;

              //  col *= v.col;
if( tCol.x > .9){
    discard;
}


//col = skyColor;
                return float4(col,1);;
            }
            ENDCG
        }
    }
}
