using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Tool: "Stamp" a Terrain with a selection of meshes.
// For every heightmap column under the selected meshes it finds the BOTTOM
// surface at that column ( ray-casting upward from below ) and sets the terrain
// height there, so the terrain tucks in just under the meshes.
//
// All heightmap work is done on a flat 1D array ( idx = z * res + x ) for speed.
public class StampMeshIntoTerrain : EditorWindow
{
    private enum StampMode
    {
        SetUnderMesh, // terrain height = mesh bottom ( only where a mesh covers it )
        OnlyLower,    // push terrain down to the mesh bottom, never up
        OnlyRaise     // pull terrain up to the mesh bottom, never down
    }

    private Terrain   terrain;
    private StampMode mode         = StampMode.SetUnderMesh;
    private float     heightOffset = 0f;   // terrain sits this far BELOW the mesh bottom
    private float     smoothRadius = 4f;   // blend radius in WORLD units ( 0 = hard edge )

    private const int SmoothPasses = 2;    // box-blur passes ( more = softer falloff )

    // Edge texture stamp ( optional ) — scatters a grayscale stamp along the
    // footprint rim, sculpting DOWN only, to break up / texturize the edges.
    private Texture2D stampTexture;
    private int       stampCount    = 200;
    private float     stampMinScale = 2f;
    private float     stampMaxScale = 6f;
    private float     stampDepth    = 1f; // max downward push in world units ( at white )
    private float     edgeBandWidth = 2f; // how far off the rim ( world units ) stamps may land
    private int       stampSeed     = 0;

    [MenuItem( "Tools/Terrain/Stamp Mesh Into Terrain" )]
    private static void Init()
    {
        GetWindow<StampMeshIntoTerrain>( "Stamp Mesh Into Terrain" ).Show();
    }

    private void OnSelectionChange()
    {
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Select one or more mesh objects in the scene, then Stamp. The terrain is set to sit " +
            "just under the bottom surface of the selected meshes, per column.",
            MessageType.Info );

        terrain = (Terrain)EditorGUILayout.ObjectField( "Terrain" , terrain , typeof(Terrain) , true );

        mode         = (StampMode)EditorGUILayout.EnumPopup( "Mode" , mode );
        heightOffset = EditorGUILayout.FloatField(
            new GUIContent( "Height Offset" , "Terrain is placed this far BELOW the mesh bottom. 0 = flush." ) ,
            heightOffset );
        smoothRadius = EditorGUILayout.FloatField(
            new GUIContent( "Smooth Radius" ,
                "Blend radius around the stamp edge, in WORLD units. Any size ( no cap ) — cost is the " +
                "same regardless of radius. The terrain is never allowed to rise above the mesh bottom." ) ,
            smoothRadius );
        if ( smoothRadius < 0f ) {
            smoothRadius = 0f;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField( "Edge Texture Stamp ( optional )" , EditorStyles.boldLabel );
        stampTexture = (Texture2D)EditorGUILayout.ObjectField(
            new GUIContent( "Stamp Texture" , "Grayscale texture ( Read/Write enabled ). Brighter = deeper. " +
                                              "Leave empty to skip edge texturing." ) ,
            stampTexture , typeof(Texture2D) , false );

        using ( new EditorGUI.DisabledScope( stampTexture == null ) ) {
            stampCount = EditorGUILayout.IntField(
                new GUIContent( "Count" , "How many stamp instances to scatter along the rim." ) , stampCount );
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel( new GUIContent( "Scale Range" , "World-space size of each stamp instance." ) );
            stampMinScale = EditorGUILayout.FloatField( stampMinScale );
            stampMaxScale = EditorGUILayout.FloatField( stampMaxScale );
            EditorGUILayout.EndHorizontal();
            stampDepth    = EditorGUILayout.FloatField(
                new GUIContent( "Depth" , "Max downward push in world units ( at white pixels )." ) , stampDepth );
            edgeBandWidth = EditorGUILayout.FloatField(
                new GUIContent( "Edge Band Width" , "How far off the rim ( world units ) stamps may land." ) ,
                edgeBandWidth );
            stampSeed = EditorGUILayout.IntField(
                new GUIContent( "Seed" , "Same seed = same scatter. Change it to reroll." ) , stampSeed );
        }

        EditorGUILayout.Space();

        var filters = GatherSelectedMeshFilters();
        EditorGUILayout.LabelField( "Selected mesh parts" , filters.Count + " from " +
                                                            Selection.gameObjects.Length + " object(s)" );

        using ( new EditorGUI.DisabledScope( terrain == null || filters.Count == 0 ) ) {
            if ( GUILayout.Button( "Stamp Selected Meshes" ) ) {
                Stamp( filters );
            }
        }

        if ( terrain == null ) {
            EditorGUILayout.HelpBox( "Assign a Terrain ( or select one along with your meshes ).", MessageType.Warning );
        }
    }

