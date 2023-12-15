using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
public class WrenBod : MonoBehaviour
{
    public Wren wren;

    public Transform[] leftWingPoints;
    public Transform[] rightWingPoints;

    public Transform[] leftWingCubes;
    public Transform[] rightWingCubes;


    public LineRenderer lineRenderer;

    public Transform head;
    public Transform tail;

    public Vector3 lookPos;

    public TrailRenderer mainTrail;


    public void SetColor()
    {
        var col = Color.HSVToRGB(Random.Range(0.001f, .999f), .9f, .5f);

        for (int i = 0; i < leftWingPoints.Length; i++)
        {
            var t = leftWingPoints[i].GetComponent<TrailRenderer>();
            if (t != null)
            {
                t.material.SetColor("_Color", col);
            }
        }


        for (int i = 0; i < rightWingPoints.Length; i++)
        {
            var t = rightWingPoints[i].GetComponent<TrailRenderer>();
            if (t != null)
            {
                t.material.SetColor("_Color", col);
            }
        }

        mainTrail.material.SetColor("_Color", col);
    }

    public void OnEnable()
    {
        SetColor();
    }


    public void LateUpdate()
    {

        float width;
        Vector3 v1 = new Vector3();
        Vector3 v2 = new Vector3();
        Vector3 v3 = new Vector3();
        Vector3 v4 = new Vector3();

        Vector3 v5 = new Vector3();
        float pMult;

        v1 = wren.transform.position;
        v2 = wren.physics.leftWing.transform.right;
        v3 = wren.physics.leftWing.transform.forward;
        v4 = wren.physics.leftWing.transform.up;
        v5 = transform.right;





        pMult = 1 - .3f * wren.physics.tuckAmountL;
        leftWingPoints[0].position = v1 - v5 * .4f;
        leftWingPoints[1].position = leftWingPoints[0].position - (v5 + v2) * pMult + v4 * 2 * (1 - pMult);
        leftWingPoints[2].position = leftWingPoints[1].position - v2 * 2 * pMult - v4 * 2 * (1 - pMult);

        leftWingPoints[0].LookAt(leftWingPoints[0].position + v3, v4);
        leftWingPoints[1].LookAt(leftWingPoints[1].position + v3, v4);
        leftWingPoints[2].LookAt(leftWingPoints[2].position + v3, v4);


        width = (leftWingPoints[0].position - leftWingPoints[1].position).magnitude;
        leftWingCubes[0].position = (leftWingPoints[0].position + leftWingPoints[1].position) / 2;
        leftWingCubes[0].localScale = new Vector3(width * .5f, width * .1f, width * 1.0f);
        leftWingCubes[0].LookAt(leftWingPoints[1], wren.physics.leftWing.up);


        width = (leftWingPoints[0].position - leftWingPoints[1].position).magnitude;
        leftWingCubes[1].position = (leftWingPoints[1].position + leftWingPoints[2].position) / 2;
        leftWingCubes[1].localScale = new Vector3(width * .5f, width * .1f, width * 1.0f);
        leftWingCubes[1].LookAt(leftWingPoints[2], wren.physics.leftWing.up);



        v1 = wren.transform.position;
        v2 = -wren.physics.rightWing.transform.right;
        v3 = wren.physics.rightWing.transform.forward;
        v4 = wren.physics.rightWing.transform.up;
        v5 = -transform.right;





        pMult = 1 - .3f * wren.physics.tuckAmountR;
        rightWingPoints[0].position = v1 - v5 * .4f;
        rightWingPoints[1].position = rightWingPoints[0].position - (v5 + v2) * pMult + v4 * 2 * (1 - pMult);
        rightWingPoints[2].position = rightWingPoints[1].position - v2 * 2 * pMult - v4 * 2 * (1 - pMult);

        rightWingPoints[0].LookAt(rightWingPoints[0].position + v3, v4);
        rightWingPoints[1].LookAt(rightWingPoints[1].position + v3, v4);
        rightWingPoints[2].LookAt(rightWingPoints[2].position + v3, v4);


        width = (rightWingPoints[0].position - rightWingPoints[1].position).magnitude;
        rightWingCubes[0].position = (rightWingPoints[0].position + rightWingPoints[1].position) / 2;
        rightWingCubes[0].localScale = new Vector3(width * .5f, width * .1f, width * 1.0f);
        rightWingCubes[0].LookAt(rightWingPoints[1], wren.physics.rightWing.up);


        width = (rightWingPoints[0].position - rightWingPoints[1].position).magnitude;
        rightWingCubes[1].position = (rightWingPoints[1].position + rightWingPoints[2].position) / 2;
        rightWingCubes[1].localScale = new Vector3(width * .5f, width * .1f, width * 1.0f);
        rightWingCubes[1].LookAt(rightWingPoints[2], wren.physics.rightWing.up);


        lookPos = Vector3.Lerp(lookPos, ((wren.physics.vel - wren.physics.oVel).normalized + wren.physics.vel.normalized).normalized, .06f);
        head.LookAt(head.position + lookPos, transform.up);






    }


    public Vector3 quadBez(Vector3 v1, Vector3 v2, Vector3 v3, float t)
    {
        return (1 - t) * (1 - t) * v1 + 2 * (1 - t) * t * v1 + t * t * v2;
    }


}
