Shader "Unlit/Cloud2"
{
    Properties
    {
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

            float _TimeInClip;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 rd : TEXCOORD2;
                float3 nor : TEXCOORD3;
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
                o.nor = normalize(mul( UNITY_MATRIX_M , float4(v.normal,0)).xyz);


                return o;
            }

            #include "../Chunks/noise.cginc"
            #include "../Chunks/hsv.cginc"

 
float sdBox( float3 p, float3 b ){

  float3 d = abs(p) - b;

  return min(max(d.x,max(d.y,d.z)),0.0) +
         length(max(d,0.0));

}
 
            fixed4 frag (v2f v) : SV_Target
            {

                fixed4 col = 0;
                int _NumSteps = 10;
                float dist = 100;
                 float dt = 1;
  float t = 0;
  float c = 0.;
float3 p = 0;


float totalSmoke = 0;
  float3 rd = normalize(v.rd);

  float dotMatch = dot(rd , v.nor);
    for(int i =0 ; i < _NumSteps; i++ ){

        t+=dt*exp(-2.*c);
        p = v.ro - rd * t * 2;

        float3 local = mul( unity_WorldToObject , float4(p , 1)).xyz;

        float box = sdBox(local , .5) * .4;
        
         p *= 2;
        float noiseDensity = noise( p * .01) *  .5 + noise(p * .05) * .3 + noise(p * .1) * .1;//noise( p * .01 ) + noise(p * .1) * box;//saturate(length(smoke) - _NoiseSubtractor);
         c *= 
        c = noiseDensity * (dotMatch* 1 + 0);      
        
        totalSmoke += c;

  

        col = .99*col + c  * 5;

    
    
  }

  col /= float(_NumSteps);

    totalSmoke = saturate(totalSmoke);
    
col = saturate(col);
    
  if( length(col) <= 0){
      discard;
  }

  //col.xyz = hsv(col.x *5,.5,1);
col = saturate(col);

//col = dotMatch;
                return col;
            }
            ENDCG
        }
    }

    FallBack "Unlit"
}
