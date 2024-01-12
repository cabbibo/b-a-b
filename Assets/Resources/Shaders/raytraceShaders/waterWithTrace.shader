

Shader "Volumetric/traceWithWater"
{

    Properties {

    _BaseColor ("BaseColor", Color) = (1,1,1,1)
    _SurfaceColor ("Surfac_SurfaceColor", Color) = (1,1,1,1)
    
    _NumSteps("Num Trace Steps",int) = 10
    _DeltaStepSize("DeltaStepSize",float) = .01
    _StepRefractionMultiplier("StepRefractionMultiplier", float) = 0
    
    _ColorMultiplier("ColorMultiplier",float)=1
  
    _Opaqueness("_Opaqueness",float) = 1
    _IndexOfRefraction("_IndexOfRefraction",float) = .8
    _RefractionBackgroundSampleExtraStep("_RefractionBackgroundSampleExtraStep",float) = 0

    _ReflectionColor ("ReflectionColor", Color) = (1,1,1,1)
    _ReflectionSharpness("ReflectionSharpness",float)=1
    _ReflectionMultiplier("_ReflectionMultiplier",float)=1
    
    _CenterOrbOffset ("CenterOrbOffset", Vector) = (0,0,0)
    _CenterOrbColor ("CenterOrbColor", Color) = (1,1,1,1)
    _CenterOrbFalloff("CenterOrbFalloff", float) = 6
    _CenterOrbFalloffSharpness("CenterOrbFalloffSharpness", float) = 1

    _CenterOrbImportance("CenterOrbImportance", float) = .3

    _NoiseColor ("NoiseColor", Color) = (1,1,1,1)
    _NoiseOffset ("NoiseOffset", Vector) = (0,0,0)
    _NoiseSize("NoiseSize", float) = 1
    _NoiseSpeed("NoiseSpeed", float) = 1
    _NoiseImportance("NoiseImportance", float) = 1
    _NoiseSharpness("NoiseSharpness",float) = 1
    _NoiseSubtractor("NoiseSubtractor",float)=0
    _OffsetMultiplier("OffsetMu_OffsetMultiplier",float)=0



  
    _StartHue("_StartHue", float) = 1
    _HueSize("_HueSize",float) = 1
    _Saturation("_Saturation",float) = 1
    _ColorFalloff("_ColorFalloff",float)=1
    
    _FoamMap("Foam" , 2D) = "white" {}
    _FoamSpeed ("_FoamSpeed", Vector) = (0,0,0)
    _FoamSize ("_FoamSize", Vector) = (0,0,0)

    }


  SubShader{

            // Draw ourselves after all opaque geometry
        Tags { "Queue" = "Geometry+10" }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }

      Cull Back
    Pass{
CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      
			sampler2D _CameraDepthTexture;
			sampler2D _FoamMap;
    float4 _BaseColor;
    float4 _SurfaceColor;
    float4 _CenterOrbColor;
    float4 _NoiseColor;
    int _NumSteps;
    float _DeltaStepSize;
    float _NoiseSize;
    float _CenterOrbFalloff;
    float _NoiseImportance;
    float _CenterOrbImportance;
    float _CenterOrbFalloffSharpness;
    float _StepRefractionMultiplier;
    float _NoiseSharpness;
    float _NoiseSpeed;
    float _Opaqueness;
    float _NoiseSubtractor;
    float _ColorMultiplier;
    float _RefractionBackgroundSampleExtraStep;
    float _IndexOfRefraction;
    float3 _CenterOrbOffset;
    float3 _NoiseOffset;

    float _ReflectionSharpness;
    float _ReflectionMultiplier;
    float4 _ReflectionColor;

    float _OffsetMultiplier;

  float4 _FoamSize;
  float4 _FoamSpeed;

  float _StartHue;
  float _HueSize;
  float _Saturation;
  float _ColorFalloff;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor : NORMAL;
          float3 ro : TEXCOORD1;
          float3 rd : TEXCOORD2;
          float3 eye : TEXCOORD3;
          float3 localPos : TEXCOORD4;
          float3 localNor : TEXCOORD12;
          float3 worldPos : TEXCOORD10;
          float3 worldNor : TEXCOORD5;
          float3 lightDir : TEXCOORD6;
          float4 grabPos : TEXCOORD7;
          float4 screenPos : TEXCOORD9;
          float3 unrefracted : TEXCOORD8;
          float2 uv : TEXCOORD11;
          
          
      };



             struct appdata
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                
            float4 texcoord : TEXCOORD0;
            };




