// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Quill/quillMainIslandShader_test" {
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

        Tags { "RenderType"="Opaque+1" }
        LOD 100 
        Cull Back
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


   // float ddxWorld = length(ddx(v.worldPos));
   // float ddyWorld = length(ddy(v.worldPos));

   // float maxDDX = max(ddxWorld,ddyWorld);


    

  fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);// * .5 + .5;
 
    float3 col = 0;


    //float 

  float3 traceColor = 0;
  
    for( int i = 0; i < 3; i++){

      float3 fPos = v.worldPos - normalize(v.eye) * float(i) * 1.3;
      float v = (snoise(fPos * 10)+1)/2;
      traceColor += hsv((float)i/3,1,v);

    
    }//


    float lightMatch = dot(v.nor, _WorldSpaceLightPos0.xyz);

    float reflMatch = saturate(dot( reflect( -normalize(v.eye), v.nor), _WorldSpaceLightPos0.xyz));

 

    float m = saturate(dot( v.nor, normalize(v.eye) ));

    
  col = v.color * _Color;
  col *= shadow * shadow * shadow * shadow;;


  col = shadow;
  //col = length(traceColor) * ( shadow * shadow );;

 col = (tex2D(_MainTex, v.uv * 3)* .1 + .9) * col * v.color * 2;
 col *= hsv(.4 +.2*floor(lightMatch * 6)/6,.3,1);
 col = floor( pow(reflMatch,8) *30)/6 ;
 col += .4*floor(pow(1-m,4) * 6 /6);
 //col 

 //col = m;


 if( m < length(traceColor) * .5 ){
 //   discard;
 }
 // col *= noise(v.worldPos * 1) * .8 + .2;
    
//col = v.color;
    return float4(col,1);
}

      ENDCG

    }

















  
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









