// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/matrixParticles" {
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
struct feather{
  float3 pos;
  float3 vel;
  float2 data;
  float4x4 ltw;
};

StructuredBuffer<feather> _FeatherBuffer;
      


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


      float3 extra = float3(0,0,0);

    float3 v = mul(_FeatherBuffer[base].ltw,float4(0,0,0,1)).xyz;
    float3 l = mul(_FeatherBuffer[base].ltw,float4(1,0,0,0)).xyz;
    float3 u = mul(_FeatherBuffer[base].ltw,float4(0,0,1,0)).xyz;
    
    float2 uv = float2(0,0);

    if( alternate == 0 ){ extra = v +( -l* _Size)  ; uv = float2(0,0); }
    if( alternate == 1 ){ extra = v +( +l* _Size)  ; uv = float2(1,0); }
    if( alternate == 2 ){ extra = v +(+u*3* _Size+l* _Size)  ; uv = float2(1,1); }
    if( alternate == 3 ){ extra = v +( -l* _Size)  ; uv = float2(0,0); }
    if( alternate == 4 ){ extra = v +(+u*3* _Size+l* _Size)  ; uv = float2(1,1); }
    if( alternate == 5 ){ extra = v +(+u*3* _Size-l* _Size)  ; uv = float2(0,1); }


      o.worldPos = extra;
      o.id = float(base) * .01;
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  

  return o;

}


      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
    return float4(hsv(v.id,1.,1.),1);
}

      ENDCG

    }
  }

  Fallback Off


}
