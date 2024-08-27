
Shader "IMMAT/Debug/SmallWindParticles" {
  Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _Size ("Size", float) = .01
    _MainTex ("Texture", 2D) = "white" { }
  }


  SubShader{
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    Cull Off
    Blend One One
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "../../Chunks/Struct16.cginc"

      



      uniform int _Count;
      uniform float _Size;
      uniform float3 _Color;

      
      StructuredBuffer<Vert> _VertBuffer;


      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
        float4 pos      : SV_POSITION;
        float3 nor      : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
        float3 eye      : TEXCOORD2;
        float2 debug    : TEXCOORD3;
        float2 uv       : TEXCOORD4;
        float2 uv2       : TEXCOORD6;
        float id        : TEXCOORD5;
      };


      //float _Multiplier;
      //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
      //which we transform with the view-projection matrix before passing to the pixel program.
      varyings vert (uint id : SV_VertexID){

        varyings o;

        int base = id / 6;
        int alternate = id %6;

        if( base < _Count ){

          float3 extra = float3(0,0,0);

          float3 l = UNITY_MATRIX_V[0].xyz;
          float3 u = UNITY_MATRIX_V[1].xyz;
          
          float2 uv = float2(0,0);

          
          Vert v = _VertBuffer[base];

          l = normalize(v.vel) * length(v.vel) * 30;
          u = normalize(cross( l, UNITY_MATRIX_V[2].xyz));

          if( alternate == 0 ){ extra = -l - u; uv = float2(0,0); }
          if( alternate == 1 ){ extra =  l - u; uv = float2(1,0); }
          if( alternate == 2 ){ extra =  l + u; uv = float2(1,1); }
          if( alternate == 3 ){ extra = -l - u; uv = float2(0,0); }
          if( alternate == 4 ){ extra =  l + u; uv = float2(1,1); }
          if( alternate == 5 ){ extra = -l + u; uv = float2(0,1); }



          //Vert v = _VertBuffer[base % _Count];
          o.worldPos = (v.pos) + extra * _Size * saturate(min( (5-v.debug.y * 5), 5*v.debug.y));
          o.eye = _WorldSpaceCameraPos - o.worldPos;
          o.nor =v.nor;
          o.uv = v.uv;
          o.uv2 = uv;
          o.id = base;
          o.debug = v.debug;
          o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

        }

        return o;

      }

      sampler2D _MainTex;

      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

        //if( length( v.uv2 -.5) > .5 ){ discard;}

        float4 color = float4(_Color.xyz,1);// v.debug.x * 10;
        

        color = tex2D(_MainTex, v.uv2);
        if( color.x < .001){
          discard;
        }

        color = .1;
        return float4(color.xyz,1 );
      }

      ENDCG

    }
  }


}
