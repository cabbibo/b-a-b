
Shader "Terrain/SkyboxFull"
{

    Properties {

  
    _MainTex("_MainTex", 2D) = "white" {}
    _MapScale("MapScale", float) = 1
    _Fade("_Fade", float) = 1
    _CubeMap("_CubeMap" ,Cube) = "white" {}
    
    }


  SubShader{

            // Draw ourselves after all opaque geometry
            Tags { "RenderType"="Background" "Queue"="Background" }

       
      
    Pass{

      
    //  ZWrite Off
      Cull Off
      Fog { Mode Off }
CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      
    float4 _BaseColor;
    float4 _SampleColor;
    int _NumSteps;
    float _Opaqueness;
    float _ColorMultiplier;
    float _RefractionBackgroundSampleExtraStep;
    float _IndexOfRefraction;

    float _ReflectionSharpness;
    float _ReflectionMultiplier;
    float4 _ReflectionColor;

    sampler2D _SampleTexture;
    float _SampleSize;

    sampler2D _AudioMap;
    samplerCUBE _CubeMap;

    float3 _LightDir;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor : NORMAL;
          float3 ro : TEXCOORD1;
          float3 rd : TEXCOORD2;
          float3 eye : TEXCOORD3;
          float3 localPos : TEXCOORD4;
          float3 worldNor : TEXCOORD5;
          float3 lightDir : TEXCOORD6;
          float4 grabPos : TEXCOORD7;
          float3 unrefracted : TEXCOORD8;
          float3 world : TEXCOORD9;
          
          
      };


            sampler2D _BackgroundTexture;


             struct appdata
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
            };

//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert ( appdata vertex ){



  varyings o;
     float4 p = vertex.position;
     float3 n =  vertex.normal;//_NormBuffer[id/3];

        float3 worldPos = mul (unity_ObjectToWorld, float4(p.xyz,1.0f)).xyz;
        o.pos = UnityObjectToClipPos (float4(p.xyz,1.0f));
        o.nor = n;//normalize(mul (unity_ObjectToWorld, float4(n.xyz,0.0f)));; 
        o.ro = p;//worldPos.xyz;
        o.rd  = mul(unity_ObjectToWorld, vertex.position).xyz - _WorldSpaceCameraPos;
        o.localPos = p.xyz;
        o.world = worldPos;


      
  return o;

}



float3 hsv(float h, float s, float v)
{
  return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
    h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
}


float _MapScale;
float _Fade;
sampler2D _MainTex;
#include "../Chunks/noise.cginc"

float2 GetXYCoordsInPlane(float3 p1, float3 v1, float3 up)
{
   
    float distance = dot(p1, v1);
    float3 projectedPoint = p1 - distance * v1;

    float3 right = normalize(cross(v1, up));
    float x = dot(projectedPoint, right);
    float y = dot(projectedPoint, cross(right,v1));

    return float2(x, y);
}



float4x4 rotationMatrix(float3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return float4x4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}



float3 _SunPosition;

float3 _SunColor;

float _DayNess;
float _NightNess;
float3 _MoonColor;
float3 _MoonPosition;

