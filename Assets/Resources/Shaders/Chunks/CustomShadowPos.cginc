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
