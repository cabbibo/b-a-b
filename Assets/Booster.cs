using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;


[ExecuteAlways]
public class Booster : Cycle
{
    public bool debug;
    public float boostVal=1;

    public Vector3 lifeBoostVal;

    public Life life;

    public Renderer renderer;
    public MaterialPropertyBlock mpb;

    public void OnBoost( Wren w ){

    
        lifeBoostVal = w.physics.vel;
    }

    public void OnBoost(Vector3 v){

        lifeBoostVal = v;

    }

    public override void OnLive(){

        life.BindVector3("_BoostVal", () => this.lifeBoostVal );

        if( debug ){
            OnBoost(transform.forward);
        }

    }

    public override void WhileLiving(float v){

        if( renderer == null ){ renderer = GetComponent<Renderer>(); }
        if( mpb == null ){ mpb = new MaterialPropertyBlock(); }

        
            
        WrenUtils.God.instance.SetWrenCompute( 0, life.shader);
        lifeBoostVal *= .9f;// Vector3.Scale( lifeBoostVal , .9f );

    }

}
