// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/StatsDebug" {
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

      
      StructuredBuffer<float2> _StatsBuffer;


      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor      : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 eye      : TEXCOORD2;
          float2 uv       : TEXCOORD4;
          float id        : TEXCOORD5;
          float fullOrNot : TEXCOORD6;
      };


uniform float4x4 _Transform;
uniform float4x4 _CameraTransform;


//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / 6;
  int alternate = id %6;

  int whichStat = base / 2;
  int fullOrNot = base % 2;

  if( whichStat < _Count ){

      float3 extra = float3(0,0,0);

    //float3 l = UNITY_MATRIX_V[0].xyz;
    //float3 u = UNITY_MATRIX_V[1].xyz;
    
    float2 uv = float2(0,0);

    float2 v = _StatsBuffer[whichStat];

    float filledAmount = v.x/v.y;
    filledAmount = clamp(filledAmount,0,1);

    float3 basePos = mul(_Transform, float4(0,0,0,1)).xyz;
    float3 baseRight =  normalize(mul(_Transform, float4(1,0,0,0)).xyz);
    float3 baseUp = normalize(mul(_Transform, float4(0,1,0,0)).xyz);
    float3 baseForward = normalize(mul(_Transform, float4(0,0,1,0)).xyz);
    

    float3 cameraPosition = (mul( _CameraTransform , float4(0,0,0,1)).xyz);
    float3 cameraRight = normalize(mul( _CameraTransform , float4(1,0,0,0)).xyz);
    float3 cameraUp = normalize(mul( _CameraTransform , float4(0,1,0,0)).xyz);
    float3 cameraForward = normalize(mul( _CameraTransform , float4(0,0,1,0)).xyz);


    float3 delta = basePos - cameraPosition;



    float3 pos =  cameraPosition + normalize(delta)* 1 - cameraRight * .3f + cameraUp  * (float(whichStat) * _Size * .3f  +.2);

    float3 fEye = cameraPosition- pos;

     
    pos += float(fullOrNot) *fEye * _Size * .3f;

    

      float3 u = cameraUp * .1f *(1+(1-float(fullOrNot)) * .4);
      float3 l = cameraRight*(1+(1-float(fullOrNot)) * .1);//normalize(cross(d,UNITY_MATRIX_V[2].xyz)) * _Size;

      

    float3 p1 = -l - u;
    float3 p2 = l - u;
    float3 p3 = -l + u;
    float3 p4 = l + u;

      if( fullOrNot > .5  ){

        p2 = -l + 2*l * filledAmount - u;
        p4 = -l +2*l * filledAmount + u;

      }

    p1 *= (_Size);
    p2 *= (_Size);
    p3 *= (_Size);
    p4 *= (_Size);

    if( alternate == 0 ){ extra = p1; uv = float2(0,0); }
    if( alternate == 1 ){ extra = p2; uv = float2(1,0); }
    if( alternate == 2 ){ extra = p4; uv = float2(1,1); }

    
    if( alternate == 3 ){ extra = p1; uv = float2(0,0); }
    if( alternate == 4 ){ extra = p4; uv = float2(1,1); }
    if( alternate == 5 ){ extra = p3; uv = float2(0,1); }

    

      o.worldPos = pos + extra;

      o.eye = _WorldSpaceCameraPos - o.worldPos;
      //o.nor =d;//v.nor;
      o.uv = uv;
      o.id = base;
      o.fullOrNot = float(fullOrNot);
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  }

  return o;

}


      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
    return float4(hsv(v.id * .1,1,v.fullOrNot),1 );
}

      ENDCG

    }
  }

  Fallback Off


}
