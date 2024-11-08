using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EmitFromPoints : MonoBehaviour
{



    //public Vector4[] points;

    public ComputeBuffer buffer;
    public ComputeBuffer bufferVels;

    public int countMultiplier = 10;

    public int count;

    public void SetPoints(Vector4[] pl, Vector3[] vels)
    {
        //points = pl;
        count = pl.Length;

        if (buffer != null)
        {
            buffer.Release();
            bufferVels.Release();
        }

        buffer = new ComputeBuffer(count, sizeof(float) * 4);
        buffer.SetData(pl);

        bufferVels = new ComputeBuffer(count, sizeof(float) * 3);
        bufferVels.SetData(vels);


    }



    public MaterialPropertyBlock mpb;
    public Material material;

    public float distanceToZero;
    public float distanceToMax;
    public int maxCountMulitplier;

    public float dist;
    public float distance;

    public float lastCountFade;


    // Update is called once per frame
    void Update()
    {


        distance = Vector3.Distance(Camera.main.transform.position, transform.position);

        dist = (distance - distanceToMax) / (distanceToZero - distanceToMax);


        lastCountFade = Mathf.Lerp(maxCountMulitplier, 0, Mathf.Clamp01(dist));
        countMultiplier = Mathf.FloorToInt(lastCountFade);

        lastCountFade = lastCountFade - (float)countMultiplier;



        if (count > 0 && countMultiplier > 0)
        {
            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }

            mpb.SetBuffer("_Points", buffer);
            mpb.SetBuffer("_Vels", bufferVels);
            mpb.SetInt("_Count", count);
            mpb.SetInt("_CountMultiplier", countMultiplier);
            mpb.SetFloat("_LastCountFade", lastCountFade);
            //   print("Drawing " + points.Length + " points");

            Graphics.DrawProcedural(
                material,
                new Bounds(Vector3.zero, Vector3.one * 100000),
                MeshTopology.Triangles,
                count * 3 * 2 * countMultiplier,
                1,
                null,
                mpb
            );
        }

    }
}
