// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Map/mapTerrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                float3 nor : NORMAL;
                float edge : TEXCOORD2;
                float height :TEXCOORD3;
            };

            sampler2D _Terrain;
            float2 _Center;

            float2 _MapSize;
            float _MapDepth;

        


            float3 terrainPos( float3 p ){
                float2 uv = _Center + float2(-1 , 1) * p.xy * _MapSize;
                float h  = tex2Dlod( _Terrain , float4(uv,0,0) );
                float3 pos = p;
                pos.z += h * _MapDepth;
                return pos;
            }

            
            void TerrainData( float3 vert  , out float3 pos , out float3 nor , out float h ){


                pos = terrainPos( vert );

                h = pos.z - vert.z;
                


                float eps = .01;
                float3 p1 = terrainPos( vert + float3(eps,0,0));
                float3 p2 = terrainPos( vert - float3(eps,0,0));
                float3 p3 = terrainPos( vert + float3(0,eps,0));
                float3 p4 = terrainPos( vert - float3(0,eps,0));

            
                nor = normalize(cross(p1-p2, p3-p4));//float3(0,1,0);

            }

            v2f vert (appdata v)
            {
                v2f o;



                float3 pos;
                float3 nor;
                float height;
                TerrainData( v.vertex , pos , nor , height );

                o.edge = 0;
                if( pos.x == .5 || pos.x == -.5 || pos.y == -.5 || pos.y == .5){
                    pos.z = 0;
                    o.edge = 1;
                }

                o.vertex = UnityObjectToClipPos(float4(pos,1));
                o.uv = v.uv;
                o.height = height;
                o.nor = nor;// mul(unity_ObjectToWorld,float4(nor,0)).xyz;

                return o;
            }

            fixed4 frag (v2f v) : SV_Target
            {
                // sample the texture
                float3 col =  0;
    
            float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
            float fwd = dot( normalize(viewDir) ,v.nor);
                col += fwd * 1;

                col *=  1-clamp( (sin( v.height * 500) - .9) * 10 , 0 , 1);;

                if( v.edge > 0.0001){ col = 1;}
                return float4(col,1);
            }
            ENDCG
        }
    }
}
