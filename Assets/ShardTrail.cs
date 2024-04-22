using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WrenUtils;


[ExecuteAlways]
public class ShardTrail : MonoBehaviour
{
    public int maxShards;
    public int currentShards;
    public Wren wren;
    public Material debugMaterial;
    public ComputeShader shader;
    public ComputeBuffer shardBuffer;

    public ComputeBuffer vertBuffer;
    public ComputeBuffer triBuffer;
    public Mesh[] meshes;


    public int numPointsTrail;
    public Vector3[] trailPoints;

    public ComputeBuffer trailBuffer;


    public LineRenderer debugLineRenderer;



    public Mesh shardMesh;
    /*
            public int totalMeshPoints;
            public int vertsPerMesh;
            public int totalTris;
            public int trisPerMesh;*/

    public int numGroups;
    public uint numThreads;


    public MaterialPropertyBlock mpb;

    public void OnEnable()
    {
        Create();
    }

    public void OnDisable()
    {
        Destroy();
    }


    public void Update()
    {
        RenderShards();
    }

    public void FixedUpdate()
    {
        UpdateShards();
    }

    public void Create()
    {

        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }


        //        vertBuffer = new ComputeBuffer(shardMesh.vertices.Length, 8 * sizeof(float));
        //        triBuffer = new ComputeBuffer(shardMesh.triangles.Length, sizeof(int));


        maxShards = wren.shards.maxShards - wren.shards.numShardsInBody;

        shardBuffer = new ComputeBuffer(maxShards, 16 * sizeof(float));


        trailPoints = new Vector3[numPointsTrail];
        trailBuffer = new ComputeBuffer(numPointsTrail, 3 * sizeof(float));


        for (int i = 0; i < numPointsTrail; i++)
        {
            trailPoints[i] = wren.bird.shoulder.position - wren.bird.shoulder.forward * i * 0.04f;
        }


        populateMeshData();

        shader.SetFloat("_Reset", 1);
        UpdateShards();
        shader.SetFloat("_Reset", 0);




    }


    public int totalVerts;
    public int totalTris;

    public int allVerts;
    public int allTris;


    public void populateMeshData()
    {


        totalVerts = shardMesh.vertices.Length;
        totalTris = shardMesh.triangles.Length;


        float[] values = new float[totalVerts * 8];


        Vector3[] positions = shardMesh.vertices;
        Vector3[] normals = shardMesh.normals;
        Vector2[] uvs = shardMesh.uv;

        for (int j = 0; j < shardMesh.vertices.Length; j++)
        {

            values[j * 8 + 0] = positions[j].x;
            values[j * 8 + 1] = positions[j].y;
            values[j * 8 + 2] = positions[j].z;

            values[j * 8 + 3] = normals[j].x;
            values[j * 8 + 4] = normals[j].y;
            values[j * 8 + 5] = normals[j].z;

            if (j < uvs.Length)
            {

                values[j * 8 + 6] = uvs[j].x;
                values[j * 8 + 7] = uvs[j].y;
            }
            else
            {

                values[j * 8 + 6] = 0;
                values[j * 8 + 7] = 0;
            }

        }


        vertBuffer = new ComputeBuffer(totalVerts, 8 * sizeof(float));
        triBuffer = new ComputeBuffer(totalTris, sizeof(int));

        vertBuffer.SetData(values);

        triBuffer.SetData(shardMesh.triangles);


    }


    // Start is called before the first frame update
    public void Destroy()
    {

        if (vertBuffer != null) { vertBuffer.Dispose(); }
        if (triBuffer != null) { triBuffer.Dispose(); }
        if (shardBuffer != null) { shardBuffer.Dispose(); }
        if (trailBuffer != null) { trailBuffer.Dispose(); }
    }

    // Update is called once per frame
    public void UpdateShards()
    {
        for (int i = numPointsTrail - 1; i > 0; i--)
        {
            trailPoints[i] = trailPoints[i - 1];
        }

        trailPoints[0] = wren.bird.shoulder.position;


        debugLineRenderer.positionCount = numPointsTrail;
        debugLineRenderer.SetPositions(trailPoints);

        trailBuffer.SetData(trailPoints);

        if (shardBuffer != null)
        {

            //print("GPU BODY BIRD HEAD :" + bird.head.position );

            uint y; uint z;
            shader.GetKernelThreadGroupSizes(0, out numThreads, out y, out z);


            shader.SetBuffer(0, "_VertBuffer", shardBuffer);
            shader.SetBuffer(0, "_TrailBuffer", trailBuffer);
            shader.SetInt("_VertBuffer_COUNT", maxShards);
            shader.SetInt("_TrailBuffer_COUNT", numPointsTrail);
            shader.SetVector("_Chest", wren.bird.shoulder.position);
            shader.SetFloat("_Time", Time.time);
            shader.SetMatrix("_WorldToLocal", wren.transform.worldToLocalMatrix);
            shader.SetFloat("_TotalShardsInBody", wren.shards.numShardsInBody);
            shader.SetFloat("_ONumShards", wren.shards.oNumShards);
            shader.SetFloat("_NumShards", wren.shards.numShards);
            shader.SetFloat("_TmpNumShards", wren.shards.tmpNumShards);
            shader.SetVector("_WrenVel", wren.physics.vel);
            shader.SetVector("_CollectionPosition", wren.shards.collectPosition);
            shader.SetFloat("_CollectionType", wren.shards.collectType);




            numGroups = (maxShards + ((int)numThreads - 1)) / (int)numThreads;
            if (numGroups <= 0) { numGroups = 1; }

            shader.Dispatch(0, numGroups, 1, 1);


        }



    }

    void RenderShards()
    {

        allVerts = shardMesh.vertices.Length * maxShards;
        allTris = shardMesh.triangles.Length * maxShards;

        mpb.SetBuffer("_ShardBuffer", shardBuffer);
        mpb.SetBuffer("_VertBuffer", vertBuffer);
        mpb.SetBuffer("_TriBuffer", triBuffer);
        mpb.SetInt("_Count", maxShards);
        mpb.SetInt("_TriCount", totalTris);
        mpb.SetInt("_VertCount", totalVerts);
        mpb.SetMatrix("_Model", wren.transform.localToWorldMatrix);
        Graphics.DrawProcedural(debugMaterial, new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles, maxShards * totalTris, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Default"));


    }

}
