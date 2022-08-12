﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Simulation/ShatterCrystals" {
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
     // #include "../Chunks/Struct16.cginc"



struct Vert{
  float3 pos;
  float3 vel;
  float3 nor;
  float3 tan;
  float  life;
  float3 debug;
};


      uniform int _Count;
      uniform float _Size;
      uniform float3 _Color;

      
      StructuredBuffer<Vert> _VertBuffer;


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
      };


uniform float4x4 _Transform;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / 6;
  int alternate = id %6;

  if( base < _Count ){

      float3 extra = float3(0,0,0);

    //float3 l = UNITY_MATRIX_V[0].xyz;
    //float3 u = UNITY_MATRIX_V[1].xyz;
    
    float2 uv = float2(0,0);

    Vert v = _VertBuffer[base];
    

      float3 d = normalize(v.vel);

      float3 u = d;
      float3 l = normalize(cross(d,UNITY_MATRIX_V[2].xyz));

      float fSize = _Size * v.life;

    if( alternate == 0 ){ extra = v.pos + (- u - l) * fSize; uv = float2(0,0); }
    if( alternate == 1 ){ extra = v.pos + (- u + l) * fSize; uv = float2(1,0); }
    if( alternate == 2 ){ extra = v.pos + (+ u + l) * fSize; uv = float2(1,1); }
    if( alternate == 3 ){ extra = v.pos + (- u - l) * fSize; uv = float2(0,0); }
    if( alternate == 4 ){ extra = v.pos + (+ u + l) * fSize; uv = float2(1,1); }
    if( alternate == 5 ){ extra = v.pos + (+ u - l) * fSize; uv = float2(0,1); }




      o.worldPos = extra;// mul(_Transform, float4((v.pos) ,1));
      ///o.worldPos +=  extra * _Size;
      o.eye = _WorldSpaceCameraPos - o.worldPos;
      o.nor =normalize(d);//v.nor;
      //o.uv = v.uv;
      o.uv2 = uv;
      o.id = base;
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  }

  return o;

}


      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
    return float4(v.nor*.5 +.5,1 );
}

      ENDCG

    }
  }

  Fallback Off


}
