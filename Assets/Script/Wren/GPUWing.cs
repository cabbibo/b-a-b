using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WrenUtils;

public class GPUWing : MonoBehaviour
{

    public FullBird bird;
    public Wing1 wing;

    public Material featherDebugMaterial;
    public Material featherMaterial;


    public int numberLesserCovertsRows;
    public int numberLesserCovertsCols;
    public int numberPrimaryFeathers;
    public int numberPrimaryCoverts;

    public Mesh primaryFeather;
    public Mesh secondaryFeather;
    public Mesh primaryCovert;
    public Mesh secondaryCovert;
    public Mesh lesserCovert;



    public ComputeBuffer lineBuffer;
    public ComputeBuffer featherBuffer;
    public ComputeShader shader;

    public ComputeBuffer vertBuffer;
    public ComputeBuffer triBuffer;
    public Mesh[] meshes;


    public int totalFeathers;

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


        if (mpb == null)
        {

            mpb = new MaterialPropertyBlock();
        }

        totalFeathers = numberLesserCovertsRows * numberLesserCovertsCols;
        totalFeathers += numberPrimaryFeathers + numberPrimaryCoverts;

        meshes = new Mesh[5];

        meshes[0] = primaryFeather;
        meshes[1] = secondaryFeather;
        meshes[2] = primaryCovert;
        meshes[3] = secondaryCovert;
        meshes[4] = lesserCovert;


        vertsPerMesh = meshes[0].vertices.Length;
        trisPerMesh = meshes[0].triangles.Length;

        totalMeshPoints = meshes.Length * meshes[0].vertices.Length;
        totalTris = meshes.Length * meshes[0].triangles.Length;

        vertBuffer = new ComputeBuffer(totalMeshPoints, 8 * sizeof(float));
        triBuffer = new ComputeBuffer(totalTris, sizeof(int));

        featherBuffer = new ComputeBuffer(totalFeathers, 32 * sizeof(float));


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

            //print(meshes[i].vertices.Length);
            //print(totalMeshPoints/8);

            for (int j = 0; j < meshes[i].vertices.Length; j++)
            {

                values[baseID + j * 8 + 0] = positions[j].x;
                values[baseID + j * 8 + 1] = positions[j].y;
                values[baseID + j * 8 + 2] = positions[j].z;

                values[baseID + j * 8 + 3] = normals[j].x;
                values[baseID + j * 8 + 4] = normals[j].y;
                values[baseID + j * 8 + 5] = normals[j].z;

                values[baseID + j * 8 + 6] = uvs[j].x;
                values[baseID + j * 8 + 7] = uvs[j].y;

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



        if (featherBuffer != null)
        {

            uint y; uint z;
            shader.GetKernelThreadGroupSizes(0, out numThreads, out y, out z);



            int renderedFeathers = (int)Mathf.Floor(bird.percentageRendered * (float)totalFeathers);



            // print( God.instance.terrainData.heightmapTexture  );
            // print( God.instance.terrainData.size  );

            shader.SetBuffer(0, "_FeatherBuffer", featherBuffer);


            God.instance.SetTerrainCompute(0, shader);

            shader.SetBool("_LeftOrRight", wing.leftOrRight);
            shader.SetMatrix("_Shoulder", wing.bones[0].localToWorldMatrix);
            shader.SetMatrix("_Elbow", wing.bones[1].localToWorldMatrix);
            shader.SetMatrix("_Hand", wing.bones[2].localToWorldMatrix);
            shader.SetMatrix("_Finger", wing.bones[3].localToWorldMatrix);

            shader.SetMatrix("_Chest", bird.shoulder.localToWorldMatrix);


            // Coming in from bird 
            bird.SetBirdParameters(shader);


            numGroups = (renderedFeathers + ((int)numThreads - 1)) / (int)numThreads;
            if (numGroups <= 0) { numGroups = 1; }
            shader.Dispatch(0, numGroups, 1, 1);

            mpb.SetBuffer("_FeatherBuffer", featherBuffer);
            mpb.SetBuffer("_VertBuffer", vertBuffer);
            mpb.SetBuffer("_TriBuffer", triBuffer);
            mpb.SetInt("_TrisPerMesh", trisPerMesh);
            mpb.SetInt("_NumberMeshes", meshes.Length);



            // Graphics.DrawProcedural( featherDebugMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,renderedFeathers * 3 * 2 , 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
            Graphics.DrawProcedural(featherMaterial, new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles, renderedFeathers * trisPerMesh, 1, null, mpb, ShadowCastingMode.TwoSided, true, gameObject.layer);


        }



    }
}
