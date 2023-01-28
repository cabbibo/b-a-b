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
public class Grid : MonoBehaviour
{

    public Transform center;

    public int numPoints;
    public float size;
    public float fade;

    public Material material;


    List<float3> points;


    public ComputeBuffer pointBuffer;

    // Start is called before the first frame update
    void OnEnable()
    {




        points = new List<float3>();

        for( var i = 0; i < numPoints; i++ ){
            for( var j = 0; j < numPoints; j++ ){
                for( int k = 0; k < numPoints; k++ ){
                    float x = ((float)i+.5f)/(float)numPoints;
                    float y = ((float)j+.5f)/(float)numPoints;
                    float z = ((float)k+.5f)/(float)numPoints;
                    points.Add( float3(x,y,z) * size );
                }
            }
        }


        pointBuffer = new ComputeBuffer( points.Count , 3 * sizeof(float));
        pointBuffer.SetData(points);



    }

    MaterialPropertyBlock mpb;
    
    public Vector3 extents = new Vector3(10000,10000,10000);
    // Update is called once per frame
    void LateUpdate()
    {

        if(mpb == null ){
            mpb = new MaterialPropertyBlock();
        }


        if( Camera.main != null ){
            center = Camera.main.transform;
        }


        mpb.SetVector("_Center",center.position);
        mpb.SetFloat("_GridSize",size);
        mpb.SetInt("_Dimensions",numPoints);
        mpb.SetBuffer( "_PointBuffer", pointBuffer);
        mpb.SetFloat( "_Fade", fade);
        Graphics.DrawProcedural(material, new Bounds(transform.position, extents), MeshTopology.Triangles, points.Count *3*3 * 2, 1, null, mpb, ShadowCastingMode.TwoSided, true, gameObject.layer);
    
    }
}
