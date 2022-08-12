// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.17 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.17;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,rpth:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:2865,x:33183,y:32369,varname:node_2865,prsc:2|diff-4182-OUT,spec-358-OUT,gloss-1813-OUT,normal-5964-RGB;n:type:ShaderForge.SFN_Tex2d,id:5964,x:32407,y:32978,ptovrint:True,ptlb:Normal Map,ptin:_BumpMap,varname:_BumpMap,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Slider,id:358,x:32250,y:32780,ptovrint:False,ptlb:Metallic,ptin:_Metallic,varname:node_358,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Slider,id:1813,x:32250,y:32882,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:_Metallic_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8,max:1;n:type:ShaderForge.SFN_Transform,id:8092,x:31830,y:31970,varname:node_8092,prsc:2,tffrom:0,tfto:1|IN-4197-OUT;n:type:ShaderForge.SFN_ViewVector,id:4197,x:31620,y:32072,varname:node_4197,prsc:2;n:type:ShaderForge.SFN_FragmentPosition,id:2312,x:31641,y:32248,varname:node_2312,prsc:2;n:type:ShaderForge.SFN_Transform,id:5524,x:31830,y:32151,varname:node_5524,prsc:2,tffrom:0,tfto:1|IN-2312-XYZ;n:type:ShaderForge.SFN_Code,id:5074,x:32013,y:32050,varname:node_5074,prsc:2,code:CgBmAGwAbwBhAHQAIABNAEEAWABfAFQAUgBBAEMARQBfAEQASQBTAFQAQQBOAEMARQAgAD0AIAA2AC4AMAA7ACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAALwAvACAAbQBhAHgAIAB0AHIAYQBjAGUAIABkAGkAcwB0AGEAbgBjAGUACgBmAGwAbwBhAHQAIABJAE4AVABFAFIAUwBFAEMAVABJAE8ATgBfAFAAUgBFAEMASQBTAEkATwBOACAAPQAgADAALgAwADEAOwAgACAAIAAgACAAIAAgACAALwAvACAAcAByAGUAYwBpAHMAaQBvAG4AIABvAGYAIAB0AGgAZQAgAGkAbgB0AGUAcgBzAGUAYwB0AGkAbwBuAAoAaQBuAHQAIABOAFUATQBfAE8ARgBfAFQAUgBBAEMARQBfAFMAVABFAFAAUwAgAD0AIAAxADAAMAA7AAoACgAKACAAIAAgACAACgAgACAAIAAgAGYAbABvAGEAdAAgAGgAIAA9ACAAIABJAE4AVABFAFIAUwBFAEMAVABJAE8ATgBfAFAAUgBFAEMASQBTAEkATwBOACoAMgAuADAAOwAKACAAIAAgACAAZgBsAG8AYQB0ACAAdAAgAD0AIAAwAC4AMAA7AAoAIAAgACAAIABmAGwAbwBhAHQAIAByAGUAcwAgAD0AIAAtADEALgAwADsACgAgACAAIAAgAGYAbABvAGEAdAAgAGkAZAAgAD0AIAAtADEALgAwADsACgAgACAAIAAgAAoAIAAgACAAIABmAG8AcgAoACAAaQBuAHQAIABpAD0AMAA7ACAAaQA8ACAATgBVAE0AXwBPAEYAXwBUAFIAQQBDAEUAXwBTAFQARQBQAFMAIAA7ACAAaQArACsAIAApAHsACgAgACAAIAAgACAAIAAgACAACgAgACAAIAAgACAAIAAgACAAaQBmACgAIABoACAAPAAgAEkATgBUAEUAUgBTAEUAQwBUAEkATwBOAF8AUABSAEUAQwBJAFMASQBPAE4AIAB8AHwAIAB0ACAAPgAgAE0AQQBYAF8AVABSAEEAQwBFAF8ARABJAFMAVABBAE4AQwBFACAAKQAgAGIAcgBlAGEAawA7AAoAIAAgACAAIAAgACAAIAAgAGYAbABvAGEAdAAyACAAbQAgAD0AIABmAGwAbwBhAHQAMgAoACAAbABlAG4AZwB0AGgAKAAgAHIAbwArAHIAZAAqAHQAIAAgACkAIAAtACAALgA4ACAALAAgADAALgAgACkAOwAgAAoAIAAgACAAIAAgACAAIAAgAGgAIAA9ACAAbQAuAHgAOwAKACAAIAAgACAAIAAgACAAIAB0ACAAKwA9ACAAaAA7AAoAIAAgACAAIAAgACAAIAAgAGkAZAAgAD0AIABtAC4AeQA7AAoAIAAgACAAIAAgACAAIAAgAAoAIAAgACAAIAB9AAoACgAgACAAIAAgAGkAZgAoACAAdAAgADwAIABNAEEAWABfAFQAUgBBAEMARQBfAEQASQBTAFQAQQBOAEMARQAgACkAIAByAGUAcwAgAD0AIAB0ADsACgAgACAAIAAgAGkAZgAoACAAdAAgAD4AIABNAEEAWABfAFQAUgBBAEMARQBfAEQASQBTAFQAQQBOAEMARQAgACkAIABpAGQAIAA9AC0AMQAuADAAOwAKACAAIAAgACAACgAgACAAIAAgAHIAZQB0AHUAcgBuACAAZgBsAG8AYQB0ADIAKAAgAHIAZQBzACAALAAgAGkAZAAgACkAOwAKACAAIAAgACAACgAKAAoA,output:1,fname:Function_node_5074,width:829,height:567,input:2,input:2,input_1_label:rd,input_2_label:ro|A-5382-OUT,B-5524-XYZ;n:type:ShaderForge.SFN_Code,id:4182,x:32878,y:31886,varname:node_4182,prsc:2,code:ZgBsAG8AYQB0ADMAIABjAG8AbAAgAD0AIABmAGwAbwBhAHQAMwAoACAAMAAuACAALAAgADAALgAgACwAIAAwAC4AIAApADsACgAKAGkAZgAoACAAcgBlAHMALgB5ACAAPgAgAC0ALgA1ACAAKQAgAGMAbwBsACAAPQAgAGYAbABvAGEAdAAzACgAIAAxAC4AIAAsACAAMQAuACAALAAgADEALgAgACkAOwAKAAoAcgBlAHQAdQByAG4AIABjAG8AbAA7AA==,output:2,fname:Function_node_4182,width:685,height:160,input:1,input_1_label:res|A-5074-OUT;n:type:ShaderForge.SFN_ObjectPosition,id:7102,x:31505,y:32368,varname:node_7102,prsc:2;n:type:ShaderForge.SFN_Normalize,id:5382,x:32014,y:31899,varname:node_5382,prsc:2|IN-8092-XYZ;proporder:5964-358-1813;pass:END;sub:END;*/

