Texture2D<float4> _HeightMap;
SamplerState sampler_HeightMap;
float3 _MapSize;

float3 terrainPos( float3 pos ){
  return float3( pos.x , _HeightMap.SampleLevel(sampler_HeightMap, (((pos.xz+_MapSize.xz/2)) / _MapSize.xz) , 1).x * _MapSize.y * 2, pos.z);
}


float terrainHeight( float3 pos ){
   float h =  _HeightMap.SampleLevel(sampler_HeightMap, (((pos.xz+_MapSize.xz/2)) / _MapSize.xz) , 1).x * _MapSize.y * 2;
   return pos.y - h;
}

float3 normalizedTerrainPos( float3 pos ){
  
return float3(((pos.x+_MapSize.x/2) / _MapSize.x) , pos.y / _MapSize.y ,  (pos.z+_MapSize.z/2) / _MapSize.z);

}
void GetTerrainData( float3 pos , out float3  fPos, out float3 fNor){

    float3 eps = float3(1.1,0,0);
    fPos = terrainPos( pos );

    float3 l = terrainPos( pos + eps.xyy);
    float3 r = terrainPos( pos - eps.xyy);
    float3 u = terrainPos( pos + eps.yyx);
    float3 d = terrainPos( pos - eps.yyx);

    fNor = -normalize(cross(l-r,u-d));

}