// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Quill/Basice" {
  Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _Multiplier("_Multiplier",Float)= 1
  
    



  }


  SubShader{

    Pass{

      Tags { "RenderType"="Opaque" }
      LOD 100 
      Cull Off
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 4.5
   
      
 #include "UnityCG.cginc"
      uniform float3 _Color;
      uniform float _Multiplier;



      //uniform float4x4 worldMat;

      sampler2D _MainTex;
      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
        float4 pos      : SV_POSITION;
        float3 worldPos : TEXCOORD1;
        float3 col : TEXCOORD2;
      };

      #include "../Chunks/hash.cginc"
      uniform float4x4 _Transform;
      uniform int _NumberMeshes;
      //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
      //which we transform with the view-projection matrix before passing to the pixel program.
      varyings vert (appdata_full vert){
        varyings o;
        
        o.worldPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;
        o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

          o.col = vert.color;
        return o;

      }


      uniform sampler2D _PaintTexture;

      #include "../Chunks/triplanar.cginc"
      #include "../Chunks/snoise3D.cginc"
      
      float3 _WrenPos;
      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

      float3 col = v.col  * _Multiplier;
      return float4(col,1);
    }

    ENDCG

  }


  


























}

Fallback "Diffuse"


}

