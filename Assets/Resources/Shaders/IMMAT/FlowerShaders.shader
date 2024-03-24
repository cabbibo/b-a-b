Shader "IMMAT/FlowerSpawn"
{
        
    Properties {

        _FadeFromGroundColorMultiplier("FadeFromGroundColorMultiplier", float) = 10
        
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
            #pragma target 4.5
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"


              struct Vert{
      float3 pos;
      float3 vel;
      float3 nor;
      float3 tan;
      float2 uv;
      float2 debug;
    };





            struct v2f { 
                float4 pos : SV_POSITION; 
                float3 nor : NORMAL;
                float3 world : TEXCOORD3;
                float3 eye : TEXCOORD4;
                float2 uv :TEXCOORD6;
                float3 col : TEXCOORD10;
                float3 worldCol : TEXCOORD11;
                float lerpVal : TEXCOORD12;
            };
            float4 _Color;

            StructuredBuffer<Vert> _VertBuffer;
            StructuredBuffer<Vert> _BaseVertBuffer;
            StructuredBuffer<int> _TriBuffer;

            int _NumVertsPerMesh;
            float _FadeFromGroundColorMultiplier;
            v2f vert ( uint vid : SV_VertexID )
            {
                v2f o;
                Vert v = _VertBuffer[_TriBuffer[vid]];
                int whichMesh = _TriBuffer[vid]/_NumVertsPerMesh;
                Vert b = _BaseVertBuffer[_TriBuffer[vid]%_NumVertsPerMesh];

                float2 debug = v.debug;
                 o.nor = v.nor;
                 o.col = b.vel;
                 o.worldCol = v.tan;
                o.world = v.pos;
                o.lerpVal = b.pos.y * _FadeFromGroundColorMultiplier;

                o.pos = mul (UNITY_MATRIX_VP, float4(o.world,1.0f));
               o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f v) : SV_Target
            {

              
                float3 nor = -cross(normalize(ddx(v.world)),normalize(ddy(v.world)));

                float m = dot( nor, v.eye);
                // sample the texture
                float3 col = 0;

                col = v.col;
                col = lerp( v.worldCol,v.col,v.lerpVal ); ;
                return float4(col,1);
            }

            ENDCG
        }

    }

    
        Fallback "Diffuse"
}