    // Gather every MeshFilter under the current selection, skipping any terrain.
    private List<MeshFilter> GatherSelectedMeshFilters()
    {
        var set = new HashSet<MeshFilter>();

        foreach ( var go in Selection.gameObjects ) {

            var t = go.GetComponent<Terrain>();
            if ( t != null ) {
                if ( terrain == null ) {
                    terrain = t;
                }

                continue;
            }

            foreach ( var mf in go.GetComponentsInChildren<MeshFilter>() ) {
                if ( mf.sharedMesh != null ) {
                    set.Add( mf );
                }
            }
        }

        return new List<MeshFilter>( set );
    }

    private void Stamp( List<MeshFilter> filters )
    {
        var td   = terrain.terrainData;
        var tPos = terrain.transform.position;
        var size = td.size;
        int res  = td.heightmapResolution;
        int n    = res * res;

        // Ensure each mesh part has a collider to ray-test against.
        var colliders     = new List<MeshCollider>();
        var tempColliders = new List<MeshCollider>();

        foreach ( var mf in filters ) {
            if ( mf.sharedMesh == null ) {
                continue;
            }

            var mc = mf.GetComponent<MeshCollider>();
            if ( mc == null ) {
                mc            = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                tempColliders.Add( mc );
            }

            colliders.Add( mc );
        }

        if ( colliders.Count == 0 ) {
            Debug.LogWarning( "Stamp Mesh Into Terrain: no meshes found in selection." );
            CleanUp( tempColliders );
            return;
        }

        // Sync first, then read bounds — so freshly-added colliders report bounds
        // consistent with what the ray scan below will use.
        Physics.SyncTransforms();

        Bounds worldBounds = colliders[0].bounds;
        for ( int i = 1; i < colliders.Count; i++ ) {
            worldBounds.Encapsulate( colliders[i].bounds );
        }

        Undo.RegisterCompleteObjectUndo( td , "Stamp Mesh Into Terrain" );

        float[,] heights2D = td.GetHeights( 0 , 0 , res , res );

        // Flatten to 1D for fast processing.
        float[] h       = new float[n];
        var     covered = new bool[n];
        var     target  = new float[n];   // mesh-bottom ceiling ( normalized )
        var     bottomW = new float[n];   // lowest world-space mesh bottom per column

        for ( int z = 0; z < res; z++ ) {
            int baseZ = z * res;
            for ( int x = 0; x < res; x++ ) {
                h[baseZ + x] = heights2D[z, x];
            }
        }

        // Footprint of the whole selection in heightmap indices ( for finalize + smoothing ).
        int fx0 = WorldToIndex( worldBounds.min.x , tPos.x , size.x , res );
        int fx1 = WorldToIndex( worldBounds.max.x , tPos.x , size.x , res );
        int fz0 = WorldToIndex( worldBounds.min.z , tPos.z , size.z , res );
        int fz1 = WorldToIndex( worldBounds.max.z , tPos.z , size.z , res );

        // --- Ray pass: scan each collider's OWN footprint only ( no empty gaps ). ---
        for ( int ci = 0; ci < colliders.Count; ci++ ) {

            EditorUtility.DisplayProgressBar( "Stamping Terrain" ,
                "Sampling mesh underside..." , (float)ci / colliders.Count );

            var mc = colliders[ci];
            var b  = mc.bounds;

            int cx0 = WorldToIndex( b.min.x , tPos.x , size.x , res );
            int cx1 = WorldToIndex( b.max.x , tPos.x , size.x , res );
            int cz0 = WorldToIndex( b.min.z , tPos.z , size.z , res );
            int cz1 = WorldToIndex( b.max.z , tPos.z , size.z , res );

            float rayStartY = b.min.y - 10f;
            float rayLen    = b.size.y + 20f;

            for ( int zi = cz0; zi <= cz1; zi++ ) {
                float worldZ = tPos.z + (float)zi / ( res - 1 ) * size.z;
                int   baseZ  = zi * res;

                for ( int xi = cx0; xi <= cx1; xi++ ) {
                    float worldX = tPos.x + (float)xi / ( res - 1 ) * size.x;

                    var ray = new Ray( new Vector3( worldX , rayStartY , worldZ ) , Vector3.up );

                    if ( mc.Raycast( ray , out RaycastHit hit , rayLen ) ) {
                        int idx = baseZ + xi;
                        if ( !covered[idx] || hit.point.y < bottomW[idx] ) {
                            bottomW[idx] = hit.point.y;
                            covered[idx] = true;
                        }
                    }
                }
            }
        }

        // --- Finalize the carve from the lowest bottom per covered column. ---
        int columns = 0;

        for ( int zi = fz0; zi <= fz1; zi++ ) {
            int baseZ = zi * res;
            for ( int xi = fx0; xi <= fx1; xi++ ) {
                int idx = baseZ + xi;
                if ( !covered[idx] ) {
                    continue;
                }

                float norm = Mathf.Clamp01( ( bottomW[idx] - heightOffset - tPos.y ) / size.y );
                float cur  = h[idx];

                switch ( mode ) {
                    case StampMode.SetUnderMesh: h[idx] = norm;                     break;
                    case StampMode.OnlyLower:    h[idx] = Mathf.Min( cur , norm );  break;
                    case StampMode.OnlyRaise:    h[idx] = Mathf.Max( cur , norm );  break;
                }

                target[idx] = norm;
                columns++;
            }
        }

        // --- Smoothing ( constant time regardless of radius ). ---
        float cellX = size.x / ( res - 1 );
        float cellZ = size.z / ( res - 1 );
        int   rx    = Mathf.RoundToInt( smoothRadius / cellX );
        int   rz    = Mathf.RoundToInt( smoothRadius / cellZ );

        if ( smoothRadius > 0f && ( rx > 0 || rz > 0 ) ) {
            SmoothEdges( h , covered , target , res , fx0 , fx1 , fz0 , fz1 , rx , rz );
        }

        // --- Edge texture ( after smoothing so detail survives ). ---
        if ( stampTexture != null && stampCount > 0 && stampDepth > 0f ) {
            ApplyEdgeStamps( h , covered , res , tPos , size , fx0 , fx1 , fz0 , fz1 );
        }

        // Un-flatten and write back.
        for ( int z = 0; z < res; z++ ) {
            int baseZ = z * res;
            for ( int x = 0; x < res; x++ ) {
                heights2D[z, x] = h[baseZ + x];
            }
        }

        td.SetHeights( 0 , 0 , heights2D );
        EditorUtility.SetDirty( td );

        EditorUtility.ClearProgressBar();
        CleanUp( tempColliders );

        Debug.Log( "Stamp Mesh Into Terrain: updated " + columns + " terrain columns from " +
                   colliders.Count + " mesh part(s)." );
    }

