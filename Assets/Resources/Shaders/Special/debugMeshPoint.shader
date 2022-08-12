// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/meshParticles" {
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
      #include "../Chunks/hsv.cginc"


      uniform int _Count;
      uniform float _Size;
      uniform float3 _Color;

      uniform int _TrisPerMesh;

    struct Vert{
        float3 pos;
        float3 nor;
        float2 uv;
    };

    struct Feather{
        float3 pos;
        float3 vel;
        float2 data;
        float4x4 ltw;
    };


      StructuredBuffer<Vert> _VertBuffer;
      StructuredBuffer<int> _TriBuffer;
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
          int feather:TEXCOORD7;
      };

#include "../Chunks/hash.cginc"
uniform float4x4 _Transform;
uniform int _NumberMeshes;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / _TrisPerMesh;
  int alternate = id %_TrisPerMesh;
  int whichMesh = int(_FeatherBuffer[base].data.x); //int(floor(hash(float(base)) * float(_NumberMeshes)));// %4;


float4x4 baseMatrix = _FeatherBuffer[base].ltw;
Vert v = _VertBuffer[_TriBuffer[alternate + whichMesh * _TrisPerMesh]];

      o.worldPos = mul( baseMatrix , float4(v.pos.yxz,1)).xyz;//extra;
      o.id = float(base);
      o.feather = whichMesh;
      o.nor = normalize(mul( baseMatrix , float4(v.nor,0)).xyz);
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  

  return o;

}


      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
    float3 col=hsv(v.feather * .1 ,.5,1) * (v.nor * .5 + .5);
    return float4(col,1);
}

      ENDCG

    }
  }

  Fallback Off


}
