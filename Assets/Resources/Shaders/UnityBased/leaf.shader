// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/leaf" {
	
  Properties {
  

    _NumberSteps( "Number Steps", Int ) = 10
    _StepSize( "Step Size", Float ) = .3
    _NoiseSize( "Noise Size" , Float ) = 100.
    _HueSize( "Hue Size" , Float ) = .08
    _BaseHue( "Base Hue" , Float ) = .9
    _NoiseSpeed( "Noise Speed" , Float ) = .8
    _NoiseCutoff( "Noise Cutoff" , Float ) = .3

    _Color1( "Color1" , Color ) = ( .3 , 1. , .5 , 1.)
    _Color2( "Color2" , Color ) = ( .0 , .8, .1 , 1.)


  }

  SubShader {
    
   Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
    LOD 200
    ZWrite Off
    Pass {
      Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      // Use shader model 3.0 target, to get nicer looking lighting
      #pragma target 3.0
      #define UNITY_PASS_FORWARDBASE

      #include "UnityCG.cginc"


      uniform int _NumberSteps;
      uniform float _StepSize;
      uniform float _NoiseSize;
      uniform float _NoiseSpeed;
      uniform float _NoiseCutoff;
      uniform float _HueSize;
      uniform float _BaseHue;
      uniform fixed4 _Color1;
      uniform fixed4 _Color2;


      struct VertexIn
      {
         float4 position  : POSITION; 
         float3 normal    : NORMAL; 
         float4 texcoord  : TEXCOORD0; 
         float4 tangent   : TANGENT;
      };

      struct VertexOut {
          float4 pos    : POSITION; 
          float3 normal : NORMAL; 
          float4 uv     : TEXCOORD0; 
          float3 ro     : TEXCOORD2;
          float3 rd     : TEXCOORD3;
      };
        

      

      float3 hsv(float h, float s, float v){
        return lerp( float3( 1.0 ,1.0 ,1.0 ), clamp(( abs( frac(h + float3( 3.0, 2.0, 1.0 ) / 3.0 )
        					 * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
      }


	

      VertexOut vert(VertexIn v) {
        
        VertexOut o;

        o.normal = v.normal;
        
        o.uv = v.texcoord;
  
        // Getting the position for actual position
        o.pos = UnityObjectToClipPos(  v.position );
     
        float3 mPos = mul( unity_ObjectToWorld , v.position );

        o.ro = v.position;
        o.rd = mul( unity_WorldToObject , float4( -normalize(_WorldSpaceCameraPos - mPos ) , 0. )); 

        return o;

      }

      // Fragment Shader
      fixed4 frag(VertexOut i) : COLOR {

        float3 ro = i.ro;
        float3 rd = i.rd;

        float3 col = float3( 0.0 , 0.0 , 0.0 );

        float3 p;

        float acc = 0.;

        float step = 0.;
        float hit = 0.;

        for( int i = 0; i < _NumberSteps; i++ ){

          p = ro + rd * float( i ) * _StepSize / _NumberSteps;

          /*
            THIS IS WHERE THE MAGIC HAPPENS!!!
          */

          
          float3 newP = p;// - float3( 0. ,5. *  _Time.x , 0. );

          float noiseVal = 1;// triNoise3D(  newP * _NoiseSize / 100. * float3( 1. , 1. , 2. )  , 10. , _Time * _NoiseSpeed );//length( sin( p * 10.));
          /*acc += noiseVal;
          col += hsv( acc * _HueSize + _BaseHue , 1. , 1. ) * noiseVal * acc * 2.;*/

          step = float( i ) + 30*noiseVal;//-_NoiseCutoff;
          if( noiseVal > _NoiseCutoff ){hit = 1.; break;  }

        }

        col = hsv( ( step / float( _NumberSteps ) ) * _HueSize + _BaseHue , 1. , 1. );

        //col /= _NumberSteps;


		fixed4 color;
        color = fixed4( col , 1. );
        return color;
      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}
