// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Feathers/FeatherTest1" {
    Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _Size ("Size", float) = .01
    _Saturation ("Saturation", float) = .01

    

    _IsBody("Is body" , float ) = 0
    
         _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }


  SubShader{

         Tags { "Queue" = "Geometry+10" }
    Pass{

        LOD 100 
        Cull Off
        Tags{ "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            // make fog work
            #pragma multi_compile_fogV
 #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

      #include "UnityCG.cginc"
      #include "AutoLight.cginc"
    

      #include "../Chunks/hsv.cginc"


      uniform int _Count;
      uniform float _Size;
      uniform float3 _Color;

      float _Saturation;

      uniform int _TrisPerMesh;

    struct Vert{
        float3 pos;
        float3 nor;
        float2 uv;
    };

struct Feather{
  float3 pos;
  float3 vel;
  float featherType;
  float locked;
  float4x4 ltw;
  float3 ogPos;
  float3 ogNor;
  float touchingGround;
  float debug;
};


      StructuredBuffer<Vert> _VertBuffer;
      StructuredBuffer<int> _TriBuffer;
      StructuredBuffer<Feather> _FeatherBuffer;


      //uniform float4x4 worldMat;

         sampler2D _MainTex;
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
          float hue        : TEXCOORD10;
          float offset : TEXCOORD11;
          int feather:TEXCOORD7;
          float4 data1:TEXCOORD9;
           UNITY_SHADOW_COORDS(8)
      };

#include "../Chunks/hash.cginc"
uniform float4x4 _Transform;
uniform int _NumberMeshes;

float _Hue1;
float _Hue2;
float _Hue3;
float _Hue4;

float _IsBody;
      
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

  varyings o;

  int base = id / _TrisPerMesh;
  int alternate = id %_TrisPerMesh;
  Feather feather = _FeatherBuffer[base];
  
  int whichMesh = int(feather.featherType); //int(floor(hash(float(base)) * float(_NumberMeshes)));// %4;


float4x4 baseMatrix = feather.ltw;
Vert v = _VertBuffer[_TriBuffer[alternate + whichMesh * _TrisPerMesh]];
     // o.data1 = feather.newData1;
      o.worldPos = mul( baseMatrix , float4(v.pos,1)).xyz;//extra;
      o.id = float(base);
      o.feather = whichMesh;

      o.hue = _Hue1;

      if( _IsBody > .5 ){ o.hue = _Hue4; }

      if( whichMesh == 1 ){ o.hue = _Hue2;}
      if( whichMesh == 2 ){ o.hue = _Hue3;}
      if( whichMesh == 3 ){ o.hue = _Hue4;}
      //o.data1 = feather.newData1;
      o.nor = normalize(mul( baseMatrix , float4(v.nor,0)).xyz);
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
      o.uv = v.uv;
  UNITY_TRANSFER_SHADOW(o,o.worldPos);
  

  return o;

}


//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
  fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);//* .5 + .5;
  float3 tCol = tex2D (_MainTex, v.uv);

  float3 m = dot( UNITY_MATRIX_V[2].xyz , v.nor );
  float hueOffset =   sin(v.id * 15.91) * .04 + sin( v.id * 14.1445) * .06;



float lightness = m * ( shadow * .5 + .5);
lightness = floor(lightness*3) / 2;
   // float3 col= float3(v.data1.x,v.data1.y,1.);//(1-tCol.x) * hsv(m * .3 + v.feather * .2, 1,1) * shadow;
    float3 col= hsv(v.hue , _Saturation,lightness);// * lerp(1,tCol ,1-shadow);

    //col = v.nor * .5 +.5;
    return float4(col,1);
}

      ENDCG

    }


  




































                // SHADOW PASS

    Pass
    {
      Tags{ "LightMode" = "ShadowCaster" }

         Tags { "Queue" = "Geometry+100" }

      Fog{ Mode Off }
      ZWrite On
      ZTest LEqual
      Cull Off
      Offset 1, 1
      CGPROGRAM

      #pragma target 4.5
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_shadowcaster
      #pragma fragmentoption ARB_precision_hint_fastest

      #include "UnityCG.cginc"
      #include "../Chunks/ShadowCasterPos.cginc"
     





    struct Vert{
        float3 pos;
        float3 nor;
        float2 uv;
    };
struct Feather{
  float3 pos;
  float3 vel;
  float2 data;
  float4x4 ltw;
  float4 newData1;
  float4 newData2;
};

   
    int _TrisPerMesh;
      StructuredBuffer<Vert> _VertBuffer;
      StructuredBuffer<Feather> _FeatherBuffer;
      StructuredBuffer<int> _TriBuffer;

      struct v2f {
        V2F_SHADOW_CASTER;
        float3 nor : NORMAL;
        float3 worldPos : TEXCOORD1;
        float2 uv : TEXCOORD0;
        float4 data1 : TEXCOORD2;
      };


      v2f vert(appdata_base input, uint id : SV_VertexID)
      {
                v2f o;


  //             UNITY_INITIALIZE_OUTPUT(v2f, o);


    int base = id / _TrisPerMesh;
    int alternate = id %_TrisPerMesh;

    
  Feather feather = _FeatherBuffer[base];


    int whichMesh = int(feather.data.x); //int(floor(hash(float(base)) * float(_NumberMeshes)));// %4;


    float4x4 baseMatrix = feather.ltw;
    Vert v = _VertBuffer[_TriBuffer[alternate + whichMesh * _TrisPerMesh]];

      o.worldPos = mul( baseMatrix , float4(v.pos,1)).xyz;//extra;

      o.nor = normalize(mul( baseMatrix , float4(v.nor,0)).xyz);
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));


  
        float4 position = ShadowCasterPos(o.worldPos, o.nor );
        o.pos = UnityApplyLinearShadowBias(position);


       // UNITY_TRANSFER_SHADOW(o,o.worldPos);

  return o;

      }

      float4 frag(v2f i) : COLOR
      {
        SHADOW_CASTER_FRAGMENT(i)
      }


      ENDCG
    }
  
    




    }



}

