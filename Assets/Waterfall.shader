// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Waterfall"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
       //Blend One One // Additive
       //ZWrite Off
        Cull Off
        GrabPass{"_BackgroundTexture"}
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
                float4 color : COLOR;
                float3 normal : NORMAL;

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            

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


      sampler2D _CameraDepthTexture;


            sampler2D _BackgroundTexture;
            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.normal = mul( unity_ObjectToWorld, float4(v.normal,0)).xyz;

                o.worldPos = mul( unity_ObjectToWorld, v.vertex).xyz;

                o.worldPos += float3(0,sin( o.worldPos.x  * .1 + _Time.y * 10) * 2,0);
                o.vertex = mul(UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
                
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
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
                col = tex;

                col += float4(.1,.4,1,1);


                float4  tmpCol = col;

                if( v.color.z > .1){
                    discard;
                }

                half4 bgcolor = tex2Dproj(_BackgroundTexture, v.grabPos);
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

if( tmpCol.x > .3 ){
    col = 1;
    if( tmpCol.x > .5 ){
        discard;
    }
}


//col = v.uv.x;


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