Shader "Shader Forge/sf1" {
    Properties {
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Gloss ("Gloss", Range(0, 1)) = 0.8
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _Metallic;
            uniform float _Gloss;
            float2 Function_node_5074( float3 rd , float3 ro ){
            
            float MAX_TRACE_DISTANCE = 6.0;             // max trace distance
            float INTERSECTION_PRECISION = 0.01;        // precision of the intersection
            int NUM_OF_TRACE_STEPS = 100;
            
            
                
                float h =  INTERSECTION_PRECISION*2.0;
                float t = 0.0;
                float res = -1.0;
                float id = -1.0;
                
                for( int i=0; i< NUM_OF_TRACE_STEPS ; i++ ){
                    
                    if( h < INTERSECTION_PRECISION || t > MAX_TRACE_DISTANCE ) break;
                    float2 m = float2( length( ro+rd*t  ) - .8 , 0. ); 
                    h = m.x;
                    t += h;
                    id = m.y;
                    
                }
            
                if( t < MAX_TRACE_DISTANCE ) res = t;
                if( t > MAX_TRACE_DISTANCE ) id =-1.0;
                
                return float2( res , id );
                
            
            
            
            }
            
            float3 Function_node_4182( float2 res ){
            float3 col = float3( 0. , 0. , 0. );
            
            if( res.y > -.5 ) col = float3( 1. , 1. , 1. );
            
            return col;
            }
            
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
            #endif
            #ifdef DYNAMICLIGHTMAP_ON
                o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
            #endif
            o.normalDir = UnityObjectToWorldNormal(v.normal);
            o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
            o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
            o.posWorld = mul(unity_ObjectToWorld, v.vertex);
            float3 lightColor = _LightColor0.rgb;
            o.pos = UnityObjectToClipPos(v.vertex);
            UNITY_TRANSFER_FOG(o,o.pos);
            TRANSFER_VERTEX_TO_FRAGMENT(o)
            return o;
        }
        float4 frag(VertexOutput i) : COLOR {
            i.normalDir = normalize(i.normalDir);
            float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/// Vectors:
            float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
            float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap)));
            float3 normalLocal = _BumpMap_var.rgb;
            float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
            float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
            float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
            float3 lightColor = _LightColor0.rgb;
            float3 halfDirection = normalize(viewDirection+lightDirection);
