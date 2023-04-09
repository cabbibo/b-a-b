Shader "IMMAT/Crazy/BirdWings"
{
    Properties {
        _ColorMap("_ColorMap", 2D) = "white" {}
        _Steps("_Steps", Int) = 5
        _ColorMapStart("_ColorMapStart",float) = 0
        _ColorMapSize("_ColorMapSize",float) = 0
        _OutlineWidth("_OutlineWidth",float) = 0
        _OutlineHue("_OutlineHue", float ) = .5
        
        _Multiplier("_Multiplier", float ) = 1
        _BumpMap("_BumpMap", 2D) = "white" {}
        _MainTex("_MainTex", 2D) = "white" {}
        
        _TexSize("_TexSize", float ) = 1
        _ColorID("_ColorID", float ) = 1
   
    }


CGINCLUDE
            float _TexSize;
            #include "../../Chunks/hash.cginc"
        float2 randomUV( float2 uv , int id ){

                int which = id / 12;
            
                float v1 = hash(float(which));
                float v2 = hash(float(which * 10));

                float2 randUV = float2( floor( v1 * _TexSize )/ _TexSize , floor(v2 * _TexSize)/_TexSize);
                randUV += uv / _TexSize;
                return randUV;

        }
ENDCG

    SubShader
    {
        

        
        Pass
        {

            Stencil
            {
                Ref 9
                Comp always
                Pass replace
                ZFail keep
            }

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
            #include "../../Chunks/snoise.cginc"

            sampler2D _MainTex;
            sampler2D _ColorMap;
            sampler2D _BumpMap;
            float4 _BumpMap_ST;

            sampler2D _FullColorMap;
            float _ColorID;


            int _Steps;
            float _ColorMapStart;
            float _ColorMapSize;

            struct v2f { 
              float4 pos : SV_POSITION; 
              float3 nor : NORMAL;
              float2 uv :TEXCOORD0; 
              float3 worldPos :TEXCOORD1;
              float2 debug : TEXCOORD3;
              float3 eye : TEXCOORD4;
              float2 uv2 : TEXCOORD10;
             LIGHTING_COORDS(5,6) 
                 half3 tspace0 : TEXCOORD7; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD8; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD9; // tangent.z, bitangent.z, normal.z
            };
            float4 _Color;
            float _Multiplier;

            StructuredBuffer<Vert> _VertBuffer;
            StructuredBuffer<int> _TriBuffer;

            float4 _ColorMultiplier;

            v2f vert ( uint vid : SV_VertexID )
            {
                v2f o;


            

                UNITY_INITIALIZE_OUTPUT(v2f, o);
                Vert v = _VertBuffer[_TriBuffer[vid]];
                o.pos = mul (UNITY_MATRIX_VP, float4(v.pos,1.0f));

                o.nor = v.nor;
                o.uv = TRANSFORM_TEX(v.uv,_BumpMap).xy;
                o.worldPos = v.pos;
                o.debug = v.debug;
                o.eye = v.pos - _WorldSpaceCameraPos;



                o.uv2 = randomUV(v.uv,vid);


                half3 wNormal = v.nor;//UnityObjectToWorldNormal(normal);
                half3 wTangent = v.tan;//UnityObjectToWorldDir(tangent.xyz);
                half3 wBitangent = normalize(cross(wNormal, wTangent));// * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);

                UNITY_TRANSFER_LIGHTING(o,o.worldPos);


                return o;
            }

   // normal map texture from shader properties

            fixed4 frag (v2f v) : SV_Target
            {

                      // sample the normal map, and decode from the Unity encoding
                half3 tnormal = UnpackNormal(tex2D(_BumpMap, v.uv));
                // transform normal from tangent to world space
                half3 worldNormal;
                worldNormal.x = dot(v.tspace0, tnormal);
                worldNormal.y = dot(v.tspace1, tnormal);
                worldNormal.z = dot(v.tspace2, tnormal);


                    half3 worldViewDir = normalize(UnityWorldSpaceViewDir(v.worldPos));
         half3 worldRefl = reflect(-worldViewDir, worldNormal);
         half4 skyData =UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
         half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);
//

                fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);
                float val = dot( reflect(worldNormal,normalize(v.eye)) , _WorldSpaceLightPos0.xyz);
                val += dot( worldNormal , normalize( v.eye));
                float3 col = tex2D(_ColorMap, val * _ColorMapSize + _ColorMapStart + _Time.y ) ;
                col =  1;// skyColor*2 * _Multiplier;


            float4 tCol = tex2D(_MainTex,v.uv2);
                col = tCol.xyz;

                col = skyColor * 10;
                
                float m =  dot( _WorldSpaceLightPos0.xyz  , v.nor );
                col = tex2D(_FullColorMap , float2(tCol.x * .5 + m * .1 ,.5)).xyz * skyColor * 10;//dot( _WorldSpaceLightPos0.xyz  , v.nor );

                if( tCol.a < .5 ){
                    discard;
                }



                

    float shadowStep = shadow;//floor(shadow * 3)/3;

  float3 shadowCol = 0;
  
    for( int i = 0; i < 3; i++){

      float3 fPos = v.worldPos - normalize(v.eye) * float(i) * 1.3;
      float v = (snoise(fPos * 10)+1)/2;
      shadowCol += hsv((float)i/3,1,v);

    
    }//
    shadowCol *= shadowCol;
    shadowCol *= shadowCol;
    shadowCol *= shadowCol;
    shadowCol = pow( shadowCol,5);

    ////shadowCol = length(shadowCol) * (shadowCol * .8 + .3)  * 10;//

    //shadowCol += .5;
   // shadowCol *= float3(.1 , .3 , .6);

    shadowCol /=  clamp( (.1 + .1* length( v.eye)), 2, 3);
    col = shadowStep * col * float3(1,1,.6) +  clamp( (1-shadowStep) * length(col) * length(col) * 10 , .1, 1) * shadowCol;// float3(.1,.2,.5);


col =  tCol.xyz * length(shadowCol) * (shadowCol* .8 + .2)  * 300 * float3(1, .8,.3)  * v.uv.x * 2;
col += 1* tex2D(_FullColorMap, float2(tCol.x * .3 , v.debug.x * 1/16 ))  * _ColorMultiplier;//hsv(tCol.x * .3,1,1);


float3 cMap = tex2D(_ColorMap, float2(v.debug.x / 20,.05 ));
col = length(shadowCol)* 300* cMap + cMap * (1-tCol.xyz) * m;

 if( tCol.a < .5 ){
                    discard;
                }
  //col = v.uv.x;


                return float4(col,1);
            }

            ENDCG
        }

// Shadow Pass
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

      #include "../../Chunks/Struct16.cginc"
      #include "../../Chunks/ShadowCasterPos.cginc"
   

      StructuredBuffer<Vert> _VertBuffer;
      StructuredBuffer<int> _TriBuffer;

      struct v2f {
        V2F_SHADOW_CASTER;
        float3 nor : NORMAL;
        float2 uv : TEXCOORD0;
      };


      v2f vert(appdata_base input, uint id : SV_VertexID)
      {
        v2f o;
        Vert v = _VertBuffer[_TriBuffer[id]];

        float4 position = ShadowCasterPos(v.pos, -v.nor);
        o.pos = UnityApplyLinearShadowBias(position);
        o.uv = randomUV(v.uv,id);
        return o;
      }

      sampler2D _MainTex;

      float4 frag(v2f i) : COLOR
      {

        if( tex2D(_MainTex,i.uv).a < .5){
          discard;
        }
        SHADOW_CASTER_FRAGMENT(i)
      }
      ENDCG
    }
    
  
  

  
    }
}
