// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Waterfall"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _NormalMap ("NormalMap", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-10" }
        LOD 100
       //Blend One One // Additive
       //ZWrite Off
        Cull Off
        GrabPass{"_BackgroundTexture1"}
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            
      #include "Assets/Resources/Shaders/Chunks/SunShadows.cginc"
      #include "Assets/Resources/Shaders/Chunks/hsv.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 grabPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float3 normal : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
                float3 tangent : TEXCOORD5;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

    sampler2D _NormalMap;

        sampler2D _HeightMap;
        float3 _MapSize;
        float3 _MapOffset;

        float3 worldPos( float3 pos ){
        float4 c = tex2D(_HeightMap , (pos.xz) / _MapSize.xz + .5 /1024);//tex2Dlod(_HeightMap , float4(pos.xz * _MapSize,0,0) );
        pos.y = _MapSize.y * c.x;//* sin(.1 *length(pos.xz)) ;//c.x * 1000;//_MapHeight;
        return pos;
        }


        float3 worldPosTexture( float3 pos ){

        float2 fPos = pos.xz - _MapOffset.xz;
        fPos -= _MapSize.xz;

        float4 c = tex2D(_HeightMap , (pos.xz) / _MapSize.xz  );//tex2Dlod(_HeightMap , float4(pos.xz * _MapSize,0,0) );
        pos.y = _MapSize.y * c.x *2;//* sin(.1 *length(pos.xz)) ;//c.x * 1000;//_MapHeight;
        return pos;
        }


        float terrainHeight( float3 pos ){
        return worldPosTexture(pos).y;
        }

// Generic algorithm to desaturate images used in most game engines
float4 generic_desaturate(float3 color, float factor)
{
	float3 lum = float3(0.299, 0.587, 0.114);
	float3 gray = dot(lum, color);
	return float4(lerp(color, gray, factor), 1.0);
}

float3 rgb2hsb( in float3 c ){
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz),
                 float4(c.gb, K.xy),
                 step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r),
                 float4(c.r, p.yzx),
                 step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)),
                d / (q.x + e),
                q.x);
  }

  
  
      sampler2D _CameraDepthTexture;


            sampler2D _BackgroundTexture1;
            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;

                o.worldPos = mul( unity_ObjectToWorld, v.vertex).xyz;

                o.worldPos += float3(0,(sin( o.worldPos.x  * .1 + _Time.y * 10)+1) * 2 * v.uv.y,0);
                o.vertex = mul(UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
                
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.normal = mul( unity_ObjectToWorld, float4(v.normal,0)).xyz;
                o.tangent = mul( unity_ObjectToWorld, float4(v.tangent.xyz * v.tangent.w,0)).xyz;
                return o;
            }

            fixed4 frag (v2f v) : SV_Target
            {
                
                float4 col = v.color;

       

                col = (sin(v.color.y * 100) + 1)/2;

                float3 ro = v.worldPos;
                float3 rd = normalize(v.worldPos - _WorldSpaceCameraPos);

                float4 tex = tex2D(_MainTex, v.uv + float2(sin(v.color.x * 20 ), -.3*_Time.y + .1*sin(v.color.x * 30 + _Time.y)));
                tex += tex2D(_MainTex, v.uv + float2(sin(v.color.x * 24 +31 ), -.36*_Time.y + .13*sin(v.color.x * 37.1 + 33+ _Time.y * 1.2)));
                tex += tex2D(_MainTex, v.uv + float2(sin(v.color.x * 42 +111 ), -.26*_Time.y + .15*sin(v.color.x * 5.1 + 323+ _Time.y * 1.12)));

                float waterTex = tex;

               // float3 ro = v.worldPos;
               // float3 rd = normalize(_WorldSpaceCameraPos - v.worldPos);//normalize(rd);

                
float3x3 TBN = float3x3( 

    v.normal,
    v.tangent,
    cross(v.normal, v.tangent)

  );


  float3 n1 =(tex2D(_NormalMap, v.uv));

  TBN = transpose(TBN);

  float3 fNormal = mul(TBN, float3(n1.z ,n1.y,n1.x * .01)- float3(0,0,0));
  fNormal = normalize(fNormal);

  col.xyz = normalize(fNormal)  * .5 + .5;
  //col.xyz = pow(v.normal.z,10);// * .5 + .5;


  float3 sampleWorldSpace = ro + refract(rd, fNormal, .3) * 10;
    float4 bgCol = tex2Dproj(_BackgroundTexture1, ComputeGrabScreenPos( mul(UNITY_MATRIX_VP, float4(sampleWorldSpace,1.0f))));


                col = tex;

                col += float4(.1,.4,1,1);


                float4  tmpCol = col;

                if( v.color.z > .1){
                    discard;
                }

                half4 bgcolor = tex2Dproj(_BackgroundTexture1, v.grabPos);
                col = lerp(bgcolor, col, length(col));
                col = bgcolor *  float4(.1,.4,1,1);



                        // apply depth texture
        float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, v.screenPos);
        float depth = LinearEyeDepth(depthSample).r;
        float foamLine = 1 - saturate(.1 * (depth - v.screenPos.w));
        col = foamLine;

        float height = terrainHeight( ro );
        float delta = v.worldPos.y - height;

