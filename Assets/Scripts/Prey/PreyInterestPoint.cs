using UnityEngine;

public enum InterestPointType { Perch, Updraft, NewInterest, NewCalm, Despawn }
public enum PerchSubType      { OnCollider, InArea, Field }
public enum PerchFacing       { Up, Down }
public enum CurlDirection     { CounterClockwise, Clockwise }

// How the searched-to target position is derived from the point.
// LandPoint (Perch points only): aim straight at the actual perch spot the bird will land on.
public enum SearchTargetType  { Center, RandomInRange, XZOnly, LandPoint }
// Shape used for notice / arrival detection. Cylinder ignores Y (infinite vertical extent).
// Collider = arrival fires when the bird enters an assigned collider (entranceCollider on the component);
// notice still uses noticeRadius and Enter Radius is ignored.
// Plane = a horizontal plane at the point's Y; "inside" = anything BELOW it (a kill-floor / under-line),
// radius ignored.
public enum EntranceShape     { Sphere, Cylinder, Collider, Plane }

// ── Per-type settings ─────────────────────────────────────────────────────────

[System.Serializable]
public class PerchOnColliderSettings
{
    // The colliders themselves are a scene reference and live on the PreyInterestPoint component
    // (perchColliders); only the generation params live here / on the config asset.
    public PerchFacing facing       = PerchFacing.Up;
    public float       radius       = 10f;
    public int         pointCount   = 10;
    [Range( 0f , 1f )]
    public float       minNormalDot = 0.5f;
}

[System.Serializable]
public class PerchInAreaSettings
{
    public PerchFacing facing       = PerchFacing.Up;
    public float       radius       = 10f;
    public int         pointCount   = 10;
    public float       castHeight   = 20f;   // distance above (or below for Down) to start the raycast
    public LayerMask   groundLayers = ~0;
    [Range( 0f , 1f )]
    public float       minNormalDot = 0.5f;
}

// Field: no pre-generated points — birds compute where to land in real time, spaced from each other.
[System.Serializable]
public class PerchFieldSettings
{
    public float     radius              = 20f;  // area the birds may land within (around the point)
    public float     spacing             = 3f;   // desired minimum distance between landed birds
    [Tooltip( "Shift the landing area forward along the bird's velocity by this distance (0 = centered)." )]
    public float     forwardFromVelocity = 0f;
    [Range( 0f , 1f )]
    [Tooltip( "1 = pack tightly at the minimum spacing (clump); 0 = land wherever (just closest to self)." )]
    public float     desireToBeClose     = 0.5f;
    [Tooltip( "Preference for flat, upward-facing spots. 0 = ignore slope; higher = increasingly avoid " +
              "slanted/steep surfaces (at 1, a vertical face costs about a full radius of extra distance)." )]
    public float     upImportance        = 0f;
    [Tooltip( "Cast upward to find a surface above the bird (ledge underside) instead of down to the ground." )]
    public bool      castUp              = false;
    [Tooltip( "Start the landing raycast this far past the prey along the cast direction — always a bit, " +
              "so the ray never starts inside the bird and self-intersects." )]
    public float     castHeightOffset    = 1f;
    public LayerMask groundLayers        = ~0;
}

[System.Serializable]
public class UpdraftPointSettings
{
    public float         forceUp       = 3f;
    public float         forceIn       = 1f;
    public float         curlForce     = 2f;
    public CurlDirection curlDirection = CurlDirection.CounterClockwise;

    [Header( "Desired Altitude" )]
    [Tooltip( "Top of the soaring band (height above the point)." )]
    public float desiredAltitude      = 40f;
    [Tooltip( "How far below the top the lift starts easing off — the soaring band is " +
              "[desiredAltitude - altitudeRange .. desiredAltitude]. Larger = softer, less rigid." )]
    public float altitudeRange        = 15f;
    [Tooltip( "How hard the bird is eased back down when above the top of the band." )]
    public float altitudeHoldStrength = 1f;
}

// ── Component ─────────────────────────────────────────────────────────────────

public class PreyInterestPoint : MonoBehaviour
{
    // ── Tunable params live on this asset; scene refs stay on the component ────────────────────
    [Header( "Config" )]
    public PreyInterestPointConfigSO config;

