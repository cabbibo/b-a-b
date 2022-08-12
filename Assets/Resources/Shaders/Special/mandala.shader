// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Special/mandala" {
    Properties {

        _ColorMap ("ColorMap", 2D) = "white" {}
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
#pragma multi_compile_instancing
      #include "UnityCG.cginc"
      #include "../Chunks/rotationMatrix.cginc"

    struct Vert{
      float3 pos;
      float3 nor;
      float4 color;
      float2 debug;
    };

      uniform int _Count;
      uniform float _Size;
      uniform float3 _Color;

      
      StructuredBuffer<Vert> _VertBuffer;
      StructuredBuffer<int> _TriBuffer;

      uniform int _TriCount;



      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor      : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 eye :   TEXCOORD5;
          float3 ogPos : TEXCOORD4;
          float4 color : TEXCOORD2;
          float instanceID : TEXCOORD3;
          float2 rowCol : TEXCOORD6;
      };


int _TotalInstances;
int _Rows;
int _Cols;
float3 _Offset;
uniform float4x4 _Transform;

sampler2D _ColorMap;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID , uint instanceID : SV_InstanceID){

  varyings o;

  int tri = _TriBuffer[id];
 Vert v = _VertBuffer[tri];


int whichRow = instanceID / _Cols;
int whichCol = instanceID % _Cols;




float angle; float3 axis; float4x4 rotMat;



angle = float(whichCol) / float(_Cols);
//angle *= 6.28 + (float(whichRow)  * (1/float(_Cols)) * 6.28 / float(_Cols));
angle *= 6.28;// + (float(whichRow)  * .5 * 6.28 / float(_Cols));
axis = float3(0 , 0 , 1); 
rotMat =  rotationMatrix( axis , angle );

angle = float(whichRow) / float(_Rows);
angle *= 6.28 * .2;
axis = float3(1 , 0 , 0); 

rotMat = mul(rotMat, rotationMatrix( axis , angle ));

angle = 6.28 * _Time.x + sin(_Time.y + 30*v.pos.z);
axis = float3(0 , 1 , 0); 
rotMat = mul(rotMat, rotationMatrix( axis , angle ));

angle = 6.28 * _Time.x;
axis = float3(1 , 0 , 0); 
rotMat = mul(rotMat, rotationMatrix( axis , angle ));




float3 _Scale = float3(2,2,1);
float3 fPos = v.pos  * _Scale + _Offset;


fPos = mul(rotMat,float4(fPos,1)).xyz;// + (float)instanceID * float3(.01,0,0);
o.worldPos = mul( _Transform,float4(fPos,1));
o.color = v.color;
o.ogPos = v.pos;
o.rowCol = float2(float(whichRow), float(whichCol));
      o.eye = _WorldSpaceCameraPos - o.worldPos;
o.nor =  normalize( mul( _Transform, mul(rotMat, float4(v.nor,0))));
o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
o.instanceID = (float)instanceID;
 
  return o;

}


 float3 rgb2hsv(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}     

//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {

    float3 h = rgb2hsv(v.color.xyz);
    float m = floor(dot( v.nor , normalize(v.eye)) * 3 )/3;
    float3 c = hsv2rgb(float3(h.x + v.ogPos.z *10 + v.rowCol.x  * .1+ m * .3 - .4,1- v.ogPos.z * 3,m + v.rowCol.x * .1));
   // c = v.color.xyz;


   c = tex2D(_ColorMap, h.z *3 + .4 ).xyz * m * 2;


    //float m = dot( v.nor , normalize(v.eye));
   //c = m;//v.nor * .5 + .5;
    return float4(c,1);
}

      ENDCG

    }
  }

  Fallback Off


}
