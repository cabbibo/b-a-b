using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class DebugTerrainUp : MonoBehaviour
{

    public bool useWren;
    public LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 fPos;
        if (useWren)
        {
            fPos = God.wren.transform.position;
        }
        else
        {
            fPos = transform.position;
        }


        Vector3 nPos = God.NormalizedPositionInMap(fPos);

        Vector3 n = God.terrain.terrainData.GetInterpolatedNormal(nPos.x, nPos.z);
        Vector3 p = new Vector3(fPos.x, God.terrain.SampleHeight(fPos), fPos.z);

        Debug.DrawRay(transform.position, n, Color.red);

        if (useWren)
        {
            lineRenderer.SetPosition(0, p);
            lineRenderer.SetPosition(1, p + n * 100);
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + n * 1);
        }


    }
}
