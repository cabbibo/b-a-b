Shader "Unlit/fadeToBlack"
{
    Properties
    {


        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "white" {}
        _Color ("color", Color) = (1,1,1,1)
    }
    SubShader
    {
        // inside SubShader
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "IgnoreProjector"="True" }

        LOD 100
        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }


        Pass
        {


            // inside Pass
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD2;
                float3 world : TEXCOORD3;
                
				float3 T : TEXCOORD4;
				float3 B : TEXCOORD5;
				float3 N : TEXCOORD6;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _BackgroundTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.world = mul( unity_ObjectToWorld , v.vertex );
                o.normal = normalize(mul( unity_ObjectToWorld , float4( v.normal, 0)).xyz);

                // calc Normal, Binormal, Tangent vector in world space
				// cast 1st arg to 'float3x3' (type of input.normal is 'float3')
				float3 worldNormal = mul((float3x3)unity_ObjectToWorld ,v.normal);
				float3 worldTangent = mul((float3x3)unity_ObjectToWorld , v.tangent.xyz);
				
				float3 binormal = cross(v.normal, v.tangent.xyz); // *input.tangent.w;
				float3 worldBinormal = mul((float3x3)unity_ObjectToWorld , binormal);

				// and, set them
				o.N = normalize(worldNormal);
				o.T = normalize(worldTangent);
				o.B = normalize(worldBinormal);


                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 _Color;
            sampler2D _NormalMap;
            float _UnscaledTime;

            fixed4 frag (v2f v) : SV_Target
            {

                

                float t = _UnscaledTime;
                t *= .1;
                t = 0;
                // obtain a normal vector on tangent space
				float3 tangentNormal = tex2D(_NormalMap, v.uv * 3- float2(0,-t)).xyz * 2 - 1;
			    // tangentNormal += tex2D(_NormalMap, v.uv * 2 - float2(0,-t * .7)).xyz * 2 -1;
			    // tangentNormal += tex2D(_NormalMap, v.uv * 1- float2(0,-t * .5)).xyz * 2 -1;
				// and change range of values (0 ~ 1)
				tangentNormal = normalize(tangentNormal);

				// 'TBN' transforms the world space into a tangent space
				// we need its inverse matrix
				// Tip : An inverse matrix of orthogonal matrix is its transpose matrix
				float3x3 TBN = float3x3(normalize(v.T), normalize(v.B), normalize(v.N));
				TBN = transpose(TBN);

				// finally we got a normal vector from the normal map
				float3 worldNormal = mul(TBN, tangentNormal);

                float3 ro = v.world;

                float3 rd = v.world - _WorldSpaceCameraPos;

                float3 refrR  = refract( rd , worldNormal ,1 );
                float3 refrG  = refract( rd , worldNormal ,.995 );
                float3 refrB  = refract( rd , worldNormal ,.99 );

                float4 refractedPosR = ComputeGrabScreenPos(mul(UNITY_MATRIX_VP,float4(ro + refrR * _Color.a *.04,1)));
                float4 refractedPosG = ComputeGrabScreenPos(mul(UNITY_MATRIX_VP,float4(ro + refrG * _Color.a *.04,1)));
                float4 refractedPosB = ComputeGrabScreenPos(mul(UNITY_MATRIX_VP,float4(ro + refrB * _Color.a *.04,1)));
                float3 backgroundCol = float3(
                                        tex2Dproj(_BackgroundTexture, refractedPosR).x,
                                        tex2Dproj(_BackgroundTexture, refractedPosG).y,
                                        tex2Dproj(_BackgroundTexture, refractedPosB).z);
                // sample the texture
                float3 col = backgroundCol * .4f * _Color.g ;// + _Color.g;// +.01*float3(0.1,.3 , 1);// * float4(.2, .1,.5,1);

                col += pow(  dot( worldNormal , float3(0,1,0)), 5)*200 * _Color.g * _Color.g;

                col = saturate(col);
                // col.xyz = _Color;(1-tex2D(_MainTex,i.uv * float2(2,2) - .5).xyz);
                // col.a = _Color.a;
                return float4(col,_Color.a);
            }
            ENDCG
        }
    }
}
