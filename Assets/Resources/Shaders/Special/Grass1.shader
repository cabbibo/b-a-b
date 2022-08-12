// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/Grass1" {
    Properties {

    _Color ("Color", Color) = (1,1,1,1)
    
    _MainTex ("Texture", 2D) = "white" {}
    _ParticleSize ("Size", float) = .01
    _Ratio ("Ratio", float) = 8
    }


  SubShader{
    Cull Off
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "../Chunks/hsv.cginc"
      #include "../Chunks/noise.cginc"


      uniform int _Count;
      uniform float _ParticleSize;
      uniform float3 _Color;
struct Particle{
  float3 pos;
  float3 nor;
  float life;
  float id;
};

StructuredBuffer<Particle> _ParticleBuffer;
      


      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor      : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 eye      : TEXCOORD2;
          float3 debug    : TEXCOORD3;
          float2 uv       : TEXCOORD4;
          float2 uv2       : TEXCOORD6;
          float id        : TEXCOORD5;
          float hue : TEXCOORD7;
      };


uniform float4x4 _Transform;

float _Hue1;
float _Hue2;
float _Hue3;
float _Hue4;

#include "../Chunks/TerrainData.cginc"

float _Ratio;
int _NumRows;
uniform sampler2D _PaintTexture;
uniform sampler2D _MainTex;

//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / 6;
  int alternate = id %6;

  int whichRow = base %_NumRows;
  base = base / _NumRows;


      float3 extra = float3(0,0,0);
    Particle p = _ParticleBuffer[base];

float3 u = p.nor.xyz;
float3 l =normalize(cross(u,UNITY_MATRIX_V[2].xyz));
  

    float2 uv = float2(0,0);


    float hID = hash( float(base) * 10131.41);
    hID *= 4; 
    hID = floor( hID );

    float hue = _Hue1;

    if( hID == 1){ hue = _Hue2; } 
    if( hID == 2){ hue = _Hue3; } 
    if( hID == 3){ hue = _Hue4; } 

    o.hue = hue;

    


   float ratio = _Ratio;
    float fSize = _ParticleSize * _ParticleSize * saturate(min(p.life,100*(1-p.life))); 

    float3 v = p.pos + u* float(whichRow) * fSize * ratio;

    //float fSize = _ParticleSize * pow( length(_WorldSpaceCameraPos - v) , .5); 
 

    if( alternate == 0 ){ extra = v + (  -l ) *fSize  ; uv = float2(0,float(whichRow)/ float(_NumRows)); }
    if( alternate == 1 ){ extra = v + (  +l )*fSize  ; uv = float2(1,float(whichRow)/ float(_NumRows)); }
    if( alternate == 2 ){ extra = v + ( +u*ratio +l )*fSize  ; uv = float2(1,float(whichRow+1)/ float(_NumRows)); }
    if( alternate == 3 ){ extra = v + ( -l )*fSize  ; uv = float2(0,float(whichRow)/ float(_NumRows)); }
    if( alternate == 4 ){ extra = v + ( +u*ratio +l ) *fSize ; uv = float2(1,float(whichRow+1)/ float(_NumRows)); }
    if( alternate == 5 ){ extra = v + ( +u*ratio -l )  *fSize; uv = float2(0,float(whichRow+1)/ float(_NumRows)); }


    extra += l * noise( extra * .3 + float3(0,_Time.y * 2,0) );
  
    hID = hash( float(base) * 1131.41);
    hID *= 6; 
    hID = floor( hID );

    uv.x = uv.x/6 + hID/6;

      o.uv = uv;
      o.worldPos = extra;
      o.nor = p.nor;
      o.id = float(base) * .01;
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  

  return o;

}


      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {


 /* float rVal = length(v.uv - .5); 
  if(rVal > .5 ){
    discard;
  }*/
 // if( rVal < .4 ){discard;}  float4 paintCol = tex2D(_PaintTexture,v.uv);
 
    float m2 = 1.2-dot( float3(0,1,0) , v.nor );
float3 col = lerp( m2 , v.nor * .5 + .5 , v.uv.y);
 
  
  float4 paintCol = tex2D(_PaintTexture,normalizedTerrainPos(v.worldPos).xz);
  float4 tCol = tex2D(_MainTex,v.uv);
   col = hsv(paintCol.x , 1 , paintCol.z);
   col = lerp( hsv(paintCol.x , .3 , 1 ), col , 1-v.uv.y);
   col *= tCol;
   if( tCol.a < .3 ){
     discard;
   }
   //col += saturate((gridVal-1.9)*10);

    return float4(col,1);
}

      ENDCG

    }
  }

  Fallback Off


}