col = .001 *abs(delta);

col =.001* v.worldPos.y;
col = .01*abs(height-v.worldPos.y);


int breakN = 100;
for( int i = 0; i< 10; i++ ){

    float3 p = ro + rd * float(i) * 3;

    float h = terrainHeight( p );
    if( h > p.y ){
        breakN = i;
        break;
    }

}


col = 5*bgcolor / pow(abs(height-v.worldPos.y),2);
col = (1-saturate(float( breakN)/10)*.9) * float4(.1,.4,1,1);
col *= bgcolor;

col *= tmpCol * tmpCol *tmpCol * tmpCol * tmpCol ;

if( abs(v.uv.x-.5) > .44 + tmpCol.x * .1 ){
    col =1;

    if( abs(v.uv.x-.5) > .45 + tmpCol.x * .1 ){
        discard;
    }
}

col = bgcolor * _LightColor0;
if( tmpCol.x > .3 ){
    col = _LightColor0;
    if( tmpCol.x > .5 ){
        discard;
    }
}

col = floor(tmpCol.x * 8)/6;
col *= _LightColor0;

col *= 2;
//col += tmpCol.x;
if(length(col) < .01){
    discard;
}
col = saturate(col);

col = bgcolor * _LightColor0;

col.xyz = v.tangent;
col.xyz = normalize(cross(v.normal, v.tangent));



  float shadowAttenuation = GetSunShadowsAttenuation_PCF5x5(v.worldPos, v.screenPos.z, 0).x;	

  col = dot( fNormal, _WorldSpaceLightPos0.xyz) * _LightColor0;

  col *= shadowAttenuation  * .7 +.3;

  col.xyz = fNormal * .5 + .5;

  col.xyz = bgCol.xyz;

//col = v.uv.x;

float lightHue = rgb2hsb(_LightColor0.xyz).x;
//col.xyz = (bgCol *1) *hsv(  lightHue,.2,2);
col.xyz = generic_desaturate(bgCol,.3) * hsv(.1+lightHue,.5,1);

if( tmpCol.x > .49){
    col = _LightColor0 *shadowAttenuation;
}

if( tmpCol.x > .3){
    col *=2;
}

if( tmpCol.x > .1){
    col *=2;
}


col *= shadowAttenuation+1;
//col = abs(v.uv.x-.5);

      //  col = lerp(bgcolor,  float4(.1,.4,1,1) * 0 , 1-foamLine);
            
      //  col.xyz = normalize(v.normal) ;

        //col += tex * tex * tex;

      //  col = tex * dot( normalize(v.normal), float3(0,1,0)) *4;
    
               // col *= .5;
                return col;
            }
            ENDCG
        }
    }
}
