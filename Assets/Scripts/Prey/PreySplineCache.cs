using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

// Bakes a STATIC SplineContainer into a flat world-space lookup table of
// (position, tangent) samples. Per-bird nearest-point queries then become a
// cheap array scan instead of SplineUtility.GetNearestPoint's per-call
// subdivision search on the live curve.
//
// Baked once per manager and shared by every bird that follows the spline, in
// world space — so queries do no InverseTransformPoint/TransformPoint either.
// Accuracy is set by the sample count: more samples → closer to the true
// nearest point, at a one-time bake cost only.
public class PreySplineCache
{
    private Vector3[] _pos;
    private Vector3[] _tan;
    private int       _count;

    public bool IsBaked     => _count > 1;
    public int  SampleCount => _count;

    public void Bake( SplineContainer container , int samples )
    {
        _count = 0;
        if ( container == null || container.Splines.Count == 0 ) return;

        var sp    = container.Spline;
        var xform = container.transform;

        int n = Mathf.Max( 2 , samples );
        if ( _pos == null || _pos.Length != n ) { _pos = new Vector3[n]; _tan = new Vector3[n]; }

        for ( int i = 0; i < n; i++ ) {
            float t = (float)i / ( n - 1 );

            // evaluate in local spline space (same API SplineForce used), then to world
            float3 localPos = SplineUtility.EvaluatePosition( sp , t );
            float3 localTan = SplineUtility.EvaluateTangent( sp , t );

            _pos[i] = xform.TransformPoint( new Vector3( localPos.x , localPos.y , localPos.z ) );
            Vector3 tanW = xform.TransformDirection( new Vector3( localTan.x , localTan.y , localTan.z ) );
            _tan[i] = tanW.sqrMagnitude > 1e-6f ? tanW.normalized : Vector3.forward;
        }
        _count = n;
    }

    // Nearest baked sample to a world position. False if not baked yet.
    public bool Nearest( Vector3 worldPos , out Vector3 nearestWorld , out Vector3 tangentWorld )
    {
        if ( _count < 2 ) { nearestWorld = worldPos; tangentWorld = Vector3.forward; return false; }

        int   best    = 0;
        float bestSqr = float.MaxValue;
        for ( int i = 0; i < _count; i++ ) {
            float d = ( _pos[i] - worldPos ).sqrMagnitude;
            if ( d < bestSqr ) { bestSqr = d; best = i; }
        }

        nearestWorld = _pos[best];
        tangentWorld = _tan[best];
        return true;
    }
}