// Lighting:
            float attenuation = LIGHT_ATTENUATION(i);
            float3 attenColor = attenuation * _LightColor0.xyz;
            float Pi = 3.141592654;
            float InvPi = 0.31830988618;
///// Gloss:
            float gloss = _Gloss;
            float specPow = exp2( gloss * 10.0+1.0);
/// GI Data:
            UnityLight light;
            #ifdef LIGHTMAP_OFF
                light.color = lightColor;
                light.dir = lightDirection;
                light.ndotl = LambertTerm (normalDirection, light.dir);
            #else
                light.color = half3(0.f, 0.f, 0.f);
                light.ndotl = 0.0f;
                light.dir = half3(0.f, 0.f, 0.f);
            #endif
            UnityGIInput d;
            d.light = light;
            d.worldPos = i.posWorld.xyz;
            d.worldViewDir = viewDirection;
            d.atten = attenuation;
            #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                d.ambient = 0;
                d.lightmapUV = i.ambientOrLightmapUV;
            #else
                d.ambient = i.ambientOrLightmapUV;
            #endif
            d.boxMax[0] = unity_SpecCube0_BoxMax;
            d.boxMin[0] = unity_SpecCube0_BoxMin;
            d.probePosition[0] = unity_SpecCube0_ProbePosition;
            d.probeHDR[0] = unity_SpecCube0_HDR;
            d.boxMax[1] = unity_SpecCube1_BoxMax;
            d.boxMin[1] = unity_SpecCube1_BoxMin;
            d.probePosition[1] = unity_SpecCube1_ProbePosition;
            d.probeHDR[1] = unity_SpecCube1_HDR;
            UnityGI gi = UnityGlobalIllumination (d, 1, gloss, normalDirection);
            lightDirection = gi.light.dir;
            lightColor = gi.light.color;
// Specular:
            float NdotL = max(0, dot( normalDirection, lightDirection ));
            float LdotH = max(0.0,dot(lightDirection, halfDirection));
            float3 diffuseColor = Function_node_4182( Function_node_5074( normalize(mul( unity_WorldToObject, float4(viewDirection,0) ).xyz.rgb) , mul( unity_WorldToObject, float4(i.posWorld.rgb,0) ).xyz.rgb ) ); // Need this for specular when using metallic
            float specularMonochrome;
            float3 specularColor;
            diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, _Metallic, specularColor, specularMonochrome );
            specularMonochrome = 1-specularMonochrome;
            float NdotV = max(0.0,dot( normalDirection, viewDirection ));
            float NdotH = max(0.0,dot( normalDirection, halfDirection ));
            float VdotH = max(0.0,dot( viewDirection, halfDirection ));
            float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
            float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
            float specularPBL = max(0, (NdotL*visTerm*normTerm) * unity_LightGammaCorrectionConsts_PIDiv4 );
            float3 directSpecular = 1 * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
            half grazingTerm = saturate( gloss + specularMonochrome );
            float3 indirectSpecular = (gi.indirect.specular);
            indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
            float3 specular = (directSpecular + indirectSpecular);
/// Diffuse:
            NdotL = max(0.0,dot( normalDirection, lightDirection ));
            half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
            float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
            float3 indirectDiffuse = float3(0,0,0);
            indirectDiffuse += gi.indirect.diffuse;
            float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
