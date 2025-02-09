Shader "Unlit/TerrainDebugAndPainter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GridSize ("Grid Size", Float) = 1
        _GridCutoff ("Grid Cutoff", Float) = 0.8
        _ElevationSize ("Elevation Size", Float) = 1
        _ElevationCutoff ("Elevation Cutoff", Float) = 0.8
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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 world : TEXCOORD1;
                float3 nor : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.nor = v.normal;
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }


            float _GridSize;
            float _GridCutoff;

            float _ElevationSize;
            float _ElevationCutoff;
            fixed4 frag (v2f v) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, v.uv * 100) * .3 + .7;

                col.xyz *= v.nor * .5 + .5;

                float grid =max(sin( v.world.x * _GridSize), sin(v.world.z * _GridSize));
                if( grid > _GridCutoff)
                {
                    grid = 1;// fixed4(1,0,0,1);
                    }else{
                    grid = 0;
                }


                float elevation = sin(v.world.y * _ElevationSize);
                if( elevation > _ElevationCutoff)
                {
                    elevation = 1;// fixed4(1,0,0,1);
                    }else{
                    elevation = 0;
                }
                
                //col.x = max(sin( v.world.x * _GridSize), sin(v.world.z * _GridSize));
                // col.y = sin( v.world.y);

                col *= float4(1,0,0,1)*elevation+.5;
                col *= float4(0,0,1,1)*grid+.5;

                col *= 2;
                

                return col;
            }
            ENDCG
        }
    }
}