//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert ( appdata vertex ){



  varyings o;
     float4 p = vertex.position;
     float3 n =  vertex.normal;//_NormBuffer[id/3];

        float3 worldPos = mul (unity_ObjectToWorld, float4(p.xyz,1.0f)).xyz;
        o.pos = UnityObjectToClipPos (float4(p.xyz,1.0f));
        o.nor = normalize(mul (unity_ObjectToWorld, float4(n.xyz,0.0f)).xyz);; 
        o.ro = worldPos.xyz;
        o.localPos = p.xyz;
        o.uv = vertex.texcoord;
        o.localNor = n.xyz;
        
        
        float3 localP = normalize(_WorldSpaceCameraPos - worldPos);//mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1)).xyz;
        float3 eye = normalize(_WorldSpaceCameraPos - worldPos);//normalize(localP - p.xyz);

        o.worldPos = mul( unity_ObjectToWorld , vertex.position ).xyz;
        o.unrefracted = eye;
        o.rd = eye;//refract( eye , -n , _IndexOfRefraction) ;

        o.eye =normalize(_WorldSpaceCameraPos - worldPos);// refract( -normalize(_WorldSpaceCameraPos - worldPos) , normalize(mul (unity_ObjectToWorld, float4(n.xyz,0.0f))) , _IndexOfRefraction);
    
        o.worldNor = normalize(mul (unity_ObjectToWorld, float4(-n,0.0f)).xyz);
        //o.lightDir = normalize(mul( unity_ObjectToWorld , float4(1,-1,0,0)).xyz);

        float4 refractedPos = UnityObjectToClipPos( float4(o.ro + o.rd * 1.5,1));
    o.grabPos = ComputeGrabScreenPos(refractedPos);
    
				o.screenPos = ComputeScreenPos(o.pos);
    

  return o;

}




float tri(in float x){return abs(frac(x)-.5);}
float3 tri3(in float3 p){return float3( tri(p.y+tri(p.z)), tri(p.z+tri(p.x)), tri(p.y+tri(p.x)));}
           
float triAdd( in float3 p ){ return (tri(p.x+tri(p.y+tri(p.z)))); }

float triangularNoise( float3 p ){

    float totalFog = 0;

    float noiseScale = 1;

    float3 tmpPos = p;

    float noiseContribution = 1;

    float3 offset = 0;

    p *= _NoiseSize;
    p *= 2;

   float speed = 1.1;
 
   p +=  tri3(p.xyz * .3 + _NoiseSpeed * _Time.x* .1 ) *1.6;
   totalFog += triAdd(p.yxz * .3) * .35;
    
   p +=  tri3(p.xyz * .4 + 121  + _NoiseSpeed * _Time.x* .2) * 1;
   totalFog += triAdd(p.yxz * 1) * .25;
    
   p +=  tri3(p.xyz * .8 + 31  + _NoiseSpeed * _Time.x * .3) * 1;
   totalFog += triAdd(p.yxz* 1.3) * .15;

  return totalFog;

}


float t3D( float3 pos ){
  float3 fPos = pos * .05 + _NoiseOffset;

  // Adds Randomness to noise for each crystal
 // fPos += 100 * mul(unity_ObjectToWorld,float4(0,0,0,1)).xyz;
  return triangularNoise( fPos);
}

float dT3D( float3 pos , float3 lightDir ){

  float eps = .0001;

  
  return ((t3D(pos) - t3D(pos+ lightDir * eps))/eps+.5);
}


float3 nT3D( float3 pos ){

  float3 eps = float3(.0001,0,0);

  return t3D(pos) * normalize(
         float3(  t3D(pos + eps.xyy) - t3D(pos - eps.xyy), 
                  t3D(pos + eps.yxy) - t3D(pos - eps.yxy),
                  t3D(pos + eps.yyx) - t3D(pos - eps.yyx) ));


}


     sampler2D _HeightMap;

     float3 _MapSize;
            float terrainHeight( float3 pos ){
                float height = tex2Dlod( _HeightMap , float4(((pos.xz)) / _MapSize.xz + .5, 0 ,0)).x;// , 1).x * 4000
                return height * _MapSize.y;//float3( pos.x ,height * 4000 , pos.z);
                //return float3( pos.x ,pos.y, pos.z);
            }

sampler2D _BackgroundTexture;
#include "../Chunks/hsv.cginc"
#include "../Chunks/noise.cginc"
//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
  float3 col =1;//hsv( float(v.face) * .3 , 1,1);


  
  float dt = _DeltaStepSize;
  float t = 0;
  float c = 0.;
float3 p =  v.ro;
bool broken = false;
float stepBroken = 0;
float totalSmoke = 0;
  float3 rd = v.rd;// + (noise(p *1000) * UNITY_MATRIX_V[0]+ noise(p * 1000000) * UNITY_MATRIX_V[1]) * .2;

    rd = normalize(_WorldSpaceCameraPos - v.ro);//normalize(rd);

