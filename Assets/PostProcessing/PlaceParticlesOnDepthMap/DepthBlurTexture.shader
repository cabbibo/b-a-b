Shader "Clouds/DepthBlurTexture"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite Off
            Cull Off
            ZTest Always
            Fog { Mode Off }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            v2f vert (appdata_t v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            

            sampler2D _DepthTexture;


            fixed4 frag (v2f i,out float outDepth : SV_Depth) : SV_Target {
                // Sample the texture
               // fixed4 col = tex2D(_MainTex, i.uv);

                float depth = SAMPLE_DEPTH_TEXTURE(_DepthTexture, i.uv);
                outDepth = depth;

                float4 color = tex2D(_MainTex, i.uv);

                // Ignore original color and return red
                return 0;//fixed4(1.0, 0.0, 0.0, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}