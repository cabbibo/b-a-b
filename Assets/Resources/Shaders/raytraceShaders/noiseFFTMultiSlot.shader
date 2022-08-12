Shader "Unlit/noiseFFTArray"
{
    Properties
    {
        _AudioTex ("Audio Texture", 2D) = "white" {}
        _MainTex ("Main Texture", 2D) = "white" {}
        _AudioTexArray ("Tex", 2DArray) = "" {}
        _SliceRange ("Slices", Range(0,9)) = 9
        _TimeInClip ("TimeInClip", Range(0.0001,.99999)) = .5
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
            #pragma require 2darray

            #include "UnityCG.cginc"
            #include "../Chunks/triNoise3D.cginc"

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


            
            float _UnscaledTime;
            UNITY_DECLARE_TEX2DARRAY(_AudioTexArray);

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.ro = v.vertex.xyz;//mul( UNITY_MATRIX_M , v.vertex).xyz;
                o.rd = normalize(mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1)).xyz - o.ro);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);


                return o;
            }
float3 hsv(float h, float s, float v)
{
  return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
    h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
}


float4 audioSamp( float freq , float time , float fft){
    return UNITY_SAMPLE_TEX2DARRAY(_AudioTexArray,float3(freq,time,fft));
}

float _SliceRange;

            fixed4 frag (v2f v) : SV_Target
            {

                fixed4 col = 0;
                float _NumSteps = 30;
                float dist = 1;

                bool hit = false;


                  float dt = 10;
                float t = 0;
                float c = 0.;
                float3 p = 0;



                for( float i  = 0;  i < _NumSteps; i++ ){

                 

                    
                        t+=dt*exp(-2.*c * 10 );

                        float3 pos = v.ro - v.rd * t;
                        //float d = 0;
                        float dist =  tex2D(_MainTex, (pos.xy * float2(-1,1)) * .1 +.5).a;
                        //dist = (.5- abs(.5-dist)) * 1;

                       pos *= .8;
                        //float d1 = snoise( pos * .1 +  float3(0,0,1) * _Time.y * .1);
                        //float d2 = snoise( pos * .4 +  float3(0,0,1) * _Time.y * .2);
                        //float d3 = snoise( pos * 1 +  float3(0,0,1) * _Time.y * .2);
//
                        //float4 aVal =  audioSamp( dist * .3 + d1 * .1 ,_TimeInClip + .0,0);//tex2D(_AudioTex, float2( dist * .3 + d1 * .1 , _TimeInClip + .01));
                        //float4 aVal1 =  audioSamp( dist * .3 + d2 * .1 ,_TimeInClip + .0,1);//tex2D(_AudioTex, float2( dist * .3 +d2  * .1 + .1, _TimeInClip + .01));
                        //float4 aVal2 =  audioSamp( dist * .3 + d3 * .1 ,_TimeInClip + .0,2);//tex2D(_AudioTex, float2(dist * .3 + d3  * .1+ .2 , _TimeInClip + .01));
//

                        float c = triNoise3D( pos *.001, 1, _UnscaledTime);
                        c *= c * c * 5;
                        //c -= .4;

                     
                       /* for( float id = 0; id < _SliceRange; id++ ){

                            float lookup = abs(c - id/_SliceRange) * .5;
                            lookup %= 1;
                            float4 aVal =  audioSamp(  lookup,_TimeInClip + .00,id)  * (lookup*2. +.2);
                        
                            //if( c > 0 ){
                                col.xyz += 100.1*hsv(t * .3 ,1,1) * (c+.3) * aVal.xyz*aVal.xyz;
                            //}

                        }*/

                        col.xyz += hsv(c * .6+ .8,.6,c*4);
                      

                        //col = dist;
             


                }

                col /= float(_NumSteps);


                // sample the texture
                fixed4 aCol = tex2D(_AudioTex, float2(v.uv.x , _TimeInClip+ .01));
                fixed4 tCol = tex2D(_MainTex, v.uv);

                float4 aVal =  audioSamp( v.uv.y , _TimeInClip+ .01,v.uv.x * _SliceRange);
                 // col = aVal;
                return col;
            }
            ENDCG
        }
    }
}
