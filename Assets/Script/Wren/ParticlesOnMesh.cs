using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ParticlesOnMesh : MonoBehaviour
{

    
    public int numPoints;
    

    private ComputeBuffer triBuffer;
    private ComputeBuffer vertBuffer;
    private ComputeBuffer pointBuffer;



public ComputeShader shader;
public Material debugMaterial;

private MaterialPropertyBlock mpb;

private int kernel;
private int kernel2;
private int numGroups;

    // 24
    public void OnEnable(){


         if( mpb == null ){ mpb = new MaterialPropertyBlock(); }
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] nors = mesh.normals;
        Vector3[] verts = mesh.vertices;
        Vector2[] uvs = mesh.uv;

        int[] triangles = mesh.triangles;

        float[] meshVals = new float[ verts.Length * 8 ];


        // Setting up our mesh data
        for( int i = 0; i < verts.Length; i++ ){
            meshVals[i*8+0] = verts[i].x;
            meshVals[i*8+1] = verts[i].y;
            meshVals[i*8+2] = verts[i].z;
            meshVals[i*8+3] = nors[i].x;
            meshVals[i*8+4] = nors[i].y;
            meshVals[i*8+5] = nors[i].z;
            meshVals[i*8+6] = uvs[i].x;
            meshVals[i*8+7] = uvs[i].y;
        }

        vertBuffer = new ComputeBuffer( verts.Length , 8 * sizeof(float));
        vertBuffer.SetData(meshVals);

        triBuffer = new ComputeBuffer( triangles.Length , 1 * sizeof(int));

        Vector3 pos;
        Vector3 uv;
        Vector3 tan;
        Vector3 nor;
        int baseTri; int tri0; int tri1; int tri2;

        float[] values = new float[ numPoints *2* 24 ];

        int index = 0;
        for( int i = 0; i < numPoints; i ++ ){

        baseTri = 3 * (int)Mathf.Floor(Random.Range( 0f, ((float)triangles.Length/3)));
        tri0 = baseTri + 0;
        tri1 = baseTri + 1;
        tri2 = baseTri + 2;

        tri0 = triangles[tri0];
        tri1 = triangles[tri1];
        tri2 = triangles[tri2];

        pos = GetRandomPointInTriangle(verts[tri0], verts[tri1], verts[tri2]);

        float a0 = AreaOfTriangle(pos, verts[tri1], verts[tri2]);
        float a1 = AreaOfTriangle(pos, verts[tri0], verts[tri2]);
        float a2 = AreaOfTriangle(pos, verts[tri0], verts[tri1]);

        float aTotal = a0 + a1 + a2;

        float p0 = a0 / aTotal;
        float p1 = a1 / aTotal;
        float p2 = a2 / aTotal;

        nor = (nors[tri0] * p0 + nors[tri1] * p1 + nors[tri2] * p2).normalized;
        uv = uvs[tri0] * p0 + uvs[tri1] * p1 + uvs[tri2] * p2;
        //tan = (HELP.ToV3(tans[tri0]) * p0 + HELP.ToV3(tans[tri1]) * p1 + HELP.ToV3(tans[tri2]) * p2).normalized;


for( int j = 0;  j < 2; j ++ ){

    
    //            print( pos);
        values[ index ++ ] = pos.x;
        values[ index ++ ] = pos.y;
        values[ index ++ ] = pos.z;

        values[ index ++ ] = 0;
        values[ index ++ ] = 0;
        values[ index ++ ] = 0;

        values[ index ++ ] = nor.x;
        values[ index ++ ] = nor.y;
        values[ index ++ ] = nor.z;

        values[ index ++ ] = 0;
        values[ index ++ ] = 0;
        values[ index ++ ] = 0;

        values[ index ++ ] = uv.x;
        values[ index ++ ] = uv.y;

    
        values[index++ ] = (float)i/(float)numPoints;

        values[ index ++ ] = tri0;
        values[ index ++ ] = tri1;
        values[ index ++ ] = tri2;

        values[ index ++ ] = p0;
        values[ index ++ ] = p1;
        values[ index ++ ] = p2;

        values[ index ++ ] = 1;
        values[ index ++ ] = 0;
        values[ index ++ ] = 0;
}
        }

        pointBuffer = new ComputeBuffer(numPoints*2 , 24 * sizeof(float));
        pointBuffer.SetData(values);

    kernel = shader.FindKernel("Feathers");
    kernel2 = shader.FindKernel("Feathers2");
    uint y; uint z;
    uint numThreads;
    shader.GetKernelThreadGroupSizes(kernel, out numThreads , out y, out z);
  
    numGroups = (numPoints*2+((int)numThreads-1))/(int)numThreads;
 


    
    }

    public void OnDisable(){

    }


    public void FixedUpdate(){

shader.SetBuffer(kernel,"_VertBuffer", pointBuffer);
shader.SetBuffer(kernel,"_TriBuffer", triBuffer);
shader.SetBuffer(kernel,"_BaseBuffer", vertBuffer);

shader.SetBuffer(kernel2,"_VertBuffer", pointBuffer);
shader.SetBuffer(kernel2,"_TriBuffer", triBuffer);
shader.SetBuffer(kernel2,"_BaseBuffer", vertBuffer);
Matrix4x4 ltw = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
shader.SetMatrix("_Transform",ltw);
shader.Dispatch( kernel,numGroups ,1,1);
shader.Dispatch( kernel2,numGroups ,1,1);
    }
   public void LateUpdate(){
  mpb.SetBuffer("_VertBuffer", pointBuffer);
        
        mpb.SetInt("_Count",numPoints);
        //mpb.SetMatrix("_Transform",transform.localToWorldMatrix);

    
        Graphics.DrawProcedural(debugMaterial,  new Bounds(transform.position, Vector3.one * 50000000), MeshTopology.Triangles, (numPoints/2) * 3 * 2 , 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Default"));


    }



  public static Vector3 GetRandomPointInTriangle(  Vector3 v1 , Vector3 v2 , Vector3 v3 ){
    float r1 = Random.value;
    float r2 = Random.value;
    return (1 - Mathf.Sqrt(r1)) * v1 + (Mathf.Sqrt(r1) * (1 - r2)) * v2 + (Mathf.Sqrt(r1) * r2) * v3;
  }

  public static float AreaOfTriangle( Vector3 v1 , Vector3 v2 , Vector3 v3 ){
     Vector3 v = Vector3.Cross(v1-v2, v1-v3);
     float area = v.magnitude * 0.5f;
     return area;
  }



}