    [Header( "Scene References" )]
    [Tooltip( "Optional: when set, perch point-count generation uses manager.maxPray." )]
    public PreyManager manager;
    [Tooltip( "OnCollider perch: the colliders birds may land on (scene refs — params live on the config)." )]
    public Collider[]  perchColliders;
    [Tooltip( "Collider entrance shape: the bird arrives (and a Despawn point fires) the moment it enters this collider." )]
    public Collider    entranceCollider;

    // ── Proxy properties: forward to config, null-safe with the old defaults ───────────────────
    private static readonly PerchOnColliderSettings _defOnCollider = new();
    private static readonly PerchInAreaSettings     _defInArea     = new();
    private static readonly PerchFieldSettings      _defField      = new();
    private static readonly UpdraftPointSettings    _defUpdraft    = new();

    public InterestPointType type                   => config != null ? config.type                   : InterestPointType.NewCalm;
    public float             noticeRadius           => config != null ? config.noticeRadius           : 40f;
    public float             enterRadius            => config != null ? config.enterRadius            : 5f;
    public bool              alwaysInteresting      => config != null ? config.alwaysInteresting      : false;
    public float             priority               => config != null ? config.priority               : 1f;
    public float             timeToRemainInterested => config != null ? config.timeToRemainInterested : 0f;
    public float             timeToRemainVariance   => config != null ? config.timeToRemainVariance   : 0f;
    public float             noticeUrgency          => config != null ? config.noticeUrgency          : 0f;

    public SearchTargetType  searchTargetType   => config != null ? config.searchTargetType   : SearchTargetType.Center;
    public float             searchRandomRadius => config != null ? config.searchRandomRadius : 10f;
    public float             targetRandomness   => config != null ? config.targetRandomness   : 0f;
    public EntranceShape     entranceShape      => config != null ? config.entranceShape      : EntranceShape.Sphere;

    public PerchSubType            perchSubType    => config != null ? config.perchSubType    : PerchSubType.InArea;
    public PerchOnColliderSettings perchOnCollider => config != null ? config.perchOnCollider : _defOnCollider;
    public PerchInAreaSettings     perchInArea     => config != null ? config.perchInArea     : _defInArea;
    public PerchFieldSettings      perchField      => config != null ? config.perchField      : _defField;
    public UpdraftPointSettings    updraftSettings => config != null ? config.updraftSettings : _defUpdraft;

    // ── Detection / targeting ─────────────────────────────────────────────────

    // Is pos within the given radius, respecting the entrance shape?
    // Sphere = 3D distance; Cylinder = XZ distance only (any height).
    public bool IsWithin( Vector3 pos , float radius )
    {
        // Plane: purely a Y test — "within" = at or below the point's height (radius ignored).
        if ( entranceShape == EntranceShape.Plane )
            return pos.y <= transform.position.y;

        var d = pos - transform.position;
        if ( entranceShape == EntranceShape.Cylinder ) d.y = 0f;
        return d.sqrMagnitude <= radius * radius;
    }

    // Collider entrance shape: is pos inside the assigned entrance collider? Used for arrival
    // (and thus despawn) instead of the enterRadius distance test. False if no collider assigned.
    public bool ContainsPoint( Vector3 pos )
    {
        if ( entranceCollider == null ) return false;
        return ( entranceCollider.ClosestPoint( pos ) - pos ).sqrMagnitude <= 0.0001f;
    }

    // Is pos within this point's ENTER volume, per the entrance shape? Drives arrival and the passive
    // "Despawn point = kill-zone" overlap test.
    //   Sphere/Cylinder → within enterRadius of the point.
    //   Collider        → within enterRadius of the collider SURFACE (or inside). Treating enterRadius
    //                      as a "hit reach" (not a strict inside test) makes it robust to fast birds
    //                      tunnelling through thin colliders between frames. enterRadius 0 → strictly inside.
    public bool WithinEnter( Vector3 pos )
    {
        if ( entranceShape != EntranceShape.Collider )
            return IsWithin( pos , enterRadius );

        if ( entranceCollider == null ) return false;

        // Non-convex mesh colliders don't support ClosestPoint — fall back to a bounds test.
        if ( entranceCollider is MeshCollider mc && !mc.convex )
            return entranceCollider.bounds.Contains( pos );

        float r = Mathf.Max( enterRadius , 0.01f );
        return ( entranceCollider.ClosestPoint( pos ) - pos ).sqrMagnitude <= r * r;
    }

