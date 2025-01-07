


#include "UnityCG.cginc"    
      
#include "AutoLight.cginc"
#include "UnityLightingCommon.cginc"



#include "Assets/Resources/Shaders/Chunks/hsv.cginc"
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
    float randID   : TEXCOORD13;
    float hue        : TEXCOORD10;
    float offset : TEXCOORD11;
    float baseHue : TEXCOORD12;
    int feather:TEXCOORD7;
    float4 data1:TEXCOORD9;
    float collectionType:TEXCOORD14;
    float3 barycentric : TEXCOORD15;
    float3 localPos : TEXCOORD16;
    float3 localCam : TEXCOORD17;
    float3 localRD : TEXCOORD18;
    UNITY_SHADOW_COORDS(8)
  };


  [maxvertexcount(3)]
  void geom(triangle varyings input[3], inout TriangleStream<varyings> triStream)
  {
    varyings o;
    //  float3 normal = normalize(cross(input[1].vertex - input[0].vertex, input[2].vertex - input[0].vertex));
    
    float3 normal = float3(0,1,0);

    
    o = input[0];
    o.barycentric = float3(1,0,0);
    triStream.Append(o);

    o = input[1];
    o.barycentric = float3(0,1,0);
    triStream.Append(o);

    o = input[2];
    o.barycentric = float3(0,0,1);
    triStream.Append(o);
    
    
    triStream.RestartStrip();
  }
  

  float getGrid( float3 barys , float size  , float offset  ){
    
    float val = max(max( sin( barys.x  * size), sin( barys.y  * size) ), sin( barys.z  * size));
    val -= offset;
    val /= (1-offset);
    val = clamp(val,0,1);
    return val;
  }


  
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


  #include "Assets/Resources/Shaders/Chunks/FeatherStruct.cginc"

  StructuredBuffer<Vert> _VertBuffer;
  StructuredBuffer<int> _TriBuffer;
  StructuredBuffer<Feather> _FeatherBuffer;

  uniform float _BodyShardRendered;

  
  float _TotalShardsInBody;
  float _NumShards;
  float _TmpNumShards;
  float _ONumShards;


  #include "Assets/Resources/Shaders/Chunks/InverseMatrix.cginc"
  
  #include "Assets/Resources/Shaders/Chunks/hash.cginc"

  sampler2D _MainTex;
  
  uniform float4x4 _Transform;
  uniform int _NumberMeshes;

  float _Hue1;
  float _Hue2;
  float _Hue3;
  float _Hue4;

  float _IsBody;

  

  varyings SetUpOutputValues( int id  ){

        
    varyings o;

    int base = id / _TrisPerMesh;
    int alternate = id %_TrisPerMesh;
    Feather feather = _FeatherBuffer[base];
    
    int whichMesh = int(feather.featherType); //int(floor(hash(float(base)) * float(_NumberMeshes)));// %4;


    float4x4 baseMatrix = feather.ltw;
    float4x4 worldToLocal = InverseMatrix(baseMatrix);
    Vert v = _VertBuffer[_TriBuffer[alternate + whichMesh * _TrisPerMesh]];


    float3 pos = v.pos;

    if(feather.id > _NumShards){
      //pos *= 0;
    }


    o.localPos = pos;
    o.localCam = mul( worldToLocal, float4(_WorldSpaceCameraPos,1)).xyz;
    o.localRD = normalize(o.localCam - pos);
    

    // o.data1 = feather.newData1;
    o.worldPos = mul( baseMatrix , float4(pos,1)).xyz;//extra;
    o.id = float(base);
    o.feather = whichMesh;

    o.baseHue = _Hue1;

    o.hue = _Hue1;
    o.randID = feather.id;



    if( feather.featherColor == 1 ){ o.hue = _Hue2;}
    if( feather.featherColor == 2 ){ o.hue = _Hue3;}
    if( feather.featherColor == 3 ){ o.hue = _Hue4;}
    if( feather.featherColor == 4 ){ o.hue = _Hue4; } 

    o.collectionType = feather.type;


    


    //o.data1 = feather.newData1;
    o.nor = normalize(mul( baseMatrix , float4(v.nor,0)).xyz);
    o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
    o.uv = v.uv;
    o.eye = _WorldSpaceCameraPos - o.worldPos;
    UNITY_TRANSFER_SHADOW(o,o.worldPos);

    return o;

  }




  sampler2D _FullColorMap;
  #include "Assets/Resources/Shaders/Chunks/snoise.cginc"
  #include "Assets/Resources/Shaders/Chunks/triNoise3D.cginc"  
  sampler2D _BackgroundTexture1;




  float3 getNoiseTrace(float3 ro, float3 rd , float id){

    float3 fog;
    for( int i = 0; i < 30; i++ ){


      float3 fPos = ro -rd * float(i) * .01f;
      // fPos *= 10;

      fPos += float3(0,0.03,.25); 
      //fPos += 1000; 
      
      // fPos %= .03;
      //fPos -= .015;
      /*fPos *= float3(1,1,1);
      fPos %= .1;*/
      float v = triNoise3D(fPos * 3 +id,1,_Time.x);

      v *= v*v*10;

      if( length(fPos) < .04 + v * .08){
        v += 1;
      }

      v /= 40;

      if( v > .48 ){
        //  fog += hsv(0,0,1);
      }

      
      /*if( length(fPos) < .03 + v* .1){
        fog = hsv(float(i)/10,1,1);
        break;
      }*/
      fog += hsv ( float(i)/30, 1, v );

    }

    return fog;

  }