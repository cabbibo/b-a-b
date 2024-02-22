using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;
[CustomEditor(typeof(PushableCloudGPUBrush))]
public class PushableCloudGPUBrushEditor : Editor
{

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = (PushableCloudGPUBrush)target;

        var data = t.brushData;
    }
}
#endif

public class PushableCloudGPUBrush : MonoBehaviour
{
    public PushableCloudGPU.BrushData brushData;

    public float Radius { get { return transform.localScale.x / 2.0f; } }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        if (brushData.hole)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Radius * brushData.holeFalloff);

        }

        if (brushData.light)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, brushData.lightRadius);

        }
        Gizmos.DrawWireSphere(transform.position, Radius );
        Gizmos.DrawWireSphere(transform.position + brushData.forwardAmount * transform.forward, Radius );
    }
}
