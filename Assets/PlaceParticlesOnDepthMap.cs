using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using UnityEngine.Experimental.Rendering;


[ExecuteAlways]
public class PlaceParticlesOnDepthMap : MonoBehaviour
{


    public int count;
    public ComputeBuffer _VertBuffer;
    public ComputeShader shader;
    public int kernel;

    public Camera camera;


    public Material material;
    public Material material2;
    public Material material3;
    public Material material4;
    public MaterialPropertyBlock mpb;

    public int structSize = 16;

    public string kernelName;

    // Start is called before the first frame update

    int numGroups;
    uint numThreads;

    RenderTextureDescriptor textureDescriptor;

    RenderTexture texture;
    public int renderSize = 1024;
    void OnEnable()
    {


        textureDescriptor = new RenderTextureDescriptor(renderSize, renderSize, RenderTextureFormat.Depth, 24);
        texture = RenderTexture.GetTemporary(textureDescriptor);


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



    public MeshRenderer debugDepthRenderer;

    // Update is called once per frame
    void Update()
    {

        // camera.CopyFrom(Camera.main);
        // Set out depth camera properties to be the same as the main camera
        camera.fieldOfView = Camera.main.fieldOfView;
        camera.nearClipPlane = Camera.main.nearClipPlane;
        camera.farClipPlane = Camera.main.farClipPlane;
        camera.aspect = Camera.main.aspect;

        // set our camera to have the same transform as the main camera
        camera.transform.position = Camera.main.transform.position;
        camera.transform.rotation = Camera.main.transform.rotation;

        // Render out the depth texture
        //camera.targetTexture = depthTexture;

        //camera.targetTexture = renderTextureColor;
        camera.depthTextureMode = DepthTextureMode.DepthNormals;
        camera.SetTargetBuffers(texture.colorBuffer, texture.depthBuffer);
        camera.Render();



        debugDepthRenderer.sharedMaterial.SetTexture("_MainTex", texture);

        //        print(depthTexture.depthBuffer);

        heightMap = terrainData.heightmapTexture;
        //        print(terrainData.heightmapTexture);

        shader.SetMatrix("_CameraViewMatrix", Camera.main.worldToCameraMatrix);
        shader.SetMatrix("_CameraViewMatrixInverse", Camera.main.worldToCameraMatrix.inverse);
        shader.SetMatrix("_CameraProjectionMatrix", Camera.main.projectionMatrix);
        shader.SetMatrix("_CameraProjectionMatrixInverse", Camera.main.projectionMatrix.inverse);
        shader.SetMatrix("_CameraViewProjectionMatrixInverse", Camera.main.worldToCameraMatrix.inverse * Camera.main.projectionMatrix.inverse);
        shader.SetVector("_CameraPosition", Camera.main.transform.position);
        shader.SetFloat("_CameraNear", Camera.main.nearClipPlane);
        shader.SetFloat("_CameraFar", Camera.main.farClipPlane);


        float x = 1 - (Camera.main.farClipPlane / Camera.main.nearClipPlane);
        float y = Camera.main.farClipPlane / Camera.main.nearClipPlane;
        float z = x / Camera.main.farClipPlane;
        float w = y / Camera.main.farClipPlane;

        shader.SetVector("_ZBufferParams", new Vector4(x, y, z, w));
        shader.SetTexture(kernel, "_DepthTexture", texture);

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
        // Graphics.DrawProcedural(material4, new Bounds(transform.position, Vector3.one * 500000), MeshTopology.Triangles, count * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));

    }

}