float _TimeInNight;
float _TimeInDay;
//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
  float3 col =0;//hsv( float(v.face) * .3 , 1,1);

   float3 bf = normalize(abs(v.rd));
            bf /= dot(bf, (float3)1);

            float scale = 20;

    float3 rd = normalize(v.rd);

    float3 m = dot( _WorldSpaceLightPos0.xyz , rd );

    float2 tx = rd.zy * _MapScale;
    float2 ty = rd.xz * _MapScale;
    float2 tz = rd.xy * _MapScale;

    float n = noise( v.rd * .0002 ) +  .4 * noise (v.rd * .0006) + .1 * noise(v.rd * .001)  ;//* .3 + noise(v.rd * .0001) * .6 + noise(v.rd * .0003);


    half4 cx = tex2D(_MainTex, tx )* bf.x;
    half4 cy = tex2D(_MainTex, ty )* bf.y;
    half4 cz = tex2D(_MainTex, tz )* bf.z;
    col = (cx + cy + cz).xyz;
    //col *= 10;

    float sunCol = pow( saturate(m),1000) * 20;
    //col = hsv(col.x * .2 + sunCol + dot( _WorldSpaceLightPos0.xyz , float3(0,-1,0)), 0, saturate(col.x * 5- 5*saturate(sunCol)));
    //col += hsv(dot( _WorldSpaceLightPos0.xyz , float3(0,-1,0)),1,saturate(sunCol) ) * 11.8;

    ///col *= tex2D(_AudioMap,n * .1);

    col *= _Fade;
    col = saturate(col);

    float3 starCol = col;

    float a = atan2( rd.x , rd.z);

    float v1 = rd.y + (.1*sin( a * 10 + _Time.x + sin( a * 3 + _Time.y) * .1 )+ .1*sin( a * 20 +12.3 + _Time.x * .3 + sin( a * 44 + _Time.y) * .1 ) ) * .1;;//- .01*sin( _Time.y + sin(rd.x * 20 + _Time.y))- .01*sin( _Time.y  +1321+ sin(rd.y * 15 + sin( _Time.y) * 10 + _Time.y));

    col /= (.1 + 10 *abs(v1));

   // rd.y = v1;


   // col = (col *.9 + 0) *texCUBE( _CubeMap , rd );
   // col += texCUBE( _CubeMap , rd ).b *.1* v1*v1*v1 * float3(1,.8,.6);
   // rd.y = abs(rd.y);
    col = pow( texCUBE( _CubeMap , float3(rd.x , v1 , rd.y + rd.z * .3 ) ).r ,1) * 3;//pow(texCUBE( _CubeMap , rd ).b ,10) * 100;
       // col /= (.03 + 1 *abs(v1));
        
        col = saturate( col) * 20;
       // col *= col * col * col * col; 
        
       // col.xyz += starCol;

    col *= pow( texCUBE( _CubeMap , rd ) ,1) * 10;//pow(texCUBE( _CubeMap , rd ).b ,10) * 100;

  col *= _Fade;


  col = 0;
  float3 tmpRD = rd;
  for( int i = 0; i < 3; i++ ){
    float3 fPos = _WorldSpaceCameraPos * .1  + v.ro * 100+ tmpRD * i * 10.1f;
     
    fPos = mul( rotationMatrix( float3(0,1,0) , 1* noise( _Time.y * .04 + fPos * .01* float3(1,1,1))) , float4(fPos,1)).xyz;


   float v = pow(noise( (fPos + float3(0,_Time.y * 10,0)) * .3 * float3(.8,.2 ,1)),2);//sin( fPos.x *100 + _Time.x) * .1 + sin( fPos.y *100 + _Time.y) * .1 + sin( fPos.z *100 + _Time.z) * .1;
    v += noise(fPos * .03+ float3(0,_Time.y * .1,0)) * 2;
  //v= floor(v* 2) / 2;
    v/=5;

    tmpRD.y += v * .1;
    float3 tMap = float3(.5 , 1 - 2*abs(tmpRD.y), 1-abs(tmpRD.y));// pow( texCUBE( _CubeMap , fPos ) ,1) * 2;

    if( i == 0 ){
      col += float3(v,.2,.2) * (v+.1) * tMap;
    }else if( i == 1 ){
      col += float3(.2,v,.2)*(v+.1) * tMap;;
    }else if( i == 2 ){
      col += float3(.2,.2,v)* (v+.1) * tMap;;
    }

    
    
  }

