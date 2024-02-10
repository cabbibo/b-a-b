// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/ConnectionsDebug" {
    Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _Size ("Size", float) = .01
    _Forwards ("Forwards", float) = 1
    }


  SubShader{
    Cull Off
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "Assets/Resources/Shaders/Chunks/hsv.cginc"








      uniform int _Count;
      uniform float _Size;
      uniform float _Forwards;
      uniform float3 _Color;

      
      StructuredBuffer<float2>    _ConnectionBuffer;
      StructuredBuffer<float4x4>  _TransformBuffer;


      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor      : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 eye      : TEXCOORD2;
          float2 uv       : TEXCOORD4;
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

      float2 connections = _ConnectionBuffer[base];

      float4x4 m1 = _TransformBuffer[int(connections.x)];
      float4x4 m2 = _TransformBuffer[int(connections.y)];



      float3 l1 = mul(m1, float4(0,0,0,1)).xyz;
      float3 l2 = mul(m2, float4(0,0,0,1)).xyz;

      float3 d = l2 - l1;
      float3 u = normalize(cross(d,UNITY_MATRIX_V[2].xyz));


      u *= _Size;

      float3 p1 = l1 -u;
      float3 p2 = l2 -u;
      float3 p3 = l1 + u;
      float3 p4 = l2 + u;

      float3 extra = 0;
      float2 uv = 0;

      if( alternate == 0 ){ extra = p1; uv = float2(0,0); }
      if( alternate == 1 ){ extra = p2; uv = float2(1,0); }
      if( alternate == 2 ){ extra = p4; uv = float2(1,1); }
      if( alternate == 3 ){ extra = p1; uv = float2(0,0); }
      if( alternate == 4 ){ extra = p4; uv = float2(1,1); }
      if( alternate == 5 ){ extra = p3; uv = float2(0,1); }

      o.worldPos = extra;
      
      
      // mul(_Transform, float4((v.pos) ,1));
      ///o.worldPos +=  extra * _Size;

      o.eye = _WorldSpaceCameraPos - o.worldPos;
      o.nor = d;//v.nor;
      o.uv = uv;
      o.id = base;
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  }

  return o;

}


      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
    return float4(hsv(v.id * .1,1,1),1 );
}

      ENDCG

    }
  }

  Fallback Off


}
