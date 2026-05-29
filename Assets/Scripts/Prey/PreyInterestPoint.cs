using UnityEngine;

public enum InterestPointType { Perch, Updraft, NewInterest, NewCalm }
public enum PerchSubType      { OnCollider, InArea }
public enum PerchFacing       { Up, Down }
public enum CurlDirection     { CounterClockwise, Clockwise }

// ── Per-type settings ─────────────────────────────────────────────────────────

[System.Serializable]
public class PerchOnColliderSettings
{
    public Collider    collider;
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

[System.Serializable]
public class UpdraftPointSettings
{
    public float         forceUp       = 3f;
    public float         forceIn       = 1f;
    public float         curlForce     = 2f;
    public CurlDirection curlDirection = CurlDirection.CounterClockwise;
}

// ── Component ─────────────────────────────────────────────────────────────────

public class PreyInterestPoint : MonoBehaviour
{
    public InterestPointType type                  = InterestPointType.NewCalm;
    public float             noticeRadius          = 40f;
    public bool              alwaysInteresting     = false;
    public float             priority              = 1f;
    public float             timeToRemainInterested = 0f;

    // Optional: when set, point count for generation uses manager.maxPray
    public PreyManager manager;

    // Type-specific — shown via custom editor only
    public PerchSubType            perchSubType    = PerchSubType.InArea;
    public PerchOnColliderSettings perchOnCollider = new PerchOnColliderSettings();
    public PerchInAreaSettings     perchInArea     = new PerchInAreaSettings();
    public UpdraftPointSettings    updraftSettings = new UpdraftPointSettings();

    // ── Perch point generation ────────────────────────────────────────────────

    public void GeneratePerchPoints()
    {
        ClearPerchPoints();
        if ( perchSubType == PerchSubType.OnCollider ) GenerateOnCollider();
        else                                           GenerateInArea();
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

        if ( s.collider == null ) {
            Debug.LogWarning( "[PreyInterestPoint] OnCollider: no collider assigned." , this );
            return;
        }

        bool    wantUp     = s.facing == PerchFacing.Up;
        var     bounds     = s.collider.bounds;
        float   originY    = wantUp ? bounds.max.y + 1f : bounds.min.y - 1f;
        Vector3 castDir    = wantUp ? Vector3.down : Vector3.up;
        float   castDist   = (bounds.size.y + 2f) * 2f;
        int     pointCount = manager != null ? manager.maxPray : s.pointCount;
        int     gen        = 0;

        for ( int i = 0; i < pointCount * 20 && gen < pointCount; i++ ) {
            var xz     = Random.insideUnitCircle * s.radius;
            var origin = new Vector3( transform.position.x + xz.x , originY , transform.position.z + xz.y );

            if ( !Physics.Raycast( origin , castDir , out var hit , castDist ) ) continue;
            if ( hit.collider != s.collider ) continue;

            float dot = wantUp ? hit.normal.y : -hit.normal.y;
            if ( dot < s.minNormalDot ) continue;

            CreatePerchChild( hit.point , hit.normal , gen++ );
        }

        Debug.Log( $"[PreyInterestPoint] Generated {gen}/{pointCount} points on collider." , this );
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
        else                                              col = Color.grey;

        // notice radius
        Gizmos.color = new Color( col.r , col.g , col.b , 0.12f );
        Gizmos.DrawWireSphere( transform.position , noticeRadius );

        // center sphere
        Gizmos.color = new Color( col.r , col.g , col.b , 0.9f );
        Gizmos.DrawSphere( transform.position , 0.4f );

        if ( type == InterestPointType.Perch ) {
            float r = perchSubType == PerchSubType.OnCollider ? perchOnCollider.radius : perchInArea.radius;
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
        }

        string lbl = alwaysInteresting ? $"[{type}  ★  p={priority:F1}]" : $"[{type}  p={priority:F1}]";
        UnityEditor.Handles.color = col;
        UnityEditor.Handles.Label( transform.position + Vector3.up * 1.2f , lbl );
    }
#endif
}
