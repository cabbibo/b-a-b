sampler2D _NormalMap;
float3    _TriplanarMultiplier;
float     _TriplanarSharpness;
float     _TriplanarNormalWeight;


float3 triplanarNormal( float3 p , float3 n , float3 tspace0 , float3 tspace1 , float3 tspace2 , float offset )
{

    // UDN blend
    // Triplanar uvs
    float2 uvX = p.zy * _TriplanarMultiplier + offset; // x facing plane
    float2 uvY = p.xz * _TriplanarMultiplier + offset; // y facing plane
    float2 uvZ = p.xy * _TriplanarMultiplier + offset; // z facing plane

    // Tangent space normal maps
    half3 tnormalX = UnpackNormal( tex2D( _NormalMap , uvX % 1 ) );
    half3 tnormalY = UnpackNormal( tex2D( _NormalMap , uvY % 1 ) );
    half3 tnormalZ = UnpackNormal( tex2D( _NormalMap , uvZ % 1 ) );

    // Swizzle world normals into tangent space and apply UDN blend.
    // These should get normalized, but it's very a minor visual
    // difference to skip it until after the blend.
    tnormalX = normalize( half3( tnormalX.xy * _TriplanarNormalWeight + n.zy , n.x ) );
    tnormalY = normalize( half3( tnormalY.xy * _TriplanarNormalWeight + n.xz , n.y ) );
    tnormalZ = normalize( half3( tnormalZ.xy * _TriplanarNormalWeight + n.xy , n.z ) );

    half3 blend = pow( abs( n ) , _TriplanarSharpness );
    // make sure the weights sum up to 1 (divide by sum of x+y+z)
    blend /= dot( blend , 1.0 );

    // Swizzle tangent normals to match world orientation and triblend
    half3 worldNormal = normalize(
        tnormalX.zyx * blend.x +
        tnormalY.xzy * blend.y +
        tnormalZ.xyz * blend.z
    );

    return worldNormal;

}
