Shader "Debug/RingFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"  "Queue" ="Transparent"}
        LOD 100

        Blend One One
        Cull Off

        //ZWrite off
        Blend One One
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float _AmountFilled;

            fixed4 frag (v2f v) : SV_Target
            {
                // sample the texture
                float4 col = float4(v.uv.x, v.uv.y, 0, 1);  


                if( v.uv.x < _AmountFilled){
                    col = float4(1, 1, 1, 1);
                    }else{
                    col = 10*saturate(sin(v.uv.x * 3.14159 * 60) * float4(1, 1, 1, 1)-.9);
                }


                return col;
            }
            ENDCG
        }
    }
}
