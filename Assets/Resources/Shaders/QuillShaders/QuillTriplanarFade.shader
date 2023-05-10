// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Quill/TriplanarTextureFade" {
    Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _BackfaceColor("BackfaceColor", Color )= (1,1,1,1)
    _Size ("Size", float) = .01
    _Fade ("Fade", float) = 1
    _FadeLocation ("_FadeLocation",Vector) = (0,0,0)
    
         _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}


    _TriplanarMap ("TriplanarMap", 2D) = "white" {}
    _TriplanarNormalMap ("TriplanarNormal", 2D) = "white" {}
    _TriplanarSharpness ("_TriplanarSharpness", float) = 1

    
    _TriplanarMultiplier ("TriplanarMultiplier", Vector) = (1,1,1)

    
    _WindDirection ("_WindDirection",Vector) = (1,0,0)
    _WindAmount ("_WindAmount",float) = 1
    _WindChangeSpeed ("_WindChangeSpeed",float) = 1
    _WindChangeSize ("_WindChangeSize",float) = 1
    



    }


  SubShader{

    Pass{

        Tags { "RenderType"="Opaque" }
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
 
            #pragma multi_compile_instancing

      #include "UnityCG.cginc"
      #include "AutoLight.cginc"
    

      #include "../Chunks/hsv.cginc"
      #include "../Chunks/noise.cginc"
#include "../Chunks/snoise3D.cginc"

        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)

      uniform float3 _Color;

    float _Fade;
    float _Multiplier;

  

    float3 _FadeLocation;
struct appdata_full2 {
    float4 vertex : POSITION;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
    fixed4 color : COLOR;
     uint   id                : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
    
};

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
          float4 color : TEXCOORD11;
          float id        : TEXCOORD5;
          int feather:TEXCOORD7;
          float4 data1:TEXCOORD9;   
           UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
         
           UNITY_SHADOW_COORDS(8)
                UNITY_FOG_COORDS(10)
      };

uniform float4x4 _Transform;
uniform int _NumberMeshes;

float3 _WindDirection;
float _WindAmount;
float _WindChangeSpeed;
float _WindChangeSize;

//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (appdata_full2 vert){
    varyings o;

         UNITY_SETUP_INSTANCE_ID(vert);
                UNITY_TRANSFER_INSTANCE_ID(vert, o); // necessary only if you want to access instanced properties in the fragment Shader.
 int instanceID = 0;

#ifdef INSTANCING_ON
    instanceID = vert.instanceID;
#endif


    float flooredTime = floor(_Time.y *_WindChangeSpeed + float(0) * .4);

     float3 wPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;

    //  o.worldPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;

          float3 windDirection = float3(1,0,0);

    float3 noiseVal = snoise( wPos * _WindChangeSize + windDirection * flooredTime);

          o.worldPos = wPos + _WindDirection * noiseVal * _WindAmount;//windAmount;
      

      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
      o.eye = _WorldSpaceCameraPos - o.worldPos;
      o.nor = normalize(mul( unity_ObjectToWorld,  float4(vert.normal,0)).xyz);
      o.uv = vert.texcoord.xy;
      o.color = vert.color;
  UNITY_TRANSFER_SHADOW(o,o.worldPos);
  UNITY_TRANSFER_FOG(o,o.pos);
  

  return o;

}


uniform sampler2D _PaintTexture;

#include "../Chunks/triplanar.cginc"


float sdCapsule( float3 p, float3 a, float3 b, float r )
{
    float3 pa = p - a, ba = b - a;
    float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
    return length( pa - ba*h ) - r;
}

float3 _WrenPos;
//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {

  fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);// * .5 + .5;
 
    float m = saturate(dot( normalize(v.eye), v.nor));
    float m2 = 1.2-dot( float3(0,1,0) , v.nor ) ;
    float3 col =.3;// shadow * .5 + .5;//* hsv( v.nor.y + _Time.x,1,1);// hsv(shadow, 1,1) * shadow;
   // col *= hsv( m * .5  + _Time.y * .2+ m2 * .3, 1.,1.);// * pow( (1-m),2);


  float gridVal = max( (1-sin( v.uv.x * 1000)), (1- sin( v.uv.y * 1000)));
  float noiseVal1 = snoise(v.worldPos * .3);
  float noiseVal2 = snoise(v.worldPos * 11.3);
    float sinVal = clamp( (sin(v.worldPos.y * .6+ _Time.y + .3*noiseVal1) - .9) * 20   , 0,1);
    //col *= 1-sinVal;// ; }
    ///col *= gridVal * gridVal * gridVal * .2;


