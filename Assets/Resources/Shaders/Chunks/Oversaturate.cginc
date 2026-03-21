float3 oversaturate( float3 rgb , float adjustment )
{
    // The coefficients for calculating relative luminance (intensity) based on WCAG
    //.
    const float3 W = float3( 0.2125 , 0.7154 , 0.0721 );

    // Calculate the intensity (grayscale value) of the color
    float3 intensity = dot( rgb , W );

    // Linearly interpolate between the intensity and the original color
    // If adjustment > 1.0, it extrapolates and increases saturation
    return lerp( intensity , rgb , adjustment );
}
