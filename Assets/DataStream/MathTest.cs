using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine.Playables;
using UnityEngine.Timeline;

using UnityEditor;




[ExecuteAlways]
public class MathTest : MonoBehaviour
{

    public int numPointsPerLine;
    public float randomness;
    public float lineLength;
    public float chance;
    public float maxLength;

    public float width;

    public float3 streamDirection;
    public float3 streamUp;


    List<float3> points;
    List<int> connections;


    public ComputeBuffer pointBuffer;
    public ComputeBuffer connectionBuffer;

    // Start is called before the first frame update
    void OnEnable()
    {




        points = new List<float3>();
        connections = new List<int>();

        for( var i = 0; i < 10; i++ ){
            for( var j = 0; j < 10; j++ ){
                float x = ((float)i+.5f)/10-.5f;
                float y = ((float)j+.5f)/10-.5f;
                MakeLine( float3(x,0,y) * width , streamDirection , streamUp , randomness, lineLength,numPointsPerLine);
            }
        }

        MakeConnections();


        pointBuffer = new ComputeBuffer( points.Count , 3 * sizeof(float));
        pointBuffer.SetData(points);


        connectionBuffer = new ComputeBuffer( connections.Count ,  sizeof(int));

        int[] connectionArray = connections.ToArray();

        print(connections.Count);

        connectionBuffer.SetData(connections);

        
        
    }


    void MakeLine(float3 start , float3 direction  , float3 up , float randomness,float length, int numPoints ){

        float3 left = cross( direction , up );

        float3 point = start;

        for( int i = 0; i < numPoints; i++ ){
            points.Add(new Vector3(point.x, point.y,point.z));
            point += direction * (float)1/(float)numPoints * length;
            point += up * UnityEngine.Random.Range(-randomness,randomness);
            point += left * UnityEngine.Random.Range(-randomness,randomness);
        }


    }

    void MakeConnections(){

        print( points.Count);


        for( var i =0; i < points.Count; i++){
            for( var j = i; j < points.Count;j++){

                float3 p1 = points[i];
                float3 p2 = points[j];


                 if( i != j ){

                    if( length(p2-p1) < maxLength && UnityEngine.Random.Range(0.001f , .999f) < chance ){
                        connections.Add(i);
                        connections.Add(j);
                    }
                }

            }
        }

    }


    public Material material;
    public Material material2;
    MaterialPropertyBlock mpb;
    
    public Vector3 extents = new Vector3(10000,10000,10000);
    // Update is called once per frame
    void LateUpdate()
    {

        if(mpb == null ){
            mpb = new MaterialPropertyBlock();
        }

        mpb.SetBuffer( "_ConnectionBuffer", connectionBuffer);
        mpb.SetBuffer( "_PointBuffer", pointBuffer);


        Graphics.DrawProcedural(material, new Bounds(transform.position, extents), MeshTopology.Triangles, connections.Count *3, 1, null, mpb, ShadowCastingMode.TwoSided, true, gameObject.layer);
    
        Graphics.DrawProcedural(material2, new Bounds(transform.position, extents), MeshTopology.Triangles, points.Count *3 * 2, 1, null, mpb, ShadowCastingMode.TwoSided, true, gameObject.layer);
    
    }
}
