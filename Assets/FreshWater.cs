using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreshWater : MonoBehaviour
{
    
    public float thirstFillSpeed;
    public bool waterHit;

    // Update is called once per frame
    void Update()
    {

        // TODO: make it so that if we change colliders, or move off the water, we make sure
        // we are no longer drinking? ( do a collision check from wren downwards every frame? )
        if( !God.wren.state.onGround){
            waterHit = false;
        }


        if( God.wren.state.onGround && waterHit ){
            God.wren.stats.QuenchednessAdd(thirstFillSpeed);
        }
        
    }

    public void OnCollisionEnter( Collision c ){


        if( God.IsOurWren(c) ){
            waterHit = true;
        }

    }
}
