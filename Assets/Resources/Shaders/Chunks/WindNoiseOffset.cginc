float3 _WindDirection;
float  _WindAmount;
float  _WindChangeSpeed;
float  _WindChangeSize;

float3 GetWindOffset( int id , float3 pos )
{

    float3 windDirection = float3( 1 , 0 , 0 );
    float  flooredTime   = floor( _Time.y * _WindChangeSpeed + float( id ) * .4 );
    float3 noiseVal      = hash33_float3( pos * _WindChangeSize + windDirection * flooredTime );

    float distanceMultiplier = length( pos - _WorldSpaceCameraPos ) / 1000;
    return _WindDirection * noiseVal * _WindAmount * distanceMultiplier; //windAmount;

}
