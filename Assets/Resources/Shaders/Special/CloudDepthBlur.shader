Shader "Special/CloudDepthBlur"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        // Pass 0: Initial depth conversion from color texture
        Pass
        {
            Name "DepthConvert"
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
            sampler2D _DepthTexture;
            sampler2D _ColorTexture;
            float4 _MainTex_TexelSize;
            float _BlurSize;
            float _CameraNear;
            float _CameraFar;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Sample the color texture (which has our cloud particles)
                float4 color = tex2D(_ColorTexture, i.uv);
                
                // Use the alpha or intensity as our "depth" for blur purposes
                // The particle color encodes depth info
                float depth = color.a;
                
                // If no particle, output 0
                if (color.a < 0.01)
                {
                    return float4(0, 0, 0, 0);
                }
                
                // Store linear depth in red channel, alpha in green for masking
                return float4(depth, color.a, 0, 1);
            }
            ENDCG
        }
        
        // Pass 1: Gaussian blur pass (separable)
        Pass
        {
            Name "GaussianBlur"
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
            float4 _MainTex_TexelSize;
            float _BlurSize;
            float4 _BlurDirection;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 texelSize = _MainTex_TexelSize.xy * _BlurSize;
                float2 dir = _BlurDirection.xy;
                
                // Gaussian weights for 9-tap filter
                float weights[5] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };
                
                float4 result = tex2D(_MainTex, i.uv) * weights[0];
                float totalWeight = weights[0];
                
                for (int j = 1; j < 5; j++)
                {
                    float2 offset = dir * texelSize * j;
                    
                    float4 sample1 = tex2D(_MainTex, i.uv + offset);
                    float4 sample2 = tex2D(_MainTex, i.uv - offset);
                    
                    result += sample1 * weights[j];
                    result += sample2 * weights[j];
                    totalWeight += weights[j] * 2;
                }
                
                return result / totalWeight;
            }
            ENDCG
        }
    }
    
    FallBack Off
}