//float3 neon = hsv(  m2 , 1.,1. )* (sinVal);
//col = lerp( col , neon , 1-shadow );55


                float dist  = length(_WrenPos- v.worldPos);
                
                col = max( col , (sin(dist * 6 + noiseVal1 ) * 40  ) / dist);

  col *= v.color;
                
  float3 triplanar = triplanarSample( v.worldPos , v.nor);

  col *= 1;
   col *=((floor( shadow * 2 + triplanar.x * .5 )/ 2) * .5 + .5);//(1-shadow);

  // col += saturate(pow(m,4) * 1) * float3(1,1,1);
   col += saturate(pow((1-m),10)* 3)* float3(1,1,1);
    col *= m2 +1; 



  
  float camDist = length(v.worldPos - _WorldSpaceCameraPos);



  float4 paintCol = tex2D(_PaintTexture,v.uv);
 
 
    float3 pCol = hsv(paintCol.x , 1 , paintCol.z);
    col *= pCol * .8 + .6;//max(pCol , m2 );//saturate((gridVal-1.9)*10);


 // float3 triplanar = triplanarSample( v.worldPos , v.nor);
    col = v.color;

   // col = 10/dist;

  float capDistance =sdCapsule(v.worldPos , _WorldSpaceCameraPos , _WrenPos , 1 );
  capDistance -= noiseVal2 * .2;

  if( capDistance < 0 ){
    discard;
  }else{
    //col *= saturate(capDistance * 10);
  }
    col *= _Color;


    col *=  ((floor( m * 2 + triplanar.x * .2 )/ 2) * .3 + 1);
    //col *= triplanarSample( v.worldPos , v.nor)  * .5 + .8;//* 3;

  if( length( triplanar ) < 1 ){
    //discard;
  }
    col *=((floor( shadow * 2 + triplanar.x * .3 )/ 2) * .3 + .8);//(1-shadow);

  
   
               // col = max( col , (sin(dist * 6 + noiseVal1 ) * 5  ) / dist);
    //col += saturate(pow((1-m),10)* 1)* float3(1,.5,.5);
  
    float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
    float fwd = dot( normalize(viewDir) , normalize(v.eye));


    float fadeDist = length(v.worldPos - _FadeLocation);
    fadeDist += noise(v.worldPos * .1) * 60;

    float fadeDif = _Fade - fadeDist;
    if( fadeDif < 0 ){
        discard;
    }else{

        if( fadeDif < 30 ){

          float fVal = ((30-fadeDif)/30);
          fVal = min( fVal * 1 , (1-fVal)*100);
          col *= (fVal*4+1);
        }else{

       // col *= saturate((fadeDif-30) * .1);
        }
    }



    float shadowStep = shadow;//floor(shadow * 3)/3;

  float3 shadowCol = 0;
  
    for( int i = 0; i < 3; i++){

      float3 fPos = v.worldPos - normalize(v.eye) * float(i) * 1.3;
      float v = (snoise(fPos * 10)+1)/2;
      shadowCol += hsv((float)i/3,1,v);

    
    }//
    shadowCol *= shadowCol;
    shadowCol *= shadowCol;
    shadowCol *= shadowCol;
    shadowCol *= shadowCol;

    shadowCol = length(shadowCol) * (shadowCol * .8 + .3)  * 10;//

//shadowCol += .5;
    shadowCol *= float3(.1 , .3 , .6);

    shadowCol /=  clamp( (.1 + .1* length( v.eye)), 2, 3);
    col = shadowStep * col * float3(1,1,.6)  * (shadowCol * 1+.9)  * triplanar+  pow((1-m),1) * 2 * clamp( (1-shadowStep) * shadowCol *.4 * col * 20  , 0, 1)+.05*float3(.4,0.3,1)*(1-shadowStep) *floor( pow(1-m,4) * 5)/5;;// float3(.1,.2,.5);




   // col = col * shadow * float3(1,1,.8) + (1-shadow) * col * float3(.2,.4,1);//(shadow * .5 + .5);

    float b = length(col);

  //col = length(shadowCol )* 20 *normalize( col) * (b * b * b * 1 * _Multiplier);
  col = 20*normalize( col) * (b * b * b * 1 * _Multiplier);


float _DistanceMin = 0.1;
float _DistanceMax = .5;
      float mult = 1-smoothstep( dist , _DistanceMin, _DistanceMax);

  col *= mult* 100 + 1;


   // col = v.color;

  
  //col *= col * 2 + .2;
    
        
   //if( fwd > length(v.eye) * 1  ){ discard; }
   // if( sin(v.worldPos.x * .1 ) > -.9 && sin(v.worldPos.z * .1 ) > -.9 ){ col = 0;}

   // UNITY_APPLY_FOG(v.fogCoord, col);
    return float4(col,1);
}

      ENDCG

    }


  







    }

Fallback "Diffuse"


}












