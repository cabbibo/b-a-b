using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[ExecuteAlways]
[RequireComponent( typeof(LineRenderer) )]
public class ShowSpline : MonoBehaviour
{
    public SplineContainer spline;
    public int             resolution = 64;

    private LineRenderer lr;

    private void OnEnable()
    {
        lr = GetComponent<LineRenderer>();
        Refresh();
    }

    public void Refresh()
    {
        if ( spline == null || lr == null ) {
            Debug.LogError( "Spline is null" );
            return;
        }

        int count = Mathf.Max( 2 , resolution );
        lr.positionCount = count;

        var s = spline.Spline;
        var xform = spline.transform;

        for ( int i = 0; i < count; i++ ) {
            float t = (float)i / (count - 1);
            var localPos = (Vector3)SplineUtility.EvaluatePosition( s , t );
            lr.SetPosition( i , xform.TransformPoint( localPos ) );
        }
    }
}