#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "UnityLightingCommon.cginc"
#include "Lighting.cginc"

#include "Assets/Resources/Shaders/Chunks/hsv.cginc"
#include "Assets/Resources/Shaders/Chunks/noise.cginc"
#include "Assets/Resources/Shaders/Chunks/snoise3D.cginc"


uniform float4x4 _Transform;
uniform int      _NumberMeshes;
float3           _SafeCameraPosition;

uniform float3 _Color;
float          _Fade;
float3         _FadeLocation;
sampler2D      _MainTex;


float _PainterlyLightImportance;
float _OverallMultiplier;
float _ShadowStrength;


float  _OutlineAmount;
float4 _OutlineColor;


#include "Assets/Resources/Shaders/Chunks/generic_desaturate.cginc"
#include "Assets/Resources/Shaders/Chunks/hash33.cginc"
#include "Assets/Resources/Shaders/Chunks/WindNoiseOffset.cginc"
#include "Assets/Resources/Shaders/Chunks/GetLightingData.cginc"
#include "Assets/Resources/Shaders/Chunks/triplanarNormal.cginc"
#include "Assets/Resources/Shaders/Chunks/PainterlyLight.cginc"

#include "Assets/Resources/Shaders/Chunks/Structs/BaseInputData.cginc"
#include "Assets/Resources/Shaders/Chunks/Structs/FullVaryingData.cginc"

#include "Assets/Resources/Shaders/Chunks/ShadowCasterPos.cginc"
#include "Assets/Resources/Shaders/Chunks/CustomShadowPos.cginc"

#include "Assets/Resources/Shaders/Chunks/DoWrenDiscard.cginc"
#include "Assets/Resources/Shaders/Chunks/GetXYInLightSpace.cginc"
#include "Assets/Resources/Shaders/Chunks/EdgeDiscards.cginc"
#include "Assets/Resources/Shaders/Chunks/ObjectScale.cginc"


UNITY_INSTANCING_BUFFER_START( Props )
UNITY_INSTANCING_BUFFER_END( Props )

uniform sampler2D _PaintTexture;

FullVaryingData PrepData_UNITY( BaseInputData vert )
{
    FullVaryingData o;

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


    return o;


}


FullVaryingData SetVaryings_UNITY( BaseInputData vert )
{

    FullVaryingData o = PrepData_UNITY( vert );

    UNITY_TRANSFER_SHADOW( o , o.worldPos );
    UNITY_TRANSFER_FOG( o , o.pos );

    return o;

}

FullVaryingData SetVaryingsOutline_UNITY( BaseInputData vert )
{

    FullVaryingData o = PrepData_UNITY( vert );

    o.worldPos += o.nor * _OutlineAmount - 10 * normalize( o.eye ) * _OutlineAmount;
    o.eye = _WorldSpaceCameraPos - o.worldPos;
    o.pos = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );

    UNITY_TRANSFER_SHADOW( o , o.worldPos );
    UNITY_TRANSFER_FOG( o , o.pos );

    return o;

}

FullVaryingData SetShadowVaryings_UNITY( BaseInputData vert )
{

    FullVaryingData o = PrepData_UNITY( vert );


    float3 worldNormal = o.nor;

    float4 opos = 0;
    opos        = CustomShadowPos( float4( o.worldPos , 1 ) , worldNormal );
    opos        = UnityApplyLinearShadowBias( opos );
    o.pos       = opos;


    return o;

}
