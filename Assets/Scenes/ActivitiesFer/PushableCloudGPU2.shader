Shader "Custom/PushableCloud2"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _EmissionColor ("Emission", Color) = (1,1,1,1)
        _LightCol ("Light1 Col", Color) = (1,1,1,1)
        // _Light1 ("Light1", vector) = (0,1,1,20)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        // write depth
        // ZWrite On

        Pass
        {
        CGPROGRAM
        #pragma fragment frag
        #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap 
        // #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma vertex vert
        #pragma multi_compile_fog
        #pragma multi_compile_instancing

        #pragma target 4.5

        #include "UnityCG.cginc"

        
        struct BigParticle {
            float3 startPosition;
            float3 position;
            float3 velocity;
            float size;
            float life;
        };

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            StructuredBuffer<BigParticle> particleBuffer;
        #endif

        // struct Input
        // {
        //     float2 uv_MainTex;
        //     float3 worldPos;
        // };
        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };
        struct v2f
        {
            float2 uv : TEXCOORD0;
            UNITY_FOG_COORDS(1)
            float4 vertex : SV_POSITION;
            float3 worldPos : TEXCOORD1;
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;

        fixed4 _Color;
        fixed4 _EmissionColor;

        fixed4 _LightCol;
        float4 _Light1;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        fixed4 cloudCol(v2f IN, fixed3 col)
        {
            fixed4 c = fixed4(col.rgb,0);
            float d = length(IN.worldPos - _Light1.xyz);
            d = clamp( d / _Light1.w, 0, 1);
            c.xyz = lerp(c.xyz, _LightCol.xyz, d);

            return c;
        }

        // get the world pos in vert shader
        v2f vert (appdata v)
        {
            v2f o;
                // UNITY_INITIALIZE_OUTPUT(v2f, o);
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            UNITY_TRANSFER_FOG(o,o.vertex);
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            BigParticle data = particleBuffer[unity_InstanceID];

            o.worldPos = data.position;
        #else
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        #endif
            return o;
        }

        // void surf (Input IN, inout SurfaceOutputStandard o)
        // {
        //     // Albedo comes from a texture tinted by color
        //     fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
        //     o.Albedo = c.rgb;
        //     // Metallic and smoothness come from slider variables
        //     o.Metallic = _Metallic;
        //     o.Smoothness = _Glossiness;
        //     o.Emission = _EmissionColor;
            
        //     float l1v =saturate(1 - length(IN.worldPos - _Light1.xyz) / _Light1.w);
        //     l1v = pow(l1v, 4);
        //     o.Emission = lerp(o.Emission,_LightCol,l1v);
            
        //     l1v =saturate(1 - length(IN.worldPos - _Light1.xyz) / _Light1.w * 4);
        //     l1v = pow(l1v, 6);
        //     o.Emission = lerp(o.Emission,_LightCol,l1v);

        //     // o.Emission.rgb = cloudCol(IN, o.Emission.rgb).rgb;
        //     o.Alpha = 1;
        // }

        fixed4 frag (v2f i) : SV_Target {
            return float4(1,0,0,1);
        }
        ENDCG
    }
    }
    FallBack "Diffuse"
}
