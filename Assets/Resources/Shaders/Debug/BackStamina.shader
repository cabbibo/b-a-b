// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/BackStamBar" {
    Properties {

      _Size("_Size", Float) = 1
      _Count("_Count", Float) = 1
      _VerticalOffset("_VerticalOffset", Float) = 0
      _BackAmount("_BackAmount", Float) = 0
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
 // ZTest Always


    Pass{
      Tags { "Queue"="Overlay+1000" "IgnoreProjector"="True" "RenderType"="Transparent+100000" }
      Blend SrcAlpha One
    //	AlphaTest Greater .01
      //ColorMask RGB
      Cull Off 
      ZWrite Off 

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
    #include "Assets/Resources/Shaders/Chunks/hsv.cginc"
    #include "Assets/Resources/Shaders/Chunks/noise.cginc"
    #include "Assets/Resources/Shaders/Chunks/cubicCurve.cginc"





      uniform int _Count;
      uniform float _Size;

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
          float4 extra : TEXCOORD9;
          float3 centerPos : TEXCOORD10;

      };



    uniform float4x4 _Transform;

    StructuredBuffer<float3> points;


float _PositionsCount;

float3 cubic(float val ){

  float vPP = _PositionsCount;
  #include "Assets/Resources/Shaders/Chunks/CubicInclude.cginc"


}

float _VerticalOffset;
float _ForwardOffset;
float _BarWidth;
float _BarHeight;

    //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
    //which we transform with the view-projection matrix before passing to the pixel program.
    varyings vert (uint id : SV_VertexID){

      varyings o;

      int base = id / 6;
      int alternate = id %6;


      float fBase = (float)base;
      float nBase = (float)base / float(_Count);
      float count1 = 1/float(_Count);


      float3 pos = points[0];
      
      float2 uv = float2(0,0);

      float3 forward = UNITY_MATRIX_V[2].xyz;
      float3 right = UNITY_MATRIX_V[0].xyz;
      float3 up = UNITY_MATRIX_V[1].xyz;


      float tmpBase=nBase;

      nBase *= _BarWidth;
      nBase += (1-_BarWidth)/2.0;

      float3 c1 = cubic(nBase);
      float3 c2 = cubic(nBase+count1);
      float3 c3 = cubic(nBase+count1*2);

      float3 v = normalize(c2-c1);
      float3 u = normalize(cross(v, forward));

      float3 v1 = normalize(c3-c2);
      float3 u1 = normalize(cross(v1, forward));

      



      float3 p1 = c1 + _VerticalOffset * u;
      float3 p2 = c2 +_VerticalOffset * u1;
      float3 p3 = c1 + _BarHeight * u +_VerticalOffset * u;
      float3 p4 = c2 + _BarHeight * u1 +_VerticalOffset * u1;

      float2 uv1 = float2(tmpBase,0);
      float2 uv2 = float2(tmpBase + count1,0);
      float2 uv3 = float2(tmpBase,1);
      float2 uv4 = float2(tmpBase + count1,1);


      if( alternate == 0){
        pos = p1;
        uv = uv1;
      }else if( alternate == 1){
        pos = p2;
        uv = uv2;
      }else if( alternate == 2){
        pos = p4;
        uv = uv4;
      }else if( alternate == 3){
        pos = p1;
        uv = uv3;
      }else if( alternate == 4){
        pos = p4;
        uv = uv2;
      }else if( alternate == 5){
        pos = p3;
        uv = uv3;
      }else{
        pos = p1;
        uv = uv1;
      }

     // pos += _VerticalOffset * u;
      pos += _ForwardOffset * forward;


  o.uv = uv;
      o.pos = mul( UNITY_MATRIX_VP, float4(pos, 1.0) );




      return o;

    }




float _Brightness;
float _Value;
//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {


  float3 col;
  col = float3(1,0,0);
  float2 uv = v.uv;


  float val= abs(v.uv.x -.5) * 2;


  float fVal = _Value;// * 3 -2;

  if( val < fVal ){
    col = float3(0,1,0);
  }else{
    if( abs(uv.x-.5) > .49 || abs(v.uv.y-.5) > .49 ){
    

    }else{
      discard;
    }


  }

  col = lerp(float3(1,0,0), float3(0,1,0),fVal * 2 -.3);




 // col = uv.x * float3(1,0,0) + uv.y * float3(0,1,0) + (1-uv.x-uv.y) * float3(0,0,1);


  col *= _Brightness * (1-fVal)* (1-fVal)* (1-fVal)* (1-fVal)* (1-fVal)* (1-fVal);;
  
  return float4( col , 1);

}

      ENDCG

    }
  }

  Fallback Off


}