//  col = floor(col * 5) / 5;

  col *= pow(saturate(1-abs(rd.y + noise( v.ro- float3(0,_Time.y * .12,0) )- noise(v.ro * 2 + float3(0,_Time.y * .1,0)))),1);
  col *= length(col) * length(col) * 10;



  float3 d = normalize(_WorldSpaceCameraPos - v.world);

  float3 test = 0;



  for( int i = 0; i < 3; i++ ){

    float3 p = v.world -d * i * 1000.1f;

    float3 lightDir = normalize(_SunPosition - p);
    
    float3 polarCoords = float3( atan2(lightDir.z, lightDir.x), acos(lightDir.y), length(lightDir) );

    float3 n3 = float3(

      noise( p  * .001 )-.5,
      noise( p  * .002  + float3(0,111.11,0))-.5,
      noise( p  * .001  + float3(0,0,1111))-.5
    
    );

    n3 = normalize(n3+.5);

  

    //n3 = normalize(n3-.5);

    float3 pVal = noise( float3(GetXYCoordsInPlane(p, _SunPosition, _LightDir), 0) * .00000001);

    float n = .1*noise( p  * .003+ lightDir * 1 );
    n += noise( p  * .001 + lightDir * 30) * .3;
    n += noise( p  * .0005 + lightDir * 50) * .5;  

    float nDelta = abs( n - noise( p  * .01 + lightDir * 10));

   // test += nDelta* .1 + n;

    //test = (dot(normalize(n3),-lightDir)+1)/2;// dot(normalize(n3), lightDir) * .1;


    float matchVal = (pow(dot(d, -_WorldSpaceLightPos0.xyz),3) + 1)/2;



//      float matchGood = 

//if( dot( -d, -lightDir) > .1 ){

  if( n > .6 ){
      test += 3*lerp(  float3(.3,.3,1) * length(_SunColor.xyz) ,_SunColor , matchVal );// *  saturate(dot(_WorldSpaceLightPos0,n3)) ;;
  }else{
    test += lerp(  float3(.3,.3,1) * length(_SunColor.xyz) ,_SunColor , matchVal ) + _SunColor * .4;
  }




   // test = pVal;




  }

  test *= .1;


  float verticalNess = abs( dot( float3(0,1,0), normalize(rd)) );
  test += float3(1,1,1) * .3 *pow( (1-normalize(v.world).y),4);

  test *= _DayNess;



  float3 newDir = mul( rotationMatrix( float3(1,0,0), -_TimeInNight * 3.14),float4( rd,0)).xyz;


  test +=pow( texCUBE( _CubeMap , newDir ) ,1) * 3 * _NightNess  * _MoonColor;
  test += float3(1,1,1) * .1 *pow( (1-normalize(v.world).y),10);






/// SUN
  float3 sunColor = 0;

  float3 fLightDir = -normalize(_WorldSpaceLightPos0.xyz);
  for( int i = 0; i < 3; i++ ){
    float3 fPos = _WorldSpaceCameraPos * .1  + v.ro * 100+ rd * i * 30.1f;
  sunColor += .5*float3(1,float(i) * .2 + .4,.2)* pow( noise(fPos * .1),2)*10*pow(  saturate(dot( fLightDir, -normalize(rd))),101);
  sunColor += .2*float3(1,.6-float(i) * .2,.2)* pow( noise(fPos * .4),2)*10*pow(  saturate(dot( fLightDir, -normalize(rd))),101);

    
float2  xy = GetXYCoordsInPlane(_WorldSpaceCameraPos * 1 + v.world + rd * i * 10.1f, fLightDir ,float3(0,1,0));

float ang= atan2(xy.y, xy.x);

sunColor += .2 * float3(1,float(i) * .2 + .4,.2)*noise( ang * 10 + float3(0,_Time.y* (i-1.5) * .4,0)) *  pow( saturate(dot( -fLightDir,rd)) ,10)* 1;//length(xy) * .01;//length(xy) * .1;//1 / length( xy );// * .0001;


  }


 col *= col;
    col *= _Fade;

  col += pow(texCUBE(_CubeMap, rd).xyz ,1).x * pow(abs(rd.y),2)*2;//* 1;

//col *= 10;
    col = saturate(col);  

    col = sunColor;

    col = test;


    

//col += pow( dot( -_LightDir,rd) ,100)* 10;


    //col += pow( dot( _LightDir, -normalize(rd)),30);
   // col *= .5;
   // col += .5;
  // = sin(atan2( rd.x , rd.z) * 10) * .1;

  col = saturate(col);
    return float4( col.xyz, 1);//saturate(float4(col,3*length(col) ));




}

      ENDCG

    }
  }

  Fallback Off


}