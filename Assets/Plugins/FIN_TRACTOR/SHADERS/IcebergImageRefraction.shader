Shader "Unlit/IcebergImageRefraction"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RefractionDepth("Refraction Depth", Range(0, 100)) = 1
        _RefractionBase("Refraction Base", Range(0, 1)) = .8
        _RefractionSpread("Refraction Spread", Range(0, 1)) = .1
        _FresnelMultiplier("Fresnel Multiplier", Range(0, 100)) = 1
        _FresnelPower("Fresnel Power", Range(0, 100)) = 1
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct varyings
            {
                float3 world : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 eye : TEXCOORD2;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RefractionDepth;
            float _RefractionBase;
            float _RefractionSpread;

            float _FresnelMultiplier;
            float _FresnelPower;

            varyings vert (appdata v)
            {
                varyings o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz);
                o.eye = normalize(_WorldSpaceCameraPos - o.world);

                o.screenPos = ComputeScreenPos(o.vertex);


                return o;
            }


            float2 getScreenPos( float3 pos , float3 eye, float3 nor, float depth, float refraction){
                float3 refr = refract(-eye, nor, refraction);
                float3 newPos = pos + refr * depth;

                
                float4 screenPos = ComputeScreenPos(mul(UNITY_MATRIX_VP,float4(newPos,1))) ;
                float2 fSP = (screenPos/ screenPos.w) *float2(  _ScreenParams.x/_ScreenParams.y, 1);
                return fSP;
            }
            fixed4 frag (varyings v) : SV_Target
            {


                // sample the texture
                fixed4 col = 1;
                col.r = tex2D(_MainTex, getScreenPos(v.world, v.eye,v.normal, _RefractionDepth, _RefractionBase - _RefractionSpread * 0)).r;
                col.g = tex2D(_MainTex, getScreenPos(v.world, v.eye,v.normal, _RefractionDepth, _RefractionBase - _RefractionSpread * 1)).g;
                col.b = tex2D(_MainTex, getScreenPos(v.world, v.eye,v.normal, _RefractionDepth, _RefractionBase - _RefractionSpread * 2)).b;
                //col *= .5;
                col += pow(clamp(1-dot( v.normal, normalize(v.eye)), 0, .7),_FresnelPower) *_FresnelMultiplier ;
                return col;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
