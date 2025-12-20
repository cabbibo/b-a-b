Shader "Special/PushableCloud" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _BaseSize ("Base Size", float) = 1
        _SizeMultiplier ("Size Multiplier", float) = 2
        _Softness ("Edge Softness", Range(0, 1)) = 0.3
        _DepthFalloff ("Depth Falloff", Range(0, 1)) = 0.5
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Off
        
        Pass {
            CGPROGRAM
            
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../Chunks/hsv.cginc"
            #include "../Chunks/hash.cginc"

            uniform int _Count;
            uniform float _BaseSize;
            uniform float _SizeMultiplier;
            uniform float4 _Color;
            uniform float _Softness;
            uniform float _DepthFalloff;
            
            struct CloudParticle {
                float3 homePos;
                float3 pos;
                float3 vel;
                float  size;
                float  life;
                float  debug;
            };

            StructuredBuffer<CloudParticle> _ParticleBuffer;
            uniform sampler2D _MainTex;

            struct varyings {
                float4 pos       : SV_POSITION;
                float3 worldPos  : TEXCOORD0;
                float2 uv        : TEXCOORD1;
                float  alpha     : TEXCOORD2;
                float  id        : TEXCOORD3;
                float3 color     : TEXCOORD4;
                float  depth     : TEXCOORD5;
                float3 centerPos : TEXCOORD6;
            };


            varyings vert (uint id : SV_VertexID) {
                varyings o;

                // 6 vertices per quad (2 triangles)
                int particleID = id / 6;
                int vertexInQuad = id % 6;

                CloudParticle p = _ParticleBuffer[particleID];

                // Billboard vectors
                float3 right = UNITY_MATRIX_V[0].xyz;
                float3 up = UNITY_MATRIX_V[1].xyz;

                // Calculate UV and offset for each vertex of the quad
                float2 uv = float2(0, 0);
                float3 offset = float3(0, 0, 0);
                
                // EXPANDED SIZE: multiply by sizeMultiplier for bigger circles
                float size = p.size * _SizeMultiplier;

                // Quad vertices: 0-1-2, 3-4-5 (two triangles)
                if (vertexInQuad == 0) { uv = float2(0, 0); offset = (-right - up) * size; }
                if (vertexInQuad == 1) { uv = float2(1, 0); offset = ( right - up) * size; }
                if (vertexInQuad == 2) { uv = float2(1, 1); offset = ( right + up) * size; }
                if (vertexInQuad == 3) { uv = float2(0, 0); offset = (-right - up) * size; }
                if (vertexInQuad == 4) { uv = float2(1, 1); offset = ( right + up) * size; }
                if (vertexInQuad == 5) { uv = float2(0, 1); offset = (-right + up) * size; }

                float3 worldPos = p.pos + offset;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.worldPos = worldPos;
                o.centerPos = p.pos;
                o.uv = uv;
                o.id = float(particleID);
                
                // Calculate view space depth for the particle center
                float4 viewPos = mul(UNITY_MATRIX_V, float4(p.pos, 1.0));
                o.depth = -viewPos.z; // Positive depth (distance from camera)

                // Calculate alpha based on velocity (more pushed = slightly more transparent)
                float velMag = length(p.vel);
                o.alpha = lerp(1.0, 0.7, saturate(velMag * 0.05));

                // Slight color variation per particle
                float hueOffset = hash(float(particleID * 12345)) * 0.05 - 0.025;
                float satOffset = hash(float(particleID * 54321)) * 0.2;
                o.color = hsv(0.55 + hueOffset, 0.1 + satOffset, 1.0); // Slight blue/white tint

                return o;
            }


            float4 frag (varyings v) : SV_Target {
                // Circular falloff for soft cloud particles
                float2 centered = v.uv - 0.5;
                float dist = length(centered) * 2.0;
                
                // Soft circular gradient - EXPANDED with gentler falloff
                float alpha = 1.0 - smoothstep(0.0, 1.0 + _Softness, dist);
                
                // Add depth-based pseudo-spherical falloff
                // Center of circle should be "closer" (higher depth value)
                float sphereDepth = 1.0 - dist * dist * _DepthFalloff;
                sphereDepth = max(sphereDepth, 0.0);
                
                // Normalize depth to 0-1 range for output
                float normalizedDepth = saturate(v.depth / _ProjectionParams.z);
                // Modify depth by spherical falloff
                float finalDepth = normalizedDepth - sphereDepth * 0.1;
                
                // Sample texture if provided
                float4 texCol = tex2D(_MainTex, v.uv);
                
                // Combine color
                float3 finalColor = v.color * _Color.rgb * texCol.rgb;
                float finalAlpha = alpha * v.alpha * _Color.a * texCol.a;

                // Discard fully transparent pixels
                if (finalAlpha < 0.01) {
                    discard;
                }

                // Output: RGB = color, A = alpha with depth info encoded
                // The composite shader will use adjacent pixels to calculate normals
                return float4(finalColor, finalAlpha * (1.0 - sphereDepth * 0.3));
            }

            ENDCG
        }
    }

    Fallback Off
}

