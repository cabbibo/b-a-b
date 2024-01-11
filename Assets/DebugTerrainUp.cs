using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class DebugTerrainUp : MonoBehaviour
{

    public LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 nPos = God.NormalizedPositionInMap(transform.position);

        Vector3 n = God.terrain.terrainData.GetInterpolatedNormal(nPos.x, nPos.z);

        Debug.DrawRay(transform.position, n, Color.red);
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + n * 10);


    }
}
