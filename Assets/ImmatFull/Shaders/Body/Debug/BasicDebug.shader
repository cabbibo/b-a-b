Shader "Unlit/BasicDebug"
{
    Properties
    {
      _Size("Size", Range(0.1, 1000)) = 1
      _MainTex("Texture", 2D) = "white" {}
      _ColorMultiplier("Color Multiplier", Range(0, 3)) = 1
    }
    SubShader
    {
        
    Cull Off
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag
        #include "UnityCG.cginc"

            struct Vert{
        float3 pos;
        float3 oPos;
        float3 nor;
        float3 tangent;
        float2 uv;
        float life;
        float debug;
        };



        int _Count;
      float _Size;
      float _ColorMultiplier;

      StructuredBuffer<Vert> _VertBuffer;

        //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor : NORMAL;
            float2 uv : TEXCOORD0;
            float debug : TEXCOORD1;
      };

        varyings vert (uint id : SV_VertexID){

            varyings o;
            // particle ID in compute buffer
            int base = id / 6;

            // which vert we are in the quad
        int alternate = id %6;


        
            if( base < _Count ){

                Vert vert = _VertBuffer[base];


                float3 l = normalize(cross( vert.nor, float3(0,1,0)));
                float3 u = normalize(cross( l, vert.nor));

                // Right and up of camera ( view not project ) matrix
                //float3 l = UNITY_MATRIX_V[0].xyz;
                //float3 u = UNITY_MATRIX_V[1].xyz;

                float3 basePos = vert.pos;
                float3 extra = 0;

                float3 p1 = -l -u;
                float3 p2 = l -u;
                float3 p3 = -l +u;
                float3 p4 = l +u;

                float2 uv = 0;


                if( alternate == 0 ){
                    extra = p1;
                    uv = float2(0,0);
                }else if( alternate == 1 ){
                    extra = p2;
                    
                    uv = float2(1,0);
                }else if( alternate == 2 ){
                    extra = p4;
                    
                    uv = float2(1,1);
                }else if( alternate == 3 ){
                    extra = p1;
                    uv = float2(0,0);
                }else if( alternate == 4 ){
                    extra = p4;
                    uv = float2(1,1);
                }else if( alternate == 5 ){
                    extra = p3;
                    uv = float2(0,1);
                }

                float3 eye = _WorldSpaceCameraPos - basePos;
                float3 eyeDir = normalize(eye);
                float m = dot( eyeDir, vert.nor);
                
                float lightMatch = dot( vert.nor, _WorldSpaceLightPos0.xyz );
            

                float dT = min( (1-vert.life) * 10 , vert.life );

                float3 fPos = basePos + extra * _Size * dT * pow( (1-m),2)  * (length(eye) + 30 ) * .003+ vert.nor * 1.1;//*  _VertBuffer[base].debug.y;//saturate(dT * .1);

                
                o.nor = vert.nor;
                o.uv = uv;
                
                o.debug = vert.debug;
                o.pos = mul (UNITY_MATRIX_VP, float4(fPos,1.0f));


            }

            return o;

        }

        sampler2D _MainTex;
        
      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {      
        float4 col = tex2D(_MainTex, v.uv);
        
        if( col.x < .5 ){
            discard;
        }


        float lightMatch = dot( v.nor, _WorldSpaceLightPos0.xyz );

       // col.xyz = lightMatch;

        col = 1-  saturate((v.debug-.3) * 5);
          return float4(col.xyz * (v.nor.xyz * .5 + .5) * _ColorMultiplier,1 );
      }


      ENDCG

    }

}

}
