
Shader "IMMAT/Debug/SpriteParticlesLife" {
    Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _Size ("Size", float) = .01
      _MainTex("_MainTex", 2D) = "white" {}
      _ColorMap("_ColorMap", 2D) = "white" {}
      _CubeMap("_CubeMap", Cube) = "white" {}
      _SpriteSize("_SpriteSize",float) = 5

      _ColorInfo("_ColorInfo", Vector) =  (1,0,.5,0)
    }


  SubShader{
    Cull Off
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"


      
    struct Vert{
      float3 pos;
      float3 vel;
      float3 nor;
      float3 tan;
      float3 axis;
      float life;
    };



      uniform int _Count;
      uniform float _Size;
      uniform float3 _Color;


      sampler2D _ColorMap;
      sampler2D _MainTex;
      samplerCUBE _CubeMap;


  float _SpriteSize;

  float3 _ColorInfo;
      
      StructuredBuffer<Vert> _VertBuffer;


      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor      : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 eye      : TEXCOORD2;
          float2 debug    : TEXCOORD3;
          float2 uv       : TEXCOORD4;
          float2 texUV    : TEXCOORD8;
          float3 axis     : TEXCOORD6; 
          float id        : TEXCOORD5;
          float life      : TEXCOORD7;
      };



//float _Multiplier;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / 6;
  int alternate = id %6;

  if( base < _Count ){

      float3 extra = float3(0,0,0);
      Vert v = _VertBuffer[base];


    float3 u = normalize(v.axis);
    float3 l = normalize(cross(v.nor, v.axis));

    //float3 l = UNITY_MATRIX_V[0].xyz;
    //float3 u = UNITY_MATRIX_V[1].xyz;
    
    float2 uv = float2(0,0);

    if( alternate == 0 ){ extra = -l - u; uv = float2(0,0); }
    if( alternate == 1 ){ extra =  l - u; uv = float2(1,0); }
    if( alternate == 2 ){ extra =  l + u; uv = float2(1,1); }
    if( alternate == 3 ){ extra = -l - u; uv = float2(0,0); }
    if( alternate == 4 ){ extra =  l + u; uv = float2(1,1); }
    if( alternate == 5 ){ extra = -l + u; uv = float2(0,1); }



      //Vert v = _VertBuffer[base % _Count];

      float uvX = floor( ((sin( float(base) * 40400.) + 1)/2) * _SpriteSize  ) / _SpriteSize;
      float uvY = floor( ((sin( float(base) * 81409.) + 1)/2) * _SpriteSize  ) / _SpriteSize;

      float lifeSize = min( v.life  , 3-3*v.life);
      o.worldPos = (v.pos) + extra * _Size  * lifeSize;
      o.eye = _WorldSpaceCameraPos - o.worldPos;
      o.nor =v.nor;
      o.uv = uv;
      o.texUV = uv / _SpriteSize +float2(uvX, uvY);
      o.id = base;
      o.life = v.life;
      o.axis = v.axis;
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  }

  

  return o;

}


      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

         // if( length( v.uv -.5) > .5 ){ discard;}

          float3 col = v.nor * .5 + .5;//float4(_Color.xyz,1);// v.debug.x * 10;

          col = texCUBE(_CubeMap, reflect(normalize(v.eye),v.nor)) * 3;

          float4 tCol = tex2D(_MainTex,v.texUV);

          float4 cCol = tex2D(_ColorMap, float2(v.life * _ColorInfo.x + _ColorInfo.y, _ColorInfo.z ));


          col *= cCol;

          if( tCol.x > .6){
            discard;
          }
          //col = 1;

          
          return float4(col,1 );
      }

      ENDCG

    }
  }


}