    // World position the bird should fly toward, given its current position and a
    // per-search random offset (applied to every search type — see SearchScatterRadius).
    public Vector3 GetSearchTarget( Vector3 birdPos , Vector3 randomOffset )
    {
        // Collider entrance: steer straight at the nearest point on the collider surface so the
        // bird flies into it (no scatter — we want it to actually enter and trigger arrival).
        if ( entranceShape == EntranceShape.Collider && entranceCollider != null )
            return entranceCollider.ClosestPoint( birdPos );

        Vector3 basePos = searchTargetType == SearchTargetType.XZOnly
            ? new Vector3( transform.position.x , birdPos.y , transform.position.z )
            : transform.position;

        return basePos + randomOffset;
    }

    // Total XZ scatter radius for this search: the general targetRandomness, plus the
    // larger searchRandomRadius when the type is RandomInRange.
    public float SearchScatterRadius()
    {
        return targetRandomness
               + (searchTargetType == SearchTargetType.RandomInRange ? searchRandomRadius : 0f);
    }

    // A stable random XZ offset within the given radius, picked once per search.
    public Vector3 RandomOffset( float radius )
    {
        var c = Random.insideUnitCircle * radius;
        return new Vector3( c.x , 0f , c.y );
    }

    // ── Perch point generation ────────────────────────────────────────────────

    public void GeneratePerchPoints()
    {
        ClearPerchPoints();
        if      ( perchSubType == PerchSubType.OnCollider ) GenerateOnCollider();
        else if ( perchSubType == PerchSubType.InArea )     GenerateInArea();
        // Field: nothing to pre-generate — landing spots are computed at runtime
    }

    public void ClearPerchPoints()
    {
        for ( int i = transform.childCount - 1; i >= 0; i-- ) {
            var child = transform.GetChild( i );
            if ( child.name.StartsWith( "_perch_" ) )
                DestroyImmediate( child.gameObject );
        }
    }

    private void GenerateOnCollider()
    {
        var s = perchOnCollider;

        // combined bounds across all assigned colliders → drives the raycast origin height & distance
        bool   haveBounds = false;
        Bounds bounds     = default;
        if ( perchColliders != null ) {
            foreach ( var c in perchColliders ) {
                if ( c == null ) continue;
                if ( !haveBounds ) { bounds = c.bounds; haveBounds = true; }
                else               bounds.Encapsulate( c.bounds );
            }
        }

        if ( !haveBounds ) {
            Debug.LogWarning( "[PreyInterestPoint] OnCollider: no colliders assigned." , this );
            return;
        }

        bool    wantUp     = s.facing == PerchFacing.Up;
        float   originY    = wantUp ? bounds.max.y + 1f : bounds.min.y - 1f;
        Vector3 castDir    = wantUp ? Vector3.down : Vector3.up;
        float   castDist   = (bounds.size.y + 2f) * 2f;
        int     pointCount = manager != null ? manager.maxPray : s.pointCount;
        int     gen        = 0;

        for ( int i = 0; i < pointCount * 20 && gen < pointCount; i++ ) {
            var xz     = Random.insideUnitCircle * s.radius;
            var origin = new Vector3( transform.position.x + xz.x , originY , transform.position.z + xz.y );

            if ( !Physics.Raycast( origin , castDir , out var hit , castDist ) ) continue;
            if ( !ColliderInSet( hit.collider , perchColliders ) ) continue;

            float dot = wantUp ? hit.normal.y : -hit.normal.y;
            if ( dot < s.minNormalDot ) continue;

            CreatePerchChild( hit.point , hit.normal , gen++ );
        }

        Debug.Log( $"[PreyInterestPoint] Generated {gen}/{pointCount} points on collider(s)." , this );
    }

