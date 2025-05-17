Shader "VertexFragment/AstigmaBlur"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex VertMain
            #pragma fragment FragMain

            #include "UnityCG.cginc"
            #include "Assets/Resources/Shaders/Chunks/BibPit.cginc"

            sampler2D _MainTex;
            float4    _MainTex_TexelSize;
            sampler2D _CameraDepthTexture;
            sampler2D _CameraGBufferTexture2;
            sampler2D _OcclusionDepthMap;


            int       _UseTexture;
            sampler2D _BokehTex;

            float2 blurAngle;
            float2 blurAngle2;

            struct VertData
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct FragData
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            FragData VertMain( VertData input )
            {
                FragData output;

                output.vertex   = float4( input.vertex.xy , 0.0 , 1.0 );
                output.texcoord = ( input.vertex.xy + 1.0 ) * 0.5;

                // For Direct3D Build
                output.texcoord.y = 1.0 - output.texcoord.y;

                // For Open/WebGL build
                //output.texcoord.y = output.texcoord.y;

                return output;

            }

            float _Intensity;
            float _Scale;
            float _Cutoff;
            float _AspectRatio;
            float _NumSamples;
            float _NumDirections;
            float _Angle;

            sampler2D _Half;
            sampler2D _Quarter;
            sampler2D _Eighth;
            sampler2D _Full;

            // float4 _ScreenParams;


            float4 FragMain( FragData input ) : SV_Target
            {
                float3 color = 0;
                color        = tex2D( _MainTex , input.texcoord ).rgb;
                color        = saturate( color );
                color        = 0;

                color += tex2D( _Half , input.texcoord ).rgb * 1.5;
                color += tex2D( _Quarter , input.texcoord ).rgb * .25;
                color += tex2D( _Eighth , input.texcoord ).rgb * .125;
                color += tex2D( _Full , input.texcoord ).rgb * .5;

                color *= _Intensity;



                color += tex2D( _MainTex , input.texcoord ).rgb;



                // color = tex2D( _Full , input.texcoord ).rgb;

                return float4( color , 1.0 );
            }
            ENDCG
        }
    }
}