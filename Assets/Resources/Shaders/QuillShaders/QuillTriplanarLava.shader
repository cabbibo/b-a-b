// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Quill/TriplanarLava" {
  Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _BackfaceColor("BackfaceColor", Color )= (1,1,1,1)
    _Size ("Size", float) = .01
    _Fade ("Fade", float) = 1
    _FadeLocation ("_FadeLocation",Vector) = (0,0,0)
    
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}

    _NoiseFadeSize("_NoiseFadeSize",float) = 1
    _NoiseFadePower("_NoiseFadePower",float) = 1

    _TriplanarMap ("TriplanarMap", 2D) = "white" {}
    _TriplanarNormalMap ("TriplanarNormal", 2D) = "white" {}
    _TriplanarSharpness ("_TriplanarSharpness", float) = 1

    
    _TriplanarMultiplier ("TriplanarMultiplier", Vector) = (1,1,1)
    



  }


  SubShader{

    Pass{

      Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

      LOD 100



      
      Cull Off
      // inside Pass
      ZWrite On
      //Blend One One // Additive
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
      #include "../Chunks/noise.cginc"


      #include "../Chunks/triplanar.cginc"
      

      uniform float3 _Color;

      float _Fade;

      float3 _FadeLocation;


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
        UNITY_SHADOW_COORDS(8)
        UNITY_FOG_COORDS(10)
      };

      uniform float4x4 _Transform;
      uniform int _NumberMeshes;
      //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
      //which we transform with the view-projection matrix before passing to the pixel program.
      varyings vert (appdata_full vert){
        varyings o;
        
        o.worldPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;
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

      float _NoiseFadePower;
      float _NoiseFadeSize;

      float3 _WrenPos;
      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

        // fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);// * .5 + .5;
        
        //float m = saturate(dot( normalize(v.eye), v.nor));
        //float m2 = 1.2-dot( float3(0,1,0) , v.nor ) ;
        float3 col =.3;// shadow * .5 + .5;//* hsv( v.nor.y + _Time.x,1,1);// hsv(shadow, 1,1) * shadow;
        // col *= hsv( m * .5  + _Time.y * .2+ m2 * .3, 1.,1.);// * pow( (1-m),2);


        //float gridVal = max( (1-sin( v.uv.x * 1000)), (1- sin( v.uv.y * 1000)));
        // float noiseVal1 = noise(v.worldPos * .3);
        //float sinVal = clamp( (sin(v.worldPos.y * .6+ _Time.y + .3*noiseVal1) - .9) * 20   , 0,1);
      //col *= 1-sinVal;// ; }
      ///col *= gridVal * gridVal * gridVal * .2;


      //float3 neon = hsv(  m2 , 1.,1. )* (sinVal);
      //col = lerp( col , neon , 1-shadow );


      float dist  = length(_WrenPos- v.worldPos);
      
      // col = max( col , (sin(dist * 6 + noiseVal1 ) * 40  ) / dist);


      
      float3 triplanar = triplanarSample( v.worldPos , v.nor);

      //col *= 1;
      //col *=((floor( shadow * 2 + triplanar.x * .5 )/ 2) * .5 + .5);//(1-shadow);

      // col += saturate(pow(m,4) * 1) * float3(1,1,1);
      //col += saturate(pow((1-m),10)* 3)* float3(1,1,1);
      // col *= m2 +1; 

      
      float4 paintCol = tex2D(_PaintTexture,v.uv);
      
      
      float3 pCol = hsv(paintCol.x , 1 , paintCol.z);
      //col *= pCol * .8 + .6;//max(pCol , m2 );//saturate((gridVal-1.9)*10);


      // float3 triplanar = triplanarSample( v.worldPos , v.nor);
      col = v.color;

      col *= _Color;


      //col *=  ((floor( m * 2 + triplanar.x * .2 )/ 2) * .3 + 1);
      //col *= triplanarSample( v.worldPos , v.nor)  * .5 + .8;//* 3;

      if( length( triplanar ) < 1 ){
        //discard;
      }
      //col *=((floor( shadow * 2 + triplanar.x * .3 )/ 2) * .3 + .8);//(1-shadow);

      
      // col = max( col , (sin(dist * 6 + noiseVal1 ) * 5  ) / dist);
      //col += saturate(pow((1-m),10)* 1)* float3(1,.5,.5);
      
      float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
      float fwd = dot( normalize(viewDir) , normalize(v.eye));


      float fadeDist = length(v.worldPos - _FadeLocation);
      float fadeNoise = noise(v.worldPos * _NoiseFadeSize) * _NoiseFadePower;
      
      //fadeDist += noise(v.worldPos * _NoiseFadeSize) * _NoiseFadePower;
      float fadeDif = _Fade - (fadeDist+fadeNoise);
      if( fadeDif < 0 ){
        discard;
        }else{
        col *= saturate(fadeDif * .1);
      }

      col *= ( tex2D(_TriplanarMap , float2( (fadeDist + fadeNoise * .3) * .1  - _Time.x , .1) ).xyz * 2+ .3);
      col *= ((sin((fadeDist + fadeNoise * .4) - _Time.y)+1)/2) + .4;
      //col *= abs(((fadeDist * 100 + _Time.y * 10) % 10)/10 );
      

      //col = 1;
      //  if( fwd > length(v.eye) * .03  ){ discard; }
      // if( sin(v.worldPos.x * .1 ) > -.9 && sin(v.worldPos.z * .1 ) > -.9 ){ col = 0;}

      // UNITY_APPLY_FOG(v.fogCoord, col);
      return float4(col,length(col));
    }

    ENDCG

  }


  







}


}




