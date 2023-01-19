
Texture2D<float4> _HeightMap;
SamplerState sampler_HeightMap;
float3 _MapSize;




float heightAtPosition( float3 pos ){
    float2 v = (pos.xz+(_MapSize.xz/2)) / _MapSize.xz;// _MapSize.xz;
    float4 c = _HeightMap.SampleLevel(sampler_HeightMap, v , 0);//tex2Dlod(_HeightMap , float4(pos.xz * _MapSize,0,0) );
    return 2*c.x * _MapSize.y;
}


float3 worldPos( float3 pos ){

    return float3( pos.x , heightAtPosition(pos) , pos.z);


}


float3 getTerrainNormal( float3 pos ){

  float delta = .4;
  float3 dU = worldPos( pos + float3(delta,0,0) );
  float3 dD = worldPos( pos + float3(-delta,0,0) );
  float3 dL = worldPos( pos + float3(0,0,delta) );
  float3 dR = worldPos( pos + float3(0,0,-delta) );

  return normalize(cross(dU.xyz-dD.xyz , dL.xyz-dR.xyz));

}
