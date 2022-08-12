// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/projectOnTerrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Offset ("Offset", float) = 0.1
        _Color("Color",Color) = (1,1,1,1)
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 nor : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Offset;

            sampler2D _HeightMap;


            float4 _Color;
            
            float3 terrainPos( float3 pos ){
                float height = tex2Dlod( _HeightMap , float4((pos.xz+5000) / 10000,0,0)).x;// , 1).x * 4000
                return float3( pos.x ,height * 4000 + _Offset, pos.z);
                //return float3( pos.x ,pos.y, pos.z);
            }


float3 terrainNor(float3 pos){
    float3 eps = float3(2,0,0);

    float3 l = terrainPos( pos + eps.xyy);
    float3 r = terrainPos( pos - eps.xyy);
    float3 u = terrainPos( pos + eps.yyx);
    float3 d = terrainPos( pos - eps.yyx);

    return normalize(cross(l-r,u-d));

}
            v2f vert (appdata v)
            {
                v2f o;

                float3 world = mul(unity_ObjectToWorld , v.vertex );
                float3 fPos = terrainPos( world );

                o.nor = terrainNor( world);
                o.vertex = mul( UNITY_MATRIX_VP , float4( fPos, 1));//(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f v) : SV_Target
            {

                float d = length(v.uv - .5) * 2;
                if( d > 1 ){ discard;}

                if( sin( d * 40 + _Time.y ) < .5 + d * .4 ){ discard;}
                // sample the texture
                float3 col = _Color;// v.nor * .5 + .5; 
                return float4(col, 1);
            }
            ENDCG
        }
    }
}
