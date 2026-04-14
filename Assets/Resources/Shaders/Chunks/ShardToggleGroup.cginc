struct ShardLightData
{
    float3 position;
    float  isCollected;
    float3 up;
    float  timeCollected;
    float3 forward;
    float  debug;
};

StructuredBuffer<ShardLightData> _ShardBuffer;
int                              _ShardBuffer_COUNT;

struct GridCell16
{
    uint id0;
    uint id1;
    uint id2;
    uint id3;
    uint id4;
    uint id5;
    uint id6;
    uint id7;
    uint id8;
    uint id9;
    uint id10;
    uint id11;
    uint id12;
    uint id13;
    uint id14;
    uint id15;
};

StructuredBuffer<GridCell16> _ShardGridBuffer;
float3                       _ShardGridMin;
float3                       _ShardGridMax;
float3                       _ShardGridCellSize;
float4                       _ShardGridResolution; // xyz used as ints


int3 GetShardGridCoord( float3 worldPos )
{
    int3 res = int3( _ShardGridResolution.xyz );

    float3 local = worldPos - _ShardGridMin;
    int3   cell  = (int3)floor( local / _ShardGridCellSize );
    cell         = clamp( cell , int3( 0 , 0 , 0 ) , res - 1 );

    return cell;
}

int FlattenShardGridCoord( int3 c , int3 res )
{
    return c.x + res.x * ( c.y + res.y * c.z );
}

void GetClosestShardIDs16( float3 worldPos , out uint ids[ 16 ] )
{
    int3 res = int3( _ShardGridResolution.xyz );

    float3 local = worldPos - _ShardGridMin;
    int3   cell  = (int3)floor( local / _ShardGridCellSize );
    cell         = clamp( cell , int3( 0 , 0 , 0 ) , res - 1 );

    int flat = cell.x + res.x * ( cell.y + res.y * cell.z );

    GridCell16 c = _ShardGridBuffer[ flat ];

    ids[ 0 ]  = c.id0;
    ids[ 1 ]  = c.id1;
    ids[ 2 ]  = c.id2;
    ids[ 3 ]  = c.id3;
    ids[ 4 ]  = c.id4;
    ids[ 5 ]  = c.id5;
    ids[ 6 ]  = c.id6;
    ids[ 7 ]  = c.id7;
    ids[ 8 ]  = c.id8;
    ids[ 9 ]  = c.id9;
    ids[ 10 ] = c.id10;
    ids[ 11 ] = c.id11;
    ids[ 12 ] = c.id12;
    ids[ 13 ] = c.id13;
    ids[ 14 ] = c.id14;
    ids[ 15 ] = c.id15;
}
