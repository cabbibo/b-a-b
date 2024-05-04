Shader "PostProcessing/MainPost"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    float _Hue;
    float _Saturation;
    float _Lightness;
    float _Blend;
    float _Fade;

    float3 hsv(float h, float s, float v)
    {
        return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
        h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
    }


    float4 Frag(VaryingsDefault i) : SV_Target
    {

        float4  color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

        float3 lensCol = hsv(_Hue,_Saturation,_Lightness);

        color.rgb = lerp( color.rgb,lensCol * color.rgb,_Blend * .8);//lerp( color.rgb , lensCol , _Blend);


        // color.rgb *= (1-_Fade);

        color.rgb = lerp( color.rgb, float3(0,0,0), pow(_Fade,.5));

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