    private static int WorldToIndex( float world , float origin , float sizeAxis , int res )
    {
        return Mathf.Clamp( Mathf.RoundToInt( ( world - origin ) / sizeAxis * ( res - 1 ) ) , 0 , res - 1 );
    }

    // Separable box blur ( sliding window, O(1) per cell ) with a hard ceiling:
    // covered columns are never allowed above their mesh-bottom target, so the
    // terrain can never rise over the mesh no matter how large the radius.
    private void SmoothEdges( float[] h , bool[] covered , float[] target , int res ,
                              int fx0 , int fx1 , int fz0 , int fz1 , int rx , int rz )
    {
        int marginX = Mathf.Max( 1 , rx ) * SmoothPasses + 1;
        int marginZ = Mathf.Max( 1 , rz ) * SmoothPasses + 1;
        int x0 = Mathf.Max( 0 , fx0 - marginX );
        int x1 = Mathf.Min( res - 1 , fx1 + marginX );
        int z0 = Mathf.Max( 0 , fz0 - marginZ );
        int z1 = Mathf.Min( res - 1 , fz1 + marginZ );

        int     n   = res * res;
        float[] tmp = new float[n];

        for ( int p = 0; p < SmoothPasses; p++ ) {

            EditorUtility.DisplayProgressBar( "Stamping Terrain" , "Smoothing edges..." ,
                (float)p / SmoothPasses );

            // Horizontal pass: tmp = Hblur(h). Full copy first so non-region reads stay valid.
            Array.Copy( h , tmp , n );
            if ( rx > 0 ) {
                for ( int zi = z0; zi <= z1; zi++ ) {
                    BlurRun( h , tmp , zi * res , 1 , x0 , x1 , rx , res );
                }
            }

            // Vertical pass: h = Vblur(tmp).
            Array.Copy( tmp , h , n );
            if ( rz > 0 ) {
                for ( int xi = x0; xi <= x1; xi++ ) {
                    BlurRun( tmp , h , xi , res , z0 , z1 , rz , res );
                }
            }

            // Clamp covered columns back under the mesh.
            for ( int zi = z0; zi <= z1; zi++ ) {
                int baseZ = zi * res;
                for ( int xi = x0; xi <= x1; xi++ ) {
                    int idx = baseZ + xi;
                    if ( covered[idx] && h[idx] > target[idx] ) {
                        h[idx] = target[idx];
                    }
                }
            }
        }
    }

