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

float _NormalMapStrength;


float3 _WrenPos;


void ClipHoles( float2 uv )
{
    float hole = tex2D( _TerrainHolesTexture , uv ).r;
    clip( hole == 0.0f ? -1 : 1 );
}


void SplatmapMix( varyings IN , half4 defaultAlpha , out half4 splat_control , out half weight , out fixed4 mixedDiffuse , inout fixed3 mixedNormal )
{
    ClipHoles( IN.tc.xy );


    // adjust splatUVs so the edges of the terrain tile lie on pixel centers
    float2 splatUV = ( IN.tc.xy * ( _Control_TexelSize.zw - 1.0f ) + 0.5f ) * _Control_TexelSize.xy;
    splat_control  = tex2D( _Control , splatUV );
    weight         = dot( splat_control , half4( 1 , 1 , 1 , 1 ) );



    // Normalize weights before lighting and restore weights in final modifier functions so that the overal
    // lighting result can be correctly weighted.
    splat_control /= ( weight + 1e-3f );

    float2 uvSplat0 = TRANSFORM_TEX( IN.tc.xy , _Splat0 );
    float2 uvSplat1 = TRANSFORM_TEX( IN.tc.xy , _Splat1 );
    float2 uvSplat2 = TRANSFORM_TEX( IN.tc.xy , _Splat2 );
    float2 uvSplat3 = TRANSFORM_TEX( IN.tc.xy , _Splat3 );

    mixedDiffuse = 0.0f;
    mixedDiffuse += splat_control.r * tex2D( _Splat0 , uvSplat0 ) * half4( 1.0 , 1.0 , 1.0 , defaultAlpha.r );
    mixedDiffuse += splat_control.g * tex2D( _Splat1 , uvSplat1 ) * half4( 1.0 , 1.0 , 1.0 , defaultAlpha.g );
    mixedDiffuse += splat_control.b * tex2D( _Splat2 , uvSplat2 ) * half4( 1.0 , 1.0 , 1.0 , defaultAlpha.b );
    mixedDiffuse += splat_control.a * tex2D( _Splat3 , uvSplat3 ) * half4( 1.0 , 1.0 , 1.0 , defaultAlpha.a );

    mixedNormal = UnpackNormalWithScale( tex2D( _Normal0 , uvSplat0 ) , _NormalScale0 ) * splat_control.r;
    mixedNormal += UnpackNormalWithScale( tex2D( _Normal1 , uvSplat1 ) , _NormalScale1 ) * splat_control.g;
    mixedNormal += UnpackNormalWithScale( tex2D( _Normal2 , uvSplat2 ) , _NormalScale2 ) * splat_control.b;
    mixedNormal += UnpackNormalWithScale( tex2D( _Normal3 , uvSplat3 ) , _NormalScale3 ) * splat_control.a;
    mixedNormal.z += 1e-5f; // to avoid nan after normalizing


    float3 geomNormal = IN.nor; //normalize(tex2D(_TerrainNormalmapTexture, IN.tc.zw).xyz * 2 - 1);

    float3 geomTangent   = normalize( cross( geomNormal , float3( 0 , 0 , 1 ) ) );
    float3 geomBitangent = normalize( cross( geomTangent , geomNormal ) );
    mixedNormal          = mixedNormal.x * geomTangent
        + mixedNormal.y * geomBitangent
        + mixedNormal.z * IN.nor;

}


float2 rotateUV( float amount , float2 uv )
{
    float    sinX           = sin( amount );
    float    cosX           = cos( amount );
    float    sinY           = sin( amount );
    float2x2 rotationMatrix = float2x2( cosX , -sinX , sinY , cosX );

    return mul( rotationMatrix , uv );;


}

float3 brightnessContrast( float3 value , float brightness , float contrast )
{
    return ( value - 0.5 ) * contrast + 0.5 + brightness;
}


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


struct LightingData
{
    float4 color;
    float3 pos;
    float3 nor;
    float3 eye;
    float2 uv;
    float  shadow;
    float  biomeWeights[ 8 ];
    float3 waterflow;
    float  ao;
    float4 terrainData;
};


LightingData GetLightingData( varyings v )
{

    LightingData data;
    data.shadow   = UNITY_SHADOW_ATTENUATION( v , v.worldPos );
    data.pos      = v.worldPos;
    data.color    = tex2D( _TextureMap , v.uv );
    float4 biome1 = tex2D( _BiomeMap1 , v.uv );
    float4 biome2 = tex2D( _BiomeMap2 , v.uv );



    data.biomeWeights[ 0 ] = biome1.x;
    data.biomeWeights[ 1 ] = biome1.y;
    data.biomeWeights[ 2 ] = biome1.z;
    data.biomeWeights[ 3 ] = biome1.w;
    data.biomeWeights[ 4 ] = biome2.x;
    data.biomeWeights[ 5 ] = biome2.y;
    data.biomeWeights[ 6 ] = biome2.z;
    data.biomeWeights[ 7 ] = biome2.w;


    data.waterflow   = tex2D( _WaterflowMap , v.uv ).xyz;
    data.waterflow.z = clamp( abs( data.waterflow.x - .21 ) * abs( data.waterflow.y - .212 ) * 100 , 0 , 1 );


    float4 nAO = tex2D( _NormalsAndAOMap , v.uv );

    // Do Terrain Normal Mapping here
    data.nor = v.nor + nAO.x * float3( 1 , 0 , 0 ) * _NormalMapStrength + nAO.y * float3( 0 , 0 , 1 ) * _NormalMapStrength;
    data.nor = normalize( data.nor );

    data.ao          = nAO.w;
    data.terrainData = tex2D( _DataTexture , v.uv );
    data.uv          = v.uv;
    data.eye         = v.eye;

    return data;




}
