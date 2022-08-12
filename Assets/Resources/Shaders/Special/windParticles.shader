// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/windParticles" {
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

    struct Vert{
      float3 pos;
      float3 nor;
      float2 debug;
    };

      uniform int _Count;
      uniform float _Size;
      uniform float3 _Color;

      
      StructuredBuffer<Vert> _VertBuffer;


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
  
  int pID = base;// / 63;
  int col = base;// % 63; 
  int alternate = id %6;

  if( base < _Count ){

      float3 extra = float3(0,0,0);

    float3 l = UNITY_MATRIX_V[0].xyz;
    float3 u = UNITY_MATRIX_V[1].xyz;
    
    float2 uv = float2(0,0);

      int bID = base;

      Vert v = _VertBuffer[pID ];

      //float3 p1 = v.oPos[col];
      //float3 p2 = v.oPos[col+1];

      //p1 = v.pos;
      //p2 = v.pos + u * .1;


    float3 p1 = v.pos;
     
      //float3 d = (p1 -p2);

      //u = normalize(d);
      l = normalize(cross(u,UNITY_MATRIX_V[2].xyz));


      float turnOff = 1;
      if( v.debug.y > .98 || v.debug.y < .02){
        turnOff = 0;
      }

      turnOff = 1;

      float cv1 = 1-float(col)/64;
      float cv2 = 1-float(col+1)/64;

      cv1 = saturate(min( 1-4*cv1 , cv1 ));
      cv2 = saturate(min( 1-4*cv2 , cv2 ));

      cv1 = 1;
      cv2 = 1;

    

      float lifeSize = length(v.nor) * .01 * v.debug.y  * _Size;//v.debug.y;//saturate(4*min( 1-4*v.debug.y , v.debug.y ));
    if( alternate == 0 ){ extra = p1 + (-u - l) * 1 * cv1 * lifeSize* turnOff ; uv = float2(0,0); }
    if( alternate == 1 ){ extra = p1 + (-u + l) * 1 * cv1 * lifeSize* turnOff ; uv = float2(1,0); }
    if( alternate == 2 ){ extra = p1 + (+u + l) * 1 * cv2 * lifeSize* turnOff ; uv = float2(1,1); }
    if( alternate == 3 ){ extra = p1 + (-u - l) * 1 * cv1 * lifeSize* turnOff ; uv = float2(0,0); }
    if( alternate == 4 ){ extra = p1 + (+u + l) * 1 * cv2 * lifeSize* turnOff ; uv = float2(1,1); }
    if( alternate == 5 ){ extra = p1 + (+u - l) * 1 * cv2 * lifeSize* turnOff ; uv = float2(0,1); }


      o.worldPos = extra;// mul(_Transform, float4((v.pos) ,1));
      ///o.worldPos +=  extra * _Size;
      o.eye = _WorldSpaceCameraPos - o.worldPos;
     // o.nor =normalize(d);//v.nor;
      o.uv = v.debug;
      //o.uv2 = uv;
      o.id = base;
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

  }

  return o;

}


      

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
    return 1;
}

      ENDCG

    }
  }

  Fallback Off


}
