using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crest;

[ExecuteAlways]
public class OceanDebuger : MonoBehaviour
{


    public LineRenderer lineRenderer;
    public SampleHeightHelper sampleHeightHelper;

    public GameObject otherSphere;


    void OnEnable()
    {


    }


    // Update is called once per frame
    void Update()
    {

        /* if (sampleHeightHelper == null)
         {
             sampleHeightHelper = new SampleHeightHelper();
         }

         var ocean = OceanRenderer.Instance;
         if (ocean == null) return;


         Vector3 disp;
         sampleHeightHelper.Init(transform.position, 2f * 1);
         sampleHeightHelper.Sample(out disp, out _, out _);

         //        print(disp);

         lineRenderer.SetPosition(0, transform.position);

         float seaLevelHeight = OceanRenderer.Instance.SeaLevel;

         Vector3 touchPosition = new Vector3(transform.position.x, seaLevelHeight + disp.y, transform.position.z);
         lineRenderer.SetPosition(1, touchPosition);

         otherSphere.transform.position = touchPosition;*/

    }
}
