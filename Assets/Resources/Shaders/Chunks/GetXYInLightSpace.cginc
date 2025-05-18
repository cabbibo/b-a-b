float2 GetXYInLightSpace( float3 worldPos )
{

    // this is our x value
    float distTowardLight = dot( worldPos , normalize( float3( 1 , 1 , 0 ) ) );


    //   float distTowardUp = dot( worldPos  , normalize(cross( cross(_WorldSpaceLightPos0, float3(0,1,0)), _WorldSpaceLightPos0)));
    float distTowardUp = dot( worldPos , float3( 0 , 1 , 0 ) );

    // get a perpentdicular value
    float3 perp             = cross( worldPos , _WorldSpaceLightPos0 );
    float  distTowardCamera = dot( perp , float3( 0 , 1 , 0 ) );



    return float2( distTowardLight , distTowardUp );
}
