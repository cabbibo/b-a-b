float3 _WrenPos;

float3 _WrenForward;
float3 _WrenHeadForward;
float3 _WrenLerpedForward;
float3 _WrenVel;
float3 _WrenLerpedVel;

float flashlightSpread()
{

    float v = pow( length( _WrenLerpedVel ) , 0.5 );

    return smoothstep( 0.0 , 5.23 , v ) * 0.94;
    //return clamp( pow( length( _WrenLerpedVel ) , .5 ) * .13 , 0 , .94 );
}

float flashlight( float3 worldPos )
{
    float3 dirToWren = ( worldPos - _WrenPos );

    float3 safeVel       = length( _WrenLerpedVel ) > .0001 ? normalize( _WrenLerpedVel ) : _WrenLerpedForward;
    float3 flashlightDir = lerp( _WrenLerpedForward , safeVel , 1 );





    float lightMatch = dot( flashlightDir , normalize( dirToWren ) );


    float val = pow( lightMatch - flashlightSpread() , 1 );





    return val;

}
