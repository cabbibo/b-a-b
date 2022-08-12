Shader "Unlit/skyboxDay1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
       	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
	Cull Off ZWrite Off
  

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
		#include "Lighting.cginc"
        #include "../Chunks/triNoise3D.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangent : TANGENT;
                fixed4 color : COLOR;
            };

        

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor      : TEXCOORD0;
          float3 tangent  : TEXCOORD5;
          float3 worldPos : TEXCOORD1;
          float3 eye      : TEXCOORD2;
          float3 debug    : TEXCOORD3;
          float2 uv       : TEXCOORD4;
          float4 color : TEXCOORD11;
      };




            sampler2D _MainTex;
            float4 _MainTex_ST;

            varyings vert (appdata v)
            {
                varyings o;

                UNITY_INITIALIZE_OUTPUT(varyings, o);     
                o.worldPos = mul( unity_ObjectToWorld,  float4(v.vertex.xyz,1)).xyz;
                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
                o.eye = _WorldSpaceCameraPos - o.worldPos;
                o.nor = normalize(mul( unity_ObjectToWorld,  float4(v.normal,0)).xyz);
                o.uv = v.uv.xy;
                o.color = v.color;
                return o;
            }

            fixed4 frag (varyings v) : SV_Target
            {
                // sample the texture

                float3 n = -normalize(v.eye);

                float3 pos = n * 10000;

                float3 fEye = _WorldSpaceCameraPos - pos;


float3 col = float3(.4,.4,1);


float vert = dot( n , float3(0,1,0));

float light = dot( n , _WorldSpaceLightPos0.xyz);

col *= vert;


                for( int i = 0; i < 5; i++ ){

                    float fi = float(i);

                    float3 fPos = pos + fEye  * fi * 100;

                    float val = triNoise3D( fPos * .000001 , 1 ,_Time.y);

                    val *= light;
                    if( val > .4){
                        col += 1-fi/5;
                        break;
                    };



                }



                float lp = dot(n,_WorldSpaceLightPos0.xyz);

                lp += triNoise3D( n * 1,3,_Time) * .1-.01;
                float lightPow = saturate(floor( pow(saturate(lp),30) *4)/4);



                



                //col = lerp( float3(.6,.6,.3) , float3(.2,.2,1),saturate(floor(n.y*10))/4);
                //col = lerp( col , float3(2,.7,.5)* lightPow,saturate(lightPow*1000));;//normalize(v.nor) * .5 + .5;



                return float4(col,1);
            }
            ENDCG
        }
    }
}
