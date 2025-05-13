#include "Lighting.cginc"
#include "Assets/Resources/Shaders/Chunks/hsv.cginc"
#include "Assets/Resources/Shaders/Chunks/noise.cginc"
#include "Assets/Resources/Shaders/Chunks/snoise3D.cginc"


float3 hash33_float3( float3 p )
{
    float3 q = float3( dot( p , float3( 127.1 , 311.7 , 74.7 ) ) ,
                       dot( p , float3( 269.5 , 183.3 , 246.1 ) ) ,
                       dot( p , float3( 113.5 , 271.9 , 124.6 ) ) );
    return frac( sin( q ) * 43758.5453 );
}

uniform float4x4 _Transform;
uniform int      _NumberMeshes;
float3           _WindDirection;
float            _WindAmount;
float            _WindChangeSpeed;
float            _WindChangeSize;
float3           _SafeCameraPosition;

float3 GetWindOffset( int id , float3 pos )
{

    float3 windDirection = float3( 1 , 0 , 0 );
    float  flooredTime   = floor( _Time.y * _WindChangeSpeed + float( id ) * .4 );
    float3 noiseVal      = hash33_float3( pos * _WindChangeSize + windDirection * flooredTime );

    float distanceMultiplier = length( pos - _WorldSpaceCameraPos ) / 1000;
    return _WindDirection * noiseVal * _WindAmount * distanceMultiplier; //windAmount;

}


struct LightingData
{
    float3 flatNormal;
    float  flatNormalMatch;
    float  normalMatch;
    float  lightMatch;
    float  flatLightMatch;
    float  eyeMatch;
    float  reflectionMatch;
};

void GetLightingData( float3 worldPos , float3 eye , float3 nor , float3 lightDir , out LightingData lightingData )
{

    float3 ddxW             = ddx( worldPos );
    float3 ddyW             = ddy( worldPos );
    lightingData.flatNormal = -normalize( cross( ddxW , ddyW ) );

    lightingData.flatNormalMatch = saturate( dot( normalize( eye ) , lightingData.flatNormal ) );
    lightingData.normalMatch     = saturate( dot( normalize( eye ) , nor ) );

    lightingData.lightMatch     = saturate( dot( normalize( lightDir ) , nor ) );
    lightingData.flatLightMatch = saturate( dot( normalize( lightDir ) , lightingData.flatNormal ) );

    lightingData.eyeMatch        = saturate( dot( normalize( eye ) , nor ) );
    lightingData.reflectionMatch = saturate( dot( normalize( -lightDir ) , reflect( normalize( eye ) , nor ) ) );



}


void DoEdgeDiscard( LightingData lightingData , float3 worldPos , float3 eye )
{


    float discardValue = lightingData.normalMatch - ( snoise( worldPos * 4.4 ) + 1 ) * .5;
    discardValue       = lerp( discardValue , 1 , saturate( length( eye ) * .003 ) );

    if ( discardValue < 0 )
    {
        discard;
    }

}

sampler2D _NormalMap;
float3    _TriplanarMultiplier;
float     _TriplanarSharpness;
float     _TriplanarNormalWeight;


