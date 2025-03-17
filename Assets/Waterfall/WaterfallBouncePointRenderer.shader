Shader "Unlit/WaterfallBouncePointRenderer"
{
    Properties
    {
      _Size("Size", Range(0.01, 10)) = 1
      _MainTex("Texture", 2D) = "white" {}
      _ColorMultiplier("Color Multiplier", Range(0, 3)) = 1
      _SpriteSize("Sprite Size", Range(1, 10)) = 1
      _VelocityMultiplier("Velocity Multiplier", Range(0, 10)) = 1
      _Speed("Speed", Range(0, 10)) = 1
    }
    SubShader
    {
        
    Cull Off 
     Tags { "RenderType"="Transparent" "Queue"="Transparent -1" }
    LOD 100
    //Blend One One // Additive
   // ZWrite Off
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag
        #include "UnityCG.cginc"
        #include "UnityLightingCommon.cginc"
        #include "Assets/Resources/Shaders/Chunks/hash.cginc"




        int _Count;
        int _CountMultiplier;
      float _Size;
      float _SpriteSize;

      float _LastCountFade;
      float _VelocityMultiplier;

      
      #include "Assets/Resources/Shaders/Chunks/SunShadows.cginc"

      float2 rotateUV(float2 uv, float rotation)
{
    float mid = 0.5;
    return float2(
        cos(rotation) * (uv.x - mid) + sin(rotation) * (uv.y - mid) + mid,
        cos(rotation) * (uv.y - mid) - sin(rotation) * (uv.x - mid) + mid
    );
}

float2 rotateUV(float2 uv, float rotation, float2 mid)
{
    return float2(
      cos(rotation) * (uv.x - mid.x) + sin(rotation) * (uv.y - mid.y) + mid.x,
      cos(rotation) * (uv.y - mid.y) - sin(rotation) * (uv.x - mid.x) + mid.y
    );
}

float2 rotateUV(float2 uv, float rotation, float mid)
{
    return float2(
      cos(rotation) * (uv.x - mid) + sin(rotation) * (uv.y - mid) + mid,
      cos(rotation) * (uv.y - mid) - sin(rotation) * (uv.x - mid) + mid
    );
}




      StructuredBuffer<float4> _Points;
      StructuredBuffer<float3> _Vels;

      float _Speed;

        //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor : NORMAL;
            float2 uv : TEXCOORD0;
            float2 uv2 : TEXCOORD2;
            float debug : TEXCOORD1;
            float lastCountFade : TEXCOORD3;
            float3 world : TEXCOORD4;
            float4 screenPos : TEXCOORD5;
      };

        varyings vert (uint id : SV_VertexID){

            varyings o;
            // particle ID in compute buffer
            int base = id / 6;

            // which vert we are in the quad
        int alternate = id %6;

        int fullBase = base / _CountMultiplier;
        int countID = base % _CountMultiplier;

        float offset = hash((float)fullBase + countID);


        float cycleTime = _Speed + offset;

        float timeInCycle = (_Time.y + offset * cycleTime) % cycleTime;
        timeInCycle = timeInCycle / cycleTime;

        float fadeUpAndDown = min( timeInCycle * 2, 2 - timeInCycle*2);

        float3 physicalOffset = float3(
          hash((float)fullBase + countID + 1) * 2 - 1,
          hash((float)fullBase + countID + 2) * 2 - 1,
          hash((float)fullBase + countID + 3) * 2 - 1
        );


        float lastCountFadeMultiplier = 1;

        if( countID == _CountMultiplier - 1 ){
          lastCountFadeMultiplier =  _LastCountFade;
        }
        
            if( fullBase < _Count ){

              o.lastCountFade = lastCountFadeMultiplier;

                float4 p = _Points[fullBase];
                float3 v = _Vels[fullBase];

                float size =  _Size  * p.w * fadeUpAndDown * lastCountFadeMultiplier;
                float offset = hash((float)base);


                // Right and up of camera ( view not project ) matrix
                float3 l = UNITY_MATRIX_V[0].xyz;
                float3 u = UNITY_MATRIX_V[1].xyz;
                float3 f = UNITY_MATRIX_V[2].xyz;

              //  
                float3 basePos = p.xyz  +  f * (size+_Size* p.w)+ physicalOffset * size * .3- float3(0,timeInCycle * timeInCycle,0) *5 
                + _VelocityMultiplier *(v+ 3*physicalOffset) * timeInCycle * timeInCycle* 3;
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
              
               float3 fPos = basePos + extra * size;//*  _VertBuffer[base].debug.y;//saturate(dT * .1);

                
                o.uv = uv;

                  int spriteSize = int(_SpriteSize);
                int whichSprite = floor( timeInCycle * spriteSize * spriteSize);

                float2 spriteUV = float2( whichSprite % spriteSize, floor( whichSprite / spriteSize) ) / spriteSize;

                o.uv2 = spriteUV + uv / spriteSize;
                
                o.pos = mul (UNITY_MATRIX_VP, float4(fPos,1.0f));

                o.world = fPos;
                o.screenPos = ComputeScreenPos(o.pos);


            }

            return o;

        }

        sampler2D _MainTex;
        
      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {      
        float4 col = tex2D(_MainTex, v.uv2);

        float3 tCol = col.xyz;
        
        if( col.a < .1 ){
         //   discard;
        }

        col = length(floor( col  * 4)/4);

       // col.xyz = col.xyz * .5 + .2;

        //col.xyz *= float3(.4,.6,1) * 1;

        col *=  v.lastCountFade;

        col *=   _LightColor0;

        float3 nor = 0;


                // Right and up of camera ( view not project ) matrix
                float3 l = UNITY_MATRIX_V[0].xyz;
                float3 u = UNITY_MATRIX_V[1].xyz;
                float3 f = UNITY_MATRIX_V[2].xyz;

        nor += l * (v.uv.x - .5) * 1;
        nor += u * (v.uv.y - .5)*1;
        nor += 1.5*f * length(tCol.xyz) * (3*length(v.uv-.5)+.3);
        nor = normalize(nor);

        if( length(col.xyz) < .01 ){
          discard;
        }
        col.xyz = nor * .5 + .5;

        float dotMatch =floor(5*saturate( dot(nor, _WorldSpaceLightPos0.xyz) ))/5 + .3;

        col = .9*_LightColor0 * dotMatch;// saturate(floor( 4*dot(nor,_WorldSpaceLightPos0.xyz))/4);
        //col += (1-_LightColor0)  * .2* (1-dotMatch);

        col += saturate((1-300*pow( tCol.z , 2))) * _LightColor0;


        float shadowAttenuation = GetSunShadowsAttenuation_PCF5x5(v.world + nor*10, v.screenPos.z, 0).x;	

        col *= shadowAttenuation  * .7 +.3;
    
        if( length(col.xyz) < .01 ){
         // discard;
        }

        //col = col.a;

       // col = 1;//-  saturate((v.debug-.3) * 5);

          return col;//float4(col.xyz * (v.nor * .5 +.5) * _ColorMultiplier  * _LightColor0.xyz,1 );
      }


      ENDCG

    }

}

}
