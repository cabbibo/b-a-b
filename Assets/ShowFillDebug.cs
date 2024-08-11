using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class ShowFillDebug : MonoBehaviour
{

    public LineRenderer lineRenderer;

    public float amountFilled;

    public float targetAmountFilled;


    public float radius;
    public int segments;

    public CrystalFiller crystalFiller;

    public MaterialPropertyBlock mpb;
    public void OnEnable()
    {

        mpb = new MaterialPropertyBlock();

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;
        CreatePoints();

        crystalFiller.OnPercentageFillEvent.AddListener(OnPercentageFill);

    }

    public void CreatePoints()
    {
        float x;
        float y;
        float z = 0f;

        float angle = 0f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            lineRenderer.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }



    public void OnPercentageFill(float percentage)
    {

        print("percentage: " + percentage);

        targetAmountFilled = percentage;
    }

    // Update is called once per frame
    void Update()
    {
        mpb.SetFloat("_AmountFilled", amountFilled);
        lineRenderer.SetPropertyBlock(mpb);


        amountFilled = Mathf.Lerp(amountFilled, targetAmountFilled, .01f);


    }
}
