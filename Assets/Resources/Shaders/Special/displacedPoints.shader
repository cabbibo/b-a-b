Shader "Custom/displacedPointGrid"
{
    Properties
    {
        _Size ("Size", Float) = 0.1
        _NoiseOffset ("_NoiseOffset", Float) = 1
        _NoiseSize ("_NoiseSize", Float) = 1
       _MainTex ("Texture", 2D) = "white" {} 
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
                float whichDir : TEXCOORD7;
            };

            float _Size;
            float _GridSize;
            float3 _Center;
            int _Dimensions;
            float _NoiseOffset;
            float _NoiseSize;


            float3 _WrenPos;
            
            float hash( float n ){
        return frac(sin(n)*4358.5453);
      }


            v2f vert (uint vid : SV_VertexID)
            {
                v2f o;

                int vertID = vid/18;
                int whichDir = (vid % 18 ) / 6;
                int alternate = vid % 6;


                float3 up = float3(1,0,0);//normalize( UNITY_MATRIX_VP[1].xyz);

                if( whichDir == 1 ){
                   // up = float3(0,1,0);
                }else if( whichDir == 2 ){

                   // up = float3(0,0,1);
                }



                //float3 left = normalize(cross( UNITY_MATRIX_VP[2].xyz, up ));
                float3 left = normalize(UNITY_MATRIX_VP[0].xyz);
                 up = normalize(UNITY_MATRIX_VP[1].xyz);
                
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

                float3 debugPos = pos;


                pos += curlNoise(pos *_NoiseSize)* _NoiseOffset * _GridSize/float(_Dimensions);



                float3 dirToWren = _WrenPos - pos;


                o.dirToWren = dirToWren;

               o.disform = (10 / (0.1 + .3*length(o.dirToWren)));

                pos -= (10 / (0.1 + .3*length(o.dirToWren))) * normalize(o.dirToWren);

                pos += dirToWren * .3 * float(whichDir) * _Size ;


//                pos = debugPos;

                o.dist = max( max( abs( pos.x-_Center.x) ,   abs(pos.y-_Center.y)), abs(pos.z-_Center.z));//  minRemainder;

                float fSize = (1-(o.dist*2)/_GridSize) * _Size;
                float3 p1 = pos + ( - left - up) * _Size;
                float3 p2 = pos + ( + left - up) * _Size;
                float3 p3 = pos + ( - left + up) * _Size;
                float3 p4 = pos + ( + left + up) * _Size;

                float3 fPos;
                float2 fUV = 0;

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


                fUV *= 1.0/6.0;
                fUV += floor(float2( hash(float(vertID)) , hash(float(vertID+1)) ) * 6)/6.0;// * 1.0/6.0;
                
                o.uv = fUV;
                o.id = float(vertID);
                o.world = fPos;
                o.whichDir = float(whichDir);
               
                o.vertex = mul(UNITY_MATRIX_VP, float4(fPos,1.0f));

                return o;
            
            }

            float3 hsv(float h, float s, float v)
            {
                return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
                h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
            }

            sampler2D _MainTex;

            float _Fade;
            fixed4 frag (v2f v) : SV_Target
            {


                // sample the texture
                fixed3 col = 1-(v.dist*2)/_GridSize;//float4(hsv( v.dist * 2 , 1,1),1-v.dist*2);3
               // if( length( v.uv-.5) > .5 ){discard;}
                col *= _Fade;

                col.r *= saturate( 1-abs(v.uv.x) - 0)  * (1/v.dist);
                col.g *= saturate( 1-abs(v.uv.x) - .2) * 1.2* (1/(1.3*v.dist));;
                col.b *= saturate( 1-abs(v.uv.x) - .4)* 1.4*(1/(1.5*v.dist));


                col = 1;

                col = hsv( v.whichDir/3,1,1);

                col *=  1-tex2D(_MainTex, v.uv).r;

             //  col = 1;

                if( length(col) < .01){
                    discard;
                }
                
                //col.xyz *= normalize( v.dirToWren) * .5 + .5;

                col.xyz *= v.disform * v.disform* .3;

                //col = tex2D(_MainTex,v.uv);
                col *= _Fade;

                return float4(col,1);
            }
            ENDCG
        }
    }
}
