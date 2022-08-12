Shader "Debug/simpleHueAdditive"
{
    Properties
    {
        _WhichHue ("Which Hue", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
  
            Blend One One
            ZWrite Off

            Cull Front
    

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float height : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            int _WhichHue;
            float _Hue1;
            float _Hue2;
            float _Hue3;
            float _Hue4;


            #include "../Chunks/hsv.cginc"

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.height = v.vertex.z;
                return o;
            }

            fixed4 frag (v2f v) : SV_Target
            {
                // sample the texture

                float fHue = _Hue1;
                if( _WhichHue == 1 ){ fHue = _Hue2;}
                if( _WhichHue == 2 ){ fHue = _Hue3;}
                if( _WhichHue == 3 ){ fHue = _Hue4;}
                float3 col = hsv(fHue,1,1);
                return float4(col * (1-v.height * 15) ,.001);
            }
            ENDCG
        }
    }
}
