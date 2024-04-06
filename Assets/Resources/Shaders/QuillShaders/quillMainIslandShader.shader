// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Quill/quillMainIslandShader" {
    Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _BackfaceColor("BackfaceColor", Color )= (1,1,1,1)
    _Size ("Size", float) = .01
    _Fade ("Fade", float) = 1
    _FadeLocation ("_FadeLocation",Vector) = (0,0,0)
    _WindDirection ("_WindDirection",Vector) = (1,0,0)
    _WindAmount ("_WindAmount",float) = 1
    _WindChangeSpeed ("_WindChangeSpeed",float) = 1
    _WindChangeSize ("_WindChangeSize",float) = 1

    
         _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}

    



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
// int instanceID = UNITY_GET_INSTANCE_ID(vert);
     float3 wPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;
//UNITY_VERTEX_INPUT_INSTANCE_ID

int instanceID = 0;
float flooredTime =  floor(_Time.y *_WindChangeSpeed);
#ifdef INSTANCING_ON
    instanceID = vert.instanceID;

    flooredTime = floor(_Time.y *_WindChangeSpeed + float(vert.instanceID) * .4);
#endif

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
    float3 col;


  float noiseVal2 = snoise(v.worldPos * 1.3);

  
  float camDist = length(v.worldPos - _WorldSpaceCameraPos);


    col = v.color;

    float m3 = saturate(dot( normalize(_WorldSpaceLightPos0.xyz), v.nor));
  col *= (floor((m3+1) *2 )/4)* .8 + .2;

   // Discards around bird!

  float capDistance =sdCapsule(v.worldPos , _WorldSpaceCameraPos , _WrenPos , 1 );
  capDistance -= noiseVal2 * .2;

  if( capDistance < 0 ){
    discard;
  }else{
    //col *= saturate(capDistance * 10);
  }
    col *= (shadow * .5 + .5);
    col *= _Color;


    float shadowStep = floor(shadow * 3)/3;

    //float 

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
shadowCol += .3;
    shadowCol *= float3(.1 , .3 , .6);
    shadowCol /= clamp( (.1 + .1* length( v.eye)), 1, 3);
    col = shadowStep * col * float3(1,.8,.6) +  clamp( (1-shadowStep) * length(col) * length(col) * 10 , 0.05, 1) * shadowCol+.05*float3(.35,0.4,1)*(1-shadowStep) *floor( pow(1-m,4) * 5)/5;// float3(.1,.2,.5);


    float b = length(col);

    col = col;//saturate(length(shadowCol)* .5 + .2) * 10*normalize( col) * b * b * 6;

    //col *= col * 4;

    
                float dist  = length(_WrenPos- v.worldPos);
float _DistanceMin = 0.1;
float _DistanceMax = .5;
      float mult = 1-smoothstep( dist , _DistanceMin, _DistanceMax);

  col *= mult* 10 + 1;

  col = v.color * _Color;
  col *= shadow * shadow * shadow * shadow;;

  col *= hsv( .3 * floor(m3 * 6 ) /6, .6,1);


  col = floor(m3 * 6 ) /6;
 // col += .4*(1-shadow) * hsv(.3 * floor(m3 * 6 ) /6  + .5, .5,1);// float3(0,0,1);
 // col *= noise(v.worldPos * 1) * .8 + .2;
    

    return float4(col,1);
}

      ENDCG

    }


  
