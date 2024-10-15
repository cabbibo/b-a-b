// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/RingInterface1" {
    Properties {
    }


  SubShader{

     // Tags {"Queue"="Transparent+10" "IgnoreProjector"="True" "RenderType"="Transparent"}
 // Tags {"Queue"="Background" "IgnoreProjector"="True" "RenderType"="Background"}
 
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
      uniform float _Radius;
      uniform float _Thickness;
      uniform float3 _WrenPos;
      uniform float _Value;
      uniform float _Fade;

      

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
      };


uniform float4x4 _Transform;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / 6;
  int alternate = id %6;

  if( base < _Count * 6){



      float3 pos = _WrenPos;
      float3 fwd = _WrenPos - _WorldSpaceCameraPos;

      float3 left = float3(0,1,0);
      float3 up = normalize(cross(left, fwd));

      float vDown = ((float)base / (float)_Count);
      float vUp = ((float)(base+1) / (float)_Count);

      float3 aDown = vDown * 6.283185;
      float3 aUp = vUp * 6.283185;

      float3 pDown = pos + left * cos(aDown) * _Radius + up * sin(aDown) * _Radius;
      float3 pUp = pos + left * cos(aUp) * _Radius + up * sin(aUp) * _Radius;

      float3 yDown =normalize( pDown - pos);
      float3 yUp = normalize(pUp - pos);

      float3 p1 = pDown - yDown * _Thickness;
      float3 p2 = pUp - yUp * _Thickness;
      float3 p3 = pDown + yDown * _Thickness;
      float3 p4 = pUp + yUp * _Thickness;


      float3 extra = 0;
      float2 uv = 0;

      float value = 0;

      if( alternate == 0 ){ extra = p1; uv = float2(0,0); value = vDown; }
      if( alternate == 1 ){ extra = p2; uv = float2(1,0); value = vUp;}
      if( alternate == 2 ){ extra = p4; uv = float2(1,1); value = vUp;}
      if( alternate == 3 ){ extra = p1; uv = float2(0,0); value = vDown;}
      if( alternate == 4 ){ extra = p4; uv = float2(1,1); value = vUp;}
      if( alternate == 5 ){ extra = p3; uv = float2(0,1); value = vDown;}

      o.worldPos = extra;
      
      
      // mul(_Transform, float4((v.pos) ,1));
      ///o.worldPos +=  extra * _Size;

      o.eye = _WorldSpaceCameraPos - o.worldPos;
      o.nor = normalize(UNITY_MATRIX_V[2].xyz);//v.nor;
      o.uv = uv;
      o.id = base;
      o.value = value;
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  }

  return o;

}


      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {

  float3 c1 = hsv(v.uv.x * .1,1,1);
  

  float3 fCol = 1;
  if( v.value < _Value ){
    fCol = 1;
  }else{
    fCol = 0;
  }

  if( abs( v.uv.y - .5) > .45 ){
    fCol = 1;
  }

  if( length(fCol) < .01){
    discard;
  }

  fCol *= _Fade;

  return float4( fCol , length(fCol));

}

      ENDCG

    }
  }

  Fallback Off


}
