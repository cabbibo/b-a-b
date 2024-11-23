Shader "IMMAT/Basic/Shadows/CloudSpriteShader"
{
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Size ("Size", float) = .01
        _HeightMap ("HeightMap", 2D) = "white" {}
        _SpriteSize ("SpriteSize", int) = 6
    }
    SubShader
    {

        CGINCLUDE


        #include "UnityCG.cginc"
        #include "AutoLight.cginc"
      #include "../../Chunks/ShadowCasterPos.cginc"

      
      #include "Assets/Resources/Shaders/Chunks/SunShadows.cginc"


      #pragma target 4.5
        
      float2 rotateUV(float2 uv, float rotation)
{
    float mid = 0.5;
    return float2(
        cos(rotation) * (uv.x - mid) + sin(rotation) * (uv.y - mid) + mid,
        cos(rotation) * (uv.y - mid) - sin(rotation) * (uv.x - mid) + mid
    );
}

float2 rotateUV(float2 uv, float rotation, float2 mid)
{
    return float2(
      cos(rotation) * (uv.x - mid.x) + sin(rotation) * (uv.y - mid.y) + mid.x,
      cos(rotation) * (uv.y - mid.y) - sin(rotation) * (uv.x - mid.x) + mid.y
    );
}

float2 rotateUV(float2 uv, float rotation, float mid)
{
    return float2(
      cos(rotation) * (uv.x - mid) + sin(rotation) * (uv.y - mid) + mid,
      cos(rotation) * (uv.y - mid) - sin(rotation) * (uv.x - mid) + mid
    );
}

float3 _MainCameraPos;
float3 _MainCameraForward;
float3 _MainCameraUp;
float3 _MainCameraRight;




      
      // make fog work

      uniform int _Count;
      uniform float _Size;
      uniform float3 _Color;
        uniform int _SpriteSize;

      struct Vert{
        float3 pos;
        float3 vel;
        float3 nor;
        float3 tan;
        float2 uv;
        float2 debug;
      };
  
    StructuredBuffer<Vert> _VertBuffer;

    sampler2D _HeightMap;



      float3 GetWorldPosition(int base, int alternate,Vert v, out float2 uv){
        
        float3 extra = float3(0,0,0);
  
        float3 l = UNITY_MATRIX_V[0].xyz;
        float3 u = UNITY_MATRIX_V[1].xyz;

        //l = -_MainCameraRight;
        //u = -_MainCameraUp;

       // l = float3(1,0,0);
       // u = float3(0,1,0);

        
        
        //float3 l = float3(1,0,0);
       // float3 u = float3(0,1,0);
        
        uv = float2(0,0);
    
        if( alternate == 0 ){ extra = -l - u; uv = float2(0,0); }
        if( alternate == 1 ){ extra =  l - u; uv = float2(1,0); }
        if( alternate == 2 ){ extra =  l + u; uv = float2(1,1); }
        if( alternate == 3 ){ extra = -l - u; uv = float2(0,0); }
        if( alternate == 4 ){ extra =  l + u; uv = float2(1,1); }
        if( alternate == 5 ){ extra = -l + u; uv = float2(0,1); }
  
         
          return (v.pos) + extra * _Size;
        }
      

        float3 newNormal( float2 vUV , float2 mapUV ){
            
            float2 uv = vUV * 2.0 - 1.0;

            // Reconstruct sphere normal
            float z = sqrt(1.0 - uv.x * uv.x - uv.y * uv.y); // Sphere equation: x^2 + y^2 + z^2 = 1
            float3 sphereNormal = float3(uv.x, uv.y, z);

            float height = tex2D(_HeightMap, mapUV).a;
        //  sphereNormal.z *= height *2;
        
            // Handle case where z is undefined (UV out of sphere range)
            if (uv.x * uv.x + uv.y * uv.y > 1.0) {
                sphereNormal = float3(0.0, 0.0, 1.0); // Default to a forward normal
            }
            sphereNormal = normalize(sphereNormal); 
            return sphereNormal;

        }



        float3 worldNormal( float2 uv, float2 uv2 , float3 nor, float3 tan, float3 bitan){
            float3 normal = newNormal(uv, uv2);
            return normalize( nor * normal.x + tan * normal.y + bitan * normal.z);
        }

        float2 remapUVtoSprite(float2 uv, float nID){

            int spriteSize = int(_SpriteSize);
            int whichSprite = floor( nID * _SpriteSize * _SpriteSize);
            //whichSprite = 11;
            float2 spriteUV = float2( whichSprite % float(_SpriteSize), floor( whichSprite / float(_SpriteSize)) ) / float(_SpriteSize);
            
            float2 fUV = spriteUV + uv / float(_SpriteSize);
                return fUV;
            }


            


        



ENDCG
        
        Pass
        {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off

          Tags{ "LightMode" = "ForwardBase" }
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

    
            #pragma multi_compile_fogV
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
        float4 pos      : SV_POSITION;
        float3 nor      : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
        float3 eye      : TEXCOORD2;
        float2 debug    : TEXCOORD3;
        float2 uv       : TEXCOORD4;
        float2 uv2       : TEXCOORD7;
       // float id        : TEXCOORD5;
        LIGHTING_COORDS(5,6) 
    };




//float _Multiplier;
//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert (uint id : SV_VertexID){

varyings o;

int base = id / 6;
int alternate = id %6;

if( base < _Count ){


      Vert v = _VertBuffer[base];
      float2 uv = 0;
      float3 worldPos = GetWorldPosition(base,alternate,v,  uv);

    //Vert v = _VertBuffer[base % _Count];
    o.worldPos = worldPos;
    o.eye = _WorldSpaceCameraPos - o.worldPos;
    o.nor =v.nor;
    o.uv =uv;
    o.uv2 = remapUVtoSprite(uv,float(base)/float(_Count));
    //o.id = base;
    o.debug = v.debug;
    o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
    
    UNITY_TRANSFER_LIGHTING(o,o.worldPos);


}

return o;

}

