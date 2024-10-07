
Shader "IMMAT/Debug/ShardMesh" {
  Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _Size ("Size", float) = .01
    _LifeDivider ("_LifeDivider", float) = 10
  }


  SubShader{
    Cull Off
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "../../Chunks/Struct16.cginc"
      #include "../../Chunks/hsv.cginc"


      uniform int _Count;
      uniform float _Size;
      uniform float3 _Color;

      struct MeshVert{
        float3 pos;
        float3 nor;
        float2 uv;
      };

   /*   
struct Vert{
    float3 pos;
    float3 vel;
    float3 nor;
    float3 ogPos;
    float life;
    float type;
    float2 debug;
};*/

    

      
      StructuredBuffer<Vert> _ShardBuffer;
      StructuredBuffer<MeshVert> _VertBuffer;
      StructuredBuffer<int> _TriBuffer;
      
      int _TriCount;
      int _VertCount;


      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
        float4 pos      : SV_POSITION;
        float3 nor      : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
        float3 eye      : TEXCOORD2;
        float3 debug    : TEXCOORD3;
        float2 uv       : TEXCOORD4;
        float2 uv2       : TEXCOORD6;
        float id        : TEXCOORD5;
        float life        : TEXCOORD7;
        float type : TEXCOORD8;
      };



      #include "Assets/Resources/Shaders/Chunks/rotationMatrix.cginc"
      #include "Assets/Resources/Shaders/Chunks/translationMatrix.cginc"
      #include "Assets/Resources/Shaders/Chunks/scaleMatrix.cginc"
      #include "Assets/Resources/Shaders/Chunks/Matrix.cginc"


      //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
      //which we transform with the view-projection matrix before passing to the pixel program.
      varyings vert (uint id : SV_VertexID){

        varyings o;

        int base = id / _TriCount;
        int alternate = id % _TriCount;

        

        if( base < _Count ){


          Vert v = _ShardBuffer[base % _Count];
          float3 extra = float3(0,0,0);

          float3 l = UNITY_MATRIX_V[0].xyz;
          float3 u = UNITY_MATRIX_V[1].xyz;
          
          float2 uv = float2(0,0);

          MeshVert vert = _VertBuffer[ _TriBuffer[ alternate] ];

          float type = v.uv.y;
          float life = v.uv.x;


          
          float4x4 t = translationMatrix(v.pos);
          float4x4 r = look_at_matrix(normalize(v.vel) ,normalize(cross(normalize(v.vel),float3(1,0,0))));
          float4x4 s = scaleMatrix(1);

          float4x4 rts =mul(t,mul(r,s));

          float3 fwd = normalize(v.vel *1000);
          float3 up = normalize(cross(fwd, float3(1,0,0)));
          float3 right = normalize(cross(up,fwd));
          // o.worldPos =   v.pos + vert.pos * _Size;//
          o.worldPos = mul( rts, float4(vert.pos.xzy * _Size * pow( life ,.5), 1)).xyz;
          o.nor =  normalize(mul( rts, float4(vert.nor.xzy, 0)).xyz);
          o.eye = _WorldSpaceCameraPos - o.worldPos;
        //  o.nor =;
          o.uv = v.uv;
          o.uv2 = uv;
          o.id = base;
          o.life = life;
          o.type = type;
          o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

        }

        return o;

      }

      float _LifeDivider;



      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {


        float3 flatNormal = normalize( cross( ddx(v.worldPos), ddy(v.worldPos) ) );
        //if( length( v.uv2 -.5) > .5 ){ discard;}


          //col = v.normal * .5  + .5;
        
        float3 col = float4(_Color.xyz,1);// v.debug.x * 10;

        //color.xyz = hsv(v.life / _LifeDivider,1,1).xyz;

        col = v.nor;// * .5  + .5;

       // col = flatNormal;
        return float4(col,1 );
      }

      ENDCG

    }
  }


}
