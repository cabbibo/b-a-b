Shader "Unlit/noiseFFT"
{
    Properties
    {
        _AudioTex ("Audio Texture", 2D) = "white" {}
        _MainTex ("Main Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "../Chunks/snoise.cginc"

            float _TimeInClip;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 rd : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _AudioTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.ro = mul( UNITY_MATRIX_M , v.vertex).xyz;
                o.rd = _WorldSpaceCameraPos - o.ro;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);


                return o;
            }
float3 hsv(float h, float s, float v)
{
  return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
    h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
}
            fixed4 frag (v2f v) : SV_Target
            {

                fixed4 col = 0;
                float _NumSteps = 1000;
                float dist = 2;

                bool hit = false;
                for( float i  = 0;  i < _NumSteps; i++ ){


                    if( hit == false ){

                        float val = i/_NumSteps;
                        float3 pos = v.ro - v.rd * val * dist;
                        float d = 0;
                        float dist =  tex2D(_MainTex, (pos.xy * float2(-1,1)) * .1 +.5).a;
                        //dist = (.5- abs(.5-dist)) * 1;

                        pos *= .6;
                        float d1 = snoise( pos * .1 +  float3(0,0,1) * _Time.y * .1);
                        float d2 = snoise( pos * .4 +  float3(0,0,1) * _Time.y * .2);
                        float d3 = snoise( pos * 1 +  float3(0,0,1) * _Time.y * .2);

                        float4 aVal =  tex2D(_AudioTex, float2( dist * .3 + d1 * .1 , _TimeInClip + .01));
                        float4 aVal1 =  tex2D(_AudioTex, float2( dist * .3 +d2  * .1 + .1, _TimeInClip + .01));
                        float4 aVal2 =  tex2D(_AudioTex, float2(dist * .3 + d3  * .1+ .2 , _TimeInClip + .01));

                        if( d1 > 0 || d2 > 0 || d3 > 0 ){

                            col += pow((aVal1 + aVal2 + aVal),3) * 3;
                            
                            //col.xyz +=20 *(val *val+ .6)* 3.2 * hsv( length(aVal) *2  + .5 , 1,1) * length(aVal) * .1;
                            //col.xyz += hsv(dist,1,1) * 20* dist * aVal;
                            //break;
                            hit = false;
                        }

                        //col = dist;
                    }


                }

                col /= float(_NumSteps);

                // sample the texture
                fixed4 aCol = tex2D(_AudioTex, float2(v.uv.x , _TimeInClip+ .01));
                fixed4 tCol = tex2D(_MainTex, v.uv);
                return col;
            }
            ENDCG
        }
    }
}
