// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/FeatherLinePoints" {
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

#include "../Chunks/cubicCurve.cginc"

#include "../Chunks/CubicValues.cginc"
#include "../Chunks/snoise.cginc"

int _NumScapularRows;


float4x4 _Shoulder;
float4x4 _Elbow;
float4x4 _Hand;
float4x4 _Finger;
float4x4 _Chest;

float _Locked;
float _LeftOrRight;

float _ScaleMultiplier;
int _NumLesserCovertRows;
int _NumLesserCovertCols;
int _NumPrimaryFeathers;
int _NumPrimaryCoverts;

float _BackAmountOverlapping;
float _BaseDirectionLeftRightNoise;
float _BaseDirectionUpNoise;
float _BaseNoiseSize;
float _BaseNoiseScale;

float _MiddlePrimaryFeatherScaleMultiplier;
float _BasePrimaryFeatherScale;

float _MiddleSecondaryFeatherScaleMultiplier;
float _BaseSecondaryFeatherScale;

float _MiddleCovertsFeatherScaleMultiplier;
float _BaseCovertsFeatherScale;

float3 _Velocity;

float _Explosion;
float3 _ExplosionVector;float _LockStartTime;


float _ResetValue;

float _NoiseSizeForFlutter;
float _MaxFlutter;
float _MinFlutter;
float _MaxFlutterSpeed;
float _MinFlutterSpeed;

float _ReturnToLockTime;
float _ReturnToLockForce;
float _ReturnToLockTimeMultiplier;

float _LockDistance;
float _LockLerp;

float _ExplosionOutForce;
float _ExplosionUpForce;
float _ExplosionVelForce;

float3 _GroundNor;


float _VortexInForce;
float _VortexCurlForce;
float _VortexNoiseForce;
float _VortexNoiseSize;


float _GroundVortexHeight;
float _GroundVortexForce;

float _GroundLockHeight;
float _GroundLockForce;



float3 v1;
float3 v2;


float3 shoulder; float3 shoulderU; float3 shoulderR; float3 shoulderF;
float3 elbow; float3 elbowU; float3 elbowR; float3 elbowF;
float3 hand; float3 handU; float3 handR; float3 handF;
float3 finger; float3 fingerU; float3 fingerR; float3 fingerF;
float3 chest; float3 chestU; float3 chestR; float3 chestF;

void setFromMatrix( float4x4 mat , out float3 p , out float3 r, out float3 u  , out float3 f){
    p = mul( mat , float4( 0, 0, 0, 1)).xyz;
    r = _ScaleMultiplier * normalize(mul( mat , float4( 1, 0, 0, 0)).xyz) * _LeftOrRight ;
    u = _ScaleMultiplier * normalize(mul( mat , float4( 0, 1, 0, 0)).xyz);
    f = _ScaleMultiplier * normalize(mul( mat , float4( 0, 0, 1, 0)).xyz);
} 


void SetUpValues(){
    setFromMatrix(_Hand,hand,handR,handU,handF);
    setFromMatrix(_Chest,chest,chestR,chestU,chestF);
    setFromMatrix(_Elbow,elbow,elbowR,elbowU,elbowF);
    setFromMatrix(_Shoulder,shoulder,shoulderR,shoulderU,shoulderF);
    setFromMatrix(_Finger,finger,fingerR,fingerU,fingerF);
}



#include "../Chunks/WingShaderFeatherValues.cginc"

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
          float featherType : TEXCOORD9;
          float inFeatherID : TEXCOORD10;

      };



uniform float4x4 _Transform;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / 6;
  int alternate = id %6;

  int idR = base / 100;
  int idC = base % 100;

  float3 fPos1; float3 fNor; float3 fFwd; 
  float3 fPos2;


  float rowVal = float(idR) / 101;
  float colVal = float(idC) / 101;



  SetUpValues();
 
   // offset!
  rowVal += colVal * .5;
  rowVal %= 1;

   rowVal *= .95;
   rowVal += .025;

   
   colVal *= .5;
   colVal += .025;
 
  lesserCovertsCubic(rowVal ,  colVal ,fPos1,fNor,fFwd);
  lesserCovertsCubic(rowVal , colVal + .03,fPos2,fNor,fFwd);

  
float3 d = (fPos2 - fPos1);
    float3 u = normalize(cross( UNITY_MATRIX_V[2].xyz , normalize(d)));




float2 uv = float2(0,0);

    
    float3 p1 = fPos1 + d * .1 - u *2*_Size;
    float3 p2 = fPos1 + d * .1 + u *2*_Size;
    float3 p3 = fPos2 - d * .1 - u * .2* _Size;
    float3 p4 = fPos2 - d * .1 + u * .2* _Size;
    

    o.featherType = 1;
    float3 extra;

    if( alternate == 0 ){ extra = p1; uv = float2(0,0); }
    if( alternate == 1 ){ extra = p2; uv = float2(1,0); }
    if( alternate == 2 ){ extra = p4; uv = float2(1,1); }
    if( alternate == 3 ){ extra = p1; uv = float2(0,0); }
    if( alternate == 4 ){ extra = p4; uv = float2(1,1); }
    if( alternate == 5 ){ extra = p3; uv = float2(0,1); }


    o.worldPos = extra;
    o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  
  return o;

}

#include "../Chunks/hsv.cginc"
      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {


    float3 col = hsv(v.featherType * .2 + .5 ,1,1);

    if( v.featherType == 0 ){
      col = 1;
    }

    if( v.featherType < 1.5){
      //discard;
    }
  
    return float4(col,1 );
}

      ENDCG

    }
  }

  Fallback Off


}
