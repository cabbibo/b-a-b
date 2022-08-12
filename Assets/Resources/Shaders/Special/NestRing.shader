Shader "Unlit/NestRing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NumCollectables("NumCollectables", int ) = 10
        _StartAngle("start angel", float ) = 0
        _AngleLength("AngleLength", float ) = 270
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

            
            #include "../Chunks/terrainData.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float height : TEXCOORD2;
                float inside : TEXCOORD3;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _NumCollectables;
            float _StartAngle;
            float _AngleLength;
            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul( unity_ObjectToWorld,  float4(v.vertex.xyz,1)).xyz;

                float a = atan2( v.vertex.x, v.vertex.y);
                if( a > _StartAngle && a < _StartAngle + _AngleLength){
                    o.inside = 1;
                }
                o.inside = (a + 3.14 ) / 6.28;

                float3 hVal = terrainPos( worldPos * float3(1,0,1) );

                float3 fPos; float3 fNor;
                GetTerrainData(worldPos,fPos,fNor);// (hVal+1) * float3(0,1,0);
                if( length(fPos - worldPos) < 20 ){
                    worldPos = fPos;
                    worldPos += float3(0,1,0) * v.vertex.z * length( mul( unity_ObjectToWorld,float4(1,0,0,0)).xyz);
                }

                o.height = v.vertex.z;
                o.vertex =  mul (UNITY_MATRIX_VP, float4(worldPos,1.0f));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f v) : SV_Target
            {
                // sample the texture
                fixed4 col = v.height * 400;///v.inside;;//tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
