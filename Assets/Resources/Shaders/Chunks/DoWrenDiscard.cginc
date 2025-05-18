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
