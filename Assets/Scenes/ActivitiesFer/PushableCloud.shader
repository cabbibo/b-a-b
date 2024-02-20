Shader "Custom/PushableCloud"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _EmissionColor ("Emission", Color) = (1,1,1,1)
        _LightCol ("Light1 Col", Color) = (1,1,1,1)
        _Light1 ("Light1", vector) = (0,1,1,20)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma vertex vert

        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            // worldPos
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
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

        fixed4 cloudCol(Input IN, fixed3 col)
        {
            fixed4 c = fixed4(col.rgb,0);
            float d = length(IN.worldPos - _Light1.xyz);
            d = clamp(d/_Light1.w, 0, 1);
            c.xyz = lerp(c.xyz, _LightCol.xyz, d);

            return c;
        }

        // get the world pos in vert shader
        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = _EmissionColor;

            // o.Emission.rgb = cloudCol(IN, o.Emission.rgb).rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