// Final Color:
            float3 finalColor = diffuse + specular;
            fixed4 finalRGBA = fixed4(finalColor,1);
            UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
            return finalRGBA;
        }
        ENDCG
    }
    Pass {
        Name "FORWARD_DELTA"
        Tags {
            "LightMode"="ForwardAdd"
        }
        Blend One One
        
        
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #define UNITY_PASS_FORWARDADD
        #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
        #define _GLOSSYENV 1
        #include "UnityCG.cginc"
        #include "AutoLight.cginc"
        #include "Lighting.cginc"
        #include "UnityPBSLighting.cginc"
        #include "UnityStandardBRDF.cginc"
        #pragma multi_compile_fwdadd_fullshadows
        #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
        #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
        #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
        #pragma multi_compile_fog
        #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
        #pragma target 3.0
        uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
        uniform float _Metallic;
        uniform float _Gloss;
        float2 Function_node_5074( float3 rd , float3 ro ){
        
        float MAX_TRACE_DISTANCE = 6.0;             // max trace distance
        float INTERSECTION_PRECISION = 0.01;        // precision of the intersection
        int NUM_OF_TRACE_STEPS = 100;
        
        
            
            float h =  INTERSECTION_PRECISION*2.0;
            float t = 0.0;
            float res = -1.0;
            float id = -1.0;
            
            for( int i=0; i< NUM_OF_TRACE_STEPS ; i++ ){
                
                if( h < INTERSECTION_PRECISION || t > MAX_TRACE_DISTANCE ) break;
                float2 m = float2( length( ro+rd*t  ) - .8 , 0. ); 
                h = m.x;
                t += h;
                id = m.y;
                
            }
        
            if( t < MAX_TRACE_DISTANCE ) res = t;
            if( t > MAX_TRACE_DISTANCE ) id =-1.0;
            
            return float2( res , id );
            
        
        
        
        }
        
        float3 Function_node_4182( float2 res ){
        float3 col = float3( 0. , 0. , 0. );
        
        if( res.y > -.5 ) col = float3( 1. , 1. , 1. );
        
        return col;
        }
        
        struct VertexInput {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 tangent : TANGENT;
            float2 texcoord0 : TEXCOORD0;
            float2 texcoord1 : TEXCOORD1;
            float2 texcoord2 : TEXCOORD2;
        };
        struct VertexOutput {
            float4 pos : SV_POSITION;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;
            float2 uv2 : TEXCOORD2;
            float4 posWorld : TEXCOORD3;
            float3 normalDir : TEXCOORD4;
            float3 tangentDir : TEXCOORD5;
            float3 bitangentDir : TEXCOORD6;
            LIGHTING_COORDS(7,8)
        };
        VertexOutput vert (VertexInput v) {
            VertexOutput o = (VertexOutput)0;
            o.uv0 = v.texcoord0;
            o.uv1 = v.texcoord1;
            o.uv2 = v.texcoord2;
            o.normalDir = UnityObjectToWorldNormal(v.normal);
            o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
            o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
            o.posWorld = mul(unity_ObjectToWorld, v.vertex);
            float3 lightColor = _LightColor0.rgb;
            o.pos = UnityObjectToClipPos(v.vertex);
            TRANSFER_VERTEX_TO_FRAGMENT(o)
            return o;
        }
        float4 frag(VertexOutput i) : COLOR {
            i.normalDir = normalize(i.normalDir);
            float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/// Vectors:
            float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
            float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap)));
            float3 normalLocal = _BumpMap_var.rgb;
            float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
            float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
            float3 lightColor = _LightColor0.rgb;
            float3 halfDirection = normalize(viewDirection+lightDirection);
// Lighting:
            float attenuation = LIGHT_ATTENUATION(i);
            float3 attenColor = attenuation * _LightColor0.xyz;
            float Pi = 3.141592654;
            float InvPi = 0.31830988618;
///// Gloss:
            float gloss = _Gloss;
            float specPow = exp2( gloss * 10.0+1.0);
