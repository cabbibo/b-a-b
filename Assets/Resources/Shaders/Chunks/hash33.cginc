float3 hash33_float3( float3 p )
{
    float3 q = float3( dot( p , float3( 127.1 , 311.7 , 74.7 ) ) ,
                       dot( p , float3( 269.5 , 183.3 , 246.1 ) ) ,
                       dot( p , float3( 113.5 , 271.9 , 124.6 ) ) );
    return frac( sin( q ) * 43758.5453 );
}
