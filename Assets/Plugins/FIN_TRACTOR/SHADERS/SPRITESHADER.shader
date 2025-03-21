// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Lit/SpriteCutout" {
    Properties {

        _CutoutColor("CutoutColor", Color) = (1,1,1,1)
        _CutoutCutoff("CutoutCutoff", Range(0,1)) = .5
        _UseAlpha("UseAlpha", Float) = 0
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _MainTex2  ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _OverallMultiplier("OverallMultiplier", Range(0,10)) = 1
        _ShadowStrength("ShadowStrength", Range(0,1)) = 1
        _Invert("Invert", Float) = 0

    }









    CGINCLUDE

    #include "Lighting.cginc"

    

    
    #include "UnityCG.cginc"
    #include "AutoLight.cginc"
    #include "UnityLightingCommon.cginc"
    
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _CutoutColor;
            float _CutoutCutoff;
            float4 _Color;
            float _UseAlpha;
    
    struct inputData {
        float4 vertex : POSITION;
        float4 tangent : TANGENT;
        float3 normal : NORMAL;
        float4 texcoord : TEXCOORD0;
        float4 texcoord1 : TEXCOORD1;
        fixed4 color : COLOR;
        
        uint   id                : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        
    };


    
    //A simple input struct for our pixel shader step containing a position.
    struct varyings {
        float4 pos      : SV_POSITION;
        float3 nor      : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
        float3 eye      : TEXCOORD2;
        float2 uv       : TEXCOORD4;

        // UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
        
        UNITY_SHADOW_COORDS(8)
        UNITY_FOG_COORDS(10)
    };



    #include "Assets/Resources/Shaders/Chunks/ShadowCasterPos.cginc"
    float4 CustomShadowPos(float4 vertex, float3 normal)
    {
        float4 wPos = vertex;
        float3 wNormal = normal;

        if (unity_LightShadowBias.z != 0.0)
        {
            
            float3 wLight = normalize(UnityWorldSpaceLightDir(wPos.xyz));

            // apply normal offset bias (inset position along the normal)
            // bias needs to be scaled by sine between normal and light direction
            // (http://the-witness.net/news/2013/09/shadow-mapping-summary-part-1/)
            //
            // unity_LightShadowBias.z contains user-specified normal offset amount
            // scaled by world space texel size.

            float shadowCos = dot(wNormal, wLight);
            float shadowSine = sqrt(1-shadowCos*shadowCos);
            float normalBias = unity_LightShadowBias.z * shadowSine;

            wPos.xyz -= wNormal * normalBias *10;
        }

        return mul(UNITY_MATRIX_VP, wPos);
    }

    float getDiscard(float2 uv){
        float4 color= tex2D(_MainTex, uv).rgba;

        if( _UseAlpha > .5){
            if( color.a < .5){
                return 0;
            }else{
                return 1;
            }
        }

        float4 dif = length(color - _CutoutColor);
        if( length(dif) < _CutoutCutoff){
            return 0;
        }else{
            return 1;
        }

    }



    UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_INSTANCING_BUFFER_END(Props)
    



    ENDCG





















    SubShader{

        Pass{

            
            Tags { "RenderType"="Opaque" }
            Tags{ "LightMode" = "ForwardBase" }
            LOD 100 
            Cull Off
            

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile_fogV
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_instancing

            


            float _Fade;

            float3 _FadeLocation;
            
            float _OverallMultiplier;

            float _Invert;




            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (inputData vert){
                
                varyings o;

                UNITY_SETUP_INSTANCE_ID(vert);
                UNITY_TRANSFER_INSTANCE_ID(vert, o); // necessary only if you want to access instanced properties in the fragment Shader.
                
                int instanceID = 0;
                #if defined(UNITY_INSTANCING_ENABLED)
                instanceID = UNITY_GET_INSTANCE_ID(vert);
                #endif

                float3 wPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;
                o.worldPos = wPos;
                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
                o.eye = _WorldSpaceCameraPos - o.worldPos;
                o.nor = normalize(mul( unity_ObjectToWorld,  float4(vert.normal,0)).xyz);
                o.uv = TRANSFORM_TEX(vert.texcoord, _MainTex);

                UNITY_TRANSFER_SHADOW(o,o.worldPos);
                UNITY_TRANSFER_FOG(o,o.pos);
                

                return o;

            }

            

            float _ShadowStrength;

            //Pixel function returns a solid color for each point.
            float4 frag (varyings v) : COLOR {


                float3 col;

                fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);// * .5 + .5;

                shadow = shadow * _ShadowStrength + (1-_ShadowStrength);
                
                col = tex2D(_MainTex, v.uv).rgb;
                
                if( _Invert > .5){
                    col = 1 - col;
                }
                
               col*= shadow;

                if( getDiscard(v.uv) < .5){
                    discard;
                }
                
                col *= _Color;

            
                col *= _OverallMultiplier;
               
                return float4(col,1);
            }

            ENDCG

        }


        














        
        

        

        // shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster


            varyings vert(inputData vert)
            {
                varyings o;


                
                UNITY_SETUP_INSTANCE_ID(vert);
                UNITY_TRANSFER_INSTANCE_ID(vert, o); // necessary only if you want to access instanced properties in the fragment Shader.
                
                
                int instanceID = 0;
                #if defined(UNITY_INSTANCING_ENABLED)
                instanceID = UNITY_GET_INSTANCE_ID(vert);
                #endif


                o.nor = normalize(mul( unity_ObjectToWorld,  float4( vert.normal,0)).xyz);
                // o.worldPos = worldPos.xyz;

                float3 wPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;
                
                o.worldPos = wPos;//windAmount;
                o.eye = _WorldSpaceCameraPos - o.worldPos.xyz;
                //float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 worldNormal  = o.nor;// UnityObjectToWorldNormal( vert.normal);

                float4 opos = 0;
                opos = CustomShadowPos(float4(o.worldPos,1), worldNormal);
                opos = UnityApplyLinearShadowBias(opos);
                o.pos = opos;
                
                o.uv = TRANSFORM_TEX(vert.texcoord, _MainTex);



               

                //TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }



            float4 frag(varyings v) : SV_Target
            {

                

             if( getDiscard(v.uv) < .5){
                    discard;
                }

                //DoEdgeDiscard(lightingData,i.worldPos,i.eye);
                

                SHADOW_CASTER_FRAGMENT(i);
                

            }
            ENDCG
        }


    }

    //Fallback "Diffuse"


}








