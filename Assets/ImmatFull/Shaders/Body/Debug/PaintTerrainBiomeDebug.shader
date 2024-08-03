
Shader "Debug/PaintTerrainBiomeDebug" {
  Properties {

    _BiomeColor1 ("_BiomeColor1", Color) = (1,1,1,1)
    _BiomeColor2 ("_BiomeColor2", Color) = (1,1,1,1)
    _BiomeColor3 ("_BiomeColor3", Color) = (1,1,1,1)
    _BiomeColor4 ("_BiomeColor4", Color) = (1,1,1,1)
    _Size ("Size", float) = .01
    _Up ("Up", float) = .01
  }


  SubShader{
    Cull Off
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "../../Chunks/hsv.cginc"





      uniform int _Count;
      uniform float _Size;
      uniform float _Up;
      uniform float3 _Color;
      
      StructuredBuffer<float4> _VertBuffer;
      //Buffer<float> _VertBuffer;

      int _WhichBrush;
      int _TotalBrushes;
      int _BaseBrush;
      int _ShownBrushes;


      float4 _BiomeColor1;
      float4 _BiomeColor2;
      float4 _BiomeColor3;
      float4 _BiomeColor4;
      


      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
        float4 pos      : SV_POSITION;
        float3 worldPos : TEXCOORD1;
        float3 value       : TEXCOORD5;
        float whichBrush : TEXCOORD6;
        
      };


      sampler2D _HeightMap;
      float3 _MapSize;
      float3 _MapOffset;

      int _Dimensions;
      int _Width;


      float3 getPos( int id ){
        float x = float(id % _Width);
        float z = float(id / _Width);

        float fW = float(_Width);
        
        float3 pos = float3(
        ((x + .5)/fW) * 1 * _MapSize.x + _MapOffset.x,
        0,
        ((z + .5)/fW) * 1 * _MapSize.z + _MapOffset.z
        );


        pos.y = tex2Dlod( _HeightMap , float4((x + .5)/ fW , (z + .5) / fW , 0 ,0))  * _MapSize.y*2;

        return pos;
      }
      //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
      //which we transform with the view-projection matrix before passing to the pixel program.
      varyings vert (uint id : SV_VertexID){

        varyings o;

        int safeID = id  % (_Count *3);
        int whichBrushAdd = id / (_Count*3);

        int base = safeID / 3;
        int alternate = safeID %3;

        float3 extra;

        float2 uv = float2(0,0);

        float3 pos = getPos(base);

        float4 v = _VertBuffer[base];

        int actualBrush = (_WhichBrush+whichBrushAdd)%_TotalBrushes;

        float value = v[actualBrush];

        float3 dir = float3(0,1,0) *value ;


        float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;

        float3 yVal =  normalize( -cross( dir , viewDir ));

        if( alternate == 0 ){ extra =  -yVal * .1; uv = float2(0,0); }
        if( alternate == 1 ){ extra =  +yVal  * .1; uv = float2(1,0); }
        if( alternate == 2 ){ extra =  dir; uv = float2(.5,1); }

        o.worldPos = pos.xyz + float3(0,_Up,0) + extra * _Size +  float3(0,_Up,0)*(_WhichBrush+whichBrushAdd) * .4 + viewDir *(_WhichBrush+whichBrushAdd) * .4 ;


        o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
        o.value = value;
        o.whichBrush = actualBrush;


        return o;

      }




      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

        if( v.whichBrush < .5 ){
          return _BiomeColor1;
          }else if( v.whichBrush >= .5 && v.whichBrush < 1.5){
          return _BiomeColor2;
          }else if(v.whichBrush >= 1.5 && v.whichBrush < 2.5){
          return _BiomeColor3;
          }else if( v.whichBrush >= 2.5 && v.whichBrush < 3.5){
          return _BiomeColor4;
        }

        return 1;
      }

      ENDCG

    }
  }

  Fallback Off


}
