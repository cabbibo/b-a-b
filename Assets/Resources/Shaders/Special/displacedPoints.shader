﻿Shader "Custom/displacedPointGrid"
{
    Properties
    {
        _Size ("Size", Float) = 0.1
        _NoiseOffset ("_NoiseOffset", Float) = 1
        _NoiseSize ("_NoiseSize", Float) = 1
        _Fade ("_Fade", Float) = 1
    }
    SubShader
    {
       Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
   
        
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
            #include "../Chunks/snoise.cginc"
            #include "../Chunks/curlNoise.cginc"


    StructuredBuffer<float3> _PointBuffer;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 debug : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 world : TEXCOORD3;
                float id : TEXCOORD2;
                float dist : TEXCOORD4;
                float3 dirToWren : TEXCOORD5;
                float disform : TEXCOORD6;
            };

            float _Size;
            float _GridSize;
            float3 _Center;
            int _Dimensions;
            float _NoiseOffset;
            float _NoiseSize;


            float3 _WrenPos;

            v2f vert (uint vid : SV_VertexID)
            {
                v2f o;

                int vertID = vid/18;
                int whichDir = (vid % 18 ) / 6;
                int alternate = vid % 6;


                float3 up = float3(1,0,0);//normalize( UNITY_MATRIX_VP[1].xyz);

                if( whichDir == 1 ){
                    up = float3(0,1,0);
                }else if( whichDir == 2 ){

                    up = float3(0,0,1);
                }



                float3 left = normalize(cross( UNITY_MATRIX_VP[2].xyz, up ));
                float3 v = _PointBuffer[vertID];

                float3 pos = v;//v + _Center;

        
                

                float sized = v.x / _GridSize;
                float remainder =v.x / _GridSize - floor(v.x / _GridSize);

                float minRemainder = abs( remainder - .5 );
                pos.x =  remainder * _GridSize + floor(_Center.x / (_GridSize/float(_Dimensions))  ) * (_GridSize/float(_Dimensions)) - .5 * _GridSize;

                remainder =v.y / _GridSize - floor(v.y / _GridSize);
                
                minRemainder = max( minRemainder , abs(remainder-.5));
                pos.y =  remainder * _GridSize + floor(_Center.y / (_GridSize/float(_Dimensions))  ) * (_GridSize/float(_Dimensions)) - .5 * _GridSize;


                remainder =v.z / _GridSize - floor(v.z / _GridSize);
                minRemainder = max( minRemainder , abs(remainder-.5));
                pos.z =  remainder * _GridSize + floor(_Center.z / (_GridSize/float(_Dimensions))  ) * (_GridSize/float(_Dimensions)) - .5 * _GridSize;


                /*if( pos.x > _GridSize * .5 ){
                    pos.x 
                }
                pos.x = pos.x % (_GridSize);
                pos.y = pos.y % (_GridSize);
                pos.z = pos.z % (_GridSize);

                pos += _Center;*/


                pos += curlNoise(pos *_NoiseSize)* _NoiseOffset * _GridSize/float(_Dimensions);



                float3 dirToWren = _WrenPos - pos;


                o.dirToWren = dirToWren;

               o.disform = (10 / (0.1 + .3*length(o.dirToWren)));

                pos -= (10 / (0.1 + .3*length(o.dirToWren))) * normalize(o.dirToWren);



                o.dist = max( max( abs( pos.x-_Center.x) ,   abs(pos.y-_Center.y)), abs(pos.z-_Center.z));//  minRemainder;

                float fSize = (1-(o.dist*2)/_GridSize) * _Size;
                float3 p1 = pos + ( - left ) * _Size;
                float3 p2 = pos + ( + left ) * _Size;
                float3 p3 = pos + ( - left - up * 10) * _Size;
                float3 p4 = pos + ( + left + up * 10) * _Size;

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
                    fPos = p3;
                    fUV = float2(1,1);
                }else{
                    fPos = p2;
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

            float _Fade;
            fixed4 frag (v2f v) : SV_Target
            {


                // sample the texture
                fixed4 col = 1-(v.dist*2)/_GridSize;//float4(hsv( v.dist * 2 , 1,1),1-v.dist*2);
               // if( length( v.uv-.5) > .5 ){discard;}
                col *= _Fade;

                col.xyz *= normalize( v.dirToWren) * .5 + .5;

                col.xyz *= v.disform * v.disform * v.disform * .1;
                return col;
            }
            ENDCG
        }
    }
}
