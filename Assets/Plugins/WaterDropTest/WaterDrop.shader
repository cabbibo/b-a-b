Shader "Unlit/WaterDrop"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos: TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul( unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float4 _Drop1;
            float4 _Drop2;
            float4 _Drop3;


            float4 _MainBall;


            float4 _Drops[12];






// checks to see which intersection is closer
// and makes the y of the float2 be the proper id
float2 opU( float2 d1, float2 d2 ){
    
	return (d1.x<d2.x) ? d1 : d2;
    
}

float sdBox( float3 p, float3 b )
{
  float3 d = abs(p) - b;
  return min(max(d.x,max(d.y,d.z)),0.0) +
         length(max(d,0.0));
}

float sdSphere( float3 p, float s )
{
  return length(p)-s;
}

float2 smoothU( float2 d1, float2 d2, float k)
{
    float a = d1.x;
    float b = d2.x;
    float h = clamp(0.5+0.5*(b-a)/k, 0.0, 1.0);
    return float2( lerp(b, a, h) - k*h*(1.0-h), lerp(d2.y, d1.y, pow(h, 2.0)));
}

//--------------------------------
// Modelling 
//--------------------------------
float2 map( float3 pos ){  
   

   float radiusMultiplier = .3;
     
 	float2 res = float2( sdSphere( pos - _MainBall.xyz, _MainBall.w *radiusMultiplier)   , 1 );

     for( int i = 0; i < 12; i++ ){
         res = smoothU(res,  float2( sdSphere( pos - _Drops[i].xyz , _Drops[i].w *radiusMultiplier),1   ) ,.1);
     }

   // res = opU( res , float2( sdBox( pos- _Drop2, float3( .4 , .3 , .2 )) , 2. ));
    
    return res;
    
}



float2 calcIntersection( in float3 ro, in float3 rd ){
float MAX_TRACE_DISTANCE = 30.0; 
    float INTERSECTION_PRECISION = 0.001;
    float h =  INTERSECTION_PRECISION*2.0;
    float t = 0.0;
	float res = -1.0;
    float id = -1.;
    
    for( int i=0; i< 100 ; i++ ){
        
        if( h < INTERSECTION_PRECISION || t > MAX_TRACE_DISTANCE ) break;
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
    
	float3 eps = float3( 0.001, 0.0, 0.0 );
	float3 nor = float3(
	    map(pos+eps.xyy).x - map(pos-eps.xyy).x,
	    map(pos+eps.yxy).x - map(pos-eps.yxy).x,
	    map(pos+eps.yyx).x - map(pos-eps.yyx).x );
	return normalize(nor);
}



            fixed4 frag (v2f i) : SV_Target
            {

                float3 ro = i.worldPos;
                float3 rd = normalize(ro - _WorldSpaceCameraPos);


                float4 col = 0;

                float2 res = calcIntersection( ro , rd  );

                col = res.y;
                if( res.y > -.5 ){
                    col = 1;
                }

                return col;
            }
            ENDCG
        }
    }
}
