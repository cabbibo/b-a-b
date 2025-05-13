// Generic algorithm to desaturate images used in most game engines
float3 generic_desaturate( float3 color , float factor )
{
    float3 lum  = float3( 0.299 , 0.587 , 0.114 );
    float3 gray = dot( lum , color );
    return lerp( color , gray , factor );
}
