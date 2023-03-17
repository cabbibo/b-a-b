Shader "Unlit/RingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
    _Color ("Color", Color) = (1,1,1,1)

    _Active("_Active",float) = 0
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
           

            struct v2f
            {
                
          float4 pos      : SV_POSITION;
          float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float3 localPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            #include "../Chunks/terrainData.cginc"
            #include "../Chunks/noise.cginc"

            float3 _WrenPos;
            sampler2D _FullColorMap;
            sampler2D _AudioMap;

            v2f vert (appdata_full v)
            {
                v2f o;
                
                o.worldPos = mul( unity_ObjectToWorld,  float4(v.vertex.xyz,1)).xyz;
                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));


                o.localPos = mul( unity_WorldToObject,  float4( _WrenPos,1)).xyz;

                o.uv = v.texcoord;//TRANSFORM_TEX(v.texcoord, _MainTex);
                
                return o;
            }

                float4 _Color;
                float _Active;
            fixed4 frag (v2f v) : SV_Target
            {

                float r = length(v.uv - .5) * 2;
                float a = atan2( v.uv.x- .5 , v.uv.y- .5);



            float n = noise( v.worldPos );
            float h = terrainHeight( v.worldPos);

            if( h < 0   ){
               // discard;
            }

                float speed = _Time.y * ( _Active * 3 + 1 );
                // sample the texture
                fixed4 col = (sin( a * 10 + n + speed )+1)/2;//tex2D(_MainTex, v.uv);
              
               // if( r > 1 - n * .1){ discard; }
               // if( r < .95 - .1 * float(_Active) - n * .1 ){ discard; }
                ///col *= (sin( r * 100 + n * 10+ speed)+1)/2;

               // col = saturate(h * .1) * _Color * col.a;
                //col =  _Color * col.a;
                col = 1;

                bool circle =  length((v.uv.xy-.5)- v.localPos.xy) >.1 / (1+abs(v.localPos.z));
                bool rim = length( v.uv -.5) > .47 && length(v.uv-.5) < .5;

                float aVal = ( length( v.uv -.5) - .47) / .03;

                col.xyz = tex2D( _AudioMap , float2(aVal,0)).xyz;


                if( !rim ){
                    discard;
                };

                // apply fog
                //UNITY_APPLY_FOG(v.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
