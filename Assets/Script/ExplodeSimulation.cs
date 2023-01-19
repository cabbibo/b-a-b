using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using IMMATERIA;
using WrenUtils;


[ExecuteAlways]
public class ExplodeSimulation : MonoBehaviour
{


    public int count;
    public int _NumSide;
    public float _ParticleSize;

    
    public ComputeShader shader;
    public ComputeBuffer particleBuffer;
    public Material particleMaterial;
    private int numGroups;
    private uint numThreads;

    private MaterialPropertyBlock mpb;

    public int structSize =  16;
    public float explodeTime = -1;
    public bool exploded;
    bool oExploded;
    public int sides = 6;

    void OnEnable(){

        explodeTime = -1;
        exploded = false;
        oExploded = false;
        particleBuffer = new ComputeBuffer( count , sizeof(float) * structSize );

    }

    void OnDisable(){
        if(particleBuffer != null ){ particleBuffer.Dispose(); }
    }


    public void Update(){

        if( exploded == true && oExploded == false ){
            explodeTime = 0;
        }
        oExploded = exploded;
    

        if( exploded == false ){
            explodeTime = -1;
        }


        
        if(  exploded && particleBuffer != null ){

            uint y; uint z;
            shader.GetKernelThreadGroupSizes(0, out numThreads , out y, out z);
           
            shader.SetBuffer( 0, "_PointBuffer" , particleBuffer );
            shader.SetInt("_Count", count);

            shader.SetFloat("_ExplodeTime" , explodeTime );
            shader.SetMatrix("_LTW",transform.localToWorldMatrix);

            shader.SetFloat("_Time", Time.time);
            
            numGroups = (count+((int)numThreads-1))/(int)numThreads;
            shader.Dispatch( 0,numGroups ,1,1);
            
            explodeTime += Time.deltaTime * .1f;

            if( mpb == null ){
                mpb = new MaterialPropertyBlock();
            }

             mpb.SetBuffer("_VertBuffer", particleBuffer );
             mpb.SetFloat("_Size", _ParticleSize );
             mpb.SetInt("_Count",count);
             mpb.SetInt("_Sides",sides);

            WrenUtils.God.instance.SetTerrainMPB(mpb);
            
            if( WrenUtils.God.wren ){
                WrenUtils.God.wren.state.SetStateMPB(mpb);
            }


            Graphics.DrawProcedural( particleMaterial ,  new Bounds(transform.position, Vector3.one * 5000000), MeshTopology.Triangles,count * 3 * (2+2)*sides , 1, null, mpb, ShadowCastingMode.Off, true, gameObject.layer);
       

            if( explodeTime > 1 ){
                exploded = false;
            }

            
        }
    

  }

  public void Explode(){
      exploded = true;
  }


}
