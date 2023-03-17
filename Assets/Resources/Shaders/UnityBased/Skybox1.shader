
Shader "Terrain/Skybox1"
{

    Properties {

  
    _MainTex("_MainTex", 2D) = "white" {}
    _MapScale("MapScale", float) = 1
    _Fade("_Fade", float) = 1
    _CubeMap("_CubeMap" ,Cube) = "white" {}
    }


  SubShader{

            // Draw ourselves after all opaque geometry
        Tags { "Queue" = "Geometry+10" }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }

      Cull Off
    Pass{
CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      
    float4 _BaseColor;
    float4 _SampleColor;
    int _NumSteps;
    float _Opaqueness;
    float _ColorMultiplier;
    float _RefractionBackgroundSampleExtraStep;
    float _IndexOfRefraction;

    float _ReflectionSharpness;
    float _ReflectionMultiplier;
    float4 _ReflectionColor;

    sampler2D _SampleTexture;
    float _SampleSize;

    sampler2D _AudioMap;
    samplerCUBE _CubeMap;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor : NORMAL;
          float3 ro : TEXCOORD1;
          float3 rd : TEXCOORD2;
          float3 eye : TEXCOORD3;
          float3 localPos : TEXCOORD4;
          float3 worldNor : TEXCOORD5;
          float3 lightDir : TEXCOORD6;
          float4 grabPos : TEXCOORD7;
          float3 unrefracted : TEXCOORD8;
          
          
      };


            sampler2D _BackgroundTexture;


             struct appdata
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
            };

//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert ( appdata vertex ){



  varyings o;
     float4 p = vertex.position;
     float3 n =  vertex.normal;//_NormBuffer[id/3];

        float3 worldPos = mul (unity_ObjectToWorld, float4(p.xyz,1.0f)).xyz;
        o.pos = UnityObjectToClipPos (float4(p.xyz,1.0f));
        o.nor = n;//normalize(mul (unity_ObjectToWorld, float4(n.xyz,0.0f)));; 
        o.ro = p;//worldPos.xyz;
        o.rd  = mul(unity_ObjectToWorld, vertex.position).xyz - _WorldSpaceCameraPos;
        o.localPos = p.xyz;
      


      
  return o;

}



float3 hsv(float h, float s, float v)
{
  return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
    h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
}


float _MapScale;
float _Fade;
sampler2D _MainTex;
#include "../Chunks/noise.cginc"


//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
  float3 col =0;//hsv( float(v.face) * .3 , 1,1);

   float3 bf = normalize(abs(v.rd));
            bf /= dot(bf, (float3)1);

            float scale = 20;

    float3 rd = normalize(v.rd);

    float3 m = dot( _WorldSpaceLightPos0.xyz , rd );

    float2 tx = rd.zy * _MapScale;
    float2 ty = rd.xz * _MapScale;
    float2 tz = rd.xy * _MapScale;

    float n = noise( v.rd * .0002 ) +  .4 * noise (v.rd * .0006) + .1 * noise(v.rd * .001)  ;//* .3 + noise(v.rd * .0001) * .6 + noise(v.rd * .0003);


    half4 cx = tex2D(_MainTex, tx )* bf.x;
    half4 cy = tex2D(_MainTex, ty )* bf.y;
    half4 cz = tex2D(_MainTex, tz )* bf.z;
    col = (cx + cy + cz).xyz;
    //col *= 10;

    float sunCol = pow( saturate(m),1000) * 20;
    //col = hsv(col.x * .2 + sunCol + dot( _WorldSpaceLightPos0.xyz , float3(0,-1,0)), 0, saturate(col.x * 5- 5*saturate(sunCol)));
    //col += hsv(dot( _WorldSpaceLightPos0.xyz , float3(0,-1,0)),1,saturate(sunCol) ) * 11.8;

    ///col *= tex2D(_AudioMap,n * .1);

    col *= _Fade;
    col = saturate(col);

    float3 starCol = col;

    float a = atan2( rd.x , rd.z);

    float v1 = rd.y + (.1*sin( a * 10 + _Time.x + sin( a * 3 + _Time.y) * .1 )+ .1*sin( a * 20 +12.3 + _Time.x * .3 + sin( a * 44 + _Time.y) * .1 ) ) * .1;;//- .01*sin( _Time.y + sin(rd.x * 20 + _Time.y))- .01*sin( _Time.y  +1321+ sin(rd.y * 15 + sin( _Time.y) * 10 + _Time.y));

    col /= (.1 + 10 *abs(v1));

   // rd.y = v1;


   // col = (col *.9 + 0) *texCUBE( _CubeMap , rd );
   // col += texCUBE( _CubeMap , rd ).b *.1* v1*v1*v1 * float3(1,.8,.6);
   // rd.y = abs(rd.y);
    col = pow( texCUBE( _CubeMap , float3(rd.x , v1 , rd.y + rd.z * .3 ) ).r ,1) * 3;//pow(texCUBE( _CubeMap , rd ).b ,10) * 100;
       // col /= (.03 + 1 *abs(v1));
        
        col = saturate( col) * 20;
       // col *= col * col * col * col; 
        
       // col.xyz += starCol;

    col *= pow( texCUBE( _CubeMap , rd ) ,1) * 10;//pow(texCUBE( _CubeMap , rd ).b ,10) * 100;

  col *= _Fade;
        //col = sin(atan2( rd.x , rd.z) * 10) * .1;
    return float4( col.xyz, 1);//saturate(float4(col,3*length(col) ));




}

      ENDCG

    }
  }

  Fallback Off


}