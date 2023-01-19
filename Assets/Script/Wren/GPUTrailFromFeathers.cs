using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
 using WrenUtils;

public class GPUTrailFromFeathers : MonoBehaviour
{

    public GPUWing wing;
    
    public Material featherDebugMaterial;
    public Material featherMaterial;


    public int count;

    public ComputeBuffer featherBuffer;
    public ComputeShader shader;

    public int numGroups;
    public uint numThreads;

    public int emitting;

    public bool active;

    public MaterialPropertyBlock mpb;
    public void Create(){

        
        mpb = new MaterialPropertyBlock();
        
        featherBuffer = new ComputeBuffer(count, 32 * sizeof(float));
    
    
    }
    public void Destroy(){
        if( featherBuffer != null ){ featherBuffer.Release(); }
    }

    // Update is called once per frame
    public void UpdateFeathers(){

        if( featherBuffer != null && active ){

            uint y; uint z;
            shader.GetKernelThreadGroupSizes(0, out numThreads , out y, out z);
           
           


          // print( God.instance.terrainData.heightmapTexture  );
          // print( God.instance.terrainData.size  );
           
            shader.SetBuffer( 0, "_FeatherBuffer" , featherBuffer );
            shader.SetBuffer( 0, "_BaseBuffer" , wing.featherBuffer );
            shader.SetInt("_TotalBaseFeathers", wing.numberPrimaryFeathers );
            shader.SetInt("_Emitting", emitting);
            wing.bird.SetBirdParameters( shader );
            God.instance.SetTerrainCompute(0,shader);
            God.instance.SetWrenCompute( 0, shader );


            numGroups = (count+((int)numThreads-1))/(int)numThreads;

            shader.Dispatch( 0,numGroups ,1,1);

             mpb.SetBuffer("_FeatherBuffer", featherBuffer );


            // Graphics.DrawProcedural( featherDebugMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,totalFeathers * 3 * 2 , 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
              Graphics.DrawProcedural( featherMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,count * 3 * 2 , 1, null, mpb, ShadowCastingMode.TwoSided, true, gameObject.layer);
       
       
        }



    }
}
