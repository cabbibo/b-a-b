
Shader "Debug/TerrainPointDebug" {
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
      
      uniform int _Count;
      uniform int _BrushTypes;
      uniform float _Size;
      uniform float3 _Color;



      
      StructuredBuffer<float> _VertBuffer;

  int _Width;
  float3 _MapSize;
  sampler2D _HeightMap;


      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor      : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 eye      : TEXCOORD2;
          float2 debug    : TEXCOORD3;
          float2 uv       : TEXCOORD4;
          float2 uv2       : TEXCOORD6;
          float id        : TEXCOORD5;
      };


float3 getPos( int id ){
  float x = float(id % _Width);
  float z = float(id / _Width);

  float fW = float(_Width);

  float3 pos = float3(
    ((x + .5)/fW-.5) * _MapSize.x,
    0,
    ((z + .5)/fW-.5) * _MapSize.z
  );

  pos.y = tex2Dlod( _HeightMap , float4((x + .5)/ fW , (z + .5) / fW , 0 ,0))  * _MapSize.y*2;


  return pos;
}

//float _Multiplier;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / 6;
  int alternate = id %6;

  if( base < _Count   ){

      float3 extra = float3(0,0,0);

    float3 l = UNITY_MATRIX_V[0].xyz;
    float3 u = UNITY_MATRIX_V[1].xyz;
    
    float2 uv = float2(0,0);

    if( alternate == 0 ){ extra = -l - u; uv = float2(0,0); }
    if( alternate == 1 ){ extra =  l - u; uv = float2(1,0); }
    if( alternate == 2 ){ extra =  l + u; uv = float2(1,1); }
    if( alternate == 3 ){ extra = -l - u; uv = float2(0,0); }
    if( alternate == 4 ){ extra =  l + u; uv = float2(1,1); }
    if( alternate == 5 ){ extra = -l + u; uv = float2(0,1); }

    float3 pos = getPos(base);

      //Vert v = _VertBuffer[base];
      //Vert v = _VertBuffer[base % _Count];
      o.worldPos = pos + extra * _Size;
      o.eye = _WorldSpaceCameraPos - o.worldPos;
    //  o.nor =v.nor;
     // o.uv = v.uv;
      o.uv2 = uv;
      o.id = base;
     // o.debug = v.debug;
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  }

  return o;

}




//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
    return float4(_Color,1 );
}

      ENDCG

    }
  }

  Fallback Off


}
