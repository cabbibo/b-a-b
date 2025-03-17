using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteAlways]
public class GPUCloud2 : MonoBehaviour
{

    public ComputeShader computeShader;
    public int count;

    public ComputeBuffer buffer;

    public uint numThreads;
    public int numGroups;

    public MaterialPropertyBlock mpb;

    public Material material;

    public void OnEnable()
    {

        if (buffer != null) { buffer.Release(); }
        buffer = new ComputeBuffer(count, sizeof(float) * 16);



        uint y; uint z;
        computeShader.GetKernelThreadGroupSizes(0, out numThreads, out y, out z);

        numGroups = ((int)count + ((int)numThreads - 1)) / (int)numThreads;

    }

    public void OnDisable()
    {
        buffer.Release();
    }


    public void SetShaderInfo()
    {
        computeShader.SetBuffer(0, "_VertBuffer", buffer);
        computeShader.SetInt("_VertBuffer_COUNT", count);
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetMatrix("_Transform", transform.localToWorldMatrix);



    }

    public void Dispatch() { computeShader.Dispatch(0, numGroups, 1, 1); }

    public void Update()
    {

        SetShaderInfo();
        Dispatch();

        if (mpb == null) { mpb = new MaterialPropertyBlock(); }

        mpb.SetBuffer("_VertBuffer", buffer);
        mpb.SetInt("_Count", count);
        mpb.SetVector("_MainCameraPos", Camera.main.transform.position);
        mpb.SetVector("_MainCameraForward", Camera.main.transform.forward);
        mpb.SetVector("_MainCameraRight", Camera.main.transform.right);
        mpb.SetVector("_MainCameraUp", Camera.main.transform.up);

        Graphics.DrawProcedural(material, new Bounds(transform.position, Vector3.one * 50000), MeshTopology.Triangles, count * 3 * 2, 1, null, mpb, ShadowCastingMode.On, true, LayerMask.NameToLayer("Debug"));


    }
}
