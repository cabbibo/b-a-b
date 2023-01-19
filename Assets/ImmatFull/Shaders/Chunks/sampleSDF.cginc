float3 _Dimensions;
float3 _Extents;
float3 _Center;

float4x4 _SDFTransform;
float4x4 _SDFInverseTransform;

Texture3D<float4> _SDFTexture;
SamplerState _LinearClamp;

float4 sampleSDF( float3 pos ){
    
    
    float3 tPos = mul( _SDFInverseTransform ,float4(pos,1));
    tPos -= _Center;
    tPos /= _Extents;

    tPos += 1;
    tPos /= 2;

    float4 t = _SDFTexture.SampleLevel(_LinearClamp,tPos , 0);

    return t;

}