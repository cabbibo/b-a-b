// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/BonesConnection" {
  Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _Size ("Size", float) = .01
    _Forwards ("Forwards", float) = 1
  }


  SubShader{
    Cull Off
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag
      
      #pragma multi_compile_instancing
      #pragma instancing_options procedural:setup

      #include "UnityCG.cginc"
      #include "Assets/Resources/Shaders/Chunks/hsv.cginc"



      //#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
      StructuredBuffer<float2>    _ConnectionBuffer;
      StructuredBuffer<float4x4>  _TransformBuffer;
      //#endif




      uniform int _Count;
      uniform float _Size;
      uniform float _Forwards;
      uniform float3 _Color;

      

      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
        float4 pos      : SV_POSITION;
        float3 nor      : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
        float3 eye      : TEXCOORD2;
        float2 uv       : TEXCOORD4;
        float id        : TEXCOORD5;
      };


      float4x4 createTransformationMatrix(float3 forward, float3 right, float3 up, float3 position, float3 scale) {
        // Normalize the direction vectors to ensure they are unit vectors
        forward = normalize(forward);
        right = normalize(right);
        up = normalize(up);
        
        // Return the transformation matrix by filling in the values directly
        return float4x4(
        scale.x * right.x, scale.x * right.y, scale.x * right.z, 1.0,  // First row: scaled right vector
        scale.y * up.x,    scale.y * up.y,    scale.y * up.z,    1.0,  // Second row: scaled up vector
        scale.z * forward.x, scale.z * forward.y, scale.z * forward.z, 1.0,  // Third row: scaled negative forward vector
        position.x,        position.y,        position.z,        1.0   // Fourth row: position (translation)
        );
      }

      uniform float4x4 _Transform;
      //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
      //which we transform with the view-projection matrix before passing to the pixel program.
      varyings vert ( appdata_full v ,  uint instanceID : SV_InstanceID) {

        varyings o;

        
        float2 connections = _ConnectionBuffer[instanceID];


        float4x4 m1 = _TransformBuffer[int(connections.x)];
        float4x4 m2 = _TransformBuffer[int(connections.y)];

        
        float3 p1 = mul(m1, float4(0,0,0,1)).xyz;
        float3 p2 = mul(m2, float4(0,0,0,1)).xyz;


        float3 dif = p2 - p1;


        float3 f1 = mul(m1, float4(0,0,1,0)).xyz;
        float3 f2 = mul(m2, float4(0,0,1,0)).xyz;

        float3 r1 = mul(m1, float4(1,0,0,0)).xyz;
        float3 r2 = mul(m2, float4(1,0,0,0)).xyz;

        float3 u1 = mul(m1, float4(0,1,0,0)).xyz;
        float3 u2 = mul(m2, float4(0,1,0,0)).xyz;


        float3 p = (p1 + p2) / 2;
        float3 f = (f1 + f2) / 2;
        float3 r = (r1 + r2) / 2;
        float3 u = (u1 + u2) / 2;



        r = normalize(dif);

        u = float3(0,1,0);//normalize(cross(r, f));
        f = cross(u, r);
        


        f = normalize(f);
        r = normalize(r);
        u = normalize(u);



        float scl = length(dif)  * _Size;
        

        float4x4 worldMat = float4x4(
        scl*r.x, scl*u.x, scl*f.x, p.x,
        scl*r.y, scl*u.y, scl*f.y, p.y,
        scl*r.z, scl*u.z, scl*f.z, p.z,
        0,0,0, 1);

        //worldMat = createTransformationMatrix(f, r, u, p, 1);



        float3 worldPos = mul( worldMat , float4(v.vertex.xyz * _Size,1)).xyz;

        // worldPos = v.vertex.xyz * .02 * ( f + u +r) + p;


        o.worldPos = worldPos;
        o.eye = _WorldSpaceCameraPos - o.worldPos;
        o.nor = v.normal.xyz;//v.nor;
        o.uv = v.texcoord.xy;
        o.id = instanceID;
        o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

        

        return o;

      }


      

      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {


        float3 col = 1;
        return float4(col,1 );
      }

      ENDCG

    }
  }

  Fallback Off


}