/*
  Pass
        {
            Tags{ "LightMode" = "ShadowCaster" }


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



 
            #pragma multi_compile_instancing

      #include "AutoLight.cginc"
    

      #include "../Chunks/noise.cginc"
#include "../Chunks/snoise3D.cginc"

        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)

      uniform float3 _Color;

    float _Fade;

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
         
                V2F_SHADOW_CASTER;
                float3 nor : NORMAL;  
                UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
         
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
// int instanceID = UNITY_GET_INSTANCE_ID(vert);
     float3 wPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;
//UNITY_VERTEX_INPUT_INSTANCE_ID

int instanceID = 0;
float flooredTime =  floor(_Time.y *_WindChangeSpeed);
#ifdef INSTANCING_ON
    instanceID = vert.instanceID;

    flooredTime = floor(_Time.y *_WindChangeSpeed + float(vert.instanceID) * .4);
#endif

    float3 windDirection = float3(1,0,0);

    float3 noiseVal = snoise( wPos * _WindChangeSize + windDirection * flooredTime);

        float3 worldPos = wPos + _WindDirection * noiseVal * _WindAmount;//windAmount;
      

        float3 nor = normalize(mul( unity_ObjectToWorld,  float4(vert.normal,0)).xyz);


                o.nor = normalize( nor);
                float4 position = ShadowCasterPos(worldPos, normalize(nor));
                o.pos = UnityApplyLinearShadowBias(position);
                return o;
            }





            float4 frag(varyings i) : COLOR
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }


*/



        /*Pass
        {
Tags { "RenderType"="ShadowCaster" }
            CGPROGRAM
            // Use the shadow caster vertex and fragment shaders
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

            #pragma multi_compile_instancing


            
          //  #pragma vertex vert
           // #pragma fragment frag
            #pragma target 4.5
            // make fog work
            //#pragma multi_compile_fogV
 //#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

 

      #include "UnityCG.cginc"
      #include "AutoLight.cginc"
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)
 
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                   UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
            };


uniform float4x4 _Transform;
uniform int _NumberMeshes;
float3 _WindDirection;
float _WindAmount;
float _WindChangeSpeed;
float _WindChangeSize;

    
#include "../Chunks/ShadowCasterPos.cginc"
#include "../Chunks/noise.cginc"
#include "../Chunks/snoise3D.cginc"


            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata vert)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(vert);
              //  UNITY_TRANSFER_INSTANCE_ID(vert, o); // necessary only if you want to access instanced properties in the fragment Shader.
// int instanceID = UNITY_GET_INSTANCE_ID(vert);
     float3 wPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;
//UNITY_VERTEX_INPUT_INSTANCE_ID

int instanceID = 0;
float flooredTime =  floor(_Time.y *_WindChangeSpeed);
#ifdef INSTANCING_ON
    instanceID = vert.instanceID;

    flooredTime = floor(_Time.y *_WindChangeSpeed + float(vert.instanceID) * .4);
#endif

    float3 windDirection = float3(1,0,0);

    float3 noiseVal = snoise( wPos * _WindChangeSize + windDirection * flooredTime);

          float3 worldPos = wPos + _WindDirection * noiseVal * _WindAmount;//windAmount;
        

       wPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;
         worldPos = wPos;
                //float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 nor = normalize(mul(unity_ObjectToWorld, float4(vert.normal, 0)).xyz);
                worldPos -= .1*nor; //o.pos = mul (UNITY_MATRIX_VP, float4(worldPos,1.0f));
                o.pos =  mul (UNITY_MATRIX_VP,float4(worldPos,1));//ShadowCasterPos(worldPos, nor);//
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Simply return black, shadows don't need color
                return float4(0, 0, 0, 1);
            }
            ENDCG
        }
*/

  // shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
float3 _WindDirection;
float _WindAmount;
float _WindChangeSpeed;
float _WindChangeSize;

    
#include "../Chunks/ShadowCasterPos.cginc"
#include "../Chunks/noise.cginc"
#include "../Chunks/snoise3D.cginc"


            struct v2f { 
                V2F_SHADOW_CASTER;
            };
          


          float4 CustomShadowPos(float4 vertex, float3 normal)
{
    float4 wPos = vertex;
    float3 wNormal = normal;

    if (unity_LightShadowBias.z != 0.0)
    {
     
        float3 wLight = normalize(UnityWorldSpaceLightDir(wPos.xyz));

        // apply normal offset bias (inset position along the normal)
        // bias needs to be scaled by sine between normal and light direction
        // (http://the-witness.net/news/2013/09/shadow-mapping-summary-part-1/)
        //
        // unity_LightShadowBias.z contains user-specified normal offset amount
        // scaled by world space texel size.

        float shadowCos = dot(wNormal, wLight);
        float shadowSine = sqrt(1-shadowCos*shadowCos);
        float normalBias = unity_LightShadowBias.z * shadowSine;

        wPos.xyz -= wNormal * normalBias;
    }

    return mul(UNITY_MATRIX_VP, wPos);
}


      #include "UnityCG.cginc"
      #include "AutoLight.cginc"
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)
 
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                   UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
            };


            v2f vert(appdata_base v)
            {
                v2f o;

                //float4 p = UnityObjectToClipPos(v.vertex);
                float4 p = float4( v.vertex.xyz, 1);
               // TRANSFER_SHADOW_CASTER_NOPOS(o,o.pos);

               
                UNITY_SETUP_INSTANCE_ID(vert);
              //  UNITY_TRANSFER_INSTANCE_ID(vert, o); // necessary only if you want to access instanced properties in the fragment Shader.
// int instanceID = UNITY_GET_INSTANCE_ID(vert);
     float3 wPos = mul( unity_ObjectToWorld,  float4(v.vertex.xyz,1)).xyz;
//UNITY_VERTEX_INPUT_INSTANCE_ID

int instanceID = 0;
float flooredTime =  floor(_Time.y *_WindChangeSpeed);
#ifdef INSTANCING_ON
    instanceID = v.instanceID;

    flooredTime = floor(_Time.y *_WindChangeSpeed + float(v.instanceID) * .4);
#endif

    float3 windDirection = float3(1,0,0);

    float3 noiseVal = snoise( wPos * _WindChangeSize + windDirection * flooredTime);

          float4 worldPos = float4(wPos + _WindDirection * noiseVal * _WindAmount,1);//windAmount;
        




        //float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
        float3 worldNormal  =  UnityObjectToWorldNormal(v.normal);

            float4 opos = 0;
            opos = CustomShadowPos(worldPos, worldNormal);
            opos = UnityApplyLinearShadowBias(opos);
            o.pos = opos;


                //TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }


    }

//Fallback "Diffuse"


}