float3 triplanarNormal( float3 p , float3 n , float3 tspace0 , float3 tspace1 , float3 tspace2 , float offset )
{

    // UDN blend
    // Triplanar uvs
    float2 uvX = p.zy * _TriplanarMultiplier + offset; // x facing plane
    float2 uvY = p.xz * _TriplanarMultiplier + offset; // y facing plane
    float2 uvZ = p.xy * _TriplanarMultiplier + offset; // z facing plane

    // Tangent space normal maps
    half3 tnormalX = UnpackNormal( tex2D( _NormalMap , uvX % 1 ) );
    half3 tnormalY = UnpackNormal( tex2D( _NormalMap , uvY % 1 ) );
    half3 tnormalZ = UnpackNormal( tex2D( _NormalMap , uvZ % 1 ) );

    // Swizzle world normals into tangent space and apply UDN blend.
    // These should get normalized, but it's very a minor visual
    // difference to skip it until after the blend.
    tnormalX = normalize( half3( tnormalX.xy * _TriplanarNormalWeight + n.zy , n.x ) );
    tnormalY = normalize( half3( tnormalY.xy * _TriplanarNormalWeight + n.xz , n.y ) );
    tnormalZ = normalize( half3( tnormalZ.xy * _TriplanarNormalWeight + n.xy , n.z ) );

    half3 blend = pow( abs( n ) , _TriplanarSharpness );
    // make sure the weights sum up to 1 (divide by sum of x+y+z)
    blend /= dot( blend , 1.0 );

    // Swizzle tangent normals to match world orientation and triblend
    half3 worldNormal = normalize(
        tnormalX.zyx * blend.x +
        tnormalY.xzy * blend.y +
        tnormalZ.xyz * blend.z
    );

    return worldNormal;

}


sampler2D _PainterlyLightMap;

float4 triplanarSample( float3 p , float3 n )
{

    half3 blend = pow( abs( n ) , _TriplanarSharpness );;

    // make sure the weights sum up to 1 (divide by sum of x+y+z)
    blend /= dot( blend , 1.0 );


    float4 cx = tex2D( _PainterlyLightMap , ( p.zy * _TriplanarMultiplier * .3 ) );
    float4 cy = tex2D( _PainterlyLightMap , ( p.xz * _TriplanarMultiplier * .3 ) );
    float4 cz = tex2D( _PainterlyLightMap , ( p.xy * _TriplanarMultiplier * .3 ) );


    // blend the textures based on weights
    fixed4 c = 0;
    c        = cx * blend.x + cy * blend.y + cz * blend.z;
    return c;

}


// painterly
float4 _TextureShadingWeights;

float4 _LowLightColor;
float4 _HighLightColor;

// m = match
float4 PainterlyColor( float3 pos , float3 nor , float m , float2 uv )
{


    // float4 p = triplanarSample( pos , nor );
    float4 p = tex2D( _PainterlyLightMap , uv * 3 );


    float4 weights = 0;
    if ( m < _TextureShadingWeights.x )
    {
        weights = float4( 1 , 0 , 0 , 0 );
    }
    else if ( m >= _TextureShadingWeights.x && m < _TextureShadingWeights.y )
    {
        weights = float4( 1 - ( m - _TextureShadingWeights.x ) / ( _TextureShadingWeights.y - _TextureShadingWeights.x ) , ( m - _TextureShadingWeights.x ) / ( _TextureShadingWeights.y - _TextureShadingWeights.x ) , 0 , 0 ); //lerp( p.x , p.y , m );
    }
    else if ( m >= _TextureShadingWeights.y && m < _TextureShadingWeights.z )
    {
        weights = float4( 0 , 1 - ( m - _TextureShadingWeights.y ) / ( _TextureShadingWeights.z - _TextureShadingWeights.y ) , ( m - _TextureShadingWeights.y ) / ( _TextureShadingWeights.z - _TextureShadingWeights.y ) , 0 );
    }
    else if ( m >= _TextureShadingWeights.z && m < _TextureShadingWeights.w )
    {
        weights = float4( 0 , 0 , 1 - ( m - _TextureShadingWeights.z ) / ( _TextureShadingWeights.w - _TextureShadingWeights.z ) , ( m - _TextureShadingWeights.z ) / ( _TextureShadingWeights.w - _TextureShadingWeights.z ) );
    }
    else
    {
        weights = float4( 0 , 0 , 0 , 1 );
    }

    float4 fLCol = p.x * weights.x;
    fLCol += p.y * weights.y;
    fLCol += p.z * weights.z;
    fLCol += p.w * weights.w;
    //fLCol = 1-fLCol;

    // fLCol = lerp( float4(1,.9,.6,1) * .7 + .1 , (float4(.4,.5,.8,1) * .8 + .2) * .3, 1-fLCol);
    fLCol = lerp( _HighLightColor , _LowLightColor , 1 - pow( fLCol , 4 ) );

    return fLCol;
}

