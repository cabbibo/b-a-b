// Upgrade NOTE: replaced 'defined #HSV' with 'defined (#HSV)'


float3 calcSparkles( float3 pos , float3 eye )
{
    float3 sparkleCol = 0;

    for ( int i = 0; i < 3; i++ )
    {

        float3 fPos = pos - normalize( eye ) * float( i ) * .6;
        float  v    = ( snoise( fPos * 50 ) + 1 ) / 2;
        sparkleCol += hsv( (float)i / 3 , 1 , v );


    }

    sparkleCol *= sparkleCol;
    sparkleCol *= sparkleCol;
    sparkleCol *= sparkleCol;
    sparkleCol *= sparkleCol;

    sparkleCol = length( sparkleCol ) * ( sparkleCol * .8 + .3 ) * 10; //
    sparkleCol /= clamp( ( .1 + .1 * length( eye ) ) , 1 , 3 );
    return sparkleCol;
}
