Shader "Unlit/basicLighting"
{
    Properties
    {
        _AudioTex ("Audio Texture", 2D) = "white" {}
        _MainTex ("Main Texture", 2D) = "white" {}
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

            float _TimeInClip;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 rd : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _AudioTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.ro = mul( UNITY_MATRIX_M , v.vertex).xyz;
                o.rd = _WorldSpaceCameraPos - o.ro;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);


                return o;
            }
float3 hsv(float h, float s, float v)
{
  return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
    h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
}
            fixed4 frag (v2f v) : SV_Target
            {

                fixed4 col = 0;
                int _NumSteps = 30;
                float dist = 1;
                for( int i  = 0;  i < _NumSteps; i++ ){

                    float val = float(i)/float(_NumSteps);
                    float3 pos = v.ro - v.rd * val * dist;

                    float d =  tex2D(_MainTex, (pos.xy * float2(-1,1)) * .1 +.5).a;
                    d =(.5- abs(.5-d)) * 2;

                    float4 aVal =  tex2D(_AudioTex, float2(val * .5 , _TimeInClip + .01));

                    if( d < aVal.x * .1 ){
                    col.xyz +=(val *val+ .6)* 3.2 * hsv( length(aVal) *2  + .5 , 1,1) * length(aVal) * .1;
                    //7break;
                    }


                }

                // sample the texture
                fixed4 aCol = tex2D(_AudioTex, float2(v.uv.x , _TimeInClip+ .01));
                fixed4 tCol = tex2D(_MainTex, v.uv);
                return col;
            }
            ENDCG
        }
    }
}
