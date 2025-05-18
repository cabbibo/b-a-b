void DoEdgeDiscard( LightingData lightingData , float3 worldPos , float3 eye )
{

    float discardValue = lightingData.normalMatch - ( snoise( worldPos * 4.4 ) + 1 ) * .5;
    discardValue       = lerp( discardValue , 1 , saturate( length( eye ) * .003 ) );

    if ( discardValue < 0 )
    {
        discard;
    }

}
