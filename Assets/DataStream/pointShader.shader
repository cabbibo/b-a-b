Shader "Custom/HueBasic"
{
    Properties
    {
        _Size ("Size", Float) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Geometry"  }
        LOD 100

        Cull Off

        ZWrite Off
        Blend One One // Additive

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma require compute

            #include "UnityCG.cginc"


    StructuredBuffer<float3> _PointBuffer;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 debug : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 world : TEXCOORD3;
                float id : TEXCOORD2;
            };

            float _Size;

            v2f vert (uint vid : SV_VertexID)
            {
                v2f o;

                int vertID = vid/6;
                int alternate = vid % 6;


                float3 left = normalize( UNITY_MATRIX_VP[0].xyz);
                float3 up = normalize( UNITY_MATRIX_VP[1].xyz);

                float3 v = _PointBuffer[vertID];

                float3 p1 = v + ( - left - up * 5 ) * _Size;
                float3 p2 = v + ( + left - up * 5 ) * _Size;
                float3 p3 = v + ( - left + up * 5 ) * _Size;
                float3 p4 = v + ( + left + up * 5 ) * _Size;

                float3 fPos;
                float2 fUV;

                 if( alternate == 0 ){
                    fPos = p1;
                    fUV = float2(0,0);
                }else if( alternate == 1){
                    fPos = p2;
                    fUV = float2(1,0);
                }else if( alternate == 2){
                    fPos = p4;
                    fUV = float2(1,1);
                }else if( alternate == 3){
                    fPos = p1;
                    fUV = float2(0,0);
                }else if( alternate == 4){
                    fPos = p4;
                    fUV = float2(1,1);
                }else{
                    fPos = p3;
                    fUV = float2(0,1);
                }

                
                o.uv = fUV;
                o.id = float(vertID);
                o.world = fPos;

                o.vertex = mul(UNITY_MATRIX_VP, float4(fPos,1.0f));

                return o;
            
            }

            float3 hsv(float h, float s, float v)
            {
                return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
                h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
            }


            fixed4 frag (v2f v) : SV_Target
            {


                // sample the texture
                fixed4 col = .3;
                if( length( v.uv-.5) > .5 ){discard;}
             
                return col;
            }
            ENDCG
        }
    }
}
