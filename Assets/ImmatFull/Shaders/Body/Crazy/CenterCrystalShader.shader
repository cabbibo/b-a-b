Shader "IMMAT/Crazy/CenterCrystalFlowShader"
{
    Properties {
        _ColorMap("_ColorMap", 2D) = "white" {}
        
        _AudioMap("_AudioMap", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _SkyMap("SkyMap",CUBE) = "white" {}
        _SkyMap("SkyMap",CUBE) = "white" {}
    }
    SubShader
    {
        
        Pass
        {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off

          Tags{ "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            // make fog work
            #pragma multi_compile_fogV
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            #include "../../Chunks/Struct16.cginc"
            #include "../../Chunks/hsv.cginc"
            #include "../../Chunks/noise.cginc"

            sampler2D _MainTex;
            sampler2D _ColorMap;
            sampler2D _AudioMap;
            samplerCUBE _SkyMap;

            struct v2f { 
              float4 pos : SV_POSITION; 
              float3 nor : NORMAL;
              float2 uv :TEXCOORD0; 
              float3 worldPos :TEXCOORD1;
              float2 debug : TEXCOORD3;
              float3 eye : TEXCOORD4;
              float colVal : TEXCOORD7;
             LIGHTING_COORDS(5,6) 
            };
            float4 _Color;

            StructuredBuffer<Vert> _VertBuffer;
            StructuredBuffer<int> _TriBuffer;

            v2f vert ( uint vid : SV_VertexID )
            {
                v2f o;

                UNITY_INITIALIZE_OUTPUT(v2f, o);
                Vert v = _VertBuffer[_TriBuffer[vid]];
                o.pos = mul (UNITY_MATRIX_VP, float4(v.pos,1.0f));


                o.nor = v.nor;
                o.uv = v.uv;
                o.worldPos = v.pos;
                o.debug = v.debug;
                o.eye = v.pos - _WorldSpaceCameraPos;

                o.colVal =  ((floor((v.debug.x*1.1314)) % 7)+.5)/7;


                UNITY_TRANSFER_LIGHTING(o,o.worldPos);

                return o;
            }



            fixed4 frag (v2f v) : SV_Target
            {
                // sample the texture
                fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);
                //fixed shadow = LIGHT_ATTENUATION(v) ;

               // float colVal = floor((v.debug.x*13.1314)) % 7;
                float4 c = tex2D(_ColorMap, float2(v.colVal + (.5-abs(v.uv.y-.5)) * .2,1));
                float4 aC = tex2D(_AudioMap, float2(v.uv.x * .1  + sin(v.debug.x) * .2 +.2 ,1));
                float3 col = c.xyz * .4  + .9;// + 1-v.uv.x;//hsv( v.uv.x * .1 + v.debug.x*13.1314 ,1,1);//_Color.xyz;
                float3 worldNormal = -normalize(cross(ddx(v.worldPos), ddy(v.worldPos)));// v.nor;
            half3 worldViewDir = normalize(UnityWorldSpaceViewDir(v.worldPos));
            half3 worldRefl = reflect(-worldViewDir, worldNormal);
            half4 skyData = texCUBE(_SkyMap, worldRefl);
            half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);
                
                if( abs(v.uv.y-.5) - length(aC) * .1> .2 ){
                    discard;
                }
                col = col * skyColor * (aC * aC * .8 + .2) * 5;

                //col = saturate(col);
                //col *= shadow;
                return float4(col,1);
            }

            ENDCG
        }

/*
Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}
Blend One One


			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            // make fog work
            #pragma multi_compile_fogV
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            #include "../../Chunks/Struct16.cginc"
            #include "../../Chunks/hsv.cginc"

            sampler2D _MainTex;
            sampler2D _ColorMap;

            struct v2f { 
              float4 pos : SV_POSITION; 
              float3 nor : NORMAL;
              float2 uv :TEXCOORD0; 
              float3 worldPos :TEXCOORD1;
              float2 debug : TEXCOORD3;
              float3 eye : TEXCOORD4;
             LIGHTING_COORDS(5,6) 
            };
            float4 _Color;

            StructuredBuffer<Vert> _VertBuffer;
            StructuredBuffer<int> _TriBuffer;

            v2f vert ( uint vid : SV_VertexID )
            {
                v2f o;

                UNITY_INITIALIZE_OUTPUT(v2f, o);
                Vert v = _VertBuffer[_TriBuffer[vid]];
                o.pos = mul (UNITY_MATRIX_VP, float4(v.pos,1.0f));


                o.nor = v.nor;
                o.uv = v.uv;
                o.worldPos = v.pos;
                o.debug = v.debug;
                o.eye = v.pos - _WorldSpaceCameraPos;


                UNITY_TRANSFER_LIGHTING(o,o.worldPos);

                return o;
            }



            fixed4 frag (v2f v) : SV_Target
            {
                // sample the texture
                //fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos) * .5 + .5;
                fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);
                float3 col = _Color.xyz;

                col *= shadow;
                return float4(col,1);
            }

            ENDCG
		}
    // SHADOW PASS

    Pass
    {
      Tags{ "LightMode" = "ShadowCaster" }


      Fog{ Mode Off }
      ZWrite On
      ZTest LEqual
      Cull Off
      Offset 1, 1
      CGPROGRAM

      #pragma target 4.5
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_shadowcaster
      #pragma fragmentoption ARB_precision_hint_fastest

      #include "UnityCG.cginc"

      float DoShadowDiscard( float3 pos , float2 uv ){
         return   1-length( uv - .5 );
      }

      #include "../../Chunks/Shadow16.cginc"
      ENDCG
    }*/
  
    }
}
