using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Updraft : MonoBehaviour
{
    public float distanceCutoff = 200;
    public float multiplier = 3000;
    public float divisionMultiplier = 1;
    public float upLessForceMultiplier;
    void FixedUpdate(){

        
        foreach( Wren w  in God.wrens){

            float upForce = 0;

            Vector3 dist = w.transform.position - transform.position;

            dist = Vector3.Scale( dist , Vector3.left + Vector3.forward );


            float mag = dist.magnitude;

            

            if( mag < distanceCutoff ){ upForce = ( multiplier)  / (divisionMultiplier * mag+10); }

     
            upForce /= 1+ w.transform.position.y * upLessForceMultiplier;
            if(!w.state.onGround){
                w.physics.rb.AddForce( Vector3.up * upForce );
            }

        }
    }

}
