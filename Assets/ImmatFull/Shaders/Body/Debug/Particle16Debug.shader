
Shader "IMMAT/Debug/Particles16" {
    Properties {

    _Color ("Color", Color) = (1,1,1,1)
    _Size ("Size", float) = .01
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
      #include "../../Chunks/debugVSChunk.cginc"


      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

          if( length( v.uv2 -.5) > .5 ){ discard;}

          float4 color = float4(_Color.xyz,1);// v.debug.x * 10;
          
          return float4(color.xyz,1 );
      }

      ENDCG

    }
  }


}
