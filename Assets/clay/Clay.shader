Shader "Clay/Gradient"
{
    Properties
    {
        _ColorMap ("ColorMap", 2D) = "white" {}
        _CenterOffset( "_CenterOffset" , Vector  ) = (0,0,0,0)
        _GradientStart("_GradientStart" , float ) = 0
        _GradientEnd("_GradientEnd" , float ) = 1
        _GradientChangeSize("_GradientChangeSize" , float ) = 1
        _GradientChangeSpeed("_GradientChangeSpeed" , float ) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 centerCoords : TEXCOORD2;
            };

            sampler2D _ColorMap;

            float2 _CenterOffset;
            float _GradientChangeSpeed;
            float _GradientChangeSize;
            float _GradientStart;
            float _GradientEnd;

            v2f vert (appdata v)
            {
                v2f o;
                o.centerCoords = v.vertex.xy;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float value = length(i.centerCoords.xy-_CenterOffset) / (2*pow( 2, .5));

                float gradient = _GradientStart + value * (_GradientEnd - _GradientStart);
                // sample the texture
                fixed4 col = tex2D(_ColorMap, float2(gradient + _GradientChangeSize * sin(_Time.x * _GradientChangeSpeed) ,0));

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
