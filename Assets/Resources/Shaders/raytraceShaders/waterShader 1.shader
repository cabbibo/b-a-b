Shader "water/trace"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("Bump Map", 2D) = "white" {}
    _SurfaceColor ("SurfaceColor", Color) = (1,1,1,1)
    _DeepColor ("DeepColor", Color) = (1,1,1,1)
    _WaterClarity ("_WaterClarity",float ) = .05

    _WrenShadowColor ("Player Shadow Color", Color) = (1,1,1,1)
        _WrenShadowRadius("Player shadow Radius", float) = 1
        _WrenShadowThickness("Player shadow Thickness", float) = 0.2
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

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"
            
			sampler2D _CameraDepthTexture;
            sampler2D _BackgroundTexture;
			sampler2D _FoamMap;

             struct appdata
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 texcoord : TEXCOORD0;
            };


           //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor : NORMAL;
          float3 ro : TEXCOORD1;
          float3 rd : TEXCOORD2;
          float3 eye : TEXCOORD3;
          float3 localPos : TEXCOORD4;
          float3 localNor : TEXCOORD15;
          float3 worldPos : TEXCOORD10;
          float3 worldNor : TEXCOORD5;
          float3 lightDir : TEXCOORD6;
          float4 grabPos : TEXCOORD7;
          float4 screenPos : TEXCOORD9;
          float3 unrefracted : TEXCOORD8;
          float2 uv : TEXCOORD11;
                half3 tspace0 : TEXCOORD12;
                half3 tspace1 : TEXCOORD13;
                half3 tspace2 : TEXCOORD14;
          
          
      };


            sampler2D _MainTex;
            float4 _MainTex_ST;

                 sampler2D _HeightMap;
                 sampler2D _BumpMap;

                 float4 _SurfaceColor;
                 float4 _DeepColor;
                 float _WaterClarity;
            
                 float3 _WrenPos;
                 half4 _WrenShadowColor;
            float _WrenShadowRadius;
            float _WrenShadowThickness;
            
     float3 _MapSize;
            float terrainHeight( float3 pos ){
                float height = tex2Dlod( _HeightMap , float4(((pos.xz)) / _MapSize.xz + .5, 0 ,0)).x;// , 1).x * 4000
                return height * _MapSize.y;//float3( pos.x ,height * 4000 , pos.z);
                //return float3( pos.x ,pos.y, pos.z);
            }

          
#include "../Chunks/hsv.cginc"
#include "../Chunks/noise.cginc"
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
        
    half3 wNormal = UnityObjectToWorldNormal(vertex.normal);
    half3 wTangent = UnityObjectToWorldDir(vertex.tangent.xyz);
    half tangentSign = vertex.tangent.w * unity_WorldTransformParams.w;
    half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
    o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
    o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
    o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
        
        float3 localP = normalize(_WorldSpaceCameraPos - worldPos);//mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1)).xyz;
        float3 eye = normalize(_WorldSpaceCameraPos - worldPos);//normalize(localP - p.xyz);

        o.worldPos = mul( unity_ObjectToWorld , vertex.position ).xyz;
        o.unrefracted = eye;
        o.rd = eye;
        o.eye =normalize(_WorldSpaceCameraPos - worldPos);
        o.worldNor = normalize(mul (unity_ObjectToWorld, float4(-n,0.0f)).xyz);

        float4 refractedPos = UnityObjectToClipPos( float4(o.ro + o.rd * 1.5,1));
        o.grabPos = ComputeGrabScreenPos(refractedPos);
        o.screenPos = ComputeScreenPos(o.pos);
    

        return o;

        }

            fixed4 frag (varyings v) : SV_Target
            {
                // sample the texture
                float3 col = 1;

        float eyeMatch = dot( v.rd , float3(0,1,0));
        //v.rd += nT3D( v.ro );//normalize(float3( noise( v.ro +14 ) , 0 , noise( v.ro  )))* .4;
        v.rd = normalize(v.rd);


        float3 refractedPos = v.ro + v.rd * 1;

        float4 clipRefractedPos = mul(UNITY_MATRIX_VP, float4(refractedPos,1));
        float4 refrScreenPos = ComputeScreenPos(clipRefractedPos);

        half3 tnormal = UnpackNormal(tex2D(_BumpMap, v.worldPos.xz *.0001* 30+_Time.y* .01))*2;
         tnormal += UnpackNormal(tex2D(_BumpMap, v.worldPos.xz *.0001* 4+_Time.y*.01)) * 3;
         tnormal += UnpackNormal(tex2D(_BumpMap, v.worldPos.xz *.0001* 65-_Time.y* .01)) * 1;
         tnormal = normalize( tnormal );
        half3 worldNormal;
        worldNormal.x = dot(v.tspace0, tnormal);
        worldNormal.y = dot(v.tspace1, tnormal);
        worldNormal.z = dot(v.tspace2, tnormal);

    worldNormal.y = abs(worldNormal.y);
        worldNormal *= 30;
        worldNormal = floor( worldNormal );
        worldNormal /= 30;
        

  

	//float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,v.screenPos);
	//float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,refrScreenPos);
    float depthSample =tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(v.screenPos));
				float depth = LinearEyeDepth(depthSample).r;
//depth *= .9-.1*dot( normalize(v.rd) , float3(0,1,0));
float foamLine = 1 - saturate(_WaterClarity * (depth - v.screenPos.w));
    //foamLine *= 10;
   //     foamLine = floor( foamLine);
        //foamLine /= 10;

        float3 refractedRD = refract( v.rd , worldNormal , .4);
        refractedPos = v.ro - refractedRD * (.5-abs(length( (v.screenPos.xy/v.screenPos.w)-.5)))*10;

        half3 worldViewDir = normalize(UnityWorldSpaceViewDir(v.ro));

        
        float match = dot(worldViewDir , worldNormal);
        half3 worldRefl = reflect(-worldViewDir, worldNormal);
        half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
        half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR); 

    


//foamLine += noise( v.ro * .1 ) * .1;

float4 refredPos= mul(UNITY_MATRIX_VP, float4(refractedPos,1));
  float4 refractedBGPos = ComputeGrabScreenPos(refredPos);
float4 backgroundCol = tex2Dproj(_BackgroundTexture, refractedBGPos);

                col = 0;//skyColor * pow( (1-match) , 4);//foamLine;
               // col += foamLine;
      if( dot(worldNormal,float3(0,1,0)) < .6 ){
            col = float3(1,0,0);
        }

                col = lerp( backgroundCol * _SurfaceColor , _DeepColor , 1-foamLine);
                col +=skyColor * saturate(pow( (1-match) , 10));

                col = skyColor;

                if( foamLine > .6+ noise(v.ro * .1 + _Time.y) * .1 && foamLine < 1){
                    col += 1;
                }

                // player shadow
                float2 pos = _WrenPos.xz;
                float shdist = length(pos - v.worldPos.xz);
                float radius = abs(_WrenPos.y - v.worldPos.y) * _WrenShadowRadius - 0.5;
                float shouter = shdist - radius;
                float shinner = shdist - (radius-_WrenShadowThickness);
                float sh = 1 - saturate(max(-shinner, shouter));
                // sh *= sh * sh * sh * sh;
                float sha = lerp (0, _WrenShadowColor.a, saturate(1-((radius-5) / 10)));
                col.rgb = lerp(col, _WrenShadowColor.rgb, sh * sha);

                return float4(col,1);
            }
            ENDCG
        }
    }

}
