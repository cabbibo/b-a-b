// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/TrailFeathers" {
    Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _Size ("Size", float) = .01
    }


  SubShader{
    Cull Off
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"



      uniform int _Count;
      uniform float _Size;
      uniform float3 _Color;

      uniform float _Hue1;
      uniform float _Hue2;
      uniform float _Hue3;
      uniform float _Hue4;
struct Feather{
  float3 pos;
  float3 vel;
  float featherType;
  float locked;
  float4x4 ltw;
  float3 ogPos;
  float3 ogNor;
  float touchingGround;
  float debug;
};

StructuredBuffer<Feather> _FeatherBuffer;


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
          float life        : TEXCOORD7;
          float hue : TEXCOORD8;

      };

#include "../Chunks/hash.cginc"
uniform float4x4 _Transform;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / 6;
  int alternate = id %6;


float3 extra = float3(0,0,0);

float2 uv = float2(0,0);


Feather v = _FeatherBuffer[base];

float3 u = UNITY_MATRIX_V[0].xyz;
float3 l =UNITY_MATRIX_V[1].xyz;


float3 p = v.pos;//mul(v.ltw , float4(0,0,0,1)).xyz;

// p = float3(0,1000,0);

float fSize = _Size;//  * min( v.debug * 20 , 1-v.debug); 

if( alternate == 0 ){ extra =( - l - u); uv = float2(0,0); }
if( alternate == 1 ){ extra =( + l - u); uv = float2(1,0); }
if( alternate == 2 ){ extra =( + l + u); uv = float2(1,1); }
if( alternate == 3 ){ extra =( - l - u); uv = float2(0,0); }
if( alternate == 4 ){ extra =( + l + u); uv = float2(1,1); }
if( alternate == 5 ){ extra =( - l + u); uv = float2(0,1); }


int whichHue = int( floor( hash( float( base * 1000 )) *4));
o.hue = _Hue1;
if( whichHue == 1 ){ o.hue = _Hue2;}
if( whichHue == 2 ){ o.hue = _Hue3;}
if( whichHue == 3 ){ o.hue = _Hue4;}

o.worldPos = p+extra*fSize;// mul(_Transform, float4((v.pos) ,1));
///o.worldPos +=  extra * _Size;
o.eye = _WorldSpaceCameraPos - o.worldPos;
o.uv2 = uv;
o.life = v.debug;
o.id = base;
o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));


  return o;

}

#include "../Chunks/hsv.cginc"
      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {

    if( length(v.uv2-.5) > .5 ){
        discard;
    }

    float3 col = 1;
    col = hsv(_Hue1, 1,1);
    return float4(col,1 );
}

      ENDCG

    }
  }

  Fallback Off


}
