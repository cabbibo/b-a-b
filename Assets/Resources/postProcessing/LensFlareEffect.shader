Shader "PostProcessing/LensFlareEffect"
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

    float _SunDogIntensity;
    float _SunDogSpacing;


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
    #include "Assets/Resources/Shaders/Chunks/noise.cginc"
    #include "Assets/Resources/Shaders/Chunks/zucconi.cginc"

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
    float3      _SunDirection;
    float3      _SunPosition;

    sampler2D _HexTexture;

    float _GhostMultiplier;
    float _FlareMultiplier;
    float _HexMultiplier;
    float _HaloMultiplier;

    float3 lensflare( float2 uv , float2 pos )
    {
        float2 main = uv - pos;
        float2 uvd  = uv * ( length( uv ) );

        float ang  = atan2( main.x , main.y );
        float dist = length( main );
        dist       = pow( dist , .1 );
        float n    = noise( float3( ang * 16.0 , dist * 32.0 , 0 ) );



        float f1 = max( 0.01 - pow( length( uv + 1.2 * pos ) , 1.9 ) , .0 ) * 7.0;

        float f2  = max( 1.0 / ( 1.0 + 32.0 * pow( length( uvd + 0.8 * pos ) , 2.0 ) ) , .0 ) * 00.25;
        float f22 = max( 1.0 / ( 1.0 + 32.0 * pow( length( uvd + 0.85 * pos ) , 2.0 ) ) , .0 ) * 00.23;
        float f23 = max( 1.0 / ( 1.0 + 32.0 * pow( length( uvd + 0.9 * pos ) , 2.0 ) ) , .0 ) * 00.21;

        float2 uvx = lerp( uv , uvd , -0.5 );

        float f4  = max( 0.01 - pow( length( uvx + 0.4 * pos ) , 2.4 ) , .0 ) * 6.0;
        float f42 = max( 0.01 - pow( length( uvx + 0.45 * pos ) , 2.4 ) , .0 ) * 5.0;
        float f43 = max( 0.01 - pow( length( uvx + 0.5 * pos ) , 2.4 ) , .0 ) * 3.0;

        uvx = lerp( uv , uvd , -.4 );

        float f5  = max( 0.01 - pow( length( uvx + 0.2 * pos ) , 5.5 ) , .0 ) * 2.0;
        float f52 = max( 0.01 - pow( length( uvx + 0.4 * pos ) , 5.5 ) , .0 ) * 2.0;
        float f53 = max( 0.01 - pow( length( uvx + 0.6 * pos ) , 5.5 ) , .0 ) * 2.0;

        uvx = lerp( uv , uvd , -0.5 );

        float f6  = max( 0.01 - pow( length( uvx - 0.3 * pos ) , 1.6 ) , .0 ) * 6.0;
        float f62 = max( 0.01 - pow( length( uvx - 0.325 * pos ) , 1.6 ) , .0 ) * 3.0;
        float f63 = max( 0.01 - pow( length( uvx - 0.35 * pos ) , 1.6 ) , .0 ) * 5.0;

        float3 c = 0;;

        c.r += f2 + f4 + f5 + f6;
        c.g += f22 + f42 + f52 + f62;
        c.b += f23 + f43 + f53 + f63;
        c = c * 1.3 - length( uvd ) * .02;
        c *= c;

        return c;
    }


    float3 extraRays( float2 uv , float2 pos )
    {
        float2 main = uv - pos;
        float2 uvd  = uv * ( length( uv ) );

        float ang  = atan2( main.x , main.y );
        float dist = length( main );
        dist       = pow( dist , .1 );
        float n    = noise( float3( ang * 16.0 , dist * 32.0 , 0 ) );

        float f0 = 1.0 / ( length( uv - pos ) * 16.0 + 1.0 );

        f0 = f0 + f0 * ( sin( noise( sin( ang * 2. + pos.x ) * 4.0 - cos( ang * 3. + pos.y ) ) * 16. ) * .1 + dist * .1 + .8 );


        return f0 * f0;

    }


    float3 sunDog( float2 uv )
    {


        float tightness = 100.;
        tightness *= abs( uv.y );
        tightness *= pow( abs( uv.x ) , .4 );
        float fullVal = length( uv ) * tightness - tightness * .3;

        float midBright = ( .5 - abs( fullVal - .01 ) ) * 2.;



        float3 col = 0;
        // col.x = length( uv) * 2. - .3;

        // col.y = clamp(col.x +1.,0.,1.);


        //col.x = midBright;// * midBright;
        float val = midBright; //* fullVal * 10.;

        float secondTight = 2.0;
        val *= length( uv ) * secondTight - secondTight * .2;
        val /= 5. * pow( length( uv ) , 2. );

        //col.y = fullVal;

        col = zucconi( clamp( 1.2 - 1. * abs( fullVal * 2 + .5 ) + .2 , 0 , .8 ) ) * val * 3.;

        //col = 

        if ( fullVal <= 0. || fullVal >= 1. )
        {
            //col = vec3(0.);
        }

        return col;




    }

    float2 brightness( float2 uv , float tightness , float radius )
    {


        float val = 0;

        val = ( .01 / ( .001 + abs( uv.y ) ) + .01 / ( .001 + abs( uv.x ) ) ) * .001 / ( .0001 + pow( ( 1. * ( length( uv ) - radius ) ) , 2. ) ) - 1. * pow( length( uv ) , 2. );

        float outwards = clamp( tightness * ( length( uv ) - radius * .9 ) , 0. , 1. );
        val *= outwards; //clamp(10. * (length(uv)-.3),0.,-1.);


        return float2( val , outwards );


    }

    float3 sunDog( float2 uv , float tightness , float radius , float match )
    {
        float2 vals = brightness( uv , tightness , radius );
        return ( match + .1 ) * pow( saturate( vals.x ) , 1 ) * ( zucconi( 1 - vals.y + match * .1 ) * .7 + .3 );
    }

    float3 hexes( float2 sunUV , float2 localSunUV , float2 fUV )
    {
        float3 col = 0;
        for ( int i = 0; i < 20; i++ )
        {

            float  v     = ( sin( float( i ) * 1332. ) + sin( float( i ) * 14.14 ) ) * 2;
            float2 hexUV = saturate( sunUV + localSunUV * v + fUV * ( 2 + sin( float( i ) * 32.31 ) ) * 1 + .5 );
            col.xyz += ( 1 - tex2D( _HexTexture , hexUV ) ).x * .1 * ( zucconi( abs( sin( float( i ) * 145.55 + length( localSunUV ) * .2 ) ) ) * .8 + .2 );
            //col *= match;
        }

        return col;
    }

    float _CameraInShadowLerped;

    float4 Frag( VaryingsDefault v ) : SV_Target
    {

        float3 ro = _WorldSpaceCameraPos;

        float3 rd    = GetRayDirection( v.texcoord );
        float2 uvR   = v.texcoord;
        float  depth = tex2D( _CameraDepthTexture , uvR ).r;

        float distance = LinearEyeDepth( depth );
        /* 
              float value = pow( saturate( ( distance - _FogStart ) / ( _FogEnd - _FogStart ) ) , 1 );
              // value              = 1 - exp2( -distance * .0003 );
              float4 colorValue  = lerp( _FogStartColor , _FogEndColor , value );
              colorValue         = lerp( colorValue , texCUBE( _Skybox , rd ) , _SkyboxImportance );
              float clampedValue = clamp( value , 0 , _FogAmount );
              color              = lerp( color * lerp( 1 , colorValue , clampedValue ) , colorValue , clampedValue ); //value; //float4( 1 , 0 , 0 , 1 );
      */
        float4 color    = 0;
        float3 camPos   = _WorldSpaceCameraPos;
        float3 lightPos = camPos - _SunDirection * 1000;


        float2 fUV = v.texcoord - .5;
        fUV *= 2;
        fUV.x *= _ScreenParams.x / _ScreenParams.y;



        float4 col = 0;;

        float4 clipPos = mul( UNITY_MATRIX_VP , float4( _SunPosition , 1.0 ) );
        float2 sun01   = clipPos.xy / clipPos.w * 0.5 + 0.5;
        float2 sunUV   = ( sun01 - 0.5 ) * 2.0;

        sunUV.x *= _ScreenParams.x / _ScreenParams.y;

        sunUV.y = -sunUV.y;

        bool isBehind = clipPos.w <= 0.0;

        float3 lens = 1 * lensflare( fUV , sunUV );

        // color = texCUBE( _Skybox , rd );
        // color = colorValue;






        float2 localSunUV = fUV - sunUV;

        float match = saturate( dot( normalize( localSunUV ) , normalize( fUV ) ) );

        match *= match;



        float ring = length( localSunUV * pow( abs( localSunUV.y ) , .2 ) );
        ring       = sin( ring * 100 ) * max( 1 - 10 * abs( ring - .5 ) , 0 );
        //ring 
        //ring -= .1*pow(  saturate(1-abs(  localSunUV.y ) ),10);

        //float sunDog = saturate( ring * 4 + 1 );


        //sunDog = zucconi( sunDog );
        //col.xyz		 += _DogMultiplier*match* (.5-abs(  sunDog -.5))* zucconi( 1-sunDog );

        if ( !isBehind )
        {
            float2 vals = brightness( localSunUV , 1.0 , .4 );
            col.xyz += ( match + .1 ) * _SunDogIntensity * pow( saturate( vals.x ) , 1 ) * ( zucconi( 1 - vals.y + match * .1 ) * .7 + .3 ); // (match*.4+.2)* _DogMultiplier*saturate(  sunDog(localSunUV * .5));


            for ( int i = 0; i < 20; i++ )
            {

                float  v     = ( sin( float( i ) * 1332. ) + sin( float( i ) * 14.14 ) ) * 2;
                float2 hexUV = saturate( sunUV + localSunUV * v + fUV * ( 2 + sin( float( i ) * 32.31 ) ) * 1 + .5 );
                col.xyz += ( 1 - tex2D( _HexTexture , hexUV ) ).x * .1 * ( zucconi( abs( sin( float( i ) * 145.55 + length( localSunUV ) * .2 ) ) ) * .8 + .2 );
                //col *= match;
            }
        }



        float3 hexColor      = 0;
        float3 ghostColor    = 0;
        float3 sundogColor   = 0;
        float3 extraRayColor = 0;


        // Sundog
        sundogColor = sunDog( localSunUV , 1.0 , .4 , match );
        // hexes
        hexColor = hexes( sunUV , localSunUV , fUV );
        // extraRays
        extraRayColor = extraRays( fUV , sunUV );
        // ghost Color;
        ghostColor = lensflare( fUV , sunUV );


        color = tex2D( _MainTex , v.texcoord );

        float3 extraCol = 0;
        extraCol.xyz += hexColor * _HexMultiplier;
        extraCol.xyz += ghostColor * _GhostMultiplier;

        if ( distance > 5000 )
        {
            extraCol.xyz += extraRayColor * _FlareMultiplier;
            extraCol.xyz += sundogColor * _HaloMultiplier;
        }

        extraCol *= 1 - _CameraInShadowLerped;

        extraCol *= saturate( clipPos.w / 50000 );

        color.xyz += extraCol;



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