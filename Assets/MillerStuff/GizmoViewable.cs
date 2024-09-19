using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoViewable : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 200);
    }
}
