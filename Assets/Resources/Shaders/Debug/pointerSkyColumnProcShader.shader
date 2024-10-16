// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/PointerSkyColumnProcShader1" {
    Properties {

      _Size("_Size", Float) = 1
    }


  SubShader{

     // Tags {"Queue"="Transparent+10" "IgnoreProjector"="True" "RenderType"="Transparent"}
 // Tags {"Queue"="Background" "IgnoreProjector"="True" "RenderType"="Background"}
 
 // Tags { "Queue"="Overlay+1000" "IgnoreProjector"="True" "RenderType"="Transparent+100000" }
//	Blend SrcAlpha One
//	AlphaTest Greater .01
	//ColorMask RGB
	Cull Off 
  //ZWrite Off 
 // ZTest Always

  Tags { "Queue"="Overlay+1000" "IgnoreProjector"="True" "RenderType"="Transparent+100000" }
	Blend SrcAlpha One
//	AlphaTest Greater .01
	//ColorMask RGB
	Cull Off 
  ZWrite Off 
  ZTest Always


    Pass{


      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
    #include "Assets/Resources/Shaders/Chunks/hsv.cginc"





      uniform int _Count;
      uniform float _Size;
      uniform float3 _WrenPos;
      uniform float _Fade;


      StructuredBuffer<float3> _PositionBuffer;
      StructuredBuffer<float> _FadeBuffer;
      StructuredBuffer<float> _TypeBuffer;

      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor      : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 eye      : TEXCOORD2;
          float2 uv       : TEXCOORD4;
          float id        : TEXCOORD5;
          float value : TEXCOORD6;
          float fade : TEXCOORD7;
          float type : TEXCOORD8;

      };


uniform float4x4 _Transform;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / 6;
  int alternate = id %6;

  if( base < _Count * 6){


    float3 center = _PositionBuffer[base];

    float3 left = UNITY_MATRIX_V[0].xyz; // left direciton
    float3 up = float3(0,1,0); // up direction







      float3 p1 = center - left * (_Size );
      float3 p2 =  center + left * (_Size );
      float3 p3 = center - left * (_Size) + up * (_Size * 100);
      float3 p4 = center + left * (_Size) + up * (_Size * 100);

      /*float3 p1 = center - up *_Size;
      float3 p2 =  pos  - up *_Size;
      float3 p3 = center + up *_Size;
      float3 p4 = pos + up *_Size;*/


      float3 extra = 0;
      float2 uv = 0;

      float value = 0;

      if( alternate == 0 ){ extra = p1; uv = float2(0,0); value = 0; }
      if( alternate == 1 ){ extra = p2; uv = float2(1,0); value = 0; }
      if( alternate == 2 ){ extra = p4; uv = float2(1,1); value = 1;}
      if( alternate == 3 ){ extra = p1; uv = float2(0,0); value = 0;}
      if( alternate == 4 ){ extra = p4; uv = float2(1,1); value = 1;}
      if( alternate == 5 ){ extra = p3; uv = float2(0,1); value = 0;}

      o.worldPos = extra;
      
      
      // mul(_Transform, float4((v.pos) ,1));
      ///o.worldPos +=  extra * _Size;

      o.eye = _WorldSpaceCameraPos - o.worldPos;
      o.nor = normalize(UNITY_MATRIX_V[2].xyz);//v.nor;
      o.uv = uv;
      o.id = base;
      o.fade = _FadeBuffer[base];
      o.type = _TypeBuffer[base];
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  }

  return o;

}


      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {

  float3 c1 = hsv(v.uv.x * .1,1,1);
  
  float3 fCol = 1 * v.fade;
  return float4( fCol , 1);

}

      ENDCG

    }
  }

  Fallback Off


}
