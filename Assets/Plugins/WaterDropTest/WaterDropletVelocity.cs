using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using static Unity.Mathematics.math;
using Unity.Mathematics;


[ExecuteAlways]
public class WaterDropletVelocity : MonoBehaviour
{


    public Renderer[] drops;

    private MaterialPropertyBlock mpb;

    Vector4[] dropInfo;


     float3 pos;
     float3 oPos;
     float3 ooPos;

    float3 vel;
    float3 oVel;


    private Renderer renderer;
    // Start is called before the first frame update
    void OnEnable()
    {

        mpb = new MaterialPropertyBlock();
        renderer = GetComponent<Renderer>();
        pos = float3(transform.position);
        oPos = float3(transform.position);
        ooPos = float3(transform.position);
        vel=0;
        oVel=0;

    }

    Vector4 t;


    // Update is called once per frame
    void FixedUpdate()
    {
        ooPos = oPos;
        oPos = pos;
        pos = float3(transform.position);
        vel = pos - oPos;
        oVel = oPos - ooPos;
       


        float offset = 2;
        if( length(vel) == 0 ){ vel = float3(0,.0001f,0); offset = 0;}
        if( length(oVel) == 0 ){ oVel = float3(0,.0001f,0); offset = 0;}
        
        float3 vx = 100*cross(normalize(vel), float3(0,1,0));
        if( length(vx) == 0 ){
            vx = float3(1,0,0);
        }

        float3 vy = 100*cross(vx,vel);

        
        drops[0].transform.position = float3(transform.position) - normalize(vel) * offset;
        drops[1].transform.position = float3(drops[0].transform.position) - normalize(oVel) * offset;
       
        mpb.SetVector("_MainBall", float4( transform.position , transform.lossyScale.x));
        mpb.SetVector("_Velocity", (Vector3)vel );
        mpb.SetVector("_Velocity2", (Vector3)oVel );
        mpb.SetVector("_VX", (Vector3)(normalize(vx)) );
        mpb.SetVector("_VY", (Vector3)(normalize(vy)) );
        for( int i = 0; i < drops.Length; i++ ){
            drops[i].SetPropertyBlock(mpb);
        }

        renderer.SetPropertyBlock(mpb);

   
    

    }


}
