Shader "Unlit/MagicThreads"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Amount("Amount",float) = 1
        _RepeatSize("_RepeatSize",Vector) =  (1,1,1,1)
        _FlowSpeed("_FlowSpeed",Vector) =  (1,1,1,1)
        _Color( "Color", Color) = (1,1,1,1)

        _WhichDirection("Which Direciton" , int ) = 0

        _Hue("Hue",float) = 1
        _Saturation("Saturation",float) = 1
        _Lightness("Lightness",float) = 1

        
    }
    SubShader
    {// inside SubShader
Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

        LOD 100

        Pass
        {


            
Cull Off
// inside Pass
ZWrite Off
Blend One One // Additive
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Amount;
            float4 _Color;
            float4 _FlowSpeed;

            int _WhichDirection;

            float _Hue;
            float _Saturation;
            float _Lightness;

            float2 _RepeatSize;

            v2f vert (appdata v)
            {
                v2f o;
                float3 fPos = v.vertex.xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;// TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = v.uv2;
                return o;
            }


            #include "../Chunks/hsv.cginc"

            fixed4 frag (v2f i) : SV_Target
            {


                float speedDir = 1;
                
                if(_WhichDirection > 0){
                    speedDir = -1;
                }
                
                fixed4 col = tex2D(_MainTex, i.uv * _RepeatSize + float2(3124.4141,0)*i.uv2.y + _FlowSpeed * speedDir * _Time.x);


                float val = 1-i.uv.x+ col.x * .05;
                
                if(_WhichDirection > 0){
                    val = i.uv.x- col.x * .05; 
                }

              
                if( val > _Amount * 1.1 - .05 ){
                    discard;
                }


                
                //col *= col * col * col * col  * 3;
                col.xyz *= hsv(_Hue,_Saturation,_Lightness);//_Color;

                col *= clamp( pow( i.uv.x ,2) * 3000 , 0 , 1);
                col *= clamp( pow( 1-i.uv.x ,2) * 3000 , 0 , 1);

                //col = 1;

                if( length( col ) < .2 ){
                    discard;
                }
                // apply fog
                return col;
            }
            ENDCG
        }
    }
}