// Specular:
            float NdotL = max(0, dot( normalDirection, lightDirection ));
            float LdotH = max(0.0,dot(lightDirection, halfDirection));
            float3 diffuseColor = Function_node_4182( Function_node_5074( normalize(mul( unity_WorldToObject, float4(viewDirection,0) ).xyz.rgb) , mul( unity_WorldToObject, float4(i.posWorld.rgb,0) ).xyz.rgb ) ); // Need this for specular when using metallic
            float specularMonochrome;
            float3 specularColor;
            diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, _Metallic, specularColor, specularMonochrome );
            specularMonochrome = 1-specularMonochrome;
            float NdotV = max(0.0,dot( normalDirection, viewDirection ));
            float NdotH = max(0.0,dot( normalDirection, halfDirection ));
            float VdotH = max(0.0,dot( viewDirection, halfDirection ));
            float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
            float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
            float specularPBL = max(0, (NdotL*visTerm*normTerm) * unity_LightGammaCorrectionConsts_PIDiv4 );
            float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
            float3 specular = directSpecular;
/// Diffuse:
            NdotL = max(0.0,dot( normalDirection, lightDirection ));
            half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
            float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
            float3 diffuse = directDiffuse * diffuseColor;
// Final Color:
            float3 finalColor = diffuse + specular;
            return fixed4(finalColor * 1,0);
        }
        ENDCG
    }
    Pass {
        Name "Meta"
        Tags {
            "LightMode"="Meta"
        }
        Cull Off
        
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #define UNITY_PASS_META 1
        #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
        #define _GLOSSYENV 1
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "UnityPBSLighting.cginc"
        #include "UnityStandardBRDF.cginc"
        #include "UnityMetaPass.cginc"
        #pragma fragmentoption ARB_precision_hint_fastest
        #pragma multi_compile_shadowcaster
        #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
        #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
        #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
        #pragma multi_compile_fog
        #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
        #pragma target 3.0
        uniform float _Metallic;
        uniform float _Gloss;
        float2 Function_node_5074( float3 rd , float3 ro ){
        
        float MAX_TRACE_DISTANCE = 6.0;             // max trace distance
        float INTERSECTION_PRECISION = 0.01;        // precision of the intersection
        int NUM_OF_TRACE_STEPS = 100;
        
        
            
            float h =  INTERSECTION_PRECISION*2.0;
            float t = 0.0;
            float res = -1.0;
            float id = -1.0;
            
            for( int i=0; i< NUM_OF_TRACE_STEPS ; i++ ){
                
                if( h < INTERSECTION_PRECISION || t > MAX_TRACE_DISTANCE ) break;
                float2 m = float2( length( ro+rd*t  ) - .8 , 0. ); 
                h = m.x;
                t += h;
                id = m.y;
                
            }
        
            if( t < MAX_TRACE_DISTANCE ) res = t;
            if( t > MAX_TRACE_DISTANCE ) id =-1.0;
            
            return float2( res , id );
            
        
        
        
        }
        
        float3 Function_node_4182( float2 res ){
        float3 col = float3( 0. , 0. , 0. );
        
        if( res.y > -.5 ) col = float3( 1. , 1. , 1. );
        
        return col;
        }
        
        struct VertexInput {
            float4 vertex : POSITION;
            float2 texcoord1 : TEXCOORD1;
            float2 texcoord2 : TEXCOORD2;
        };
        struct VertexOutput {
            float4 pos : SV_POSITION;
            float2 uv1 : TEXCOORD0;
            float2 uv2 : TEXCOORD1;
            float4 posWorld : TEXCOORD2;
        };
        VertexOutput vert (VertexInput v) {
            VertexOutput o = (VertexOutput)0;
            o.uv1 = v.texcoord1;
            o.uv2 = v.texcoord2;
            o.posWorld = mul(unity_ObjectToWorld, v.vertex);
            o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
            return o;
        }
        float4 frag(VertexOutput i) : SV_Target {
/// Vectors:
            float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
            UnityMetaInput o;
            UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
            
            o.Emission = 0;
            
            float3 diffColor = Function_node_4182( Function_node_5074( normalize(mul( unity_WorldToObject, float4(viewDirection,0) ).xyz.rgb) , mul( unity_WorldToObject, float4(i.posWorld.rgb,0) ).xyz.rgb ) );
            float specularMonochrome;
            float3 specColor;
            diffColor = DiffuseAndSpecularFromMetallic( diffColor, _Metallic, specColor, specularMonochrome );
            float roughness = 1.0 - _Gloss;
            o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
            
            return UnityMetaFragment( o );
        }
        ENDCG
    }
}
FallBack "Diffuse"
CustomEditor "ShaderForgeMaterialInspector"
}
