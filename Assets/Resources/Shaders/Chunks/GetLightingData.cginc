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
