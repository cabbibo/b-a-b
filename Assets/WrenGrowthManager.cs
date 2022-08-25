using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenGrowthManager : MonoBehaviour
{


   
public Wren wren;
public WrenNetworked networkInfo;

public float hurtCollisionMultiplier;
public float hurtCollisionCutoff;


public WrenState state;



//TODO: do we need to pass state on this one?
public void HurtCollision(Collision c){


    if( state.isLocal){

        state.HealthAdd( c.relativeVelocity.magnitude * hurtCollisionMultiplier );
        God.audio.Play( God.sounds.hurtClip );
        if( God.glitchHit != null ){ God.glitchHit.StartGlitch(); }
      
    }
}



public float affectFlight;

}