    private static bool ColliderInSet( Collider c , Collider[] set )
    {
        for ( int i = 0; i < set.Length; i++ )
            if ( set[i] == c ) return true;
        return false;
    }

    private void GenerateInArea()
    {
        var     s          = perchInArea;
        bool    wantUp     = s.facing == PerchFacing.Up;
        Vector3 castDir    = wantUp ? Vector3.down : Vector3.up;
        float   yOffset    = wantUp ? s.castHeight : -s.castHeight;
        int     pointCount = manager != null ? manager.maxPray : s.pointCount;
        int     gen        = 0;

        for ( int i = 0; i < pointCount * 10 && gen < pointCount; i++ ) {
            var xz     = Random.insideUnitCircle * s.radius;
            var origin = transform.position + new Vector3( xz.x , yOffset , xz.y );

            if ( !Physics.Raycast( origin , castDir , out var hit , s.castHeight * 2f , s.groundLayers ) ) continue;

            float dot = wantUp ? hit.normal.y : -hit.normal.y;
            if ( dot < s.minNormalDot ) continue;

            CreatePerchChild( hit.point , hit.normal , gen++ );
        }

        Debug.Log( $"[PreyInterestPoint] Generated {gen}/{pointCount} points in area." , this );
    }

    private void CreatePerchChild( Vector3 pos , Vector3 normal , int index )
    {
        var go = new GameObject( $"_perch_{index}" );
        go.transform.SetParent( transform , true );
        go.transform.position = pos;
        go.transform.up       = normal;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Color col;
        if      ( type == InterestPointType.Perch       ) col = new Color( 0.3f , 0.7f , 1f );
        else if ( type == InterestPointType.Updraft     ) col = Color.green;
        else if ( type == InterestPointType.NewInterest ) col = new Color( 1f , 0.7f , 0.1f );
        else if ( type == InterestPointType.NewCalm     ) col = new Color( 0.6f , 0.9f , 0.6f );
        else if ( type == InterestPointType.Despawn     ) col = new Color( 1f , 0.2f , 0.2f );
        else                                              col = Color.grey;

        // notice volume (outer) and enter volume (inner), per the entrance shape
        DrawEntranceVolumeGizmo( col );

        // center sphere
        Gizmos.color = new Color( col.r , col.g , col.b , 0.9f );
        Gizmos.DrawSphere( transform.position , 0.4f );

        if ( type == InterestPointType.Perch ) {
            float r = perchSubType == PerchSubType.OnCollider ? perchOnCollider.radius
                    : perchSubType == PerchSubType.Field      ? perchField.radius
                    :                                           perchInArea.radius;
            Gizmos.color = new Color( col.r , col.g , col.b , 0.2f );
            Gizmos.DrawWireSphere( transform.position , r );

            // generated child points
            for ( int i = 0; i < transform.childCount; i++ ) {
                var child = transform.GetChild( i );
                if ( !child.name.StartsWith( "_perch_" ) ) continue;
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere( child.position , 0.2f );
                Gizmos.DrawLine( child.position , child.position + child.up * 0.5f );
            }
        }

        if ( type == InterestPointType.Updraft ) {
            var us      = updraftSettings;
            int curlSign = us.curlDirection == CurlDirection.CounterClockwise ? 1 : -1;
            var tangent  = Vector3.Cross( Vector3.up , Vector3.forward ) * curlSign;

            Gizmos.color = Color.green;
            Gizmos.DrawLine( transform.position , transform.position + Vector3.up * us.forceUp );

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine( transform.position , transform.position + tangent.normalized * us.curlForce );

            Gizmos.color = new Color( 1f , 0.4f , 0.1f );
            var inDir = (transform.position - (transform.position + Vector3.forward * 5f)).normalized;
            Gizmos.DrawLine( transform.position + Vector3.forward * 5f ,
                             transform.position + Vector3.forward * (5f - us.forceIn) );

            // soaring band: top (desiredAltitude) and bottom (desiredAltitude - altitudeRange)
            float r     = entranceShape == EntranceShape.Cylinder ? noticeRadius : 5f;
            var   topP  = transform.position + Vector3.up * us.desiredAltitude;
            var   botP  = transform.position + Vector3.up * Mathf.Max( 0f , us.desiredAltitude - us.altitudeRange );
            UnityEditor.Handles.color = new Color( 0.4f , 0.9f , 1f , 0.7f );
            UnityEditor.Handles.DrawWireDisc( topP , Vector3.up , r );
            UnityEditor.Handles.color = new Color( 0.4f , 0.9f , 1f , 0.3f );
            UnityEditor.Handles.DrawWireDisc( botP , Vector3.up , r );
            UnityEditor.Handles.DrawLine( topP + Vector3.right * r , botP + Vector3.right * r );
            UnityEditor.Handles.Label( topP + Vector3.right * r , $"soar band {us.desiredAltitude - us.altitudeRange:F0}–{us.desiredAltitude:F0}" );
        }

        string lbl = alwaysInteresting ? $"[{type}  ★  p={priority:F1}]" : $"[{type}  p={priority:F1}]";
        UnityEditor.Handles.color = col;
        UnityEditor.Handles.Label( transform.position + Vector3.up * 1.2f , lbl );
    }

