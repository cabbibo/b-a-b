Shader "Unlit/SoccerWall"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GridSize("Grid size",float) = 1
        _GridCutoff("Grid cutoff", float) = .9
        _BallDistMultiplier("Ball Distance Multiplier" , float ) = 30
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Cull Off
        // inside Pass
ZWrite Off
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            float _BallLastHitTime;

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
                float3 world : TEXCOORD2;
                float3 scaled : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4x4 _FieldWorldToLocal;
            float3 _BallPosition;

            float _GridCutoff;
            float _GridSize;
            float _BallDistMultiplier;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.world = mul( unity_ObjectToWorld,v.vertex).xyz;
                o.scaled = mul( _FieldWorldToLocal, float4(o.world,1)).xyz;




                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


            fixed4 frag (v2f v) : SV_Target
            {
                // sample the texture
                fixed4 col;// = tex2D(_MainTex, i.uv);


                float gridVal = max(max( sin( v.scaled.x * _GridSize ) , sin( v.scaled.y* _GridSize) ),sin(v.scaled.z* _GridSize));
                col = gridVal;


                float dist  = length(_BallPosition - v.world);
                
                col = max( col , (sin(dist) * _BallDistMultiplier ) / dist);

                col = saturate(col);


                if( col.x < _GridCutoff ){
                    discard;
                }

                
                col *= .3;

                // apply fog
                UNITY_APPLY_FOG(v.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
