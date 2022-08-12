Shader "Unlit/quillTerrain"{
    Properties {

    /*_Color ("Color", Color) = (1,1,1,1)
    _BackfaceColor("BackfaceColor", Color )= (1,1,1,1)
    _Size ("Size", float) = .01
    _Fade ("Fade", float) = 1
    _FadeLocation ("_FadeLocation",Vector) = (0,0,0)
    


    _TriplanarMap ("TriplanarMap", 2D) = "white" {}
    _TriplanarNormalMap ("TriplanarNormal", 2D) = "white" {}
    _TriplanarSharpness ("_TriplanarSharpness", float) = 1

    
    _TriplanarMultiplier ("TriplanarMultiplier", Vector) = (1,1,1)*/
    

        [HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        [HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
        [HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
        [HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
        [HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
        [HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)



    }


  SubShader{

    Pass{

        Tags { "RenderType"="Opaque" }
        LOD 100 
        Cull Off
        Tags{ "LightMode" = "ForwardBase" "TerrainCompatible" = "True"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            // make fog work
            #pragma multi_compile_fogV
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
 
 
            #pragma multi_compile_instancing

       // #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
            //#pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            

            #include "../Chunks/hsv.cginc"
            #include "../Chunks/noise.cginc"





            struct appdata_full2 {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                float4 texcoord3 : TEXCOORD3;
                fixed4 color : COLOR;
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
          float4 data1:TEXCOORD9;   
          float4 tc:TEXCOORD12;
      //    float3 debug : TEXCOORD13;
           UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
         
           UNITY_SHADOW_COORDS(8)
            UNITY_FOG_COORDS(10)
      };




        half _Metallic0;
        half _Metallic1;
        half _Metallic2;
        half _Metallic3;

        half _Smoothness0;
        half _Smoothness1;
        half _Smoothness2;
        half _Smoothness3;

        float4 _MainTex_ST;
    
/*struct Input
{
    float4 tc;
    #ifndef TERRAIN_BASE_PASS
        UNITY_FOG_COORDS(0) // needed because finalcolor oppresses fog code generation.
    #endif
};*/

sampler2D _Control;
float4 _Control_ST;
float4 _Control_TexelSize;
sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

    sampler2D _TerrainHeightmapTexture;
    sampler2D _TerrainNormalmapTexture;
    float4    _TerrainHeightmapRecipSize;   // float4(1.0f/width, 1.0f/height, 1.0f/(width-1), 1.0f/(height-1))
    float4    _TerrainHeightmapScale;       // float4(hmScale.x, hmScale.y / (float)(kMaxHeight), hmScale.z, 0.0f)


UNITY_INSTANCING_BUFFER_START(Terrain)
    UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData) // float4(xBase, yBase, skipScale, ~)
UNITY_INSTANCING_BUFFER_END(Terrain)

    sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
    float _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;


    sampler2D _TerrainHolesTexture;



uniform float4x4 _Transform;
uniform int _NumberMeshes;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (appdata_full2 v){
    varyings o;

        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.
  
   UNITY_INITIALIZE_OUTPUT(varyings, o);
 
    float2 patchVertex = v.vertex.xy;
    float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);

    float4 uvscale = instanceData.z * _TerrainHeightmapRecipSize;
    float4 uvoffset = instanceData.xyxy * uvscale;
    uvoffset.xy += 0.5f * _TerrainHeightmapRecipSize.xy;
    float2 sampleCoords = (patchVertex.xy * uvscale.xy + uvoffset.xy);

    float hm = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(sampleCoords, 0, 0)));
   // v.vertex.xz = (patchVertex.xy + instanceData.xy) * _TerrainHeightmapScale.xz * instanceData.z;  //(x + xBase) * hmScale.x * skipScale;
   // v.vertex.y = hm * _TerrainHeightmapScale.y;
   // v.vertex.w = 1.0f;



    //o.debug.xy = v.texcoord.xy;
    //v.texcoord.xy = (patchVertex.xy * uvscale.zw + uvoffset.zw);
    v.texcoord3 = v.texcoord2 = v.texcoord1 = v.texcoord;
    
    //v.normal = float3(0, 1, 0); // TODO: reconstruct the tangent space in the pixel shader. Seems to be hard with surface shader especially when other attributes are packed together with tSpace.
    //o.tc.zw = sampleCoords;
        
  
    //v.tangent.xyz = cross(v.normal, float3(0,0,1));
    //v.tangent.w = -1;

  //  o.debug.xy = v.texcoord.xy;

   // o.tc.xy = v.texcoord.xy;//TRANSFORM_TEX(v.texcoord.xy, _MainTex);

        
      o.worldPos = mul( unity_ObjectToWorld,  float4(v.vertex.xyz,1)).xyz;
      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
      o.eye = _WorldSpaceCameraPos - o.worldPos;
      o.nor = normalize(mul( unity_ObjectToWorld,  float4(v.normal,0)).xyz);
      o.uv = v.texcoord.xy;
      o.tc = v.texcoord;
      o.color = v.color;

      //o.debug = instanceData.xyz;
        UNITY_TRANSFER_SHADOW(o,o.worldPos);
        UNITY_TRANSFER_FOG(o,o.pos);
        

  return o;

}


uniform sampler2D _PaintTexture;

#include "../Chunks/triplanar.cginc"
#include "../Chunks/snoise3D.cginc"
#include "../Chunks/triNoise3D.cginc"

    void ClipHoles(float2 uv)
    {
        float hole = tex2D(_TerrainHolesTexture, uv).r;
        clip(hole == 0.0f ? -1 : 1);
    }


void SplatmapMix(varyings IN,  half4 defaultAlpha, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal)
{
        ClipHoles(IN.tc.xy);


    // adjust splatUVs so the edges of the terrain tile lie on pixel centers
    float2 splatUV = (IN.tc.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
    splat_control = tex2D(_Control, splatUV);
    weight = dot(splat_control, half4(1,1,1,1));



    // Normalize weights before lighting and restore weights in final modifier functions so that the overal
    // lighting result can be correctly weighted.
    splat_control /= (weight + 1e-3f);

    float2 uvSplat0 = TRANSFORM_TEX(IN.tc.xy, _Splat0);
    float2 uvSplat1 = TRANSFORM_TEX(IN.tc.xy, _Splat1);
    float2 uvSplat2 = TRANSFORM_TEX(IN.tc.xy, _Splat2);
    float2 uvSplat3 = TRANSFORM_TEX(IN.tc.xy, _Splat3);

    mixedDiffuse = 0.0f;
        mixedDiffuse += splat_control.r * tex2D(_Splat0, uvSplat0)* half4(1.0, 1.0, 1.0, defaultAlpha.r);
        mixedDiffuse += splat_control.g * tex2D(_Splat1, uvSplat1)* half4(1.0, 1.0, 1.0, defaultAlpha.g);
        mixedDiffuse += splat_control.b * tex2D(_Splat2, uvSplat2)* half4(1.0, 1.0, 1.0, defaultAlpha.b);
        mixedDiffuse += splat_control.a * tex2D(_Splat3, uvSplat3)* half4(1.0, 1.0, 1.0, defaultAlpha.a);

        mixedNormal  = UnpackNormalWithScale(tex2D(_Normal0, uvSplat0), _NormalScale0) * splat_control.r;
        mixedNormal += UnpackNormalWithScale(tex2D(_Normal1, uvSplat1), _NormalScale1) * splat_control.g;
        mixedNormal += UnpackNormalWithScale(tex2D(_Normal2, uvSplat2), _NormalScale2) * splat_control.b;
        mixedNormal += UnpackNormalWithScale(tex2D(_Normal3, uvSplat3), _NormalScale3) * splat_control.a;
        mixedNormal.z += 1e-5f; // to avoid nan after normalizing


        float3 geomNormal = IN.nor;//normalize(tex2D(_TerrainNormalmapTexture, IN.tc.zw).xyz * 2 - 1);
   
            float3 geomTangent = normalize(cross(geomNormal, float3(0, 0, 1)));
            float3 geomBitangent = normalize(cross(geomTangent, geomNormal));
            mixedNormal = mixedNormal.x * geomTangent
                          + mixedNormal.y * geomBitangent
                          + mixedNormal.z * IN.nor;

}


float2 rotateUV( float amount , float2 uv ){
          float sinX = sin ( amount );
            float cosX = cos ( amount );
            float sinY = sin ( amount );
            float2x2 rotationMatrix = float2x2( cosX, -sinX, sinY, cosX);

            return  mul(rotationMatrix , uv);;


}

float3 brightnessContrast(float3 value, float brightness, float contrast)
{
    return (value - 0.5) * contrast + 0.5 + brightness;
}








/*



sanddddd


*/

float3 DoSandColor(float3 pos, float3 baseNor, float3 nor, float3 eye ,float shadow){

  float3 col = float3(1, .6,.3);


float3 refl = normalize(reflect( -eye, nor  ));



  half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, refl);
                // decode cubemap data into actual color
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);



  float3 spec = dot(refl, _WorldSpaceLightPos0.xyz); 
  col +=  float3(1,.4,.3)  * floor( pow(spec,10) * 3) * 1.3;


  float m = saturate( dot( normalize(eye), baseNor));





  //float3 cubeCol = texCube( _CubeMap,refl);



  float sparkleSize = 1;
  float sparkleForce = .3;
  refl = reflect( -eye, normalize( nor + float3(snoise(pos*sparkleSize),0,0)*sparkleForce + float3(0,0,snoise(pos * 1.2*sparkleSize + 13103))*sparkleForce )  );
  spec = saturate(dot(normalize(refl) , _WorldSpaceLightPos0.xyz)); 
 // col += col * floor( pow(spec,100) * 3) *3;


col += float3(1,.3,.1)* floor( pow(spec,100) * 3) *10;




  // canyon
 if( dot( nor , float3(0,1,0)) < .5 ){

    col = float3(1,.3,.1);
    col *= floor(noise( float3(pos.xz,pos.y* 30) * .1 /*+noise( float3(pos.xz * 1,pos.y* 100)) * 1*/)*4)/4;
    //col *= floor(noise( float3(pos.xz,pos.y* 30) * .1 )*4)/4;

  }

  float lMap = dot(nor ,_WorldSpaceLightPos0.xyz);

lMap *= shadow;


col *= (floor(lMap * 4 ) /4 + .3);




  col += float3(1,.6,.3)*2*floor(pow(saturate((1-m)),10) * 4)/4;


  col = saturate(col) * .95;
  return col;


};










float3 DoRockColor(float3 pos,float3 baseNor, float3 nor, float3 eye ,float shadow){

 


float lMap = dot(nor ,_WorldSpaceLightPos0.xyz);
float lMapB = dot(baseNor ,_WorldSpaceLightPos0.xyz);

lMap *= shadow;

//float vertness = 0;//dot( nor , float3(0,1,0));

  float vertness = dot( nor , float3(0,1,0));
  float baseVert = dot( baseNor , float3(0,1,0));
float3 col = .3;//float3(.4,.5,.2);//float3(0,0,0);//float3(1,1,.3);

float fV = 0;
  for( int i =0; i < 3; i++){

    float fi = float(i+1);
   
    float3 fPos = pos - normalize(eye) * fi *3;
    float3 lightPos = fPos - _WorldSpaceLightPos0.xyz ;

    float v = triNoise3D( fPos * .01 + fi * 113.3 + vertness*.1, 0 , _Time.y)  * fi/5;
    float v2 = triNoise3D( lightPos * .01 + fi * 113.3  + vertness*.1, 0 , _Time.y)  * fi/5;


  //float4 tMap = tex2D(_Splat2, rotateUV(fPos.y * .004, fPos.xz * .01)+ fi * .1) * .2;
  float4 tMap =0;// tex2D(_Splat1, fPos.xz * .04 + fi * .1) * .2;
   tMap += tex2D(_Splat1,  rotateUV(fPos.y * .001 ,fPos.xz * .03 + fi * .1)) * .2;

  fV  += tMap.a * .2;
  fV += tMap.r * .5;
      col += pow(tMap.g ,2)* float3(.5,.5,.5) * 500;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;
      //col += pow(tMap.r,2) * float3(.6,.4,.4) * .2;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;


float delta = saturate(-1*(v2-v));
    //fV += saturate(-1*(v2-v)) * 40;
    //if( v > .1){


      if( delta > .02* (1-lMap) * (1-fi/4)){
       // col = float3(.6,.8,.4) * ((1-fi/5) * .5 +.5);
        //break;
      }
  // col += saturate(-1*(v2-v))*10 * float3(1,1,.3);//float3(.4,1,.4) * (1-fi/5);//(1-(fi/5));
    //  break;
    //}


  } 

  float m4 = col.x;

  col = floor( col * 4)/4;
  col *= .1;
  col += .5;

  //col *= 20;



float rainExtra = floor( (triNoise3D( pos  * .0005 ,1,_Time.y * .01)+1) * 1000 ) /1000;

col *= (floor(triNoise3D( pos * .001,0,0) * 6) / 6) *1 + .4;

  if( baseVert > .95 ){
    col *= 4*float3(.3,1,.1);
  }

  if(rainExtra < 1.1 ){
    col = hsv( m4 * .01 + .3 , 1,m4* .1);//brightnessContrast(col, .3 , 11);
  }



  if( pos.y + triNoise3D(pos * .004,0,0) * 200 - lMapB * 100 > 400 &&  ( baseVert  + pos.y * .0001> .5 )){
    col = 1;
  }
  //col *= 5;
 // col = float3(.7,.8,.4);

 // col /= 5;




//col = floor((fV * 4)/4) * float3(.3,1,.5) * 1;

//col +=float3(.6,1,.5) * .4;

col *= (floor(lMap * 4 ) /4  + .3);



  return col;


};






/*
 mEADOW

*/


float3 DoMeadowColor(float3 pos, float3 baseNor, float3 nor, float3 eye ,float shadow){
 

    


float lMap = dot(nor ,_WorldSpaceLightPos0.xyz);

lMap *= shadow;

  float vertness = dot( nor , float3(0,1,0));
  float baseVert = dot( baseNor , float3(0,1,0));

float3 col = 0;//float3(.4,.5,.2);//float3(0,0,0);//float3(1,1,.3);

float fV = 0;
  for( int i =0; i < 3; i++){

    float fi = float(i);
   
    float3 fPos = pos - normalize(eye) * fi *1;
    float3 lightPos = fPos - _WorldSpaceLightPos0.xyz ;

    float v = triNoise3D( fPos * .01 + fi * 113.3 + vertness*.1, 0 , _Time.y)  * fi/5;
    float v2 = triNoise3D( lightPos * .01 + fi * 113.3  + vertness*.1, 0 , _Time.y)  * fi/5;


float delta = saturate(-1*(v2-v));

  float4 tMap = tex2D(_Splat2, rotateUV(fPos.y * .004 +delta, fPos.xz * .01)+ fi * .1) * .2;

  fV  += tMap.a * .2;
  fV += tMap.r * .5;
      col += tMap.a * float3(.5,1,.4) * .1;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;
      col += tMap.r * float3(1,1,.4) * .2;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;


    //fV += saturate(-1*(v2-v)) * 40;
    //if( v > .1){


      if( delta > .02* (1-lMap) * (1-fi/4)){
       // col = float3(.6,.8,.4) * ((1-fi/5) * .5 +.5);
        //break;
      }
  // col += saturate(-1*(v2-v))*10 * float3(1,1,.3);//float3(.4,1,.4) * (1-fi/5);//(1-(fi/5));
    //  break;
    //}


  }

  //col *= 5;
  //col = float3(.7,.8,.4);

  //col = nor;
 // col /= 5;

//col += .1;

float rainExtra = floor( triNoise3D( pos  * .0005 ,1,_Time.y * .01) * 1000 ) /1000;


col *= .6 + 3*rainExtra * float3(1.3,2*rainExtra* 3,1.4);//floor( triNoise3D( pos  * .0001 ,1,0) * 5 ) /5;

if( baseVert > .8 && baseVert < .9){
  
  col *= 2;
}
 if( baseVert > .9){
  col *= 3;
 }


if( baseVert < .6){
  col *= float3(3,.5,1);
}


//col = floor((fV * 4)/4) * float3(.3,1,.5) * 1;

//col +=float3(.6,1,.5) * .4;

col *= (floor(lMap * 4 ) /4  + .3);



  return col;
};













float3 DoForestColor(float3 pos, float3 baseNor, float3 nor, float3 eye ,float shadow){
 

    


float lMap = dot(nor ,_WorldSpaceLightPos0.xyz);

lMap *= shadow;

  float vertness = dot( nor , float3(0,1,0));
  float baseVert = dot( baseNor , float3(0,1,0));

float3 col = 0;//float3(.4,.5,.2);//float3(0,0,0);//float3(1,1,.3);

float fV = 0;
  for( int i =0; i <3; i++){

    float fi = float(i);
   
    float3 fPos = pos - normalize(eye) * fi *1;
    float3 lightPos = fPos - _WorldSpaceLightPos0.xyz ;

    float v = triNoise3D( fPos * .01 + fi * 113.3 + vertness*.1, 0 , _Time.y)  * fi/5;
    float v2 = triNoise3D( lightPos * .01 + fi * 113.3  + vertness*.1, 0 , _Time.y)  * fi/5;


float delta = saturate(-1*(v2-v));

  float4 tMap = tex2D(_Splat3, rotateUV(fPos.y * .004 +delta, fPos.xz * .01)+ fi * .1) * .2;

  fV  += tMap.a * .2;
  fV += tMap.r * .5;
      col += tMap.a * float3(.2,1,.1) * .1;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;
      col += tMap.r * float3(.2,1,.3) * .2;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;


    //fV += saturate(-1*(v2-v)) * 40;
    //if( v > .1){


      if( delta > .02* (1-lMap) * (1-fi/4)){
       // col = float3(.6,.8,.4) * ((1-fi/5) * .5 +.5);
        //break;
      }
  // col += saturate(-1*(v2-v))*10 * float3(1,1,.3);//float3(.4,1,.4) * (1-fi/5);//(1-(fi/5));
    //  break;
    //}


  }

  //col *= 5;
  //col = float3(.7,.8,.4);

  //col = nor;
 // col /= 5;


if( baseVert > .8 && baseVert < .9){
  
  col *= 2;
}
 if( baseVert > .9){
  col *= 3;
 }



//col = floor((fV * 4)/4) * float3(.3,1,.5) * 1;

//col +=float3(.6,1,.5) * .4;

col *= (floor(lMap * 4 ) /4  + .3);



  return col;
};






float3 _WrenPos;
//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {

  fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);// * .5 + .5;

   half4 splat_control;
  half weight;
  fixed4 mixedDiffuse;
  half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);

  fixed3 normal;

 float3 eye = _WorldSpaceCameraPos - v.worldPos;
  
  SplatmapMix(v, defaultSmoothness, splat_control, weight, mixedDiffuse, normal);

float4 control = tex2D(_Control,v.uv);
float3 col = tex2D(_Control,v.uv);
col = mixedDiffuse;


float3 fNor =  normalize(normal);;// * .5 ;// normalize(v.nor);
float3 color1;
float3 color2;
float3 color3;
float3 color4;

 if( control.r < .1 ){ color1= 0;  }else{ color1 = DoSandColor( v.worldPos, v.nor,fNor,eye,shadow); }
 if( control.g < .1 ){ color2 = 0; }else{ color2 = DoRockColor(v.worldPos,v.nor,fNor,eye,shadow); }
 if( control.b < .1 ){ color3 = 0; }else{ color3 = DoMeadowColor(v.worldPos,v.nor,fNor,eye,shadow); }
 if( control.a < .1 ){ color4 = 0; }else{ color4 = DoForestColor(v.worldPos,v.nor,fNor,eye,shadow); }


control = pow(control,2);
control = normalize( control);

col = 0;
col += color1 * control.r;
col += color2 * control.g;
col += color3 * control.b;
col += color4 * control.a;

//col = color3;

/*col = color1;

if( control.b > control.r){
  col = color3;
}*/



//col = fNor ;


  //col = v.uv.x * 100;//splat_control.xyz;//mixedDiffuse;
  //col *= shadow;

 // col = v.debug;
// col = m;

 //col = floor(m * 4 ) /4 ;


 //col *= mixedDiffuse;//floor( mixedDiffuse.xyz*10)/10;
 
    return float4(col,1);
}

      ENDCG

    }


  







    }

Fallback "Diffuse"


}









