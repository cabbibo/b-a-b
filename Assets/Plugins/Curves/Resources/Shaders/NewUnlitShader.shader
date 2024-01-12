Shader "Unlit/Curvey"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

               // input info
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 texcoord : TEXCOORD0;
                uint id : SV_VertexID;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

           #if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL) || defined(SHADER_API_METAL) || defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN) || defined(SHADER_API_SWITCH) // D3D11, D3D12, XB1, PS4, iOS, macOS, tvOS, glcore, gles3, webgl2.0, Switch
           
                #include "CurveInfo.cginc"
            #endif


            sampler2D _MainTex;
            float4 _MainTex_ST;

              #pragma target 4.5

            v2f vert (appdata IN)
            {
  v2f o;

    #if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL) || defined(SHADER_API_METAL) || defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN) || defined(SHADER_API_SWITCH) // D3D11, D3D12, XB1, PS4, iOS, macOS, tvOS, glcore, gles3, webgl2.0, Switch
          
                int id = IN.id /(2 * _ResolutionX);
                int which = IN.id%2;
                int inRez = (IN.id/2) % _ResolutionX;

                float v = float(id)/float(_Resolution);

                float3 p; float3 dir; float3 tang; float w;

                GetCubicInformation( v, p , dir, tang,w);

              

                float val = float(inRez+which) / _ResolutionX;

                float3 fPos = p;
                fPos += (val -.5) * tang * w;




                o.vertex = mul( UNITY_MATRIX_VP , float4(fPos,1));
                #endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = 1;
                return col;
            }
            ENDCG
        }
    }
}
