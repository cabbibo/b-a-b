// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/RaceParticles" {
    Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _ParticleSize ("Size", float) = .01
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
      #include "../Chunks/hash.cginc"


      uniform int _Count;
      uniform float _ParticleSize;
      uniform float3 _Color;
struct Particle{
  float3 pos;
  float3 vel;
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


//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / 6;
  int alternate = id %6;


      float3 extra = float3(0,0,0);

float3 u = UNITY_MATRIX_V[0].xyz;
float3 l =UNITY_MATRIX_V[1].xyz;


    float2 uv = float2(0,0);

    Particle p = _ParticleBuffer[base];

    float hID = hash( float(base) * 10131.41);
    hID *= 4; 
    hID = floor( hID );

    float hue = _Hue1;

    if( hID == 1){ hue = _Hue2; } 
    if( hID == 2){ hue = _Hue3; } 
    if( hID == 3){ hue = _Hue4; } 

    o.hue = hue;

    


    float3 v =p.pos;

    float fSize = _ParticleSize * min(p.life,1-p.life); 

    if( alternate == 0 ){ extra = v +( -u-l) *fSize  ; uv = float2(0,0); }
    if( alternate == 1 ){ extra = v +( -u+l)*fSize  ; uv = float2(1,0); }
    if( alternate == 2 ){ extra = v +(+u+l)*fSize  ; uv = float2(1,1); }
    if( alternate == 3 ){ extra = v +( -u-l)*fSize  ; uv = float2(0,0); }
    if( alternate == 4 ){ extra = v +(+u+l) *fSize ; uv = float2(1,1); }
    if( alternate == 5 ){ extra = v +(+u-l)  *fSize; uv = float2(0,1); }

  o.uv = uv;
      o.worldPos = extra;
      o.id = float(base) * .01;
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  

  return o;

}


      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {


  float rVal = length(v.uv - .5); 
  if(rVal > .5 ){
    discard;
  }
 // if( rVal < .4 ){discard;}


    return float4(hsv(v.hue,1.,1),1);
}

      ENDCG

    }
  }

  Fallback Off


}
