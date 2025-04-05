// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/PointerSkyColumnProcShader1" {
  Properties {

    _Size("_Size", Float) = 1
  }


  SubShader{

    Tags { "Queue"="Overlay+1000" "IgnoreProjector"="True" "RenderType"="Transparent+100000" }
    Blend SrcAlpha One
    //	AlphaTest Greater .01
    //ColorMask RGB
    Cull Off 
    ZWrite Off 

    Pass{


      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "Assets/Resources/Shaders/Chunks/hsv.cginc"
      #include "Assets/Resources/Shaders/Chunks/noise.cginc"





      uniform int _Count;
      uniform float _Size;
      uniform float3 _WrenPos;
      uniform float _Fade;


      StructuredBuffer<float3> _PositionBuffer;
      StructuredBuffer<float> _FadeBuffer;
      StructuredBuffer<float> _TypeBuffer;
      StructuredBuffer<float4> _ExtraDataBuffer;

      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
        float4 pos      : SV_POSITION;
        float3 nor      : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
        float3 eye      : TEXCOORD2;
        float2 uv       : TEXCOORD4;
        float id        : TEXCOORD5;
        float value : TEXCOORD6;
        float fade : TEXCOORD7;
        float type : TEXCOORD8;
        float4 extra : TEXCOORD9;

      };


      uniform float4x4 _Transform;
      //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
      //which we transform with the view-projection matrix before passing to the pixel program.
      varyings vert (uint id : SV_VertexID){

        varyings o;

        int base = id / 6;
        int alternate = id %6;

        if( base < _Count * 6){


          float3 center = _PositionBuffer[base];

          float3 left = UNITY_MATRIX_V[0].xyz; // left direciton
          float3 up = float3(0,1,0); // up direction




          float sizeMultiplier = (200 + length(center - _WrenPos)) / _Size;

          float fSize = sizeMultiplier * _Size * .05;


          float3 p1 = center - left * (fSize) ;
          float3 p2 =  center + left * (fSize);
          float3 p3 = center - left * (fSize) + up * (fSize* 40);
          float3 p4 = center + left * (fSize) + up * (fSize* 40);

          /*float3 p1 = center - up *_Size;
          float3 p2 =  pos  - up *_Size;
          float3 p3 = center + up *_Size;
          float3 p4 = pos + up *_Size;*/


          float3 extra = 0;
          float2 uv = 0;

          float value = 0;

          if( alternate == 0 ){ extra = p1; uv = float2(0,0); value = 0; }
          if( alternate == 1 ){ extra = p2; uv = float2(1,0); value = 0; }
          if( alternate == 2 ){ extra = p4; uv = float2(1,1); value = 1;}
          if( alternate == 3 ){ extra = p1; uv = float2(0,0); value = 0;}
          if( alternate == 4 ){ extra = p4; uv = float2(1,1); value = 1;}
          if( alternate == 5 ){ extra = p3; uv = float2(0,1); value = 0;}

          o.worldPos = extra;
          
          
          // mul(_Transform, float4((v.pos) ,1));
          ///o.worldPos +=  extra * _Size;

          o.eye = _WorldSpaceCameraPos - o.worldPos;
          o.nor = normalize(UNITY_MATRIX_V[2].xyz);//v.nor;
          o.uv = uv;
          o.id = base;
          o.fade = _FadeBuffer[base];
          o.type = _TypeBuffer[base];
          o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
          o.extra = _ExtraDataBuffer[base];

          // means we hide just the sky column
          if( o.extra.w == 1 ){
            o.pos = 0;
          }

        }

        return o;

      }


      float sdTriangle(float2 p, float2 a, float2 b, float2 c) {
        // Compute edge floattors
        float2 ab = b - a;
        float2 bc = c - b;
        float2 ca = a - c;

        // Compute floattors from point p to triangle vertices
        float2 pa = p - a;
        float2 pb = p - b;
        float2 pc = p - c;

        // Edge normal directions
        float2 abNormal = float2(-ab.y, ab.x);
        float2 bcNormal = float2(-bc.y, bc.x);
        float2 caNormal = float2(-ca.y, ca.x);

        // Signed distances to the triangle edges
        float d1 = dot(pa, normalize(abNormal));
        float d2 = dot(pb, normalize(bcNormal));
        float d3 = dot(pc, normalize(caNormal));

        // Inside-outside test using cross products
        float inside = max(max(dot(abNormal, pa), dot(bcNormal, pb)), dot(caNormal, pc));

        // Return the signed distance
        return max(max(d1, d2), d3) * (inside < 0.0 ? 1.0 : -1.0);

      }

      float sdEquilateralTriangle( in float2 p, in float r )
      {
        const float k = sqrt(3.0);
        p.x = abs(p.x) - r;
        p.y = p.y + r/k;
        if( p.x+k*p.y>0.0 ) p = float2(p.x-k*p.y,-k*p.x-p.y)/2.0;
        p.x -= clamp( p.x, -2.0*r, 0.0 );
        return -length(p)*sign(p.y);
      }

      

      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

        float3 c1 = hsv(v.uv.x * .1,1,1);
        

        float3 typeCol = hsv(v.type*.1,.5,1);
        float3 fCol = 0;//hsv(v.type*.1,.5,1) * v.fade;

        if( v.uv.y > .05 ){
          //   fCol = float3(0,0,0);
        }


        float n = noise( float3(1*v.uv.x,v.uv.y * 30,_Time.x%20) * 10);

        float baseY = saturate(v.uv.y * 20);

        if( baseY >= 1){
          //fCol = float3(0,0,1);

          for( int i = 0; i < v.extra.x-.001; i++){


            if( abs((v.uv.y) - (abs(v.uv.x-.5) * .02 + (.001+.1*( 1-((float)i/10))))) < .002 + n * .001   ){
              fCol = typeCol * pow(v.fade,.5);
            }
            
          }

          if( abs(v.uv.x-.5) > .2){
            fCol = float3(0,0,0);
          }

          if( abs(v.uv.x-.5) > .18 && abs(v.uv.x-.5) < .2){
            fCol = typeCol * v.fade * (1-v.uv.y);
          }

          }else{

          
          float baseTri = sdEquilateralTriangle(float2(v.uv.x - .5 ,(1-baseY) - .5),.5);

          if( baseTri < 0 - n * .1){
            fCol = typeCol *1  * pow(v.fade,.5);
            }else{
            if( abs(v.uv.x-.5) > .18 && abs(v.uv.x-.5) < .2){
              fCol = typeCol * v.fade * (1-v.uv.y) * baseY*baseY;
            }
          }

        }


        fCol *= n;

        if( v.type > 9){
          fCol *= 10 *sin(_Time.y * 100);
        }

        //fCol = baseTri;
        //fCol = v.uv.y;
        return float4( fCol , length(fCol));


      }

      ENDCG

    }
  }

  Fallback Off


}