    // One 1D box-blur line. base+ i*stride walks the line; i runs [i0, i1];
    // window is +/- radius, clamped to [0, res-1] along the line.
    private static void BlurRun( float[] src , float[] dst , int baseIndex , int stride ,
                                 int i0 , int i1 , int radius , int res )
    {
        int wLo = Mathf.Max( 0 , i0 - radius );
        int wHi = Mathf.Min( res - 1 , i1 + radius );

        int   lo  = Mathf.Max( wLo , i0 - radius );
        int   hi  = Mathf.Min( wHi , i0 + radius );
        float sum = 0f;
        for ( int k = lo; k <= hi; k++ ) {
            sum += src[baseIndex + k * stride];
        }

        dst[baseIndex + i0 * stride] = sum / ( hi - lo + 1 );

        int pLo = lo, pHi = hi;
        for ( int i = i0 + 1; i <= i1; i++ ) {
            int nLo = Mathf.Max( wLo , i - radius );
            int nHi = Mathf.Min( wHi , i + radius );

            for ( int k = pHi + 1; k <= nHi; k++ ) {
                sum += src[baseIndex + k * stride];
            }

            for ( int k = pLo; k < nLo; k++ ) {
                sum -= src[baseIndex + k * stride];
            }

            dst[baseIndex + i * stride] = sum / ( nHi - nLo + 1 );
            pLo = nLo;
            pHi = nHi;
        }
    }

