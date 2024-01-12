Shader "Custom/RaytracedNoiseSkyboxShader" {
    Properties {
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Parallax ("Parallax", Range(0, 0.1)) = 0.02
        _Steps ("Raymarch Steps", Range(1, 100)) = 10
        _Intensity ("Intensity", Range(0, 2)) = 1
    }

    SubShader {
        Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="SkyboxCube"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert
        #pragma target 3.0

        sampler2D _NoiseTex;
        float _Parallax;
        int _Steps;
        float _Intensity;
        samplerCUBE _TexCube;

        struct Input {
            float3 worldPos;
        };

        float3 rayDirection(float3 worldPos, float3 cameraPos) {
            return normalize(worldPos - cameraPos);
        }

        float customNoise(float3 pos) {
            return tex2D(_NoiseTex, pos.xy).r;
        }

        float3 raymarch(float3 start, float3 dir, int steps) {
            float3 currentPosition = start;
            for (int i = 0; i < steps; i++) {
                float noiseValue = customNoise(currentPosition);
                currentPosition += dir * noiseValue * _Parallax;
            }
            return currentPosition;
        }

        void vert (inout appdata_full v, out Input o) {
            o.worldPos = UnityObjectToWorldDir(v.vertex.xyz);
        }

        void surf (Input IN, inout SurfaceOutput o) {
            float3 viewDirection = rayDirection(IN.worldPos, _WorldSpaceCameraPos);
            float3 raymarchedDirection = raymarch(viewDirection, viewDirection, _Steps);
            o.Emission = texCUBE(_TexCube, raymarchedDirection).rgb * _Intensity;
            o.Albedo = 0;
        }
        ENDCG
    }
    FallBack "Skybox/6 Sided"
}