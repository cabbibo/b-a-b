using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CrystalCluster : MonoBehaviour
{
    public int numberCrystals;


    public Vector3 spread;



    public float minRadius;
    public float maxRadius;
    public float minRadiusWeight;




    public float minScale;
    public float maxScale;
    public float minScaleWeight;
    public float minAngleUp;
    public float maxAngleUp;
    public float angleUpWeight;
    public float minWidth;
    public float maxWidth;

    public float minWidthWeight;
    public float minHeight;
    public float maxHeight;
    public float minHeightWeight;



    public float matchWidthToHeightVal;
    public float matchWidthToHeightRatio;
    public float cutAngleMin;
    public float cutAngleMax;


    public CrystalCutter crystal;



    // Make sure first time we drag prefab into scene it generates for us :)
    public void OnEnable()
    {
        if (transform.GetComponent<MeshFilter>().sharedMesh == null)
        {
            RegenerateCluster();
        }
    }
    public void RegenerateCluster()
    {

        CombineInstance[] combine = new CombineInstance[numberCrystals];

        // For each crystal, place, rotate, etc. the transform
        // and assign values that will effect the 'cut' of the crystal
        // THEN, cut the crystal, and put its mesh into our CombineInstance list
        // also add our desire location

        for (int i = 0; i < numberCrystals; i++)
        {

            float r;

            r = Mathf.Pow(Random.Range(0.0f, 1.0f), minHeightWeight);
            crystal.crystalHeight = Mathf.Lerp(minHeight, maxHeight, r);

            r = Mathf.Pow(Random.Range(0.0f, 1.0f), minWidthWeight);
            crystal.crystalRadius = Mathf.Lerp(Mathf.Lerp(minWidth, maxWidth, r), crystal.crystalHeight * matchWidthToHeightRatio, matchWidthToHeightVal);


            crystal.cutAngle = Random.Range(cutAngleMin, cutAngleMax);
            crystal.Cut();

            float x = Random.Range(-.99f, .99f) * spread.x;
            float y = Random.Range(-.99f, .99f) * spread.y;
            float z = Random.Range(-.99f, .99f) * spread.z;
            Vector3 pos = new Vector3(x, y, z);

            float a = Random.Range(0.0f, 1.0f) * 2 * Mathf.PI;
            r = Mathf.Pow(Random.Range(0.0f, 1.0f), minRadiusWeight);
            float radius = Mathf.Lerp(minRadius, maxRadius, r);
            x = Mathf.Sin(a) * radius;
            z = -Mathf.Cos(a) * radius;
            Vector3 circlePos = new Vector3(x, 0, z);

            pos = circlePos + pos;// Vector3.Lerp( pos , circlePos , r);




            r = Mathf.Pow(Random.Range(0.0f, 1.0f), angleUpWeight);
            Quaternion rot = Quaternion.Slerp(Quaternion.identity, Random.rotation, Mathf.Lerp(minAngleUp, maxAngleUp, r));

            Vector3 up = rot * Vector3.up;
            Vector3 forward = rot * Vector3.forward;


            r = Mathf.Pow(Random.Range(0.0f, 1.0f), minScaleWeight);
            Vector3 scale = Vector3.one * Mathf.Lerp(minScale, maxScale, r);


            /*

                inserting exta data!

            */
            Vector4[] uvs2 = new Vector4[crystal.mesh.vertices.Length];
            Vector4[] uvs3 = new Vector4[crystal.mesh.vertices.Length];
            Vector4[] uvs4 = new Vector4[crystal.mesh.vertices.Length];
            Vector4[] uvs5 = new Vector4[crystal.mesh.vertices.Length];



            Vector3 averagePosition = Vector3.zero;

            for (int j = 0; j < crystal.mesh.vertices.Length; j++)
            {
                averagePosition += crystal.mesh.vertices[j];
            }

            averagePosition /= crystal.mesh.vertices.Length;
            averagePosition = rot * averagePosition;
            averagePosition += pos;


            averagePosition = pos;

            for (int j = 0; j < crystal.mesh.vertices.Length; j++)
            {
                uvs2[j] = new Vector4(averagePosition.x, averagePosition.y, averagePosition.z, i);
                uvs3[j] = new Vector4(up.x, up.y, up.z, 0);
                uvs4[j] = new Vector4(forward.x, forward.y, forward.z, 0);
                uvs5[j] = new Vector4(scale.x, scale.y, scale.z, 0);
            }

            crystal.mesh.SetUVs(2, new List<Vector4>(uvs2));
            crystal.mesh.SetUVs(3, new List<Vector4>(uvs3));
            crystal.mesh.SetUVs(4, new List<Vector4>(uvs4));
            crystal.mesh.SetUVs(5, new List<Vector4>(uvs5));



            combine[i].transform = Matrix4x4.TRS(pos, rot, scale);
            combine[i].mesh = crystal.mesh;
        }


        // GO ahead and combine the meshes 
        // ( using the big index incase there are too many verts! )
        transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        transform.GetComponent<MeshFilter>().sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
    }


}