    // Scatters the stamp texture along the footprint rim, sculpting DOWN only.
    private void ApplyEdgeStamps( float[] h , bool[] covered , int res , Vector3 tPos , Vector3 size ,
                                  int fx0 , int fx1 , int fz0 , int fz1 )
    {
        if ( !stampTexture.isReadable ) {
            Debug.LogWarning( "Stamp Mesh Into Terrain: stamp texture '" + stampTexture.name +
                              "' must have Read/Write Enabled in its import settings. Skipping edge texture." );
            return;
        }

        // Rim cells: covered cells with at least one uncovered / off-grid 4-neighbour.
        var edgeCells = new List<Vector2Int>();
        for ( int zi = fz0; zi <= fz1; zi++ ) {
            int baseZ = zi * res;
            for ( int xi = fx0; xi <= fx1; xi++ ) {
                int idx = baseZ + xi;
                if ( !covered[idx] ) {
                    continue;
                }

                bool isEdge =
                    xi == 0 || xi == res - 1 || zi == 0 || zi == res - 1 ||
                    !covered[idx - 1]   || !covered[idx + 1] ||
                    !covered[idx - res] || !covered[idx + res];

                if ( isEdge ) {
                    edgeCells.Add( new Vector2Int( xi , zi ) );
                }
            }
        }

        if ( edgeCells.Count == 0 ) {
            Debug.LogWarning( "Stamp Mesh Into Terrain: no footprint edge found to texture." );
            return;
        }

        float minScale = Mathf.Max( 0.001f , Mathf.Min( stampMinScale , stampMaxScale ) );
        float maxScale = Mathf.Max( minScale , stampMaxScale );

        var oldState = UnityEngine.Random.state;
        UnityEngine.Random.InitState( stampSeed );

        for ( int s = 0; s < stampCount; s++ ) {

            if ( ( s & 31 ) == 0 ) {
                EditorUtility.DisplayProgressBar( "Stamping Terrain" , "Texturing edges..." ,
                    (float)s / stampCount );
            }

            var ec = edgeCells[UnityEngine.Random.Range( 0 , edgeCells.Count )];

            float cx = tPos.x + (float)ec.x / ( res - 1 ) * size.x + UnityEngine.Random.Range( -edgeBandWidth , edgeBandWidth );
            float cz = tPos.z + (float)ec.y / ( res - 1 ) * size.z + UnityEngine.Random.Range( -edgeBandWidth , edgeBandWidth );

            float scale = UnityEngine.Random.Range( minScale , maxScale );
            float half  = scale * 0.5f;
            float rot   = UnityEngine.Random.Range( 0f , Mathf.PI * 2f );
            float cosr  = Mathf.Cos( rot );
            float sinr  = Mathf.Sin( rot );

            int sx0 = WorldToIndex( cx - half , tPos.x , size.x , res );
            int sx1 = WorldToIndex( cx + half , tPos.x , size.x , res );
            int sz0 = WorldToIndex( cz - half , tPos.z , size.z , res );
            int sz1 = WorldToIndex( cz + half , tPos.z , size.z , res );

            for ( int zi = sz0; zi <= sz1; zi++ ) {
                float wz    = tPos.z + (float)zi / ( res - 1 ) * size.z;
                int   baseZ = zi * res;

                for ( int xi = sx0; xi <= sx1; xi++ ) {
                    float wx = tPos.x + (float)xi / ( res - 1 ) * size.x;

                    float lx = wx - cx;
                    float lz = wz - cz;
                    float u  = ( lx * cosr - lz * sinr ) / scale + 0.5f;
                    float v  = ( lx * sinr + lz * cosr ) / scale + 0.5f;

                    if ( u < 0f || u > 1f || v < 0f || v > 1f ) {
                        continue;
                    }

                    float bright   = stampTexture.GetPixelBilinear( u , v ).grayscale;
                    float pushNorm = bright * stampDepth / size.y;
                    int   idx      = baseZ + xi;

                    h[idx] = Mathf.Max( 0f , h[idx] - pushNorm );
                }
            }
        }

        UnityEngine.Random.state = oldState;
    }

    private void CleanUp( List<MeshCollider> tempColliders )
    {
        foreach ( var mc in tempColliders ) {
            if ( mc != null ) {
                DestroyImmediate( mc );
            }
        }
    }
}
