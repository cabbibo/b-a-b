using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteAlways]
public class PlaceParticlesOnDepthMap : MonoBehaviour
{


    public int count;
    public ComputeBuffer _VertBuffer;
    public ComputeShader shader;
    public int kernel;

    public Texture2D depthMap;

    public Material material;
    public Material material2;
    public Material material3;
    public MaterialPropertyBlock mpb;

    public int structSize = 16;

    public string kernelName;

    // Start is called before the first frame update

    int numGroups;
    uint numThreads;
    void OnEnable()
    {
        _VertBuffer = new ComputeBuffer(count, sizeof(float) * structSize);
        kernel = shader.FindKernel(kernelName);
        GetNumThreads();
        GetNumGroups();

    }

    void OnDisable()
    {
        if (_VertBuffer != null) { _VertBuffer.Release(); }
        //_VertBuffer.Release();
        //_VertBuffer.Dispose();
    }


    public void GetNumThreads()
    {
        uint y; uint z;
        shader.GetKernelThreadGroupSizes(kernel, out numThreads, out y, out z);
    }

    public void GetNumGroups()
    {
        // numGroups = ((int)count + ((int)numThreads - 1)) / (int)numThreads;
        numGroups = ((int)count) / (int)numThreads;
    }


    public TerrainData terrainData;

    public Texture heightMap;


    // Update is called once per frame
    void Update()
    {


        heightMap = terrainData.heightmapTexture;
        //        print(terrainData.heightmapTexture);

        shader.SetMatrix("_CameraViewMatrix", Camera.main.worldToCameraMatrix);
        shader.SetMatrix("_CameraViewMatrixInverse", Camera.main.worldToCameraMatrix.inverse);
        shader.SetMatrix("_CameraProjectionMatrix", Camera.main.projectionMatrix);
        shader.SetMatrix("_CameraProjectionMatrixInverse", Camera.main.projectionMatrix.inverse);
        shader.SetVector("_CameraPosition", Camera.main.transform.position);

        shader.SetTexture(kernel, "_HeightMap", terrainData.heightmapTexture);
        shader.SetVector("_MapSize", terrainData.size);
        shader.SetFloat("_Time", Time.time);

        shader.SetBuffer(kernel, "_VertBuffer", _VertBuffer);
        shader.SetInt("_VertBuffer_COUNT", count);

        shader.Dispatch(kernel, numGroups, 1, 1);


        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }

        mpb.SetBuffer("_VertBuffer", _VertBuffer);
        mpb.SetInt("_Count", count);

        Graphics.DrawProcedural(material, new Bounds(transform.position, Vector3.one * 500000), MeshTopology.Triangles, count * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
        Graphics.DrawProcedural(material2, new Bounds(transform.position, Vector3.one * 500000), MeshTopology.Triangles, count * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
        Graphics.DrawProcedural(material3, new Bounds(transform.position, Vector3.one * 500000), MeshTopology.Triangles, count * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));

    }

}
