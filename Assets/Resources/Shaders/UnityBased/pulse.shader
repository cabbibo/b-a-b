Shader "Wren/pulse"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    _On("On", float )=0
    }
    SubShader
    {
               Tags { "RenderType"="Transparent"  "Queue" ="Transparent"}
        LOD 100

        Blend One One
        Cull Off

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
                float3 world : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.world = mul( unity_ObjectToWorld , v.vertex ).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float _On;

            #include "../Chunks/noise.cginc"
            fixed4 frag (v2f i) : SV_Target
            {

                float n = noise( i.world * 2);
                // sample the texture
                fixed4 col = saturate(sin( i.uv.x * 200 - _Time.y * 10 + float4(0,1,2,3)) + n* 3 * _On);//tex2D(_MainTex, i.uv);

                col *=saturate( (_On - i.uv.x) * 5);
                col -=  abs(i.uv.y -.5)*2* (abs(n)+1);
                col = saturate(col);

                if( length(col) < .4 ){discard;}



                return col;
            }
            ENDCG
        }
    }
}
