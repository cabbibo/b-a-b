Shader "Special/CloudComposite"
{
    Properties
    {
        _MainTex ("Scene Texture", 2D) = "white" {}
        _CloudDepthTexture ("Cloud Depth", 2D) = "black" {}
        _CloudColorTexture ("Cloud Color", 2D) = "black" {}
        _NormalStrength ("Normal Strength", Range(0.1, 10)) = 2.0
        _CloudColor ("Cloud Tint", Color) = (1, 1, 1, 1)
        _CloudOpacity ("Cloud Opacity", Range(0, 1)) = 0.8
        _LightDir ("Light Direction", Vector) = (0.5, 1, 0.5, 0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _CloudDepthTexture;
            sampler2D _CloudColorTexture;
            float4 _MainTex_TexelSize;
            float4 _CloudDepthTexture_TexelSize;
            
            float _NormalStrength;
            float4 _CloudColor;
            float _CloudOpacity;
            float4 _LightDir;
            float _CameraNear;
            float _CameraFar;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Calculate normal from depth using Sobel operator
            float3 CalculateNormalFromDepth(float2 uv, float2 texelSize)
            {
                // Sample neighboring depths
                float depthL = tex2D(_CloudDepthTexture, uv + float2(-texelSize.x, 0)).r;
                float depthR = tex2D(_CloudDepthTexture, uv + float2( texelSize.x, 0)).r;
                float depthU = tex2D(_CloudDepthTexture, uv + float2(0,  texelSize.y)).r;
                float depthD = tex2D(_CloudDepthTexture, uv + float2(0, -texelSize.y)).r;
                
                // Calculate gradients
                float dX = (depthR - depthL) * _NormalStrength;
                float dY = (depthU - depthD) * _NormalStrength;
                
                // Construct normal
                float3 normal = normalize(float3(-dX, -dY, 1.0));
                
                return normal;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Sample the scene
                float4 sceneColor = tex2D(_MainTex, i.uv);
                
                // Sample cloud depth/mask
                float4 cloudData = tex2D(_CloudDepthTexture, i.uv);
                float cloudDepth = cloudData.r;
                float cloudMask = cloudData.g;
                
                // Sample cloud color
                float4 cloudColor = tex2D(_CloudColorTexture, i.uv);
                
                // If no cloud, just return scene
                if (cloudMask < 0.01)
                {
                    return sceneColor;
                }
                
                // Calculate normal from blurred depth
                float2 texelSize = _CloudDepthTexture_TexelSize.xy;
                float3 normal = CalculateNormalFromDepth(i.uv, texelSize);
                
                // Convert normal to viewable color (0-1 range)
                float3 normalColor = normal * 0.5 + 0.5;
                
                // Simple diffuse lighting using normal
                float3 lightDir = normalize(_LightDir.xyz);
                float ndotl = saturate(dot(normal, lightDir));
                
                // Add some ambient
                float3 ambient = float3(0.3, 0.35, 0.4);
                float3 diffuse = float3(1.0, 0.95, 0.9) * ndotl;
                
                // Combine lighting with cloud color
                float3 litCloud = _CloudColor.rgb * (ambient + diffuse);
                
                // For now, just show the normal map as requested
                // You can switch between these modes:
                
                // Option 1: Normal map visualization
                float3 cloudResult = normalColor;
                
                // Option 2: Lit cloud (uncomment to use)
                // float3 cloudResult = litCloud;
                
                // Option 3: Blend both
                // float3 cloudResult = lerp(normalColor, litCloud, 0.5);
                
                // Composite onto scene
                float finalAlpha = cloudMask * _CloudOpacity * cloudColor.a;
                float3 finalColor = lerp(sceneColor.rgb, cloudResult, finalAlpha);
                
                return float4(finalColor, 1.0);
            }
            ENDCG
        }
    }
    
    FallBack Off
}

