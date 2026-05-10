using UnityEngine;
using System.Collections.Generic;

// Place in scene to create a thermal/updraft column birds will spiral inside.
public class UpdraftZone : MonoBehaviour
{
    public static readonly List<UpdraftZone> All = new List<UpdraftZone>();

    public float radius   = 20f;
    public float strength = 1f;

    private void OnEnable()  => All.Add( this );
    private void OnDisable() => All.Remove( this );

    public static UpdraftZone FindNearest( Vector3 pos , float maxDist )
    {
        UpdraftZone best    = null;
        float       bestSqr = maxDist * maxDist;
        foreach ( var z in All ) {
            float d = (z.transform.position - pos).sqrMagnitude;
            if ( d < bestSqr ) { bestSqr = d; best = z; }
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color( 0.2f , 0.8f , 1f , 0.3f );
        Gizmos.DrawWireSphere( transform.position , radius );
    }
}
