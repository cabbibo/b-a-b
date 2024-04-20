Shader "PostProcessing/FogEffect"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    TEXTURE2D_SAMPLER2D(_DepthTex, sampler_DepthTex);
    TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

    float _Intensity;

    float4 Frag(VaryingsDefault i) : SV_Target
    {


        float2 uvR = i.texcoord;
        float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvR);
        float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uvR).r;

        color *= _Intensity;

        

        color.rgb = lerp( color.rgb , float3(1,1,1), saturate(10*pow((LinearEyeDepth(depth)/20000),2)));

        return color;
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex VertDefault
            #pragma fragment Frag

            ENDHLSL
        }
    }
}