struct ForwardFragmentOutput
{
    float4 Color : SV_Target;
    float  Depth : SV_Depth;
};

float LinearDepthToRawDepth(float linearDepth)
{
    return (1.0f - (linearDepth * _ZBufferParams.y)) / (linearDepth * _ZBufferParams.x);
}

ForwardFragmentOutput frag (varyings v) 
{
  // sample the texture

  // raytrace to get real position

  float3 wNor = worldNormal(v.uv, v.uv2,
    UNITY_MATRIX_V[0].xyz,

    UNITY_MATRIX_V[1].xyz,
    UNITY_MATRIX_V[2].xyz
    );

    //if( tex2D(_HeightMap, v.uv2).a < .1 ){ discard; }
    if( length( v.uv-.5 ) > .5 ){ discard;}

  fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);
  //fixed shadow = LIGHT_ATTENUATION(v) ;
  float3 col = _Color.xyz;

  col *= shadow;

  col = wNor;
  col *= shadow;


  float3 worldPos = v.worldPos;
  float3 rd = normalize( worldPos - _WorldSpaceCameraPos);
  worldPos += rd *  sqrt(1.0 - v.uv.x * v.uv.x - v.uv.y * v.uv.y) * 51; // Sphere equation: x^2 + y^2 + z^2 = 1


  fixed4 cascadeWeights = GET_CASCADE_WEIGHTS(v.worldPos.xyz, 0);

  float sVal = unity_sampleShadowmap(GET_SHADOW_COORDINATES(float4(worldPos.xyz, 1), cascadeWeights));
  float fogValue = sVal;//clamp( 1/(pow( d, _FogHeightPower) * _FogHeightMultiplier),0,1000) * lerp(_FogDensityAtNear, _FogDensityAtFar, ni);

  //col = fogValue;

float3 cameraPosition = _WorldSpaceCameraPos;           // Unity provided position of the camera/eye.
float3 positionWS = worldPos;        // Position of the sample you want a depth value for.

float distanceToCamera = length(positionWS - cameraPosition);
float linearDepth = (distanceToCamera - _ProjectionParams.y) / (_ProjectionParams.z - _ProjectionParams.y);

  ForwardFragmentOutput output = (ForwardFragmentOutput)0;

  output.Color = float4(worldPos,1);//rd;
  output.Depth = LinearDepthToRawDepth(linearDepth);//input.position.w;


  return output;
}

            

            ENDCG
        }

    // SHADOW PASS

    /*Pass
    {
      Tags{ "LightMode" = "ShadowCaster" }


      Fog{ Mode Off }
      ZWrite On
      ZTest LEqual
      Cull Off
      Offset 1, 1
      CGPROGRAM

      #pragma vertex vert2
      #pragma fragment frag2
      #pragma multi_compile_shadowcaster
      #pragma fragmentoption ARB_precision_hint_fastest




      
      struct v2f {
        V2F_SHADOW_CASTER;
        float3 nor : NORMAL;
        float2 uv : TEXCOORD0;
        float2 uv2 : TEXCOORD1;
        float3 worldPos : TEXCOORD2;
      };
    
    v2f vert2 (appdata_base input,uint id : SV_VertexID){
    
        v2f o;
      
        int base = id / 6;
        int alternate = id %6;
 
      
            Vert v = _VertBuffer[base];
            float2 uv = 0;
            float3 worldPos = GetWorldPosition(base,alternate,v,  uv);
    

            float4 position = ShadowCasterPos(worldPos, v.nor);
            o.uv = uv;
            o.uv2 = remapUVtoSprite(uv,float(base)/float(_Count));
            o.pos = UnityApplyLinearShadowBias(position);
            o.nor = v.nor;
            o.worldPos = worldPos;
            
      
      
      
        return o;
      
      }
    
    
      float calculateShadowDepth(float3 worldPos){
        float4 projPos = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
        projPos = UnityApplyLinearShadowBias(projPos);
        return projPos.z/projPos.w;
    }


      float4 frag2(v2f v) : COLOR
      {

       // if( tex2D(_HeightMap, v.uv2).a < .1 ){ discard; }
        if( length( v.uv-.5 ) > .5 ){ discard;}

       v2f fakeVaryings;
         fakeVaryings.uv = v.uv;
            fakeVaryings.uv2 = v.uv2;
            fakeVaryings.nor = v.nor;

            float3 wNor = worldNormal(v.uv, v.uv2,
                UNITY_MATRIX_V[0].xyz,
            
                UNITY_MATRIX_V[1].xyz,
                UNITY_MATRIX_V[2].xyz
                );
            
               // if( tex2D(_HeightMap, v.uv2).a < .1 ){ discard; }


            float3 worldPos = v.worldPos;
            float3 rd = normalize( worldPos - _WorldSpaceCameraPos);
            worldPos += rd * sqrt(1.0 - v.uv.x * v.uv.x - v.uv.y * v.uv.y) * 11; // Sphere equation: x^2 + y^2 + z^2 = 1
          

            
            float4 position = ShadowCasterPos(worldPos, -wNor);
            fakeVaryings.pos = 0;// UnityApplyLinearShadowBias(position);

            // add to make it spherical

                v.pos = fakeVaryings.pos;

        //SHADOW_CASTER_FRAGMENT(v);
        SHADOW_CASTER_FRAGMENT(fakeVaryings);
      }

      ENDCG
    }*/
  


    }
}
