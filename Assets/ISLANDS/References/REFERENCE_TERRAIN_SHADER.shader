Shader "REFERENCES/REFERENCE_TERRAIN_SHADER"
{

    Properties
    {

        _Debug("_Debug",int) = 0
        _DataTexture("_DataTexture", 2D) = "white" {}
        _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        _WaterflowMap ("WaterflowMap (RGB)", 2D) = "white" {}
        _NormalsAndAOMap ("AOMap (RGB)", 2D) = "white" {}
        _TextureMap("TextureMap (RGB)", 2D) = "white" {}


        _NormalMapStrength ("Normal Map Strength", Range(0, 10)) = 1.0

        [HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        [HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
        [HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
        [HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
        [HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
        [HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)

    }

    CGINCLUDE
    #include "Assets/Resources/Shaders/Chunks/TerrainShaderIncludes.cginc"
    ENDCG

    SubShader
    {

        Pass
        {

            Tags
            {
                "RenderType" = "Opaque" "LightMode" = "ForwardBase" "Queue" = "Geometry-1"
            }

            LOD 100
            Cull Off
            ZWrite On

            CGPROGRAM
            #pragma target 4.5
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag


            float3 DoShadingModel( LightingData data )
            {
                return data.color * data.shadow * _LightColor0;
            }

            //Pixel function returns a solid color for each point.
            float4 frag( varyings v ): COLOR
            {
                //DoShadingModel( GetLightingData( v ) );
                float3 col = DoShadingModel( GetLightingData( v ) );

                return float4( col , 1 );


            }
            ENDCG

        }



    }

    //Just use diffuse for shadows
    Fallback "Diffuse"


}