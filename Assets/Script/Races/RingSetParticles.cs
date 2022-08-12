using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteAlways]
public class RingSetParticles : MonoBehaviour
{



    public int count;

    [Range(0,1)]
    public float inRaceValue;
    public bool setFromRace;

    public float _ParticleSize;
    public float _StreamWidth;
    public float _StreamHeight;

    
    public RingSet race;
    public ComputeShader shader;

    public ComputeBuffer ringBuffer;
    public int ringBufferCount;
    public ComputeBuffer particleBuffer;


    public Material particleMaterial;
    public int numGroups;
    public uint numThreads;

    private MaterialPropertyBlock mpb;


    public bool started;
    public float startTime;
    public float endTime;

    void OnEnable(){


        ringBufferCount = race.rings.Count;
        ringBuffer = new ComputeBuffer( ringBufferCount , sizeof(float) * 8 );


        particleBuffer = new ComputeBuffer( count , sizeof(float) * 8 );

        UpdateRingBuffer();



    }


    void OnDisable(){
        if( ringBuffer     != null ){ringBuffer.Dispose();}
        if( particleBuffer != null ){particleBuffer.Dispose();}
    }

    public float[] ringFloats;
    public void UpdateRingBuffer(){

            ringFloats = new float[ ringBufferCount * 8];

        int index = 0;

        for(int i = 0; i < ringBufferCount; i++ ){

          
            ringFloats[index++] = race.rings[i].position.x; 
            ringFloats[index++] = race.rings[i].position.y; 
            ringFloats[index++] = race.rings[i].position.z; 
            ringFloats[index++] = race.rings[i].forward.x; 
            ringFloats[index++] = race.rings[i].forward.y; 
            ringFloats[index++] = race.rings[i].forward.z; 
            ringFloats[index++] = i; 
            ringFloats[index++] = 0; 

        }

        ringBuffer.SetData(ringFloats);

    }

    public void Begin(){
        startTime = Time.time;
        started = true;
    }

    public void End(){
        endTime = Time.time;
        started = false;
    }


  public void Update(){


    if( started ){

      if( setFromRace ){
          inRaceValue = race.inRaceVal;
      }

        if( particleBuffer != null  && ringBuffer != null){

            uint y; uint z;
            shader.GetKernelThreadGroupSizes(0, out numThreads , out y, out z);
           



          // print( God.instance.terrainData.heightmapTexture  );
          // print( God.instance.terrainData.size  );
           
            shader.SetBuffer( 0, "_RingBuffer" , ringBuffer );
            shader.SetBuffer( 0, "_ParticleBuffer" , particleBuffer );
            shader.SetInt("_Count", count);
            shader.SetInt("_RingBufferCount", ringBufferCount);
            shader.SetFloat("_InRaceValue", inRaceValue);
            shader.SetFloat("_StreamWidth",_StreamWidth );
            shader.SetFloat("_StreamHeight",_StreamHeight );




            //wing.bird.SetBirdParameters( shader );

            numGroups = (count+((int)numThreads-1))/(int)numThreads;
            shader.Dispatch( 0,numGroups ,1,1);

            if( mpb == null ){
                mpb = new MaterialPropertyBlock();
            }
             mpb.SetBuffer("_ParticleBuffer", particleBuffer );
             mpb.SetFloat("_ParticleSize", _ParticleSize );


            if( God.wren ){
                God.wren.state.SetStateMPB(mpb);
            }

            // Graphics.DrawProcedural( featherDebugMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,totalFeathers * 3 * 2 , 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
              Graphics.DrawProcedural( particleMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,count * 3 * 2 , 1, null, mpb, ShadowCastingMode.TwoSided, true, gameObject.layer);
       
       
        }



    }

  }


}
