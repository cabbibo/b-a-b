// Assets/Shaders/Hidden_AudioSpectrumToTexture.shader
Shader "Unlit/AudioSpectrumToTexture"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Overlay"
        }
        Pass
        {
            ZTest Always ZWrite Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            sampler2D _SamplesTex; // 1xN RGBAFloat from CPU upload
            sampler2D _PrevTex; // previous output
            float     _Decay;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f Vert( appdata v )
            {
                v2f o;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.uv  = v.uv;
                return o;
            }

            half4 Frag( v2f i ) : SV_Target
            {
                half4 prev = tex2D( _PrevTex , i.uv );
                half4 cur  = tex2D( _SamplesTex , i.uv );

                // scale like your old *128 and decay blend
                half4 outv = prev * _Decay + cur * ( 1.0h - _Decay ) * 128.0h;
                return outv;
            }
            ENDHLSL
        }
    }
}