#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "UnityLightingCommon.cginc"


struct inputData
{
    float4 vertex : POSITION;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
    fixed4 color : COLOR;

    uint id : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


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
    int    feather:TEXCOORD7;
    float4 data1:TEXCOORD9;

    float3 tangent : TEXCOORD12;
    float3 tspace0 : TEXCOORD13;
    float3 tspace1 : TEXCOORD14;
    float3 tspace2 : TEXCOORD15;
    float  offsetAmount : TEXCOORD16;
    uint   instanceID : SV_InstanceID;

    // UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.

    UNITY_SHADOW_COORDS( 8 )
    UNITY_FOG_COORDS( 10 )
};


#include "Assets/Resources/Shaders/Chunks/ShadowCasterPos.cginc"

float4 CustomShadowPos( float4 vertex , float3 normal )
{
    float4 wPos    = vertex;
    float3 wNormal = normal;

    if ( unity_LightShadowBias.z != 0.0 )
    {

        float3 wLight = normalize( UnityWorldSpaceLightDir( wPos.xyz ) );

        // apply normal offset bias (inset position along the normal)
        // bias needs to be scaled by sine between normal and light direction
        // (http://the-witness.net/news/2013/09/shadow-mapping-summary-part-1/)
        //
        // unity_LightShadowBias.z contains user-specified normal offset amount
        // scaled by world space texel size.

        float shadowCos  = dot( wNormal , wLight );
        float shadowSine = sqrt( 1 - shadowCos * shadowCos );
        float normalBias = unity_LightShadowBias.z * shadowSine;

        wPos.xyz -= wNormal * normalBias * 10;
    }

    return mul( UNITY_MATRIX_VP , wPos );
}


UNITY_INSTANCING_BUFFER_START( Props )
UNITY_INSTANCING_BUFFER_END( Props )


float2 GetXYInLightSpace( float3 worldPos )
{

    // this is our x value
    float distTowardLight = dot( worldPos , normalize( float3( 1 , 1 , 0 ) ) );


    //   float distTowardUp = dot( worldPos  , normalize(cross( cross(_WorldSpaceLightPos0, float3(0,1,0)), _WorldSpaceLightPos0)));
    float distTowardUp = dot( worldPos , float3( 0 , 1 , 0 ) );



    //float


    // get a perpentdicular value
    float3 perp             = cross( worldPos , _WorldSpaceLightPos0 );
    float  distTowardCamera = dot( perp , float3( 0 , 1 , 0 ) );



    return float2( distTowardLight , distTowardUp );
}


uniform sampler2D _PaintTexture;

// Generic algorithm to desaturate images used in most game engines
float3 generic_desaturate( float3 color , float factor )
{
    float3 lum  = float3( 0.299 , 0.587 , 0.114 );
    float3 gray = dot( lum , color );
    return lerp( color , gray , factor );
}

float sdCapsule( float3 p , float3 a , float3 b , float r )
{
    float3 pa = p - a, ba = b - a;
    float  h  = clamp( dot( pa , ba ) / dot( ba , ba ) , 0.0 , 1.0 );
    return length( pa - ba * h ) - r;
}

float3 _WrenPos;


void DoWrenDiscard( float3 worldPos )
{

    // Discards around bird!

    float capDistance = sdCapsule( worldPos , _WorldSpaceCameraPos , _WrenPos , 1 );
    capDistance -= snoise( worldPos ) * .2;

    if ( capDistance < 0 )
    {
        discard;
    }
    else
    {
        //col *= saturate(capDistance * 10);
    }


}