float3 nor = nT3D( p * _NoiseSize  );
 float vertness = dot( rd , float3(0,1,0));
  float depthVal= 0;
  for(int i =0 ; i < _NumSteps; i++ ){
     // t+=dt*exp(-2.*c);
    depthVal = v.ro.y - p.y;///rd.y * t * 2;


    float height = terrainHeight( p );
    float delta = p.y - height;




    if( !broken ){
         float3 smoke = nT3D( p * _NoiseSize  );
         float3 nor = normalize(smoke);

         float noiseDensity = saturate(length(smoke) - _NoiseSubtractor);


        noiseDensity =   pow( noiseDensity , _NoiseSharpness)  * _NoiseImportance;


       
        c= saturate(noiseDensity);//saturate(centerOrbDensity +noiseDensity);   
         totalSmoke += c;

       // col -= .1;

        //rd = normalize(rd * (1-c*_StepRefractionMultiplier) + nor *  c*_StepRefractionMultiplier);
        //col -= _NoiseColor * noiseDensity  + _NoiseColor;// + float3(1,1,0) * depthVal * .01;//+lerp( lerp(_BaseColor,_CenterOrbColor , saturate(centerOrbDensity)), _NoiseColor , saturate(noiseDensity));// saturate(dot(v.lightDir , nor)) * .1 *c;//hsv(c,.4, dT3D(p*3,float3(0,-1,0))) * c;//hsv(c * .8 + .3,1,1)*c;;// hsv(smoke,1,1) * saturate(smoke);
    
        if( p.y < height +6+ noiseDensity ){
            broken = true;
            stepBroken = float(i);;
            break;
        }
    }

    
    p -= rd * (dt/vertness);

 
 
  }


if( !broken ){stepBroken = _NumSteps;}

float3 localEye = mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1)).xyz - v.localPos;

float3 refr = refract( normalize(localEye) , normalize(v.localNor + nor * 3), .8);
       // float4 refractedPos = UnityObjectToClipPos( float4(o.ro + o.rd * 1.5,1));
  float4 refractedPos = ComputeGrabScreenPos(UnityObjectToClipPos(float4(v.localPos + refr * .0003,1)));
float4 backgroundCol = tex2Dproj(_BackgroundTexture, refractedPos);
//float4 backgroundCol = tex2Dproj(_BackgroundTexture, v.grabPos);

 //col /= float(stepBroken+1);
 //col *= _ColorMultiplier;



  float nBroken = float(stepBroken) / float(_NumSteps);
 col = hsv( _StartHue + float(stepBroken) * _HueSize , _Saturation,(1- nBroken* _ColorFalloff) );;
col = float(stepBroken) * .01;

col =lerp(  backgroundCol * _SurfaceColor , _BaseColor , float(stepBroken)/_NumSteps);

float upMatch = 1-dot(normalize(v.nor) ,float3(0,1,0));

float4 foam = tex2D(_FoamMap , v.uv * _FoamSize  + _FoamSpeed * _Time.x);

//col += upMatch * foam * hsv( _StartHue, _Saturation,1) ;


//col = 1-upMatch;


 //col = 1- col;//

if( stepBroken == 0 ){
  col = 1;
}

  float3 baseCol =_BaseColor.xyz;

     //  col = lerp(col*backgroundCol,col,saturate(totalSmoke * _Opaqueness));

       
 //float m = dot( normalize(v.unrefracted), normalize(v.nor) );
 //col += pow((1-m),_ReflectionSharpness) * _ReflectionMultiplier * _ReflectionColor;


    float3 reflection=-v.eye;//normalize(reflect( normalize(-v.eye) , -v.worldNor));
    half4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflection,0); //UNITY_SAMPLE_TEXCUBE_LOD('cubemap', 'sample coordinate', 'map-map level')
    half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR); // This is done because the cubemap is stored HDR
        //col = skyColor;

    //col = v.nor * .5 + .5;
    // col *= pow((1-m),5) * 60;
    // col += (v.nor * .5 + .5 ) * .4;


    // apply depth texture
				float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, v.screenPos);
				float depth = LinearEyeDepth(depthSample).r;
float foamLine = 1 - saturate(.1 * (depth - v.screenPos.w));
                //col *= foamLine;


    float height = terrainHeight( v.ro );
    float delta = v.worldPos.y - height;

//col = rd.xyz;
    //col = height * 1000;

    //col = lerp( col , float3(0,.8,.3), abs(delta) * .05);//  * float3(0,1,0);//  float3(0,1,0) * delta;
//col = v.nor * .5 + .5;
    return float4( col.xyz , 1);//saturate(float4(col,3*length(col) ));




}

      ENDCG

    }
  }

  Fallback Off


}
