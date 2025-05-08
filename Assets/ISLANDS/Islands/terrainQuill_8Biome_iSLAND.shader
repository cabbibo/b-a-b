Shader "Terrain/quillTerrain_8Biome_Island"
{
    Properties
    {

        _Debug("_Debug",int) = 0
        _DataTexture("_DataTexture", 2D) = "white" {}
        _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        _WaterflowMap ("WaterflowMap (RGB)", 2D) = "white" {}
        _NormalsAndAOMap ("AOMap (RGB)", 2D) = "white" {}
        _TextureMap("TextureMap (RGB)", 2D) = "white" {}



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
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 4.5
    // make fog work
    //#pragma multi_compile_fogV
    #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight


    #pragma multi_compile_instancing

    // #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
    //#pragma multi_compile_instancing

    #include "UnityCG.cginc"
    #include "AutoLight.cginc"
    #include "UnityLightingCommon.cginc"


    #include "Assets/Resources/Shaders/Chunks/hsv.cginc"
    #include "Assets/Resources/Shaders/Chunks/noise.cginc"

    float sdCapsule( float3 p , float3 a , float3 b , float r )
    {
        float3 pa = p - a, ba = b - a;
        float  h  = clamp( dot( pa , ba ) / dot( ba , ba ) , 0.0 , 1.0 );
        return length( pa - ba * h ) - r;
    }


    struct appdata_full2
    {
        float4 vertex : POSITION;
        float4 tangent : TANGENT;
        float3 normal : NORMAL;
        float4 texcoord : TEXCOORD0;
        float4 texcoord1 : TEXCOORD1;
        float4 texcoord2 : TEXCOORD2;
        float4 texcoord3 : TEXCOORD3;
        fixed4 color : COLOR;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    //uniform float4x4 worldMat;
    sampler2D _TerrainTexture1;


    //A simple input struct for our pixel shader step containing a position.
    struct varyings
    {
        float4 pos : SV_POSITION;
        float3 nor : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
        float3 eye : TEXCOORD2;
        float3 debug : TEXCOORD3;
        float2 uv : TEXCOORD4;
        float2 uv2 : TEXCOORD6;
        float4 color : TEXCOORD11;
        float  id : TEXCOORD5;
        float4 data1:TEXCOORD9;
        float4 tc:TEXCOORD12;
        float4 screenPos : TEXCOORD7;
        //    float3 debug : TEXCOORD13;
        UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.

        UNITY_SHADOW_COORDS( 8 )
        UNITY_FOG_COORDS( 10 )
    };


    sampler2D _Control;
    float4    _Control_ST;
    float4    _Control_TexelSize;
    sampler2D _Splat0,    _Splat1,    _Splat2,    _Splat3;
    float4    _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

    sampler2D _TerrainHeightmapTexture;
    sampler2D _TerrainNormalmapTexture;
    float4    _TerrainHeightmapRecipSize; // float4(1.0f/width, 1.0f/height, 1.0f/(width-1), 1.0f/(height-1))
    float4    _TerrainHeightmapScale; // float4(hmScale.x, hmScale.y / (float)(kMaxHeight), hmScale.z, 0.0f)


    UNITY_INSTANCING_BUFFER_START( Terrain )
        UNITY_DEFINE_INSTANCED_PROP( float4 , _TerrainPatchInstanceData ) // float4(xBase, yBase, skipScale, ~)
    UNITY_INSTANCING_BUFFER_END( Terrain )

    sampler2D _Normal0,      _Normal1,      _Normal2,      _Normal3;
    float     _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;


    sampler2D _TerrainHolesTexture;


    uniform float4x4 _Transform;
    uniform int      _NumberMeshes;
    ENDCG

    SubShader
    {

        Pass
        {

            Tags
            {
                "RenderType"="Opaque" "LightMode" = "ForwardBase" "Queue" = "Geometry-1"
            }

            LOD 100
            Cull Off
            ZWrite On
            CGPROGRAM
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert( appdata_full2 v )
            {
                varyings o;

                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v , o ); // necessary only if you want to access instanced properties in the fragment Shader.

                UNITY_INITIALIZE_OUTPUT( varyings , o );

                float2 patchVertex  = v.vertex.xy;
                float4 instanceData = UNITY_ACCESS_INSTANCED_PROP( Terrain , _TerrainPatchInstanceData );

                float4 uvscale  = instanceData.z * _TerrainHeightmapRecipSize;
                float4 uvoffset = instanceData.xyxy * uvscale;
                uvoffset.xy += 0.5f * _TerrainHeightmapRecipSize.xy;
                float2 sampleCoords = ( patchVertex.xy * uvscale.xy + uvoffset.xy );

                float hm = UnpackHeightmap( tex2Dlod( _TerrainHeightmapTexture , float4( sampleCoords , 0 , 0 ) ) );

                v.texcoord3 = v.texcoord2 = v.texcoord1 = v.texcoord;

                o.worldPos = mul( unity_ObjectToWorld , float4( v.vertex.xyz , 1 ) ).xyz;
                o.pos      = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );
                o.eye      = _WorldSpaceCameraPos - o.worldPos;
                o.nor      = normalize( mul( unity_ObjectToWorld , float4( v.normal , 0 ) ).xyz );
                o.uv       = v.texcoord.xy;
                o.tc       = v.texcoord;
                o.color    = v.color;

                o.screenPos = ComputeScreenPos( o.pos );

                UNITY_TRANSFER_SHADOW( o , o.worldPos );
                UNITY_TRANSFER_FOG( o , o.pos );


                return o;

            }


            uniform sampler2D _PaintTexture;

            #include "Assets/Resources/Shaders/Chunks/triplanar.cginc"
            #include "Assets/Resources/Shaders/Chunks/snoise3D.cginc"
            #include "Assets/Resources/Shaders/Chunks/triNoise3D.cginc"
            #include "Assets/Resources/Shaders/Chunks/SunShadows.cginc"


            sampler2D _BiomeMap;

            float _BiomeMapWeight;

            sampler2D _BiomeMap1;
            sampler2D _BiomeMap2;

            sampler2D _TextureMap;
            sampler2D _MainTex;
            sampler2D _WaterflowMap;
            sampler2D _NormalsAndAOMap;
            sampler2D _DataTexture;


            float3 _WrenPos;
            //Pixel function returns a solid color for each point.
            float4 frag( varyings v ) : COLOR
            {

                fixed  shadow = UNITY_SHADOW_ATTENUATION( v , v.worldPos );
                float4 col    = tex2D( _MainTex , v.uv );
                float4 biome1 = tex2D( _BiomeMap1 , v.uv );
                float4 biome2 = tex2D( _BiomeMap2 , v.uv );

                float4 waterflow = tex2D( _WaterflowMap , v.uv );
                float4 normals   = tex2D( _NormalsAndAOMap , v.uv );
                float4 data      = tex2D( _DataTexture , v.uv );


                float3 normal = v.nor;
                float3 fNor   = normalize( normal );; // * .5 ;// normalize(v.nor);

                float4 biomeValues1 = tex2D( _BiomeMap1 , v.uv );
                float4 biomeValues2 = tex2D( _BiomeMap2 , v.uv );


                float biomeWeights[ 8 ] = {biomeValues1.x, biomeValues1.y, biomeValues1.z, biomeValues1.w, biomeValues2.x, biomeValues2.y, biomeValues2.z, biomeValues2.w};

                //col *= _LightColor0;
                col *= shadow;


                return float4( col.xyz , 1 );
            }
            ENDCG

        }










    }

    Fallback "Diffuse"


}