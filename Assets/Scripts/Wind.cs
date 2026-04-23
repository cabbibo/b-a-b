using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
 using WrenUtils;


[ExecuteAlways]
public class Wind : MonoBehaviour
{

    public Transform transformCopy;

    public float size;
    public int  numGridPoints;

    public float furthestHeight;
    public float closestForce;
    public ComputeBuffer gridPoints;
    public ComputeBuffer windPoints;

    public int totalCount;

    public ComputeShader getDistanceShader;
    public ComputeShader windShader;

    public Material gridMat;
    public Material windMat;

    public bool drawGrid;
    public bool drawParticles;

    public int numWindParticles;


    public WindZoneBuffer windZoneBuffer;

    public Vector3 oGridPos;
    public Vector3 gridPos;


    private float cellSize;

    // Start is called before the first frame update
    void OnEnable()
    {
        totalCount = numGridPoints * numGridPoints * numGridPoints;


        cellSize = size / (float)numGridPoints;

        gridPoints = new ComputeBuffer( totalCount , sizeof(float) * 8);
        windPoints = new ComputeBuffer( numWindParticles , sizeof(float) * (8));
        
    }

    void OnDisable(){
        if( gridPoints != null ){ gridPoints.Dispose(); }
        if( windPoints != null ){ windPoints.Dispose(); }

    }

    uint numThreads;
    int numGroups;
    MaterialPropertyBlock mpb;
    MaterialPropertyBlock windMPB;
    // Update is called once per frame
    void FixedUpdate()
    {
         uint y; uint z;
        if( God.wren ){

            furthestHeight = God.wren.physics.furthestHeight;
            closestForce = God.wren.physics.closestForce;
        }

 
        transform.position = transformCopy.position;

        oGridPos = gridPos;

        float whichGridX = Mathf.Floor( transform.position.x / cellSize ); 
        float whichGridY = Mathf.Floor( transform.position.y / cellSize ); 
        float whichGridZ = Mathf.Floor( transform.position.z / cellSize ); 

        gridPos = new Vector3(whichGridX,whichGridY,whichGridZ)  * cellSize;


        if( oGridPos != gridPos ){

   
        getDistanceShader.GetKernelThreadGroupSizes(0, out numThreads , out y, out z);
        numGroups = (totalCount+((int)numThreads-1))/(int)numThreads;
        getDistanceShader.SetMatrix( "_LTW" , transform.localToWorldMatrix);
        getDistanceShader.SetMatrix( "_WTL" , transform.worldToLocalMatrix);
        getDistanceShader.SetBuffer(0,"_GridBuffer", gridPoints );
        getDistanceShader.SetInt("_NumGridPoints",numGridPoints);
        getDistanceShader.SetFloat("_Size",size);
        getDistanceShader.SetVector("_GridPos" , gridPos);
        getDistanceShader.SetFloat("_FurthestHeight",furthestHeight);
        getDistanceShader.SetFloat("_ClosestForce",closestForce);


    if( windZoneBuffer._buffer  != null ){
        getDistanceShader.SetInt("_WindZoneBufferCount", windZoneBuffer.windZones.Length);
        getDistanceShader.SetBuffer(0,"_WindZoneBuffer", windZoneBuffer._buffer);
    }
        
        God.instance.SetTerrainCompute(0, getDistanceShader);
        getDistanceShader.Dispatch( 0,numGroups ,1,1);


        }




        windShader.SetVector("_GridPos" , gridPos);
        windShader.SetFloat("_Size",size);

        windShader.SetMatrix( "_LTW" , transform.localToWorldMatrix);
        windShader.SetMatrix( "_WTL" , transform.worldToLocalMatrix);
        windShader.SetFloat( "_Time" ,Time.time);

        Vector3 vel = Vector3.left;
        
        if( God.wren ){ vel = God.wren.physics.vel; }
        windShader.SetVector( "_WrenVel" ,vel );

        windShader.SetBuffer(0,"_GridBuffer", gridPoints );
        windShader.SetBuffer(0,"_PointBuffer", windPoints );

     
        windShader.GetKernelThreadGroupSizes(0, out numThreads , out y, out z);
        numGroups = (numWindParticles+((int)numThreads-1))/(int)numThreads;
        
        God.instance.SetTerrainCompute(0, windShader);
        windShader.Dispatch( 0,numGroups ,1,1);

    }   

    void LateUpdate(){

        if( drawGrid ){
            if( mpb == null ){
                mpb = new MaterialPropertyBlock();     
            }

            mpb.SetBuffer("_VertBuffer", gridPoints );
            mpb.SetInt("_Count", totalCount);

            Graphics.DrawProcedural(gridMat ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,totalCount * 3 * 3 , 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Default"));
        }



        if( drawParticles ){
   
            if( windMPB == null ){
                windMPB = new MaterialPropertyBlock();     
            }

            windMPB.SetBuffer("_VertBuffer", windPoints );
            windMPB.SetInt("_Count", numWindParticles);

            Graphics.DrawProcedural(windMat ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,numWindParticles* 3 * 2 , 1, null, windMPB, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Default"));
    
        }

    }
}
