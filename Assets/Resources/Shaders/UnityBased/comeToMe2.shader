Shader "Unlit/ComeToMe"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
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
            };

            sampler2D _MainTex;
            float3 _Color;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            #include "../chunks/hsv.cginc"
            #include "../chunks/noise.cginc"

            fixed4 frag (v2f i) : SV_Target
            {

                float l = length(i.uv-.5);

                l += noise( float3(i.uv * 10,_Time.x*2) ) * .1;
                float4 col = 1;
                
                if( l > .5 ){
                   discard;
                }

                if( l < .2 ){
                    discard;
                }

                float inOuterRing = saturate((l - .4) * 10);

                inOuterRing *= inOuterRing;
                inOuterRing *= 10;

                

                float ring = floor( inOuterRing );
                inOuterRing %= 1;
                if( inOuterRing < .3 ){ discard;}

                float lightness = .5;
                float whichRing =9-(floor( ((_Time.y * .5) * 10) % 10));
                
                
                if( ring == whichRing ){
                    lightness = 1;
                }

                col.xyz = hsv( l - .4 , 1,lightness );


                return col;
            }
            ENDCG
        }
    }
}
