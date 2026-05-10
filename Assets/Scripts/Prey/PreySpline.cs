using UnityEngine;
using System.Collections.Generic;

// A waypoint-based path birds can follow. Add waypoints as child Transforms,
// assign them to the waypoints array, enable loop for circular routes.
public class PreySpline : MonoBehaviour
{
    public static readonly List<PreySpline> All = new List<PreySpline>();

    public Transform[] waypoints;
    public bool        loop = true;

    private void OnEnable()  => All.Add( this );
    private void OnDisable() => All.Remove( this );

    // Returns the nearest point on this spline and sets t (0-1 along total path).
    public Vector3 GetNearestPoint( Vector3 pos , out float t )
    {
        t = 0;
        if ( waypoints == null || waypoints.Length < 2 ) return pos;

        float   bestSqr  = float.MaxValue;
        Vector3 best     = pos;
        int     segments = loop ? waypoints.Length : waypoints.Length - 1;

        for ( int i = 0; i < segments; i++ ) {
            int     j = (i + 1) % waypoints.Length;
            Vector3 a = waypoints[i].position;
            Vector3 b = waypoints[j].position;
            float   s = Mathf.Clamp01( Vector3.Dot( pos - a , b - a ) / (b - a).sqrMagnitude );
            Vector3 p = a + s * (b - a);
            float   d = (p - pos).sqrMagnitude;
            if ( d < bestSqr ) { bestSqr = d; best = p; t = (i + s) / segments; }
        }
        return best;
    }

    // Returns the forward direction at position t along the spline.
    public Vector3 GetForwardAt( float t )
    {
        if ( waypoints == null || waypoints.Length < 2 ) return Vector3.forward;
        int segments = loop ? waypoints.Length : waypoints.Length - 1;
        int i        = Mathf.FloorToInt( t * segments ) % waypoints.Length;
        int j        = (i + 1) % waypoints.Length;
        return (waypoints[j].position - waypoints[i].position).normalized;
    }

    // Advances t along the spline by a world-space distance, returning new t.
    public float AdvanceT( float t , float worldDist )
    {
        if ( waypoints == null || waypoints.Length < 2 ) return t;
        int segments = loop ? waypoints.Length : waypoints.Length - 1;
        // approximate: one segment's length
        int   i      = Mathf.FloorToInt( t * segments ) % waypoints.Length;
        int   j      = (i + 1) % waypoints.Length;
        float segLen = Vector3.Distance( waypoints[i].position , waypoints[j].position );
        float dt     = segLen > 0 ? worldDist / (segLen * segments) : 0;
        return loop ? (t + dt) % 1f : Mathf.Clamp01( t + dt );
    }

    public static PreySpline FindNearest( Vector3 pos , float maxDist )
    {
        PreySpline best    = null;
        float      bestSqr = maxDist * maxDist;
        foreach ( var s in All ) {
            float   dummy;
            Vector3 p = s.GetNearestPoint( pos , out dummy );
            float   d = (p - pos).sqrMagnitude;
            if ( d < bestSqr ) { bestSqr = d; best = s; }
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        if ( waypoints == null || waypoints.Length < 2 ) return;
        Gizmos.color = Color.yellow;
        int segments = loop ? waypoints.Length : waypoints.Length - 1;
        for ( int i = 0; i < segments; i++ ) {
            int j = (i + 1) % waypoints.Length;
            if ( waypoints[i] != null && waypoints[j] != null )
                Gizmos.DrawLine( waypoints[i].position , waypoints[j].position );
        }
    }
}
