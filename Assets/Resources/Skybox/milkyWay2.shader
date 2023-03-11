// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Skybox/MilkyWay" {
Properties {
    _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        #include "UnityCG.cginc"

        samplerCUBE _Tex;
        half4 _Tex_HDR;
        half4 _Tint;
        half _Exposure;
        float _Rotation;
        float _Fade;

        float3 RotateAroundYInDegrees (float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        struct appdata_t {
            float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        

        sampler2D _FullColorMap;

        v2f vert (appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.vertex.xyz;
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {


            float3 direction = i.texcoord.xyz;

            float upDown = 1;
            
            if( direction.y < 0 ){ upDown = -1; }//if( direction)
          //  direction.y = abs( direction.y);

            half4 tex = texCUBE (_Tex, direction);



            half3 c = DecodeHDR (tex, _Tex_HDR);
            c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
            c *= _Exposure;


            float horizonValue = 1 / (.02 + .4  *abs(direction.y));
            
            c.xyz = pow(length( c.xyz ),2);
            c.xyz *= horizonValue * 10;

           // c.xyz *= float3(.7 + upDown * .5 ,.3 - .3 * upDown,.3 - .3 * upDown) * .3 + .3;// + upDown;


            c.xyz *= tex2D(_FullColorMap,float2((direction.y+1) * .1 + .2,.6)).xyz * .3 + .4;
            c.xyz *= _Exposure;

            c.xyz *= _Fade;

          


            return half4(c, 1);
        }
        ENDCG
    }
}


Fallback Off

}