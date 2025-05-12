Shader "PostProcessing/SketchEffect"
{



    CGINCLUDE
    sampler2D _MainTex;
    sampler2D _PaintMap;
    sampler2D _DepthTex;
    sampler2D _CameraDepthTexture;
    sampler2D _HeightMap;


    //#include "UnityLightingCommon.cginc"


    //samplerCube _Cubemap;
    float _Intensity;

    float _StartDistance;
    float _EndDistance;

    float4x4 _InverseProjection;
    float4x4 _InverseView;

    float4x4 _InverseViewProjection;

    float4 _LightColor0;

    /* transform uv -> NDC
    
    then create clipspacePos =[NDCx, NDCy, -1, 1]
    
    viewPos = invPorjectMtarix * clipspacePos
    worldPos = invViewMatrix * viewPos
    
    rayDir = worldPos-cameraPos*/

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

    float3 _MapSize;
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

    const float e = 2.7182818284590452353602874713527;

    float staticNoise( float2 texCoord )
    {
        //float G = e + (_Time.y * 0.00001+1000);
        float  G = e + ( 0.00001 + 10 );
        float2 r = ( G * sin( G * texCoord.xy ) );
        return ( frac( r.x * r.y * ( 1.0 + texCoord.x ) ) );
    }


    // TODO NEED OFFSET!
    float getTerrainHeight( float3 p )
    {
        float2 samplePosition = p.xz - _MapOffset.xz;
        float2 uv             = ( samplePosition + _MapSize.xz / 2 ) / _MapSize.xz;

        float h = tex2D( _HeightMap , uv ) * _MapSize.y * 2;

        return h;

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


    #include "Assets/Resources/Shaders/Chunks/SunShadows.cginc"
    #include "Assets/Resources/Shaders/Chunks/snoise.cginc"
    #include "Assets/Resources/Shaders/Chunks/noise.cginc"
    #include "Assets/Resources/Shaders/Chunks/triNoise3D.cginc"
    #include "Assets/Resources/Shaders/Chunks/rotateUV.cginc"

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


    float3 GetWorldPos( float2 uv )
    {

        float3 ro = _WorldSpaceCameraPos;

        // float3 rd = GetRayDirection( uv );
        float3 viewVector = mul( _InverseProjection , float4( uv.x * 2 - 1 , uv.y * 2 - 1 , 0 , 1 ) );
        viewVector        = mul( _InverseView , viewVector ).xyz;

        float depth = tex2D( _CameraDepthTexture , uv ).r;
        return ro + normalize( viewVector ) * LinearEyeDepth( depth );

    }

    float3 GetNormal( float2 uv )
    {
        float  eps = .0006;
        float3 nl  = GetWorldPos( uv + float2( eps , 0 ) );
        float3 nr  = GetWorldPos( uv - float2( eps , 0 ) );
        float3 nt  = GetWorldPos( uv + float2( 0 , eps ) );
        float3 nb  = GetWorldPos( uv - float2( 0 , eps ) );

        float3 n = cross( ( nr - nl ) * 1 , ( nt - nb ) * 1 ) * 10;

        return normalize( n );
    }

    float3 blurredPosition( float2 uv )
    {

        float  eps    = .001;
        float3 center = GetWorldPos( uv );
        float3 left   = GetWorldPos( uv + float2( -eps , 0 ) );
        float3 right  = GetWorldPos( uv + float2( eps , 0 ) );
        float3 up     = GetWorldPos( uv + float2( 0 , -eps ) );
        float3 down   = GetWorldPos( uv + float2( 0 , eps ) );
        float3 blur   = ( center * 3 + left + right + up + down ) / 7;

        return blur;

    }


    float4 Frag( VaryingsDefault v ) : SV_Target
    {

        float3 ro = _WorldSpaceCameraPos;

        float3 rd = GetRayDirection( v.texcoord );

        float2 uvR   = v.texcoord;
        float4 color = tex2D( _MainTex , uvR );
        float4 bgCol = color;
        float  depth = tex2D( _CameraDepthTexture , uvR ).r;

        float3 viewVector = mul( _InverseProjection , float4( v.texcoord.x * 2 - 1 , v.texcoord.y * 2 - 1 , 0 , 1 ) );
        viewVector        = mul( _InverseView , viewVector ).xyz;

        float distance = LinearEyeDepth( depth );

        float  totalFog      = 0;
        float4 totalFogColor = 0;


        //  fixed4 cascadeWeights = GET_CASCADE_WEIGHTS( worldPos.xyz + triNoise3D( worldPos * .03 , 1,_Time.y ) * 100 , 0 );



        float offsetPaint = tex2D( _PaintMap , v.texcoord * 3 ).r;


        float4 worldPos       = float4( GetWorldPos( v.texcoord ) , 1 );
        float4 p              = worldPos;
        fixed4 cascadeWeights = GET_CASCADE_WEIGHTS( worldPos.xyz , 0 );

        // fixed4 cascadeWeights = GET_CASCADE_WEIGHTS( p.xyz , 0 );

        float sVal = unity_sampleShadowmap( GET_SHADOW_COORDINATES( float4(p.xyz, 1) , cascadeWeights ) );

        // float4 worldPos          = float4( ro + rd * distance , 1 );
        float shadowAttenuation = GetSunShadowsAttenuation_PCF5x5( worldPos , 100 , -10 ).x;

        float offset = _FogStepSize * staticNoise( v.texcoord + _Time.y % 1 );


        color.xyz = ( ( 1 - sVal )
            )
            * bgCol + sVal; // + bgCol; //float4( 1 , 0 , 0 , 1 );

        float dotVal = dot( _WorldSpaceLightPos0.xyz , GetNormal( v.texcoord + offsetPaint * .02 ) );

        color.xyz = -2 * bgCol * dotVal * dotVal * dotVal + bgCol * sVal; // GetNormal( v.texcoord );
        color.xyz = GetNormal( v.texcoord + offsetPaint * .02 ); // )

        float delta = length( GetWorldPos( v.texcoord + offsetPaint * .01 * distance * .00001 ) - blurredPosition( v.texcoord + offsetPaint * .01 ) );
        color       = saturate( abs( delta ) * .1 );
        color.xyz *= bgCol;

        color.xyz = GetNormal( v.texcoord );

        float x = dot( GetNormal( v.texcoord ) , float3( 1 , 0 , 0 ) );
        float y = worldPos.y;

        float3 pos = GetWorldPos( v.texcoord );
        float3 nor = GetNormal( v.texcoord );

        float offsetVal = tex2D( _PaintMap , rotateUV( pos.xz * .001 + 100 * sin( floor( _Time.y * 3 ) ) , pos.y * .001 ) ).b;

        offsetVal = tex2D( _PaintMap , rotateUV( v.texcoord * 4 + .8 + floor( _Time.y * 1 ) , .5 ) ).b;
        offsetVal += tex2D( _PaintMap , rotateUV( v.texcoord * 5 + .3 + floor( _Time.y * 1 ) , .7 ) ).g;
        //  offsetVal += tex2D( _PaintMap , rotateUV( v.texcoord * 5 + .3 + floor( _Time.y * 4 ) , -.5 ) ).g;


        float2 offsetUV = rotateUV( v.texcoord , offsetVal * .1 );

        color.r = tex2D( _MainTex , v.texcoord + offsetVal * ( .000 + .002 ) ).r;
        color.g = tex2D( _MainTex , v.texcoord + offsetVal * ( .000 + .003 ) ).g;
        color.b = tex2D( _MainTex , v.texcoord + offsetVal * ( .000 + .004 ) ).b;
        color += color * ( offsetVal * .2 + .8 );
        // color += ( offsetVal * .3 + .8 ) * color;

        float fade = abs( v.texcoord.x - .5 ) * 2;
        fade       = saturate( ( fade - .9 ) * 10 + offsetVal );;
        fade       = max( fade , saturate( ( abs( v.texcoord.y - .5 ) * 2 - .9 ) * 10 + offsetVal ) );;
        // color      = fade;

        color = lerp( color , 1 , fade );

        //  color.xyz = tex2D( _MainTex , offsetUV ).xyz;

        // color.xyz *= 1 + ( offsetVal - .5 ) * .1;
        float d1 = distance;
        float d2 = LinearEyeDepth( tex2D( _CameraDepthTexture , offsetUV ).r );

        // color.xyz -= saturate( abs( d2 - d1 ) * .01 );
        // color.xyz *= offsetVal;


        //color.xyz = tex2D( _PaintMap , rotateUV( pos.y * .001 , pos.xz * .01 ) ) * 100.2 / distance;
        //color.xyz = tex2D( _PaintMap , float2( x , y ) ).xyz;

        //  color.xyz = saturate( sin( length( worldPos.xyz ) * .01 ) );
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