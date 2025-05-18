sampler2D _PainterlyLightMap;
float4    _TextureShadingWeights;
float4    _LowLightColor;
float4    _HighLightColor;


float Painterly( float val , float2 uv )
{


    float4 p = tex2D( _PainterlyLightMap , uv );

    float m = val * 3;
    float f = 0;
    if ( m < 1 )
    {
        f = lerp( p.x , p.y , m );
    }
    else if ( m >= 1 && m < 2 )
    {
        f = lerp( p.y , p.z , m - 1 );
    }
    else if ( m >= 2 && m < 3 )
    {
        f = lerp( p.z , p.w , m - 2 );
    }

    return f;

}


float4 TriplanarPainterlySample( float3 p , float3 n )
{

    half3 blend = pow( abs( n ) , _TriplanarSharpness );;

    // make sure the weights sum up to 1 (divide by sum of x+y+z)
    blend /= dot( blend , 1.0 );


    float4 cx = tex2D( _PainterlyLightMap , ( p.zy * _TriplanarMultiplier * .3 ) );
    float4 cy = tex2D( _PainterlyLightMap , ( p.xz * _TriplanarMultiplier * .3 ) );
    float4 cz = tex2D( _PainterlyLightMap , ( p.xy * _TriplanarMultiplier * .3 ) );


    // blend the textures based on weights
    fixed4 c = 0;
    c        = cx * blend.x + cy * blend.y + cz * blend.z;
    return c;

}


// painterly

float4 PainterlyWeights( float m )
{


    float4 weights = 0;
    if ( m < _TextureShadingWeights.x )
    {
        weights = float4( 1 , 0 , 0 , 0 );
    }
    else if ( m >= _TextureShadingWeights.x && m < _TextureShadingWeights.y )
    {
        weights = float4( 1 - ( m - _TextureShadingWeights.x ) / ( _TextureShadingWeights.y - _TextureShadingWeights.x ) , ( m - _TextureShadingWeights.x ) / ( _TextureShadingWeights.y - _TextureShadingWeights.x ) , 0 , 0 ); //lerp( p.x , p.y , m );
    }
    else if ( m >= _TextureShadingWeights.y && m < _TextureShadingWeights.z )
    {
        weights = float4( 0 , 1 - ( m - _TextureShadingWeights.y ) / ( _TextureShadingWeights.z - _TextureShadingWeights.y ) , ( m - _TextureShadingWeights.y ) / ( _TextureShadingWeights.z - _TextureShadingWeights.y ) , 0 );
    }
    else if ( m >= _TextureShadingWeights.z && m < _TextureShadingWeights.w )
    {
        weights = float4( 0 , 0 , 1 - ( m - _TextureShadingWeights.z ) / ( _TextureShadingWeights.w - _TextureShadingWeights.z ) , ( m - _TextureShadingWeights.z ) / ( _TextureShadingWeights.w - _TextureShadingWeights.z ) );
    }
    else
    {
        weights = float4( 0 , 0 , 0 , 1 );
    }

    return weights;

}

float4 PainterlyTextureUV( float m , float2 uv )
{
    // float4 p = triplanarSample( pos , nor );
    float4 p       = tex2D( _PainterlyLightMap , uv * 3 );
    float4 weights = PainterlyWeights( m );

    float4 flCol = p.x * weights.x;
    flCol += p.y * weights.y;
    flCol += p.z * weights.z;
    flCol += p.w * weights.w;

    return flCol;

}

// m = match
float4 PainterlyColor( float3 pos , float3 nor , float m , float2 uv )
{

    float4 fLCol = PainterlyTextureUV( m , uv );
    fLCol        = lerp( _HighLightColor , _LowLightColor , 1 - pow( fLCol , 4 ) );

    return fLCol;
}
