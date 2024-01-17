using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MagicCurve;
using WrenUtils;


[ExecuteAlways]
public class WindTunnel : MonoBehaviour
{

    public LineRenderer lineRenderer;
    public Curve curve;

    public Transform debugTransform;

    public float tunnelForwardForce = 1;
    public float tunnelInForce = 1;

    public float tunnelForceFalloff = 1;

    public float dampenForce;

    public float maxDistance;

    public bool debug;





    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 fPos = debugTransform.position;
        if (God.wren)
        {
            fPos = God.wren.transform.position;
        }


        Vector3 curveInfo = curve.GetClosestPointData(fPos);

        float lerpVal = Mathf.Clamp(curveInfo.z, 0, 1);


        Vector3 closestPos = Vector3.Lerp(curve.bakedPoints[(int)curveInfo.x], curve.bakedPoints[(int)curveInfo.y], lerpVal);

        Vector3 inDir = (closestPos - fPos).normalized;
        float dist = Vector3.Distance(closestPos, fPos);


        Vector3 closestForward = Vector3.Lerp(curve.bakedDirections[(int)curveInfo.x], curve.bakedDirections[(int)curveInfo.y], lerpVal);





        if (debug)
        {
            lineRenderer.SetPosition(0, fPos);
            lineRenderer.SetPosition(1, closestPos);

            print(curveInfo);
            print(lerpVal);
            print(closestForward);
            print(inDir);
            print(dist);
        }
        else
        {
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }


        if (dist > maxDistance)
        {
            return;
        }

        if (God.wren != null)
        {

            God.wren.physics.rb.AddForce(closestForward * tunnelForwardForce * (1 / tunnelForceFalloff * dist));
            God.wren.physics.rb.AddForce(inDir * tunnelInForce * (1 / tunnelForceFalloff * dist));
            God.wren.physics.rb.AddForce(-God.wren.physics.vel * dampenForce * (1 / tunnelForceFalloff * dist));

        }



    }




}
