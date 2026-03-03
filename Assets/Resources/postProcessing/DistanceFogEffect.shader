Shader "PostProcessing/DistanceFog"
{



    CGINCLUDE
    sampler2D _MainTex;
    sampler2D _DepthTex;
    sampler2D _CameraDepthTexture;
    sampler2D _HeightMap;

    float  _FogStart;
    float  _FogEnd;
    float  _FogAmount;
    float4 _FogStartColor;
    float4 _FogEndColor;
    float  _SkyboxImportance;

    float4x4 _InverseProjection;
    float4x4 _InverseView;

    float4x4 _InverseViewProjection;
    //#include "UnityLightingCommon.cginc"


    //samplerCube _Cubemap;
    float _Intensity;

    float _StartDistance;
    float _EndDistance;


    float4 _LightColor0;

    /* transform uv -> NDC
    
    then create clipspacePos =[NDCx, NDCy, -1, 1]
    
    viewPos = invPorjectMtarix * clipspacePos
    worldPos = invViewMatrix * viewPos
    
    rayDir = worldPos-cameraPos*/


    /*float3 _MapSize;
    float3 _MapOffset;


    float  _FogMultiplier;
    float  _FogHeightMultiplier;
    float  _FogHeightPower;
    float  _FogDensityAtFar;
    float  _FogDensityAtNear;
    float  _FogStepSize;
    float  _MaxFogTotal;
    float4 _FogColorNear;
    float4 _FogColorFar;
    float4 _FogColorDistant;
    float  _OceanHeight;

    float _LightColorImportance;
    int   _FogSamples;
    #define _FogSamples 40
*/

    float3 GetRayDirection( float2 texcoord )
    {
        // Convert texcoord to NDC
        float2 ndc = texcoord * 2.0 - 1.0;



        // Create a clip space position with z=1 and w=1 (for far plane)
        float4 clipSpacePos = float4( ndc , 1.0 , 1.0 );

        // Transform clip space position to camera space
        float4 viewPos = mul( _InverseProjection , clipSpacePos );
        viewPos /= viewPos.w;

        float3 worldPos = mul( _InverseView , viewPos ).xyz;
        //   cameraSpaceDir.w = 0;


        // Normalize the direction
        float3 rayDirection = normalize( worldPos.xyz - _WorldSpaceCameraPos );


        //rayDirection = normalize(mul(_InverseViewProjection, float4(ndc.x, ndc.y,-1, 0)).xyz);
        return rayDirection;
    }

    const float e = 2.7182818284590452353602874713527;

    float staticNoise( float2 texCoord )
    {
        //float G = e + (_Time.y * 0.00001+1000);
        float  G = e + ( 0.00001 + 10 );
        float2 r = ( G * sin( G * texCoord.xy ) );
        return ( frac( r.x * r.y * ( 1.0 + texCoord.x ) ) );
    }


    float hash21( float2 p )
    {
        p = 50.0 * frac( p * 0.3183099 + float2( 0.71 , 0.113 ) );
        return frac( p.x * p.y * ( p.x + p.y ) );
    }


    float LinearEyeDepth( float z )
    {
        return rcp( _ZBufferParams.z * z + _ZBufferParams.w );
    }


    // #include "Assets/Resources/Shaders/Chunks/snoise.cginc"
    //#include "Assets/Resources/Shaders/Chunks/noise.cginc"

    struct AttributesDefault
    {
        float3 vertex : POSITION;
    };

    struct VaryingsDefault
    {
        float4 vertex : SV_POSITION;
        float2 texcoord : TEXCOORD0;
        float4 sceenPos : TEXCOORD1;
    };

    // Vertex manipulation
    float2 TransformTriangleVertexToUV( float2 vertex )
    {
        float2 uv = ( vertex + 1.0 ) * 0.5;
        return uv;
    }


    VaryingsDefault VertDefault( AttributesDefault v )
    {
        VaryingsDefault o;
        o.vertex   = float4( v.vertex.xy , 0.0 , 1.0 );
        o.texcoord = ( v.vertex.xy + 1.0 ) * 0.5;

        #if UNITY_UV_STARTS_AT_TOP
    o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
        #endif


        return o;
    }

    samplerCUBE _Skybox;

    float4 Frag( VaryingsDefault v ) : SV_Target
    {

        float3 ro = _WorldSpaceCameraPos;

        float3 rd    = GetRayDirection( v.texcoord );
        float2 uvR   = v.texcoord;
        float4 color = tex2D( _MainTex , uvR );
        float  depth = tex2D( _CameraDepthTexture , uvR ).r;

        float distance = LinearEyeDepth( depth );

        float value = pow( saturate( ( distance - _FogStart ) / ( _FogEnd - _FogStart ) ) , 1 );
        // value              = 1 - exp2( -distance * .0003 );
        float4 colorValue  = lerp( _FogStartColor , _FogEndColor , value );
        colorValue         = lerp( colorValue , texCUBE( _Skybox , rd ) , _SkyboxImportance );
        float clampedValue = clamp( value , 0 , _FogAmount );
        color              = lerp( color * lerp( 1 , colorValue , clampedValue ) , colorValue , clampedValue ); //value; //float4( 1 , 0 , 0 , 1 );


        // color = texCUBE( _Skybox , rd );
        // color = colorValue;



        return color;
    }
    ENDCG
























    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            ENDCG
        }
    }













}