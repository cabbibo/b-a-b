using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteAlways]
public class Mandala : MonoBehaviour
{

    public Mesh mesh;
    public Material mat;

    public ComputeBuffer meshBuffer;    
    public ComputeBuffer triBuffer;    
    
    public Vector3 offset;
    public int rows;
    public int columns;

    public int instances;

    int totalCount;
    int numTris;
    void OnEnable()
    {

        instances = rows * columns;
        totalCount = mesh.vertices.Length * instances;
        numTris = mesh.triangles.Length * instances;


        meshBuffer = new ComputeBuffer( totalCount , sizeof(float) * 12);
        triBuffer = new ComputeBuffer( numTris , sizeof(int) * 1);

        PopulateBuffer();
        
    }

    void OnDisable(){
        if( meshBuffer != null ){ meshBuffer.Dispose(); }
        if( triBuffer != null ){ triBuffer.Dispose(); }
    }

    struct vert{
        Vector3 pos;
        Vector3 nor;

        Vector4 color;
        Vector2 uv;

        public vert(Vector3 p , Vector3 n , Vector4 c , Vector3 u){
            this.pos = p;
            this.color = c;
            this.nor = n;
            this.uv = u;
        }
    }
    void PopulateBuffer(){
        Vector3[] verts = mesh.vertices;
        Vector3[] nors = mesh.normals;
        Color[] colors = mesh.colors;
        Vector2[] uv = mesh.uv;

        vert[] vert2 = new vert[ mesh.vertices.Length ];
        int index = 0;
       // for( int i = 0; i < instances; i++ ){
            for( int k = 0;  k < verts.Length; k++ ){
                vert2[index] = new vert( verts[k], nors[k] , (Vector4)colors[k],uv[k]);
                index ++;

            }

        //}
        
        print( vert2[0]);
        meshBuffer.SetData(vert2);

        triBuffer.SetData(mesh.triangles);

    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    private MaterialPropertyBlock mpb;
    // Update is called once per frame
    void Update()
    {
        if( mpb == null ){
                mpb = new MaterialPropertyBlock();     
            }

            mpb.SetBuffer("_VertBuffer", meshBuffer );
            mpb.SetBuffer("_TriBuffer", triBuffer );
            mpb.SetInt("_Count", totalCount);
            mpb.SetMatrix( "_Transform", transform.localToWorldMatrix);
            mpb.SetInt("_TriCount", mesh.triangles.Length);
            mpb.SetInt("_TotalInstances", instances);
            mpb.SetInt("_Rows",rows);
            mpb.SetInt("_Cols",columns);
            mpb.SetVector("_Offset", offset);


            Graphics.DrawProcedural(mat ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,mesh.triangles.Length  , instances, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Default"));
       

    }
}
