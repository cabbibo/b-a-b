using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteAlways]
public class Grass : MonoBehaviour
{


    public PaintTerrain paintTerrain;
    public int count;
    public int _NumRows;
    public float _Ratio;
    public float _ParticleSize;

    
    public ComputeShader shader;
    public ComputeBuffer particleBuffer;
    public Material particleMaterial;
    private int numGroups;
    private uint numThreads;

    private MaterialPropertyBlock mpb;

    public int structSize =  8;


    void OnEnable(){

        particleBuffer = new ComputeBuffer( count , sizeof(float) * structSize );

     

    }

    void OnDisable(){
        if(particleBuffer != null ){ particleBuffer.Dispose(); }
    }
  public void Update(){

        if(  particleBuffer != null){

            uint y; uint z;
            shader.GetKernelThreadGroupSizes(0, out numThreads , out y, out z);
           
            shader.SetBuffer( 0, "_ParticleBuffer" , particleBuffer );
            shader.SetInt("_Count", count);


            shader.SetTexture(0,"_PaintTexture",paintTerrain._PaintTexture);

            God.instance.SetTerrainCompute( 0,shader);

            shader.SetFloat("_Time", Time.time);
            shader.SetVector("_CameraPosition", Camera.main.transform.position);
            shader.SetVector("_CameraForward", Camera.main.transform.forward);
            shader.SetVector("_CameraRight", Camera.main.transform.right);

            if( God.wren ){
                shader.SetVector("_WrenPosition", God.wren.transform.position);
            }

            
            shader.SetMatrix("_CameraViewMatrix",Camera.main.worldToCameraMatrix);
            shader.SetMatrix("_CameraViewMatrixInverse",Camera.main.worldToCameraMatrix.inverse);
            shader.SetMatrix("_CameraProjectionMatrix",Camera.main.projectionMatrix);
            shader.SetMatrix("_CameraProjectionMatrixInverse",Camera.main.projectionMatrix.inverse);
                    
            numGroups = (count+((int)numThreads-1))/(int)numThreads;
            shader.Dispatch( 0,numGroups ,1,1);

            if( mpb == null ){
                mpb = new MaterialPropertyBlock();
            }

             mpb.SetBuffer("_ParticleBuffer", particleBuffer );
             mpb.SetFloat("_ParticleSize", _ParticleSize );
             mpb.SetFloat("_Ratio", _Ratio );
             mpb.SetInt("_NumRows", _NumRows);
             mpb.SetTexture("_PaintTexture",paintTerrain._PaintTexture);

            God.instance.SetTerrainMPB(mpb);
            
            if( God.wren ){
                God.wren.state.SetStateMPB(mpb);
            }


            Graphics.DrawProcedural( particleMaterial ,  new Bounds(transform.position, Vector3.one * 5000000), MeshTopology.Triangles,count * 3 * 2 * _NumRows , 1, null, mpb, ShadowCastingMode.TwoSided, true, gameObject.layer);
       
       
        }



    

  }


}
