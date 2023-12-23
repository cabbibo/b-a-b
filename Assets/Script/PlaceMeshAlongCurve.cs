using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicCurve;

using static Unity.Mathematics.math;
using Unity.Mathematics;

[ExecuteAlways]
public class PlaceMeshAlongCurve : MonoBehaviour
{

    public Transform modelNormalizationTransform;
    public Mesh baseMesh;
    public bool remake;

    public Curve curve;

    public Mesh mesh;

    public float xyScaleMultiplier = 1;



    // Start is called before the first frame update
    void OnEnable()
    {
        FullBake();
    }


    public void FullBake()
    {

        if (remake)
        {

            Vector3[] vertices = baseMesh.vertices;
            Vector3[] normals = baseMesh.normals;
            Color[] colors = baseMesh.colors;
            Vector2[] uv = baseMesh.uv;

            int vertexCount = vertices.Length;

            Vector3[] newVerts = new Vector3[vertexCount];
            Vector2[] newUVs = new Vector2[vertexCount];
            Color[] newColors = new Color[vertexCount];


            float3 pos; float3 fwd; float3 up; float3 rit; float scale; float2 idd;

            for (int i = 0; i < vertexCount; i++)
            {


                Vector3 vertex = vertices[i];
                Vector3 position = modelNormalizationTransform.TransformPoint(vertex);

                if (i < 100)
                {
                    print(position);
                }

                curve.GetDataFromValueAlongCurve(position.z, out pos, out fwd, out up, out rit, out scale, out idd);

                newVerts[i] = pos + up * position.y * scale * xyScaleMultiplier + rit * position.x * scale * xyScaleMultiplier;

                if (i < 100)
                {
                    print(vertices[i]);
                }
                newColors[i] = colors[i];
                newUVs[i] = uv[i];



            }


            mesh = new Mesh();
            mesh.vertices = newVerts;
            mesh.colors = newColors;
            mesh.uv = newUVs;
            mesh.triangles = baseMesh.triangles;
            mesh.RecalculateNormals();

            print(mesh.vertices.Length);//

            GetComponent<MeshFilter>().mesh = mesh;
            //  GetComponent<MeshCollider>().sharedMesh = mesh;

        }
    }

}


