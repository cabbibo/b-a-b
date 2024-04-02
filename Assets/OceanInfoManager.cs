using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;
using WrenUtils;

public class OceanInfoManager : MonoBehaviour
{

    public float height;
    public Vector3 normal;
    public Vector3 displacement;

    SampleHeightHelper sampleHeightHelper;

    void OnEnable()
    {
        sampleHeightHelper = new SampleHeightHelper();
    }
    // Update is called once per frame
    void Update()
    {

        if (God.wren)
        {

            if (OceanRenderer.Instance != null)
            {

                print("heyyyy");
                //SampleHeightHelper sampleHeightHelper = new SampleHeightHelper();
                //sampleHeightHelper.Init(transform.position, 2f * transform.lossyScale.magnitude);
                //sampleHeightHelper.Sample(out displacement, out normal, out var waterSurfaceVel);
                //height = OceanRenderer.Instance.SeaLevel + displacement.y;

            }

        }

    }
}
