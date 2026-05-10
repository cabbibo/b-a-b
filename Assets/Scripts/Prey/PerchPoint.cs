using UnityEngine;
using System.Collections.Generic;

// Add this component to any Transform you want birds to land on.
// Birds query the global registry; PreyManager can also supply an explicit list.
public class PerchPoint : MonoBehaviour
{
    public static readonly List<PerchPoint> All = new List<PerchPoint>();

    [HideInInspector] public bool occupied;

    private void OnEnable()  => All.Add( this );
    private void OnDisable() => All.Remove( this );

    // Returns nearest unoccupied perch within maxDist.
    // If overrideList is provided, searches that instead of the global registry.
    public static Transform FindNearest( Vector3 pos , float maxDist , Transform[] overrideList = null )
    {
        float     bestSqr = maxDist * maxDist;
        Transform best    = null;

        if ( overrideList != null && overrideList.Length > 0 ) {
            foreach ( var t in overrideList ) {
                if ( t == null ) continue;
                float d = (t.position - pos).sqrMagnitude;
                if ( d < bestSqr ) { bestSqr = d; best = t; }
            }
            return best;
        }

        foreach ( var p in All ) {
            if ( p.occupied ) continue;
            float d = (p.transform.position - pos).sqrMagnitude;
            if ( d < bestSqr ) { bestSqr = d; best = p.transform; }
        }

        return best;
    }
}