    // Draws the notice volume (faint) and enter volume (strong) for this point's entrance shape,
    // including the Plane shape. Shared by OnDrawGizmosSelected and PreyManager's spawn-point
    // debug overlay so both render the same volumes.
    public void DrawEntranceVolumeGizmo( Color col )
    {
        var faint  = new Color( col.r , col.g , col.b , 0.12f );
        var strong = new Color( col.r , col.g , col.b , 0.35f );

        if ( entranceShape == EntranceShape.Cylinder ) {
            float h = Mathf.Max( noticeRadius , type == InterestPointType.Updraft ? updraftSettings.desiredAltitude : 0f );
            DrawWireCylinder( transform.position , noticeRadius , h , faint );
            DrawWireCylinder( transform.position , enterRadius  , h , strong );
        } else if ( entranceShape == EntranceShape.Collider ) {
            // notice is still a sphere; the enter volume IS the assigned collider's bounds
            Gizmos.color = faint;
            Gizmos.DrawWireSphere( transform.position , noticeRadius );
            if ( entranceCollider != null ) {
                var b = entranceCollider.bounds;
                Gizmos.color = strong;
                Gizmos.DrawWireCube( b.center , b.size );
            }
        } else if ( entranceShape == EntranceShape.Plane ) {
            // horizontal plane at the point's Y — everything below it is "inside"
            float s2 = Mathf.Max( noticeRadius , enterRadius , 1f );
            var c = transform.position;
            UnityEditor.Handles.color = strong;
            UnityEditor.Handles.DrawSolidRectangleWithOutline(
                new[] { c + new Vector3( -s2 , 0 , -s2 ) , c + new Vector3( -s2 , 0 , s2 ) ,
                        c + new Vector3(  s2 , 0 ,  s2 ) , c + new Vector3(  s2 , 0 , -s2 ) } ,
                new Color( col.r , col.g , col.b , 0.10f ) , strong );
            UnityEditor.Handles.Label( c + Vector3.right * s2 , "inside = below this plane" );
        } else {
            Gizmos.color = faint;
            Gizmos.DrawWireSphere( transform.position , noticeRadius );
            Gizmos.color = strong;
            Gizmos.DrawWireSphere( transform.position , enterRadius );
        }
    }

    // Vertical cylinder: bottom disc at base, top disc at base + height, plus 4 risers.
    private static void DrawWireCylinder( Vector3 baseCenter , float radius , float height , Color color )
    {
        UnityEditor.Handles.color = color;
        var top = baseCenter + Vector3.up * height;
        UnityEditor.Handles.DrawWireDisc( baseCenter , Vector3.up , radius );
        UnityEditor.Handles.DrawWireDisc( top        , Vector3.up , radius );

        for ( int i = 0; i < 4; i++ ) {
            float a   = i * Mathf.PI * 0.5f;
            var   off = new Vector3( Mathf.Cos( a ) , 0f , Mathf.Sin( a ) ) * radius;
            UnityEditor.Handles.DrawLine( baseCenter + off , top + off );
        }
    }
#endif
}
