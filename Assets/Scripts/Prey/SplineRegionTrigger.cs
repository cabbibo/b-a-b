using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using WrenUtils;

// Add this component alongside a PreyManager to use a spline as the enter/exit region
// instead of a collider trigger. Calls manager.OnWrenEnter / OnWrenExit directly.
public class SplineRegionTrigger : MonoBehaviour
{
    public PreyManager     manager;
    public SplineContainer spline;
    public float           enterDistance = 20f;
    public float           exitDistance  = 30f;
    public float           checkInterval = 0.1f;

    private Vector3 boundsCenter;
    private float   boundingRadius;

    private void OnEnable()
    {
        CacheBounds();
        StartCoroutine( CheckRoutine() );
    }

    private void CacheBounds()
    {
        if ( spline == null ) return;

        var s       = spline.Spline;
        var xform   = spline.transform;
        int samples = Mathf.Max( 32 , s.Count * 4 );

        var center = Vector3.zero;
        for ( int i = 0 ; i < samples ; i++ ) {
            float t = (float)i / (samples - 1);
            center += xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( s , t ) );
        }
        center /= samples;

        float maxSqr = 0f;
        for ( int i = 0 ; i < samples ; i++ ) {
            float t = (float)i / (samples - 1);
            var p = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( s , t ) );
            maxSqr = Mathf.Max( maxSqr , (p - center).sqrMagnitude );
        }

        boundsCenter   = center;
        boundingRadius = Mathf.Sqrt( maxSqr );
    }

    private IEnumerator CheckRoutine()
    {
        var wait = new WaitForSeconds( checkInterval );
        while ( true ) {
            CheckRegion();
            yield return wait;
        }
    }

    private void CheckRegion()
    {
        if ( spline == null || manager == null ) return;

        var wrenT = God.wren != null ? God.wren.transform
                  : manager.debugWren != null ? manager.debugWren.transform
                  : null;
        if ( wrenT == null ) return;

        var   wrenPos   = wrenT.position;
        bool  inside    = manager.birdInsideRegion;
        float threshold = inside ? exitDistance : enterDistance;

        float outerLimit = boundingRadius + threshold;
        if ( (wrenPos - boundsCenter).sqrMagnitude > outerLimit * outerLimit ) {
            if ( inside ) manager.OnWrenExit();
            return;
        }

        var   localWren = (float3)spline.transform.InverseTransformPoint( wrenPos );
        SplineUtility.GetNearestPoint( spline.Spline , localWren , out float3 nearestLocal , out float _ );
        float dist = Vector3.Distance( wrenPos , spline.transform.TransformPoint( (Vector3)nearestLocal ) );

        if ( !inside && dist <= enterDistance ) manager.OnWrenEnter();
        else if ( inside && dist > exitDistance ) manager.OnWrenExit();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if ( spline == null ) return;

        var s       = spline.Spline;
        var xform   = spline.transform;
        int samples = Mathf.Max( 64 , s.Count * 8 );

        for ( int i = 0 ; i < samples ; i++ ) {
            float t0 = (float)i       / samples;
            float t1 = (float)(i + 1) / samples;
            var p0 = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( s , t0 ) );
            var p1 = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( s , t1 ) );

            var tangent = (p1 - p0).normalized;
            var perp    = Vector3.Cross( tangent , Vector3.up ).normalized;

            Gizmos.color = new Color( 0.2f , 1f , 0.3f , 0.4f );
            Gizmos.DrawLine( p0 + perp * enterDistance , p1 + perp * enterDistance );
            Gizmos.DrawLine( p0 - perp * enterDistance , p1 - perp * enterDistance );

            Gizmos.color = new Color( 1f , 0.5f , 0.1f , 0.25f );
            Gizmos.DrawLine( p0 + perp * exitDistance , p1 + perp * exitDistance );
            Gizmos.DrawLine( p0 - perp * exitDistance , p1 - perp * exitDistance );
        }
    }
#endif
}
