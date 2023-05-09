// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/TrailFeathers" {
    Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _Size ("Size", float) = .01
      _MainTex("_MainTex", 2D) = "white" {}
      _SpriteSize("_SpriteSize",float) = 5
      _CubeMap("_CubeMap", Cube) = "white" {}
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

      
      sampler2D _ColorMap;
      sampler2D _MainTex;
      samplerCUBE _CubeMap;
      sampler2D _FullColorMap;

      float _SpriteSize;

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


#include "../Chunks/hash.cginc"


float4x4 rotation(float3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return float4x4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}

float3 newAxis( float id ){
  float3 a = float3(hash(id),hash(id*10),hash(id*20));
  return normalize(a * 2 - 1);
}


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

l = normalize(mul( rotation(newAxis(base),v.debug * 1 + _Time.y * 8*(.2 + hash(float(base) * 33.3))+ hash(float(base) * 131.13)), float4(1,0,0,0)));//normalize(v.vel);
u = normalize(mul( rotation(newAxis(base),v.debug * 1 + _Time.y * 8*(.2 + hash(float(base) * 33.3))+ hash(float(base) * 131.13)), float4(0,1,0,0)));//normalize(v.vel);
o.nor = normalize(mul( rotation(newAxis(base),v.debug * 1 + _Time.y * 8*(.2 + hash(float(base) * 33.3))+ hash(float(base) * 131.13)), float4(0,1,0,0)));//normalize(v.vel);
//l = normalize( cross( dir, u));
//u = normalize( cross( l, dir));


float3 p = v.pos;//mul(v.ltw , float4(0,0,0,1)).xyz;

// p = float3(0,1000,0);

float dieSize = 1/(1+(_Time.y- v.locked) * .1);

v.debug = saturate( v.debug );
float fSize = _Size  * min( v.debug , (1-v.debug) * 40) * dieSize; 
fSize *= fSize;

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


      float uvX = floor( ((sin( float(base) * 40400.) + 1)/2) * _SpriteSize  ) / _SpriteSize;
      float uvY = floor( ((sin( float(base) * 81409.) + 1)/2) * _SpriteSize  ) / _SpriteSize;


o.worldPos = p+extra*fSize;// mul(_Transform, float4((v.pos) ,1));
///o.worldPos +=  extra * _Size;
o.eye = _WorldSpaceCameraPos - o.worldPos;
o.uv2 = uv / _SpriteSize +float2(uvX, uvY);
o.life = v.debug;
o.id = base;
o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));


  return o;

}

#include "../Chunks/hsv.cginc"
      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {


  
          float4 cCol = texCUBE(_CubeMap, reflect(normalize(v.eye),v.nor)) * 3;
          float4 tCol = tex2D(_MainTex,v.uv2);
          float4 fCol = tex2D(_FullColorMap, float2( v.life * .1 , v.hue));

  

    float3 col = 1;
    
    col = cCol * fCol;

    col *= v.life * 4;

    if( tCol.x > .9){
      discard;
    }
    return float4(col,1 );
}

      ENDCG

    }
  }

  Fallback Off


}
