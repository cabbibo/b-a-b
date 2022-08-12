// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Raytrace/Monolith"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _OrbHeight ("OrbHeight", float) =0 
        _OrbSize ("OrbSize", float) = 1
        _Active ("Active", float) = 1
        _Scale ("Scale",float) = 40
        _BaseHue ("BaseHue",float) = .3
        _Cube ("Reflection Map", Cube) = "" {}
        _NoiseSize ("NoiseSize",float) = 1
        _NoisePower ("NoisePower",float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            float _TimeInClip;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 rd : TEXCOORD2;
                float3 up : TEXCOORD3;
                float3 center : TEXCOORD4;
                float3 scale  : TEXCOORD5;
            };

            sampler2D _MainTex;
            sampler2D _AudioTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.ro = mul( UNITY_MATRIX_M , v.vertex).xyz;
                o.rd = _WorldSpaceCameraPos - o.ro;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);


                return o;
            }

            #include "../Chunks/noise.cginc"
            #include "../Chunks/hsv.cginc"
            #include "../Chunks/sdfFunctions.cginc"

 
#include "../Chunks/noise.cginc"
float3 center;
float3 up;

float _OrbHeight;
float _OrbSize;
float _Active;
float _Scale;
float _BaseHue;
float _NoiseSize;
float _NoisePower;
  uniform samplerCUBE _Cube;

//--------------------------------
// Modelling 
//--------------------------------
float2 map( float3 pos ){  
    float3 localPos = mul( unity_WorldToObject , float4(pos,1)).xyz;
 	float2 res = float2( sdBox( localPos, .35 )* _Scale, 0. ) ; 
     //res = hardS( res, float2( sdSphere( center-pos ,40) , 2 ));



float3 fPos = center + up * _OrbHeight - pos;
res =smoothS( float2(sdSphere( fPos , _OrbSize * _Scale / 3),2),res, 2);//    

float n = noise( pos * .2  * _NoiseSize);
n += noise( pos * .6* _NoiseSize) * .5;
res.x -= n*2*_NoisePower* _Scale/10 ;
res.y += n * 1;
res = hardU( float2(sdSphere( fPos , _OrbSize * .4 * _Scale / 3),2),res);//
//res = float2(sdSphere( center-pos , _OrbSize * .5 * _Scale / 3),3);//


    return res;
    
}




float2 calcIntersection( float3 ro, float3 rd ){

float MAX_TRACE_DISTANCE = 30.0;           // max trace distance
float INTERSECTION_PRECISION = 1.01;        // precision of the intersection


    
    float h = .1*2.0;
    float t = 0.0;
	float res = -1.0;
    float id = -1.;
    
    for( int i=0; i< 10 ; i++ ){
        
        if( h <  .1 || t > MAX_TRACE_DISTANCE ) break;
	   	float2 m = map( ro+rd*t );
        h = m.x;
        t += h;
        id = m.y;
        
    }

    if( t < MAX_TRACE_DISTANCE ) res = t;
    if( t > MAX_TRACE_DISTANCE ) id =-1.0;
    
    return float2( res , id );
    
}



// Calculates the normal by taking a very small distance,
// remapping the function, and getting normal for that
float3 calcNormal( in float3 pos ){
    
	float3 eps = float3( 0.01, 0.0, 0.0 );
	float3 nor = float3(
	    map(pos+eps.xyy).x - map(pos-eps.xyy).x,
	    map(pos+eps.yxy).x - map(pos-eps.yxy).x,
	    map(pos+eps.yyx).x - map(pos-eps.yyx).x );
	return normalize(nor);
}


 
            fixed4 frag (v2f v) : SV_Target
            {

                float3 ro = v.ro;

                center = mul(unity_ObjectToWorld, float4(0,0,0,1));
                up = mul(unity_ObjectToWorld, float4(0,1,0,0));

            
                //ro -= center;
                float3 rd =  normalize(_WorldSpaceCameraPos - ro );//normalize(v.rd);

                float2 res = calcIntersection( ro, -rd );

                float3 col = float3(1,0.3,0);


                float3 dif = mul( unity_WorldToObject , float4(ro,1)).xyz;
                col = 0;



                if( res.y > -.5 ){
                    float3 pos = ro + res.x * rd;
                    float3 nor = calcNormal( pos );

                    float m = dot( rd , nor );

                    float3 refl = reflect( -rd, nor );
                    float3 cubeCol= texCUBE(_Cube, refl);

                    //float lightMatch = 
                    col = cubeCol * 2 * hsv(res.y * .1 + abs(dif.y) * .1 + abs(dif.x) * .1 + _BaseHue + .1 * m,_Active,_Active * .5 + .5 + abs((res.y-2)) * .2) * m;//m;//hsv( m * m * m * .2 + _BaseHue , m,1-m*m*m*m*m);//nor * .5  + .5;
                }else{
                    discard;
                }
  
                return float4(col,1);
            }
            ENDCG
        }
    }

    FallBack "Unlit"
}
