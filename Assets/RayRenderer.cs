using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteAlways]
public class RayRenderer : MonoBehaviour
{

    public Material material;

    public float rayLength = 1000;
    public float rayWidth = 0.1f;
    public float raySpeed = 1;

    public int numRays;

    public MaterialPropertyBlock mpb;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }

        mpb.SetFloat("_Length", rayLength);
        mpb.SetFloat("_Width", rayWidth);
        mpb.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);

        //print("rendeerrrr)");
        Graphics.DrawProcedural(material, new Bounds(Vector3.zero, Vector3.one * 10000), MeshTopology.Triangles, numRays * 3, 1, null, mpb, ShadowCastingMode.Off, false, 0);
    }
}