half3 ObjectScale()
{
    return half3(
        length( unity_ObjectToWorld._m00_m10_m20 ) ,
        length( unity_ObjectToWorld._m01_m11_m21 ) ,
        length( unity_ObjectToWorld._m02_m12_m22 )
    );
}


varyings SetVaryings( inputData vert )
{
    varyings o;

    UNITY_SETUP_INSTANCE_ID( vert );
    UNITY_TRANSFER_INSTANCE_ID( vert , o ); // necessary only if you want to access instanced properties in the fragment Shader.



    int instanceID = 0;
    #if defined(UNITY_INSTANCING_ENABLED)
                    instanceID = UNITY_GET_INSTANCE_ID(vert);
    #endif

    float3 wPos       = mul( unity_ObjectToWorld , float4( vert.vertex.xyz , 1 ) ).xyz;
    float3 windOffset = GetWindOffset( instanceID , wPos );


    o.worldPos     = wPos + windOffset; //windAmount;
    o.pos          = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );
    o.eye          = _WorldSpaceCameraPos - o.worldPos;
    o.nor          = normalize( mul( unity_ObjectToWorld , float4( vert.normal , 0 ) ).xyz );
    o.uv           = vert.texcoord.xy;
    o.color        = vert.color;
    o.tangent      = vert.tangent.xyz * vert.tangent.w;
    o.offsetAmount = length( windOffset );

    half3 wNormal  = o.nor;
    half3 wTangent = mul( unity_ObjectToWorld , float4( vert.tangent.xyz , 0 ) ).xyz * vert.tangent.w;
    // compute bitangent from cross product of normal and tangent
    //half tangentSign = tangent.w * unity_WorldTransformParams.w;
    half3 wBitangent = cross( wNormal , wTangent ); // * tangentSign;
    // output the tangent space matrix
    o.tspace0 = half3( wTangent.x , wBitangent.x , wNormal.x );
    o.tspace1 = half3( wTangent.y , wBitangent.y , wNormal.y );
    o.tspace2 = half3( wTangent.z , wBitangent.z , wNormal.z );


    UNITY_TRANSFER_SHADOW( o , o.worldPos );
    UNITY_TRANSFER_FOG( o , o.pos );

    return o;

}


uniform float3 _Color;

float _Fade;

float3 _FadeLocation;

sampler2D _MainTex;


float _PainterlyLightImportance;

float _OverallMultiplier;

float _ShadowStrength;


varyings SetShadowVaryings( inputData vert )
{
    varyings o;

    //float4 p = UnityObjectToClipPos(v.vertex);
    float4 p = float4( vert.vertex.xyz , 1 );
    // TRANSFER_SHADOW_CASTER_NOPOS(o,o.pos);


    UNITY_SETUP_INSTANCE_ID( vert );
    UNITY_TRANSFER_INSTANCE_ID( vert , o ); // necessary only if you want to access instanced properties in the fragment Shader.


    int instanceID = 0;
    #if defined(UNITY_INSTANCING_ENABLED)
    instanceID = UNITY_GET_INSTANCE_ID(vert);
    #endif


    o.nor = normalize( mul( unity_ObjectToWorld , float4( vert.normal , 0 ) ).xyz );
    // o.worldPos = worldPos.xyz;

    float3 wPos       = mul( unity_ObjectToWorld , float4( vert.vertex.xyz , 1 ) ).xyz;
    float3 windOffset = GetWindOffset( instanceID , wPos );

    o.worldPos = wPos + windOffset; //windAmount;
    // o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));


    // o.pos = mul (UNITY_MATRIX_VP, float4(worldPos,1.0f));
    o.eye = _WorldSpaceCameraPos - o.worldPos.xyz;
    //float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
    float3 worldNormal = o.nor; // UnityObjectToWorldNormal( vert.normal);

    float4 opos = 0;
    opos        = CustomShadowPos( float4( o.worldPos , 1 ) , worldNormal );
    opos        = UnityApplyLinearShadowBias( opos );
    o.pos       = opos;


    //TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
    return o;

}
