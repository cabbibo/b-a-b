// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/triNoiseNormal1" {
	Properties {
  

    _NumberSteps( "Number Steps", Int ) = 3
    _StepSize( "Step Size", Float ) = 0.16
    _NoiseSize( "Noise Size" , Float ) = 0.6
    _HueSize( "Hue Size" , Float ) = 2.45
    _BaseHue( "Base Hue" , Float ) = 1.67
    _NoiseSpeed( "Noise Speed" , Float ) = 0.8


  }
  SubShader {
    Tags { "RenderType"="Transparent" "Queue" = "Transparent" }

    //Tags { "RenderType"="Opaque" }
    LOD 200
    ZWrite On
    Pass {
      //Blend SrcAlpha OneMinusSrcAlpha // Alpha blending


      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      // Use shader model 3.0 target, to get nicer looking lighting
      #pragma target 3.0

      #include "UnityCG.cginc"


      uniform int _NumberSteps;
      uniform float _StepSize;
      uniform float _NoiseSize;
      uniform float _NoiseSpeed;
      uniform float _HueSize;
      uniform float _BaseHue;


      struct VertexIn{
         float4 position  : POSITION; 
         float3 normal    : NORMAL; 
         float4 texcoord  : TEXCOORD0; 
         float4 tangent   : TANGENT;
      };


      struct VertexOut {
          float4 pos    	: POSITION; 
          float3 normal 	: NORMAL; 
          float4 uv     	: TEXCOORD0; 
          float3 ro     	: TEXCOORD1;
          float3 rd     	: TEXCOORD2;
          float3 camPos 	: TEXCOORD3;
          float3 lightPos : TEXCOORD4;
      };

      

      float3 hsv(float h, float s, float v){
        return lerp( 1, clamp(( abs( frac(h + float3( 3.0, 2.0, 1.0 ) / 3.0 )
        					 * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
      }


			// Taken from https://www.shadertoy.com/view/4ts3z2
			// By NIMITZ  (twitter: @stormoid)
			// good god that dudes a genius...

			float tri( float x ){ 
			  return abs( frac(x) - .5 );
			}

			float3 tri3( float3 p ){
			 
			  return float3( 
			      tri( p.z + tri( p.y * 1. ) ), 
			      tri( p.z + tri( p.x * 1. ) ), 
			      tri( p.y + tri( p.x * 1. ) )
			  );

			}
			                                 

			float triNoise3D( float3 p, float spd , float time){
			  
			  float z  = 1.4;
				float rz =  0.;
			  float3  bp =   p;

				for( float i = 0.; i <= 3.; i++ ){
			   
			    float3 dg = tri3( bp * 2. );
			    p += ( dg + time * .1 * spd );

			    bp *= 1.8;
					z  *= 1.5;
					p  *= 1.2; 
			      
			    float t = tri( p.z + tri( p.x + tri( p.y )));
			    rz += t / z;
			    bp += 0.14;

				}

				return rz;

			}

			float3 triNoiseNormal( float3 p ){

				float3 eps = float3( 0.1 , 0.0 , 0.0 );
				float3 triNormal = float3( 
					triNoise3D( ( p + eps.xyy ) * _NoiseSize , _NoiseSpeed , _Time ) - triNoise3D( ( p - eps.xyy ) * _NoiseSize , _NoiseSpeed , _Time ),
					triNoise3D( ( p + eps.yxy ) * _NoiseSize , _NoiseSpeed , _Time ) - triNoise3D( ( p - eps.yxy ) * _NoiseSize , _NoiseSpeed , _Time ),
					triNoise3D( ( p + eps.yyx ) * _NoiseSize , _NoiseSpeed , _Time ) - triNoise3D( ( p - eps.yyx ) * _NoiseSize , _NoiseSpeed , _Time )
				);

				return normalize( triNormal );

			}

    

      VertexOut vert(VertexIn v) {
        
        VertexOut o;

        o.normal = v.normal;
        
        o.uv = v.texcoord;
  
        // Getting the position for actual position
        o.pos = UnityObjectToClipPos(  v.position );
     
        float3 mPos = mul( unity_ObjectToWorld , v.position );

        o.ro = mPos;
        o.rd =  -normalize(_WorldSpaceCameraPos - mPos ); 

       	o.camPos =  _WorldSpaceCameraPos;
       // o.lightPos =  float3( 2. , 2. , -2.);
        return o;

      }

      // Fragment Shader
      fixed4 frag(VertexOut i) : COLOR {


				float3 oldNorm = i.normal;	

       // float4x4 iTBN = float4x4( i.iTang , i.iBino , i.iNorm , i.iLast );

        float3 ro 			= i.ro;
        float3 rd 			= i.rd; 
        float3 camPos 	= i.camPos;
        float3 lightPos = i.lightPos;
         
        float3 col = float3( 0.0 , 0.0 , 0.0 );

        float3 p;

       	float3 lightDir = lightPos - ro;
				lightDir = _WorldSpaceLightPos0.xyz;//normalize( lightDir );

				float3 camDir = camPos - ro;
				camDir = -normalize( camDir );


				float refl = reflect( lightDir , oldNorm );
				float match = max( 0. , dot( refl , camDir ));
				//match = pow( match , 10. );

				//col = oldNorm * .5 + .5;


        float acc = 0.;
        for( int i = 0; i < _NumberSteps; i++ ){

          p = ro + rd * float( i ) * _StepSize / _NumberSteps;

 
 					float3 norm = -triNoiseNormal( p * (float(i)+1.) );
 					
 					norm = normalize( norm + oldNorm );

       		//lightDir = normalize( lightPos - p );
					camDir   = normalize( camPos   - p );


 					float refl = reflect( lightDir , norm );
 					float match = dot( lightDir , norm );

 					match = pow( match , 4. );
 					match *= 1.;
      
          match = clamp( match, 0. , 1. );
          acc += match;


          col += hsv( match * _HueSize + _BaseHue , 1. , 1. )  * acc * 2.;

          if( match > 1. ){ break; }

        }

        col /= _NumberSteps;

       





		    fixed4 color;
        color = fixed4( col * .3, 1. );
        return color;
      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}