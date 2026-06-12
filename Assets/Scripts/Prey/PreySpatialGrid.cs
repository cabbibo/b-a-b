using UnityEngine;
using System.Collections.Generic;

// Uniform spatial hash over one manager's birds. Rebuilt once per frame, it
// turns neighbor queries (flock / social) from "scan every bird" — O(n) per
// query, O(n²) per frame — into "scan a handful of nearby cells" ~O(1).
//
// Cell lists are pooled and reused frame to frame so a steady population
// produces no per-frame GC. The grid stores positions snapshotted at rebuild
// time for cell placement; queries still distance-test against each bird's
// live position, so the only staleness is which cell a fast bird sits in —
// well within a frame of motion.
public class PreySpatialGrid
{
    private float _cell = 10f;
    private float _inv  = 0.1f;

    private readonly Dictionary<long, List<PreyController>> _cells = new();
    private readonly Stack<List<PreyController>>            _pool  = new();

    public int BirdCount { get; private set; }
    public int CellCount => _cells.Count;

    // pack 3 cell coords into one long (21 bits each → ±1,048,575 cells per axis)
    private static long Key( int x , int y , int z )
        => ( (long)( x & 0x1FFFFF ) << 42 ) | ( (long)( y & 0x1FFFFF ) << 21 ) | (long)( z & 0x1FFFFF );

    private int Coord( float v ) => Mathf.FloorToInt( v * _inv );

    public void Rebuild( IReadOnlyList<PreyController> birds , float cellSize )
    {
        _cell = Mathf.Max( 0.5f , cellSize );
        _inv  = 1f / _cell;

        // recycle every list back to the pool, then clear the map
        foreach ( var kv in _cells ) { kv.Value.Clear(); _pool.Push( kv.Value ); }
        _cells.Clear();
        BirdCount = 0;

        if ( birds == null ) return;

        int n = birds.Count;
        for ( int i = 0; i < n; i++ ) {
            var bird = birds[i];
            if ( bird == null ) continue;

            var p = bird.position;
            long k = Key( Coord( p.x ) , Coord( p.y ) , Coord( p.z ) );
            if ( !_cells.TryGetValue( k , out var list ) ) {
                list = _pool.Count > 0 ? _pool.Pop() : new List<PreyController>( 8 );
                _cells[k] = list;
            }
            list.Add( bird );
            BirdCount++;
        }
    }

    public void Query( Vector3 pos , float radius , PreyController exclude , List<PreyController> results )
    {
        float sqr = radius * radius;
        int minX = Coord( pos.x - radius ), maxX = Coord( pos.x + radius );
        int minY = Coord( pos.y - radius ), maxY = Coord( pos.y + radius );
        int minZ = Coord( pos.z - radius ), maxZ = Coord( pos.z + radius );

        for ( int x = minX; x <= maxX; x++ )
        for ( int y = minY; y <= maxY; y++ )
        for ( int z = minZ; z <= maxZ; z++ ) {
            if ( !_cells.TryGetValue( Key( x , y , z ) , out var list ) ) continue;
            for ( int i = 0; i < list.Count; i++ ) {
                var b = list[i];
                if ( b == null || b == exclude ) continue;
                if ( ( b.position - pos ).sqrMagnitude < sqr ) results.Add( b );
            }
        }
    }
}
