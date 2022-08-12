using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using static Unity.Mathematics.math;
using Unity.Mathematics;


[ExecuteAlways]
public class BakeMeshOnTerrain : MonoBehaviour
{


    public int gridSize;
    //public float size;


    public Terrain terrain;
    // Start is called before the first frame update
    void OnEnable()
    {
       BakeMesh(); 
    }

    void Update(){
       // BakeMesh();
    }


    void BakeMesh(){

        Vector3[] verts = new Vector3[ gridSize * gridSize ];
        Vector3[] normals = new Vector3[ gridSize * gridSize ];
        Vector2[] uvs = new Vector2[ gridSize * gridSize ];


        int[] tris = new int[ (gridSize-1) *(gridSize-1) * 3 * 2 ];



    int index = 0;

    for( int i =0; i < gridSize; i++ ){
        float x = (float)i  / (float)gridSize;
        for( int j=0; j < gridSize; j++ ){
            float y = (float)j / (float)gridSize;

            float3 pos = float3(x-.5f , 0, y-.5f);


            pos = transform.TransformPoint( pos );

            uvs[index] = float2( x,y);
            verts[index] = transform.InverseTransformPoint(TerrainPos( pos ));
            //verts[index] = transform.InverseTransformPoint( pos );
            normals[index] = transform.InverseTransformDirection(TerrainNorm( pos ));

        
            index ++;
        }
    }


    index = 0;
    for( int i =0; i < gridSize-1; i++ ){

        for( int j=0; j < gridSize-1; j++ ){

            int id1  = i * gridSize + j;
            int id2  = (i+1) * gridSize + j;
            int id3  = (i) * gridSize + j+1;
            int id4  = (i+1) * gridSize + j+1;


            tris[index * 6 + 0 ]= id1;
            tris[index * 6 + 1 ]= id4;
            tris[index * 6 + 2 ]= id2;
            tris[index * 6 + 3 ]= id1;
            tris[index * 6 + 4 ]= id3;
            tris[index * 6 + 5 ]= id4;

            index ++;


        }
    }


    Mesh m = new Mesh();
    m.vertices = verts;
    m.uv = uvs;
    m.normals = normals;
    m.triangles = tris;

    GetComponent<MeshFilter>().mesh = m;



    }



    float3 TerrainPos( float3 pos ){


        return float3(pos.x , terrain.SampleHeight(pos) , pos.z);

    }


    float3 TerrainNorm( float3 pos ){

        float3 eps = float3(.1f,0,0);

        float3 l = terrain.SampleHeight(pos + eps.xyy);
        float3 r = terrain.SampleHeight(pos - eps.yyx);
        float3 u = terrain.SampleHeight(pos + eps.xyy);
        float3 d = terrain.SampleHeight(pos - eps.yyx);

        return normalize(cross(l-r,u-d));

    }
 
}
