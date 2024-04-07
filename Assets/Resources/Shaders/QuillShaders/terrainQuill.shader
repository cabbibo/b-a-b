Shader "Unlit/quillTerrain"{
  Properties {


    _MeadowColor("_MeadowColor", Color) = (1,1,1,1)
    _MossColor1("_MossColor1", Color) = (1,1,1,1)
    _MossColor2("_MossColor2", Color) = (1,1,1,1)

    _SandFresnelColor("_SandFresnelColor",Color) = (1,1,1,1)

    _SandBaseColor("_SandBaseColor",Color) = (1,1,1,1)
    _SandSpecularColor1("_SandSpecularColor1",Color) = (1,1,1,1)

    _SandSpecularColor2("_SandSpecularColor2",Color) = (1,1,1,1)
    _SandStriationColorLight("_SandStriationColorLight",Color) = (1,1,1,1)
    _SandStriationColorDark("_SandStriationColorDark",Color) = (1,1,1,1)
    _SandStriationSize("_SandStriationSize", Vector) = (1,1,1)
    _SandStriationColorSteps("_SandStriationColorSteps",float) = 4
    _SandStriationVerticalCutoff("_SandStriationVerticalCutoff",float) = .5


    _SandSaturation("_SandSaturation",float) = 1
    _SandBrightness("_SandBrightness",float) = 1
    _SandMatchMultiplier("_SandMatchMultiplier",float) = 1 
    _SandShadowBase("_SandShadowBase",float) = 1
    _SandSpecularPower1("_SandSpecularPower1",float) = 1
    _SandSpecularPower2("_SandSpecularPower2",float) = 1
    _SandSpecularPower2("_SandSpecularPower2",float) = 1


    
    _NoiseTextureStrength("_NoiseTextureStrength",float) = .2
    _NoiseTextureBase("_NoiseTextureBase",float) = 1


    _CityFresnelColor("_CityFresnelColor",Color) = (1,1,1,1)

    _CityBaseColor("_CityBaseColor",Color) = (1,1,1,1)
    _CitySpecularColor1("_CitySpecularColor1",Color) = (1,1,1,1)

    _CitySpecularColor2("_CitySpecularColor2",Color) = (1,1,1,1)
    _CityStriationColorLight("_CityStriationColorLight",Color) = (1,1,1,1)
    _CityStriationColorDark("_CityStriationColorDark",Color) = (1,1,1,1)
    _CityStriationSize("_CityStriationSize", Vector) = (1,1,1)
    _CityStriationColorSteps("_CityStriationColorSteps",float) = 4
    _CityStriationVerticalCutoff("_CityStriationVerticalCutoff",float) = .5


    _CitySaturation("_CitySaturation",float) = 1
    _CityBrightness("_CityBrightness",float) = 1
    _CityMatchMultiplier("_CityMatchMultiplier",float) = 1 
    _CityShadowBase("_CityShadowBase",float) = 1
    _CitySpecularPower1("_CitySpecularPower1",float) = 1
    _CitySpecularPower2("_CitySpecularPower2",float) = 1
    _CitySpecularPower2("_CitySpecularPower2",float) = 1





    _BiomeMap ("BiomeMap", 2D) = "white" {}
    _BiomeMapWeight ("BiomeMapWeight", float) = .01
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


    _FogColor ("Fog Color", Color) = (1,1,1,1)
    _FogAmount("_Fog Amount", Range(0,1)) = 0
    _FogStart("_Fog Start", float) = 100
    _FogEnd("_Fog End", float) = 1000
    _FogNoiseAmount("_Fog Noise Amount", float) = 0
    _FogNoiseScale("_Fog Noise Scale", float) = 1
    _HeightFogColor ("Height Fog Color", Color) = (1,1,1,1)
    _HeightFogAmount("_Height Fog Amount", Range(0,1)) = 0
    _HeightFogStart("_Height Fog Start", float) = 100
    _HeightFogEnd("_Height Fog End", float) = 1000
    _HeightFogMinDistance("_Height Fog Min Distance", float) = 0
    _HeightFogMaxDistance("_Height Fog Max Distance", float) = 1000
    _HeightFogNoiseAmount("_Height Fog Noise Amount", float) = 0
    _HeightFogNoiseScale("_Height Fog Noise Scale", float) = 1

    _WrenShadowColor ("Player Shadow Color", Color) = (1,1,1,1)
    _WrenShadowRadius("Player shadow Radius", float) = 1
    _WrenShadowThickness("Player shadow Thickness", float) = 0.2

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

      float sdCapsule( float3 p, float3 a, float3 b, float r )
      {
        float3 pa = p - a, ba = b - a;
        float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
        return length( pa - ba*h ) - r;
      }




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
      
      half4 _FogColor;
      float _FogStart;
      float _FogEnd;
      float _FogAmount;
      float _FogNoiseAmount;
      float _FogNoiseScale;

      half4 _HeightFogColor;
      float _HeightFogStart;
      float _HeightFogEnd;
      float _HeightFogAmount;
      float _HeightFogMinDistance;
      float _HeightFogMaxDistance;
      float _HeightFogNoiseAmount;
      float _HeightFogNoiseScale;

      half4 _WrenShadowColor;
      float _WrenShadowRadius;
      float _WrenShadowThickness;

      float _NoiseTextureStrength;
      float _NoiseTextureBase;
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





      float _SandMatchMultiplier; 
      float _SandShadowBase;
      float4 _SandFresnelColor;

      float _SandSaturation;
      float _SandBrightness;

      float4 _SandBaseColor;
      float4 _SandSpecularColor1;
      float _SandSpecularPower1;

      float4 _SandSpecularColor2;
      float _SandSpecularPower2;


      float4 _SandStriationColorLight;
      float4 _SandStriationColorDark;
      float3 _SandStriationSize;
      float _SandStriationColorSteps;
      float _SandStriationVerticalCutoff;


      float _CityMatchMultiplier; 
      float _CityShadowBase;
      float4 _CityFresnelColor;

      float _CitySaturation;
      float _CityBrightness;

      float4 _CityBaseColor;
      float4 _CitySpecularColor1;
      float _CitySpecularPower1;

      float4 _CitySpecularColor2;
      float _CitySpecularPower2;


      float4 _CityStriationColorLight;
      float4 _CityStriationColorDark;
      float3 _CityStriationSize;
      float _CityStriationColorSteps;
      float _CityStriationVerticalCutoff;


      /*



      sanddddd


      */

      float3 DoSandColor(float3 pos, float3 baseNor, float3 nor, float3 eye ,float shadow){



        float3 col = _SandBaseColor;
        float3 refl = normalize(reflect( -eye, nor  ));



        half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, refl);
        // decode cubemap data into actual color
        half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);



        float3 spec = dot(refl, _WorldSpaceLightPos0.xyz); 
        col +=  shadow* _SandSpecularColor1  * floor( pow(spec,10) * 3) * _SandSpecularPower1;


        float m = saturate( dot( normalize(eye), baseNor));





        //float3 cubeCol = texCube( _CubeMap,refl);



        float sparkleSize = 1;
        float sparkleForce = .3;
        refl = reflect( -eye, normalize( nor + float3(snoise(pos*sparkleSize),0,0)*sparkleForce + float3(0,0,snoise(pos * 1.2*sparkleSize + 13103))*sparkleForce )  );
        spec = saturate(dot(normalize(refl) , _WorldSpaceLightPos0.xyz)); 
        // col += col * floor( pow(spec,100) * 3) *3;


        col += shadow * _SandSpecularColor2  *  floor( pow(spec,100) * 3) * _SandSpecularPower2;




        // canyon
        if( dot( nor , float3(0,1,0)) < _SandStriationVerticalCutoff ){

          col = float3(1,.3,.1);


          float noiseVal = noise( pos * _SandStriationSize);//snoise( pos * _SandStriationSize );

          noiseVal = floor( ((noiseVal +1 )/2) * _SandStriationColorSteps ) / _SandStriationColorSteps;

          col = lerp( _SandStriationColorLight , _SandStriationColorDark , noiseVal);
          //col *= floor(noise( float3(pos.xz,pos.y* 30) * .1 /*+noise( float3(pos.xz * 1,pos.y* 100)) * 1*/)*4)/4;
          //col *= floor(noise( float3(pos.xz,pos.y* 30) * .1 )*4)/4;

        }

        float lMap = dot(nor ,_WorldSpaceLightPos0.xyz);

        lMap *= shadow;


        col *= ((floor(lMap * 4 ) /4 ) * _SandMatchMultiplier + _SandShadowBase);




        col += _SandFresnelColor*2*floor(pow(saturate((1-m)),10) * 4)/4;

        col = col * _SandSaturation + 1-_SandSaturation;
        col *= _SandBrightness;


        col = saturate(col) * .95;
        return col;


      };


      /*


      CityColor

      */

      float3 DoCityColor(float3 pos, float3 baseNor, float3 nor, float3 eye ,float shadow){



        float3 col = _CityBaseColor;
        float3 refl = normalize(reflect( -eye, nor  ));



        half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, refl);
        // decode cubemap data into actual color
        half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);



        float3 spec = dot(refl, _WorldSpaceLightPos0.xyz); 
        col +=  _CitySpecularColor1  * floor( pow(spec,10) * 3) * _CitySpecularPower1;


        float m = saturate( dot( normalize(eye), baseNor));





        //float3 cubeCol = texCube( _CubeMap,refl);



        float sparkleSize = 1;
        float sparkleForce = .3;
        refl = reflect( -eye, normalize( nor + float3(snoise(pos*sparkleSize),0,0)*sparkleForce + float3(0,0,snoise(pos * 1.2*sparkleSize + 13103))*sparkleForce )  );
        spec = saturate(dot(normalize(refl) , _WorldSpaceLightPos0.xyz)); 
        // col += col * floor( pow(spec,100) * 3) *3;


        col += _CitySpecularColor2  *  floor( pow(spec,100) * 3) * _CitySpecularPower2;




        // canyon
        if( dot( nor , float3(0,1,0)) < _CityStriationVerticalCutoff ){

          col = float3(1,.3,.1);


          float noiseVal = noise( pos * _CityStriationSize);//snoise( pos * _CityStriationSize );

          noiseVal = floor( ((noiseVal +1 )/2) * _CityStriationColorSteps ) / _CityStriationColorSteps;

          col = lerp( _CityStriationColorLight , _CityStriationColorDark , noiseVal);
          //col *= floor(noise( float3(pos.xz,pos.y* 30) * .1 /*+noise( float3(pos.xz * 1,pos.y* 100)) * 1*/)*4)/4;
          //col *= floor(noise( float3(pos.xz,pos.y* 30) * .1 )*4)/4;

        }

        float lMap = dot(nor ,_WorldSpaceLightPos0.xyz);

        lMap *= shadow;


        col *= ((floor(lMap * 4 ) /4 ) * _CityMatchMultiplier + _CityShadowBase);




        col += _CityFresnelColor*2*floor(pow(saturate((1-m)),10) * 4)/4;

        col = col * _CitySaturation + 1-_CitySaturation;
        col *= _CityBrightness;


        col = saturate(col) * .95;
        return col;


      };



      /*



      ROCK ROCK ROCK ROCK ROCK ROCK ROCK ROCK ROCK ROCK ROCK ROCK ROCK ROCK ROCK ROCK ROCK 


      */


      float4 _MeadowColor;

      float3 DoRockColor(float3 pos,float3 baseNor, float3 nor, float3 eye ,float shadow){

        


        float lMap = dot(nor ,_WorldSpaceLightPos0.xyz);
        float lMapB = dot(baseNor ,_WorldSpaceLightPos0.xyz);

        lMap *= shadow;

        //float vertness = 0;//dot( nor , float3(0,1,0));

        float vertness = dot( nor , float3(0,1,0));
        float baseVert = dot( baseNor , float3(0,1,0));
        float3 col = .3;//float3(.4,.5,.2);//float3(0,0,0);//float3(1,1,.3);

        float fV = 0;

        float3 gemCol = 0;
        for( int i =0; i < 3; i++){

          float fi = float(i+1);
          
          float3 fPos = pos - normalize(eye) * fi *5;
          float3 lightPos = fPos - _WorldSpaceLightPos0.xyz ;

          float v = triNoise3D( fPos * .01 + fi * 113.3 + vertness*.1, 0 , _Time.y)  * fi/5;
          float v2 = triNoise3D( lightPos * .01 + fi * 113.3  + vertness*.1, 0 , _Time.y)  * fi/5;


          //float4 tMap = tex2D(_Splat2, rotateUV(fPos.y * .004, fPos.xz * .01)+ fi * .1) * .2;
          float4 tMap =0;// tex2D(_Splat1, fPos.xz * .04 + fi * .1) * .2;
          tMap += tex2D(_Splat1,  rotateUV(pos.y * .001 + nor.y * .003 ,fPos.xz * .01 + fi * .1)) * .2;

          fV  += tMap.a * .2;
          fV += tMap.r * .5;
          //col += pow(tMap.g ,2)* float3(.5,.5,.5) * 500;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;
          col += pow(tMap.g ,2)* float3(fi/4,.5,.5) * 50;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;
          //col += pow(tMap.r,2) * float3(.6,.4,.4) * .2;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;

          gemCol += hsv(tMap.r * .4+ fi/10 + .5, 1,tMap.r * 3);

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
        

        //col = fV/2;

        //col *= 20;



        float rainExtra = floor( (triNoise3D( pos  * .0005 ,1,_Time.y * .0)+1) * 1000 ) /1000;
        rainExtra -=   .5 * floor( (triNoise3D( pos  * .003,1,_Time.y * .0)+1) * 1000 ) /1000;

        rainExtra /=.5;


        col *= (floor(triNoise3D( pos * .001,0,0) * 6) / 6) *1 + .4;

        // meadow;
        if( baseVert > .95 ){
          col *= (m4+.5)*_MeadowColor;//4*float3(.3,1,.1);
        }

        // Gemstones?
        if(rainExtra < .85 ){

          float fade = rainExtra-.85;
          col = gemCol *-fade * 10;//brightnessContrast(col, .3 , 11);
        }


        // snow
        if( pos.y + triNoise3D(pos * .004,0,0) * 200 - lMapB * 100 > 400 &&  ( baseVert  + pos.y * .0001> .5 )){
          col = 1;
        }
        //col *= 5;
        // col = float3(.7,.8,.4);

        // col /= 5;




        //col = floor((fV * 4)/4) * float3(.3,1,.5) * 1;

        //col +=float3(.6,1,.5) * .4;

        col *= (floor(lMap * 4 ) /4  + .3);

        col *= shadow;



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








      float4 _MossColor1;
      float4 _MossColor2;




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
          col += tMap.a * _MossColor1 ;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;
          col += tMap.r * _MossColor2 ;//tex2D(_Splat2, (fPos.xyz -10*nor).xz * .03+ fi * .1) * .2;


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


        if( baseVert < .3 ){
          col = DoRockColor(pos,baseNor, nor, eye ,shadow);
        }

        return col;
      };



      sampler2D _BiomeMap;

      float _BiomeMapWeight;


      float3 _WrenPos;
      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

        fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);

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

        if( control.r < .1 ){ color1 = 0;  }else{ color1 = DoSandColor( v.worldPos, v.nor,fNor,eye,shadow); }
        if( control.g < .1 ){ color2 = 0;  }else{ color2 = DoRockColor(v.worldPos,v.nor,fNor,eye,shadow); }
        if( control.b < .1 ){ color3 = 0;  }else{ color3 = DoMeadowColor(v.worldPos,v.nor,fNor,eye,shadow); }
        if( control.a < .1 ){ color4 = 0;  }else{ color4 = DoForestColor(v.worldPos,v.nor,fNor,eye,shadow); }



        //float max = 

        //control = pow(control,30);

        control = normalize( control);

        col = 0;
        col += color1 * control.r;
        col += color2 * control.g;
        col += color3 * control.b;
        col += color4 * control.a;


        float lVal = 1/(.2*pow(length(v.worldPos - _WrenPos) * .3,4)+1);
        col += (col * .8 )* lVal;

        //col = color3;

        /*col = color1;

        if( control.b > control.r){
          col = color3;
        }*/


        float max1 = 0;
        float maxID = 0;
        float max2 = 0;
        float maxID2 = 0;

        for( int i= 0; i < 4; i++ ){

          if( control[i] > max1){
            max1 = control[i];
            maxID = i;
          }

          if( control[i] < max1 && control[i]> max2 ){
            max2 = control[i];
            maxID2 = i;
          }
        }


        float d = max1 - max2;


        // DEBUG Borders;
        if( d < .4 ){
          //col = float3(1,0,0);
        }

        float4 bMap = tex2D(_BiomeMap, (v.worldPos.xz+4096) / 8192);
        //col *= lerp( 1, bMap.xyz * _BiomeMapWeight + (1-_BiomeMapWeight) , bMap.a);

        float cityDot = dot( normalize(bMap.xyz) , normalize(float3(1,0,.5)));

        float cityValue = saturate((pow( cityDot,30)-.3) * 10) * bMap.a;
        float3 cityColor = DoCityColor( v.worldPos, v.nor,fNor,eye,shadow);
        col = lerp(col ,cityColor,cityValue); 

        //col = bMap.xyz * bMap.a;

        //col =   * bMap.a * bMap.a;//lerp( col , pow( length(col )/3,3) * 10 * (bMap.xyz * _BiomeMapWeight + (1-_BiomeMapWeight)) ,bMap.a);
        //col = fNor ;


        //col = v.uv.x * 100;//splat_control.xyz;//mixedDiffuse;
        //col *= shadow;

        // col = v.debug;
        // col = m;

        //col = floor(m * 4 ) /4 ;


        //col *= mixedDiffuse;//floor( mixedDiffuse.xyz*10)/10;


        // player shadow
        float2 pos = _WrenPos.xz;
        float shdist = length(pos - v.worldPos.xz);
        float radius = abs(_WrenPos.y - v.worldPos.y) * _WrenShadowRadius - 0.5;
        float shouter = shdist - radius;
        float shinner = shdist - (radius-_WrenShadowThickness);
        float sh = 1 - saturate(max(-shinner, shouter));
        // sh *= sh * sh * sh * sh;
        float sha = lerp (0, _WrenShadowColor.a, saturate(1-((radius-5) / 10)));

        // fog
        float fogZ = distance(_WorldSpaceCameraPos, v.worldPos);
        float ff = saturate((fogZ-_FogStart)/(_FogEnd-_FogStart));
        // use noise
        ff += clamp(_FogNoiseAmount * (triNoise3D(v.worldPos * _FogNoiseScale * .01, 0, 0) - 0.5f) * ff, -1 ,1);
        ff *= ff;
        col.rgb = lerp( col.rgb, _FogColor.rgb, ff * _FogAmount);

        // height fog
        float heightFog = saturate((v.worldPos.y-_HeightFogStart)/(_HeightFogEnd-_HeightFogStart));
        // use min max distance
        heightFog *= saturate((_HeightFogMaxDistance - fogZ) / (_HeightFogMaxDistance - _HeightFogMinDistance));

        // add noise
        heightFog += _HeightFogNoiseAmount * (triNoise3D(v.worldPos * _HeightFogNoiseScale * .01 + float3(0,_Time.y*0.01,0), 0, 0) - 0.5f) * heightFog;
        heightFog *= heightFog;
        heightFog *= heightFog;
        heightFog *= heightFog;
        heightFog = clamp(heightFog, 0, 1);
        col.rgb = lerp( col.rgb, _HeightFogColor.rgb, heightFog * _HeightFogAmount);

        // col.rgb = lerp(col, _WrenShadowColor.rgb, sh * sha);


        // Discards around bird!

        float noiseVal2 = snoise(v.worldPos * 1.3);

        float capDistance =sdCapsule(v.worldPos , _WorldSpaceCameraPos , _WrenPos , 1 );
        capDistance -= noiseVal2 * .2;

        col *= noiseVal2 * _NoiseTextureStrength * (1/(1+ .1*fogZ)) + _NoiseTextureBase;



        //col = shadow;
        

        //col = shadow;
        if( capDistance < 0 &&  dot( v.nor , v.eye ) < 0 ){
          discard;
          }else{

          if(dot( v.nor , v.eye ) < 0  ){
            col *= saturate(capDistance * 10);
          }
        }
        
        return float4(col,1);
      }



      ENDCG

    }


    







  }

  Fallback "Diffuse"


}









