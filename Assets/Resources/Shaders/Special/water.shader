Shader "Custom/Water"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_MainTex("Main Texture", 2D) = "white" {}
		_DepthFactor("Depth Factor", float) = 1.0
		_WaveSpeed("Water Speed", float) = 1.0
		_NoiseSize("NoiseSize", float) = 1.0
		_WaveMaxHeight("WaveMaxHeight", float) = 1.0
		_WaveHeightFalloff("WaveHeightFalloff", float) = 1
		_WaveDirection("Wave Direction", Vector) = (0,1,0,0) 
	}

	SubShader
	{
        Tags
		{ 
			"Queue" = "Transparent"
		}

		// Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }

		Pass
		{
            //Blend SrcAlpha OneMinusSrcAlpha

			Cull Off

			CGPROGRAM
            #include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag
			
			// Properties
			float4 _Color;
			float4 _EdgeColor;
			float  _DepthFactor;
			float  _WaveSpeed;
			sampler2D _CameraDepthTexture;
			sampler2D _NoiseTex;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BackgroundTexture;

			float2 _WaveDirection;
			float _WaveMaxHeight;
			float _WaveHeightFalloff;
			float _NoiseSize;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos 			: SV_POSITION;
				float2 uv 			: TEXCOORD0;
				float4 screenPos 	: TEXCOORD1;
                float3 nor 		 	: NORMAL;
                float3 worldPos 		 	: TEXCOORD2;
			};

			#include "../Chunks/noise.cginc"


			float3 getWorldPos( float3 pos , float2 inputPos){

				
				float n1 = 1-tex2Dlod(_NoiseTex, float4( ( _WaveDirection * 100 * _WaveSpeed * _Time.y + pos.xz)  * .00001   * _NoiseSize  , 0, 0)).x;
				float n2 = 1-tex2Dlod(_NoiseTex, float4( ( _WaveDirection * 100 * _WaveSpeed * _Time.y + pos.xz)  * .00001   * _NoiseSize * 2, 0, 0)).x;
				float n3 = 1-tex2Dlod(_NoiseTex, float4( ( _WaveDirection * 100 * _WaveSpeed * _Time.y + pos.xz)  * .00001   * _NoiseSize * 20, 0, 0)).x;
				float n4 = 1-tex2Dlod(_NoiseTex, float4( ( -_WaveDirection * 100 * _WaveSpeed * _Time.y + pos.xz)  * .00001  * _NoiseSize * 30, 0, 0)).x;
				return pos + (n1 + n2 * 1 + n3 * 1 + n4 * 1) * _WaveMaxHeight * float3(0,100,0)/ (1 + length(pos.xz) * _WaveHeightFalloff * .001 );
			}

			v2f vert(appdata input)
			{
				v2f o;

				float3 worldPos = mul( unity_ObjectToWorld , input.vertex );
				worldPos = getWorldPos( worldPos , input.vertex.xy);
				float eps = 10;
				float3 wpU = getWorldPos( worldPos + float3(eps,0,0), input.vertex.xy);
				float3 wpR = getWorldPos( worldPos + float3(0,0,eps), input.vertex.xy);
				float3 wpU1 = getWorldPos( worldPos + float3(-eps,0,0), input.vertex.xy);
				float3 wpR1 = getWorldPos( worldPos + float3(0,0,-eps), input.vertex.xy);


				o.nor = normalize(cross(1000*(wpR-wpR1) , 1000*(wpU-wpU1)));

				// convert to world space
				o.pos =mul( UNITY_MATRIX_VP , float4(worldPos,1));
				o.worldPos = worldPos;

				// apply wave animation
				float noiseSample = tex2Dlod(_NoiseTex, float4(input.texcoord.xy, 0, 0));
				
				// compute depth
				o.screenPos = ComputeScreenPos(o.pos);

				// texture coordinates 
				o.uv = TRANSFORM_TEX(input.texcoord, _MainTex);

                //o.nor = mul( unity_ObjectToWorld , float4( input.normal , 0 ) ).xyz;

				return o;
			}

			float4 frag(v2f v) : COLOR
			{
				
                
                // apply depth texture
				float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, v.screenPos);
				float depth = LinearEyeDepth(depthSample).r;


                float4 bgTex = tex2Dproj(_BackgroundTexture, v.screenPos);
				// create foamline
				float foamLine = 1 - saturate(_DepthFactor * (depth - v.screenPos.w));

				// sample main texture
				float4 albedo = tex2D(_MainTex, v.worldPos.xz * .1);

				float m = dot( _WorldSpaceLightPos0 , v.nor);

                float depthVal = _DepthFactor*(depth - v.screenPos.w);
               /* float3 col = lerp( bgTex, _Color,depthVal);     
                if( depthVal - albedo.x - albedo2.x < .1 * dot(v.nor , float3(0,1,0))){
                    col = 1;
                }*/


				float3 col =  m;//float3(v.uv , 1);//1;// _Color;//v.nor * .5 + .5;//albedo;// saturate(col);

				//col *= (.5 - length( v.uv - .5)) * 2; 


                return float4(col,1);
			}

			ENDCG
		}
	}
}