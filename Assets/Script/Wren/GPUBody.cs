using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WrenUtils;

public class GPUBody : MonoBehaviour
{

    public FullBird bird;

    public Material featherDebugLineMaterial;
    public Material featherDebugMaterial;
    public Material featherMaterial;


    public bool debugLinePoints;
    public bool debugFeatherPoints;
    public bool drawFeathers;

    public int numberScapularColumns;
    public int numberScapularRows;
    public int numberScapulars;


    public int numberTailFeathers;


    public int totalLinePoints;
    public int totalFeatherPoints;

    public ComputeBuffer lineBuffer;
    public ComputeBuffer featherBuffer;
    public ComputeShader shader;

    public ComputeBuffer vertBuffer;
    public ComputeBuffer triBuffer;
    public Mesh[] meshes;

    public Mesh scapularFeather;
    public Mesh tailFeather;

    public int totalMeshPoints;
    public int vertsPerMesh;
    public int totalTris;
    public int trisPerMesh;

    public int numGroups;
    public uint numThreads;


    public float lockedValue;

    public MaterialPropertyBlock mpb;
    public void Create()
    {

        meshes = new Mesh[2];
        meshes[0] = scapularFeather;
        meshes[1] = tailFeather;

        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
        totalFeatherPoints = (numberScapularRows) * numberScapularColumns + numberTailFeathers;
        // 8 lines
        totalLinePoints = numberScapularRows * 8;

        vertsPerMesh = meshes[0].vertices.Length;
        trisPerMesh = meshes[0].triangles.Length;

        totalMeshPoints = meshes.Length * meshes[0].vertices.Length;
        totalTris = meshes.Length * meshes[0].triangles.Length;

        vertBuffer = new ComputeBuffer(totalMeshPoints, 8 * sizeof(float));
        triBuffer = new ComputeBuffer(totalTris, sizeof(int));

        lineBuffer = new ComputeBuffer(totalLinePoints, 3 * sizeof(float));

        featherBuffer = new ComputeBuffer(totalFeatherPoints, 32 * sizeof(float));


        populateMeshData();



    }

    public void populateMeshData()
    {

        float[] values = new float[totalMeshPoints * 8];

        for (int i = 0; i < meshes.Length; i++)
        {
            int baseID = i * 8 * vertsPerMesh;

            Vector3[] positions = meshes[i].vertices;
            Vector3[] normals = meshes[i].normals;
            Vector2[] uvs = meshes[i].uv;
            //Colors[] colors = meshes[i].colors;


            print(positions.Length);

            for (int j = 0; j < meshes[i].vertices.Length; j++)
            {

                values[baseID + j * 8 + 0] = positions[j].x;
                values[baseID + j * 8 + 1] = positions[j].y;
                values[baseID + j * 8 + 2] = positions[j].z;

                values[baseID + j * 8 + 3] = normals[j].x;
                values[baseID + j * 8 + 4] = normals[j].y;
                values[baseID + j * 8 + 5] = normals[j].z;

                if (j < uvs.Length)
                {

                    values[baseID + j * 8 + 6] = uvs[j].x;
                    values[baseID + j * 8 + 7] = uvs[j].y;
                }
                else
                {

                    values[baseID + j * 8 + 6] = 0;
                    values[baseID + j * 8 + 7] = 0;
                }

            }

        }

        vertBuffer.SetData(values);


        int[] v2 = new int[totalTris];

        for (int i = 0; i < meshes.Length; i++)
        {

            int baseID = i * trisPerMesh;
            int baseVertID = i * vertsPerMesh;
            int[] tris = meshes[i].triangles;

            for (int j = 0; j < tris.Length; j++)
            {

                v2[baseID + j] = tris[j] + baseVertID;

            }

        }

        triBuffer.SetData(v2);


    }


    // Start is called before the first frame update
    public void Destroy()
    {
        if (lineBuffer != null) { lineBuffer.Release(); }



        if (vertBuffer != null) { vertBuffer.Dispose(); }
        if (triBuffer != null) { triBuffer.Dispose(); }
        if (featherBuffer != null) { featherBuffer.Dispose(); }
    }

    // Update is called once per frame
    public void UpdateFeathers()
    {

        if (lineBuffer != null)
        {

            //print("GPU BODY BIRD HEAD :" + bird.head.position );

            uint y; uint z;
            shader.GetKernelThreadGroupSizes(0, out numThreads, out y, out z);


            shader.SetBuffer(0, "_LineBuffer", lineBuffer);
            shader.SetBuffer(1, "_LineBuffer", lineBuffer);
            shader.SetBuffer(1, "_FeatherBuffer", featherBuffer);

            shader.SetMatrix("_Head", bird.head.localToWorldMatrix);
            shader.SetMatrix("_Neck", bird.neck.localToWorldMatrix);
            shader.SetMatrix("_Chest", bird.shoulder.localToWorldMatrix);
            shader.SetMatrix("_Spine", bird.spine.localToWorldMatrix);
            shader.SetMatrix("_Hip", bird.hip.localToWorldMatrix);
            shader.SetMatrix("_TailBase", bird.tail.transform.localToWorldMatrix);
            shader.SetMatrix("_TailLeft", bird.tail.tailBones[0].localToWorldMatrix);
            shader.SetMatrix("_TailCenter", bird.tail.tailBones[1].localToWorldMatrix);
            shader.SetMatrix("_TailRight", bird.tail.tailBones[2].localToWorldMatrix);
            shader.SetMatrix("_WingLeft", bird.leftWing.bones[0].localToWorldMatrix);
            shader.SetMatrix("_WingRight", bird.rightWing.bones[0].localToWorldMatrix);





            int renderedFeathers = (int)Mathf.Floor(bird.percentageRendered * (float)totalFeatherPoints);



            // Coming in from bird 
            bird.SetBirdParameters(shader);


            shader.SetFloat("_Time", Time.time);


            numGroups = (renderedFeathers + ((int)numThreads - 1)) / (int)numThreads;
            if (numGroups <= 0) { numGroups = 1; }

            shader.Dispatch(0, numGroups, 1, 1);

            God.instance.SetTerrainCompute(1, shader);

            shader.Dispatch(1, numGroups, 1, 1);

            mpb.SetBuffer("_LineBuffer", lineBuffer);
            mpb.SetBuffer("_FeatherBuffer", featherBuffer);
            mpb.SetBuffer("_VertBuffer", vertBuffer);
            mpb.SetBuffer("_TriBuffer", triBuffer);
            mpb.SetInt("_TrisPerMesh", trisPerMesh);
            mpb.SetInt("_NumberMeshes", meshes.Length);
            mpb.SetInt("_Count", totalLinePoints);

            mpb.SetFloat("_Locked", lockedValue);

            if (debugLinePoints)
            {
                Graphics.DrawProcedural(featherDebugLineMaterial, new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles, totalLinePoints * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
            }

            if (debugFeatherPoints)
            {
                Graphics.DrawProcedural(featherDebugMaterial, new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles, renderedFeathers * 3 * 2 * 4, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
            }

            if (drawFeathers)
            {
                Graphics.DrawProcedural(featherMaterial, new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles, renderedFeathers * trisPerMesh, 1, null, mpb, ShadowCastingMode.On, true, LayerMask.NameToLayer("Debug"));
            }

        }



    }
}
